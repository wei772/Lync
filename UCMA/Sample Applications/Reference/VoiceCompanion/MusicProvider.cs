/*=====================================================================
  File:      MusicProvider.cs

  Summary:   Abstracts a music source.

***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/


using System;
using System.Diagnostics;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    #region MusicProvider

    public class MusicProvider : ComponentBase
    {

        #region private fields

        private string m_mohFilePath;
        private WmaFileSource m_mohFileSource;
        private Player m_mohPlayer;

        #endregion

        #region Constructor

        internal MusicProvider(AppPlatform platform, string mohFilePath):base(platform)
        {
            m_mohFilePath = mohFilePath;
        }

        #endregion

        public void StartMusic(AudioVideoCall audioVideoCall)
        {
            AudioVideoFlow flow = audioVideoCall.Flow;

            if (null != flow && flow.State == MediaFlowState.Active)
            {
                try
                {                                        
                    m_mohPlayer.AttachFlow(flow);
                }
                catch (InvalidOperationException ioe)
                {
                    this.Logger.Log(Logger.LogLevel.Error,ioe);
                }
                catch (OperationFailureException ofe)
                {
                    this.Logger.Log(Logger.LogLevel.Error,ofe);
                }
            }
        }

        public void StopMusic(AudioVideoCall audioVideoCall)
        {
            AudioVideoFlow flow = audioVideoCall.Flow;

            if (null != flow)
            {
                m_mohPlayer.DetachFlow(flow);
            }
        }

        protected override void StartupCore()
        {

            try
            {
                m_mohFileSource = new WmaFileSource(m_mohFilePath);
                m_mohPlayer = new Player();
                m_mohPlayer.SetMode(PlayerMode.Manual);
                m_mohPlayer.SetSource(m_mohFileSource);

                m_mohPlayer.StateChanged += this.OnPlayerStateChanged;
                
                m_mohFileSource.BeginPrepareSource(
                    MediaSourceOpenMode.Buffered,
                    ar =>
                    {
                        try
                        {
                            m_mohFileSource.EndPrepareSource(ar);
                            m_mohPlayer.Start();
                            this.CompleteStartup(null);
                        }
                        catch (InvalidOperationException ioe)
                        {
                            this.CompleteStartup(ioe);
                        }
                        catch (OperationFailureException rte)
                        {
                            this.CompleteStartup(rte);
                        }
                    },
                   null);
            }
            catch (InvalidOperationException ioe)
            {
                this.CompleteStartup(ioe);
            }
        }

        protected override void ShutdownCore()
        {
            if (m_mohPlayer != null)
            {
                m_mohPlayer.Stop();

                foreach (var flow in m_mohPlayer.AudioVideoFlows)
                {
                    m_mohPlayer.DetachFlow(flow);
                }

                m_mohPlayer.StateChanged -= this.OnPlayerStateChanged;

                m_mohPlayer = null;
            }

            this.CompleteShutdown();
        }

        public override void CompleteStartup(Exception exception)
        {
            if (exception != null)
            {
                this.BeginShutdown(ar => this.EndShutdown(ar), null);
            }

            base.CompleteStartup(exception);
        }

        private void OnPlayerStateChanged(object sender, PlayerStateChangedEventArgs args)
        {
            if (args.State == PlayerState.Stopped && args.TransitionReason == PlayerStateTransitionReason.PlayCompleted)
            {
                m_mohPlayer.SetSource(m_mohFileSource);

                try
                {
                    m_mohPlayer.Start();
                }
                catch (InvalidOperationException ioe)
                {
                    this.Logger.Log(Logger.LogLevel.Error,ioe);
                    this.BeginShutdown(ar => this.EndShutdown(ar), null);
                }
                catch (OperationFailureException ofe)
                {
                    this.Logger.Log(Logger.LogLevel.Error,ofe);
                    this.BeginShutdown(ar => this.EndShutdown(ar), null);
                }
            }

        }


    }

    #endregion
}
