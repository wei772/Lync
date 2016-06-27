/*=====================================================================
  File:      AcdPortal.cs

  Summary:   Manages a portal instance and all Calls within. 
             Handles all incoming calls and routes them appropriately.

/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Internal.Collaboration;
using Microsoft.Rtc.Collaboration.Samples.Utilities;
using Microsoft.Rtc.Collaboration.Samples.ApplicationSharing;
using System.Net.Mime;
using System.Xml.Linq;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Utilities;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;
using System.Xml;
using System.Threading;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    interface IAgentAvailabilityConsumer
    {
        void HandleAgentAvailabilityChanged(Agent agent, AgentAvailability availability);
    }

    enum AgentAvailability 
    { 
        Unavailable, 
        Available 
    };

    internal class AcdPortal
    {         
        internal event EventHandler<PortalStateChangedEventArgs> PortalStateChanged;
        internal delegate void PresenceChangedCallback(Agent agent, PresenceAvailability presence);
        internal int _numberOfSessionsEstablished = 0;
        internal int _numberOfIVRSessions = 0;
        internal int _numberOfMOHSessions = 0;

        private static readonly TimeSpan OneMinuteSpan = new TimeSpan(0, 1, 0);
        private static readonly TimeSpan FiveMinuteSpan = new TimeSpan(0, 5, 0);

        #region Private Members

        private AcdPlatform _acdPlatform;
        private string _uri;
        private AcdPortalConfiguration _configuration;
        private ApplicationEndpoint _endpoint;
        private AcdAgentMatchMaker _matchMaker;
        private PortalState _portalState;
        private List<AcdCustomerSession> _sessions;
        private AcdLogger _logger;
        private TimerWheel _wheel = new TimerWheel();
        private object _syncRoot = new object();
        private AcdAgentHunter _agentHunter;
        private ShutdownAsyncResult _shutdownAsyncResult;
        private List<ShutdownAsyncResult> _listOfShutdownAsyncResults = new List<ShutdownAsyncResult>();
        private string _anonymousSubscriberUri;
        private const int ContainerIDForWebUsers = 500;
        private TimerItem _averageQueueTimeRefreshTimer;
       
        #endregion Private members

        #region Constructor

        public AcdPortal(AcdPlatform platform, AcdPortalConfiguration config, ApplicationEndpointSettings endpointSettings)
        {
            _acdPlatform = platform;
            _configuration = config;
            _uri = config.Uri;
            _sessions = new List<AcdCustomerSession>();
            _logger = new AcdLogger();

            endpointSettings.AutomaticPresencePublicationEnabled = true;
            endpointSettings.Presence.PreferredServiceCapabilities = null;
            endpointSettings.UseRegistration = true;
            endpointSettings.SupportedMimePartContentTypes = new List<ContentType>() { new ContentType("application/octet-stream") };
            _endpoint = new ApplicationEndpoint(this.Platform.CollaborationPlatform, endpointSettings);
            _endpoint.InnerEndpoint.AddSipExtension("picav");

            this.UpdatePortalState(PortalState.Created);
        }
        #endregion Constructor

        #region Properties

        internal AcdAgentHunter AgentHunter
        {
            get { return _agentHunter; }
        }
        internal AcdPlatform Platform
        {
            get { return _acdPlatform; }
        }
        
        internal TimerWheel TmrWheel
        {
            get { return _wheel; }
        }

        internal AcdPortalConfiguration Configuration
        {
            get { return _configuration; }
        }

        internal ApplicationEndpoint Endpoint
        {
            get { return _endpoint; }
        }

        internal PortalState PortalState
        {
            get { return _portalState; }
        }

        internal string Uri
        {
            get { return _uri; }
        }

        #endregion Properties


        private void OnAnonymousSubscriberUriChanged(object sender, AcdPlatformAnonymousSubscriberUriChangedEventArgs args)
        {
            if (!SipUriCompare.Equals(args.AnonymousSubscriberUri, _anonymousSubscriberUri))
            {
                lock (_syncRoot)
                {

                   this.UpdatePresenceRelationshipWithAnonymousSubsriber(args.AnonymousSubscriberUri, _anonymousSubscriberUri);

                   _anonymousSubscriberUri = args.AnonymousSubscriberUri;
                }
            }
    
        }

        private void OnRepublishingRequired(object sender, RePublishingRequiredEventArgs args)
        {
            if (!String.IsNullOrEmpty(_anonymousSubscriberUri))
            {
                this.UpdatePresenceRelationshipWithAnonymousSubsriber(_anonymousSubscriberUri, null);
            }
        }


        private void UpdatePresenceRelationshipWithAnonymousSubsriber(string newAnonymousSubscriberUri, string oldAnonymousSubscriberUri)
        {

            SipUriParser parser;

            Debug.Assert(SipUriParser.TryParse(newAnonymousSubscriberUri, out parser));

            ContainerUpdateOperation AddACE = new ContainerUpdateOperation(ContainerIDForWebUsers);
            AddACE.AddUri(newAnonymousSubscriberUri);

            if (!String.IsNullOrEmpty(oldAnonymousSubscriberUri))
            {
                Debug.Assert(SipUriParser.TryParse(oldAnonymousSubscriberUri, out parser));
                AddACE.DeleteUri(oldAnonymousSubscriberUri);
            }

            List<ContainerUpdateOperation> listOfOperations = new List<ContainerUpdateOperation>();
            listOfOperations.Add(AddACE);

            try
            {

                _endpoint.LocalOwnerPresence.BeginUpdateContainerMembership(listOfOperations,
                    add =>
                    {
                        try
                        {
                          _endpoint.LocalOwnerPresence.EndUpdateContainerMembership(add);
                        }
                        catch(RealTimeException rtex)
                        {
                           _logger.Log("AcdPortal was unable to end update the container membership for Anonymous Subscriber", rtex);
                        }
                    },
                    null);
            }
            catch(InvalidOperationException ivoex)
            {
              _logger.Log("AcdPortal was unable to begin update the container membership for Anonymous Subscriber", ivoex);
            }
        }

        /// <summary>
        /// PublishPresenceToWebCustomers is a method that indicates Web Users different levels of Presence based on
        /// the average queue time, or whether the portal is getting drained.
        /// </summary>
        private void PublishPresenceToWebCustomers()
        {

            //Limitation: Note that this publication only makes sense if only one instance of the Application is in service.
            // If multiple instances of the application were running at the same time, one shall consider using a database to coordinate the publication
            // of the information.

            PresenceState aggregateState;

            if (   _portalState == PortalState.Started
                || _portalState == PortalState.Draining)
            {
                if (_portalState == ContactCenter.PortalState.Started)
                {
                    TimeSpan averageWaitTime = _agentHunter.AverageQueueTime.Value;
                    if (averageWaitTime <= AcdPortal.OneMinuteSpan)
                    {
                        var presenceActivity = new PresenceActivity(new LocalizedString(1033, String.Format("Expected Wait Time is \r\n {0} min and {1} sec",
                                                                                                                      averageWaitTime.Minutes,
                                                                                                                      averageWaitTime.Seconds)));

                        presenceActivity.SetAvailabilityRange(3500, 3500);
                        aggregateState = new PresenceState(PresenceStateType.AggregateState,
                                                           3500,
                                                           presenceActivity);
                    }
                    else if (averageWaitTime <= AcdPortal.FiveMinuteSpan)
                    {
                        var presenceActivity = new PresenceActivity(new LocalizedString(1033, String.Format("Expected Wait Time is \r\n {0} min and {1} sec",
                                                                                              averageWaitTime.Minutes,
                                                                                              averageWaitTime.Seconds)));
                        presenceActivity.SetAvailabilityRange(6500, 6500);

                        aggregateState = new PresenceState(PresenceStateType.AggregateState,
                                                          6500,
                                                          presenceActivity);
                    }
                    else
                    {

                        string activityString = null;
                        if (averageWaitTime.Hours > 0)
                        {
                            activityString = String.Format("Expect Long Wait Time; \r\n currently {0} hour {1} min and {2} sec", averageWaitTime.Hours, averageWaitTime.Minutes, averageWaitTime.Seconds);
                        }
                        else
                        {
                            activityString = String.Format("Expect Long Wait Time; \r\n currently {0} min and {1} sec", averageWaitTime.Minutes, averageWaitTime.Seconds);
                        }

                        var presenceActivity = new PresenceActivity(new LocalizedString(1033, activityString));
                        presenceActivity.SetAvailabilityRange(6500, 6500);

                        aggregateState = new PresenceState(PresenceStateType.AggregateState,
                                                          6500,
                                                          presenceActivity);
                    }

                }
                else
                {
                    var presenceActivity = new PresenceActivity(new LocalizedString(1033, "This service is not available at this time; please call again later."));
                    presenceActivity.SetAvailabilityRange(9500, 9500);
                    aggregateState = new PresenceState(PresenceStateType.AggregateState,
                                                      9500,
                                                      presenceActivity);
                }

                PresenceCategoryWithMetaData averageQueueTimePublication = new PresenceCategoryWithMetaData(1,
                                                                                                            ContainerIDForWebUsers,
                                                                                                            aggregateState);

                try
                {
                    _endpoint.LocalOwnerPresence.BeginPublishPresence(new List<PresenceCategoryWithMetaData>() { averageQueueTimePublication },
                                                                      pub =>
                                                                      {
                                                                          try
                                                                          {
                                                                              _endpoint.LocalOwnerPresence.EndPublishPresence(pub);

                                                                          }

                                                                          catch (OperationFailureException ofex)
                                                                          {
                                                                              _logger.Log("AcdPortal failed to end publish average queue time", ofex);
                                                                          }
                                                                          catch (PublishSubscribeException psex)
                                                                          {
                                                                              _logger.Log("AcdPortal failed to end publish average queue time", psex);
                                                                          }
                                                                          catch (RealTimeException rtex)
                                                                          {
                                                                              _logger.Log("AcdPortal failed to end publish average queue time", rtex);
                                                                          }

                                                                      },
                                                                      null);

                }
                catch (InvalidOperationException ivoex)
                {
                    _logger.Log("AcdPortal failed to begin publish average queue time", ivoex);
                }
            }
        }


        internal void OnPortalEndpointStateChanged(object sender, LocalEndpointStateChangedEventArgs args)
        {
            switch (args.Reason)
            {
                case LocalEndpointStateTransitionReason.OwnerDisabledOrRemoved:
                    this.BeginShutdown(sd => { this.EndShutdown(sd); }, null);
                    break;

            }
        }

        /// <summary>
        /// Registers event handlers to process incoming calls
        /// </summary>
        private void RegisterForIncomingCalls()
        {
            _endpoint.RegisterForIncomingCall<AudioVideoCall>(this.HandleAudioVideoCallReceived);
            _endpoint.RegisterForIncomingCall<InstantMessagingCall>(this.HandleInstantMessagingCallReceived);
            _endpoint.RegisterForIncomingCall<ApplicationSharingCall>(this.HandleApplicationSharingCallReceived);       
        }

        /// <summary>
        /// Handles incoming AudioVideoCall. The incoming call can be one of three things: a brand new conversation,
        /// a self transfer of an existing customer-facing call, an audio escalation. In Draining mode, only the incoming
        /// calls for existing Conversations will be processed. Initial Conversation calls coming in will be declined by the
        /// endpoint.
        /// </summary>
        private void HandleAudioVideoCallReceived(object sender, CallReceivedEventArgs<AudioVideoCall>  args)
        {
            //Determines whether it is a new Call
            if (args.IsNewConversation  && args.CallToBeReplaced == null)  //New Acd customer session
            {
                AcdCustomerSession session = new AcdCustomerSession(_logger, _matchMaker, this);

                //add the session to the list.
                lock (_syncRoot)
                {
                    _sessions.Add(session);
                    _numberOfSessionsEstablished++;
                }

                //registering for an event to monitor the customer session state in order to determine when the draining is complete.
                session.CustomerSessionStateChanged += CustomerSessionStateChanged;

                if (args.CustomMimeParts.Count > 0)
                {
                    productType productInformation;

                    List<AgentSkill> listOfRequestedSkills = new List<AgentSkill>(this.ProcessMimeParts(args.CustomMimeParts, out productInformation));

                    ThreadPool.QueueUserWorkItem((waitState) =>
                    {
                        session.HandleInitialCall(args.Call, listOfRequestedSkills, productInformation); 
                    });
                    
                }
                else
                {

                    ThreadPool.QueueUserWorkItem((waitState) =>
                    {
                       session.HandleInitialCall(args.Call, null, null);
                    });
                }
            }
            //it is not a new call but a modality addition
            else if (args.IsNewConversation == false && args.CallToBeReplaced == null)
            {
                if (null != args.Call.Conversation.ApplicationContext)
                {
                    //this an audio escalation, hand-off the call to the AcdCustomerSession for futher processing
                    AcdCustomerSession session = args.Call.Conversation.ApplicationContext as AcdCustomerSession;
                    args.RingBackDisabled = true;
                    session.HandleNewModality(args.Call);
                }
                else
                {
                    // There was an issue retrieving the AcdCustomerSession, we are declining the call.
                    try
                    {
                        args.Call.Decline(new CallDeclineOptions(ResponseCode.TemporarilyUnavailable));
                    }
                    catch (RealTimeException ex)
                    {
                        _logger.Log("AcdPortal cannot decline incoming audio call properly", ex);
                    }
                 }
            }
            else if (args.CallToBeReplaced != null)
            {
                try
                {
                    args.Call.Decline();
                }
                catch (RealTimeException ex)
                {
                    _logger.Log("AcdPortal failed declining an incoming call replacing an exisiting one", ex);
                }
            }
            else
            { 
              //unexpected. Decline the incoming call
                try
                {
                    args.Call.Decline();
                }
                catch (RealTimeException ex)
                {
                    _logger.Log("Acd customer session failed to decline an unexpected incoming Audio call", ex);
                }            
            }
        }

        /// <summary>
        /// Handles an incoming Instant Messaging call. The incoming Instant Messaging call can be one of two things:
        /// this can be a brand new customer session or a modality escalation. Else decline.
        /// </summary>
        private void HandleInstantMessagingCallReceived(object sender, CallReceivedEventArgs<InstantMessagingCall> args)
        {
           //Determines whether it is a new Call
            if (args.IsNewConversation == true)  //New acd customer session
            {
                //Decline the call if the portal is draining its calls, else hand it off to a newly created Acd Customer session.
                if (_portalState < PortalState.Draining)  //Only take new calls is we're not in the process of shutting down.
                {
                    AcdCustomerSession session = new AcdCustomerSession(_logger, _matchMaker, this);

                    //add the session to the list.
                    lock (_syncRoot)
                    {
                        _sessions.Add(session);
                        _numberOfSessionsEstablished++;
                    }
                    session.CustomerSessionStateChanged += CustomerSessionStateChanged;

                    if (args.CustomMimeParts.Count > 0)
                    {
                        productType productInfo;
                        List<AgentSkill> listOfRequestedSkills = new List<AgentSkill>(this.ProcessMimeParts(args.CustomMimeParts, out productInfo));

                        ThreadPool.QueueUserWorkItem((waitState) =>
                        {
                            session.HandleInitialCall(args.Call, listOfRequestedSkills, productInfo); 
                        });
                       
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem((waitState) =>
                        {
                            session.HandleInitialCall(args.Call, null, null);
                        });
                    }
                }
                else //If the call comes in while we're shutting down, terminate it
                {
                    //UNDONE: Nice to have: - Play a message to the user that the portal is shutting down.
                    try
                    {
                        args.Call.Decline(new CallDeclineOptions(ResponseCode.TemporarilyUnavailable));
                    }
                    catch (RealTimeException ex)
                    {
                        _logger.Log("AcdPortal cannot decline incoming IM call properly while draining/terminating the portal", ex);                
                    }
                }
            }
            else
            {
                //this an audio escalation, hand off the call to the AcdCustomerSession for futher processing
                if (null != args.Call.Conversation.ApplicationContext)
                {
                    AcdCustomerSession session = args.Call.Conversation.ApplicationContext as AcdCustomerSession;

                    ThreadPool.QueueUserWorkItem((waitState) =>
                    {
                        session.HandleNewModality(args.Call);
                    });

                }
                else
                {
                    // There was an issue retrieving the AcdCustomerSession, we are declining the call.

                    try
                    {
                        args.Call.Decline();
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.Log("AcdPortal cannot decline incoming IM call properly; the call must already be terminated", ex);                    
                    }
                    catch (RealTimeException ex)
                    {
                        _logger.Log("AcdPortal cannot decline incoming IM call properly", ex);
                    }
                 }
            }
        }

        private void OnAverageQueueTimeRefreshNeeded(object sender, EventArgs args)
        {
            if (_portalState == ContactCenter.PortalState.Started
                || _portalState == ContactCenter.PortalState.Draining)
            {
                this.PublishPresenceToWebCustomers();
                _averageQueueTimeRefreshTimer.Reset();
            }

        }

        private IEnumerable<AgentSkill> ProcessMimeParts(IEnumerable<MimePartContentDescription> contextualData, out productType productInformation)
        {
            List<AgentSkill> listOfRequestedSkills = new List<AgentSkill>();

            List<MimePartContentDescription> listOfMimeParts = new List<MimePartContentDescription>(contextualData);

            productType product = null;

            _logger.Log(String.Format("Processing MIME parts {0}", listOfMimeParts.Count));

            bool success = true;
            listOfMimeParts.ForEach(mp =>
            {
                if (mp.ContentType.Equals(new ContentType("application/octet-stream")))
                {
                    try
                    {

                        //Now deserialize and make sure we have right data.

                        XmlSerializer serializer = new XmlSerializer(typeof(productType));

                        product = SerializerHelper.DeserializeObjectFragment(mp.GetBody(), serializer) as productType;

                        if (product != null
                            && product.agentSkillsList != null
                            && product.agentSkillsList.Length > 0)
                        {

                            List<agentSkillType> list = new List<agentSkillType>(product.agentSkillsList);
                            list.ForEach(sk =>
                                    {
                                        _logger.Log(String.Format("Finding skill name {0}", sk.name));
                                        Skill skillFound = Skill.FindSkill(sk.name, _matchMaker.Configuration.Skills);
                                        if (null != skillFound)
                                        {
                                            if (!String.IsNullOrEmpty(sk.Value))
                                            {
                                                try
                                                {
                                                    AgentSkill agentSkill = new AgentSkill(skillFound, sk.Value);
                                                    listOfRequestedSkills.Add(agentSkill);
                                                    _logger.Log(String.Format("Adding agent skill {0} value = {1}", skillFound, sk.Value));
                                                }
                                                catch (ArgumentException aex)
                                                {
                                                    _logger.Log(String.Format("AcdPortal detected a skill value that is not provisioned {0}", sk.Value), aex);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _logger.Log(String.Format("Skill name {0}. Not found", sk.name));
                                        }
                                    });


                        }
                        else
                        {
                            _logger.Log("Product is null or agentSkillsList is empty");
                        }

                    }
                    catch (XmlException xex)
                    {
                        _logger.Log("AcdPortal detected a derialization isssue", xex);
                        success = false;
                    }

                }
                else
                {
                    _logger.Log("Invalid content type");
                }
            });

            
            productInformation = product;
            

            if (success)
                return listOfRequestedSkills;
            else
                return new List<AgentSkill>();
        }

        /// <summary>
        /// Handles an incoming Application Sharing call. The incoming Application  Sharing call can be one of two things:
        /// this can be a brand new customer session or a modality escalation. Else decline.
        /// </summary>
        private void HandleApplicationSharingCallReceived(object sender, CallReceivedEventArgs<ApplicationSharingCall> args)
        {
            //Declines if it is a brand new Conversation as we only expect, modality escalation in
            // an existing customer session for Application Sharing.
            if (args.IsNewConversation == true)
            {
                try
                {
                    args.Call.Decline(new CallDeclineOptions(ResponseCode.TemporarilyUnavailable));
                }
                catch (InvalidOperationException ivoex)
                {
                    _logger.Log("AcdPortal cannot decline an imcoming Application Sharing Call properly", ivoex);
                
                }
                catch (RealTimeException ex)
                {
                    _logger.Log("AcdPortal cannot decline an imcoming Application Sharing Call properly", ex);
                }
            }
            else if (args.IsNewConversation == false)
            {
                //this an Application Sharing escalation, hand off the call to the AcdCustomerSession for futher processing
                if (null != args.Call.Conversation.ApplicationContext)
                {
                    AcdCustomerSession session = args.Call.Conversation.ApplicationContext as AcdCustomerSession;
                    args.RingBackDisabled = true;

                    ThreadPool.QueueUserWorkItem((waitState) =>
                    {
                        session.HandleNewModality(args.Call);
                    });

                }
                else
                {
                    // There was an issue retrieving the AcdCustomerSession, we are declining the call.
                    try
                    {
                        args.Call.Decline(new CallDeclineOptions(ResponseCode.TemporarilyUnavailable));
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _logger.Log("AcdPortal cannot decline an imcoming Application Sharing Call properly", ivoex);

                    }
                    catch (RealTimeException ex)
                    {
                        _logger.Log("AcdPortal cannot decline incoming Application Sharing call properly", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Initiates the Acd portal start up operation
        /// </summary>
        internal IAsyncResult BeginStartUp(AsyncCallback userCallback, object state)
        {
            StartupAsyncResult asyncResult = new StartupAsyncResult(userCallback, state, this);

            bool firstTime = false;
            lock (_syncRoot)
            {
                if (_portalState == PortalState.Created)
                {
                    this.UpdatePortalState(PortalState.Starting);
                    _matchMaker = this.Platform.MatchMaker;
                    _agentHunter = new AcdAgentHunter(this, _matchMaker, _logger);
                    firstTime = true;
                }
                else
                { 
                   throw new InvalidOperationException("AcdPortal is already being started");
                }
            }

            if (firstTime)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    var tempAr = waitState as StartupAsyncResult;
                    tempAr.Process();
                }, asyncResult);
            }

            return asyncResult;

        }


        /// <summary>
        /// Ends the start up operation. May throw if called more than once.
        /// </summary>
        internal void EndStartUp(IAsyncResult ar)
        {
           var result = ar as StartupAsyncResult;
           result.EndInvoke();
        }

        /// <summary>
        /// Monitors the state of the portal.  If the portal is shutting down, trigger
        /// the shutdown asyncResult processing.
        /// </summary>
        internal void CustomerSessionStateChanged(object sender, CustomerSessionStateChangedEventArgs e)
        {
            if (e.NewState == CustomerSessionState.CollectingData && e.PreviousState == CustomerSessionState.Incoming)
            {
                lock (_syncRoot)
                {
                    _numberOfIVRSessions++;
                }
            }
            else if (e.NewState == CustomerSessionState.AgentMatchMaking && e.PreviousState == CustomerSessionState.CollectingData)
            {
                lock (_syncRoot)
                {
                    _numberOfIVRSessions--;
                    _numberOfMOHSessions++;
                }
            
            }
            else if (e.NewState == CustomerSessionState.ConnectedToAgent && e.PreviousState == CustomerSessionState.AgentMatchMaking)
            {
                lock (_syncRoot)
                {
                    _numberOfMOHSessions--;
                }
            
            }
            else if (e.NewState == CustomerSessionState.Terminating && e.PreviousState == CustomerSessionState.CollectingData)
            {
                lock (_syncRoot)
                {
                    _numberOfIVRSessions--;
                }
            }
            else if (e.NewState == CustomerSessionState.Terminating && e.PreviousState == CustomerSessionState.AgentMatchMaking)
            {
                lock (_syncRoot)
                {
                    _numberOfMOHSessions--;
                }

            }
            else if (e.NewState == CustomerSessionState.Terminated)
            {
                // maintains the active AcdCustomerSession by removing a terminated Acd Customer Session
                lock (_syncRoot)
                {
                    _sessions.Remove(sender as AcdCustomerSession);
                    _numberOfSessionsEstablished--;
                }

                //retrieve the session and unwire the event handler
                var session = sender as AcdCustomerSession;

                session.CustomerSessionStateChanged -= this.CustomerSessionStateChanged;

                if (_portalState >= PortalState.Draining)
                {
                    // if we hit 0, there is no Acd customer session left while draining. 
                    // we can finish terminating the portal.
                    if (_sessions.Count == 0)
                    {
                        //the acd customer sessions are drained.
                        if (null != _shutdownAsyncResult)
                        {
                            ThreadPool.QueueUserWorkItem((waitState) =>
                            {
                                var tempAr = waitState as ShutdownAsyncResult;
                                tempAr.Process();
                            }, _shutdownAsyncResult);
                        }
                        else
                        {
                            _logger.Log("AcdPortal is drained, but there is no async result. Unexpected.");
                        }
                    }
                }
            }

            _logger.Log(String.Format(" ACDPORTAL IVR SESSIONS: {0}, MOH SESSIONS: {1}, TOTAL SESSIONS: {2}", _numberOfIVRSessions, _numberOfMOHSessions, _numberOfSessionsEstablished));
        }
       
        /// <summary>
        /// Updates the portal state to the new state and fires an event to all listeners informing them of the state change
        /// </summary>
        private void UpdatePortalState(PortalState newState)
        {
            PortalState oldState = _portalState;
            _portalState = newState;
            if (PortalStateChanged != null)
            {
                PortalStateChanged(this, new PortalStateChangedEventArgs
                    (oldState, newState));
            }
        }

        /// <summary>
        /// Initiates the portal shutdown.
        /// </summary>
        internal IAsyncResult BeginShutdown(AsyncCallback userCallback, object state)
        { 
          var shutdownAsyncResult = new ShutdownAsyncResult(userCallback, state, this);

          lock (_syncRoot)
          {
              if (_portalState < PortalState.Draining)
              {
               
                  _logger.Log(String.Format("AcdPortal:AcdPortal {0} is draining.", this.Uri));
                  

                  //Entering Draining and setting the endpoint Draining Mode.

                  try
                  {
                      _endpoint.BeginDrain(sd => 
                      {
                          _endpoint.EndDrain(sd);
                           
                      }, 
                      null);
                  }
                  catch (InvalidOperationException ivoex)
                  {

                      _logger.Log("AcdPortal: the endpoint is not in the right state for draining.", ivoex);
                  }
                  this.UpdatePortalState(PortalState.Draining);

                  //verify if there is anything to drain
                  if (_sessions.Count == 0)
                  { 
                    //process the async result right away
                      ThreadPool.QueueUserWorkItem((waitState) =>
                      {
                          var tempAr = waitState as ShutdownAsyncResult;
                          tempAr.Process();
                      }, shutdownAsyncResult);
                  }

                  _shutdownAsyncResult = shutdownAsyncResult;
              }
              else if (_portalState == PortalState.Terminated)
              {
                  //completes the request synchronously
                  shutdownAsyncResult.SetAsCompleted(null, true);
              }
              else if (_portalState == PortalState.Draining || _portalState == PortalState.Terminating)
              {
                  _listOfShutdownAsyncResults.Add(shutdownAsyncResult);
              }
          }
          return shutdownAsyncResult;
        }

        /// <summary>
        /// Ends the portal shutdown operation
        /// </summary>
        internal void EndShutdown(IAsyncResult ar)
        {
            var result = ar as ShutdownAsyncResult;
            result.EndInvoke();
        }

        #region ShutdownAsyncResult
        /// <summary>
        /// Represents the portal shut-down operation
        /// </summary>
        private class ShutdownAsyncResult : AsyncResultNoResult
        {
            private AcdPortal _acdPortal;

            internal ShutdownAsyncResult(AsyncCallback userCallback, object state, AcdPortal portal): 
                base(userCallback, state)
            {
                _acdPortal = portal;
            }

            internal void Process()
            { 
                //Draining is done
                _acdPortal.UpdatePortalState(PortalState.Terminating);
                _acdPortal._endpoint.StateChanged-= _acdPortal.OnPortalEndpointStateChanged;

                
                //Dispose the TimerWheel
                _acdPortal.TmrWheel.Dispose();

                //unregister the events
                _acdPortal._endpoint.UnregisterForIncomingCall<InstantMessagingCall>(_acdPortal.HandleInstantMessagingCallReceived);
                _acdPortal._endpoint.UnregisterForIncomingCall<AudioVideoCall>(_acdPortal.HandleAudioVideoCallReceived) ;
                _acdPortal._endpoint.UnregisterForIncomingCall<ApplicationSharingCall>(_acdPortal.HandleApplicationSharingCallReceived) ;
 
                //Terminate the endpoint
                _acdPortal._endpoint.BeginTerminate(delegate(IAsyncResult ar)
                                                       {
                                                           _acdPortal._endpoint.EndTerminate(ar);
                                                           lock (_acdPortal._syncRoot)
                                                           {
                                                               _acdPortal.UpdatePortalState(PortalState.Terminated);
                                                           }

                                                           //complete the current shutdown operation
                                                           this.SetAsCompleted(null, false);

                                                           //if there are other pending shutdown operations in the queue, complete them
                                                           foreach (ShutdownAsyncResult result in _acdPortal._listOfShutdownAsyncResults)
                                                           {
                                                               result.SetAsCompleted(null, false);
                                                           }
                                                       },
                                                       null);

              
            }

        }
        #endregion

        #region StartAsyncResult

        /// <summary>
        /// Represents the portal start-up operation
        /// </summary>
        private class StartupAsyncResult : AsyncResultNoResult
        {
            AcdPortal _acdPortal;

            internal StartupAsyncResult(AsyncCallback asyncCallback, Object state, AcdPortal portal)
                : base(asyncCallback, state)
            {
                _acdPortal = portal;
            }

            internal void Process()
            {
              _acdPortal._endpoint.StateChanged += _acdPortal.OnPortalEndpointStateChanged;
              _acdPortal._endpoint.RepublishingRequired += _acdPortal.OnRepublishingRequired;

              _acdPortal._endpoint.InnerEndpoint.AddFeatureParameter("isAcd");

              //Register incoming calls event handlers
              _acdPortal.RegisterForIncomingCalls();
                           
              try
              {
                  _acdPortal._endpoint.BeginEstablish(this.OnEndpointEstablishComplete, null);
              }
              catch (InvalidOperationException ex)
              {
                  _acdPortal._logger.Log("AcdPortal failed to establish its endpoint", ex);
                  _acdPortal.BeginShutdown(OnShutdownComplete, null);
                  this.SetAsCompleted(new OperationFailureException("AcdPortal failed to establish its endpoint", ex), false);
              }
              catch (RealTimeException ex)
              {
                  _acdPortal._logger.Log("AcdPortal failed to establish its endpoint", ex);
                  _acdPortal.BeginShutdown(OnShutdownComplete, null);
                  this.SetAsCompleted(ex, false);
              }                                                                 
            }

            /// <summary>
            /// Finish the endpoint establishment, and notify the AgentManager to start listening for 
            /// presence notifications for the agents in the system.
            /// </summary>
            private void OnEndpointEstablishComplete(IAsyncResult ar)
            {
                try
                {
                    _acdPortal._endpoint.EndEstablish(ar);
                    _acdPortal._acdPlatform.AnonymousSubscriberUriChanged += _acdPortal.OnAnonymousSubscriberUriChanged;
                    _acdPortal.UpdatePortalState(PortalState.Started);
                    _acdPortal.PublishPresenceToWebCustomers();
                    _acdPortal._averageQueueTimeRefreshTimer = new TimerItem(_acdPortal._wheel, new TimeSpan(0, 0, 0, 15));
                    _acdPortal._averageQueueTimeRefreshTimer.Expired += _acdPortal.OnAverageQueueTimeRefreshNeeded;
                    _acdPortal._averageQueueTimeRefreshTimer.Start();

                    this.SetAsCompleted(null, false);
        
                }
                catch (RealTimeException ex)
                {
                    _acdPortal._logger.Log("AcdPortal failed establishing the endpoint",ex);
                    _acdPortal.BeginShutdown(OnShutdownComplete, null);
                    this.SetAsCompleted(ex, false);
                    return;
                }


            }

            private void OnShutdownComplete(IAsyncResult ar)
            {
                _acdPortal.EndShutdown(ar);
            }


          }


    }
    #endregion

    internal enum PortalState 
    { 
        Created, 
        Starting, 
        Started, 
        Draining, 
        Terminating, 
        Terminated 
    };

    internal class PortalStateChangedEventArgs : EventArgs
    {
        private PortalState _previousState;
        private PortalState _newState;

        internal PortalStateChangedEventArgs(PortalState previousState, PortalState newState)
        {
            _previousState = previousState;
            _newState = newState;
        }

        internal PortalState PreviousState
        {
            get { return _previousState; }
        }

        internal PortalState NewState
        {
            get { return _newState; }
        }
    }
}
