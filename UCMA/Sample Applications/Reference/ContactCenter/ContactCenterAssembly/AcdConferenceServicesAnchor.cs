/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration.Samples.Utilities;
using System.Diagnostics;
using Microsoft.Rtc.Collaboration.ConferenceManagement;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.AudioVideo;
using System.Threading;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{

    internal static class SipUriCompare
    {
        /// <summary>
        /// Compares if 2 uris are equal.
        /// </summary>
        /// <param name="uri1">Uri1</param>
        /// <param name="uri2">Uri2</param>
        /// <returns>True if 2 uris are equal. False otherwise.</returns>
        public static bool Equals(string uri1, string uri2)
        {
            bool areEqual = false;

            if (String.Equals(uri1, uri2, StringComparison.OrdinalIgnoreCase))
            {
                areEqual = true;
            }
            else if(!String.IsNullOrEmpty(uri1))
            {
                try
                {
                    RealTimeAddress address1 = new RealTimeAddress(uri1);
                    RealTimeAddress address2 = new RealTimeAddress(uri2);
                    areEqual = (address1 == address2);
                }
                catch (ArgumentException)
                {
                }
            }

            return areEqual;
        }
    }

            
    
    internal class AcdConferenceServicesAnchor
    {
        #region Private Fields
        private ApplicationEndpoint _endpoint;
        private ConferenceServicesAnchorState _state;
        private object _syncRoot = new object();
        private Conference _conference;
        private string _conferenceUri;
        private AcdLogger _logger;
        private Conversation _conversation;
        private AcdCustomerSession _session;
        private List<AcdServiceChannel> _listOfServiceChannels = new List<AcdServiceChannel>();
        private List<ShutDownAsyncResult> _listOfShutDownAsyncResults = new List<ShutDownAsyncResult>();
        private List<string> _listOfAuthorizedParticipants = new List<string>();
        private List<string> _listOfPresenters = new List<string>();
        private bool _isPrimaryChannelCreated;
        #endregion

        #region constuctors
        internal AcdConferenceServicesAnchor(AcdCustomerSession session, ApplicationEndpoint endpoint, AcdLogger logger)
        {
            _endpoint = endpoint;
            _state = ConferenceServicesAnchorState.Idle;
            _logger = logger;
            _session = session;
            _conversation = new Conversation(endpoint);
            _conversation.Impersonate("sip:" + Guid.NewGuid() + "@" + endpoint.DefaultDomain, null, null);
            _conversation.ApplicationContext = this;
        }

        internal AcdConferenceServicesAnchor(string conferenceUri, ApplicationEndpoint endpoint, AcdLogger logger)
        {
            _endpoint = endpoint;
            _logger = logger;
            _conferenceUri = conferenceUri;
            _state = ConferenceServicesAnchorState.Idle;
            _conversation = new Conversation(endpoint);
            _conversation.Impersonate("sip:" + Guid.NewGuid() + "@" + endpoint.DefaultDomain, null, null);
            _conversation.ApplicationContext = this;
        }
        #endregion

        #region internal properties and events

        internal Conversation Conversation
        {
            get { return _conversation; }
        }

        internal string ConferenceUri
        {
            get { return _conferenceUri; }
        }

        internal List<AcdServiceChannel> ServiceChannels     
        {
            get { return new List<AcdServiceChannel>(_listOfServiceChannels); }
        }

        internal ConferenceServicesAnchorState State
        {
            get { return _state; }
        }

        internal bool IsPrimaryChannelCreated
        {
            get { return _isPrimaryChannelCreated; }        
        }

        internal ApplicationEndpoint Endpoint
        {
            get { return _endpoint; }
        }

        internal event EventHandler<ConferenceServicesAnchorStateChangedEventArgs> StateChanged;

        #endregion

        #region internal Methods

        internal IAsyncResult BeginStartUp(string customerUri, AsyncCallback userCallback,object state)
        {

            StartUpAsyncResult ar = new StartUpAsyncResult(customerUri, userCallback, state, this);

            bool firstTime = false;

            lock (_syncRoot)
            {
                if (_state == ConferenceServicesAnchorState.Idle)
                {
                    this.UpdateState(ConferenceServicesAnchorState.Establishing);
                    firstTime = true;
                }
                else
                {
                   throw new InvalidOperationException("AcdConferenceServicesAnchor: anchor instance already started");
                }
            }

            if (firstTime)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    var tempAr = waitState as StartUpAsyncResult;
                    tempAr.Process();
                }, ar);
            }

            return ar;
        }

        internal IAsyncResult BeginStartUp(AsyncCallback userCallback, object state)
        {

            StartUpAsyncResult ar = new StartUpAsyncResult(String.Empty, userCallback, state, this);

            bool firstTime = false;

            lock (_syncRoot)
            {
                if (_state == ConferenceServicesAnchorState.Idle)
                {
                    this.UpdateState(ConferenceServicesAnchorState.Establishing);
                    firstTime = true;
                }
                else
                {
                    throw new InvalidOperationException("AcdConferenceServicesAnchor: anchor instance already started") ;
                }
            }

            if (firstTime)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    var tempAr = waitState as StartUpAsyncResult;
                    tempAr.Process();
                }, ar);
            }

            return ar;
        }


        internal void EndStartup(IAsyncResult ar)
        { 
           StartUpAsyncResult result = ar as StartUpAsyncResult;
           result.EndInvoke();
        }

        internal void AddServiceChannel(AcdServiceChannel serviceChannel)
        {
          lock (_syncRoot)
          {
            if (serviceChannel.IsPrimaryServiceChannel)
            {
                if (_isPrimaryChannelCreated == true)
                {
                    throw new InvalidOperationException("AcdConferenceServicesAnchor already has a primary channel created");
                }
                else
                {
                    _isPrimaryChannelCreated = true;
                }
            }
            _listOfServiceChannels.Add(serviceChannel);
          }
        }

        internal void RemoveServiceChannel(AcdServiceChannel serviceChannel)
        {
            lock (_syncRoot)
            {

                if (serviceChannel.IsPrimaryServiceChannel)
                {
                  _isPrimaryChannelCreated = false;

                }
                _listOfServiceChannels.Remove(serviceChannel);
            }       
        
        }

        internal bool ElevateToPresenter(string participantUri)
        {
            SipUriParser parser;

            if (SipUriParser.TryParse(participantUri, out parser))
            {
                lock (_syncRoot)
                {
                    _listOfPresenters.Add(participantUri);
                }

                List<ConversationParticipant> listOfParticipants = new List<ConversationParticipant>(_conversation.RemoteParticipants);

                listOfParticipants.ForEach(cp =>
                {
                    if (SipUriCompare.Equals(cp.Uri, participantUri))
                    {
                        if (cp.Role != ConferencingRole.Leader)
                        {
                            _conversation.ConferenceSession.BeginModifyRole(cp,
                                                                            ConferencingRole.Leader,
                                                                            mr =>
                                                                            {
                                                                                try
                                                                                {
                                                                                _conversation.ConferenceSession.EndModifyRole(mr);
                                                                                }
                                                                                catch (RealTimeException rtex)
                                                                                {
                                                                                    this._logger.Log("ModifyRole failed", rtex);
                                                                                }
                                                                            },
                                                                            null);


                        }
                    }
                });

                return true;
            }

            return false;
        
        
        }


        internal bool AuthorizeParticipant(string participantUri)
        {
            SipUriParser parser;

            if (SipUriParser.TryParse(participantUri, out parser))
            {
                lock (_syncRoot)
                {
                    _listOfAuthorizedParticipants.Add(participantUri);
                }

                List<ConversationParticipant> listOfLobbyParticipants = new List<ConversationParticipant>(_conversation.GetLobbyParticipants());

                listOfLobbyParticipants.ForEach(cp => {
                if (SipUriCompare.Equals(cp.Uri, participantUri))
                {
                    try
                    {
                        _conversation.ConferenceSession.LobbyManager.BeginAdmitLobbyParticipants(new List<ConversationParticipant>() { cp },
                                                                                                 ar =>
                                                                                                 {
                                                                                                     LobbyManager lobbyMgr = ar.AsyncState as LobbyManager;
                                                                                                     try
                                                                                                     {
                                                                                                         lobbyMgr.EndAdmitLobbyParticipants(ar);
                                                                                                     }
                                                                                                     catch (RealTimeException rtex)
                                                                                                     {
                                                                                                         _logger.Log("AcdConferenceServices failed to end admit a participant in the lobby", rtex);
                                                                                                     }
                                                                                                 },
                                                                                                 _conversation.ConferenceSession.LobbyManager);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _logger.Log("AcdConferenceServicesAnchor failed to begin admit a participant in the lobby", ivoex);

                    }
                }              
            });
                return true;
            }

            return false;
        }

        internal bool DisallowParticipant(string participantUri)
        {
            SipUriParser parser;
            bool result = false;
            if (SipUriParser.TryParse(participantUri, out parser))
            {
                lock (_syncRoot)
                {
                    result = _listOfAuthorizedParticipants.Remove(participantUri);
                }
            }

            return result;
        
        }

    
        internal IAsyncResult BeginShutDown(AsyncCallback userCallback, object state)
        {
            ShutDownAsyncResult ar = new ShutDownAsyncResult(userCallback, state, this);

            bool firstTime = false;
            lock (_syncRoot)
            {
                if (_state < ConferenceServicesAnchorState.Terminating)
                {
                    this.UpdateState(ConferenceServicesAnchorState.Terminating);
                    firstTime = true;
                }
                else if (_state == ConferenceServicesAnchorState.Terminating)
                {
                    _listOfShutDownAsyncResults.Add(ar);
                
                }
                else if (_state == ConferenceServicesAnchorState.Terminated)
                {
                    ar.SetAsCompleted(null, true);
                }
            }

            if (firstTime)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    var tempAr = waitState as ShutDownAsyncResult;
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

        #endregion

        #region Private methods
        /// <summary>
        /// Updates the state of the current AcdConferenceServicesAnchor
        /// </summary>
        private void UpdateState(ConferenceServicesAnchorState state)
        {
            ConferenceServicesAnchorState previousState = _state;

            lock (_syncRoot)
            {
                switch (state)
                { 
                    case ConferenceServicesAnchorState.Idle:
                        _state = state;
                        break;

                    case ConferenceServicesAnchorState.Establishing:
                        if (_state == ConferenceServicesAnchorState.Idle)
                        {
                            _state = state;
                        }
                        break;

                    case ConferenceServicesAnchorState.Established:
                        if (_state == ConferenceServicesAnchorState.Establishing)
                        {
                            _state = state;
                        }
                        break;
                    case ConferenceServicesAnchorState.Terminating:
                        if ((_state != ConferenceServicesAnchorState.Terminating) &&
                            (_state != ConferenceServicesAnchorState.Terminated))
                        {
                            _state = state;
                        }
                        break;
                    case ConferenceServicesAnchorState.Terminated:
                        if (_state == ConferenceServicesAnchorState.Terminating)
                        {
                            _state = state;
                        }
                        break;
                }
            }
            
            EventHandler<ConferenceServicesAnchorStateChangedEventArgs> handler = this.StateChanged;

            if (handler != null)
               handler(this, new ConferenceServicesAnchorStateChangedEventArgs(previousState, state));
        }

        private void RegisterForEvents()
        {
            _conversation.LobbyParticipantAttendanceChanged += this.ProcessPendingParticipants;
            _conversation.RemoteParticipantAttendanceChanged += this.ProcessAllowedParticipants;
            _conversation.StateChanged += this.OnConversationTerminated;
                   
        }

        void OnConversationTerminated(object sender, StateChangedEventArgs<ConversationState> e)
        {
            if (e.State == ConversationState.Terminating)
            {
                this.BeginShutDown(sd => { this.EndShutDown(sd); }, null);
            }
        }

       

        private void ProcessPendingParticipants(Object sender, LobbyParticipantAttendanceChangedEventArgs args)
        {
            List<ConversationParticipant> listOfLobbyParticipants = new List<ConversationParticipant>(args.Added);
            
            listOfLobbyParticipants.ForEach(cp =>
            {

                lock (_syncRoot)
                {
                    _listOfAuthorizedParticipants.ForEach(ap =>
                    {
                        if (SipUriCompare.Equals(ap, cp.Uri))
                        {
                            try
                            {
                                _conversation.ConferenceSession.LobbyManager.BeginAdmitLobbyParticipants(new List<ConversationParticipant>() { cp },
                                ar =>
                                {
                                    LobbyManager lobbyMgr = ar.AsyncState as LobbyManager;

                                    try
                                    {
                                     lobbyMgr.EndAdmitLobbyParticipants(ar);
                                    }
                                    catch (RealTimeException rtex)
                                    {
                                        this._logger.Log("AdmitLobbyParticipant failed", rtex);
                                    }
                                 },
                                 _conversation.ConferenceSession.LobbyManager);

                            }
                            catch (InvalidOperationException ivo)
                            {
                                this._logger.Log("AdmitLobbyParticipant failed", ivo);
                            }
                       }
                   });

            }
          });
        }


        private void ProcessAllowedParticipants(Object sender, ParticipantAttendanceChangedEventArgs args)
        {
            List<ConversationParticipant> listOfParticipants = new List<ConversationParticipant>(args.Added);
            listOfParticipants.ForEach(cp =>{

            lock (_syncRoot)
            {
                _listOfPresenters.ForEach(p =>
                {
                    if (SipUriCompare.Equals(p, cp.Uri))
                    {
                        if (cp.Role != ConferencingRole.Leader)
                        {
                            try
                            {
                                _conversation.ConferenceSession.BeginModifyRole(cp,
                                ConferencingRole.Leader,
                                mr =>
                                {
                                    try
                                    {
                                        _conversation.ConferenceSession.EndModifyRole(mr);
                                    }
                                    catch (RealTimeException rtex)
                                    {
                                        this._logger.Log("ModifyRole failed", rtex);
                                    }
                                },
                                null);
                            }
                            catch (InvalidOperationException ivoex)
                            {
                                _logger.Log("ModifyRole failed", ivoex);
                            }
                        }
                    }
                });

                }  
            });                                          
        
        }

        #endregion


        #region StartUpAsyncResult
        private class StartUpAsyncResult:AsyncResultNoResult 
        {
            private AcdConferenceServicesAnchor _anchor;
            private string _presenterUri;

            internal StartUpAsyncResult(string presenterUri, AsyncCallback userCallback, Object state, AcdConferenceServicesAnchor anchor)
                : base(userCallback, state)
            {
                Debug.Assert(anchor != null);
                _anchor = anchor;
                _presenterUri = presenterUri;
              
            }

            internal void Process()
            {
                if (String.IsNullOrEmpty(_anchor._conferenceUri))
                {
                   
                    _anchor.AuthorizeParticipant(_presenterUri);
                    _anchor.ElevateToPresenter(_presenterUri);

                    ConferenceServices conferenceManagement = _anchor._endpoint.ConferenceServices;

                    //Create a conference to anchor the incoming customer call
                    ConferenceScheduleInformation conferenceScheduleInfo = new ConferenceScheduleInformation();
                    conferenceScheduleInfo.AutomaticLeaderAssignment = AutomaticLeaderAssignment.Disabled;
                    conferenceScheduleInfo.LobbyBypass = LobbyBypass.Disabled;
                    conferenceScheduleInfo.AccessLevel = ConferenceAccessLevel.Locked;
                    conferenceScheduleInfo.PhoneAccessEnabled = false;
                    conferenceScheduleInfo.Mcus.Add(new ConferenceMcuInformation(MediaType.ApplicationSharing));
                    conferenceScheduleInfo.Mcus.Add(new ConferenceMcuInformation(McuType.AudioVideo));
                    conferenceScheduleInfo.Mcus.Add(new ConferenceMcuInformation(McuType.InstantMessaging));

                    try
                    {
                        //schedule the conference
                        conferenceManagement.BeginScheduleConference(conferenceScheduleInfo,
                        ar =>
                        {
                            try
                            {
                             _anchor._conference = conferenceManagement.EndScheduleConference(ar);
                            }
                            catch (RealTimeException rtex)
                            {
                                this.SetAsCompleted(rtex, false);
                                return;
                            }
                            _anchor._conferenceUri = _anchor._conference.ConferenceUri;
                            _anchor.RegisterForEvents();

                            //Join the conference as a trusted conferencing user (invisible in the roster)
                            ConferenceJoinOptions options = new ConferenceJoinOptions();
                            options.JoinMode = JoinMode.TrustedParticipant;


                            try
                            {
                                _anchor._conversation.ConferenceSession.BeginJoin(_anchor._conference.ConferenceUri,
                                                                                  options,
                                jar =>
                                {
                                    Conversation conv = jar.AsyncState as Conversation;
                                    try
                                    {
                                        conv.ConferenceSession.EndJoin(jar);

                                        //Update the state of the anchor when the operation succeeds
                                        _anchor.UpdateState(ConferenceServicesAnchorState.Established);

                                        this.SetAsCompleted(null, false);
                                    }
                                    catch (RealTimeException rtex)
                                    {
                                        if (null != _anchor)
                                        {
                                            _anchor._logger.Log("AcdConferenceServicesAnchor failed to create a conference", rtex);
                                        }
                                        _anchor.BeginShutDown(sar =>
                                        {
                                            AcdConferenceServicesAnchor anchor = sar.AsyncState as AcdConferenceServicesAnchor;
                                            anchor.EndShutDown(sar);
                                        },
                                        _anchor);
                                        this.SetAsCompleted(rtex, false);

                                    }

                                }
                                , _anchor._conversation);
                            }
                            catch (InvalidOperationException ivoex)
                            {
                                _anchor._logger.Log("AcdConferenceServicesAnchor failed to create a conference", ivoex);
                                _anchor.BeginShutDown(sar =>
                                {
                                    AcdConferenceServicesAnchor anchor = sar.AsyncState as AcdConferenceServicesAnchor;
                                    anchor.EndShutDown(sar);
                                },
                                _anchor);
                                this.SetAsCompleted(new OperationFailureException("AcdConferenceServicesAnchor failed creating a conference" , ivoex), false);

                            }

                        },
                        conferenceManagement);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _anchor._logger.Log("AcdConferenceServicesAnchor failed to create a conference", ivoex);
                        _anchor.BeginShutDown(sar =>
                        {
                            AcdConferenceServicesAnchor anchor = sar.AsyncState as AcdConferenceServicesAnchor;
                            anchor.EndShutDown(sar);
                        },
                         _anchor);
                        this.SetAsCompleted(new OperationFailureException("AcdConferenceServicesAnchor failed to create a conference", ivoex), false);
                    }
                }
                else
                {

                     _anchor.RegisterForEvents();

                     ConferenceJoinOptions options = new ConferenceJoinOptions();
                     options.JoinMode = JoinMode.TrustedParticipant;


                     try
                     {
                         _anchor._conversation.ConferenceSession.BeginJoin(_anchor._conferenceUri,
                                                                           options,
                            jar =>
                            {
                                Conversation conv = jar.AsyncState as Conversation;
                                try
                                {
                                    conv.ConferenceSession.EndJoin(jar);
                                    _anchor.UpdateState(ConferenceServicesAnchorState.Established);
                                    this.SetAsCompleted(null, false);
                                }
                                catch (RealTimeException rtex)
                                {
                                    if (null != _anchor)
                                    {
                                        _anchor._logger.Log("AcdConferenceServicesAnchor failed to create a conference", rtex);
                                    }
                                    _anchor.BeginShutDown(sar =>
                                    {
                                        AcdConferenceServicesAnchor anchor = sar.AsyncState as AcdConferenceServicesAnchor;
                                        anchor.EndShutDown(sar);
                                    },
                                    _anchor);
                                    this.SetAsCompleted(rtex, false);

                                }

                            },
                            _anchor._conversation);
                     }
                     catch (InvalidOperationException ivoex)
                     {
                         _anchor._logger.Log("AcdConferenceServicesAnchor failed to create a conference", ivoex);
                         _anchor.BeginShutDown(sar =>
                            {
                                AcdConferenceServicesAnchor anchor = sar.AsyncState as AcdConferenceServicesAnchor;
                                anchor.EndShutDown(sar);
                            },
                                              _anchor);
                         this.SetAsCompleted(new OperationFailureException("AcdConferenceServicesAnchor failed to create a conference", ivoex), false);
                     }
                
                }

            }        
        
        }

        #endregion

        #region ShutDownAsyncResult
        private class ShutDownAsyncResult: AsyncResultNoResult
        {
            AcdConferenceServicesAnchor _anchor;
            internal ShutDownAsyncResult(AsyncCallback userCallback, object state, AcdConferenceServicesAnchor anchor): base(userCallback, state)
            {
                _anchor = anchor;
             
            }

            internal void Process()
            {
                if (_anchor._conversation.State != ConversationState.Idle)
                {
                    if (null != _anchor._conference)
                    {
                        _anchor._conversation.ConferenceSession.BeginTerminateConference(ar =>
                        {
                            ConferenceSession confSession = ar.AsyncState as ConferenceSession;
                            confSession.EndTerminateConference(ar);

                            confSession.Conversation.Endpoint.ConferenceServices.BeginCancelConference(_anchor._conference.ConferenceId,
                            cac =>
                            {
                                ConferenceServices confServices = cac.AsyncState as ConferenceServices;
                                try
                                {
                                    confServices.EndCancelConference(cac);
                                }
                                catch (RealTimeException)
                                {
                                    //TODO: trace statement
                                }
                                finally
                                {
                                    this.SetAsCompleted(null, false);
                                    _anchor.UpdateState(ConferenceServicesAnchorState.Terminated);
                                    foreach (ShutDownAsyncResult sar in _anchor._listOfShutDownAsyncResults)
                                    {
                                        sar.SetAsCompleted(null, false);
                                    }


                                }
                            },
                            confSession.Conversation.Endpoint.ConferenceServices);
                        },
                        _anchor._conversation.ConferenceSession);
                    }
                    else
                    {

                        _anchor.Conversation.BeginTerminate(ter =>
                        {
                            _anchor.Conversation.EndTerminate(ter);

                            _anchor.UpdateState(ConferenceServicesAnchorState.Terminated);

                            foreach (ShutDownAsyncResult sar in _anchor._listOfShutDownAsyncResults)
                            {
                                sar.SetAsCompleted(null, false);
                            }

                            this.SetAsCompleted(null, false);

                        },
                        null);
                    }

                }
                else
                {

                    _anchor.Conversation.BeginTerminate(ter =>
                    {
                        _anchor.Conversation.EndTerminate(ter);

                        _anchor.UpdateState(ConferenceServicesAnchorState.Terminated);

                        foreach (ShutDownAsyncResult sar in _anchor._listOfShutDownAsyncResults)
                        {
                            sar.SetAsCompleted(null, false);
                        }

                        this.SetAsCompleted(null, false);

                    },
                    null);
                }
              

            }
        
        }
        #endregion


    }
        
    internal enum ConferenceServicesAnchorState { Idle = 0, Establishing = 1, Established = 2, Terminating = 3, Terminated = 4 };
    
  

    internal class ConferenceServicesAnchorStateChangedEventArgs : EventArgs
    {
        private ConferenceServicesAnchorState _previousState;
        private ConferenceServicesAnchorState _newState;

        internal ConferenceServicesAnchorStateChangedEventArgs(ConferenceServicesAnchorState previousState, ConferenceServicesAnchorState newState)
        {
            _previousState = previousState;
            _newState = newState;
        }

        internal ConferenceServicesAnchorState PreviousState
        {
            get { return _previousState; }
        }

        internal ConferenceServicesAnchorState NewState
        {
            get { return _newState; }
        }
    }


}
