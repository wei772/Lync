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
using Microsoft.Rtc.Collaboration.ConferenceManagement;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;
using System.Threading;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
	internal class AcdMusicOnHoldServer
    {
        #region private fields
        private AcdAgentMatchMaker _matchMaker;
        private string _mohFilePath;
        private WmaFileSource _mohFileSource;
        private Player _mohPlayer;
        private List<AudioVideoCall> _listOfMohCalls = new List<AudioVideoCall>();
        private List<EstablishMoHChannelAsyncResult> _listOfPendingMohCallAsyncResults = new List<EstablishMoHChannelAsyncResult>();
        private AcdLogger _logger;
        private MusicOnHoldServerState _state;
        private List<ShutDownAsyncResult> _listOfShutDownAsyncResults = new List<ShutDownAsyncResult>();
        private object _syncRoot = new object();
        
        #endregion

        #region Internal Events
        internal event EventHandler<MusicOnHoldServerStateChangedEventArgs> StateChanged;
        #endregion

        #region Constructor
        internal AcdMusicOnHoldServer(AcdAgentMatchMaker matchMaker, string mohFilePath, AcdLogger logger)
        {
            _matchMaker = matchMaker;
            _mohFilePath = mohFilePath;
            _logger = logger;
            _mohFileSource = new WmaFileSource(mohFilePath);
            _mohPlayer = new Player();
            _mohPlayer.SetMode(PlayerMode.Manual);
            _mohPlayer.SetSource(_mohFileSource);
            this.UpdateState(MusicOnHoldServerState.Created);
        }
        #endregion

        #region Internal Properties

        Player MulticastPlayer
        {
          get {return _mohPlayer; }
        }
            
        #endregion

        #region Internal methods

        internal IAsyncResult BeginEstablishMohChannel(AcdServiceChannel channel, AsyncCallback userCallback, object state)
        {
            EstablishMoHChannelAsyncResult ar = new EstablishMoHChannelAsyncResult(this, channel, userCallback, state);

            lock (_syncRoot)
            {
                if (_state == MusicOnHoldServerState.Started)
                {
                    ThreadPool.QueueUserWorkItem((waitState) =>
                    {
                        var tempAr = waitState as EstablishMoHChannelAsyncResult;
                        tempAr.Process();
                    }, ar);
                }
                else
                {
                    throw new InvalidOperationException("AcdMusicOnHoldServer is in an invalid State to start a new Moh channel.");
                }
            }

            return ar;        
        }

        internal void EndEstablishMohChannel(IAsyncResult ar)
        {
            EstablishMoHChannelAsyncResult result = ar as EstablishMoHChannelAsyncResult;
            result.EndInvoke();        
        }

        internal IAsyncResult BeginTerminateMohChannel(AcdServiceChannel channel, AsyncCallback userCallback, object state)
        {
            TerminateMoHChannelAsyncResult ar = new TerminateMoHChannelAsyncResult(this, channel, userCallback, state);

            lock (_syncRoot)
            {
                if (_state == MusicOnHoldServerState.Started)
                {
                    ThreadPool.QueueUserWorkItem((waitState) =>
                    {
                        var tempAr = waitState as TerminateMoHChannelAsyncResult;
                        tempAr.Process();
                    }, ar);
                }
                else
                {
                    throw new InvalidOperationException("AcdMusicOnHoldServer is shutting down or in an invalid state to perform this action.");
                }
            }

            return ar;
        }

        internal void EndTerminateMohChannel(IAsyncResult ar)
        {
            TerminateMoHChannelAsyncResult result = ar as TerminateMoHChannelAsyncResult;
            result.EndInvoke();
        }

        internal IAsyncResult BeginStartUp(AsyncCallback userCallback, object state)
        {
            StartUpAsyncResult ar = new StartUpAsyncResult(this, userCallback, state);

            bool firstTime = false;
            lock (_syncRoot)
            {
                if (_state == MusicOnHoldServerState.Created)
                {
                    firstTime = true;
                    this.UpdateState(MusicOnHoldServerState.Starting);
                }
                else
                {
                    throw new InvalidOperationException("AcdMusicOnHoldServer instance is an invalid state.");
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

        internal IAsyncResult BeginShutDown(AsyncCallback userCallback, object state)
        {
            ShutDownAsyncResult ar = new ShutDownAsyncResult(this, userCallback, state);
            bool firstTime = false;
            lock (_syncRoot)
            {
                if (_state < MusicOnHoldServerState.Terminating)
                {
                    firstTime = true;
                    this.UpdateState(MusicOnHoldServerState.Terminating);
                }
                else if (_state == MusicOnHoldServerState.Terminating)
                {
                    _listOfShutDownAsyncResults.Add(ar);
                }
                else if (_state == MusicOnHoldServerState.Terminated)
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

        private void UpdateState(MusicOnHoldServerState newState)
        {
            MusicOnHoldServerState oldState = _state;
            _state = newState;

            EventHandler<MusicOnHoldServerStateChangedEventArgs> stateChanged = this.StateChanged;
            if (stateChanged != null)
            {
                stateChanged(this, new MusicOnHoldServerStateChangedEventArgs
                    (oldState, newState));
            }
        }

        private void CompleteGetMohCallAsyncResult(
            object sender, 
            CallReceivedEventArgs<AudioVideoCall> callReceivedArgs)
        {
            if (object.ReferenceEquals(callReceivedArgs.Call.Conversation.ApplicationContext, this))
            { 
                EstablishMoHChannelAsyncResult result = null;
                lock (_syncRoot)
                {
                    try
                    {
                        result = _listOfPendingMohCallAsyncResults.First<EstablishMoHChannelAsyncResult>();
                    }
                    catch (InvalidOperationException)
                    {
                        return;
                    }
                    _listOfPendingMohCallAsyncResults.Remove(result);    
                }

                AcdServiceChannel mohChannel= new AcdServiceChannel(result.Anchor, _logger);

                try
                {
                    object[] args = new object[4];
                    args[0] = mohChannel;
                    args[1] = result.ParticipantUri;
                    args[2] = callReceivedArgs;
                    args[3] = result;

                    mohChannel.BeginStartUp(callReceivedArgs.Call,
                                           ServiceChannelType.DialIn,
                                           McuMediaChannelStatus.None,
                                           this.StartUpMohChannelCompleted,
                                           args);
                }
                catch (InvalidOperationException ivoex)
                {
                    callReceivedArgs.Call.Decline();
                    result.SetAsCompleted(new OperationFailureException("AcdMusicOnHoldServer failed begin starting up the MOH channel", ivoex), false);
                }
            }
        }

        private void StartUpMohChannelCompleted(IAsyncResult result)
        {
            object[] args = result.AsyncState as object[];

            AcdServiceChannel musicChannel = args[0] as AcdServiceChannel;
            string participantUri = args[1] as String;
            CallReceivedEventArgs<AudioVideoCall> callReceivedArgs = args[2] as CallReceivedEventArgs<AudioVideoCall>;
            EstablishMoHChannelAsyncResult ear = args[3] as EstablishMoHChannelAsyncResult;

            try
            {
                musicChannel.EndStartUp(result);

                try
                {
                    musicChannel.BeginEstablishPrivateAudioChannel(participantUri,
                                                                   true /*outsideMix*/,
                                                                   EstablishPrivateAudioChannelCompleted,
                                                                   musicChannel);
                }
                catch (InvalidOperationException ivoex)
                {
                    musicChannel.BeginShutDown(dar =>
                    {
                        AcdServiceChannel channel = dar.AsyncState as AcdServiceChannel;
                        channel.EndShutDown(dar);
                    },
                    musicChannel);

                    ear.SetAsCompleted(new OperationFailureException("AcdMusicOnHoldServer failed begin establishing private audio channel", ivoex), false);
                }
            }
            catch (RealTimeException rtex)
            {
                callReceivedArgs.Call.Decline();
                ear.SetAsCompleted(rtex, false);
            }
        }

        private static void EstablishPrivateAudioChannelCompleted(
            IAsyncResult result)
        {
            AcdServiceChannel musicChannel = result.AsyncState as AcdServiceChannel;
            EstablishMoHChannelAsyncResult ear = result as EstablishMoHChannelAsyncResult;
            try
            {
                musicChannel.EndEstablishPrivateAudioChannel(result);
            }
            catch (RealTimeException rtex)
            {
                musicChannel.BeginShutDown(dar =>
                {
                    AcdServiceChannel channel = dar.AsyncState as AcdServiceChannel;
                    channel.EndShutDown(dar);
                },
                musicChannel);
                ear.SetAsCompleted(rtex, false);
            }
        }

        #endregion

        #region StartUpAsyncResult
        private class StartUpAsyncResult:AsyncResultNoResult
        {
          private AcdMusicOnHoldServer _mohServer;

          internal StartUpAsyncResult(AcdMusicOnHoldServer mohServer, AsyncCallback userCallback, object state): base (userCallback, state)
          {
            _mohServer = mohServer;
            _mohServer._mohPlayer.StateChanged += this.OnPlayerStateChanged;
          }

          internal void Process()
          {
              _mohServer._mohFileSource.BeginPrepareSource(
                    MediaSourceOpenMode.Buffered,
                    ar =>
                    {
                        WmaFileSource fileSource = ar.AsyncState as WmaFileSource;

                        try
                        {
                            fileSource.EndPrepareSource(ar);

                            _mohServer._mohPlayer.Start();
                            lock (_mohServer._syncRoot)
                            {
                                _mohServer.UpdateState(MusicOnHoldServerState.Started);
                            }
                            this.SetAsCompleted(null, false);
                        }
                        catch (OperationFailureException ex)
                        {
                            _mohServer.BeginShutDown(
                                sar =>
                                {
                                    AcdMusicOnHoldServer mohServer = sar.AsyncState as AcdMusicOnHoldServer;

                                    mohServer.EndShutDown(sar);

                                },
                                _mohServer);

                            this.SetAsCompleted(ex, false);
                        }
                    },
                    _mohServer._mohFileSource);                    
          }

          private void OnMohFeedCallStateChanged(object sender, CallStateChangedEventArgs args)
          {
              if (args.State == CallState.Terminated)
              {
                  AudioVideoCall call = sender as AudioVideoCall;

                  if (call.Flow != null)
                  {
                      if (call.Flow.Player != null)
                      {
                          call.Flow.Player.Stop();
                          call.AudioVideoFlowConfigurationRequested -= this.StartMusicOnHoldPlayback;
                          call.StateChanged -= this.OnMohFeedCallStateChanged;
                          call.Flow.Player.StateChanged -= this.OnPlayerStateChanged;
                          call.Flow.Player.DetachFlow(call.Flow);
                      }
                  }
              }
          }
          
          private void StartMusicOnHoldPlayback(object sender, AudioVideoFlowConfigurationRequestedEventArgs args)
          {
              _mohServer._mohPlayer.AttachFlow(args.Flow);
              _mohServer._mohPlayer.StateChanged += this.OnPlayerStateChanged;
              
              _mohServer._mohFileSource.BeginPrepareSource(
                    MediaSourceOpenMode.Buffered,
                    ar =>
                    {
                        WmaFileSource fileSource = ar.AsyncState as WmaFileSource;                                                              
                                                                
                        try
                        {
                            fileSource.EndPrepareSource(ar);

                            _mohServer._mohPlayer.Start();
                        }
                        catch (OperationFailureException ex)
                        {
                            _mohServer.BeginShutDown(
                                sar =>
                                    {
                                        AcdMusicOnHoldServer mohServer = sar.AsyncState as AcdMusicOnHoldServer;
                                                                                              
                                        mohServer.EndShutDown(sar);
                                    },
                                    _mohServer);
                                                            
                            this.SetAsCompleted(ex, false);
                        }
                    },
                    _mohServer._mohFileSource);          
          }

          private void OnPlayerStateChanged(object sender, PlayerStateChangedEventArgs args)
          {
             if (args.State== PlayerState.Stopped && args.TransitionReason == PlayerStateTransitionReason.PlayCompleted)
             {
                 if (  _mohServer._state != MusicOnHoldServerState.Terminating
                     && _mohServer._state != MusicOnHoldServerState.Terminated)
                 {
                     _mohServer._mohPlayer.Start();
                 }
             }            
          }
        }
        #endregion

        #region ShutDownAsyncResult
        private class ShutDownAsyncResult:AsyncResultNoResult
        {
            private AcdMusicOnHoldServer _mohServer;

            internal ShutDownAsyncResult(AcdMusicOnHoldServer mohServer, AsyncCallback userCallback, object state)
                : base(userCallback, state)
            {
                _mohServer = mohServer;
            }

            internal void Process()
            {
                _mohServer._mohPlayer.Stop();

                //close the source to release unmanaged resources
                _mohServer._mohFileSource.Close();

                List<AudioVideoFlow> listOfAudioVideoFlows = new List<AudioVideoFlow>(_mohServer._mohPlayer.AudioVideoFlows);

                listOfAudioVideoFlows.ForEach(avFlow => {_mohServer._mohPlayer.DetachFlow(avFlow);});

                lock (_mohServer._syncRoot)
                {
                    _mohServer._listOfPendingMohCallAsyncResults.ForEach(sdar => {sdar.SetAsCompleted(null, false);});
                    _mohServer.UpdateState(MusicOnHoldServerState.Terminated);
                }
                this.SetAsCompleted(null, false);
            }
        }
        #endregion

        #region GetMohCallAsyncResult

        private class EstablishMoHChannelAsyncResult : AsyncResultNoResult
        {
            private AcdMusicOnHoldServer _mohServer;
            private AcdConferenceServicesAnchor _anchor;
            private string _participantUri;
            private AcdServiceChannel _channel;

            internal EstablishMoHChannelAsyncResult
                (AcdMusicOnHoldServer mohServer, 
                string participantUri,
                AcdConferenceServicesAnchor anchor, 
                AsyncCallback userCallback, 
                object state)
                : base(userCallback, state)
            {
                _mohServer = mohServer;
                _anchor = anchor;
                _participantUri = participantUri;
            
            }
            internal EstablishMoHChannelAsyncResult(                
                AcdMusicOnHoldServer mohServer, 
                AcdServiceChannel channel, 
                AsyncCallback userCallback, 
                object state)
                : base(userCallback, state)
            {
                _mohServer = mohServer;
                _channel = channel;
            }

            internal AcdConferenceServicesAnchor Anchor
            {
                get { return _anchor; }
            }

            internal string ParticipantUri
            {
                get { return _participantUri; }
            }

            internal void Process()
            {
                AudioVideoCall avCall = _channel.Call as AudioVideoCall;

                if (null != avCall)
                {
                    AudioVideoFlow flow = avCall.Flow;

                    if (null != flow)
                    {
                        try
                        {
                            _mohServer._mohPlayer.AttachFlow(flow);
                            this.SetAsCompleted(null, false);
                        }
                        catch (InvalidOperationException ivoex)
                        {
                            this.SetAsCompleted(new OperationFailureException("AcdMusicOnHoldServer failed attaching the player to the flow",ivoex), false);
                        }
                        catch (OperationFailureException ofex)
                        {
                            this.SetAsCompleted(ofex, false);
                        }
                    }
                    else
                    {
                        this.SetAsCompleted(new OperationFailureException("AcdMusicOnHoldServer cannot establish a MoH channel because the channel is not established"), false);
                    }                    
                }
                else
                {
                    this.SetAsCompleted(new OperationFailureException("AcdMusicOnHoldServer cannot establish a MoH channel because the call is not of the correct type"), false);
                }                          
            }


        }
        #endregion

        #region TerminateMoHChannelAsyncResult
        private class TerminateMoHChannelAsyncResult: AsyncResultNoResult
        {
            private AcdMusicOnHoldServer _mohServer;
            private AcdServiceChannel _mohChannel;

            internal TerminateMoHChannelAsyncResult(
                AcdMusicOnHoldServer mohServer, 
                AcdServiceChannel channel, 
                AsyncCallback userCallback, 
                object state): base (userCallback, state)
            {
                _mohServer = mohServer;
                _mohChannel = channel;
            }

            internal void Process()
            { 
                AudioVideoCall avCall = _mohChannel.Call as AudioVideoCall;

                if (null != avCall)
                {
                    if (null != avCall.Flow)
                    {
                        _mohServer._mohPlayer.DetachFlow(avCall.Flow);
                        this.SetAsCompleted(null, false);
                    }
                    else
                    {
                        throw new InvalidOperationException("AcdMusicOnHoldServer cannot detach the flow as the service channel does not have a flow");
                    }
                }
                else
                {
                    throw new InvalidOperationException("AcdMusicOnHoldServer cannot detach the flow as the service channel does not have a call");
                }            
            }            
        }

        #endregion
    }

    internal enum MusicOnHoldServerState 
    { 
        Created, 
        Starting, 
        Started, 
        Terminating, 
        Terminated 
    };

    internal class MusicOnHoldServerStateChangedEventArgs : EventArgs
    {
        private MusicOnHoldServerState _previousState;
        private MusicOnHoldServerState _newState;

        internal MusicOnHoldServerStateChangedEventArgs(MusicOnHoldServerState previousState, MusicOnHoldServerState newState)
        {
            _previousState = previousState;
            _newState = newState;
        }

        internal MusicOnHoldServerState PreviousState
        {
            get { return _previousState; }
        }

        internal MusicOnHoldServerState NewState
        {
            get { return _newState; }
        }
    }
}
