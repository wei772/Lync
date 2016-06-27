/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

// .NET namespaces
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

// UCMA namespaces
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration.Conferencing;
using Microsoft.Rtc.Collaboration.ConferenceManagement;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.ConferenceManagement
{
    public class UCMASampleConferenceManagement
    {
        #region Locals
        // The conference object, populated when the conference is created.
        private string _conferenceUri;
        
        // A helper that takes care of establishing the endpoints.
        private UCMASampleHelper _helper;

        // The response of the lobby operations of admitting and denying users.
        private LobbyOperationResponse _lobbyOperationResponse;

        // User endpoint that schedules the conference, joins, denies and admits 
        // user from lobby.
        private UserEndpoint _organizerEndpoint;

        // User endpoint that joins conference as an invited participant.
        private UserEndpoint _participantAEndpoint;

        // User endpoint that joins conference as non-invited participant and 
        // lands in the conference lobby.
        private UserEndpoint _participantBEndpoint;

        // User endpoint that joins conference as non-invited participant and 
        // lands in the conference lobby.
        private UserEndpoint _participantCEndpoint;

        // The application endpoint that joins as a gateway participant 
        // (impersonating a phone user) and does not land in lobby even though 
        // they are not invited to the conference.
        private ApplicationEndpoint _gatewayEndpoint;

        // Event to notify the main thread when the endpoint has admitted lobby 
        // participants into conference.
        private AutoResetEvent _waitForAdmitLobbyParticipants = new AutoResetEvent(false);

        // Event to notify the main thread when the endpoint has established a 
        // call to the MCU.
        private AutoResetEvent _waitForCallEstablish = new AutoResetEvent(false);

        // Event to notify the main thread when the endpoint has scheduled a conference.
        private AutoResetEvent _waitForConferenceScheduling = new AutoResetEvent(false);

        // Event to notify the main thread when the endpoint has joined a conference.
        private AutoResetEvent _waitForConferenceJoin = new AutoResetEvent(false);

        // Event to notify the main thread when the endpoint has denied lobby 
        // participants access to the conference.
        private AutoResetEvent _waitForDenyLobbyParticipants = new AutoResetEvent(false);

        // Event to notify the main thread when the endpoint has modified the 
        // conference access level.
        private AutoResetEvent _waitForModifyAccessLevel = new AutoResetEvent(false);
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the SampleConferenceManagement quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleConferenceManagement ucmaSampleConferenceManagement = new UCMASampleConferenceManagement();
            ucmaSampleConferenceManagement.Run();
        }

        private void Run()
        {
            try
            {
                // A helper class to take care of platform and endpoint setup and
                // cleanup. This has been abstracted from this sample to focus on 
                // Call Control.
                _helper = new UCMASampleHelper();

                // Create and start endpoints
                CreateEndpoints();

                // Schedule conference
                ScheduleConference();

                // Now that the conference is scheduled, it's time to join it.
                // As we already have a reference to the conference object 
                // populated from the EndScheduleConference call, we will use the
                // conference's uri to join the correct conference.

                // Initialize a conversation off of the participantA endpoint.
                Conversation participantAConversation = new Conversation(_participantAEndpoint);

                #region ParticipantA joins conference
                PrintHeaderInConsole("Invited Participant A joins conference.");

                // Participant A joins conference
                JoinConferenceWithDefaultJoinMode(participantAConversation);

                // Wait until join completes
                _waitForConferenceJoin.WaitOne();

                // Placing a call on the conference-connected conversation and 
                // connects to the respective MCUs.
                PlaceCallToAVMcu(participantAConversation);
                #endregion ParticipantA joins conference

                #region ParticipantB joins conference lobby
                PrintHeaderInConsole("Uninvited Participant B joins conference and land in lobby.");

                // Initialize a conversation off of the participantB endpoint.
                Conversation participantBConversation = new Conversation(_participantBEndpoint);

                // Join conference, but since this participant was not in the 
                // invited list it will land in lobby and the call back for the 
                // BeginJoin will only be triggered as soon as the user is 
                // admitted to the conference or when the LobbyTimeout is reached. 
                // Will not force synchronization here nor establish a call to the MCU.
                JoinConferenceWithDefaultJoinMode(participantBConversation);
                #endregion ParticipantB joins conference lobby

                #region ParticipantC joins conference lobby
                PrintHeaderInConsole("Uninvited Participant C joins conference and land in lobby.");

                // Initialize a conversation off of the participantC endpoint.
                Conversation participantCConversation = new Conversation(_participantCEndpoint);

                // Join conference, but since this participant was not in the 
                // invited list it will land in lobby and the call back for the 
                // BeginJoin will only be triggered as soon as the user is 
                // admitted to the conference or when the LobbyTimeout is reached. 
                // Will not force synchronization here nor establish a call to the MCU.
                JoinConferenceWithDefaultJoinMode(participantCConversation);
                #endregion ParticipantC joins conference lobby

                #region Gateway participant impersonating phone user joins conference
                PrintHeaderInConsole("Gateway participant joins conference impersonating phone user.");

                // Initialize a conversation off of the gateway endpoint.
                Conversation gatewayParticipantConversation = new Conversation(_gatewayEndpoint);

                // Impersonated a phone user.
                gatewayParticipantConversation.Impersonate("sip:ImpersonatedPhoneUser@contoso.com", 
                    "tel:+12223334444", "Impersonated Phone User");

                // register for conferencesession state changes events.
                RegisterForConferenceSessionEvents(gatewayParticipantConversation.ConferenceSession);

                // Register for Conversation events.
                RegisterForConverstationEvents(gatewayParticipantConversation);

                // Indicate how the participant will joining the conference.
                // ConferenceJoinOptions allows a participant to join as a 
                // gatewayparticipant or not.
                ConferenceJoinOptions gatewayParticipant_ConfJoinOptions = new ConferenceJoinOptions();

                // Join as a gateway participant. Since LobbyBypass is enabled 
                // for gateway participants even though the phone user is not in
                // the invited list of the conference it should not land in lobby
                gatewayParticipant_ConfJoinOptions.JoinMode = JoinMode.GatewayParticipant;

                // Join and wait, again forcing synchronization.
                gatewayParticipantConversation.ConferenceSession.BeginJoin(_conferenceUri, 
                    gatewayParticipant_ConfJoinOptions, EndJoinConference, 
                    gatewayParticipantConversation.ConferenceSession);
                
                // Wait until join completes.
                _waitForConferenceJoin.WaitOne();

                // Placing the call to the AudioVideo MCU.
                PlaceCallToAVMcu(gatewayParticipantConversation);
                #endregion Gateway participant impersonating phone user joins conference

                #region Organizer joins conference
                PrintHeaderInConsole("Organizer joins conference.");

                // Initialize a conversation off of the organizer endpoint.
                Conversation organizerConversation = new Conversation(_organizerEndpoint);

                // Participant A joins conference.
                JoinConferenceWithDefaultJoinMode(organizerConversation);

                // Wait until join completes.
                _waitForConferenceJoin.WaitOne();

                // Placing the call to the AudioVideo MCU.
                PlaceCallToAVMcu(organizerConversation);
                #endregion Organizer joins conference

                #region Organizer lobby operations
                PrintHeaderInConsole("Organizer performs lobby operations.");

                // Obtain the list of participants currently in lobby state.
                Collection<ConversationParticipant> lobbyParticipants = organizerConversation.GetLobbyParticipants();

                // Create two collections of participants to be admitted and 
                // participants to be denied access to conference.
                Collection<ConversationParticipant> lobbyParticipantsToAdmit = 
                    new Collection<ConversationParticipant>();
                Collection<ConversationParticipant> lobbyParticipantsToDeny = 
                    new Collection<ConversationParticipant>();

                // Fill in the collections with participants to be denied and 
                // admitted.
                foreach (ConversationParticipant lobbyParticipant in lobbyParticipants)
                {
                    if (String.Equals(lobbyParticipant.UserAtHost, 
                        participantBConversation.LocalParticipant.UserAtHost, StringComparison.Ordinal))
                    {
                        lobbyParticipantsToAdmit.Add(lobbyParticipant);
                    }

                    if (String.Equals(lobbyParticipant.UserAtHost, 
                        participantCConversation.LocalParticipant.UserAtHost, StringComparison.Ordinal))
                    {
                        lobbyParticipantsToDeny.Add(lobbyParticipant);
                    }
                }

                AdmitLobbyParticipants(organizerConversation, lobbyParticipantsToAdmit);

                // Recently admitted participant places a call to the AVMcu.
                PlaceCallToAVMcu(participantBConversation);

                DenyLobbyParticipants(organizerConversation, lobbyParticipantsToDeny);
                #endregion Organizer lobby operations

                #region Organizer modifies conference AccessLevel
                PrintHeaderInConsole("Organizer opens the conference to users of the same company.");

                // Organizer opens the conference to everyone in the same enterprise.
                ModifyConferenceAccess(organizerConversation);
                #endregion Organizer modifies conference AccessLevel

                #region Participant C joins conference again
                PrintHeaderInConsole("Participant C joins conference again.");

                // Creates a brand new conversation for participant C.
                participantCConversation = new Conversation(_participantCEndpoint);

                // Join conference and since the conference access level is now 
                // SameEnterprise, user should join successfully.
                JoinConferenceWithDefaultJoinMode(participantCConversation);

                // Wait until join completes.
                _waitForConferenceJoin.WaitOne();

                // Placing the calls on the conference-connected conversation 
                // connects to the respective MCUs.
                PlaceCallToAVMcu(participantCConversation);
                #endregion Participant C joins conference again

                Console.Write("\n\n********************\n");
                Console.WriteLine("Press enter to exit.");
                Console.WriteLine("********************\n\n");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                // Terminate the platform which would in turn terminate any 
                // endpoints.
                Console.WriteLine("Shutting down the platform.");
                _helper.ShutdownPlatform();
            }
        }

        private static void PrintHeaderInConsole(string header)
        {
            Console.WriteLine();
            Console.WriteLine("*****************************");
            Console.WriteLine(header);
            Console.WriteLine("*****************************");
            Console.WriteLine();
        }

        #region Private Methods
        /// <summary>
        /// Modify current conference access level.
        /// </summary>
        /// <param name="conversation">Conversation instance of the endpoint 
        /// performing the modification of the conference AccessLevel</param>
        private void ModifyConferenceAccess(Conversation conversation)
        {
            // Modifies conference AccessLevel.
            conversation.ConferenceSession.BeginModifyConferenceConfiguration(
                ConferenceAccessLevel.SameEnterprise,
                conversation.ConferenceSession.LobbyBypass,
                conversation.ConferenceSession.AutomaticLeaderAssignment,
                EndModifyConferenceConfiguration,
                conversation.ConferenceSession);

            // Wait until AccessLevel modification completes.
            _waitForModifyAccessLevel.WaitOne();
        }

        /// <summary>
        /// Deny conference access to participants.
        /// </summary>
        /// <param name="conversation">Conversation instance of the endpoint 
        /// performing the lobby operation</param>
        /// <param name="lobbyParticipantsToDeny">List of participants to be 
        /// denied</param>
        private void DenyLobbyParticipants(Conversation conversation, 
            Collection<ConversationParticipant> lobbyParticipantsToDeny)
        {
            // Denies participant C from lobby.
            conversation.ConferenceSession.LobbyManager.BeginDenyLobbyParticipants(
                    lobbyParticipantsToDeny,
                    EndDenyLobbyParticipants,
                    conversation.ConferenceSession.LobbyManager);

            // Wait until denial of lobby participants completes.
            _waitForDenyLobbyParticipants.WaitOne();

            // When a participant lands in the lobby, the callback of the 
            // BeginJoin method will not be called until the lobby operation is 
            // completed, i.e. the participant is admitted or denied access to 
            // the conference, or the LobbyTimeout is reached.
            // Will wait here for the begin join completion since it will also 
            // mean that the lobby operation is completed.
            _waitForConferenceJoin.WaitOne();

            LogLobbyOperationResponse(false /*denial*/);
        }

        /// <summary>
        /// Admits participants from lobby into the conference.
        /// </summary>
        /// <param name="conversation">Conversation instance of the endpoint 
        /// performing the lobby operation</param>
        /// <param name="lobbyParticipantsToAdmit">List of participants to be 
        /// admitted</param>
        private void AdmitLobbyParticipants(Conversation conversation, 
            Collection<ConversationParticipant> lobbyParticipantsToAdmit)
        {
            // Admits participants from lobby.
            conversation.ConferenceSession.LobbyManager.BeginAdmitLobbyParticipants(
                    lobbyParticipantsToAdmit,
                    EndAdmitLobbyParticipants,
                    conversation.ConferenceSession.LobbyManager);

            // Wait until admission of lobby participants completes.
            _waitForAdmitLobbyParticipants.WaitOne();

            // When a participant lands in the lobby, the callback of the 
            // BeginJoin method will not be called until the lobby operation is 
            // completed, i.e. the participant is admitted or denied access to 
            // the conference, or the LobbyTimeout is reached.
            // Will wait here for the begin join completion since it will also 
            // mean that the lobby operation is completed.
            _waitForConferenceJoin.WaitOne();
        }

        /// <summary>
        /// Joins the conference with default join mode in the 
        /// ConferenceJoinOptions.
        /// </summary>
        /// <param name="conversation">Conversation instance of the endpoint 
        /// joining the conference</param>
        private void JoinConferenceWithDefaultJoinMode(Conversation conversation)
        {
            // Register for Conversation events.
            RegisterForConverstationEvents(conversation);

            // Register for ConferenceSession events.
            RegisterForConferenceSessionEvents(conversation.ConferenceSession);

            // Indicate how the participant will join the conference.
            // ConferenceJoinOptions allows a participant to join as a 
            // gatewayparticipant or not.
            ConferenceJoinOptions conferenceJoinOptions = new ConferenceJoinOptions();

            // Join as a default participant.
            conferenceJoinOptions.JoinMode = JoinMode.Default;

            // Indicates the amount of time the user will be in the lobby before
            // being expelled by timeout.
            conferenceJoinOptions.LobbyTimeout = new TimeSpan(0, 5, 0);

            // Join and wait, again forcing synchronization.
            conversation.ConferenceSession.BeginJoin(_conferenceUri, conferenceJoinOptions, EndJoinConference,
                conversation.ConferenceSession);
        }

        /// <summary>
        /// Schedules a conference.
        /// </summary>
        /// <remarks>
        /// Schedules a conference with the following settings:
        /// 
        /// AdmissionPolicy: ClosedAuthenticated (Invited users only)
        /// Passcode: Optional (1357924680)
        /// Description: Conference Description
        /// ExpiryTime: 5 hours
        /// Participants: ParticipantA
        /// LobbyBypass: Enabled for gateway participants. These participants 
        /// will not land in lobby even though are not invited.
        /// Mcus: AudioVideo
        ///
        /// A reference to the conference will be saved from the 
        /// EndScheduleConference method and will be used to reference the 
        /// scheduled conference uri on participant joining.
        /// </remarks>
        private void ScheduleConference()
        {
            // One of the endpoints schedules the conference in advance. At 
            // schedule time, all the conference settings are set.

            // The base conference settings object, used to set the policies for
            // the conference.
            ConferenceScheduleInformation conferenceScheduleInformation = new ConferenceScheduleInformation();

            // A closed meeting (only participants in the list of the scheduled
            // conference can join), but requiring authentication.
            conferenceScheduleInformation.AccessLevel = ConferenceAccessLevel.Invited;

            // This flag determines whether or not the passcode is optional for 
            // users joining the conference.
            conferenceScheduleInformation.IsPasscodeOptional = true;

            // The conference passcode.
            conferenceScheduleInformation.Passcode = "1357924680";

            // The verbose description of the conference.
            conferenceScheduleInformation.Description = "Conference Description";

            // This field indicates the date and time after which the conference
            // can be deleted.
            conferenceScheduleInformation.ExpiryTime = System.DateTime.Now.AddHours(5);

            // Create a Conference Participant Information object with 
            // Participant A uri and conference role.
            ConferenceParticipantInformation participantA_Information =
                new ConferenceParticipantInformation(_participantAEndpoint.OwnerUri, ConferencingRole.Attendee);

            // Add Participant A to the list of conference participants.
            conferenceScheduleInformation.Participants.Add(participantA_Information);

            // This property indicates if the lobby bypass for gateway participants
            // feature is enabled. If enabled for gateway participant, a 
            // participant joining from a phone will not land in the lobby if 
            // the JoinMode is GatewayParticipant.
            conferenceScheduleInformation.LobbyBypass = LobbyBypass.EnabledForGatewayParticipants;

            // This property indicates if the feature that automatic promotes 
            // participants to leader upon joining is enabled.
            conferenceScheduleInformation.AutomaticLeaderAssignment = AutomaticLeaderAssignment.SameEnterprise;

            // These two lines assign a set of modalities (here, only AudioVideo)
            // from the available MCUs to the conference. Custom modalities 
            // (and their corresponding MCUs) may be added at this time, as part
            // of the extensibility model.
            ConferenceMcuInformation audioVideoMCU = new ConferenceMcuInformation(McuType.AudioVideo);
            conferenceScheduleInformation.Mcus.Add(audioVideoMCU);

            // Now that the setup object is complete, schedule the conference 
            // using the conference services of the organizer’s endpoint.
            // Note: the conference organizer is considered a leader of the 
            // conference by default.
            _organizerEndpoint.ConferenceServices.BeginScheduleConference(conferenceScheduleInformation, 
                EndScheduleConference, _organizerEndpoint.ConferenceServices);

            // Wait until scheduling of the conference completes.
            _waitForConferenceScheduling.WaitOne();
        }

        /// <summary>
        /// Create an user endpoint, using the network credential object defined.
        /// The credentials used must be for a user enabled for Microsoft Lync Server,
        /// and capable of logging in from the machine that is running this code.
        /// </summary>
        private void CreateEndpoints()
        {
            // Organizer of the conference.
            _organizerEndpoint = _helper.CreateUserEndpointWithServerPlatform(
                "Conference Organizer and Leader" /*endpointFriendlyName*/);

            // Participant A.
            _participantAEndpoint = _helper.CreateUserEndpointWithServerPlatform(
                "Participant A" /*endpointFriendlyName*/);

            // Participant B.
            _participantBEndpoint = _helper.CreateUserEndpointWithServerPlatform(
                "Participant B" /*endpointFriendlyName*/);

            // Participant C.
            _participantCEndpoint = _helper.CreateUserEndpointWithServerPlatform(
                "Participant C" /*endpointFriendlyName*/);

            // Gateway participant that impersonates a phone user
            _gatewayEndpoint = _helper.CreateApplicationEndpoint(
                "GatewayParticipant" /*endpointFriendlyName*/);
        }

        /// <summary>
        /// Logs the lobby operation responses to the console.
        /// </summary>
        /// <param name="isAdmitting">Indicates if the lobby operation is for 
        /// admission or denial</param>
        private void LogLobbyOperationResponse(bool isAdmitting)
        {
            // If the operation response is null, no reason to log anything;
            // exit.
            if (null == _lobbyOperationResponse)
            {
                Console.WriteLine("** Warning! Lobby operation response is null. **");
                Console.WriteLine();

                return;
            }

            string operationDescription;
            string operationAction;

            // Log admittance or rejection based on variable passed in.
            if (isAdmitting)
            {
                operationDescription = "admission";
                operationAction = " was admitted to the conference.";
            }
            else
            {
                operationDescription = "denial";
                operationAction = " was denied access to the conference.";
            }

            // Logs to console all participants that had admission failed and the
            // reason for the failure.
            foreach (ConversationParticipant failedParticipant in _lobbyOperationResponse.Failed.Keys)
            {
                Console.WriteLine("Lobby {0} failure. Participant {1}  fail {2}  with reason: {3}",
                    operationDescription,
                    failedParticipant.UserAtHost,
                    operationDescription,
                    _lobbyOperationResponse.Failed[failedParticipant]);

                Console.WriteLine();
            }

            // Logs to console all participants that had admission succeeded.
            foreach (ConversationParticipant failedParticipant in _lobbyOperationResponse.Succeeded)
            {
                Console.WriteLine("Lobby {0} succeeded. Participant {1} {2}",
                    operationDescription,
                    failedParticipant.UserAtHost,
                    operationAction);

                Console.WriteLine();
            }

            return;
        }

        /// <summary>
        /// Place a call to the AVMCU.
        /// </summary>
        /// <param name="conversation">conversation which will place the call to
        /// the AVMCU</param>
        private void PlaceCallToAVMcu(Conversation conversation)
        {
            // Create an AudioVideoCall.
            AudioVideoCall avCall = new AudioVideoCall(conversation);

            // Register for AudioVideoMcuSession events.
            RegisterForAudioVideoMcuSessionEvents(conversation.ConferenceSession.AudioVideoMcuSession);

            // Register for AudioVideoCall events.
            RegisterForAudioVideoCallEvents(avCall);

            // Establish call to MCU
            avCall.BeginEstablish(EndCallEstablish, avCall);

            // Wait until call establishment completes
            _waitForCallEstablish.WaitOne();
        }
        
        /// <summary>
        /// Register for Conversation events
        /// </summary>
        /// <param name="conversation">Conversation instance of the endpoint 
        /// performing the lobby operation</param>
        private void RegisterForConverstationEvents(Conversation conversation)
        {
            // Register for notifications to change in lobby's roster.
            conversation.LobbyParticipantAttendanceChanged += this.Conversation_LobbyParticipantAttendanceChanged;
        }

        /// <summary>
        /// Register for AudioVideoCall events
        /// </summary>
        /// <param name="call">Call to register for the events</param>
        private void RegisterForAudioVideoCallEvents(AudioVideoCall call)
        {
            // Register to get notification that the AVFlow was created
            // so that media can be sent through it. Also register for
            // state changes.
            call.AudioVideoFlowConfigurationRequested += this.AVCall_AudioVideoFlowConfigurationRequested;
            call.StateChanged += this.AVCall_StateChanged;
        }

        /// <summary>
        /// Register for AudioVideoMcuSession events
        /// </summary>
        /// <param name="avMcuSession">AVMcuSession that will register for the 
        /// events</param>
        private void RegisterForAudioVideoMcuSessionEvents(AudioVideoMcuSession avMcuSession)
        {
            // Register for AVMCU roster changes.
            avMcuSession.ParticipantEndpointAttendanceChanged += 
                this.AudioVideoMcuSession_ParticipantEndpointAttendanceChanged;
            avMcuSession.ParticipantEndpointPropertiesChanged += 
                this.AudioVideoMcuSession_ParticipantEndpointPropertiesChanged;
            avMcuSession.PropertiesChanged += this.AudioVideoMcuSession_PropertiesChanged;
            avMcuSession.StateChanged += this.AudioVideoMcuSession_StateChanged;
        }

        /// <summary>
        /// Register for ConferenceSession events
        /// </summary>
        /// <param name="conversation"></param>
        private void RegisterForConferenceSessionEvents(ConferenceSession conferenceSession)
        {
            // Register for ConferenceSession events.
            conferenceSession.StateChanged += this.ConferenceSession_StateChanged;
            conferenceSession.PropertiesChanged += this.ConferenceSession_PropertiesChanged;
            conferenceSession.ParticipantEndpointAttendanceChanged += 
                this.ConferenceSession_ParticipantEndpointAttendanceChanged;
            conferenceSession.ParticipantEndpointPropertiesChanged += 
                this.ConferenceSession_ParticipantEndpointPropertiesChanged;
        }
        #endregion Private Methods

        #region Event handlers
        // Just to record the state transitions in the console.
        void Conversation_LobbyParticipantAttendanceChanged(object sender, 
            LobbyParticipantAttendanceChangedEventArgs e)
        {
            Conversation conversation = sender as Conversation;

            // Log each participant as s/he gets added/deleted from the Lobby's roster.
            foreach (ConversationParticipant addedParticipant in e.Added)
            {
                Console.WriteLine("{0} is notified of participant landing in the lobby: {1}",
                    conversation.LocalParticipant.UserAtHost,
                    addedParticipant.UserAtHost);
            }

            foreach (KeyValuePair<ConversationParticipant, LobbyRemovalReason> removedParticipant in e.Removed)
            {
                Console.WriteLine("{0} is notified of participant leaving the lobby: {1} with reason: {2}",
                    conversation.LocalParticipant.UserAtHost,
                    removedParticipant.Key.UserAtHost,
                    removedParticipant.Value);
            }

            Console.WriteLine();
        }


        //Just to record the state transitions in the console.
        void ConferenceSession_ParticipantEndpointAttendanceChanged(object sender, 
            ParticipantEndpointAttendanceChangedEventArgs<ConferenceParticipantEndpointProperties> e)
        {
            ConferenceSession confSession = sender as ConferenceSession;

            // Log each participant as s/he gets added/deleted from the ConferenceSession's roster.
            foreach (KeyValuePair<ParticipantEndpoint, ConferenceParticipantEndpointProperties> pair in e.Joined)
            {
                Console.WriteLine("{0} is notified of participant joining the conference: {1}",
                    confSession.Conversation.LocalParticipant.UserAtHost,
                    pair.Key.Participant.UserAtHost);
            }

            foreach (KeyValuePair<ParticipantEndpoint, ConferenceParticipantEndpointProperties> pair in e.Left)
            {
                Console.WriteLine("{0} is notified of participant leaving the conference: {1}",
                    confSession.Conversation.LocalParticipant.UserAtHost,
                    pair.Key.Participant.UserAtHost);
            }
        
            Console.WriteLine();
        }

        // Just to record the state transitions in the console.
        void ConferenceSession_ParticipantEndpointPropertiesChanged(object sender, 
            ParticipantEndpointPropertiesChangedEventArgs<ConferenceParticipantEndpointProperties> e)
        {
            ConferenceSession confSession = sender as ConferenceSession;

            Console.WriteLine(
                "{0} is notified of ConferenceSession participant property change for user: {1}. Role:{2}, CanManageLobby:{3}, InLobby:{4}",
                confSession.Conversation.LocalParticipant.UserAtHost,
                e.ParticipantEndpoint.Participant.UserAtHost,
                e.Properties.Role,
                e.Properties.CanManageLobby,
                e.Properties.IsInLobby);
            
            Console.WriteLine();
        }

        // Just to record the state transitions in the console.
        void ConferenceSession_PropertiesChanged(object sender, 
            PropertiesChangedEventArgs<ConferenceSessionProperties> e)
        {
            ConferenceSession confSession = sender as ConferenceSession;
            string propertyValue = null;

            foreach (string property in e.ChangedPropertyNames)
            {
                // Record all ConferenceSession property changes.
                switch (property)
                {
                    case "AccessLevel":
                        propertyValue = e.Properties.AccessLevel.ToString();
                        break;
                    case "AutomaticLeaderAssignment":
                        propertyValue = e.Properties.AutomaticLeaderAssignment.ToString();
                        break;
                    case "ConferenceUri":
                        propertyValue = e.Properties.ConferenceUri;
                        break;
                    case "Disclaimer":
                        propertyValue = e.Properties.Disclaimer;
                        break;
                    case "DisclaimerTitle":
                        propertyValue = e.Properties.DisclaimerTitle;
                        break;
                    case "HostingNetwork":
                        propertyValue = e.Properties.HostingNetwork.ToString();
                        break;
                    case "LobbyBypass":
                        propertyValue = e.Properties.LobbyBypass.ToString();
                        break;
                    case "Organizer":
                        propertyValue = e.Properties.Organizer.UserAtHost;
                        break;
                    case "ParticipantData":
                        propertyValue = e.Properties.ParticipantData;
                        break;
                    case "RecordingPolicy":
                        propertyValue = e.Properties.RecordingPolicy.ToString();
                        break;
                    case "SchedulingTemplate":
                        propertyValue = e.Properties.SchedulingTemplate.ToString();
                        break;
                    case "Subject":
                        propertyValue = e.Properties.Subject;
                        break;
                }
                Console.WriteLine("{0} is notified of ConferenceSession property change. {1}: {2}",
                    confSession.Conversation.LocalParticipant.UserAtHost,
                    property,
                    propertyValue);
            }
            
            Console.WriteLine();
        }
        
        // Just to record the state transitions of the AVMCU in the console.
        void AudioVideoMcuSession_StateChanged(object sender, StateChangedEventArgs<McuSessionState> e)
        {
            AudioVideoMcuSession avSession = sender as AudioVideoMcuSession;

            Console.WriteLine("{0} is notified of AudioVideoMcuSession state change. From: \"{1}\" To: \"{2}\"",
                avSession.ConferenceSession.Conversation.LocalParticipant.UserAtHost,
                e.PreviousState,
                e.State);

            Console.WriteLine();
        }

        //Just to record the property changes of AVMCU in the console.
        void AudioVideoMcuSession_PropertiesChanged(object sender, 
            PropertiesChangedEventArgs<AudioVideoMcuSessionProperties> e)
        {
            AudioVideoMcuSession avSession = sender as AudioVideoMcuSession;
            
            string propertyValue = null;

            foreach (string property in e.ChangedPropertyNames)
            {
                switch (property)
                {
                    case "SupportsAudio":
                        propertyValue = e.Properties.SupportsAudio.ToString();
                        break;
                    case "SupportsVideo":
                        propertyValue = e.Properties.SupportsVideo.ToString();
                        break;
                }

                Console.WriteLine("{0} is notified of AudioVideoMcuSession property change. {1}: {2}",
                    avSession.ConferenceSession.Conversation.LocalParticipant.UserAtHost,
                    property,
                    propertyValue);
            }

            Console.WriteLine();
        }

        // Just to record the AVMCU participant's property changes in the console.
        void AudioVideoMcuSession_ParticipantEndpointPropertiesChanged(object sender, 
            ParticipantEndpointPropertiesChangedEventArgs<AudioVideoMcuParticipantEndpointProperties> e)
        {
            AudioVideoMcuSession avSession = sender as AudioVideoMcuSession;

            string propertyValue = null;

            foreach (string property in e.ChangedPropertyNames)
            {
                switch (property)
                {
                    case "AccessMethod":
                        propertyValue = e.Properties.AccessMethod.ToString();
                        break;
                    case "AuthenticationMethod":
                        propertyValue = e.Properties.AuthenticationMethod.ToString();
                        break;
                    case "IsAudioMuted":
                        propertyValue = e.Properties.IsAudioMuted.ToString();
                        break;
                    case "IsVideoMuted":
                        propertyValue = e.Properties.IsVideoMuted.ToString();
                        break;
                    case "JoinMethod":
                        propertyValue = e.Properties.JoinMethod.ToString();
                        break;
                    case "Media":
                        foreach (McuMediaChannel channel in e.Properties.Media)
                        {
                            propertyValue = String.Format("DisplayText:{0}", channel.DisplayText);
                            propertyValue += String.Format(", Id:{0}", channel.Id);
                            propertyValue += String.Format(", Label:{0}", channel.Label);
                            propertyValue += String.Format(", MediaType:{0}", channel.MediaType);
                            propertyValue += String.Format(", Status:{0}", channel.Status);
                        }
                        break;
                    case "PreferredLanguages":
                        propertyValue = e.Properties.PreferredLanguages.ToString();
                        break;
                    case "Role":
                        propertyValue = e.Properties.Role.ToString();
                        break;
                    case "State":
                        propertyValue = e.Properties.State.ToString();
                        break;
                }

                Console.WriteLine(
                    "{0} is notified of AudioVideoMcuSession participant property change for user: {1} where {2}: {3}",
                        avSession.ConferenceSession.Conversation.LocalParticipant.UserAtHost,
                        e.ParticipantEndpoint.Participant.UserAtHost,
                        property,
                        propertyValue);
            }

            Console.WriteLine();
        }

        // Just to record the changes in AVMCU roster in the console.
        void AudioVideoMcuSession_ParticipantEndpointAttendanceChanged(object sender, 
            ParticipantEndpointAttendanceChangedEventArgs<AudioVideoMcuParticipantEndpointProperties> e)
        {
            AudioVideoMcuSession avSession = sender as AudioVideoMcuSession;

            foreach (KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties> pair in e.Joined)
            {
                Console.WriteLine("{0} is notified of participant joining the AVMcu: {1}",
                        avSession.ConferenceSession.Conversation.LocalParticipant.UserAtHost,
                        pair.Key.Participant.UserAtHost);
            }

            foreach (KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties> pair in e.Left)
            {
                Console.WriteLine("{0} is notified of participant leaving the AVMcu: {1}",
                        avSession.ConferenceSession.Conversation.LocalParticipant.UserAtHost,
                        pair.Key.Participant.UserAtHost);
            }

            Console.WriteLine();
        }

        // Just to record the state transitions of the ConferenceSession in the console.
        void ConferenceSession_StateChanged(object sender, StateChangedEventArgs<ConferenceSessionState> e)
        {
            ConferenceSession confSession = sender as ConferenceSession;

            Console.WriteLine("{0} is notified of ConferenceSession state change. From: \"{1}\" To: \"{2}\"",
                    confSession.Conversation.LocalParticipant.UserAtHost,
                    e.PreviousState,
                    e.State);

            Console.WriteLine();
        }

        // Just to record the state transitions of the Call in the console.
        void AVCall_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Call call = sender as Call;

            Console.WriteLine("{0} is notified of Call state change. From: \"{1}\" To: \"{2}\"",
                    call.Conversation.LocalParticipant.UserAtHost,
                    e.PreviousState,
                    e.State);

            Console.WriteLine();
        }

        // Flow created indicates that there is a flow present to begin media 
        // operations with, and that it is no longer null.
        void AVCall_AudioVideoFlowConfigurationRequested(object sender, 
            AudioVideoFlowConfigurationRequestedEventArgs e)
        {
            Call call = sender as Call;

            Console.WriteLine("{0} is notified that AudioVideoCall Flow was created.",
                    call.Conversation.LocalParticipant.UserAtHost);

            Console.WriteLine();

            // Now that the flow is non-null, bind the event handlers for State Changed
            e.Flow.StateChanged += this.AudioVideoFlow_StateChanged;
        }

        // Just to record the state transitions of the AVFlow in the console.
        void AudioVideoFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            AudioVideoFlow avFlow = sender as AudioVideoFlow;

            Console.WriteLine("{0} is notified of AudioVideoFlow state change. From: \"{1}\" To: \"{2}\"",
                    avFlow.Call.Conversation.LocalParticipant.UserAtHost,
                    e.PreviousState,
                    e.State);

            Console.WriteLine();
        }
        #endregion Event handlers

        #region Callback methods

        /// <summary>
        /// Callback for Call.BeginEstablish
        /// </summary>
        /// <param name="ar"></param>
        private void EndCallEstablish(IAsyncResult ar)
        {
            Call call = ar.AsyncState as Call;
            try
            {
                call.EndEstablish(ar);
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(exception.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _waitForCallEstablish.Set();
            }
        }

        /// <summary>
        /// Callback for BeginScheduleConference method
        /// </summary>
        /// <param name="ar"></param>
        private void EndScheduleConference(IAsyncResult ar)
        {
            ConferenceServices confSession = ar.AsyncState as ConferenceServices;
            try
            {   
                // End schedule conference returns the conference object, which 
                // contains the vast majority of the data relevant to that conference.
                Conference conference  = confSession.EndScheduleConference(ar);
                _conferenceUri = conference.ConferenceUri;

                Console.WriteLine("");
                Console.WriteLine(" The conference is now scheduled.");
                Console.WriteLine("");

            }
            catch (ConferenceFailureException confFailEx)
            {
                // ConferenceFailureException may be thrown on failures to schedule
                // due to MCUs being absent or unsupported, or due to malformed parameters.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(confFailEx.ToString());
            }

            //Again, for sync. reasons.
            _waitForConferenceScheduling.Set();
        }

        /// <summary>
        /// Callback for BeginJoinConference method
        /// </summary>
        /// <param name="ar"></param>
        private void EndJoinConference(IAsyncResult ar)
        {
            ConferenceSession confSession = ar.AsyncState as ConferenceSession;
            try
            {
                confSession.EndJoin(ar);
            }
            catch (ConferenceFailureException confFailEx)
            {
                // ConferenceFailureException may be thrown on failures due to 
                // MCUs being absent or unsupported, or due to malformed parameters.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(confFailEx.ToString());
            }
            catch (RealTimeException rTEx)
            {
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(rTEx.ToString());
            }
            finally
            {
                //Again, for sync. reasons.
                _waitForConferenceJoin.Set();
            }
        }

        /// <summary>
        /// Callback for BeginAdmitLobbyParticipants method
        /// </summary>
        /// <param name="ar"></param>
        private void EndAdmitLobbyParticipants(IAsyncResult ar)
        {
            EndAdmitOrDenyLobbyParticipant(ar, true /*Admission*/);
        }

        /// <summary>
        /// Callback for BeginDenyLobbyParticipants method
        /// </summary>
        /// <param name="ar"></param>
        private void EndDenyLobbyParticipants(IAsyncResult ar)
        {
            EndAdmitOrDenyLobbyParticipant(ar, false /*Denial*/);
        }

        /// <summary>
        /// Handles the callback for BeginAdmitLobbyParticipants and 
        /// BeginDenyLobbyParticipants 
        /// </summary>
        /// <param name="ar">AsyncResult of the callback</param>
        /// <param name="isAdmitting">If the call back is for admission 
        /// operation</param>
        private void EndAdmitOrDenyLobbyParticipant(IAsyncResult ar, bool isAdmitting)
        {
            LobbyManager lobbyManager = ar.AsyncState as LobbyManager;

            try
            {
                if (isAdmitting)
                {
                    // Saves the lobby operation response so this can be verified
                    _lobbyOperationResponse = lobbyManager.EndAdmitLobbyParticipants(ar);
                }
                else
                {
                    // Saves the lobby operation response so this can be verified
                    _lobbyOperationResponse = lobbyManager.EndDenyLobbyParticipants(ar);
                }


                // log the lobby operation response to console
                LogLobbyOperationResponse(isAdmitting);
            }
            catch (ConferenceFailureException confFailEx)
            {
                // ConferenceFailureException may be thrown on failures due to 
                // MCUs being absent or unsupported, or due to malformed parameters.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(confFailEx.ToString());
            }
            catch (RealTimeException rTEx)
            {
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(rTEx.ToString());
            }
            finally
            {
                if (isAdmitting)
                {
                    //Again, for sync. reasons.
                    _waitForAdmitLobbyParticipants.Set();
                }
                else
                {
                    //Again, for sync. reasons.
                    _waitForDenyLobbyParticipants.Set();
                }
            }
        }

        /// <summary>
        /// Callback for BeginModifyConferenceConfiguration method
        /// </summary>
        /// <param name="ar"></param>
        private void EndModifyConferenceConfiguration(IAsyncResult ar)
        {
            ConferenceSession conferenceSession = ar.AsyncState as ConferenceSession;
            try
            {
                conferenceSession.EndModifyConferenceConfiguration(ar);
            }
            catch (ConferenceFailureException confFailEx)
            {
                // ConferenceFailureException may be thrown on failures due to 
                // MCUs being absent or unsupported, or due to malformed parameters.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(confFailEx.ToString());
            }
            catch (RealTimeException rTEx)
            {
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(rTEx.ToString());
            }
            finally
            {
                //Again, for sync. reasons.
                _waitForModifyAccessLevel.Set();
            }
        }

        #endregion Callback methods
        #endregion
    }
    
}
