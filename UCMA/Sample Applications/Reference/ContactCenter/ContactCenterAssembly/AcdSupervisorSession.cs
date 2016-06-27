/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter;
using System.Diagnostics;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.Samples.Utilities;
using System.Threading;
using Microsoft.Rtc.Collaboration.AudioVideo;
using System.Net.Mime;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Agents;
using System.Collections.ObjectModel;
using Microsoft.Rtc.Collaboration.Samples.ApplicationSharing;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    internal class AcdSupervisorSession
    {
        #region Private Members

        private AcdAgentMatchMaker _matchMaker;
        private Supervisor _supervisor;
        private AcdLogger _logger;
        private SupervisorContextChannel _supervisorControlChannel;
        private InstantMessagingCall _initialSupervisorCall;
        private Conversation _frontEndConversation;
        private SupervisorSessionState _state;
        private object _syncRoot = new object();
        private List<ShutDownAsyncResult> _listOfShutDownAsyncResults = new List<ShutDownAsyncResult>();
        private List<Agent> _listOfTrackedAgents = new List<Agent>();
        private const int AgentActivityRefreshPollingValue = 5;
        private bool _forceAgentActivityRefresh = false;
        private AcdCustomerSession _agentSessionToMonitor;
        private AcdMonitoringSession _monitoringSession;
        private Agent _agentToMonitor;
        private TimerWheel _wheel = new TimerWheel();
        private TimerItem _tmrItem;

        #endregion

        #region Constructor

        internal AcdSupervisorSession(AcdAgentMatchMaker matchMaker, Supervisor supervisor, AcdLogger logger)
        {
            _matchMaker = matchMaker;
            _supervisor = supervisor;
            _logger = logger;
        }

        #endregion

        #region Internal Properties

        internal event EventHandler<SupervisorSessionStateChangedEventArgs> StateChanged;

        internal SupervisorSessionState State
        {
            get {return _state;}
        }

        internal AcdMonitoringSession MonitoringSession
        {
            get { return _monitoringSession; }
        }

        #endregion

        #region Internal Methods

        internal void HandleSupervisorInitialCall(InstantMessagingCall imCall)
        {
            Debug.Assert(imCall is InstantMessagingCall);
            _logger.Log("AcdSupervisorSession receives first incoming Call from " + imCall.RemoteEndpoint.Participant.Uri);

            if (_state == SupervisorSessionState.Incoming)
            {

                // sets the initial customer call
                _initialSupervisorCall = imCall;

                // monitors the customer-facing conversation state
                this.SetFrontEndConversationProperties(imCall);

                try
                {

                    imCall.BeginAccept(ar =>
                    {
                        try
                        {
                            imCall.EndAccept(ar);

                            this.InitializeSupervisorControlChannel();

                        }
                        catch (RealTimeException rtex)
                        {
                            _logger.Log("AcdSupervisorSession failed to end accept the incoming call from Supervisor", rtex);
                            this.BeginShutDown(sdar => { this.EndShutDown(sdar); }, null);
                        }
                    },
                    null);
                }
                catch (InvalidOperationException ivoex)
                {
                    _logger.Log("AcdSupervisorSession failed to accept the incoming call from Supervisor", ivoex);
                    this.BeginShutDown(sdar => { this.EndShutDown(sdar); }, null);
                }

            }
            else
            {
                _logger.Log("AcdSupervisorSession is not in the correct state to process the new initial call");
                this.BeginShutDown(ar => { this.EndShutDown(ar); }, null);
            }

        }

 

        internal IAsyncResult BeginShutDown(AsyncCallback userCallback, object state)
        {
            ShutDownAsyncResult ar = new ShutDownAsyncResult(this, userCallback, state);

            bool firstTime = false;

            lock (_syncRoot)
            {
                if (_state < SupervisorSessionState.Terminating)
                {
                    this.UpdateState(SupervisorSessionState.Terminating);
                    firstTime = true;
                }
                else if (_state == SupervisorSessionState.Terminating)
                {
                    _listOfShutDownAsyncResults.Add(ar);

                }
                else if (_state == SupervisorSessionState.Terminated)
                {
                    ar.SetAsCompleted(null, true);
                }
            }

            if (firstTime)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    ShutDownAsyncResult tempAr = waitState as ShutDownAsyncResult;
                    tempAr.Process();
                }, ar);
            }

            return ar;
        }

        internal void EndShutDown(IAsyncResult ar)
        { 
            ShutDownAsyncResult result = ar as ShutDownAsyncResult;
            result.EndInvoke();        
        }

        internal IAsyncResult BeginStartMonitoringSession(MonitoringChannel channel, Agent agentToMonitor, AsyncCallback userCallback, object state)
        {

            StartMonitoringSessionAsyncResult asyncResult = new StartMonitoringSessionAsyncResult(channel, agentToMonitor, this, userCallback, state);

            bool Process = false;

            lock(_syncRoot)
            {
               if (_state == SupervisorSessionState.GeneralAgentActivityTracking)
               {
                 this.UpdateState(SupervisorSessionState.JoiningCustomerSession);
                 Process = true;
               }
               else
               {
                 throw new InvalidOperationException("AcdSupervisorSession is not in the correct state to start monitoring an agent");
               }
            
            }

            if (Process)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    var tempAr = waitState as StartMonitoringSessionAsyncResult;
                    tempAr.Process();
                }, asyncResult);            
            }
            return asyncResult;
        }

        internal void EndStartMonitoringSession(IAsyncResult ar)
        {
            StartMonitoringSessionAsyncResult asyncResult = ar as StartMonitoringSessionAsyncResult;

            asyncResult.EndInvoke();
        
        }
        #endregion
      
        #region Private methods

        /// <summary>
        /// Updates the state of the current AcdCustomerSession
        /// </summary>
        private void UpdateState(SupervisorSessionState state)
        {
            SupervisorSessionState previousState = _state;
            lock (_syncRoot)
            {
                switch (state)
                {
                    case SupervisorSessionState.Incoming:
                        _state = state;
                        break;

                    case SupervisorSessionState.GeneralAgentActivityTracking:
                        if (_state == SupervisorSessionState.JoiningCustomerSession
                            || _state == SupervisorSessionState.Incoming
                            || _state == SupervisorSessionState.AgentMonitoring)
                        {
                            _state = state;
                        }
                        break;

                    case SupervisorSessionState.JoiningCustomerSession:
                        if (_state == SupervisorSessionState.GeneralAgentActivityTracking)
                        {
                            _state = state;
                        }
                        break;

                    case SupervisorSessionState.AgentMonitoring:
                        if (_state == SupervisorSessionState.JoiningCustomerSession)
                        {
                            _state = state;
                        }
                        break;

                    case SupervisorSessionState.Terminating:
                        if (_state < SupervisorSessionState.Terminating)
                        {
                            _state = state;
                        }
                        break;
                    case SupervisorSessionState.Terminated:
                        if (_state == SupervisorSessionState.Terminating)
                        {
                            _state = state;
                        }
                        break;
                }
            }

            EventHandler<SupervisorSessionStateChangedEventArgs> handler = this.StateChanged;
            if (handler != null)
            {
                handler(this, new SupervisorSessionStateChangedEventArgs(previousState, state));
            }
        }

        private void OnMonitoringSessionTerminated(object sender, MonitoringSessionStateChangedEventArgs args)
        {
            AcdMonitoringSession monitoringSession = sender as AcdMonitoringSession;
            switch (args.NewState)
            {
                case MonitoringSessionState.Terminating:
                    _agentSessionToMonitor = null;
                    _agentToMonitor = null;
                    this.UpdateState(SupervisorSessionState.GeneralAgentActivityTracking);

                    break;

                case MonitoringSessionState.Terminated:
                    monitoringSession.StateChanged-=this.OnMonitoringSessionTerminated;
                    break;
            }
        }
       
        private void SetFrontEndConversationProperties(InstantMessagingCall incomingCall)
        {
            _frontEndConversation =  incomingCall.Conversation;
            _frontEndConversation.ApplicationContext = this;
            _frontEndConversation.StateChanged += this.HandleFrontEndConversationStateChanged;
        }

        private void InitializeSupervisorControlChannel()
        {
            _supervisorControlChannel = new SupervisorContextChannel(_frontEndConversation, _initialSupervisorCall.RemoteEndpoint);

            _supervisorControlChannel.RequestReceived+= ProcessSupervisorControlChannelRequestReceived;
            _supervisorControlChannel.StateChanged+= ProcessSupervisorStateChanged;

            
            try
            {
                _supervisorControlChannel.BeginEstablish(_matchMaker.Configuration.SupervisorDashboardGuid,
                                                         est =>
                {
                    try
                    {
                      _supervisorControlChannel.EndEstablish(est);
                                          

                      this.UpdateState(SupervisorSessionState.GeneralAgentActivityTracking);

                      this.InitializeAgentTracking();


                    }
                    catch(RealTimeException rtex)
                    {
                        _logger.Log("AcdSupervisorSession fails to end establish a supervisor session", rtex);
                         this.BeginShutDown(sd => { this.EndShutDown(sd); }, null);
                        return;                      
                    }
                },
                                                          null);
            
            }
            catch(InvalidOperationException ivoex)
            {
                _logger.Log("AcdSupervisorSession fails to begin establish a supervisor session", ivoex);
                 this.BeginShutDown(sd => { this.EndShutDown(sd); }, null);
                return;
            }
        }

        private void ProcessSupervisorStateChanged(object sender, ConversationContextChannelStateChangedEventArgs args)
        {
            if (args.State == ConversationContextChannelState.Terminating)
            {
                this.BeginShutDown(sd => { this.EndShutDown(sd); }, null);
            
            }
        
        }

        private void ProcessSupervisorControlChannelRequestReceived(object sender, ContextChannelRequestReceivedEventArgs args)
        {
            _logger.Log("RECEIVED REQUEST: AcdSupervisorSession received the following request: " + args.RequestType);

            switch (args.RequestType)
            { 
                case ContextChannelRequestType.StartMonitoring:

                    MonitoringRequest startMonitoringRequest = args.Request as MonitoringRequest;

                    Agent agentToMonitor = null;

                    foreach(Agent agent in _matchMaker.Configuration.Agents)
                    {
                        if (SipUriCompare.Equals(agent.SignInAddress, startMonitoringRequest.MonitoringChannel.Uri.ToString()))
                        {
                            agentToMonitor = agent;
                            break;                        
                        }
                    }

                    if (null == agentToMonitor)
                    {
                      _logger.Log("AcdSupervisorSession could not find the agent to monitor.");
                      args.Request.SendResponse("failure");
                      return;
                    }

                    if (agentToMonitor.IsAllocated == false)
                    {
                        _logger.Log(String.Format("AcdSupervisorSession detected that the agent to monitor was deallocated {0}", agentToMonitor.SignInAddress));
                        args.Request.SendResponse("failure");
                        return;
                    }
                    if (_state == SupervisorSessionState.AgentMonitoring)
                    {
                        _monitoringSession.BeginShutDown(sd =>
                        {
                            _monitoringSession.EndShutDown(sd);
                            _monitoringSession = null;
                            args.Request.SendResponse("Success");

                            this.UpdateState(SupervisorSessionState.GeneralAgentActivityTracking);

                            try
                            {

                                this.BeginStartMonitoringSession(startMonitoringRequest.MonitoringChannel,
                                                                 agentToMonitor,
                                sms =>
                                {
                                    try
                                    {
                                        this.EndStartMonitoringSession(sms);
                                        args.Request.SendResponse("success");
                                        this.UpdateState(SupervisorSessionState.AgentMonitoring);
                                        return;
                                    }
                                    catch (RealTimeException rtex)
                                    {
                                        _logger.Log("AcdSupervisorSession fails to end start the monitoring session", rtex);
                                        args.Request.SendResponse("failure");
                                        this.UpdateState(SupervisorSessionState.GeneralAgentActivityTracking);
                                        return;
                                    }
                                },
                                null);
                            }
                            catch (InvalidOperationException ivoex)
                            {
                                _logger.Log("AcdSupervisorSession fails to begin start the monitoring session", ivoex);
                                args.Request.SendResponse("failure");
                                this.UpdateState(SupervisorSessionState.GeneralAgentActivityTracking);
                                return;

                            }





                        },
                                                       null);
                        return;
                    }

                    try
                    {

                        this.BeginStartMonitoringSession(startMonitoringRequest.MonitoringChannel,
                                                         agentToMonitor,
                                                         sms =>
                        {
                          try
                          {
                               this.EndStartMonitoringSession(sms);
                               args.Request.SendResponse("success");
                               lock(_syncRoot)
                               {
                                 if(_state == SupervisorSessionState.JoiningCustomerSession)
                                 {
                                   this.UpdateState(SupervisorSessionState.AgentMonitoring);
                                 }
                               }
                          }
                          catch(RealTimeException rtex)
                          {
                              _logger.Log("AcdSupervisorSession fails to end start the monitoring session", rtex);
                              args.Request.SendResponse("failure");
                              this.UpdateState(SupervisorSessionState.GeneralAgentActivityTracking);
                          }
                        },
                        null);
                    }
                    catch(InvalidOperationException ivoex)
                    {
                      _logger.Log("AcdSupervisorSession fails to begin start the monitoring session", ivoex);
                      args.Request.SendResponse("failure");
                      this.UpdateState(SupervisorSessionState.GeneralAgentActivityTracking);

                    }
 
                    break;

                case ContextChannelRequestType.StopMonitoring:
                    
                      if (_state != SupervisorSessionState.AgentMonitoring)
                      {

                        _logger.Log("AcdSupervisorSession fails to stop monitoring the session as the supervisorsession is in an invalid state");
                        args.Request.SendResponse("Failure");
                      }
                      else
                      {
                         _monitoringSession.BeginShutDown(sd =>
                         {
                           _monitoringSession.EndShutDown(sd);
                           _monitoringSession = null;
                           args.Request.SendResponse("Success");
                           this.UpdateState(SupervisorSessionState.GeneralAgentActivityTracking);
                         },
                         null);
                      
                      }

                    break;

                default:
                    _logger.Log("AcdSupervisorSession received a non-supported command");
                    args.Request.SendResponse("Failure");
                    break;
            
            }
         
        
        }

        private void InitializeAgentTracking()
        {
            //Populate a list of agents assigned to Supervisor
            _matchMaker.Configuration.Agents.ForEach(agent =>
            {
                if (SipUriCompare.Equals(agent.SupervisorUri, this._supervisor.SignInAddress))
                {
                    _listOfTrackedAgents.Add(agent);
                }
            });

            //send the initial activity status to the Supervisor
            this.SendFullAgentActivityStatus();

            //Fire a timer to send the agent activity deltas
            _tmrItem = new TimerItem(_wheel, new TimeSpan(0, 0, 0, AgentActivityRefreshPollingValue));
            _tmrItem.Expired += RefreshAgentActivity;
            _tmrItem.Start();
        }

        private void SendFullAgentActivityStatus()
        {
            List<agentType> listOfAgentInfo = new List<agentType>();

            _listOfTrackedAgents.ForEach(agent =>
            {
                agentType agentInfo = Agent.Convert(agent, null);
                listOfAgentInfo.Add(agentInfo);

            });

            try
            {
                _supervisorControlChannel.BeginUpdateAgents(listOfAgentInfo,
                                                            null,
                upa =>
                {
                    try
                    {
                        _supervisorControlChannel.EndUpdateAgents(upa);
                        _forceAgentActivityRefresh = false;
                    }
                    catch (RealTimeException rtex)
                    {
                        _logger.Log("AcdSupervisorSession failed to end refresh the full activity deltas of the agents", rtex);
                        _forceAgentActivityRefresh = true;
                    }
                },
                null);
            }
            catch (InvalidOperationException ivoex)
            {
                _logger.Log("AcdSupervisorSession failed to begin refresh the full activity deltas of the agents", ivoex);
                _forceAgentActivityRefresh = true;

            }
        }


        private void RefreshAgentActivity(object sender, EventArgs args)
        {
            if (_forceAgentActivityRefresh)
            {
                this.SendFullAgentActivityStatus();
            }
            else
            {

                //Filtering out the agents that have not changed during the polling TimeSpan
                List<agentType> listOfAgentInfo = new List<agentType>();

                _listOfTrackedAgents.ForEach(agent =>
                {
                    if (agent.GetWhetherPropertiesChanged())
                    {
                        agentType agentInfo = new agentType();
                        agentInfo.uri = agent.SignInAddress;
                        agentInfo.displayname = new SipUriParser(agent.SignInAddress).User;
                        KeyValuePair<DateTime, bool> activityStatus = agent.GetWhetherAllocated();
                        agentInfo.status = activityStatus.Value ? "Allocated" : "Idle";
                        agentInfo.statuschangedtime = activityStatus.Key.Ticks.ToString();
                        listOfAgentInfo.Add(agentInfo);
                    }

                });


                try
                {
                 _supervisorControlChannel.BeginUpdateAgents(listOfAgentInfo,
                                                             null,
                    upa =>
                    {
                        try
                        {
                            _supervisorControlChannel.EndUpdateAgents(upa);
                            _forceAgentActivityRefresh = false;
                        }
                        catch (RealTimeException rtex)
                        {
                            _logger.Log("AcdSupervisorSession failed to end refresh the partial activity deltas of the agents", rtex);
                            _forceAgentActivityRefresh = true;
                        }
                    },
                    null);
                }
                catch (InvalidOperationException ivoex)
                {

                    _logger.Log("AcdSupervisorSession failed to begin refresh the partial activity deltas of the agents", ivoex);
                    _forceAgentActivityRefresh = true;

                }
            }

            _tmrItem.Reset();

        }


        private void HandleFrontEndConversationStateChanged(object sender, StateChangedEventArgs<ConversationState> args)
        {
            Conversation conversation = sender as Conversation;
            
            switch (args.State)
            {
                case ConversationState.Terminating:
                    this.BeginShutDown(sdar => { this.EndShutDown(sdar); }, null);
                    break;

                case ConversationState.Terminated:
                    _frontEndConversation.StateChanged -= HandleFrontEndConversationStateChanged;
                    break;


                default:
                    _logger.Log("Unhandled state " + args.State.ToString());
                    break;
            }
        }
        #endregion

        #region ShutDownAsyncResult

        private class ShutDownAsyncResult : AsyncResultNoResult
        {
            private AcdSupervisorSession _supervisorSession;

            internal ShutDownAsyncResult(AcdSupervisorSession session, AsyncCallback userCallback, object state): base(userCallback, state)
            {
                _supervisorSession = session;
            }

            internal void Process()
            {
                InstantMessagingCall imCall = _supervisorSession._initialSupervisorCall;

                if (null != _supervisorSession._tmrItem)
                {
                    _supervisorSession._tmrItem.Stop();
                }

                if (null != imCall)
                { 
                    if (null !=  imCall.Flow)
                    {
                        try
                        {
                            imCall.Flow.BeginSendInstantMessage(_supervisorSession._matchMaker.Configuration.FinalMessageToSupervisor,
                            smar =>
                            {
                                try
                                {
                                    imCall.Flow.EndSendInstantMessage(smar);
                                }
                                catch (RealTimeException)
                                {
                                    // TODO: _logger.Log("Failed to send final message to supervisor.");
                                }
                            },
                            null);
                         }
                         catch (InvalidOperationException)
                         {
                              // TODO: _logger.Log("Unable to send final message to supervisor, because the call flow terminated.");
                         }                      
                    }
                }
                
                _supervisorSession._frontEndConversation.BeginTerminate(ter =>
                {
                    _supervisorSession._frontEndConversation.EndTerminate(ter);
                    if (_supervisorSession._monitoringSession != null)
                    {
                        _supervisorSession._monitoringSession.BeginShutDown(sd =>
                        {
                            _supervisorSession._monitoringSession.EndShutDown(sd);

                            _supervisorSession.UpdateState(SupervisorSessionState.Terminated);

                            this.SetAsCompleted(null, false);

                            _supervisorSession._listOfShutDownAsyncResults.ForEach(sdar =>
                            {
                                sdar.SetAsCompleted(null, true);
                            });
                        },
                        null);
                    }

                },
                null);
            } // Process()
        }
        #endregion              

        #region StartMonitoringSessionAsyncResult
        private class StartMonitoringSessionAsyncResult:AsyncResultNoResult
        {
            AcdSupervisorSession _session;
            Agent _agentToMonitor;
            MonitoringChannel _channel;

            internal StartMonitoringSessionAsyncResult(MonitoringChannel channel, Agent agentToMonitor, AcdSupervisorSession session, AsyncCallback userCallback, object state)
              :base(userCallback, state)
            {
                _session = session;
                _agentToMonitor = agentToMonitor;
                _channel = channel;
            
            }

            internal void Process()
            {
                _session._agentSessionToMonitor = _agentToMonitor.Owner;
                _session._agentToMonitor = _agentToMonitor;

                _session._monitoringSession = new AcdMonitoringSession(_channel,_session, _agentToMonitor, _session._initialSupervisorCall, _session._logger, _session._matchMaker);
                _session._monitoringSession.StateChanged += _session.OnMonitoringSessionTerminated;

                    try
                    {
                        _session._monitoringSession.BeginStartUp(sar =>
                        {
                            try
                            {
                                _session._monitoringSession.EndStartUp(sar);
                                this.SetAsCompleted(null, false);
                            }
                            catch (RealTimeException rtex)
                            {
                                _session._logger.Log("AcdSupervisorSession: failed to end start monitoring the session.", rtex);
                                this.SetAsCompleted(rtex, false);
                            }
                        },
                                                          null);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _session._logger.Log("AcdSupervisorSession: failed to begin start monitoring the session.", ivoex);
                        this.SetAsCompleted(new OperationFailureException("AcdSupervisorSession: failed to begin start monitoring the session.", ivoex), false);
                    }
                }
            }
        #endregion
    }

    internal enum SupervisorSessionState 
    { 
        Incoming =0, 
        GeneralAgentActivityTracking =1, 
        JoiningCustomerSession =2 ,
        AgentMonitoring =3, 
        Terminating=4, 
        Terminated=5
    };

    internal class SupervisorSessionStateChangedEventArgs : EventArgs
    {
        private SupervisorSessionState _previousState;
        private SupervisorSessionState _newState;

        internal SupervisorSessionStateChangedEventArgs(SupervisorSessionState previousState, SupervisorSessionState newState)
        {
            _previousState = previousState;
            _newState = newState;
        }

        internal SupervisorSessionState PreviousState
        {
            get { return _previousState; }
        }

        internal SupervisorSessionState NewState
        {
            get { return _newState; }
        }
    }
}