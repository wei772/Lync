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
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Agents;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter;
using Microsoft.Rtc.Collaboration;
using System.Threading;
using System.Collections.ObjectModel;
using System.Net.Mime;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
	internal class AcdMonitoringSession
	{
        #region Private Fields
        private AcdLogger _logger;
        private List<AcdServiceChannel> _listOfMonitoringChannels = new List<AcdServiceChannel>();        
        private Agent _agentToMonitor;
        private InstantMessagingCall _initialSupervisorCall;
        private AcdAgentMatchMaker _matchMaker;
        private MonitoringSessionState _state;
        private Conversation _backEndConversation;
        private AcdConferenceServicesAnchor _anchor;
        private MonitoringChannel _channel;
        private AcdSupervisorSession _supervisorSession;
        private object _syncRoot = new object();
        private List<ShutDownAsyncResult> _listOfShutDownAsyncResults = new List<ShutDownAsyncResult>();

        #endregion 

        #region Constructors
        internal AcdMonitoringSession(MonitoringChannel channel, AcdSupervisorSession supervisorSession, Agent agentToMonitor, InstantMessagingCall supervisorInitialCall, AcdLogger logger, AcdAgentMatchMaker matchMaker)
        {
            _channel = channel;
            _supervisorSession = supervisorSession;
            _matchMaker = matchMaker;
            _agentToMonitor = agentToMonitor;
            _initialSupervisorCall = supervisorInitialCall;
            _logger = logger;

            
        }

        #endregion

        #region Internal Methods
        internal void HandleMonitoringSessionRequestReceived(object sender, ContextChannelRequestReceivedEventArgs args)
        {
            _logger.Log("RECEIVED REQUEST: AcdMonitoringSession received the following request: " + args.RequestType);

            lock(_syncRoot)
            {
              if (  _state < MonitoringSessionState.Terminating
                  &&_state > MonitoringSessionState.Idle)
              {

                  switch(args.RequestType)
                  {
                      case ContextChannelRequestType.Whisper:

                          WhisperRequest whisperRequest = args.Request as WhisperRequest;

                          try
                          {
                              this.BeginWhisper(whisperRequest.Uri.ToString(),
                                                war =>
                              {
                                  try
                                  {
                                      this.EndWhisper(war);
                                      this.UpdateState(MonitoringSessionState.Whispering);
                                      args.Request.SendResponse("Success");
                                  }
                                  catch (RealTimeException rtex)
                                  {
                                      _logger.Log("AcdMonitoringSession failed to end whispering", rtex);
                                      args.Request.SendResponse("Failure");
                                  }
                              },
                              null);
                          }
                          catch (InvalidOperationException ivoex)
                          {
                             _logger.Log("AcdMonitoringSession failed to start whispering", ivoex);
                             args.Request.SendResponse("Failure");

                          }
                          break;

                      case ContextChannelRequestType.BargeIn:

                          try
                          {
                              this.BeginBargeIn(bi =>
                              {
                                  try
                                  {
                                      this.EndBargeIn(bi);
                                      args.Request.SendResponse("Success");
                                  }
                                  catch (RealTimeException rtex)
                                  {
                                      _logger.Log("AcdMonitoringSession failed to end bargeing in", rtex);
                                      args.Request.SendResponse("Failure");
                                  }
                              },
                              null);
                          }
                          catch (InvalidOperationException ivoex)
                          {
                             _logger.Log("AcdMonitoringSession failed to start bargeing in", ivoex);
                             args.Request.SendResponse("Failure");

                          }
                          break;
 
                      default:
                          _logger.Log("AcdMonitoringSession failed understanding the commmand");
                          args.Request.SendResponse("Failure");
                          break;
                  
                  
                  }
                  

              }
            
            }
        }


        internal IAsyncResult BeginStartUp(AsyncCallback userCallback, object state)
        {
            StartUpAsyncResult ar = new StartUpAsyncResult(this, userCallback, state);

            bool process = false;

            lock (_syncRoot)
            {

                if (_state == MonitoringSessionState.Idle)
                {
                    this.UpdateState(MonitoringSessionState.Starting);
                    process = true;
                }
                else
                {
                    throw new InvalidOperationException("AcdMonitoringSession was already started.");
                }
              
            }

            if (process)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    StartUpAsyncResult tempAr = waitState as StartUpAsyncResult;
                    tempAr.Process();
                }, ar);
            }


            return ar;

        }

        internal void EndStartUp(IAsyncResult ar)
        {
            StartUpAsyncResult result = ar as StartUpAsyncResult;
            result.EndInvoke();
        }



        internal IAsyncResult BeginShutDown(AsyncCallback userCallback, object state)
        {
            ShutDownAsyncResult ar = new ShutDownAsyncResult(this, userCallback, state);

            bool firstTime = false;

            lock (_syncRoot)
            {
                if (_state < MonitoringSessionState.Terminating)
                {
                    this.UpdateState(MonitoringSessionState.Terminating);
                    firstTime = true;
                
                }
                else if (_state == MonitoringSessionState.Terminating)
                {
                    _listOfShutDownAsyncResults.Add(ar);
                
                }
                else if (_state == MonitoringSessionState.Terminated)
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

        #region Private Methods

        private void UpdateState(MonitoringSessionState state)
        {
            MonitoringSessionState previousState = _state;

            lock (_syncRoot)
            {
                switch (state)
                { 
                    case MonitoringSessionState.Idle:
                        _state = state;
                        break;

                    case MonitoringSessionState.Starting:
                        if (_state == MonitoringSessionState.Idle)
                        {
                            _state = state;
                        }
                        break;

                    case MonitoringSessionState.SilentMonitoring:
                        if (_state == MonitoringSessionState.Starting)
                        {
                            _state = state;
                        }
                        break;
                    case MonitoringSessionState.SilentToWhispering:
                        if (_state == MonitoringSessionState.SilentMonitoring)
                        {
                            _state = state;
                        }
                        break;

                    case MonitoringSessionState.Whispering:
                        if (_state == MonitoringSessionState.SilentToWhispering)
                        {
                            _state = state;
                        }
                        break;

                    case MonitoringSessionState.BargeingIn:
                        if (_state == MonitoringSessionState.SilentMonitoring
                            || _state == MonitoringSessionState.Whispering)
                        {
                            _state = state;
                        }
                        break;
                }

                             
            }

            EventHandler<MonitoringSessionStateChangedEventArgs> stateChanged = this.StateChanged;

            if (stateChanged != null)
                stateChanged(this, new MonitoringSessionStateChangedEventArgs(previousState, state));
        }

        private void BeginStartSilentMonitoring(AsyncCallback userCallback, object state)
        {
            StartSilentMonitoringAsyncResult ar = new StartSilentMonitoringAsyncResult(this, userCallback, state);

            bool firstTime = false;

            lock (_syncRoot)
            {
                if (_state == MonitoringSessionState.Starting)
                {

                    firstTime = true;

                }
                else
                {
                    throw new InvalidOperationException("AcdMonitoringSession is not in a state that can allow silent monitoring");

                }
            }

            if (firstTime)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    var tempAr = waitState as StartSilentMonitoringAsyncResult;
                    tempAr.Process();
                }, ar);
            }


        }

        private List<AcdServiceChannel> EndStartSilentMonitoring(IAsyncResult ar)
        {
            StartSilentMonitoringAsyncResult result = ar as StartSilentMonitoringAsyncResult;
            return result.EndInvoke();
        }

        private void HandleBackEndConversationStateChanged(object sender, StateChangedEventArgs<ConversationState> args)
        {
            Conversation conversation = sender as Conversation;

            switch(args.State)
            { 
                case ConversationState.Terminating:
                    lock (_syncRoot)
                    {
                        if (_state < MonitoringSessionState.Terminating)
                        {
                            this.BeginShutDown(sd=>
                                               {
                                                 this.EndShutDown(sd);
                                               },
                                               null);

                        }
                     }
                     break;

                case ConversationState.Terminated:
                     _backEndConversation.StateChanged -= this.HandleBackEndConversationStateChanged;
                    break;
  
            }

        }


        private void SendMessageToSupervisor(string message)
        {
            try
            {
                _initialSupervisorCall.Flow.BeginSendInstantMessage(message,
                                                                  sm =>
                                                                  {
                                                                      try
                                                                      {
                                                                          _initialSupervisorCall.Flow.EndSendInstantMessage(sm);
                                                                      }
                                                                      catch (RealTimeException)
                                                                      {
                                                                          //to do logging
                                                                      }
                                                                  },
                                                                  null);
            }
            catch (InvalidOperationException)
            {
                //to do logging
            }        
        }
        
        private void SetBackEndConversationProperties()
        {
            _backEndConversation.ApplicationContext = this;
            _backEndConversation.StateChanged += this.HandleBackEndConversationStateChanged;
        }



        /// <summary>
        /// Copies instant messages received from the supervisor to the anchor
        /// </summary>
        private void WhisperOnSupervisorMessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            if (null != this._backEndConversation)
            {
                List<Call> listOfCalls = new List<Call>(_backEndConversation.Calls);
                InstantMessagingCall imCall = null;

                listOfCalls.ForEach(call =>
                {
                    if (call is InstantMessagingCall)
                    {
                        imCall = (InstantMessagingCall)call;
                    }

                });


                if (null == imCall)
                    return;

                if (null == imCall.Flow)
                    return;

                lock (_syncRoot)
                {
                    if (  _state < MonitoringSessionState.Terminating
                        &&_state > MonitoringSessionState.Idle)
                    {

                        try
                        {
                            imCall.Flow.BeginSendInstantMessage(e.TextBody,
                                                          new AsyncCallback(delegate(IAsyncResult result)
                                                          {
                                                              try
                                                              {
                                                                  imCall.Flow.EndSendInstantMessage(result);
                                                              }
                                                              catch (RealTimeException ex)
                                                              {
                                                                  _logger.Log("AcdMonitoringSession failed to forward IM to agents", ex);
                                                              }
                                                          }),
                                                          null);
                        }
                        catch (InvalidOperationException)
                        {
                            _logger.Log("AcdMonitoringSession failed to send an IM to the agents");

                        }

                    }
                }
            }
        }

        private void HandleAttendancePropertiesChanged(object sender, ParticipantPropertiesChangedEventArgs args)
        {
            List<agentType> listOfAddedAgents = new List<agentType>();
            List<participantType> listOfAddedAttendees = new List<participantType>();



           if (    args.Participant.RosterVisibility != ConferencingRosterVisibility.Hidden
                && args.ChangedPropertyNames.Contains("ActiveMediaTypes"))
           {
               Agent agent = null;

               if (SipUriCompare.Equals(args.Participant.Uri, _agentToMonitor.SignInAddress))
               {
                   agent = _matchMaker.LookupAgent(args.Participant.Uri);
               }
               
               if (null != agent)
               {                
                   agentType agentT = Agent.Convert(agent, args.Participant);
                   listOfAddedAgents.Add(agentT);

                   try
                   {
                       this._channel.BeginUpdateAgents(listOfAddedAgents,
                                                       null,
                                                       ua =>
                        {
                            try
                            {
                                this._channel.EndUpdateAgents(ua);
                            }
                            catch (RealTimeException rtex)
                            {
                                _logger.Log("AcdMonitoringSession failed end updating the list of Agents in the monitoring session.", rtex);
                            }
                        },
                                                       null);
                   }
                   catch (InvalidOperationException ivoex)
                   {
                       _logger.Log("AcdMonitoringSession failed start updating the list of Agents in the monitoring session", ivoex);
                   }

               }
               else
               {
                    participantType participantT = new participantType();
                    participantT.uri = args.Participant.Uri;
                    participantT.displayname = args.Participant.DisplayName;
                    participantT.iscustomer = (args.Participant.Role == ConferencingRole.Leader);

                    int numberOfActiveModalities = args.Participant.GetActiveMediaTypes().Count;

                    if (numberOfActiveModalities != 0)
                    {
                        participantT.mediatypes = new string[numberOfActiveModalities];

                        int i = 0;
                        foreach (String mediaType in args.Participant.GetActiveMediaTypes())
                        {
                            participantT.mediatypes[i] = mediaType;
                            i++;
                        }

                    }

                    listOfAddedAttendees.Add(participantT);
                    try
                    {
                        this._channel.BeginUpdateParticipants(listOfAddedAttendees,
                                                        null,
                                                        ua =>
                        {
                            try
                            {
                                this._channel.EndUpdateParticipants(ua);
                            }
                            catch (RealTimeException rtex)
                            {
                                _logger.Log("AcdMonitoringSession failed end updating the list of Participants in the monitoring session.", rtex);
                            }
                        },
                                                        null);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _logger.Log("AcdMonitoringSession failed start updating the list of Participants in the monitoring session", ivoex);
                    }

               }

           }
        
        }

        private void HandleAttendanceChanged(object sender, ParticipantAttendanceChangedEventArgs args)
        {

            List<ConversationParticipant> listOfAddedParticipants = new List<ConversationParticipant>(args.Added);
            List<ConversationParticipant> listOfRemovedParticipants = new List<ConversationParticipant>(args.Removed);
            List<agentType> listOfAddedAgents = new List<agentType>();
            List<String> listOfRemovedAgents = new List<String>();
            List<participantType> listOfAddedAttendees = new List<participantType>();
            List<String> listOfRemovedAttendees = new List<String>();

            listOfAddedParticipants.ForEach(participant =>
            {
               Agent agent = null;

               if (SipUriCompare.Equals(participant.Uri, _agentToMonitor.SignInAddress))
                {
                   agent = _matchMaker.LookupAgent(participant.Uri);
                }

               if (null != agent)
               {                
                   agentType agentT = Agent.Convert(agent, participant);
                   listOfAddedAgents.Add(agentT);

                   try
                   {
                       this._channel.BeginUpdateAgents(listOfAddedAgents,
                                                       null,
                                                       ua =>
                                                       {
                                                           try
                                                           {
                                                               this._channel.EndUpdateAgents(ua);
                                                           }
                                                           catch (RealTimeException rtex)
                                                           {
                                                               _logger.Log("AcdMonitoringSession failed end updating the list of Agents in the monitoring session.", rtex);
                                                           }
                                                       },
                                                       null);
                   }
                   catch (InvalidOperationException ivoex)
                   {
                       _logger.Log("AcdMonitoringSession failed start updating the list of Agents in the monitoring session", ivoex);
                   }

               }
               else
               {
                   if (participant.RosterVisibility != ConferencingRosterVisibility.Hidden)
                   {
                       participantType participantT = new participantType();
                       participantT.uri = participant.Uri;
                       participantT.displayname = participant.DisplayName;
                       participantT.iscustomer = (participant.Role == ConferencingRole.Leader); //only the customer is leader as the system impersonates

                       int numberOfActiveModalities = participant.GetActiveMediaTypes().Count;


                       if (numberOfActiveModalities != 0)
                       {
                           participantT.mediatypes = new string[numberOfActiveModalities];

                           
                           int i = 0;
                           foreach (String mediaType in participant.GetActiveMediaTypes())
                           {
                               participantT.mediatypes[i] = mediaType;
                               i++;
                           }

                       }
                       listOfAddedAttendees.Add(participantT);

                       try
                       {
                           this._channel.BeginUpdateParticipants(listOfAddedAttendees,
                                                                 null,
                                                                 up =>
                                                                 {
                                                                     try
                                                                     {
                                                                         this._channel.EndUpdateParticipants(up);
                                                                     }
                                                                     catch (RealTimeException rtex)
                                                                     {
                                                                         _logger.Log("AcdMonitoringSession failed end updating the list of Attendees in the monitoring session", rtex);
                                                                     }
                                                                 },
                                                                 null);

                       }
                       catch (InvalidOperationException ivoex)
                       {
                           _logger.Log("AcdMonitoringSession failed start updating the list of Attendees in the monitoring session", ivoex);
                       }


                   }
                  
               }

            });

            listOfRemovedParticipants.ForEach(participant =>
            {
               Agent agent = _matchMaker.LookupAgent(participant.Uri);
               
               if (null != agent)
               {   
                   listOfRemovedAgents.Add(participant.Uri);

               }
               else
               {
                   if (participant.RosterVisibility != ConferencingRosterVisibility.Hidden)
                   {
                       listOfRemovedAttendees.Add(participant.Uri);
                   }
               }

            });

            if (listOfAddedAgents.Count != 0
                || listOfRemovedAgents.Count != 0)
            {
                try
                {
                    this._channel.BeginUpdateAgents(listOfAddedAgents,
                                                    listOfRemovedAgents,
                                                    ua =>
                                                    {
                                                        try
                                                        {
                                                            this._channel.EndUpdateAgents(ua);
                                                        }
                                                        catch (RealTimeException rtex)
                                                        {
                                                            _logger.Log("AcdMonitoringSession failed end updating the list of Agents in the monitoring session.", rtex);
                                                        }
                                                    },
                                                    null);
                }
                catch (InvalidOperationException ivoex)
                {
                    _logger.Log("AcdMonitoringSession failed start updating the list of Agents in the monitoring session", ivoex);
                }
            }

            if (listOfAddedAttendees.Count != 0
                || listOfRemovedAttendees.Count != 0)
            {

                try
                {
                    this._channel.BeginUpdateParticipants(listOfAddedAttendees,
                                                          listOfRemovedAttendees,
                                                          up =>
                                                          {
                                                              try
                                                              {
                                                                  this._channel.EndUpdateParticipants(up);
                                                              }
                                                              catch (RealTimeException rtex)
                                                              {
                                                                  _logger.Log("AcdMonitoringSession failed end updating the list of Attendees in the monitoring session", rtex);
                                                              }
                                                          },
                                                          null);

                }
                catch (InvalidOperationException ivoex)
                {
                    _logger.Log("AcdMonitoringSession failed start updating the list of Attendees in the monitoring session", ivoex);
                }
            }
        
        }

        private IAsyncResult BeginBargeIn(AsyncCallback userCallback, object state)
        {
            BargeInAsyncResult asyncResult = new BargeInAsyncResult(this, userCallback, state);

            bool firstTime = false;

            lock (_syncRoot)
            {
                if (   _state == MonitoringSessionState.Whispering 
                    || _state == MonitoringSessionState.SilentMonitoring)
                {
                    this.UpdateState(MonitoringSessionState.BargeingIn);
                    firstTime = true;
                }
                else
                {
                    throw new InvalidOperationException("AcdMonitoringSession cannot start bargeing in as the session is in an invalid state.");
                }
            }

            if (firstTime)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    BargeInAsyncResult tempAr = waitState as BargeInAsyncResult;
                    tempAr.Process();
                }, asyncResult);
            }
            return asyncResult;


        }
        private void EndBargeIn(IAsyncResult ar)
        {
            BargeInAsyncResult result = ar as BargeInAsyncResult;
            result.EndInvoke();

        }


        private IAsyncResult BeginWhisper(string agentToWhisperTo,AsyncCallback userCallback, object state)
        {
            WhisperAsyncResult ar = new WhisperAsyncResult(agentToWhisperTo,this, userCallback, state);

            bool firstTime = false;

            lock (_syncRoot)
            {
                if (_state == MonitoringSessionState.SilentMonitoring)
                {
                    this.UpdateState(MonitoringSessionState.SilentToWhispering);
                    firstTime = true;
                }
                else
                {
                    throw new InvalidOperationException("AcdMonitoringSession cannot start whispering to agent as the session is in an invalid state.");
                }
            }

            if (firstTime)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    WhisperAsyncResult tempAr = waitState as WhisperAsyncResult;
                    tempAr.Process();
                }, ar);
            }

            return ar;

        
        }

        private void EndWhisper(IAsyncResult ar)
        {
            WhisperAsyncResult result = ar as WhisperAsyncResult;
            result.EndInvoke();
          
        }


        private void BridgeSupervisorImFlowToAnchorInWhisperingMode(InstantMessagingCall call)
        { 
            _initialSupervisorCall.Flow.MessageReceived += this.WhisperOnSupervisorMessageReceived;
            _initialSupervisorCall.Flow.RemoteComposingStateChanged += this.OnTypingNotificationReceivedFromSupervisor;

        }

        private void RegisterSilentMonitoringEvents(InstantMessagingCall imMonitoringBackEndCall)
        {
            imMonitoringBackEndCall.Flow.RemoteComposingStateChanged += this.OnTypingNotificationReceivedFromAnchor;
            imMonitoringBackEndCall.Flow.MessageReceived += this.OnMessageReceivedFromAnchor;

        }

        private void OnTypingNotificationReceivedFromAnchor(object sender, ComposingStateChangedEventArgs args)
        {
            if (!SipUriCompare.Equals(args.Participant.Uri, _agentToMonitor.SupervisorUri))
            {
                _initialSupervisorCall.Flow.LocalComposingState = args.ComposingState;
            }
        }


        private void OnMessageReceivedFromAnchor(object sender, InstantMessageReceivedEventArgs args)
        {
            InstantMessageId MessageId = args.MessageId;
            InstantMessagingFlow imChannelFlow = sender as InstantMessagingFlow;
            if (SipUriCompare.Equals(args.Sender.Uri, _agentToMonitor.SupervisorUri))
            {
                try
                {
                    imChannelFlow.BeginSendSuccessDeliveryNotification( MessageId,
                                                                        ssdn =>
                    {
                        try
                        {
                          imChannelFlow.EndSendSuccessDeliveryNotification(ssdn);
                        }
                        catch (RealTimeException rtex)
                        {
                          _logger.Log("AcdMonitoringSession failed end sending a delivery notification", rtex);
                        }
                    },
                                                                        null);
                }
                catch (InvalidOperationException ivoex)
                {
                  _logger.Log("AcdMonitoringSession failed start sending a delivery notification", ivoex);
                }


            }
            else
            {
                bool success = true;
                try
                {
                    _initialSupervisorCall.Flow.BeginSendInstantMessage(new ContentType("text/html"),
                                                                                  Encoding.UTF8.GetBytes(FormatHtmlSupervisorMessage(args)),
                                                                                  mer =>
                    {
                        try
                        {
                            _initialSupervisorCall.Flow.EndSendInstantMessage(mer);
                            try
                            {
                                imChannelFlow.BeginSendSuccessDeliveryNotification( MessageId,
                                                                                    ssdn =>
                                {
                                    try
                                    {
                                      imChannelFlow.EndSendSuccessDeliveryNotification(ssdn);
                                    }
                                    catch (RealTimeException rtex)
                                    {
                                      _logger.Log("AcdMonitoringSession failed end sending a delivery notification", rtex);
                                    }
                                },
                                                                                    null);
                            }
                            catch (InvalidOperationException ivoex)
                            {
                              _logger.Log("AcdMonitoringSession failed start sending a delivery notification", ivoex);
                            }
                        }
                        catch (RealTimeException ex)
                        {
                            int respCode;

                            if (ex is FailureResponseException)
                            {
                                var fre = ex as FailureResponseException;
                                respCode = fre.ResponseData.ResponseCode;
                            }
                            else
                            {
                                respCode = ResponseCode.TemporarilyUnavailable;
                            }

                            try
                            {
                                imChannelFlow.BeginSendFailureDeliveryNotification( MessageId,
                                                                                    respCode,
                                                                                    sfdn =>
                                {
                                    try
                                    {
                                      imChannelFlow.EndSendFailureDeliveryNotification(sfdn);
                                    }
                                    catch (RealTimeException rtex)
                                    {
                                      _logger.Log("AcdMonitoringSession failed end sending a failure delivery notification", rtex);
                                    }
                                },
                                                                                    null);
                            }
                            catch (InvalidOperationException ivoex)
                            {
                              _logger.Log("AcdMonitoringSession failed start sending a failure delivery notification", ivoex);
                            }

                        }
                    },
                                                                                    null);
                }
                catch (RealTimeException)
                {

                    success = false;
                }
                catch (InvalidOperationException)
                {
                    success = false;                
                }
                finally
                {
                  if (!success)
                  {
                    try
                    {
                        imChannelFlow.BeginSendFailureDeliveryNotification( MessageId,
                                                                            ResponseCode.TemporarilyUnavailable,
                                                                            sfdn =>
                        {
                            try
                            {
                                imChannelFlow.EndSendFailureDeliveryNotification(sfdn);
                            }
                            catch (RealTimeException rtex)
                            {
                                _logger.Log("AcdMonitoringSession failed end sending a failure delivery notification", rtex);
                            }
                        },
                                                                            null);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _logger.Log("AcdMonitoringSession failed start sending a failure delivery notification", ivoex);
                    }                  
                  
                  }
                
                }
            }

        }
        /// <summary>
        /// This helper method takes care of formatting a message received from an Agent, Supervisor, or Expert into
        /// a message to the customer. This will take care of the anonymity of identity.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private string FormatHtmlSupervisorMessage(InstantMessageReceivedEventArgs e)
        {
            //looks up the agent that sent the message
            Agent agent = _matchMaker.LookupAgent("sip:" + e.Sender.UserAtHost);
            string publicName = null;
            string color = null;

            if (null != agent)
            {
                publicName = e.Sender.DisplayName;
                color = agent.InstantMessageColor;
            }
            else
            {
                // this is not an agent we can give away the name; application can change this behavior
                publicName = e.Sender.DisplayName;
                color = "black";
            }
            //prepends the message with the agent's name
            string message = String.Format("<html><body><span style=\"color:{0};bold\">{1}:</span> {2}</body></html>", color, publicName, e.TextBody);

            return message;
        }

        private string FormatHtmlSupervisorMessage(string displayName, string message)
        {
            //looks up the agent that sent the message
            string color = "black";

            //prepends the message with the agent's name
            message = String.Format("<html><body><span style=\"color:{0};bold\">{1}:</span> {2}</body></html>", color, displayName, message);

            return message;
        }

        private void OnTypingNotificationReceivedFromSupervisor(object sender, ComposingStateChangedEventArgs args)
        {
            if (null != this._backEndConversation)
            {
                List<Call> listOfCalls = new List<Call>(_backEndConversation.Calls);

                listOfCalls.ForEach(call =>
                                    {
                                        if (call is InstantMessagingCall)
                                        {
                                            if (null != ((InstantMessagingCall)call).Flow)
                                            {
                                                ((InstantMessagingCall)call).Flow.LocalComposingState = args.ComposingState;
                                            }
                                        }

                                    });

            }

        }



        #endregion

        #region Internal Properties

        internal MonitoringSessionState State
        {
            get { return _state; }
        }

        internal AcdConferenceServicesAnchor Anchor
        {
            get { return _anchor; }
        }

        internal event EventHandler<MonitoringSessionStateChangedEventArgs> StateChanged;
        #endregion

        #region StartUpAsyncResult
        private class StartUpAsyncResult : AsyncResultNoResult
        {
            private AcdMonitoringSession _session;

            internal StartUpAsyncResult(AcdMonitoringSession session, AsyncCallback userCallback, object state)
                : base(userCallback, state)
            {
                _session = session;
            
            }

            internal void Process()
            { 
                AcdCustomerSession customerSession = _session._agentToMonitor.Owner;


                if (customerSession != null)
                {

                    AcdConferenceServicesAnchor customerCallAnchor = customerSession.Anchor;

                    if (customerCallAnchor != null)
                    {
                        _session._anchor = new AcdConferenceServicesAnchor(customerCallAnchor.ConferenceUri,_session._matchMaker.Endpoint, _session._logger);
                        _session._anchor.Conversation.RemoteParticipantAttendanceChanged += _session.HandleAttendanceChanged;
                        _session._anchor.Conversation.ParticipantPropertiesChanged += _session.HandleAttendancePropertiesChanged;
                        _session._channel.RequestReceived += _session.HandleMonitoringSessionRequestReceived;
                        try
                        {
                         _session._anchor.BeginStartUp(sup=>
                        {
                            try
                            {
                            _session._anchor.EndStartup(sup);

                            try
                            {
                                _session.BeginStartSilentMonitoring(ssm =>
                                {
                                    try
                                    {
                                        _session._listOfMonitoringChannels = _session.EndStartSilentMonitoring(ssm);
                                        _session.UpdateState(MonitoringSessionState.SilentMonitoring);
                                        _session.SendMessageToSupervisor("\r\n :::::::::: Start Monitoring Session of agent: " + _session._agentToMonitor.SignInAddress + " :::::::::::::::::");

                                        this.SetAsCompleted(null, false);
                                    }
                                    catch (RealTimeException rtex)
                                    {
                                        _session.BeginShutDown(sd => { _session.EndShutDown(sd); }, null);
                                        this.SetAsCompleted(rtex, false);
                                    }
                                },
                                                                        null);

                            }
                            catch (InvalidOperationException ivoex)
                            {
                                _session.BeginShutDown(sd => { _session.EndShutDown(sd); }, null);
                                this.SetAsCompleted(new OperationFailureException("AcdMonitoringSession failed begin silent monitoring ", ivoex), false);
                            }
                                                           
                            }
                            catch(RealTimeException rtex)
                            {
                            _session.BeginShutDown(sd => {_session.EndShutDown(sd);}, null);
                            this.SetAsCompleted(rtex, false);                         
                            }
                                                     
                        },
                                                      null);
                        }
                        catch(InvalidOperationException ivoex)
                        {
                           _session.BeginShutDown(sd => {_session.EndShutDown(sd);}, null);

                          this.SetAsCompleted(new OperationFailureException("AcdMonitoringSession failed to begin start up the anchor", ivoex), false);
                        }


                    }
                    else
                    {
                      this.SetAsCompleted( new OperationFailureException("AcdMonitoringSession failed to start maybe because the agent just got deallocated."), false);

                    }
                }
                else
                {
                  this.SetAsCompleted(new OperationFailureException("AcdMonitoringSession failed to start maybe because the agent got deallocated."), false);    
                }
                
            
            }
        
        }


        #endregion


        #region ShutDownAsyncResult
        private class ShutDownAsyncResult : AsyncResultNoResult
        {
            private AcdMonitoringSession _session;

            internal ShutDownAsyncResult(AcdMonitoringSession session, AsyncCallback userCallback, object state)
                : base(userCallback, state)
            {
                _session = session;
            }

            internal void Process()
            {
                if (null != _session._backEndConversation)
                {
                    _session._backEndConversation.BeginTerminate(ter =>
                    { _session._backEndConversation.EndTerminate(ter); },
                    null);
                }

                if (null != _session._anchor)
                {

                    _session._anchor.BeginShutDown(sd => 
                    {
                       _session._anchor.EndShutDown(sd);
                       _session.UpdateState(MonitoringSessionState.Terminated);
                       _session._listOfShutDownAsyncResults.ForEach(sdar => { sdar.SetAsCompleted(null, false); });
                       this.SetAsCompleted(null, false);

                    }, null);


                }
               
            }

        }
        #endregion


        #region StartSilentMonitoringAsyncResult
        private class StartSilentMonitoringAsyncResult : AsyncResult<List<AcdServiceChannel>>
        {
            private AcdMonitoringSession _monitoringSession;
            private List<AcdServiceChannel> _listOfMonitoringChannels = new List<AcdServiceChannel>();
            private bool _proceedingWithAudioMonitoring = false;
            private object _syncRoot = new object();

            internal StartSilentMonitoringAsyncResult(AcdMonitoringSession monitoringSession, AsyncCallback userCallback, object state)
                : base(userCallback, state)
            {
                _monitoringSession = monitoringSession;
            }


            internal void HandleModalityAdditionsWhileMonitoring(object sender, PropertiesChangedEventArgs<ConversationProperties> e)
            {
                Conversation conversation = sender as Conversation;
                ParticipantEndpoint localEndpoint = conversation.LocalParticipant.GetEndpoints()[0];

                foreach (string key in e.ChangedPropertyNames)
                {
                    if (key.Equals("ConversationActiveMediaTypes", StringComparison.OrdinalIgnoreCase))
                    {

                        // Enumerate all modalities of the conference and evaluate those where the local participant
                        // does not participate. Add this modality if not already available.
                        foreach (string modality in e.Properties.ActiveMediaTypes)
                        {
                            if (!localEndpoint.GetActiveMediaTypes().Contains(modality))
                            {

                                if (modality.Equals(MediaType.Audio, StringComparison.OrdinalIgnoreCase))
                                {
                                    lock (_syncRoot)
                                    {
                                        if (_proceedingWithAudioMonitoring == true)
                                            return;
                                        else
                                            _proceedingWithAudioMonitoring = true;
                                    }

                                    AcdServiceChannel backEndAudioMonitoringChannel = null;
                                    _listOfMonitoringChannels.ForEach( sc =>
                                    { 
                                        if (String.IsNullOrEmpty(sc.MediaType) ||
                                            sc.MediaType.Equals(MediaType.Audio, StringComparison.OrdinalIgnoreCase))
                                        {
                                            backEndAudioMonitoringChannel = sc; 
                                        }
                                    });
                                    
                                    bool success = true;
                                    Exception exception = null;

                                    AudioVideoCall audioCallToSupervisor = new AudioVideoCall(_monitoringSession._initialSupervisorCall.Conversation);
                                     
                                     try
                                     {
                                         backEndAudioMonitoringChannel.BeginStartUp(audioCallToSupervisor,
                                                                                    ServiceChannelType.DialOut,
                                                                                    McuMediaChannelStatus.SendReceive,
                                                                                    sup2 =>
                                        {
                                            try
                                            {
                                                backEndAudioMonitoringChannel.EndStartUp(sup2);
                                                backEndAudioMonitoringChannel.BeginStartSilentMonitoring(sm =>
                                                {
                                                    try
                                                    {
                                                        backEndAudioMonitoringChannel.EndStartSilentMonitoring(sm);


                                                    }
                                                    catch (InvalidOperationException ivoex)
                                                    {
                                                        success = false;
                                                        this.BeginShutDown(sdp => { this.EndShutDown(sdp); }, null);
                                                        exception = new OperationFailureException("AcdMonitoringSession failed end start the silent monitoring session", ivoex);
                                                                                                                                                       
                                                    }
                                                    catch (RealTimeException rtex)
                                                    {
                                                        success = false;
                                                        this.BeginShutDown(sdp => { this.EndShutDown(sdp); }, null);
                                                        exception = rtex;
                                                                                                                                                         
                                                    }        
                                                    finally
                                                    {
                                                        if (success)
                                                        {
                                                            if (!this.IsCompleted)
                                                                this.SetAsCompleted(_listOfMonitoringChannels, false);

                                                        }
                                                        else
                                                        {
                                                            if (!this.IsCompleted)
                                                            {
                                                                this.SetAsCompleted(exception, false);
                                                            }                                         
                                                        }
                        
                                                    }
                                                },
                                                            null);


                                                }
                                                catch (InvalidOperationException ivoex)
                                                {
                                                    success = false;
                                                    exception = new OperationFailureException("AcdMonitoringSesion failed start silent monitoring", ivoex);
                                                }
                                                catch (RealTimeException rtex)
                                                {
                                                    success = false;
                                                    exception =rtex;                                                    
                                                        
                                                }
                                                finally
                                                {
                                                   if (!success)
                                                   {
                                                    if (!this.IsCompleted)
                                                    {   
                                                        this.SetAsCompleted(exception, false);
                                                    }

                                                   }
                                            
                                                }

                                            },
                                                                                   null);
                                     }
                                     catch(InvalidOperationException ivoex)
                                     {
                                       if (!this.IsCompleted)
                                         this.SetAsCompleted( new OperationFailureException("AcdMonitoringSession failed to begin starting the channel " ,ivoex), false);
                                     }

                                    /////////////////////////////
                                    if (  _monitoringSession._state == MonitoringSessionState.Whispering
                                        ||_monitoringSession._state == MonitoringSessionState.SilentToWhispering)
                                    {
                                        try
                                        {
                                            backEndAudioMonitoringChannel.BeginEstablishPrivateAudioChannel(_monitoringSession._agentToMonitor.SignInAddress,
                                                                                                           false,
                                                                                                           eap =>
                                            {
                                                try
                                                {
                                                    backEndAudioMonitoringChannel.EndEstablishPrivateAudioChannel(eap);
                                                                         
                                                }
                                                catch (RealTimeException)
                                                {
                                                    success = false;
                                                }
                                                catch (InvalidOperationException)
                                                {
                                                    success = false;
                                                }
                                                finally
                                                {
                                                    if (success)
                                                    {
                                                        _monitoringSession.SendMessageToSupervisor("Audio whisper success \r\n");
                                                    }
                                                    else
                                                    {
                                                        _monitoringSession._logger.Log("AcdMonitoringSession failed to start the whispering mode on audio escalation");
                                                                        
                                                    }

                                                }
                                            },
                                                                                null);
                                        }
                                        catch (InvalidOperationException ivoex)
                                        {
                                            success = false;
                                            exception = ivoex;
                                        }
                                        finally
                                        {
                                            if (!success)
                                            {
                                                _monitoringSession._logger.Log("AcdMonitoringSession failed to start the whispering mode on audio escalation");
                                            }

                                        }                                                                                                                                                           
                                    }
                                    else if (_monitoringSession._state == MonitoringSessionState.BargeingIn)
                                    {

                                        try
                                        {
                                            backEndAudioMonitoringChannel.BeginBargeIn(bi =>
                                            {
                                                try
                                                {
                                                    backEndAudioMonitoringChannel.EndBargeIn(bi);
                                                }
                                                catch (RealTimeException rtex)
                                                {
                                                    _monitoringSession._logger.Log("AcdMonitoringSession failed to end bargeing in on audio escalation", rtex);
                                                                    
                                                }
                                            },
                                                                          null);
                                        }
                                        catch (InvalidOperationException ivoex)
                                        {
                                           _monitoringSession._logger.Log("AcdMonitoringSession failed to start bargeing in on audio escalation", ivoex);                                                            
                                        }
                                                        
                                                        
                                    }
                                                                                                                                                         
                                    if (null != _monitoringSession._backEndConversation)
                                    {
                                        AudioVideoCall avCall = new AudioVideoCall(_monitoringSession._backEndConversation);

                                        backEndAudioMonitoringChannel.ApplicationContext = avCall;
                                        try
                                        {
                                            avCall.BeginEstablish(est =>
                                            {
                                                try
                                                {
                                                    avCall.EndEstablish(est);
                                                }
                                                catch (RealTimeException rtex)
                                                {
                                                    _monitoringSession._logger.Log("AcdMonitoringSession: failed to end establish an audio video call.", rtex);
                                                }
                                            },
                                                                null);
                                        }
                                        catch (InvalidOperationException ivoex)
                                        {
                                            _monitoringSession._logger.Log("AcdMonitoringSession failed to start establish audio call upon dialing into AVMCU for whispering", ivoex);
                                        }
                                    }

                                 }

                             }
                         }

                    }
                
                }
            }





            internal void OnSilentMonitoringSessionEnding(object sender, ConferenceServicesAnchorStateChangedEventArgs e)
            {
                AcdConferenceServicesAnchor anchor = sender as AcdConferenceServicesAnchor;
                switch (e.NewState)
                {

                    case ConferenceServicesAnchorState.Terminating:

                        _monitoringSession.BeginShutDown(sd =>
                                                         {
                                                            _monitoringSession.EndShutDown(sd);
                                                         },
                                                         null);
                        break;

                    case ConferenceServicesAnchorState.Terminated:

                        anchor.Conversation.PropertiesChanged -= this.HandleModalityAdditionsWhileMonitoring;
                        anchor.StateChanged-=this.OnSilentMonitoringSessionEnding;
                        break;
                }


            }

            internal void Process()
            {
                    AcdConferenceServicesAnchor anchor = _monitoringSession._anchor;

                    anchor.StateChanged += this.OnSilentMonitoringSessionEnding;

                    AcdServiceChannel backEndImMonitoringChannel = new AcdServiceChannel(anchor, _monitoringSession._logger);
                    AcdServiceChannel backEndAudioMonitoringChannel = new AcdServiceChannel(anchor, _monitoringSession._logger, true);

                    _listOfMonitoringChannels.Add(backEndImMonitoringChannel);
                    _listOfMonitoringChannels.Add(backEndAudioMonitoringChannel);

                    bool success = true;
                    Exception exception = null;

                    try
                    {
                        backEndImMonitoringChannel.BeginStartUp(MediaType.Message,
                                                                ServiceChannelType.DialIn,
                                                                sup =>
                        {
                                try
                                {
                                    backEndImMonitoringChannel.EndStartUp(sup);

                                    InstantMessagingCall imCall = (InstantMessagingCall)backEndImMonitoringChannel.Call;
                                    _monitoringSession.RegisterSilentMonitoringEvents(imCall);

                                    anchor.Conversation.PropertiesChanged += this.HandleModalityAdditionsWhileMonitoring;

                                    if (!anchor.Conversation.GetActiveMediaTypes().Contains(MediaType.Audio))
                                    {
                                        this.SetAsCompleted(_listOfMonitoringChannels, false);
                                        return;
                                    }


                                    lock (_syncRoot)
                                    {
                                        if (_proceedingWithAudioMonitoring == true)
                                            return;
                                        else
                                            _proceedingWithAudioMonitoring = true;
                                    }

                                    AudioVideoCall audioCallToSupervisor = new AudioVideoCall(_monitoringSession._initialSupervisorCall.Conversation);

                                    try
                                    {
                                        backEndAudioMonitoringChannel.BeginStartUp(audioCallToSupervisor,
                                                                                ServiceChannelType.DialOut,
                                                                                McuMediaChannelStatus.SendReceive,
                                                                                sup2 =>
                                        {
                                            try
                                            {
                                                backEndAudioMonitoringChannel.EndStartUp(sup2);
                                                backEndAudioMonitoringChannel.BeginStartSilentMonitoring(sm =>
                                                {
                                                    try
                                                    {
                                                        backEndAudioMonitoringChannel.EndStartSilentMonitoring(sm);
                                                    }
                                                    catch (InvalidOperationException ivoex)
                                                    {
                                                        success = false;
                                                        exception = new OperationFailureException("AcdMonitoringSession failed end starting silent monitoring",ivoex);
                                                    }
                                                    catch (RealTimeException rtex)
                                                    {
                                                        success = false;
                                                        exception = rtex;

                                                    }
                                                    finally
                                                    {
                                                        if (success)
                                                        {
                                                            if (!this.IsCompleted)
                                                                this.SetAsCompleted(_listOfMonitoringChannels, false);
                                                        }
                                                        else
                                                        {
                                                            if (!this.IsCompleted)
                                                                this.SetAsCompleted(exception, false);
                                                        }
                                                    }
                                                },
                                                                                                        null);


                                            }
                                            catch (InvalidOperationException ivoex)
                                            {
                                                success = false;
                                                if (!this.IsCompleted)
                                                    this.SetAsCompleted(new OperationFailureException("AcdMonitoringSession failed begin starting the channel", ivoex), false);
                                            }
                                            catch (RealTimeException rtex)
                                            {
                                                success = false;
                                                if (!this.IsCompleted)
                                                 this.SetAsCompleted(rtex, false);
                                            }

                                        },
                                                                                null);
                                    }
                                    catch (InvalidOperationException ivoex)
                                    {
                                        if (!this.IsCompleted)
                                            this.SetAsCompleted(new OperationFailureException("AcdMonitoringSession failed begin starting the audio channel",ivoex), false);
                                    }


                                }
                                catch (InvalidOperationException ivoex)
                                {
                                    this.SetAsCompleted(new OperationFailureException("AcdMonitoringSession failed end starting the im channel", ivoex), false);
                                }
                                catch (RealTimeException rtex)
                                {
                                    this.SetAsCompleted(rtex, false);
                                }

                            },
                                                                 null);

                    }
                    catch (InvalidOperationException ivoex)
                    {
                        this.SetAsCompleted(new OperationFailureException("AcdMonitoringSession failed to begin start im channel " ,ivoex), false);
                    }
            }


            private IAsyncResult BeginShutDown(AsyncCallback userCallback, object state)
            {
                ShutDownAsyncResult ar = new ShutDownAsyncResult(this, userCallback, state);

                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    ShutDownAsyncResult tempAr = waitState as ShutDownAsyncResult;
                    tempAr.Process();
                }, ar);

                return ar;
            }

            private void EndShutDown(IAsyncResult ar)
            {
                ShutDownAsyncResult result = ar as ShutDownAsyncResult;
                result.EndInvoke();

            }


            private class ShutDownAsyncResult : AsyncResultNoResult
            {
                private StartSilentMonitoringAsyncResult _smar;
                private int _numberOfChannels = 0;

                internal ShutDownAsyncResult(StartSilentMonitoringAsyncResult smar, AsyncCallback userCallback, object state)
                    : base(userCallback, state)
                {
                    _smar = smar;
                }

                internal void Process()
                {
                    List<AcdServiceChannel> listOfMonitoringChannelsClone = new List<AcdServiceChannel>(_smar._listOfMonitoringChannels);
                    _numberOfChannels = listOfMonitoringChannelsClone.Count();

                    listOfMonitoringChannelsClone.ForEach(sc => { sc.BeginShutDown(OnServiceChannelStopped, sc); });

                }

                private void OnServiceChannelStopped(IAsyncResult ar)
                {
                    AcdServiceChannel serviceChannel = ar.AsyncState as AcdServiceChannel;

                    serviceChannel.EndShutDown(ar);

                    if (0 == Interlocked.Decrement(ref _numberOfChannels))
                    {
                        this.SetAsCompleted(null, false);
                    }
                }


            }
        }
        #endregion

        #region WhisperAsyncResult

        private class WhisperAsyncResult: AsyncResultNoResult
        {
            private AcdConferenceServicesAnchor _anchor;
            private List<AcdServiceChannel> _listOfMonitoringChannels;
            private AcdMonitoringSession _monitoringSession;
            private string _agentToWhisperTo;
            private object _syncRoot = new object();

            internal WhisperAsyncResult(string agentToWhisperTo, AcdMonitoringSession monitoringSession, AsyncCallback userCallback, object state): base(userCallback, state)
            {
              _monitoringSession = monitoringSession;
              _anchor = _monitoringSession._anchor;
              _agentToWhisperTo = agentToWhisperTo;
              _listOfMonitoringChannels = new List<AcdServiceChannel>(_monitoringSession._listOfMonitoringChannels);
            }



            internal void Process()
            {
                _listOfMonitoringChannels.ForEach(sc =>
                {
                    if (sc.MediaType != null
                        && sc.MediaType.Equals(MediaType.Message, StringComparison.OrdinalIgnoreCase))
                    {

                        _monitoringSession._backEndConversation = new Conversation(_monitoringSession._matchMaker.Endpoint);
                        _monitoringSession._backEndConversation.Impersonate(_monitoringSession._agentToMonitor.SupervisorUri, null, null);

                        try
                        {
                            _monitoringSession._backEndConversation.ConferenceSession.BeginJoin(_anchor.ConferenceUri,
                                                                                                 null,
                                                                                                 jar =>
                            {
                                try
                                {
                                    _monitoringSession._backEndConversation.ConferenceSession.EndJoin(jar);
                                    InstantMessagingCall imCall = new InstantMessagingCall(_monitoringSession._backEndConversation);

                                    try
                                    {
                                      imCall.BeginEstablish(ear =>
                                      {

                                          try
                                          {
                                            imCall.EndEstablish(ear);
                                            _monitoringSession.BridgeSupervisorImFlowToAnchorInWhisperingMode(imCall);
                                            _monitoringSession.SendMessageToSupervisor("IM whisper success \r\n");
                                          }
                                          catch(RealTimeException rtex)
                                          {
                                            _monitoringSession._logger.Log("AcdMonitoringSession failed establishing the instant Messaging Call", rtex);
                                          }
                                      },
                                                            null);
                                     }
                                     catch(InvalidOperationException ivoex)
                                     {
                                        _monitoringSession._logger.Log("AcdMonitoringSession failed  start establishing the instant Messaging Call", ivoex);
                                     }
   
                                     if (!_anchor.Conversation.GetActiveMediaTypes().Contains(MediaType.Audio))
                                     {
                                        this.SetAsCompleted(null, false);
                                        return;
                                     }


                                        _listOfMonitoringChannels.ForEach(asc =>
                                        {
                                          if (   asc.MediaType != null
                                              && asc.MediaType == MediaType.Audio)
                                          {
                                            if (asc.State == ServiceChannelState.Established)
                                            {
                                               bool success = true;
                                               Exception exception = null;
                                               try
                                               {
                                                  asc.BeginEstablishPrivateAudioChannel(_agentToWhisperTo,
                                                                                        false,
                                                                                        eap =>
                                                {
                                                    try
                                                    {
                                                        asc.EndEstablishPrivateAudioChannel(eap); 

                                                        AudioVideoCall avCall = new AudioVideoCall(_monitoringSession._backEndConversation);

                                                        asc.ApplicationContext = avCall;

                                                        avCall.BeginEstablish(est =>
                                                        {
                                                            try
                                                            {
                                                                avCall.EndEstablish(est);
                                                            }
                                                            catch (RealTimeException rtex)
                                                            {
                                                                _monitoringSession._logger.Log("AcdMonitoringSession: failed to end establish an audio video call.", rtex);
                                                            }
                                                            finally
                                                            {
                                                                this.SetAsCompleted(null, false);
                                                            }

                                                        },
                                                                              null);
                                                        
                                                    }
                                                    catch (RealTimeException rtex)
                                                    {

                                                        _monitoringSession._logger.Log("AcdMonitoringSession: failed to establish a private channel.", rtex);
                                                        exception = rtex;
                                                        success = false;
                                                    }
                                                    catch (InvalidOperationException ivoex)
                                                    {
                                                        _monitoringSession._logger.Log("AcdMonitoringSession: failed to establish a private channel.", ivoex);
                                                        exception = new OperationFailureException("AcdMonitoringSession: failed to establish a private channel.", ivoex);
                                                        success = false;
                                                    }
                                                    finally
                                                    {
                                                        if (!success)
                                                        {
                                                            this.SetAsCompleted(exception, false);
                                                        }


                                                    }
                                                },
                                                                                        null);
                                            }
                                            catch (InvalidOperationException ivoex)
                                            {
                                                _monitoringSession._logger.Log("AcdMonitoringSession: failed to establish a private channel.", ivoex);                                                success = false;
                                                exception = new OperationFailureException("AcdMonitoringSession: failed to establish a private channel.", ivoex);
                                            }
                                            finally
                                            {
                                                if (!success)
                                                {
                                                    this.SetAsCompleted(exception, false);
                                                }

                                            }
                                          }
                                        }
            
                                    });
                                }
                                catch (RealTimeException rtex)
                                {
                                    this.SetAsCompleted(rtex, false);
                                    _monitoringSession._logger.Log("AcdMonitoringSession failed joining the customer session anchor", rtex);

                                }

                            },
                                                                                    null);
                        } 
                        catch (InvalidOperationException ivoex)
                        {
                            this.SetAsCompleted(new OperationFailureException("AcdMonitoringSession failed end joining a conference" ,ivoex), false);
                            _monitoringSession._logger.Log("AcdMonitoringSession failed start beginning joining the customer session anchor.");
                        }
                    }
                });

            }

        }
        #endregion

        #region BargeInAsyncResult

        private class BargeInAsyncResult:AsyncResultNoResult
        {
            AcdMonitoringSession _monitoringSession;
            List<AcdServiceChannel> _listOfMonitoringChannels;
            AcdConferenceServicesAnchor _anchor;
            internal BargeInAsyncResult(AcdMonitoringSession monitoringSession, AsyncCallback userCallback, object state):base(userCallback, state)
            {
                _monitoringSession = monitoringSession;
                _listOfMonitoringChannels = new List<AcdServiceChannel>(_monitoringSession._listOfMonitoringChannels);
                _anchor = monitoringSession._anchor;
            }

            internal void Process()
            {
                if (_monitoringSession._backEndConversation == null)
                {
                    _listOfMonitoringChannels.ForEach(sc =>
                    {
                        if (sc.MediaType != null
                            && sc.MediaType.Equals(MediaType.Message, StringComparison.OrdinalIgnoreCase))
                        {

                            _monitoringSession._backEndConversation = new Conversation(_monitoringSession._matchMaker.Endpoint);
                            _monitoringSession._backEndConversation.Impersonate(_monitoringSession._agentToMonitor.SupervisorUri, null, null);

                            try
                            {
                                _monitoringSession._backEndConversation.ConferenceSession.BeginJoin(_anchor.ConferenceUri,
                                                                                                     null,
                                                                                                     jar =>
                                {
                                    try
                                    {
                                        _monitoringSession._backEndConversation.ConferenceSession.EndJoin(jar);
                                        InstantMessagingCall imCall = new InstantMessagingCall(_monitoringSession._backEndConversation);

                                        try
                                        {
                                            imCall.BeginEstablish(ear =>
                                            {

                                                try
                                                {
                                                    imCall.EndEstablish(ear);
                                                    _monitoringSession.BridgeSupervisorImFlowToAnchorInWhisperingMode(imCall);
                                                }
                                                catch (RealTimeException rtex)
                                                {
                                                    _monitoringSession._logger.Log("AcdMonitoringSession failed to establish the ImCall upon bargeing in", rtex);
                                                }

                                            },
                                                                null);
                                        }
                                        catch (InvalidCastException ivoex)
                                        {
                                            _monitoringSession._logger.Log("AcdMonitoringSession failed to end establish the ImCall upon bargeing in", ivoex);
                                        }
                                    }
                                    catch (RealTimeException rtex)
                                    {
                                        _monitoringSession._logger.Log("AcdMonitoringSession failed to end join the customer conference", rtex);

                                    }
                                },
                                   null);
                            }
                            catch (InvalidOperationException ivoex)
                            {
                                _monitoringSession._logger.Log("AcdMonitoringSession failed to join the conference of the customer as a visible user", ivoex);
                            }
                        }

                    });
                }


                if (!_anchor.Conversation.GetActiveMediaTypes().Contains(MediaType.Audio))
                {
                    this.SetAsCompleted(null, false);
                    return;
                }


                _listOfMonitoringChannels.ForEach(asc =>
                {
                    if (asc.MediaType != null
                        && asc.MediaType.Equals(MediaType.Audio, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            asc.BeginBargeIn(bi =>
                            {
                                try
                                {
                                    asc.EndBargeIn(bi);
                                    this.SetAsCompleted(null, false);

                                }
                                catch(RealTimeException rtex)
                                {
                                    _monitoringSession._logger.Log("AcdMonitoring session failed to end barge in", rtex);
                                    this.SetAsCompleted(rtex, false);
                                }
                            },
                                            null);
                        }
                        catch(InvalidOperationException ivoex)
                        {
                            this.SetAsCompleted(new OperationFailureException("AcdMonitoringSession failed to begin barge in.",ivoex), false);
                            _monitoringSession._logger.Log("AcdMonitoringSession failed to begin barge in", ivoex);
                        }
                        }
                    });
               
                }
        
        }

        #endregion


    }

    internal enum MonitoringSessionState {Idle=0, Starting=1, SilentMonitoring = 2, SilentToWhispering = 3, Whispering =4, BargeingIn = 5, Terminating = 6, Terminated = 7};
    internal class MonitoringSessionStateChangedEventArgs : EventArgs
    {
        private MonitoringSessionState _previousState;
        private MonitoringSessionState _newState;

        internal MonitoringSessionStateChangedEventArgs(MonitoringSessionState previousState, MonitoringSessionState newState)
        {
            _previousState = previousState;
            _newState = newState;
        }

        internal MonitoringSessionState PreviousState
        {
            get { return _previousState; }
        }

        internal MonitoringSessionState NewState
        {
            get { return _newState; }
        }
    }
}
