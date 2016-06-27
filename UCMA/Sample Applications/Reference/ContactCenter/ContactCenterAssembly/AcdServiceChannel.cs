/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.AudioVideo;
using System.Diagnostics;
using Microsoft.Rtc.Collaboration.Samples.Utilities;
using System.Collections.ObjectModel;
using System.Runtime.Remoting.Messaging;
using Microsoft.Rtc.Collaboration.Samples.ApplicationSharing;
using System.Threading;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    internal class AcdServiceChannel
    {
        #region Private Fields
        private AcdConferenceServicesAnchor _anchor;
        private Call _call;
        private ServiceChannelState _state;
        private string _mediaType;
        private object _syncRoot = new object();
        private AcdLogger _logger;
        private ServiceChannelType _channelType;
        private List<ParticipantEndpoint> _listOfParticipantsOutsideTheMix = new List<ParticipantEndpoint>();
        private List<ShutDownAsyncResult> _listOfShutDownAsyncResults = new List<ShutDownAsyncResult>();
        private bool _primaryChannel = false;
        #endregion

        #region constructor

        internal AcdServiceChannel(AcdConferenceServicesAnchor anchor, AcdLogger logger)
        {
            _anchor = anchor;
            _logger = logger;
            anchor.AddServiceChannel(this);
            this.UpdateState(ServiceChannelState.Idle);
        }

        internal AcdServiceChannel(AcdConferenceServicesAnchor anchor, AcdLogger logger, bool primaryChannel)
        {
            _anchor = anchor;
            _logger = logger;
            _primaryChannel = primaryChannel;
            anchor.AddServiceChannel(this);
            this.UpdateState(ServiceChannelState.Idle);
        }

        #endregion

        #region Internal Properties

        internal Call Call
        { 
            get { return _call; } 
        }

        internal ServiceChannelState State
        {
            get { return _state; }
        }

        internal ServiceChannelType ChannelType
        {
            get { return _channelType; }
        }

        internal string MediaType
        {
            get { return _mediaType; }
        }

        internal bool IsPrimaryServiceChannel
        {
            get { return _primaryChannel; }
        }

        internal object ApplicationContext
        {
            get;
            set;

        }


        #endregion

        #region Internal events

        internal event EventHandler<ServiceChannelStateChangedEventArgs> StateChanged;

        #endregion

        #region Internal Methods

        internal IAsyncResult BeginStartUp(string mediaType, ServiceChannelType type, AsyncCallback userCallback, Object state)
        {
            StartUpAsyncResult ar = new StartUpAsyncResult(this, type, McuMediaChannelStatus.None, null, userCallback, state);

            _mediaType = mediaType;

            bool firstTime = false;
            lock (_syncRoot)
            {
                if (_state == ServiceChannelState.Idle)
                {
                    this.UpdateState(ServiceChannelState.Establishing);
                    firstTime = true;
                }
                else
                {
                   throw new InvalidOperationException("ServiceChannel is already started");
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

        internal IAsyncResult BeginStartUp(
            AudioVideoCall call, 
            ServiceChannelType type, 
            McuMediaChannelStatus dialOutMediaDirection, 
            AsyncCallback userCallback, 
            Object state)
        {
            StartUpAsyncResult ar = new StartUpAsyncResult(this, type, dialOutMediaDirection, call, userCallback, state);

            _channelType = type;
            _mediaType = Microsoft.Rtc.Collaboration.MediaType.Audio;

            bool firstTime = false;
            lock (_syncRoot)
            {
                if (_state == ServiceChannelState.Idle)
                {
                    this.UpdateState(ServiceChannelState.Establishing);
                    firstTime = true;
                }
                else
                {
                   throw new InvalidOperationException("ServiceChannel is already started");
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

        internal void EndStartUp(IAsyncResult ar)
        {
            StartUpAsyncResult result = ar as StartUpAsyncResult;
            result.EndInvoke();
        }

        internal IAsyncResult BeginEstablishPrivateAudioChannel(
            string participantUri, 
            bool putParticipantOutsideTheMix, 
            AsyncCallback userCallback, 
            object state)
        {
            EstablishPrivateAudioChannelAsyncResult ar = 
                new EstablishPrivateAudioChannelAsyncResult(this, participantUri,putParticipantOutsideTheMix, _logger, userCallback, state);
            bool process = false;

            SipUriParser uriParser;
            if (!SipUriParser.TryParse(participantUri, out uriParser))
            {
                ar.SetAsCompleted(new ArgumentException("ServiceChannel cannot establish a private channel.", "participantUri"), true);
            }

            if (putParticipantOutsideTheMix && !_anchor.IsPrimaryChannelCreated)
            {
                ar.SetAsCompleted(new ArgumentException("ServiceChannel cannot establish a private channel if the channel is not a primary one.", "putParticipantOutsideTheMix"), true);
            }

            lock (_syncRoot)
            {
                if (this._state == ServiceChannelState.Established)
                {
                    process = true;
                }
                else
                {
                    throw new InvalidOperationException("ServiceChannel is not in a state allowing to establish a private channel.");
                }
            }

            if (process)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    var tempAr = waitState as EstablishPrivateAudioChannelAsyncResult;
                    tempAr.Process();
                }, ar);
            }

            return ar;

        }

        internal void EndEstablishPrivateAudioChannel(IAsyncResult ar)
        {
            EstablishPrivateAudioChannelAsyncResult result = ar as EstablishPrivateAudioChannelAsyncResult;
            result.EndInvoke();
        }

        internal IAsyncResult BeginShutDown(AsyncCallback userCallback, Object state)
        {
            ShutDownAsyncResult ar = new ShutDownAsyncResult(this, userCallback, state);

            bool firstTime = false;

            lock (_syncRoot)
            {
                if (_state < ServiceChannelState.Terminating)
                { 
                    firstTime = true;
                    this.UpdateState(ServiceChannelState.Terminating);
                }
                else if (_state == ServiceChannelState.Terminating)
                {
                    _listOfShutDownAsyncResults.Add(ar);                  
                }
                else if (_state == ServiceChannelState.Terminated)
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

        internal IAsyncResult BeginStartSilentMonitoring(AsyncCallback userCallBack, object state)
        {
            StartSilentMonitoringAsyncResult ar = new StartSilentMonitoringAsyncResult(this, userCallBack, state);

            bool process = false;

            lock (_syncRoot)
            {
                if (this._state == ServiceChannelState.Established)
                {
                    process = true;
                }
                else
                {
                    throw new InvalidOperationException("ServiceChannel is not in a state allowing to establish a private channel.");
                }
            }

            if (process)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    var tempAr = waitState as StartSilentMonitoringAsyncResult;
                    tempAr.Process();
                }, ar);
            }

            return ar;
            
        }

        internal void EndStartSilentMonitoring(IAsyncResult ar)
        {
            StartSilentMonitoringAsyncResult result = ar as StartSilentMonitoringAsyncResult;
            result.EndInvoke();    
        }

        internal IAsyncResult BeginBringAllChannelParticipantsInMainAudioMix(AsyncCallback userCallBack, object state)
        {
            BringAllChannelParticipantsInMainAudioMixAsyncResult ar = new BringAllChannelParticipantsInMainAudioMixAsyncResult(this, _logger, userCallBack, state);

            bool process = false;

            lock (_syncRoot)
            {
                if (  _state == ServiceChannelState.Established
                    ||_state == ServiceChannelState.Terminating)
                {
                    process = true;
                }
                else
                {
                    throw new InvalidOperationException("ServiceChannel is not in a state allowing to establish a private channel.");
                }
            }

            if (process)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    var tempAr = waitState as BringAllChannelParticipantsInMainAudioMixAsyncResult;
                    tempAr.Process();
                }, ar);            
            }

            return ar;                
        }

        internal void EndBringAllChannelParticipantsInMainAudioMix(IAsyncResult ar)
        {
            BringAllChannelParticipantsInMainAudioMixAsyncResult result = ar as BringAllChannelParticipantsInMainAudioMixAsyncResult;
            result.EndInvoke();
        }

        internal IAsyncResult BeginBargeIn(AsyncCallback userCallback, object state)
        {
            BargeInAsyncResult ar = new BargeInAsyncResult(this, _logger, userCallback, state);

            bool process = false;

            lock (_syncRoot)
            {
                if (  _state == ServiceChannelState.Established)
                {
                    process = true;
                }
                else
                {
                    throw new InvalidOperationException("ServiceChannel is not in a state allowing to barge in.");
                }
            }

            if (process)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    BargeInAsyncResult tempAr = waitState as BargeInAsyncResult;
                    tempAr.Process();
                }, ar);
            }

            return ar;                
        
        }

        internal void EndBargeIn(IAsyncResult ar)
        {
            BargeInAsyncResult result = ar as BargeInAsyncResult;
            result.EndInvoke();
        
        }
        #endregion

        #region Private Methods
        private void UpdateState(ServiceChannelState state)
        {
            ServiceChannelState previousState = _state;
            _state = state;
            EventHandler<ServiceChannelStateChangedEventArgs> handler = this.StateChanged;

            if (handler != null)
            {
                handler(this, new ServiceChannelStateChangedEventArgs(previousState, state));
            }
        }

        private void OnCallStateChanged(object sender, CallStateChangedEventArgs args)
        {
            if (args.State == CallState.Terminating)
            {
                if (this.ApplicationContext is Call)
                {
                    Call call = (Call)this.ApplicationContext;
                    call.BeginTerminate(ter => { call.EndTerminate(ter); }, null);
                }

                lock (_syncRoot)
                {
                    this.UpdateState(ServiceChannelState.Terminating);
                }
            }
            else if (args.State == CallState.Terminated)
            {
                if (null != _call)
                {
                    if (_call is AudioVideoCall)
                    {
                        AudioVideoCall call = _call as AudioVideoCall;
                    

                        if (null != call.Flow)
                        {
                            if (null != call.Flow.Player)
                            {
                                call.Flow.Player.DetachFlow(call.Flow);

                            }

                            if (null != call.Flow.Recorder)
                            {
                                call.Flow.Recorder.DetachFlow();

                            }

                            if (null != call.Flow.SpeechRecognitionConnector)
                            {
                                SpeechRecognitionConnector connector = call.Flow.SpeechRecognitionConnector;
                                connector.DetachFlow();
                                connector.Dispose();   
                            }

                            if (null != call.Flow.SpeechSynthesisConnector)
                            {
                                SpeechSynthesisConnector connector = call.Flow.SpeechSynthesisConnector;
                                connector.DetachFlow();
                                connector.Dispose();
                            }

                            if (null != call.Flow.ToneController)
                            {
                                call.Flow.ToneController.DetachFlow();
                            }
                        }
                    }
                }

                lock (_syncRoot)
                {
                    this.UpdateState(ServiceChannelState.Terminated);
                }
            }
        }

        #endregion

        #region StartUpAsyncResult

        private class StartUpAsyncResult : AsyncResultNoResult
        {
            private AcdServiceChannel _channel;
            private ServiceChannelType _type;
            private Call _call;
            private McuMediaChannelStatus _dialOutMediaDirection;
            private object _syncRoot = new object();

            internal StartUpAsyncResult(
                AcdServiceChannel channel, 
                ServiceChannelType type, 
                McuMediaChannelStatus dialOutMediaDirection, 
                Call call, 
                AsyncCallback userCallback, 
                object state)
                : base(userCallback, state)
            {
                _channel = channel;
                _type = type;
                _call = call;
                _dialOutMediaDirection = dialOutMediaDirection;
            }

            internal void Process()
            {
                if (_channel._anchor.State == ConferenceServicesAnchorState.Established)
                {
                    if (_type == ServiceChannelType.DialIn)
                    {
                        if (null != _call)
                        {
                            if (_call is AudioVideoCall)
                            {
                                if (_call.State == CallState.Incoming)
                                {
                                    _channel._call = new AudioVideoCall(_channel._anchor.Conversation);

                                    _channel._call.StateChanged += _channel.OnCallStateChanged;

                                    BackToBackCallSettings settings = new BackToBackCallSettings(_channel._call);

                                    AudioVideoCallEstablishOptions options = new AudioVideoCallEstablishOptions();

                                    options.UseGeneratedIdentityForTrustedConference = !_channel._primaryChannel;
                                    options.AudioVideoMcuDialInOptions.RemoveFromDefaultRouting = true;

                                    settings.CallEstablishOptions = options;

                                    BackToBackCall backToBackUA =  new BackToBackCall(new BackToBackCallSettings((AudioVideoCall)_call), settings);

                                    try
                                    {
                                        backToBackUA.BeginEstablish(ar=>
                                                                   {
                                                                       try
                                                                       {
                                                                           backToBackUA.EndEstablish(ar);

                                                                           lock (_channel._syncRoot)
                                                                           {
                                                                               _channel.UpdateState(ServiceChannelState.Established);
                                                                           }
                                                                           this.SetAsCompleted(null, false);
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
                                        _channel._logger.Log("ServiceChannel failed to back to back the incoming Call", ivoex);
                                        this.SetAsCompleted(new OperationFailureException("ServiceChannel failed to back to back the incoming Call", ivoex), false);
                                    }
                                }
                                else
                                {
                                    _channel._logger.Log("ServiceChannel failed to back to back Call 1 as it is in an invalid state.");
                                     this.SetAsCompleted(new OperationFailureException("ServiceChannel failed back to backing the call as it is in an invalid state."), false);
                                }
                            }
                            else
                            {
                                throw new NotImplementedException("AcdServiceChannel did not implement this feature");
                            }
                        }
                        else
                        {
                            if (_channel._mediaType == Microsoft.Rtc.Collaboration.MediaType.Message)
                            {
                                _channel._call = new InstantMessagingCall(_channel._anchor.Conversation);
                                _channel._call.StateChanged += _channel.OnCallStateChanged;

                                InstantMessagingCall imCall = (InstantMessagingCall)_channel._call;
                                imCall.InstantMessagingFlowConfigurationRequested += this.OnImServiceChannelFlowCreated;

                                try
                                {
                                    imCall.BeginEstablish(ar =>
                                                             {
                                                                 InstantMessagingCall call = ar.AsyncState as InstantMessagingCall;

                                                                 try
                                                                 {
                                                                     call.EndEstablish(ar);
                                                                     lock (_channel._syncRoot)
                                                                     {
                                                                         _channel.UpdateState(ServiceChannelState.Established);
                                                                     }
                                                                     this.SetAsCompleted(null, false);
                                                                 }
                                                                 catch (RealTimeException rtex)
                                                                 {
                                                                     this.SetAsCompleted(rtex, false);
                                                                 }
                                                             },
                                                             _channel._call);
                                }
                                catch (InvalidOperationException ivoex)
                                {
                                    this.SetAsCompleted(new OperationFailureException("AcdServiceChannel failed begin starting an IM call ",ivoex), false);
                                }
                            }
                            else if (_channel._mediaType == Microsoft.Rtc.Collaboration.MediaType.Audio)
                            {
                                _channel._call = new AudioVideoCall(_channel._anchor.Conversation);
                                _channel._call.StateChanged += _channel.OnCallStateChanged;
                                AudioVideoCall avCall = _channel._call as AudioVideoCall;

                                avCall.AudioVideoFlowConfigurationRequested += new EventHandler<AudioVideoFlowConfigurationRequestedEventArgs>(avCall_AudioVideoFlowConfigurationRequested);

                                AudioVideoCall audioCall = (AudioVideoCall)_channel._call;

                                AudioVideoCallEstablishOptions options = new AudioVideoCallEstablishOptions();
                                options.UseGeneratedIdentityForTrustedConference = !_channel._primaryChannel;
                                options.AudioVideoMcuDialInOptions.RemoveFromDefaultRouting = true;

                                try
                                {
                                    audioCall.BeginEstablish(options,
                                    ar =>
                                    {
                                        AudioVideoCall call = ar.AsyncState as AudioVideoCall;

                                        try
                                        {
                                            call.EndEstablish(ar);

                                        }
                                        catch (RealTimeException rtex)
                                        {
                                            this.SetAsCompleted(rtex, false);

                                        }

                                    },
                                    _channel._call);
                                }
                                catch (InvalidOperationException ivoex)
                                {
                                    this.SetAsCompleted(new OperationFailureException("AcdServiceChannel failed to establish audio call", ivoex), false);
                                }
                            }
                            else
                            {
                                this.SetAsCompleted(new NotImplementedException("AcdServiceChannel does not support this action."), false);
                            }
                        }
                    }
                    else if (_type == ServiceChannelType.DialOut)
                    {
                        if (null != _call)
                        {
                            if (_channel._mediaType == Microsoft.Rtc.Collaboration.MediaType.Audio)
                            {

                                _channel._anchor.Conversation.Endpoint.RegisterForIncomingCall<AudioVideoCall>(this.ProcessIncomingCall);

                                AudioVideoMcuSession mcuSession = _channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession;

                                var options = new AudioVideoMcuDialOutOptions();
                                options.PrivateAssistantDisabled = true;
                                options.Media.Add(new McuMediaChannel(Microsoft.Rtc.Collaboration.MediaType.Audio, _dialOutMediaDirection));
                                options.ParticipantUri = _channel._anchor.Conversation.LocalParticipant.Uri;
                                options.RemoveFromDefaultRouting = true;


                                try
                                {
                                    mcuSession.BeginDialOut(_channel._anchor.Endpoint.EndpointUri,
                                    options,
                                    ar =>
                                    {
                                        AudioVideoMcuSession avMcuSession = ar.AsyncState as AudioVideoMcuSession;
                                        try
                                        {
                                            avMcuSession.EndDialOut(ar);
                                            lock (_channel._syncRoot)
                                            {
                                                _channel.UpdateState(ServiceChannelState.Established);
                                                if (!this.IsCompleted)
                                                {
                                                    _channel._anchor.Conversation.Endpoint.UnregisterForIncomingCall<AudioVideoCall>(this.ProcessIncomingCall);

                                                    this.SetAsCompleted(null, false);
                                                }
                                            }

                                        }
                                        catch (RealTimeException rtex)
                                        {
                                            _channel.BeginShutDown(sd => { _channel.EndShutDown(sd); }, null);
                                            lock (_syncRoot)
                                            {
                                                if (!this.IsCompleted)
                                                {
                                                    _channel._anchor.Conversation.Endpoint.UnregisterForIncomingCall<AudioVideoCall>(this.ProcessIncomingCall);
                                                    this.SetAsCompleted(rtex, false);
                                                }
                                            }
                                        }
                                    },
                                    mcuSession);
                                }
                                catch (InvalidOperationException ivoex)
                                {
                                    _channel.BeginShutDown(sd => { _channel.EndShutDown(sd); }, null);
                                    _channel._anchor.Conversation.Endpoint.UnregisterForIncomingCall<AudioVideoCall>(this.ProcessIncomingCall);
                                    this.SetAsCompleted(ivoex, false);                                
                                }
                            }
                            else
                            {
                                this.SetAsCompleted(new NotImplementedException("AcdServiceChannel does not support this operation."), false);
                            }
                        }
                        else
                        {
                            this.SetAsCompleted(new NotImplementedException("AcdServiceChannel does not support this operation."), false);
                        }
                    }
                }
                else
                {
                   this.SetAsCompleted(new OperationFailureException("AcdServiceChannel could not be started because the anchor is not Established"), false);
                }
            }


            private void HandleAudioFlowStateChanged(object sender, MediaFlowStateChangedEventArgs e)
            {

                bool isActive = e.State == MediaFlowState.Active;

                AudioVideoFlow flow = sender as AudioVideoFlow;

                //we dont need this anymore                
                flow.StateChanged -= this.HandleAudioFlowStateChanged;

                if (e.State == MediaFlowState.Active)
                {
                    lock (this._channel._syncRoot)
                    {
                        _channel.UpdateState(ServiceChannelState.Established);

                        if (!this.IsCompleted)
                        {
                            this.SetAsCompleted(null, false);
                        }
                    }                    
                }
                else
                {
                    _channel.BeginShutDown(sd => { _channel.EndShutDown(sd); }, null);
                    if (!this.IsCompleted)
                    {
                        this.SetAsCompleted(new OperationFailureException("AcdServiceChannel: service channel got terminated unexpectedly"), false);
                    }
                }
                           
            }

            private void avCall_AudioVideoFlowConfigurationRequested(object sender, AudioVideoFlowConfigurationRequestedEventArgs e)
            {
                e.Flow.StateChanged += this.HandleAudioFlowStateChanged;
                AudioVideoFlowTemplate template = new AudioVideoFlowTemplate(e.Flow);
                template.Audio.GetChannels()[0].UseHighPerformance = false;
                e.Flow.Initialize(template);
            }



            private void ProcessIncomingCall(object sender, CallReceivedEventArgs<AudioVideoCall> args)
            {
                if (args.Call.RemoteEndpoint.EndpointType == EndpointType.Conference
                    && args.Call.Conversation.ApplicationContext is AcdConferenceServicesAnchor)
                {
                    AudioVideoCall avCall = args.Call;
                    AcdConferenceServicesAnchor anchor = avCall.Conversation.ApplicationContext as AcdConferenceServicesAnchor;

                    anchor.ServiceChannels.ForEach(sc =>
                    {
                        if (sc == _channel)
                        {
                            _channel._call = avCall;

                            BackToBackCall backToback = new BackToBackCall(new BackToBackCallSettings(avCall), new BackToBackCallSettings(_call));

                            try
                            {
                                backToback.BeginEstablish(par =>
                                {
                                    try
                                    {
                                        backToback.EndEstablish(par);

                                    }
                                    catch (RealTimeException rtex)
                                    {
                                        _channel.BeginShutDown(sd => { _channel.EndShutDown(sd); }, null);

                                        lock (_syncRoot)
                                        {
                                            if (!this.IsCompleted)
                                            {
                                                _channel._anchor.Conversation.Endpoint.UnregisterForIncomingCall<AudioVideoCall>(this.ProcessIncomingCall);
                                                this.SetAsCompleted(rtex, false);
                                            }
                                        }
                                    }

                                },
                                null);

                            }
                            catch (InvalidOperationException ivoex)
                            {
                                _channel.BeginShutDown(sd => { _channel.EndShutDown(sd); }, null);

                                lock (_syncRoot)
                                {
                                    if (!this.IsCompleted)
                                    {
                                        _channel._anchor.Conversation.Endpoint.UnregisterForIncomingCall<AudioVideoCall>(this.ProcessIncomingCall);
                                        this.SetAsCompleted(new OperationFailureException("AcdServiceChannel failed begin establishing the back to back call",ivoex), false);
                                    }
                                }
                            }

                        }
                    });

                } // if

            }

            private void OnImServiceChannelFlowCreated(object sender, InstantMessagingFlowConfigurationRequestedEventArgs args)
            {
                InstantMessagingFlowTemplate template = new InstantMessagingFlowTemplate();
                template.MessageConsumptionMode = InstantMessageConsumptionMode.ProxiedToRemoteEntity;

                args.Flow.Initialize(template);
            }
        }
        #endregion

        #region ShutDownAsyncResult
        private class ShutDownAsyncResult : AsyncResultNoResult
        {
            private AcdServiceChannel _channel;
            internal ShutDownAsyncResult(AcdServiceChannel channel, AsyncCallback userCallback, object state)
                : base(userCallback, state)
            {
                _channel = channel;
            }

            internal void Process()
            {
                try
                {
                    _channel.BeginBringAllChannelParticipantsInMainAudioMix(bacpimam =>
                    {
                        try
                        {
                            _channel.EndBringAllChannelParticipantsInMainAudioMix(bacpimam);
                        }
                        catch (RealTimeException)
                        {
                        }

                        if (null != _channel._call)
                        {

                            _channel._call.BeginTerminate(ar => { AcdServiceChannel channel = ar.AsyncState as AcdServiceChannel; _channel.Call.EndTerminate(ar); },
                                                            _channel);
                        }

                        _channel._anchor.RemoveServiceChannel(_channel);

                        lock (_channel._syncRoot)
                        {
                            _channel.UpdateState(ServiceChannelState.Terminated);
                        }

                        _channel._listOfShutDownAsyncResults.ForEach( sdar => {sdar.SetAsCompleted(null, false);});

                        this.SetAsCompleted(null, false);
                    },
                    null);

                }
                catch (InvalidOperationException)
                {
                    // TODO:_logger.Log("Unable to bring all participants into main audio mix.");
                }
            }
        }
        #endregion

        #region  BringAllChannelParticipantsInMainAudioMixAsyncResult
        private class BringAllChannelParticipantsInMainAudioMixAsyncResult : AsyncResultNoResult
        { 
            private AcdLogger _logger;
            private AcdServiceChannel _channel;
            private int _superRefCount;

            internal BringAllChannelParticipantsInMainAudioMixAsyncResult(AcdServiceChannel sChannel, AcdLogger logger, AsyncCallback userCallback, object state)
                : base(userCallback, state)
            {
                _logger = logger;
                _channel = sChannel;
            }

            internal void Process()
            {
                lock (_channel._syncRoot)
                {
                    _superRefCount = _channel._listOfParticipantsOutsideTheMix.Count;
                    Exception exception = null;

                    _channel._listOfParticipantsOutsideTheMix.ForEach(pe =>
                    {
                        AudioVideoMcuSession avMcu = _channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession;

                        try
                        {
                            avMcu.BeginAddToDefaultRouting(pe,
                                                           atdm =>
                                                           {
                                                               try
                                                               {
                                                                   avMcu.EndAddToDefaultRouting(atdm);
                                                               }
                                                               catch (RealTimeException rtex)
                                                               {
                                                                   exception = rtex;
                                                                   _logger.Log("AcdServiceChannel: failed to bring the participant in the mix.", rtex);
                                                               }
                                                               finally
                                                               {                                    
                                                                  if (0 == Interlocked.Decrement(ref _superRefCount))
                                                                  {
                                                                      this.SetAsCompleted(null, false);
                                                                  }
                                                               
                                                               
                                                               
                                                               }

                                                           },
                                                           null);
                        }
                        catch (InvalidOperationException ivoex)
                        {
                            _logger.Log("AcdServiceChannel: failed to bring the participant in the mix.", ivoex);
                            exception = ivoex;
                        }
                        finally
                        {
                            if (0 == Interlocked.Decrement(ref _superRefCount))
                            {
                                this.SetAsCompleted(null, false);
                            }
                        }

                    });
                }
                
        
            }        
        }

        #endregion 

        #region EstablishPrivateAudioChannelAsyncResult
        private class EstablishPrivateAudioChannelAsyncResult : AsyncResultNoResult
        {
            private AcdLogger _logger;
            private AcdServiceChannel _channel;
            private ParticipantEndpoint _participantEndpoint = null;
            private string _participantUri;
            private bool _putParticipantOutsideTheMix;
            private bool _proceeding = false;
            private object _syncRoot = new object();

            internal EstablishPrivateAudioChannelAsyncResult(AcdServiceChannel sChannel,string participantUri, bool putParticipantOutsideTheMix, AcdLogger logger, AsyncCallback userCallback, object state)
                : base(userCallback, state)
            {
                _logger = logger;
                _channel = sChannel;
                _participantUri = participantUri;
                _putParticipantOutsideTheMix = putParticipantOutsideTheMix;
            }

            private void AvmcuAttendanceChanged(object sender, ParticipantEndpointAttendanceChangedEventArgs<AudioVideoMcuParticipantEndpointProperties> args)
            {
                var endpoints = new Collection<KeyValuePair<ParticipantEndpoint,AudioVideoMcuParticipantEndpointProperties>>();
                foreach (var kvp in args.Joined)
                {
                    //If the endpoint is connected establish private channel.
                    if (kvp.Value.State == ConferenceEndpointState.Connected)                    
                    {
                        endpoints.Add(kvp);
                    }
                }
                this.EstablishPrivateChannel(endpoints);
            }

            private void AudioVideoMcuSession_ParticipantEndpointPropertiesChanged(object sender, ParticipantEndpointPropertiesChangedEventArgs<AudioVideoMcuParticipantEndpointProperties> e)
            {
                var endpoints = new Collection<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>>();
                if((e.ChangedPropertyNames.Contains("State") &&
                    e.Properties.State == ConferenceEndpointState.Connected))
                {
                    endpoints.Add(new KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>(e.ParticipantEndpoint, e.Properties));
                }
                this.EstablishPrivateChannel(endpoints);
            }

            private void EstablishPrivateChannel(Collection<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>> participantEndpointKeyValues)
            {
                AudioVideoCall call = _channel._call as AudioVideoCall;


                List<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>> listOfEndpoints = new List<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>>(participantEndpointKeyValues);

                listOfEndpoints.ForEach(endpoint =>
                {
                    if (SipUriCompare.Equals(endpoint.Key.Participant.Uri, _participantUri))
                    {
                        _participantEndpoint = endpoint.Key;
                    }
                });

                if (null == _participantEndpoint)
                {
                    return;
                }

                lock (_syncRoot)
                {
                    this._channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession.ParticipantEndpointAttendanceChanged -= this.AvmcuAttendanceChanged;
                    this._channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession.ParticipantEndpointPropertiesChanged -= this.AudioVideoMcuSession_ParticipantEndpointPropertiesChanged;

                    if (this.IsCompleted)
                        return;
                    if (_proceeding == true)
                        return;
                    else
                        _proceeding = true;
                }

                if (_putParticipantOutsideTheMix)
                {
                    AudioVideoMcuSession avMcu = _channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession;
                    RemoveFromDefaultRoutingOptions options = new RemoveFromDefaultRoutingOptions();
                    options.Duration = TimeSpan.FromMilliseconds(3600000);

                    try
                    {
                        avMcu.BeginRemoveFromDefaultRouting(_participantEndpoint,
                                                        options,
                                                       ar =>
                                                       {
                                                           try
                                                           {
                                                               avMcu.EndRemoveFromDefaultRouting(ar);
                                                           }
                                                           catch (RealTimeException rtex)
                                                           {
                                                               this.SetAsCompleted(rtex, false);
                                                               return;
                                                           }
                                                           lock (_channel._syncRoot)
                                                           {
                                                               _channel._listOfParticipantsOutsideTheMix.Add(_participantEndpoint);
                                                           }

                                                           OutgoingAudioRoute outgoingRoute = new OutgoingAudioRoute(_participantEndpoint);
                                                           outgoingRoute.Operation = RouteUpdateOperation.Add;
                                                           IncomingAudioRoute incomingRoute = new IncomingAudioRoute(_participantEndpoint);
                                                           incomingRoute.IsDtmfEnabled = true;
                                                           incomingRoute.Operation = RouteUpdateOperation.Add;

                                                           try
                                                           {
                                                               call.AudioVideoMcuRouting.BeginUpdateAudioRoutes(new List<OutgoingAudioRoute>() { outgoingRoute },
                                                                                                                new List<IncomingAudioRoute>() { incomingRoute },
                                                                                                                upar =>
                                                                                                                {
                                                                                                                    AudioVideoMcuRouting mcuRouting = upar.AsyncState as AudioVideoMcuRouting;
                                                                                                                    try
                                                                                                                    {
                                                                                                                        mcuRouting.EndUpdateAudioRoutes(upar);
                                                                                                                        this.SetAsCompleted(null, false);
                                                                                                                    }
                                                                                                                    catch (RealTimeException rtex)
                                                                                                                    {
                                                                                                                        this.SetAsCompleted(rtex, false);
                                                                                                                    }

                                                                                                                },
                                                                                                                call.AudioVideoMcuRouting);
                                                           }
                                                           catch (InvalidOperationException ivoex)
                                                           {
                                                               this.SetAsCompleted(new OperationFailureException("AcdServiceChannel failed begin updating the audio channel", ivoex), false);
                                                           }


                                                       }, null);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        this.SetAsCompleted(new OperationFailureException("AcdServiceChannel failed begin removing from default routing",ivoex), false);
                    }
                }
                else
                {
                    OutgoingAudioRoute outgoingRoute = new OutgoingAudioRoute(_participantEndpoint);
                    outgoingRoute.Operation = RouteUpdateOperation.Add;
                    IncomingAudioRoute incomingRoute = new IncomingAudioRoute(_participantEndpoint);
                    incomingRoute.IsDtmfEnabled = true;
                    incomingRoute.Operation = RouteUpdateOperation.Add;

                    try
                    {
                        call.AudioVideoMcuRouting.BeginUpdateAudioRoutes(new List<OutgoingAudioRoute>() { outgoingRoute },
                                                                         new List<IncomingAudioRoute>() { incomingRoute },
                                                                         upar =>
                                                                         {
                                                                             AudioVideoMcuRouting mcuRouting = upar.AsyncState as AudioVideoMcuRouting;
                                                                             try
                                                                             {
                                                                                 mcuRouting.EndUpdateAudioRoutes(upar);
                                                                                 this.SetAsCompleted(null, false);
                                                                             }
                                                                             catch (RealTimeException rtex)
                                                                             {
                                                                                 this.SetAsCompleted(rtex, false);
                                                                             }

                                                                         },
                                                                         call.AudioVideoMcuRouting);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        this.SetAsCompleted(new OperationFailureException("AcdServiceChannel failed begin updating the audio routes ", ivoex), false);
                    }
                }                
            }

            internal void Process()
            {
                AudioVideoCall call = _channel._call as AudioVideoCall;

                if (null != call)
                {
                    _channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession.ParticipantEndpointAttendanceChanged += this.AvmcuAttendanceChanged;
                    _channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession.ParticipantEndpointPropertiesChanged += this.AudioVideoMcuSession_ParticipantEndpointPropertiesChanged;

                    List<ParticipantEndpoint> listOfParticipantEndpoints = new List<ParticipantEndpoint>(_channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession.GetRemoteParticipantEndpoints());

                    listOfParticipantEndpoints.ForEach(ep =>
                    {
                        if (SipUriCompare.Equals(ep.Participant.Uri, _participantUri))
                        {
                            _participantEndpoint = ep;
                        }
                    });

                    if (null == _participantEndpoint)
                        return;

                    lock (_syncRoot)
                    {
                        if (this.IsCompleted)
                            return;
                        if (_proceeding == true)
                            return;
                        else
                            _proceeding = true;
                    }

                    this._channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession.ParticipantEndpointAttendanceChanged -= this.AvmcuAttendanceChanged;
                    _channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession.ParticipantEndpointPropertiesChanged -= this.AudioVideoMcuSession_ParticipantEndpointPropertiesChanged;

                    if (_putParticipantOutsideTheMix)
                    {
                        AudioVideoMcuSession avMcu = _channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession;
                        RemoveFromDefaultRoutingOptions options = new RemoveFromDefaultRoutingOptions();
                        options.Duration = TimeSpan.FromMilliseconds(3600000);

                        try
                        {
                            avMcu.BeginRemoveFromDefaultRouting(_participantEndpoint,
                                                                options,
                                                                 ar =>
                                                                 {
                                                                     try
                                                                     {
                                                                         avMcu.EndRemoveFromDefaultRouting(ar);
                                                                     }
                                                                     catch (RealTimeException rtex)
                                                                     {
                                                                         this.SetAsCompleted(rtex, false);
                                                                         return;
                                                                     }
                                                                     lock (_channel._syncRoot)
                                                                     {
                                                                         _channel._listOfParticipantsOutsideTheMix.Add(_participantEndpoint);
                                                                     }

                                                                     OutgoingAudioRoute outgoingRoute = new OutgoingAudioRoute(_participantEndpoint);
                                                                     outgoingRoute.Operation = RouteUpdateOperation.Add;
                                                                     IncomingAudioRoute incomingRoute = new IncomingAudioRoute(_participantEndpoint);
                                                                     incomingRoute.IsDtmfEnabled = true;
                                                                     incomingRoute.Operation = RouteUpdateOperation.Add;

                                                                     try
                                                                     {
                                                                         call.AudioVideoMcuRouting.BeginUpdateAudioRoutes(new List<OutgoingAudioRoute>() { outgoingRoute },
                                                                                                                          new List<IncomingAudioRoute>() { incomingRoute },
                                                                                                                          upar =>
                                                                                                                          {
                                                                                                                              AudioVideoMcuRouting mcuRouting = upar.AsyncState as AudioVideoMcuRouting;
                                                                                                                              try
                                                                                                                              {
                                                                                                                                  mcuRouting.EndUpdateAudioRoutes(upar);
                                                                                                                                  this.SetAsCompleted(null, false);
                                                                                                                              }
                                                                                                                              catch (RealTimeException rtex)
                                                                                                                              {
                                                                                                                                  this.SetAsCompleted(rtex, false);
                                                                                                                              }

                                                                                                                          },
                                                                                                                          call.AudioVideoMcuRouting);
                                                                     }
                                                                     catch (InvalidOperationException ivoex)
                                                                     {
                                                                         this.SetAsCompleted(new OperationFailureException( "AcdServiceChannel failed begin updating audio routes",ivoex), false);
                                                                     }


                                                                 }, null);
                        }
                        catch (InvalidOperationException ivoex)
                        {
                            this.SetAsCompleted(ivoex, false);
                        }
                    }
                    else
                    {
                        OutgoingAudioRoute outgoingRoute = new OutgoingAudioRoute(_participantEndpoint);
                        outgoingRoute.Operation = RouteUpdateOperation.Add;
                        IncomingAudioRoute incomingRoute = new IncomingAudioRoute(_participantEndpoint);
                        incomingRoute.IsDtmfEnabled = true;
                        incomingRoute.Operation = RouteUpdateOperation.Add;

                        try
                        {
                            call.AudioVideoMcuRouting.BeginUpdateAudioRoutes(new List<OutgoingAudioRoute>() { outgoingRoute },
                                                                             new List<IncomingAudioRoute>() { incomingRoute },
                                                                             upar =>
                                                                             {
                                                                                 AudioVideoMcuRouting mcuRouting = upar.AsyncState as AudioVideoMcuRouting;
                                                                                 try
                                                                                 {
                                                                                     mcuRouting.EndUpdateAudioRoutes(upar);
                                                                                     this.SetAsCompleted(null, false);
                                                                                 }
                                                                                 catch (RealTimeException rtex)
                                                                                 {
                                                                                     this.SetAsCompleted(rtex, false);
                                                                                 }

                                                                             },
                                                                             call.AudioVideoMcuRouting);
                        }
                        catch (InvalidOperationException ivoex)
                        {
                            this.SetAsCompleted(new OperationFailureException("AcdServiceChannel failed begin updating the audio routes" ,ivoex), false);
                        }
                    }
                }
                else
                {
                    this.SetAsCompleted(new OperationFailureException("ServiceChannel cannot establish a private audio channel as the call is not of the correct type"), false);
                }
            }

        }
        #endregion

        #region StartSilentMonitoring
        private class StartSilentMonitoringAsyncResult : AsyncResultNoResult
        {
            private AcdServiceChannel _channel;

            internal StartSilentMonitoringAsyncResult(AcdServiceChannel channel, AsyncCallback userCallBack, object state)
                : base(userCallBack, state)
            {
                _channel = channel;
            }


            //Silent monitoring Audio Route Management
            internal void Process()
            {
                if (_channel.MediaType == Microsoft.Rtc.Collaboration.MediaType.Audio)
                {
                    AudioVideoCall call = _channel.Call as AudioVideoCall;
                    AudioVideoMcuSession avMcu = _channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession;

                    call.StateChanged += this.OnMonitoringEnding;
                    avMcu.ParticipantEndpointAttendanceChanged += this.HandleSilentMonitoringAudioRouteAddition;

                    //Look whether participants in the conference are visible and start monitoring them
                    Collection<ParticipantEndpoint> participantEndpoints = avMcu.GetRemoteParticipantEndpoints();
                    List<ParticipantEndpoint> listOfEndpoints = new List<ParticipantEndpoint>(participantEndpoints);
                    
                    //Create a list of incoming audio routes to silently monitor the visible participants of the conference
                    List<IncomingAudioRoute> listOfNewMonitoringRoutes = new List<IncomingAudioRoute>();

                    listOfEndpoints.ForEach(endpoint =>
                    {
                        if (endpoint.Participant.RosterVisibility == ConferencingRosterVisibility.Visible)
                        {
                            IncomingAudioRoute incomingRoute = new IncomingAudioRoute(endpoint);
                            incomingRoute.IsDtmfEnabled = true;
                            incomingRoute.Operation = RouteUpdateOperation.Add;
                            listOfNewMonitoringRoutes.Add(incomingRoute);

                        }});

                    if (listOfNewMonitoringRoutes.Count == 0)
                    {
                        this.SetAsCompleted(null, false);
                        return;
                    }
                    
                    // Apply the routes to the service channel
                    try
                    {
                        call.AudioVideoMcuRouting.BeginUpdateAudioRoutes(null,
                                                                         listOfNewMonitoringRoutes,
                        ar =>
                        {
                            try
                            {
                                AudioVideoMcuRouting mcuRouting = ar.AsyncState as AudioVideoMcuRouting;
                                mcuRouting.EndUpdateAudioRoutes(ar);
                                this.SetAsCompleted(null, false);
                            }
                            catch (RealTimeException rtex)
                            {
                                this.SetAsCompleted(rtex, false);
                            }

                        },
                                                                         call.AudioVideoMcuRouting);

                    }
                    catch (InvalidOperationException ivoex)
                    {
                        this.SetAsCompleted(new OperationFailureException("AcdServiceChannel failed begin updating the audio routes", ivoex), false);
                    }
                }
                else
                { 
                    this.SetAsCompleted(new NotImplementedException("AcdServiceChannel did not implement this functionality at this time for this modality"), false);                
                }
            }

            private void OnMonitoringEnding(object sender, CallStateChangedEventArgs args)
            {
            
               if (args.State == CallState.Terminated)
               {
                   AudioVideoMcuSession avMcu = _channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession;

                   avMcu.ParticipantEndpointAttendanceChanged -= this.HandleSilentMonitoringAudioRouteAddition;
               
               }
              
            }

            private void HandleSilentMonitoringAudioRouteAddition(object sender, ParticipantEndpointAttendanceChangedEventArgs<AudioVideoMcuParticipantEndpointProperties> args)
            {
        
                AudioVideoCall call = _channel._call as AudioVideoCall;

                Collection<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>> participantEndpointKeyValues = args.Joined;

                List<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>> listOfEndpoints = new List<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>>(participantEndpointKeyValues);

                List<IncomingAudioRoute> listOfNewMonitoringRoutes = new List<IncomingAudioRoute>();

                listOfEndpoints.ForEach(endpoint =>
                {
                    if (endpoint.Key.Participant.RosterVisibility == ConferencingRosterVisibility.Visible)
                    {

                        IncomingAudioRoute incomingRoute = new IncomingAudioRoute(endpoint.Key);
                        incomingRoute.IsDtmfEnabled = true;
                        incomingRoute.Operation = RouteUpdateOperation.Add;
                        listOfNewMonitoringRoutes.Add(incomingRoute);

                    }
                });

                if (listOfNewMonitoringRoutes.Count == 0)
                    return;

                try
                {
                    call.AudioVideoMcuRouting.BeginUpdateAudioRoutes(null,
                                                                     listOfNewMonitoringRoutes,
                                                                     ar =>
                                                                     {
                                                                         try
                                                                         {
                                                                             AudioVideoMcuRouting mcuRouting = ar.AsyncState as AudioVideoMcuRouting;
                                                                             mcuRouting.EndUpdateAudioRoutes(ar);
                                                                         }
                                                                         catch (RealTimeException)
                                                                         {
                                                                            // TODO: write a log entry
                                                                         }

                                                                     },
                                                                     call.AudioVideoMcuRouting);

                }
                catch (InvalidOperationException)
                {
                    // TODO: Write a log entry
                }            
            }
        }

        #endregion


        #region BargeInAsyncResult

        private class BargeInAsyncResult : AsyncResultNoResult
        {
            AcdLogger _logger;
            AcdServiceChannel _channel;

            internal BargeInAsyncResult(AcdServiceChannel channel, AcdLogger logger, AsyncCallback userCallback, object state)
            :base(userCallback, state)
            {
                _logger = logger;
                _channel = channel;

            }

            private void HandleBargeInAudioRouteAddition(object sender, ParticipantEndpointAttendanceChangedEventArgs<AudioVideoMcuParticipantEndpointProperties> args)
            {

                AudioVideoCall call = _channel._call as AudioVideoCall;
                
                Collection<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>> participantEndpointKeyValues = args.Joined;

                List<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>> listOfEndpoints = new List<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>>(participantEndpointKeyValues);

                List<OutgoingAudioRoute> newOutgoingRoutes = new List<OutgoingAudioRoute>();

                listOfEndpoints.ForEach(endpoint =>
                {
                    OutgoingAudioRoute outgoingRoute = new OutgoingAudioRoute(endpoint.Key);
                    outgoingRoute.Operation = RouteUpdateOperation.Add;
                    newOutgoingRoutes.Add(outgoingRoute);
                });

                if (newOutgoingRoutes.Count == 0)
                    return;

                try
                {
                    call.AudioVideoMcuRouting.BeginUpdateAudioRoutes(newOutgoingRoutes,
                                                                     null,
                                                                     ar =>
                                                                     {
                                                                         try
                                                                         {
                                                                             AudioVideoMcuRouting mcuRouting = ar.AsyncState as AudioVideoMcuRouting;
                                                                             mcuRouting.EndUpdateAudioRoutes(ar);
                                                                         }
                                                                         catch (RealTimeException)
                                                                         {
                                                                             // TODO: write a log entry
                                                                         }

                                                                     },
                                                                     call.AudioVideoMcuRouting);

                }
                catch (InvalidOperationException)
                {
                    // TODO: Write a log entry
                }
            }


            internal void Process()
            {
                if (_channel.MediaType == Microsoft.Rtc.Collaboration.MediaType.Audio)
                {
                    
                    AudioVideoMcuSession avMcu = _channel._anchor.Conversation.ConferenceSession.AudioVideoMcuSession;
                    avMcu.ParticipantEndpointAttendanceChanged += this.HandleBargeInAudioRouteAddition;

                    List<ParticipantEndpoint> listOfEndpoints = new List<ParticipantEndpoint>(avMcu.GetRemoteParticipantEndpoints());

                    List<OutgoingAudioRoute> routes = new List<OutgoingAudioRoute>();

                    listOfEndpoints.ForEach( pe =>
                    {
                        OutgoingAudioRoute route = new OutgoingAudioRoute(pe);
                        route.Operation = RouteUpdateOperation.Add;
                        routes.Add(route);
                    });

                    try
                    {
                        AudioVideoCall avCall = _channel._call as AudioVideoCall;

                        avCall.AudioVideoMcuRouting.BeginUpdateAudioRoutes(routes,
                                                                           null,
                        uar =>
                        {
                            try
                            {
                                avCall.AudioVideoMcuRouting.EndUpdateAudioRoutes(uar);
                                this.SetAsCompleted(null, false);
                            }
                            catch (RealTimeException rtex)
                            {
                                this.SetAsCompleted(rtex, false);
                                _channel._logger.Log("AcdServiceChannel failed to end update the audio routes for bargeing in", rtex);

                            }
                        },
                                                                           null);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        this.SetAsCompleted(new OperationFailureException("AcdServiceChannel failed begin updating the audio routes",ivoex), false);
                        _channel._logger.Log("AcdServiceChannel failed to start update the audio routes for bargeing in", ivoex);
                      
                    }
                
                }
           
            }
        
        }
        #endregion
    }
        internal enum ServiceChannelType { DialIn = 0, DialOut = 1 };
        internal enum ServiceChannelState { Idle = 0, Establishing = 1, Established = 2, Terminating = 3, Terminated = 4 };
        internal class ServiceChannelStateChangedEventArgs : EventArgs
        {
            private ServiceChannelState _previousState;
            private ServiceChannelState _newState;

            internal ServiceChannelStateChangedEventArgs(ServiceChannelState previousState, ServiceChannelState newState)
            {
                _previousState = previousState;
                _newState = newState;
            }

            internal ServiceChannelState PreviousState
            {
                get { return _previousState; }
            }

            internal ServiceChannelState NewState
            {
                get { return _newState; }
            }
        }
 }
