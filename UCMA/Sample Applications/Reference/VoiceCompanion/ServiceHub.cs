/*=====================================================================
  File:      ServiceHub.cs

  Summary:   Implements abstractions that provide conference services.
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.ConferenceManagement;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    public class ServiceHub : ComponentBase
    {
        #region private fields

        private readonly CustomerSession m_parent;
        private Conversation m_tcuConversation;
        private AudioVideoMcuSession m_avmcuSession;
        private RosterTrackingService m_customerTracker;
        private RosterTrackingService m_primaryTracker;
        private ServiceChannel m_primaryChannel;
        #endregion

        #region Public methods

        public ServiceHub(CustomerSession parent, RosterTrackingService tracker):base(parent.AppFrontEnd.AppPlatform)
        {
            m_parent = parent;
            m_customerTracker = tracker;
        }

        /// <summary>
        /// Gets the customer endpoint in the conference. Can be null.
        /// </summary>
        public ParticipantEndpoint CustomerEndpoint
        {
            get
            {
                ParticipantEndpoint endpoint = null;
                if (m_customerTracker != null)
                {
                    endpoint = m_customerTracker.TargetUriEndpoint;
                }
                return endpoint;
            }
        }

        /// <summary>
        /// Gets the customer session that corresponds to this service hub.
        /// </summary>
        public CustomerSession CustomerSession
        {
            get
            {
                return m_parent;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "This is a SIP uri.")]
        public string ConferenceUri
        {
            get
            {
                if (m_tcuConversation != null)
                {
                    return m_tcuConversation.ConferenceSession.ConferenceUri;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the primary service channel.
        /// </summary>
        public ServiceChannel PrimaryServiceChannel
        {
            get
            {
                return m_primaryChannel;
            }
        }

        /// <summary>
        /// Gets the convesation created by the service hub for the service channel.
        /// </summary>
        public Conversation Conversation
        {
            get
            {
                return m_tcuConversation;
            }
        }

        public IAsyncResult BeginRemoveCustomerFromDefaultRouting(int duration, AsyncCallback userCallback, object state)
        {
            if (this.CustomerEndpoint == null)
            {
                throw new InvalidOperationException("Customer is not in the AVMCU yet");
            }
            
            var options = new RemoveFromDefaultRoutingOptions();
            options.Duration = TimeSpan.FromMilliseconds(duration);

            return m_avmcuSession.BeginRemoveFromDefaultRouting(
                this.CustomerEndpoint,
                options,
                userCallback,
                state);

        }

        public void EndRemoveCustomerFromDefaultRouting(IAsyncResult result)
        {
            if (m_avmcuSession == null)
            {
                throw new InvalidOperationException("Invalid state");
            }

            m_avmcuSession.EndRemoveFromDefaultRouting(result);
        }

        public IAsyncResult BeginAddCustomerToDefaultRouting(AsyncCallback userCallback, object state)
        {
            if (this.CustomerEndpoint == null)
            {
                throw new InvalidOperationException("Customer is not in the AVMCU yet");
            }

            return m_avmcuSession.BeginAddToDefaultRouting(
                this.CustomerEndpoint,
                userCallback,
                state);
        }

        public void EndAddCustomerToDefaultRouting(IAsyncResult result)
        {
            if (m_avmcuSession == null)
            {
                throw new InvalidOperationException("Invalid state");
            }

            m_avmcuSession.EndAddToDefaultRouting(result);
        }

        #endregion

        #region Internal methods
        protected override void StartupCore()
        {
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.FailureCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.SuccessCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.AddTask(new AsyncTask(this.StartupScheduleConference));
            sequence.AddTask(new AsyncTask(this.StartupJoinConference));
            sequence.AddTask(new AsyncTask(this.StartupPrimaryChannel));
            sequence.Start();
        }

        protected override void ShutdownCore()
        {
            if (m_tcuConversation != null)
            {
                m_avmcuSession.ParticipantEndpointAttendanceChanged -= this.ParticipantEndpointAttendanceChanged;
                m_tcuConversation.BeginTerminate(
                    ar =>
                    {
                        m_tcuConversation.EndTerminate(ar);
                        this.CompleteShutdown();
                    }, null);
            }
            else
            {
                this.CompleteShutdown();
            }
        }

        public override void CompleteShutdown()
        {
            this.Logger.Log(Logger.LogLevel.Info, string.Format( 
                CultureInfo.InvariantCulture,
                "ServiceHub for customer {0} has been shutdown",
                m_parent.Customer.DisplayName));
            
            base.CompleteShutdown();
        }

        private class ConferenceActionResult : AsyncTaskResult
        {
            Conference m_conference;
            public ConferenceActionResult(Conference conference):base(null)
            {
                m_conference = conference;
            }

            public Conference Conference
            {
                get
                {
                    return m_conference;
                }
            }
        }
        
    
    
        private  void StartupScheduleConference(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    m_tcuConversation = new Conversation(m_parent.AppFrontEnd.Endpoint);
                    m_tcuConversation.ApplicationContext = this;

                    m_avmcuSession = m_tcuConversation.ConferenceSession.AudioVideoMcuSession;
                    m_avmcuSession.ParticipantEndpointAttendanceChanged += this.ParticipantEndpointAttendanceChanged;

                    ConferenceServices conferenceManagement = m_parent.AppFrontEnd.Endpoint.ConferenceServices;

                    //Create a conference to anchor the incoming customer call
                    ConferenceScheduleInformation conferenceScheduleInfo = new ConferenceScheduleInformation();
                    conferenceScheduleInfo.AutomaticLeaderAssignment = AutomaticLeaderAssignment.SameEnterprise;
                    conferenceScheduleInfo.LobbyBypass = LobbyBypass.EnabledForGatewayParticipants;
                    conferenceScheduleInfo.AccessLevel = ConferenceAccessLevel.SameEnterprise;
                    conferenceScheduleInfo.PhoneAccessEnabled = false;
                    conferenceScheduleInfo.AttendanceAnnouncementsStatus = AttendanceAnnouncementsStatus.Disabled;
                    conferenceScheduleInfo.Mcus.Add(new ConferenceMcuInformation(McuType.AudioVideo));

                    //schedule the conference
                    conferenceManagement.BeginScheduleConference(conferenceScheduleInfo,
                        delegate(IAsyncResult sch)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    Conference conference;
                                    conference = conferenceManagement.EndScheduleConference(sch);
                                    task.TaskResult = new ConferenceActionResult(conference); // Store so that next task can get it.
                                });
                        },
                        null);
                });
        }


        protected void StartupJoinConference(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    Conference conference = null;
                    AsyncTaskResult conferenceActionResult = task.PreviousActionResult;
                    // Normally, the previous task should be the one that scheduled the conference. But, play safe to allow other actions in between.
                    while (conferenceActionResult != null)
                    {
                        ConferenceActionResult conferenceResult = conferenceActionResult as ConferenceActionResult;
                        if (conferenceResult != null)
                        {
                            conference = conferenceResult.Conference;
                            break;
                        }
                        conferenceActionResult = conferenceActionResult.PreviousActionResult;
                    }
                    if (conference == null)
                    {
                        task.Complete(new InvalidOperationException("StartupConferenceJoin: Conference must be scheduled before conference join operation."));
                        return;
                    }
                    ConferenceJoinOptions options = new ConferenceJoinOptions();
                    options.JoinMode = JoinMode.TrustedParticipant;

                    m_tcuConversation.ConferenceSession.BeginJoin(
                        conference.ConferenceUri,
                        options,
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_tcuConversation.ConferenceSession.EndJoin(ar);
                                    this.Logger.Log(Logger.LogLevel.Verbose,
                                        String.Format("ServiceHub {0} joined TCU conference {1}.",
                                        this.CustomerSession.Customer.UserUri,
                                        m_tcuConversation.ConferenceSession.ConferenceUri));
                                });

                        }, 
                        null);
                });
        }

        private void StartupPrimaryChannel(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    m_primaryTracker = new RosterTrackingService(this.CustomerSession.AppFrontEnd, new TimeSpan(0, 0, 1));
                    m_primaryTracker.TargetUri = this.CustomerSession.AppFrontEnd.Endpoint.OwnerUri;
                    m_primaryChannel = new ServiceChannel(this, m_customerTracker);
                    m_primaryChannel.IsPrimaryServiceChannel = true;
                    m_primaryChannel.BeginStartup(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_primaryChannel.EndStartup(ar);
                                    //Logger.Log(Logger.LogLevel.Info, "Started primary service channel(TCU).");
                                });
                        },
                        null);
                });
        }
        #endregion

        #region private methods

        private void ParticipantEndpointAttendanceChanged(
            object sender,
            ParticipantEndpointAttendanceChangedEventArgs<AudioVideoMcuParticipantEndpointProperties> e)
        {
            if (m_customerTracker != null)
            {
                m_customerTracker.ParticipantEndpointAttendanceChanged(sender, e);
            }
            if (m_primaryTracker != null)
            {
                m_primaryTracker.ParticipantEndpointAttendanceChanged(sender, e);
            }
        }

        #endregion

    }


    public class ServiceChannel : ComponentBase
    {
        #region enum InteractivityMode

        public enum InteractionMode
        {
            None = 0x0,
            ListenSpeech,
            ListenSpeechAndDtmf,
            AnnounceSpeech,
            AnnounceSpeechAndDtmf
        }

        #endregion

        #region Private fields
        private readonly ServiceHub m_serviceHub;
        private AudioVideoCall m_serviceChannelCall;
        private RosterTrackingService m_tracker;
        private bool m_isPrimaryServiceChannel;
        #endregion

        #region Public methods

        public ServiceChannel(ServiceHub hub, RosterTrackingService targetUriTracker)
            : base(hub.CustomerSession.AppFrontEnd.AppPlatform)
        {
            m_serviceHub = hub;
            Debug.Assert(targetUriTracker != null);
            m_tracker = targetUriTracker;
        }

        /// <summary>
        /// Gets the endpoint in the conference of the target uri. Can be null.
        /// </summary>
        public ParticipantEndpoint TargetUriEndpoint
        {
            get
            {
                return m_tracker.TargetUriEndpoint;
            }
        }

        /// <summary>
        /// Gets the target uri for this service channel.
        /// </summary>
        public string TargetUri
        {
            get
            {
                return m_tracker.TargetUri;
            }
        }

        /// <summary>
        /// Gets or sets whether this channel is promary. Primary channel uses the identity of the application.
        /// </summary>
        /// <remarks>Non-Primary channle uses a generated identity. Primary channel may be needed for some trusted operations in the conference.</remarks>
        public bool IsPrimaryServiceChannel
        {
            get
            {
                return m_isPrimaryServiceChannel;
            }
            set
            {
                m_isPrimaryServiceChannel = value;
            }
        }

        public AudioVideoCall ServiceChannelCall
        {
            get { return m_serviceChannelCall; }
        }
        #endregion

        #region Internal methods

        internal void HandleIncomingMcuDialOut(AudioVideoCall incomingCall)
        {
            try
            {
                incomingCall.Decline();
            }
            catch (InvalidOperationException)
            {
            }
        }

        protected override void StartupCore()
        {
            AsyncTaskSequenceSerial startupActions = new AsyncTaskSequenceSerial(this);
            startupActions.Name = "StartupHub";
            startupActions.SuccessCompletionReportHandlerDelegate = this.CompleteStartup;
            startupActions.FailureCompletionReportHandlerDelegate = this.CompleteStartup;
            startupActions.AddTask(new AsyncTask(this.EstablishServiceChannel));
            startupActions.Start();
        }

        #endregion

        #region Private methods

        private void ServiceChannelCallStateChanged(object sender, CallStateChangedEventArgs e)
        {
            if (e.State == CallState.Terminating)
            {
                this.Shutdown();
            }
            else if (e.State == CallState.Terminated)
            {
                this.UnregisterServiceCallHandlers();
            }
        }

        private void EstablishServiceChannel(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    m_serviceChannelCall = new AudioVideoCall(m_serviceHub.Conversation);

                    var options = new AudioVideoCallEstablishOptions();
                    // Ee need to use generated user identity for the call as this is hidden participant 
                    // of the conference for service purpose. 
                    options.UseGeneratedIdentityForTrustedConference = !this.IsPrimaryServiceChannel;
                    if (!this.IsPrimaryServiceChannel)
                    {
                        this.RegisterServiceCallHandlers();
                        // Service call does not need to be in default mix of the conference. The purpose is to service a specific target user in the conference.
                        options.AudioVideoMcuDialInOptions.RemoveFromDefaultRouting = true;
                    }

                    m_serviceChannelCall.BeginEstablish(
                        options,
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                               delegate()
                               {
                                   m_serviceChannelCall.EndEstablish(ar);
                               });
                        }, 
                        null);
                });
        }

        /// <summary>
        /// Setup mode of interaction for the customer endpoint. This can be done only after the customer endpoint is seen.
        /// </summary>
        /// <param name="task">The task instance for this operation.</param>
        /// <remarks>To add announce but remove listening (or vice versa), two operations would be needed.</remarks>
        public void UpdateInteractiveMode(AsyncTask task, object state)
        {
            ArgumentTuple args = (ArgumentTuple)state;
            task.DoOneStep(
                delegate()
                {
                    InteractionMode interactiveMode = InteractionMode.None;
                    bool isAdd = false;
                    isAdd = (bool)args.One;
                    interactiveMode = (InteractionMode)args.Two; // Integer

                    var outgoingRoutes = new List<OutgoingAudioRoute>();
                    var incomingRoutes = new List<IncomingAudioRoute>();

                    InteractionMode announce = InteractionMode.AnnounceSpeech | InteractionMode.AnnounceSpeechAndDtmf;
                    InteractionMode listen = InteractionMode.ListenSpeech | InteractionMode.ListenSpeechAndDtmf;

                    if ((interactiveMode & announce) != 0)
                    {
                        OutgoingAudioRoute outRoute = new OutgoingAudioRoute(this.TargetUriEndpoint);
                        outRoute.IsDtmfEnabled = (interactiveMode & InteractionMode.AnnounceSpeechAndDtmf) != 0;
                        outRoute.Operation = isAdd ? RouteUpdateOperation.Add : RouteUpdateOperation.Remove;
                        outgoingRoutes.Add(outRoute);
                    }
                    if ((interactiveMode & listen) != 0)
                    {
                        IncomingAudioRoute inRoute = new IncomingAudioRoute(this.TargetUriEndpoint);
                        inRoute.IsDtmfEnabled = (interactiveMode & InteractionMode.ListenSpeechAndDtmf) != 0;
                        inRoute.Operation = isAdd ? RouteUpdateOperation.Add : RouteUpdateOperation.Remove;
                        incomingRoutes.Add(inRoute);
                    }

                    m_serviceChannelCall.AudioVideoMcuRouting.BeginUpdateAudioRoutes(
                        outgoingRoutes,
                        incomingRoutes,
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_serviceChannelCall.AudioVideoMcuRouting.EndUpdateAudioRoutes(ar);
                                });
                        }, 
                        null);
                });
        }

        protected override void ShutdownCore()
        {
            if (m_serviceChannelCall != null)
            {
                AsyncTaskSequence shutdownSequence = new AsyncTaskSequenceSerial(this);
                shutdownSequence.Name = "ShutdownHub";
                shutdownSequence.FailureCompletionReportHandlerDelegate = this.CompleteShutdown;
                shutdownSequence.SuccessCompletionReportHandlerDelegate = this.CompleteShutdown;
                shutdownSequence.AddTask(new AsyncTask(this.TerminateServiceChannelCall));
                shutdownSequence.Start();
            }
            else
            {
                this.CompleteShutdown(null);
            }
        }

        /// <summary>
        /// Terminates the service channel.
        /// </summary>
        /// <param name="task">The task for this operation.</param>
        private void TerminateServiceChannelCall(AsyncTask task, object state)
        {
            Exception exception = null;

            try
            {
                Logger.Log(Logger.LogLevel.Info, "Terminating service channel call.");
                m_serviceChannelCall.BeginTerminate(
                    ar =>
                    {
                        Exception ex = null;
                        try
                        {
                            m_serviceChannelCall.EndTerminate(ar);
                            Logger.Log(Logger.LogLevel.Info, "Terminated service channel call.");
                        }
                        catch (RealTimeException rte)
                        {
                            ex = rte;
                        }
                        finally
                        {
                            task.Complete(ex);
                        }
                    }, null);
            }
            catch (InvalidOperationException ioe)
            {
                exception = ioe;
            }
            finally
            {
                if (exception != null)
                {
                    task.Complete(exception);
                }
            }
        }

        public override void CompleteStartup(Exception exception)
        {
            base.CompleteStartup(exception);
            if (exception != null)
            {
                // If startup failed, we should shutdown automatically.
                this.BeginShutdown(
                    ar => this.EndShutdown(ar),
                    null);
            }
        }

        private void Shutdown()
        {
            this.BeginShutdown(ar => this.EndShutdown(ar), null);
        }

        private void RegisterServiceCallHandlers()
        {
            m_serviceChannelCall.StateChanged += this.ServiceChannelCallStateChanged;
        }

        private void UnregisterServiceCallHandlers()
        {
            m_serviceChannelCall.StateChanged -= this.ServiceChannelCallStateChanged;
        }
        #endregion
    }
}
