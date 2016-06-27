/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

// .NET namespaces
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;

// UCMA namespaces
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.TrustedConferenceParticipant
{
    public class UCMASampleTrustedConferenceParticipant
    {
        #region Locals
        // The application endpoint that impersonates a user, creates the
        // conference and joins as a trusted participant
        private ApplicationEndpoint _appEndpoint;

        // Av call for the endpoint when it's impersonating a user
        private AudioVideoCall _impersonatingAvCall;

        // A helper that takes care of establishing the endpoint
        private UCMASampleHelper _helper;

        // The uri of the conference, populated when the conference is created.
        private string _conferenceUri;

        // Used to indicate if a user accepted the conference invitation or not.
        private bool _invitedParticipantAccepted = false;

        // The conversation that will be used to establish the AV calls
        // to communicate directly with conference attendees.
        private Conversation _trustedParticipantConversation;

        // A dictionary to map the uri of remote participants to the
        // Trusted Participant AV call used to communicate with them.
        private Dictionary<string, AudioVideoCall> _trustedParticipantCalls =
            new Dictionary<string, AudioVideoCall>();

        // A dictionary to map call Id's to the Participant that the call is
        // communicating with.
        private Dictionary<string, string> _trustedParticipantCallIDToParticipantUriStore
            = new Dictionary<string, string>();

        // Event to notify the main thread when the trusted participant has
        // created the ad-hoc conference.
        private AutoResetEvent _trustedParticipantConferenceJoinCompleted
            = new AutoResetEvent(false);

        // Event to signal that Audio Route updates for the Av call for the new
        // conference participant are completed.
        private AutoResetEvent _audioRouteUpdateForNewParticipantCallCompleted
            = new AutoResetEvent(false);

        // Used to signal when the invitation process is completed.
        private AutoResetEvent _conferenceInvitationCompleted = new AutoResetEvent(false);
        #endregion

        /// <summary>
        /// Instantiate and run the TrustedConferenceParticipant quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        static void Main(string[] args)
        {
            UCMASampleTrustedConferenceParticipant ucmaSampleTrustedConferenceParticipant
                = new UCMASampleTrustedConferenceParticipant();
            ucmaSampleTrustedConferenceParticipant.Run();
        }

        /// <summary>
        /// Retrieves the application configuration and begins running the
        /// sample.
        /// </summary>
        public void Run()
        {
            try
            {
                _helper = new UCMASampleHelper();
                _appEndpoint = _helper.CreateApplicationEndpoint("TrustedConferenceParticipant");

                if (_appEndpoint.State == LocalEndpointState.Established)
                {
                    Console.WriteLine("The Application Endpoint owned by URI {0}, is now established and "
                        + "registered.", _appEndpoint.OwnerUri);
                }
                else
                {
                    Console.WriteLine("The Application endpoint is not currently in the Established state, "
                        + "exiting...");
                    return;
                }

                // Impersonate a user and create an ad-hoc conference.
                ImpersonateAndCreateConference();

                // Have the application endpoint join the ad-hoc conference as a
                // trusted participant.
                JoinConferenceAsTrustedParticipant();

                // Start monitoring the AVMCU session for attendance changes.
                StartAVMCUSessionAttendanceMonitoring();

                int invitationCounter = 1;
                while (true)
                {
                    // Retrieve the uri of the user to be invited from the
                    // config file.
                    string prompt = "Please enter the uri of the user who should be sent an invitation to the"
                        + "conference (Enter to Skip) => ";

                    string invitationTargetUri = UCMASampleHelper.PromptUser(prompt, "InvitationTargetURI"
                        + invitationCounter);
                    if (!string.IsNullOrEmpty(invitationTargetUri))
                    {
                        InviteUserToConference(invitationTargetUri);
                        invitationCounter++;

                        if (_invitedParticipantAccepted)
                        {
                            // Wait for the invited participant to have the
                            // Trusted User establish an Av call and update the
                            // audio routes to communicate with them. This is
                            // purely so the logging for each invited user
                            // occurs in sequence for this sample.
                            Console.WriteLine("Waiting for the AudioRoute update on the Av Call for the new "
                                + "participant to complete.");
                            _audioRouteUpdateForNewParticipantCallCompleted.WaitOne();
                        }
                    }
                    else
                    {
                        Console.WriteLine("No invitation uri provided, skipping conference invitation.");
                        break;
                    }
                }

                Console.WriteLine(RetrieveConversationParticipantsProperties(
                    _impersonatingAvCall.Conversation));

                Console.Write("\n\n********************\n");
                Console.WriteLine("Press enter to exit.");
                Console.WriteLine("********************\n\n");
                Console.ReadLine();
            }
            finally
            {
                //Terminate the platform which would in turn terminate any
                // endpoints.
                Console.WriteLine("Shutting down the platform.");
                _helper.ShutdownPlatform();
            }
        }

        /// <summary>
        /// Starts monitoring the attendance changed event on the AVMCU session.
        /// </summary>
        private void StartAVMCUSessionAttendanceMonitoring()
        {
            // Monitor the Av Mcu session, when a new participant is detected,
            // add a new call to route audio to the participant and listen for
            // DTMF from the participant. Terminate the call when the
            // participant departs the conference.
            _trustedParticipantConversation.ConferenceSession.AudioVideoMcuSession
                .ParticipantEndpointAttendanceChanged += new EventHandler<
                    ParticipantEndpointAttendanceChangedEventArgs<
                    AudioVideoMcuParticipantEndpointProperties>>(
                    AudioVideoMcuSession_ParticipantEndpointAttendanceChanged);

        }

        /// <summary>
        /// Indicates what to do when a new attendee is detected and when an
        /// attendee departs.
        /// </summary>
        /// <param name="sender">The Av Mcu session raising the event.</param>
        /// <param name="e">The AV Mcu Participant endpoint attendance changed
        /// event arguments object.</param>
        private void AudioVideoMcuSession_ParticipantEndpointAttendanceChanged(object sender,
            ParticipantEndpointAttendanceChangedEventArgs<AudioVideoMcuParticipantEndpointProperties> e)
        {
            foreach (var joiningParticipant in e.Joined)
            {
                var joiningParticipantEndpoint = joiningParticipant.Key;

                // If this participant is hidden on the roster, and therefore a
                // trusted application, move onto the next joining participant.
                if (joiningParticipantEndpoint.Participant.RosterVisibility ==
                    ConferencingRosterVisibility.Hidden)
                {
                    continue;
                }

                if (!_trustedParticipantCalls.ContainsKey(joiningParticipant.Key.Uri))
                {
                    Console.WriteLine("Detected a new participant on the AVMCU Session with the displayname "
                        + "{0}.", joiningParticipant.Key.Participant.DisplayName);

                    EstablishAvCallAndAudioRouteForNewAttendee(joiningParticipant.Key);
                }
            }

            foreach (var departingParticipant in e.Left)
            {
                if (_trustedParticipantCalls.ContainsKey(departingParticipant.Key.Uri))
                {
                    Console.WriteLine("Detected a departing participant on the AVMCU Session with the "
                        + "displayname {0}.", departingParticipant.Key.Participant.DisplayName);

                    // Terminate the call that the trusted app has listening for
                    // DTMF from the user.
                    AudioVideoCall departingParticipantAvCall = _trustedParticipantCalls[departingParticipant
                        .Key.Uri];

                    if (CallState.Terminating != departingParticipantAvCall.State && CallState.Terminated
                        != departingParticipantAvCall.State)
                    {
                        departingParticipantAvCall.BeginTerminate(CallTerminationCompleted,
                            departingParticipantAvCall);
                    }

                    // Remove the call from the collection.
                    _trustedParticipantCalls.Remove(departingParticipant.Key.Uri);
                    _trustedParticipantCallIDToParticipantUriStore.Remove(departingParticipantAvCall.CallId);
                }
            }
        }

        private void CallTerminationCompleted(IAsyncResult result)
        {
            ((AudioVideoCall)result.AsyncState).EndTerminate(result);
        }

        /// <summary>
        /// This function creates an ad-hoc conference and joins as a trusted
        /// participant. Anytime a new participant to the conference is
        /// detected, a new AV call will be created to route audio to that user
        /// and receive DTMF from them. When a DTMF digit is detected, an
        /// utterance of the detected digit will be played back to that user
        /// such that they are the only ones that hear it.
        /// </summary>
        private void JoinConferenceAsTrustedParticipant()
        {
            Console.WriteLine("Joining the conference as a Trusted Participant.");

            //Create a new conversation for the application endpoint.
            _trustedParticipantConversation = new Conversation(_appEndpoint);

            ConferenceJoinOptions confJoinOptions = new ConferenceJoinOptions();

            // Set the app endpoint to join the conference as a trusted
            // participant which will result in it being hidden on the roster
            // as well as execute conference commands on behalf of other
            // participants.
            confJoinOptions.JoinMode = JoinMode.TrustedParticipant;

            //Join the conference
            _trustedParticipantConversation.ConferenceSession.BeginJoin(_conferenceUri, confJoinOptions,
                TrustedParticipantConferenceJoinCompleted, _trustedParticipantConversation.ConferenceSession);


            //Wait for the trusted participant to create and join the ad-hoc
            // conference.
            Console.WriteLine("Waiting for the conference join to complete.");
            _trustedParticipantConferenceJoinCompleted.WaitOne();
        }

        /// <summary>
        /// The Callback executed when the conference join operation completes
        /// for the trusted participant.
        /// </summary>
        /// <param name="result">The IAsyncResult of the operation.</param>
        private void TrustedParticipantConferenceJoinCompleted(IAsyncResult result)
        {
            try
            {
                var trustedParticipantConfSession = result.AsyncState as ConferenceSession;
                trustedParticipantConfSession.EndJoin(result);
                Console.WriteLine("The conference has been joined as a trusted participant.");
            }
            catch (OperationTimeoutException opTimeOutEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An OperationTimeoutException occured when ending the operation of joining "
                    + "the conference session: {0}", opTimeOutEx.ToString());
            }
            catch (ConferenceFailureException confFailEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A ConferenceFailureException occured when ending the operation of joining "
                    + "the conference session: {0}", confFailEx.ToString());
            }
            catch (FailureRequestException failRequestEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A FailureRequestException occured when ending the operation of joining "
                    + "the conference session: {0}", failRequestEx.ToString());
            }
            catch (OperationFailureException opFailEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An OperationFailureException occured when ending the operation of ending "
                    + "the operation of joining the conference session: {0}", opFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A RealTimeException occured when ending the operation of joining the "
                    + "conference session: {0}", realTimeEx.ToString());
            }
            finally
            {
                // Signal that the conference join has completed.
                _trustedParticipantConferenceJoinCompleted.Set();
            }

        }

        private void EstablishAvCallAndAudioRouteForNewAttendee(ParticipantEndpoint newAttendeeParticipantEndpoint)
        {
            AudioVideoCall newAttendeeCall = new AudioVideoCall(_trustedParticipantConversation);

            // Save the new Attendee Participant Endpoint in the Application
            // Context.
            newAttendeeCall.ApplicationContext = newAttendeeParticipantEndpoint;

            AudioVideoCallEstablishOptions avCallEstablishOptions = new AudioVideoCallEstablishOptions();

            // Remove the call from the default Mcu route because we will be
            // specifying custom routes after the call is established.
            avCallEstablishOptions.AudioVideoMcuDialInOptions.RemoveFromDefaultRouting = true;

            // When the Flow is active, add the tone handler
            newAttendeeCall.AudioVideoFlowConfigurationRequested += new EventHandler<
                AudioVideoFlowConfigurationRequestedEventArgs>(
                NewAttendeeCall_AudioVideoFlowConfigurationRequested);

            newAttendeeCall.BeginEstablish(avCallEstablishOptions, NewAttendeeCallEstablishCompleted,
                newAttendeeCall);

            // Add the call to the collection so it can be retrieved later.
            _trustedParticipantCalls.Add(newAttendeeParticipantEndpoint.Uri, newAttendeeCall);
        }

        private void NewAttendeeCallEstablishCompleted(IAsyncResult result)
        {
            var newParticipantCall = (AudioVideoCall)result.AsyncState;

            try
            {
                newParticipantCall.EndEstablish(result);
            }
            catch (FailureResponseException failureResEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A FailureResponseException occured when ending the establishment of an Av "
                    + "call for a new participant: {0}", failureResEx.ToString());
            }
            catch (OperationFailureException opFailEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An OperationFailureException occured when ending the establishment of an "
                    + "Av call for a new participant: {0}", opFailEx.ToString());
            }
            catch (OperationTimeoutException opTimeoutEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An OperationTimeoutException occured when ending the establishment of an "
                    + "Av call for a new participant: {0}",
                    opTimeoutEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A RealTimeException occured when ending the establishment of an Av call "
                    + "for a new participant: {0}",
                    realTimeEx.ToString());
            }

            // Retrieve the Participant endpoint from the call's Application
            // context
            var newAttendeeParticipantEndpoint = (ParticipantEndpoint)newParticipantCall.ApplicationContext;

            Console.WriteLine("ID of the new call = {0}", newParticipantCall.CallId);
            _trustedParticipantCallIDToParticipantUriStore.Add(newParticipantCall.CallId,
                newAttendeeParticipantEndpoint.Participant.Uri);
            Console.WriteLine("Av Call Established to communicate with the new conference participant.");

            //Create a new outgoing route which dictates who the app is speaking
            // to via this call.
            OutgoingAudioRoute newOutgoingRoute = new OutgoingAudioRoute(newAttendeeParticipantEndpoint);

            // We're not going to send DTMF to the participant so disable DTMF
            newOutgoingRoute.IsDtmfEnabled = false;

            //Add this outbound route.
            newOutgoingRoute.Operation = RouteUpdateOperation.Add;

            //Create a new incoming route which dictates who the app is
            // listening to via this call.
            IncomingAudioRoute newIncomingRoute = new IncomingAudioRoute(newAttendeeParticipantEndpoint);

            // The app is listening for DTMF input from the user so enable it on
            // the incoming route.
            newIncomingRoute.IsDtmfEnabled = true;

            //Add this incoming route.
            newIncomingRoute.Operation = RouteUpdateOperation.Add;
            Console.WriteLine("Updating the Audio Routes to communicate with the new conference participant: "
                + "{0}", newAttendeeParticipantEndpoint.Participant.DisplayName);

            newParticipantCall.AudioVideoMcuRouting.BeginUpdateAudioRoutes(
                new List<OutgoingAudioRoute> { newOutgoingRoute },
                new List<IncomingAudioRoute> { newIncomingRoute }, UpdateAudioRoutesCompleted,
                newParticipantCall);
        }

        private void UpdateAudioRoutesCompleted(IAsyncResult mcuRouteUpdateResult)
        {
            var mcuRouteUpdateCall = (AudioVideoCall)mcuRouteUpdateResult.AsyncState;
            try
            {
                mcuRouteUpdateCall.AudioVideoMcuRouting.EndUpdateAudioRoutes(mcuRouteUpdateResult);
                Console.WriteLine("Audio Routes update has completed.");
            }
            catch (ConferenceFailureException confFailEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A ConferenceFailureException occured when ending the audio route update "
                    + "operation: {0}", confFailEx.ToString());
            }
            catch (OperationTimeoutException opTimeoutEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An OperationTimeoutException occured when ending the audio route update "
                    + "operation: {0}", opTimeoutEx.ToString());
            }
            catch (RealTimeException rtEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A RealTimeException occured when ending the audio route update operation:"
                    + "{0}", rtEx.ToString());
            }
            finally
            {
                // Indicate that the audio route updates are completed for the
                // call.
                _audioRouteUpdateForNewParticipantCallCompleted.Set();
            }
        }

        private void NewAttendeeCall_AudioVideoFlowConfigurationRequested(object sender,
            AudioVideoFlowConfigurationRequestedEventArgs e)
        {
            // Attach a Wma file player to the flow. It will be used to play
            // back a WMA file to the participant matching the detected DTMF
            // digit that the user pressed in the ToneReceived EventHandler.
            Player wmaFilePlayer = new Player();
            wmaFilePlayer.StateChanged += new EventHandler<PlayerStateChangedEventArgs>(
                WmaFilePlayer_StateChanged);

            var currentAvFlow = ((AudioVideoCall)sender).Flow;

            wmaFilePlayer.AttachFlow(currentAvFlow);

            ToneController currentToneController = new ToneController();

            // Add a handler for the tonereceived event so that the app can play
            // a wave file back to the user echoing the digit received.ss
            currentToneController.ToneReceived += new EventHandler<
                ToneControllerEventArgs>(ToneController_ToneReceived);

            // Attach the flow to the tone controller.
            currentToneController.AttachFlow(currentAvFlow);
        }

        private void WmaFilePlayer_StateChanged(object sender, PlayerStateChangedEventArgs e)
        {
            Console.WriteLine("A player changed state from {0} to {1} because {2}.",
                e.PreviousState,
                e.State,
                e.TransitionReason.ToString());

            if (e.State == PlayerState.Stopped)
            {
                //Detach the source from the player.
                Player currentPlayer = (Player)sender;
                if (null != currentPlayer.Source)
                {
                    currentPlayer.Source.Close();
                }

                currentPlayer.RemoveSource();
            }
        }

        private void ToneController_ToneReceived(object sender, ToneControllerEventArgs e)
        {
            Console.WriteLine("Detected tone {0}, from the user with uri={1}.",
                e.Tone,
                _trustedParticipantCallIDToParticipantUriStore[((ToneController)sender).AudioVideoFlow.Call
                .CallId]);

            string wmaFileName = string.Empty;
            if (0 <= e.Tone && 9 >= e.Tone)
            {
                //retrieve the name of the wma file from the config file.
                wmaFileName = ConfigurationManager.AppSettings[string.Format("DTMF{0}WmaFile", e.Tone)];
            }
            else
            {
                if (e.Tone == (int)ToneId.Pound)
                {
                    wmaFileName = ConfigurationManager.AppSettings["DTMFPoundWmaFile"];
                }
                if (e.Tone == (int)ToneId.Star)
                {
                    wmaFileName = ConfigurationManager.AppSettings["DTMFStarWmaFile"];
                }
            }

            // Play the message to the user corresponding to the detected
            // keypress.
            PlayWmaFileAudio((sender as ToneController).AudioVideoFlow, wmaFileName);
        }

        /// <summary>
        /// Plays a WMA file using the specified Audio Video Flow.
        /// </summary>
        /// <param name="flowToPlayUsing">
        /// The flow to use when playing the file.
        /// </param>
        /// <param name="fileToPlay">
        /// The path to the wma file to be played.
        /// </param>
        private void PlayWmaFileAudio(AudioVideoFlow flowToPlayUsing, string fileToPlay)
        {
            // Set the media source that the player will use.
            WmaFileSource wmaSource = new WmaFileSource(fileToPlay);

            MediaSourceAndAvFlowContainer sourceAndFlowContainer = new MediaSourceAndAvFlowContainer(
                wmaSource, flowToPlayUsing);
            try
            {
                wmaSource.BeginPrepareSource(MediaSourceOpenMode.Unbuffered,
                    WmaSourcePreparationCompleted,
                    sourceAndFlowContainer);
            }
            catch (ArgumentOutOfRangeException argOOREx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An ArgumentOutOfRangeException occured when preparing the media source: "
                    + "{0}", argOOREx.ToString());
            }
        }

        private void WmaSourcePreparationCompleted(IAsyncResult prepareSourceResult)
        {
            var sourceAndFlowContainer = (MediaSourceAndAvFlowContainer)prepareSourceResult.AsyncState;

            sourceAndFlowContainer.StoredMediaSource.EndPrepareSource(prepareSourceResult);
            sourceAndFlowContainer.StoredAvFlow.Player.RemoveSource();
            sourceAndFlowContainer.StoredAvFlow.Player.SetSource(sourceAndFlowContainer.StoredMediaSource);
            try
            {
                sourceAndFlowContainer.StoredAvFlow.Player.Start();
            }
            catch (OperationFailureException opFailEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An OperationFailureException occured when starting media playback: {0}",
                    opFailEx.ToString());
            }

        }

        private void ImpersonateAndCreateConference()
        {
            AutoResetEvent conferenceJoinCompleted = new AutoResetEvent(false);

            //Create a new conversation for the application endpoint.
            Conversation impersonatingConversation = new Conversation(_appEndpoint);

            //Impersonate the user.
            impersonatingConversation.Impersonate("sip:JohnDoe@Contoso.com", null /*Phone Uri*/, "John Doe");

            //Join the ad hoc conference using its uri.
            var conferenceJoinResult = impersonatingConversation.ConferenceSession.BeginJoin(
                new ConferenceJoinOptions(), ConferenceSessionJoinCompleted, impersonatingConversation);

            // Wait for the conference join to complete before establishing the
            // AvCall
            Console.WriteLine("Waiting for the conference join to complete.");
            conferenceJoinResult.AsyncWaitHandle.WaitOne();

            _impersonatingAvCall = EstablishAvCallToConference(impersonatingConversation);
        }

        private void ConferenceSessionJoinCompleted(IAsyncResult result)
        {
            try
            {
                var impersonatingConversation = (Conversation)result.AsyncState;
                impersonatingConversation.ConferenceSession.EndJoin(result);
                _conferenceUri = impersonatingConversation.ConferenceSession.ConferenceUri;
                //Save the uri of the conference so other users can join.
                Console.WriteLine("The Uri of the conference is " + _conferenceUri);

            }
            catch (OperationTimeoutException opTimeOutEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An OperationTimeoutException occured when joining the conference session: "
                    + "{0}", opTimeOutEx.ToString());
            }
            catch (ConferenceFailureException confFailEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A ConferenceFailureException occured when joining the conference session: "
                    + "{0}", confFailEx.ToString());
            }
            catch (FailureRequestException failRequestEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A FailureRequestException occured when joining the conference session: "
                    + "{0}", failRequestEx.ToString());
            }
            catch (OperationFailureException opFailEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An OperationFailureException occured when joining the conference session: "
                    + "{0}", opFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A RealTimeException occured when joining the conference session: {0}",
                    realTimeEx.ToString());
            }
        }

        /// <summary>
        /// Establishes an Av call to the conferencesession on the supplied
        /// conversation.
        /// </summary>
        /// <param name="conversationToEstablishImCallOn">
        /// The conversation to establish the Av call on.
        /// </param>
        private AudioVideoCall EstablishAvCallToConference(Conversation conversationToEstablishImCallOn)
        {
            Console.WriteLine("Establishing an Av call to the conference.");

            AutoResetEvent _avFlowCreated = new AutoResetEvent(false);
            var newAvCall = new AudioVideoCall(conversationToEstablishImCallOn);

            //Calling begin Establish with no parameters will establish an Av
            // call to the conference session on the conversation.
            var establishCallResult = newAvCall.BeginEstablish(ConferenceAvCallEstablishCompleted, newAvCall);

            Console.WriteLine("Waiting for the AvCall to be established to the conference.");
            establishCallResult.AsyncWaitHandle.WaitOne();

            return newAvCall;
        }

        private void ConferenceAvCallEstablishCompleted(IAsyncResult result)
        {
            try
            {
                ((AudioVideoCall)result.AsyncState).EndEstablish(result);
                Console.WriteLine("An Av call has been established to the conference.");
            }
            catch (FailureResponseException failureResEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A FailureResponseException occured when ending the establishment of an Av "
                    + "call to the conference session: {0}", failureResEx.ToString());
            }
            catch (OperationFailureException opFailEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An OperationFailureException occured when ending the establishment of an "
                    + "Av call to the conference session: {0}", opFailEx.ToString());
            }
            catch (OperationTimeoutException opTimeoutEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An OperationTimeoutException occured when ending the establishment of an "
                    + "Av call to the conference session: {0}", opTimeoutEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A RealTimeException occured when ending the establishment of an Av call "
                    + "to the conference session: {0}", realTimeEx.ToString());
            }
        }

        /// <summary>
        /// Sends a conference invitation to the specified uri.
        /// </summary>
        /// <param name="invitationTargetUri">The uri that the invitation should
        /// be sent to.</param>
        private void InviteUserToConference(string invitationTargetUri)
        {
            ConferenceInvitation confInvitation = new ConferenceInvitation(_impersonatingAvCall.Conversation);
            ConferenceInvitationDeliverOptions deliverOptions = new ConferenceInvitationDeliverOptions();

            const string sipPrefix = "sip:";
            if (!invitationTargetUri.Trim().StartsWith(sipPrefix, StringComparison.OrdinalIgnoreCase))
            {
                invitationTargetUri = sipPrefix + invitationTargetUri.Trim();
            }

            Console.WriteLine("Inviting {0} to the conference.", invitationTargetUri);
            _invitedParticipantAccepted = false;
            var confInvitationResult = confInvitation.BeginDeliver(invitationTargetUri,
                deliverOptions,
                ConfInvitationCompleted,
                confInvitation);

            Console.WriteLine("Waiting for the conference invitation to complete.");
            _conferenceInvitationCompleted.WaitOne();

            Console.WriteLine("Finished inviting {0} to the conference.", invitationTargetUri);
            Console.WriteLine();
        }

        private void ConfInvitationCompleted(IAsyncResult result)
        {
            try
            {
                var invitationResponse = ((ConferenceInvitation)result.AsyncState).EndDeliver(result);
                Console.WriteLine("The invitation's response text is {0}", invitationResponse.ResponseText);

                _invitedParticipantAccepted = true;
            }
            catch (FailureResponseException failureResEx)
            {
                // TODO (Left to the reader): Write actual handling code for
                // the occurrence.
                Console.WriteLine("A FailureResponseException occured when ending the delivery of a "
                    + "conference invitation: {0}", failureResEx.ToString());
            }
            catch (OperationFailureException opFailEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An OperationFailureException occured when ending the delivery of a "
                    + "conference invitation: {0}", opFailEx.ToString());
            }
            catch (OperationTimeoutException opTimeoutEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An OperationTimeoutException occured when ending the delivery of a "
                    + "conference invitation: {0}", opTimeoutEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("A RealTimeException occured when ending the delivery of a conference "
                    + "invitation: {0}", realTimeEx.ToString());
            }
            finally
            {
                _conferenceInvitationCompleted.Set();
            }

        }

        /// <summary>
        /// Retrieves the properties of the local and remote participants of the
        /// supplied conversation.
        /// </summary>
        /// <param name="currentConversation">The conversation whose
        /// participant's properties are to be retrieved.</param>
        /// <returns>A string of the retrieved properties.</returns>
        private string RetrieveConversationParticipantsProperties(Conversation currentConversation)
        {
            StringBuilder detailsSB = new StringBuilder();
            detailsSB.Append(RetrieveParticipantProperties(currentConversation.LocalParticipant));

            foreach (var remoteParticipant in currentConversation.RemoteParticipants)
            {
                detailsSB.Append(RetrieveParticipantProperties(remoteParticipant));
            }
            return detailsSB.ToString();
        }

        /// <summary>
        /// Retrieves the properties for the specified Conversation Participant.
        /// </summary>
        /// <param name="currentParticipant">The participant who's properties
        /// are to be retrieved.</param>
        /// <returns>A string of the retrieved properties.</returns>
        private string RetrieveParticipantProperties(ConversationParticipant currentParticipant)
        {
            StringBuilder participantPropertiesSB = new StringBuilder();
            participantPropertiesSB.AppendLine("Properties for the participant with Uri = "
                + currentParticipant.Uri);
            if (0 < currentParticipant.GetActiveMediaTypes().Count)
            {
                participantPropertiesSB.Append("\tActiveMediaTypes = ");
                Collection<string> activeMediaTypes = currentParticipant.GetActiveMediaTypes();
                foreach (var activeMediaType in activeMediaTypes)
                {
                    participantPropertiesSB.Append(string.Format("'{0}' ", activeMediaType));
                }
                participantPropertiesSB.AppendLine();
            }

            if (!string.IsNullOrEmpty(currentParticipant.DisplayName))
            {
                participantPropertiesSB.AppendLine("\t DisplayName = " + currentParticipant.DisplayName);
            }

            if (!string.IsNullOrEmpty(currentParticipant.OtherPhoneUri))
            {
                participantPropertiesSB.AppendLine("\t OtherPhoneUri = " + currentParticipant.OtherPhoneUri);
            }

            if (!string.IsNullOrEmpty(currentParticipant.PhoneUri))
            {
                participantPropertiesSB.AppendLine("\t PhoneUri = " + currentParticipant.PhoneUri);
            }
            participantPropertiesSB.AppendLine("\t Role = " + currentParticipant.Role);
            participantPropertiesSB.AppendLine("\t RosterVisibility = "
                + currentParticipant.RosterVisibility);
            if (!string.IsNullOrEmpty(currentParticipant.UserAtHost))
            {
                participantPropertiesSB.AppendLine("\t UserAtHost = " + currentParticipant.UserAtHost);
            }
            return participantPropertiesSB.ToString();
        }
    }

    /// <summary>
    /// A class used to pass a media source and AV flow around. This is used to
    /// play a wma file to the user when a DTMF digit is detected.
    /// </summary>
    public class MediaSourceAndAvFlowContainer
    {
        MediaSource _storedMediaSource;
        AudioVideoFlow _storedAvFlow;

        public MediaSource StoredMediaSource
        {
            get { return _storedMediaSource; }
        }

        public AudioVideoFlow StoredAvFlow
        {
            get { return _storedAvFlow; }
        }

        /// <summary>
        /// The constructor for the class.
        /// </summary>
        /// <param name="specifiedMediaSource">
        /// The media source that is to be stored.
        /// </param>
        /// <param name="specifiedAvFlow">
        /// The Audio Video flow that is to be stored.
        /// </param>
        public MediaSourceAndAvFlowContainer(MediaSource specifiedMediaSource, AudioVideoFlow specifiedAvFlow)
        {
            if (null == specifiedMediaSource)
            {
                throw new ArgumentNullException("specifiedMediaSource");
            }

            if (null == specifiedAvFlow)
            {
                throw new ArgumentNullException("specifiedAvFlow");
            }

            _storedMediaSource = specifiedMediaSource;
            _storedAvFlow = specifiedAvFlow;
        }
    }
}