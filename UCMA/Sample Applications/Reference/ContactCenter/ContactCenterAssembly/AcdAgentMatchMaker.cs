/*=====================================================================
  File:      AcdAgentMatchMaker.cs

  Summary:   Engine that manages the Agent availability and maintain their skills.
 Note that this is a rudimentary engine intended to be replaced with a 
 more industry-standards compliant solution.

/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/



using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.Samples.Utilities;
using Microsoft.Rtc.Collaboration.ConferenceManagement;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Agents;
using Microsoft.Rtc.Collaboration.Samples.ApplicationSharing;
using System.Threading;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    internal class AcdAgentMatchMaker
    {
        private AcdPlatform _platform;
        private AcdLogger _logger;
        private ApplicationEndpoint _endpoint;
        private object _syncRoot = new object();
        private List<PendingAgentMatchRequestQueueItem> _pendingAgentRequestQueueItems;
        private AcdAgentMatchMakerConfiguration _configuration;
        private TimerWheel _timerWheel = new TimerWheel();
        private MatchMakerState _matchMakerState;
        private AcdMusicOnHoldServer _mohServer;
        private List<ShutdownAsyncResult> _listOfShutdownAsyncResults = new List<ShutdownAsyncResult>();
        private List<AcdSupervisorSession> _sessions = new List<AcdSupervisorSession>();

        internal event EventHandler<MatchMakerStateChangedEventArgs> StateChanged;

        #region constructor
        /// <summary>
        /// AcdAgentMatchMaker constructs the Automatic Call Distributor match maker
        /// </summary>
        internal AcdAgentMatchMaker(AcdPlatform platform, AcdAgentMatchMakerConfiguration configuration, ApplicationEndpointSettings endpointSettings, AcdLogger logger)
        {
            _platform = platform;
            _configuration = configuration;
            _logger = logger;
            _matchMakerState = MatchMakerState.Created;
            _pendingAgentRequestQueueItems = new List<PendingAgentMatchRequestQueueItem>();

            endpointSettings.AutomaticPresencePublicationEnabled = true;
            endpointSettings.Presence.RemotePresenceSubscriptionCategories.Clear();
            endpointSettings.Presence.RemotePresenceSubscriptionCategories.Add("state");


            //Create the endpoint that will be used by the Agent match maker.
            _endpoint = new ApplicationEndpoint(platform.CollaborationPlatform, endpointSettings);

        }
        #endregion

        #region Internal Properties
        internal ApplicationEndpoint Endpoint
        {
            get { return _endpoint; }
        }
        internal AcdAgentMatchMakerConfiguration Configuration
        {
            get { return _configuration; }
        }

        internal AcdMusicOnHoldServer MusicOnHoldServer
        {
            get { return _mohServer; }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Updates the state of the current AcdAgentMatchMaker
        /// </summary>
        private void UpdateState(MatchMakerState state)
        {
            MatchMakerState previousState = _matchMakerState;
            _matchMakerState = state;

            EventHandler<MatchMakerStateChangedEventArgs> stateChanged = this.StateChanged;

            if (stateChanged != null)
                stateChanged(this, new MatchMakerStateChangedEventArgs(previousState, state));
        }

        private void CleanupPendingRequests()
        {
            lock (_syncRoot)
            {
                //flush the queue of pending requests
                foreach (PendingAgentMatchRequestQueueItem queueItem in _pendingAgentRequestQueueItems)
                {
                    queueItem.StopTimer();

                    FindAgentAsyncResult asyncResult = queueItem.FindAgentAsyncResult;

                    if (null != asyncResult)
                    {
                        //Complete the async operation with failure
                        asyncResult.SetAsCompleted(new OperationFailureException("Acd Agent Match Maker is shutting down"), false);
                    }
                }
            }
        }


        private void HandleIncomingInstantMessagingCalls(object sender, CallReceivedEventArgs<InstantMessagingCall> args)
        {
            if (_matchMakerState == MatchMakerState.Started)
            {

                AcdSupervisorSession supervisorSession = null;

                Configuration.Supervisors.ForEach(sup =>
                {
                    if (SipUriCompare.Equals(sup.SignInAddress, args.RemoteParticipant.Uri))
                    {
                        supervisorSession = new AcdSupervisorSession(this, sup, _logger);
                        supervisorSession.StateChanged += this.OnSupervisorSessionStateChanged;
                        lock (_syncRoot)
                        {
                            _sessions.Add(supervisorSession);
                        }
                        supervisorSession.HandleSupervisorInitialCall(args.Call);
                        return;
                    }
                });

                if (null == supervisorSession)
                {
                   args.Call.Decline();

                }

            }
            else
            {
                args.Call.Decline();
            }
    
        }


        private void OnSupervisorSessionStateChanged(object sender, SupervisorSessionStateChangedEventArgs e)
        {
            AcdSupervisorSession session = sender as AcdSupervisorSession;

            switch (e.NewState)
            {
               case SupervisorSessionState.Terminating:
                    lock (_syncRoot)
                    {
                        _sessions.Remove(session);
                    }
                 break;

               case SupervisorSessionState.Terminated:
                 session.StateChanged -= this.OnSupervisorSessionStateChanged;
                    break;
                             
            }
        
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// BeginShutdown initiates the termination of the Acd Agent Match Maker.
        /// </summary>
        internal IAsyncResult BeginShutdown(AsyncCallback callback, object state)
        {
            ShutdownAsyncResult asyncResult = new ShutdownAsyncResult(callback, state, this);

            bool firstTime = false;

            lock (_syncRoot)
            {
                if (_matchMakerState < MatchMakerState.Terminating)
                {
                    firstTime = true;
                    this.UpdateState(MatchMakerState.Terminating);
                }
                else if (_matchMakerState == MatchMakerState.Terminating)
                {
                    _listOfShutdownAsyncResults.Add(asyncResult);
                }
                else if (_matchMakerState == MatchMakerState.Terminated)
                {
                    asyncResult.SetAsCompleted(null, true);
                }
            }
            
            if (true == firstTime)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    var tempAr = waitState as ShutdownAsyncResult;
                    tempAr.Process();
                }, asyncResult);
            }

            return asyncResult;
        }

        internal void EndShutdown(IAsyncResult result)
        {
            ShutdownAsyncResult asyncResult = result as ShutdownAsyncResult;
            asyncResult.EndInvoke();
        }

        /// <summary>
        /// BeginStartup is the entry point to start the AcdAgentMatchMaker.
        /// The AcdAgentMatchMaker will start subscribing to the Presence of Agents
        /// </summary>
        internal IAsyncResult BeginStartup(AsyncCallback callback, object state)
        {
            StartupAsyncResult asyncResult = new StartupAsyncResult(callback, state, this);

            lock (_syncRoot)
            {
                if (_matchMakerState == MatchMakerState.Created)
                {
                    this.UpdateState(MatchMakerState.Starting);
                    ThreadPool.QueueUserWorkItem((waitState) =>
                    {
                        var tempAr = waitState as StartupAsyncResult;
                        tempAr.Process();
                    }, asyncResult);
                }
                else
                {
                    throw new InvalidOperationException("AcdAgentMatchMaker has already been started");
                }
            }

            return asyncResult;
        }

        internal void EndStartup(IAsyncResult result)
        {
            StartupAsyncResult asyncResult = result as StartupAsyncResult;

            asyncResult.EndInvoke();
        }

        /// <summary>
        /// Attempts to find an agent available to serve a customer. The operation may time out if no agent
        /// is available within a configurable max duration.
        /// </summary>
        internal IAsyncResult BeginFindAgent(AcdCustomerSession owner, List<Agent> exclusionList, List<AgentSkill> requestedSkills, AsyncCallback callback, object state)
        {
            var result = new FindAgentAsyncResult(callback, state, owner, this, exclusionList, requestedSkills);

            ThreadPool.QueueUserWorkItem((waitState) =>
            {
                var tempAr = waitState as FindAgentAsyncResult;
                tempAr.Process();
            }, result);

            return result;
        }

        /// <summary>
        /// Completes the FindAgent Operation.
        /// </summary>
        internal Agent EndFindAgent(IAsyncResult result)
        {
            FindAgentAsyncResult asyncResult = result as FindAgentAsyncResult;

            return asyncResult.EndInvoke();
        }

        /// <summary>
        /// Removes a pending request for an Agent match from the queue
        /// </summary>
        private bool RemoveFromQueue(PendingAgentMatchRequestQueueItem queueItem)
        {

            if (null != queueItem)
            {
                lock (_syncRoot)
                {
                    if (   _matchMakerState != MatchMakerState.Terminating
                        && _matchMakerState != MatchMakerState.Terminated)
                    {
                        // if not terminating, process the queue removal else this will be
                        // done by the shutdown operation.
                       return _pendingAgentRequestQueueItems.Remove(queueItem);
                    }
                }

            }

            return false ;
        }

        /// <summary>
        /// Determines whether an agent has become available.
        /// </summary>
        private void HandleAgentAvailabilityChanged(object sender, RemotePresentitiesNotificationEventArgs e)
        {

            foreach (RemotePresentityNotification notification in e.Notifications)
            {
                Agent agent = this.LookupAgent(notification.PresentityUri);

                if (agent == null)
                {
                    continue;
                }

                lock (agent)
                {
                    agent.IsOnline = IsAgentAvailable(notification);
                }

                if (agent.IsOnline && !agent.IsAllocated)
                {
                    this.HandleNewAvailableAgent(agent); ;
                }
            }
        }

        /// <summary>
        /// Determines whether an agent who becomes available has the correct skillset to be connected
        /// to a customer who is waiting. If it does, the agent is allocated.
        /// </summary>
        internal void HandleNewAvailableAgent(Agent agent)
        {
            PendingAgentMatchRequestQueueItem pendingRequestToBeRemoved = null;

            lock (_syncRoot)
            {
                foreach (PendingAgentMatchRequestQueueItem pendingRequestQueueItem in _pendingAgentRequestQueueItems)
                {
                    // Retrieve the associated asyncResult from the item in the queue
                    FindAgentAsyncResult asyncResult = pendingRequestQueueItem.FindAgentAsyncResult;

                    if (null == asyncResult)
                        continue;

                    //Check if the agent is a match for the current pending request
                    if (asyncResult.IsAgentMatch(agent))
                    {
                        //It is a match, allocate the Agent
                        _logger.Log("Allocating Agent " + agent.SignInAddress);

                        try
                        {
                            //It is a match, allocate the Agent
                            agent.Allocate(pendingRequestQueueItem._Requestor);
                            _logger.Log("Agent " + agent.SignInAddress + " is allocated to :" + agent.Owner.ToString());
                        }
                        catch (InvalidOperationException ex)
                        {
                            //this is a race condition where an agent got allocated while the pending 
                            // request was still in the queue
                            _logger.Log("AcdAgentMatchMaker failed allocation of Agent " + agent.SignInAddress, ex);

                            return;
                        }

                        // if we allocate the agent, we need to remove the pending request from the queue
                        pendingRequestToBeRemoved = pendingRequestQueueItem;

                        //Stop the timer
                        TimerItem tmrItem = pendingRequestQueueItem._TmrItem;
                        if (null != tmrItem)
                            tmrItem.Stop();
                        break;

                    }
                }

                //Attempt to remove the pending operation from the queue
                //If it fails removing the pending request, the match maker is probably in terminating state
                //in which case, shut down will take care of the clean up.
                if (null != pendingRequestToBeRemoved)
                {
                    //let's remove the pend operation first to ensure that the async result does not
                    //get executed twice in a row.
                    this.RemoveFromQueue(pendingRequestToBeRemoved);
                    //Complete the async operation with success
                    pendingRequestToBeRemoved.FindAgentAsyncResult.SetAsCompleted(agent, false);
                }
            }
          }

        /// <summary>
        /// Determines whether an Agent is available or not based on a Presence State change
        /// notification.
        /// </summary>
        /// <param name="notification">presence state change notification associated with an agent</param>
        private bool IsAgentAvailable(RemotePresentityNotification notification)
        {
            bool isAgentAvailable = false;
            long availability = notification.AggregatedPresenceState.AvailabilityValue;
            _logger.Log("Presence Changed: " + notification.PresentityUri + ": " + availability.ToString());
            
           //return true only if the availability is 
           return isAgentAvailable =  (availability > (long)PresenceAvailability.Online && availability < (long)PresenceAvailability.Busy);
        }

        /// <summary>
        /// LookupAgent returns the Agent corresponding to the SIP URI supplied in parameter
        /// </summary>
        /// <param name="sipUri"> SIP URI of the Agent </param>
        /// <returns></returns>
        internal Agent LookupAgent(string sipUri)
        {

            // Enumerate all the agents and attempt to find a match.
            foreach (Agent agent in _configuration.Agents)
            {
                if (SipUriCompare.Equals(sipUri, agent.SignInAddress))
                   return agent;
            }
            return null;
        }
        #endregion

        #region FindAgentAsyncResult
        /// <summary>
        /// Asynchronous operation to find and allocate an available Agent.
        /// </summary>
        private class FindAgentAsyncResult : AsyncResult<Agent>
        {
            private AcdAgentMatchMakerConfiguration _configuration;
            private AcdAgentMatchMaker _matchMaker;
            private List<Agent> _exclusionList;
            private List<AgentSkill> _requestedSkills;
            private PendingAgentMatchRequestQueueItem _queueItem;
            private AcdCustomerSession _requestor;

            internal FindAgentAsyncResult(AsyncCallback asyncCallback, Object state, AcdCustomerSession requestor, AcdAgentMatchMaker matchMaker, List<Agent> exclusionList, List<AgentSkill> requestedSkills)
            : base(asyncCallback, state)
            {
                _matchMaker = matchMaker;
                _configuration = matchMaker._configuration;
                _exclusionList = exclusionList;
                _requestedSkills = requestedSkills;
                _requestor = requestor;
            }

            internal void Process()
            {
                Agent agent = null;

                // Try to find a match synchronously
                agent = MatchAndAllocate();
             
                if (agent != null)
                {
                    // Completes the operation synchronously
                    this.SetAsCompleted(agent, false);
                }
                else
                {
                    //Add the agent match request to the list of pending requests and start a timer
                    lock (_matchMaker._syncRoot)
                    {
                        if (_matchMaker._matchMakerState < MatchMakerState.Terminating)
                        {
                            // Using the match maker timer wheel to trigger the operation time out.
                            TimerItem timerItem = new TimerItem(_matchMaker._timerWheel, new TimeSpan(0, 0, 0, _matchMaker._configuration.MaxWaitTimeOut, 0));
                            timerItem.Expired += new EventHandler(HandlePendingRequestTimeOut);
                            timerItem.Start();

                            //create a queue item and cache a reference
                            _queueItem = new PendingAgentMatchRequestQueueItem(_requestor,this, timerItem);

                            //add the queueItem to the queue
                            _matchMaker._pendingAgentRequestQueueItems.Add(_queueItem);
                            return;
                        }
                    }

                    this.SetAsCompleted(new OperationFailureException("AcdAgentMatchMaker is terminating and cannot process any new async requests"), false);

                }
            }

            /// <summary>
            /// Handles the case where no agent match could be done in time.
            /// </summary>
            void HandlePendingRequestTimeOut(object sender, EventArgs args)
            {
                if (_matchMaker.RemoveFromQueue(_queueItem))
                {
                    TimerItem timerItem = sender as TimerItem;
                    timerItem.Stop();

                    _matchMaker._logger.Log("AcdAgentMatchMaker could not find a match in time");

                    // complete the asynchronous operation
                    this.SetAsCompleted(new TimeoutException("AcdAgentMatchMaker could not find a match in time"), false);
                }
            }

            /// <summary>
            /// Determines whether there is an Agent Match in the pool of agents
            /// </summary>
            private Agent MatchAndAllocate()
            {
                //loops through all agents and find the first available agent.               
                //UNDONE: Nice to have: Add randomization to the allocation of the agents.  Right now, it is in order of the portalsConfiguration.
                //UNDONE: Nice to have: Handle call severity escalation.  
                //        E.G. If the exclusion list includes all possible agents 
                //        because each agent declined to answer the call, escalate the call to each agent
                //        with the text that there exists no other agent available.  
                //        Without this piece, a client call can wait indefinitly if all agents decline the call.
                foreach (Agent agent in _configuration.Agents)
                {
                    if (IsAgentMatch(agent))
                    {
                        //It is a match, allocate the Agent
                        _matchMaker._logger.Log("Allocating Agent " + agent.SignInAddress);

                        try
                        {
                            agent.Allocate(_requestor);
                            _matchMaker._logger.Log("Agent " + agent.SignInAddress + " is allocated to :" + agent.Owner.ToString());
                            return agent;
                        }
                        catch (InvalidOperationException ex)
                        {
                            _matchMaker._logger.Log("AcdAgentMatchMaker failed the synchronous allocation of Agent " + agent.SignInAddress, ex);
                        }
                    }
                }
                //None of the agents matched
                return null;
            }

            /// <summary>
            /// Indicates whether an available Agent has the requested skills or not for a 
            /// given pending match request.
            /// </summary>
            internal bool IsAgentMatch(Agent agent)
            {
                bool isMatch = true;

                if (null != _exclusionList)
                {
                    if (_exclusionList.Contains(agent))
                        return false;
                }

                //only match online agents
                if (!agent.IsOnline)
                {
                    return false;
                }

                //only match unallocated agents
                if (agent.IsAllocated)
                {
                    return false;
                }

                //Check that the agent has each required skill
                foreach (AgentSkill agentSkill in _requestedSkills)
                {
                    if (!agent.Skills.Contains(agentSkill))
                    {
                        isMatch = false;
                        break;
                    }
                }
                return isMatch;
            }
        }
        #endregion FindAgent async result


        #region Startup async result
        /// <summary>
        /// AcdAgentMatchMaker start-up async result registers an ApplicationEndpoint. Upon successful registration
        /// it subscribes to the Presence of Remote Agents.
        /// </summary>
        private class StartupAsyncResult : AsyncResultNoResult
        {
            AcdAgentMatchMaker _matchMaker;

            internal StartupAsyncResult(AsyncCallback asyncCallback, Object state, AcdAgentMatchMaker matchMaker)
                : base(asyncCallback, state)
            {
                _matchMaker = matchMaker;
            }

            internal void Process()
            {
                try
                {
                    _matchMaker._endpoint.StateChanged+= matchMakerStateChanged;
                    _matchMaker._endpoint.RegisterForIncomingCall<InstantMessagingCall>(_matchMaker.HandleIncomingInstantMessagingCalls);
                    _matchMaker._endpoint.BeginEstablish(this.OnEstablishComplete, null);
                }
                catch (RealTimeException ex)
                {
                    this.SetAsCompleted(ex, false);
                }
            }
            /// <summary>
            /// Starts subscribing to the Presence of agents upon successful registration of the endpoint
            /// </summary>
            private void OnEstablishComplete(IAsyncResult result)
            {
                try
                {
                    _matchMaker._endpoint.EndEstablish(result);

                    //create a SubscriptionTarget for each agent
                    RemotePresentitySubscriptionTarget[] contacts = new RemotePresentitySubscriptionTarget[_matchMaker._configuration.Agents.Count];
                    for (int i = 0; i < _matchMaker.Configuration.Agents.Count; i++)
                    {
                        contacts[i] = new RemotePresentitySubscriptionTarget(_matchMaker._configuration.Agents[i].SignInAddress);
                    }


                    RemotePresenceView MatchMakerPresence = new RemotePresenceView(_matchMaker._endpoint);

                    //Initiate the persistent batch subscription to the list of agents
                    //Only interested in subscribing to the Agents Availability. We should not expect more than one category instance
                    //per Remote Presentity notification change. Always register the event handler before starting the subscription.
                    MatchMakerPresence.PresenceNotificationReceived += _matchMaker.HandleAgentAvailabilityChanged;


                    MatchMakerPresence.StartSubscribingToPresentities(contacts);

                    try
                    {
                        _matchMaker._mohServer = new AcdMusicOnHoldServer(_matchMaker, _matchMaker.Configuration.MusicOnHoldFilePath, _matchMaker._logger);

                        _matchMaker._mohServer.BeginStartUp(ar =>
                                                            {
                                                                AcdMusicOnHoldServer mohServer = ar.AsyncState as AcdMusicOnHoldServer;

                                                                mohServer.EndStartUp(ar);

                                                                lock (_matchMaker._syncRoot)
                                                                {
                                                                    _matchMaker.UpdateState(MatchMakerState.Started);
                                                                }

                                                                this.SetAsCompleted(null, false);

                                                            },
                                                            _matchMaker._mohServer);


                    }
                    catch (RealTimeException ex)
                    {
                        _matchMaker._logger.Log("AcdAgentMatchMaker failed to subscribe to the Presence of its Agents", ex);
                        _matchMaker.BeginShutdown((asyncResult) =>
                                                  {
                                                      _matchMaker.EndShutdown(asyncResult);
                                                      this.SetAsCompleted(ex, false);
                                                  },
                                                  ex);
                    }
                }
                catch (RealTimeException ex)
                {
                    _matchMaker._logger.Log("AcdAgentMatchMaker failed to subscribe to the Presence of its Agents; verify your agent configuration.", ex);
                    this.SetAsCompleted(ex, false);
                    _matchMaker.BeginShutdown((asyncResult) =>
                                                {
                                                    _matchMaker.EndShutdown(asyncResult);
                                                    this.SetAsCompleted(ex, false);

                                                },
                                               ex);
                }
            }

            /// <summary>
            /// matchMakerStateChanged is the event handler that takes care of detecting when the matchmaker
            /// gets removed from the data base. When this happens, there is no other option but to shutdown the
            /// platform
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="args"></param>
            private void matchMakerStateChanged(object sender, LocalEndpointStateChangedEventArgs args)
            {
                AcdPlatform platform = _matchMaker._platform;
                AcdLogger logger = _matchMaker._logger;

                if (args.Reason == LocalEndpointStateTransitionReason.OwnerDisabledOrRemoved)
                {
                    if (null != platform)
                    {
                        platform.BeginShutdown(ar => { platform.EndShutdown(ar); }, null);

                        if (null != logger)
                            logger.Log("AcdAgentMatchMaker: Contact has been moved or removed, shutting down the application.");

                    }
                    else
                    {
                        if (null != logger)
                          _matchMaker._logger.Log("AcdAgentMatchMaker: Contact has been moved or removed, but the platform is null. Cannot shut down the application.");
                    }
                }
            
            }
        }

        #endregion Startup async operation

        #region Shutdown async operation

        /// <summary>
        /// ShutdownAsyncResult takes care of terminating the AcdAgentMatchMaker and clean-up all resources
        /// related to the subscription to Agents
        /// </summary>
        internal class ShutdownAsyncResult : AsyncResultNoResult
        {
            private AcdAgentMatchMaker _matchMaker;

            internal ShutdownAsyncResult(AsyncCallback asyncCallback, Object state, AcdAgentMatchMaker matchMaker)
                : base(asyncCallback, state)
            {
                _matchMaker = matchMaker;
            }

            internal void Process()
            {
                _matchMaker.CleanupPendingRequests();

                _matchMaker._sessions.ForEach(session => { session.BeginShutDown(sd => { session.EndShutDown(sd); }, null); });

                _matchMaker._endpoint.UnregisterForIncomingCall<InstantMessagingCall>(_matchMaker.HandleIncomingInstantMessagingCalls);
                //_matchMaker._endpoint.UnregisterForIncomingCall<ApplicationSharingCall>(_matchMaker.HandleIncomingApplicationSharingCalls);

                //terminate the endpoint, this will also terminate the Presence subscription
                _matchMaker._endpoint.BeginTerminate(HandleEndpointTermination, _matchMaker._endpoint);
                
            }
            /// <summary>
            /// Finishes the endpoint termination. This will indirectly take care of cleaning the Presence subscription resource as well.
            /// </summary>
            private void HandleEndpointTermination(IAsyncResult result)
            {
                ApplicationEndpoint endpoint = result.AsyncState as ApplicationEndpoint;
                endpoint.EndTerminate(result);

                _matchMaker.UpdateState(MatchMakerState.Terminated);

                _matchMaker._listOfShutdownAsyncResults.ForEach( ar => ar.SetAsCompleted(null, false));
                  

                //Terminate operation never throws
                this.SetAsCompleted(null, false);
            }
        }
        #endregion Shutdown async operation
        /// <summary>
        /// Represents an item in the queue of pending Agent Match requests
        /// </summary>
        private class PendingAgentMatchRequestQueueItem
        {
            #region Properties
            internal FindAgentAsyncResult FindAgentAsyncResult {get; set; }
            internal TimerItem _TmrItem { get; set; }
            internal AcdCustomerSession _Requestor { get; set; }
            #endregion

            #region Methods
            internal PendingAgentMatchRequestQueueItem(AcdCustomerSession requestor, FindAgentAsyncResult findAgentAsyncResult, TimerItem tmrItem)
            {
                _Requestor = requestor;
                this.FindAgentAsyncResult = findAgentAsyncResult;
                _TmrItem = tmrItem;
            }

            internal void StopTimer()
            {
                TimerItem timer = _TmrItem;

                if (timer != null)
                {
                    timer.Stop();
                }
            }
            #endregion
        }

    }

    internal enum MatchMakerState { Created, Starting, Started, Terminating, Terminated };

    internal class MatchMakerStateChangedEventArgs : EventArgs
    {
        private MatchMakerState _previousState;
        private MatchMakerState _newState;

        internal MatchMakerStateChangedEventArgs(MatchMakerState previousState, MatchMakerState newState)
        {
            _previousState = previousState;
            _newState = newState;
        }

        internal MatchMakerState PreviousState
        {
            get { return _previousState; }
        }

        internal MatchMakerState NewState
        {
            get { return _newState; }
        }
    }
}
