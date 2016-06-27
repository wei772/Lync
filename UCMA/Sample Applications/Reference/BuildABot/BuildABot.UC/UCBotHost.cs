/*====================================================
THIS CODE AND INFORMATION ARE PROVIDED "AS IS."  YOU BEAR THE RISK OF USING IT.  
MICROSOFT GIVES NO EXPRESS WARRANTIES, GUARANTIES OR CONDITIONS.  
TO THE EXTENT PERMITTED UNDER YOUR LOCAL LAWS, MICROSOFT EXCLUDES 
THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NON-INFRINGEMENT. 
=====================================================*/

namespace BuildABot.UC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using BuildABot.Core;
    using BuildABot.Core.Feedback;
    using BuildABot.Core.MessageHandlers;
    using BuildABot.Util;
    using Microsoft.Rtc.Collaboration;
    using Microsoft.Rtc.Collaboration.ConferenceManagement;
    using Microsoft.Rtc.Signaling;
    /// <summary>
    /// Hosts Unified Communications (Lync) bots.
    /// </summary>
    public class UCBotHost
    {

        private string applicationUserAgent;
        private string applicationUrn;
  

        // Just some global keepers.
        private CollaborationPlatform collabPlatform;

        /// <summary>
        /// ApplicationEndpoint is used as a Bot.
        /// </summary>
        private ApplicationEndpoint botAppEndPoint;

        /// <summary>
        /// Helptext is displayed if bot does not understand user's message.
        /// </summary>
        private string helpText = string.Format("Sorry, I didn't get you. Type {0}  to see what I can understand by default.", "help".EncloseRtfBold()).EncloseRtf();

        // Conference related variables.
        private ConferenceScheduleInformation conferenceScheduleInformation;
        private Conference conference;
        private Conversation callerConversation;

        /// <summary>
        /// Gets the list of all conferences attended by the bot.
        /// </summary>
        private List<string> conferenceUris = new List<string>();

        /// <summary>
        /// Gets the dictionary of all flows and calls.
        /// This will help with getting conversation properties whenever instant message received by Bot.
        /// </summary>
        private Dictionary<InstantMessagingFlow, InstantMessagingCall> messageCalls = new Dictionary<InstantMessagingFlow, InstantMessagingCall>();

        /// <summary>
        /// In case Bot didn't understand what user said, it doesn't make sense to show same help message over and over again when users are part of the conference because most likely they are chatting to themselves.
        /// Thus, this collection will help with understanding when Bot need to show help message in case he didn't understand the user.
        /// </summary>
        private Dictionary<InstantMessagingFlow, bool> conversationFlowMisunderstandingAlerts = new Dictionary<InstantMessagingFlow, bool>();

        private bool escalateToConference;

        // Event to notify application main thread for completion of the operation.
        private AutoResetEvent applicationCompletedEvent = new AutoResetEvent(false);
        private AutoResetEvent applicationConferenceSchedulingCompletedEvent = new AutoResetEvent(false);
        private AutoResetEvent applicationConferenceJoinCompletedEvent = new AutoResetEvent(false);
        private AutoResetEvent applicationWaitForCallEstablish = new AutoResetEvent(false);

        /// <summary>
        /// List of bots maintained by the UC host. Every user (actually every messaging flow) gets it's own bot.
        /// </summary>
        private Dictionary<InstantMessagingFlow, Bot> bots = new Dictionary<InstantMessagingFlow, Bot>();


        /// <summary>
        /// Initializes a new instance of the <see cref="UCBotHost"/> class.
        /// </summary>
        /// <param name="applicationUserAgent"> Name of the application user agent.</param>
        /// <param name="applicationUrn">The application urn.</param>
        public UCBotHost(string applicationUserAgent, string applicationUrn)
        {
            this.applicationUserAgent = applicationUserAgent;
            this.applicationUrn = applicationUrn;
            this.FeedbackEngine = new FeedbackEngine();
            UcBotHostHelper.CurrentHost = this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UCBotHost"/> class.
        /// </summary>
        /// <param name="applicationUserAgent"> Name of the application user agent.</param>
        /// <param name="applicationUrn">The application urn.</param>
        /// <param name="helpText">The help text to be displayed when the bot doesn't understand the user message.</param>
        public UCBotHost(string applicationUserAgent, string applicationUrn, string helpText)
            : this(applicationUserAgent, applicationUrn)
        {
            this.helpText = helpText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UCBotHost"/> class.
        /// </summary>
        /// <param name="applicationUserAgent"> Name of the application user agent.</param>
        /// <param name="applicationUrn">The application urn.</param>  
        /// <param name="helpText">The help text.</param>
        /// <param name="conferenceInformation">The conference information that will be used to create a conference.
        /// <example>
        ///  ConferenceInformation allow developer to set up the conference. So when developer creates an application and uses BuildABot framework he/she can add conferencing information as such:
        ///     <code>
        ///          ConferenceInformation conferenceScheduleInformation = new ConferenceInformation(); // The base conference settings object, used to set the policies for the conference.
        ///          conferenceInformation.IsPasscodeOptional = true; // This flag determines whether or not the passcode is optional for users joining the conference.
        ///          conferenceInformation.Passcode = "1357924680"; // The conference passcode.
        ///          conferenceInformation.Description = "Interesting Description"; // The verbose description of the conference.
        ///          conferenceInformation.Subject = "This is subject of the conference"; // Subject will appear in the header.
        ///          conferenceInformation.ExpiryTime = System.DateTime.Now.AddHours(5); // This field indicates the date and time after which the conference can be deleted.
        ///    </code>
        /// </example>
        /// </param>
        /// <remarks></remarks>
        public UCBotHost(string applicationUserAgent, string applicationUrn,  string helpText, ConferenceInformation conferenceInformation)
            : this(applicationUserAgent, applicationUrn, helpText)
        {
            if (conferenceInformation != null)
            {
                this.conferenceScheduleInformation = new ConferenceScheduleInformation();

                this.conferenceScheduleInformation.AccessLevel = ConferenceAccessLevel.Everyone;
                this.conferenceScheduleInformation.IsPasscodeOptional = conferenceInformation.IsPasscodeOptional;
                this.conferenceScheduleInformation.Passcode = conferenceInformation.Passcode;
                this.conferenceScheduleInformation.Description = conferenceInformation.Description;
                this.conferenceScheduleInformation.ExpiryTime = conferenceInformation.ExpiryTime;
                this.conferenceScheduleInformation.Subject = conferenceInformation.Subject;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UCBotHost"/> class.
        /// </summary>
        /// <param name="applicationUserAgent"> Name of the application user agent.</param>
        /// <param name="applicationUrn">The application urn.</param>
        /// <param name="helpText">The help text.</param>
        /// <param name="conferenceUri">The conference URI. Should contain only id and initiator.
        /// <example>
        /// sip:nryabov@microsoft.com;gruu;opaque=app:conf:focus:id:E339C29D99BBE4429930B21B0B623175
        /// </example>
        /// </param>
        /// <remarks></remarks>
        public UCBotHost(string applicationUserAgent, string applicationUrn,string helpText, string conferenceUri)
            : this(applicationUserAgent, applicationUrn, helpText)
        {
            if (UcBotHostHelper.TryValidateConferenceUri(ref conferenceUri) && !this.conferenceUris.Contains(conferenceUri))
            {
                this.conferenceUris.Add(conferenceUri);
            }
        }


        /// <summary>
        /// Occurs whenever the UCHost receives a message.
        /// </summary>
        public event MessageEventHandler MessageReceived;


        /// <summary>
        /// Occurs when an error occurs.
        /// </summary>
        public event ErrorEventHandler ErrorOccurred;

        /// <summary>
        /// Occurs when conference is created.
        /// </summary>
        public event ConferenceCreatedEventHandler ConferenceCreated;

        /// <summary>
        /// Occurs when UC bot replied to the message.
        /// </summary>
        public event ReplyEventHandler Replied;

        /// <summary>
        /// Gets or sets the feedback engine.
        /// </summary>
        /// <value>The feedback engine.</value>
        public FeedbackEngine FeedbackEngine { get; set; }



        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {

            // Used provisioned application end point settings.
            ProvisionedApplicationPlatformSettings platformSettings = new ProvisionedApplicationPlatformSettings(
                                                                                                                this.applicationUserAgent,
                                                                                                                this.applicationUrn);
            this.collabPlatform = new CollaborationPlatform(platformSettings);
            // Register handler for application end point settings discovered. 
            this.collabPlatform.RegisterForApplicationEndpointSettings(this.ApplicationEndpointOwnerDiscovered);

            this.collabPlatform.BeginStartup(
                        delegate(IAsyncResult ar)
                        {
                            this.collabPlatform.EndStartup(ar);
                            Debug.WriteLine("Collaboration Platform started.");

                        },
                        null);

            // Wait for shutdown to occur.
            this.applicationCompletedEvent.WaitOne();
        }

        /// <summary>
        /// Starts a conversation.
        /// </summary>
        /// <param name="destinationUri">The destination URI.</param>
        /// <param name="message">The message.</param>
        /// <param name="outgoingMessage">The outgoing message.</param>
        public void StartConversation(string destinationUri, Message message, Reply outgoingMessage)
        {
            if (Bot.CanProcess(message))
            {
                //Conversation represents a collection of modalities in the context of a dialog with one or multiple callees.             
                Conversation conversation = new Conversation(this.botAppEndPoint);

                InstantMessagingCall outgoingIMCall = new InstantMessagingCall(conversation);

                //Register a call event handler. 
                outgoingIMCall.StateChanged += this.Call_StateChanged;

                //Establish the call.
                //The following code assumes that no custom headers or mime parts are being sent with the INVITE.
                // TODO: check the CallEstabilishOptions. For now, passing null.                
                outgoingIMCall.BeginEstablish(destinationUri, null, this.OutgoingIMCallEstablished, new ConversationProperties(destinationUri, message, outgoingMessage, outgoingIMCall));

            }
            else
            {
                throw new StartConversationFailedException(string.Format("Couldn't find a message handler to process the requested message {0}", message));
            }
        }

        /// <summary>
        /// Event handler for event of end point settings are discovered.(Auto provisioning of application end point). 
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Rtc.Collaboration.ApplicationEndpointSettingsDiscoveredEventArgs"/> instance containing the event data.</param>
        private void ApplicationEndpointOwnerDiscovered(object sender, ApplicationEndpointSettingsDiscoveredEventArgs e)
        {
            ApplicationEndpointSettings applicationEndPointSettings;
            //Unregister event handler.
            this.collabPlatform.UnregisterForApplicationEndpointSettings(this.ApplicationEndpointOwnerDiscovered);

            // Get the application end point settings.
            applicationEndPointSettings = e.ApplicationEndpointSettings;
            applicationEndPointSettings.AutomaticPresencePublicationEnabled = true;

            // Create an instance of an application end point.
            this.botAppEndPoint = new ApplicationEndpoint(this.collabPlatform, applicationEndPointSettings);
            // Register end point for IM  call handler.
            this.botAppEndPoint.RegisterForIncomingCall<InstantMessagingCall>(this.InstantMessagingCallHandler);
           

            try
            {
                //Start establishing end point.
                this.botAppEndPoint.BeginEstablish(
                    ar =>
                    {
                        try
                        {
                            botAppEndPoint.EndEstablish(ar);
                            Debug.WriteLine("The Application Endpoint owned by URI: ");
                            Debug.WriteLine(botAppEndPoint.OwnerUri);
                            Debug.WriteLine(" is now established and registered.");

                            // In case developer specified conference configuration in constructor, here is the place we will do the all required settings.
                            if (conferenceScheduleInformation != null)
                            {
                                SetupConference();
                            }
                            else if (conferenceUris.Count > 0)
                            {
                                foreach (string uri in conferenceUris)
                                {
                                    JoinPublicConference(uri);
                                }
                            }

                        }
                        catch (RealTimeException ex)
                        {
                            string originator = "Failed to establish application end point";
                            this.RaiseErrorOccured(originator, ex);

                        }
                    },
                    null);
            }
            catch (InvalidOperationException ex)
            {
                string originator = "Failed to start  application end point";
                this.RaiseErrorOccured(originator, ex);
            }
        }

        #region Conference Call Related Methods
        /// <summary>
        /// Setup the conference if needed.
        /// </summary>
        /// <param name="conferenceScheduleInformation">The conference schedule information.</param>
        public void SetupConference(ConferenceScheduleInformation conferenceScheduleInformation)
        {
            if (conferenceScheduleInformation != null)
            {
                this.conferenceScheduleInformation = conferenceScheduleInformation;
            }
            this.SetupConference();
        }

        /// <summary>
        /// Joins the public conference.
        /// </summary>
        /// <param name="conferenceUri">The conference URI.</param>
        public void JoinPublicConference(string conferenceUri)
        {
            if (UcBotHostHelper.TryValidateConferenceUri(ref conferenceUri))
            {
                // Now that the conference is scheduled, it's time to join it.
                // As we already have a reference to the conference object populated from the EndScheduleConference call, we do not need to get the conference first.
                this.JoinConference(conferenceUri);

                // ucBotEndpoint already registered for incoming calls and will handle it correctly only when someone is directly calling the Bot.
                // In order to listen to conversation in the conference we need to call it first.
                this.CallConference();
            }
        }

        /// <summary>
        /// Setup the conference if needed.
        /// </summary>
        /// <remarks></remarks>
        private void SetupConference()
        {
            this.ScheduleConference(this.conferenceScheduleInformation);

            // It make sense to continue only if conference was created
            if (this.conference != null)
            {
                this.JoinPublicConference(this.conference.ConferenceUri);
            }
        }


        /// <summary>
        /// Schedules the conference.
        /// </summary>
        /// <remarks></remarks>
        private void ScheduleConference(ConferenceScheduleInformation conferenceScheduleInformation)
        {
            // Custom modalities (and their corresponding MCUs) may be added at this time, as part of the extensibility model.
            // [Note] from http://www.webopedia.com/TERM/M/MCU.html. 
            // MSU = multipoint control unit,a device in videoconferencing that connects two or more audiovisual terminals together into one single videoconference call. (In our case Communicator)
            ConferenceMcuInformation instantMessageMCU = new ConferenceMcuInformation(McuType.InstantMessaging);
            conferenceScheduleInformation.Mcus.Add(instantMessageMCU);

            // Now that the setup object is complete, schedule the conference using the conference services off of Endpoint.
            // Note: the conference organizer (bot in this case) is considered a leader of the conference by default.

            // botEndpoint.ConferenceServices.BeginScheduleConference(conferenceScheduleInformation, EndScheduleConference, botEndpoint.ConferenceServices);
            this.botAppEndPoint.ConferenceServices.BeginScheduleConference(conferenceScheduleInformation, this.EndScheduleConference, this.botAppEndPoint.ConferenceServices);

            // Wait for the scheduling to complete.
            this.applicationConferenceSchedulingCompletedEvent.WaitOne();
        }

        /// <summary>
        /// Joins the conference.
        /// </summary>
        /// <param name="conferenceUri">The conference URI.</param>
        /// <remarks></remarks>
        private void JoinConference(string conferenceUri)
        {
            try
            {
                // Initialize a conversation off of the endpoint, and join the conference from the uri provided above.

                //callerConversation = new Conversation(botEndpoint);
                this.callerConversation = new Conversation(this.botAppEndPoint);
                this.callerConversation.ConferenceSession.StateChanged += new EventHandler<StateChangedEventArgs<ConferenceSessionState>>(this.ConferenceSession_StateChanged);

                // Join and wait, again forcing synchronization.
                this.callerConversation.ConferenceSession.BeginJoin(conferenceUri, null, this.EndJoinConference, this.callerConversation.ConferenceSession);
            }
            catch (ArgumentException exception)
            {
                Debug.WriteLine(exception.ToString());
                this.RaiseErrorOccured("Was unable to join conference.", exception);
            }
            finally
            {
                this.applicationConferenceJoinCompletedEvent.WaitOne();
            }
        }

        /// <summary>
        /// Calls the conference. This is appropriate after bot joined the conference.
        /// </summary>
        /// <remarks>
        /// Placing the calls on the conference-connected conversation connects to the respective MCUs.
        /// These calls may then be used to communicate with the conference/MCUs.
        /// </remarks>
        private void CallConference()
        {
            InstantMessagingCall instantMessagingCall = new InstantMessagingCall(this.callerConversation);

            // Hooking up event handlers and then placing the call.
            instantMessagingCall.InstantMessagingFlowConfigurationRequested += this.InstantMessagingCall_InstantMessagingFlowConfigurationRequested;
            instantMessagingCall.StateChanged += this.Call_StateChanged;
            instantMessagingCall.BeginEstablish(this.EndCallEstablish, instantMessagingCall);
            // Synchronize to ensure that call has completed.
            this.applicationWaitForCallEstablish.WaitOne();
        }

        /// <summary>
        /// Handles the ConferenceInvitationReceived event of the bot endpoint.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Rtc.Collaboration.ConferenceInvitationReceivedEventArgs"/> instance containing the event data.</param>
        /// <remarks></remarks>
        private void ConferenceInvitationReceived(object sender, ConferenceInvitationReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Invitation.ConferenceUri))
            {
                if (!this.conferenceUris.Contains(e.Invitation.ConferenceUri))
                {
                    this.conferenceUris.Add(e.Invitation.ConferenceUri);
                }

                this.JoinPublicConference(e.Invitation.ConferenceUri);
            }
        }

        /// <summary>
        /// Occurs when conference is scheduled.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <remarks></remarks>
        private void EndScheduleConference(IAsyncResult argument)
        {
            ConferenceServices conferenceSession = argument.AsyncState as ConferenceServices;
            Exception exception = null;
            try
            {
                // End schedule conference returns the conference object, which contains the vast majority of the data relevant to that conference.
                this.conference = conferenceSession.EndScheduleConference(argument);
                Debug.WriteLine("The conference is now scheduled.");
                if (!this.conferenceUris.Contains(this.conference.ConferenceUri))
                {
                    this.conferenceUris.Add(this.conference.ConferenceUri);
                }
            }
            catch (ConferenceFailureException conferenceFailureException)
            {
                // ConferenceFailureException may be thrown on failures to schedule due to MCUs being absent or unsupported, or due to malformed parameters.
                // It is left to the application to perform real error handling here.
                Debug.WriteLine(conferenceFailureException.ToString());
                exception = conferenceFailureException;
            }
            catch (PublishSubscribeException subscribeException)
            {
                // PublishSubscribeException may be thrown on failures to parce required information from the sip server
                Debug.WriteLine(subscribeException.ToString());
                exception = subscribeException;
            }
            finally
            {
                // Again, for sync. reasons.
                this.applicationConferenceSchedulingCompletedEvent.Set();
                if (exception != null)
                {
                    string originator = string.Format("Error when scheduling the conference.");
                    this.RaiseErrorOccured(originator, exception);
                }
            }
        }

        /// <summary>
        /// Occurs when bot joined the conference.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <remarks></remarks>
        private void EndJoinConference(IAsyncResult argument)
        {
            ConferenceSession conferenceSession = argument.AsyncState as ConferenceSession;
            Exception exception = null;
            try
            {
                Debug.WriteLine("Joined the conference");
                conferenceSession.EndJoin(argument);
                Debug.WriteLine(string.Format(
                                              CultureInfo.InvariantCulture,
                                              "Conference Url: conf:{0}%3Fconversation-id={1}",
                                              conferenceSession.ConferenceUri,
                                              conferenceSession.Conversation.Id));
            }
            catch (ConferenceFailureException conferenceFailureException)
            {
                // ConferenceFailureException may be thrown on failures due to MCUs being absent or unsupported, or due to malformed parameters.
                // It is left to the application to perform real error handling here.
                Debug.WriteLine(conferenceFailureException.ToString());
                exception = conferenceFailureException;
            }
            catch (RealTimeException realTimeException)
            {
                // It is left to the application to perform real error handling here.
                Debug.WriteLine(realTimeException.ToString());
                exception = realTimeException;
            }
            finally
            {
                // Again, for sync. reasons.
                this.applicationConferenceJoinCompletedEvent.Set();

                if (exception != null)
                {
                    string originator = string.Format("Error when joining the conference.");
                    this.RaiseErrorOccured(originator, exception);
                }

                // In case Bot was dragged into existing conversation or someone was dragged into existing conversation with Bot; 
                // it will create ad-hoc conference and here is the place where we need to escalate current call into conference.
                if (this.escalateToConference)
                {
                    this.escalateToConference = false;
                    conferenceSession.Conversation.BeginEscalateToConference(this.EndEscalateConference, conferenceSession.Conversation);
                }
            }
        }

        /// <summary>
        /// Ends the call establish.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <remarks></remarks>
        private void EndCallEstablish(IAsyncResult argument)
        {
            Call call = argument.AsyncState as Call;
            Exception exception = null;
            try
            {
                call.EndEstablish(argument);
                Debug.WriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "The call with Local Participant: {0} and Remote Participant: {1} is now in the established state.",
                    call.Conversation.LocalParticipant,
                    call.RemoteEndpoint.Participant));
            }
            catch (OperationFailureException operationFailureException)
            {
                // OperationFailureException: Indicates failure to connect the call to the remote party.
                // It is left to the application to perform real error handling here.
                Debug.WriteLine(operationFailureException.ToString());
                exception = operationFailureException;
            }
            catch (RealTimeException realTimeException)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // It is left to the application to perform real error handling here.
                Debug.WriteLine(realTimeException.ToString());
                exception = realTimeException;
            }
            finally
            {
                // Again, just to sync the completion of the code.
                this.applicationWaitForCallEstablish.Set();

                if (exception != null)
                {
                    string originator = string.Format("Error when establishing call.");
                    this.RaiseErrorOccured(originator, exception);
                }

                this.RaiseConferenceCreatedEvent(call.Conversation.ConferenceSession);
            }
        }

        /// <summary>
        /// Handles the StateChanged event of the Call control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Rtc.Collaboration.CallStateChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks></remarks>
        private void Call_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Call call = sender as Call;
            //Call participants allow for disambiguation.
            Debug.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "The call with Local Participant: {0} has changed state. The previous call state was: {1} and the current state is: {2}",
                call.Conversation.LocalParticipant,
                e.PreviousState,
                e.State));
        }

        /// <summary>
        /// Handles the StateChanged event of the ConferenceSession.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        /// <remarks></remarks>
        private void ConferenceSession_StateChanged(object sender, StateChangedEventArgs<ConferenceSessionState> e)
        {
            ConferenceSession conferenceSession = sender as ConferenceSession;

            //Session participants allow for disambiguation.
            Debug.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "The conference session with Local Participant: {0} has changed state. The previous conference state was: {1}  and the current state is: {2}",
                conferenceSession.Conversation.LocalParticipant,
                e.PreviousState,
                e.State));

        }

        /// <summary>
        /// Handles the InstantMessagingFlowConfigurationRequested event of the InstantMessagingCall control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Rtc.Collaboration.InstantMessagingFlowConfigurationRequestedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Flow created indicates that there is a flow present to begin media operations with, and that it is no longer null.
        /// </remarks>
        private void InstantMessagingCall_InstantMessagingFlowConfigurationRequested(object sender, InstantMessagingFlowConfigurationRequestedEventArgs e)
        {
            InstantMessagingFlow instantMessagingFlow = e.Flow;
            Debug.WriteLine("Flow Created.");
            InstantMessagingCall instantMessagingCall = sender as InstantMessagingCall;

            // Message Received is the event used to indicate that a message has been received from the far end.
            instantMessagingFlow.MessageReceived += this.InstantMessagingFlow_MessageReceived;

            if (!this.messageCalls.ContainsKey(instantMessagingFlow) && !this.conversationFlowMisunderstandingAlerts.ContainsKey(instantMessagingFlow))
            {
                this.messageCalls.Add(instantMessagingFlow, instantMessagingCall);
                this.conversationFlowMisunderstandingAlerts.Add(instantMessagingFlow, false);
            }
        }

        /// <summary>
        /// Handler for the Conversation_EscalateToConferenceRequested event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The EventArgs instance containing the event data.</param>
        private void Conversation_EscalateToConferenceRequested(object sender, EventArgs e)
        {
            Conversation conversation = sender as Conversation;

            // Note that it will not cause any new conference to be created.
            // First, we bind the conference state changed event handler, largely for logging reasons.
            conversation.ConferenceSession.StateChanged += new EventHandler<StateChangedEventArgs<ConferenceSessionState>>(this.ConferenceSession_StateChanged);

            // Next, the prepare the session to escalate, by calling ConferenceSession.BeginJoin on the conversation that received the escalation request.
            // This prepares the calls for actual escalation by binding the appropriate conference multipoint Control Unit (MCU) sessions.
            // You cannot escalate directly in response to an escalation request.
            this.escalateToConference = true;
            conversation.ConferenceSession.BeginJoin(null as ConferenceJoinOptions, this.EndJoinConference, conversation.ConferenceSession);
        }

        /// <summary>
        /// Ends the escalation to conference.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <remarks></remarks>
        private void EndEscalateConference(IAsyncResult argument)
        {
            Conversation conversation = argument.AsyncState as Conversation;
            Exception exception = null;
            try
            {
                conversation.EndEscalateToConference(argument);
                Debug.WriteLine("Conversation was escalated into conference");
            }
            catch (OperationFailureException operationFailureException)
            {
                // OperationFailureException: Indicates failure to connect the call to the remote party.
                // It is left to the application to perform real error handling here.
                Debug.WriteLine(operationFailureException.ToString());
                exception = operationFailureException;
            }
            catch (RealTimeException realTimeException)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // It is left to the application to perform real error handling here.
                Debug.WriteLine(realTimeException.ToString());
                exception = realTimeException;
            }
            finally
            {
                //Again, just to sync the completion of the code.
                this.applicationConferenceJoinCompletedEvent.Set();
                if (exception != null)
                {
                    string originator = string.Format("Error when escalating to conference.");
                    this.RaiseErrorOccured(originator, exception);
                }
            }
        }
        #endregion

        /// <summary>
        /// Handler for the InstantMessagingCall event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The CallReceivedEventArgs instance containing the event data.</param>
        private void InstantMessagingCallHandler(object sender, CallReceivedEventArgs<InstantMessagingCall> e)
        {
            InstantMessagingCall imCall = e.Call as InstantMessagingCall;
            if (imCall != null)
            {
                // Call: StateChanged: Only hooked up for logging.
                e.Call.StateChanged += this.InstantMessagingCall_StateChanged;

                e.Call.InstantMessagingFlowConfigurationRequested += new EventHandler<InstantMessagingFlowConfigurationRequestedEventArgs>(Call_InstantMessagingFlowConfigurationRequested);

                // Accept the call from remote party.
                e.Call.BeginAccept(this.CallAcceptCompleted, e.Call);


                // When an escalation request is received on the existing call, this event handler will be called.
                e.Call.Conversation.EscalateToConferenceRequested += this.Conversation_EscalateToConferenceRequested;
            }

        }

        /// <summary>
        /// When an incoming instant messaging call is received, then this event handler is registered to handle the flow configuration requested event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Call_InstantMessagingFlowConfigurationRequested(object sender, InstantMessagingFlowConfigurationRequestedEventArgs e)
        {
            var imCall = (InstantMessagingCall)sender;

            //Register for message received event of flow.
            e.Flow.MessageReceived += new EventHandler<InstantMessageReceivedEventArgs>(this.InstantMessagingFlow_MessageReceived);

            //Create InstantMessagingFlowTemplate instance to customize it.
            var template = new InstantMessagingFlowTemplate();

            //Set ToastFormatSupport as unsupported. Thus if toast format is not supported, InstantMessagingFlow_MessageReceived will be raised for the first message when an application end point is used.
            //By default, an application end point supports the toast format, so for the first message Messagerecieved event is not raised.
            template.ToastFormatSupport = CapabilitySupport.UnSupported;

            //Overwrite the default settings with the create template instance.
            e.Flow.Initialize(template);

            //Unregister call's InstantMessagingFlowConfigurationRequested.
            imCall.InstantMessagingFlowConfigurationRequested -= this.Call_InstantMessagingFlowConfigurationRequested;

        }


        /// <summary>
        /// Handles the StateChanged event of the InstantMessagingCall.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Rtc.Collaboration.CallStateChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks></remarks>
        private void InstantMessagingCall_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Debug.WriteLine("Call has changed state. The previous call state was: " + e.PreviousState + " and the current state is: " + e.State);
        }

        /// <summary>
        /// Handles the MessageReceived event of the InstantMessagingFlow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Rtc.Collaboration.InstantMessageReceivedEventArgs"/> instance containing the event data.</param>
        private void InstantMessagingFlow_MessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            InstantMessagingFlow instantMessagingFlow = (InstantMessagingFlow)sender;
            string conferenceUri = string.Empty;
            string conversationId = string.Empty;
            if (this.messageCalls.ContainsKey(instantMessagingFlow))
            {
                conferenceUri = this.messageCalls[instantMessagingFlow].Conversation.ConferenceSession.ConferenceUri;
                conversationId = this.messageCalls[instantMessagingFlow].Conversation.Id;
            }

            Message message = new Message(e.TextBody, e.Sender.DisplayName, StringHelper.GetAliasFromSip(e.Sender.UserAtHost), DateTime.Now, conversationId, conferenceUri);

            if (this.MessageReceived != null)
            {
                this.MessageReceived(this, new MessageEventArgs(message));
            }

            Debug.WriteLine(message.SenderAlias + " said: " + message.Content);

            if (!this.bots.ContainsKey(instantMessagingFlow))
            {
                this.RegisterBot(instantMessagingFlow, conferenceUri);
            }

            try
            {
                this.bots[instantMessagingFlow].ProcessMessage(message);
            }

            catch (Exception ex)
            {
                string errorMessage =
                    "Sorry, I'm having trouble to get the requested information because an error happened! "
                    + Emoticons.Sad
                    + Environment.NewLine
                    + "The error was: " + ex.Message;

                this.RaiseErrorOccured(message.Content, ex);
                this.SendReply(instantMessagingFlow, new ReplyMessage(errorMessage));
            }
        }


        /// <summary>
        /// Setups a bot.
        /// </summary>
        /// <param name="instantMessagingFlow">The instant messaging flow.</param>
        /// <param name="conferenceUri">The conference URI.</param>
        private void RegisterBot(InstantMessagingFlow instantMessagingFlow, string conferenceUri)
        {
            this.RegisterBot(this.CreateBot(), instantMessagingFlow, conferenceUri);
        }


        /// <summary>
        /// Setups a bot.
        /// </summary>
        /// <param name="bot">The bot.</param>
        /// <param name="instantMessagingFlow">The instant messaging flow.</param>
        /// <param name="conferenceUri">The conference URI.</param>
        private void RegisterBot(Bot bot, InstantMessagingFlow instantMessagingFlow, string conferenceUri)
        {
            this.bots[instantMessagingFlow] = bot;

            // We need inline anonymous methods since we need to use the local variable instantMessagingFlow.
            this.bots[instantMessagingFlow].FailedToUnderstand +=
                  delegate(object messageHandlingCompletedSender, MessageEventArgs messageEventArgs)
                  {
                      // Need to send reply only if help text is specified. 
                      // If Bot is part of the conference, just need to notify only once.
                      if (!string.IsNullOrWhiteSpace(this.helpText))
                      {
                          bool needToRaiseRepliedEvent = false;
                          // In case Bot didn't understand what user said, it doesn't make sense to show same help message over and over again when users are part of the conference because most likely they are chatting to themselves.
                          // Thus, this will help with understanding when Bot need to show help message in case he didn't understand the user.
                          if (string.IsNullOrWhiteSpace(conferenceUri))
                          {
                              this.SendReply(instantMessagingFlow, new ReplyMessage(this.helpText, ReplyMessage.RtfTextContent));
                              needToRaiseRepliedEvent = true;
                          }
                          else if (this.conversationFlowMisunderstandingAlerts.ContainsKey(instantMessagingFlow) &&
                          !this.conversationFlowMisunderstandingAlerts[instantMessagingFlow])
                          {
                              this.conversationFlowMisunderstandingAlerts[instantMessagingFlow] = true;
                              this.SendReply(instantMessagingFlow, new ReplyMessage(this.helpText, ReplyMessage.RtfTextContent));
                              needToRaiseRepliedEvent = true;
                          }

                          if (needToRaiseRepliedEvent && this.Replied != null)
                          {
                              this.Replied(this, new ReplyEventArgs(new Reply(this.helpText), new Message(this.helpText), ReplyContext.RegularReplyMessage, 0, null));
                          }
                      }
                  };

            this.bots[instantMessagingFlow].Replied +=
                  delegate(object messageHandlingCompletedSender, ReplyEventArgs replyEventArgs)
                  {
                      this.SendReply(instantMessagingFlow, replyEventArgs.Reply);
                      if (this.Replied != null)
                      {
                          this.Replied(this, replyEventArgs);
                      }
                  };
        }

        /// <summary>
        /// Creates the bot.
        /// </summary>
        /// <returns></returns>
        public Bot CreateBot()
        {
            Bot bot = new Bot();
            bot.FeedbackEngine = this.FeedbackEngine;
            return bot;
        }

        /// <summary>
        /// Sends the reply.
        /// </summary>
        /// <param name="instantMessagingFlow">The instant messaging flow.</param>
        /// <param name="reply">The reply.</param>
        private void SendReply(InstantMessagingFlow instantMessagingFlow, Reply reply)
        {
            if (reply != null)
            {
                foreach (ReplyMessage replyMessage in reply.Messages)
                {
                    this.SendReply(instantMessagingFlow, replyMessage);
                }
            }
        }

        /// <summary>
        /// Sends the reply.
        /// </summary>
        /// <param name="instantMessagingFlow">The instant messaging flow.</param>
        /// <param name="replyMessage">The reply message.</param>
        private void SendReply(InstantMessagingFlow instantMessagingFlow, ReplyMessage replyMessage)
        {
            if (replyMessage != null && instantMessagingFlow.State != MediaFlowState.Terminated)
            {
                instantMessagingFlow.BeginSendInstantMessage(replyMessage.ContentType, replyMessage.Content.ToByteArray(), this.SendMessageCompleted, instantMessagingFlow);
            }
        }


        /// <summary>
        /// Outgoing instant messaging call is established.
        /// </summary>
        /// <param name="ar">The IAsyncResult ar.</param>
        private void OutgoingIMCallEstablished(IAsyncResult ar)
        {
            ConversationProperties conversationProperties = ar.AsyncState as ConversationProperties;
            try
            {
                conversationProperties.InstantMessagingCall.EndEstablish(ar);
            }
            catch (Exception exception)
            {
                string originator = "Failed to establish an instant messaging call.";
                this.RaiseErrorOccured(originator, exception);
            }

            InstantMessagingFlow flow = conversationProperties.InstantMessagingCall.Flow;
            this.RegisterBot(flow, string.Empty);
            flow.MessageReceived += this.InstantMessagingFlow_MessageReceived;
            this.bots[flow].ProcessMessage(conversationProperties.Message);
            if (this.bots[flow].MessageHandler != null)
            {
                this.SendReply(flow, conversationProperties.OutgoingMessage);
            }
        }


        /// <summary>
        /// CallAcceptCompleted callback.
        /// </summary>
        /// <param name="result">The result.</param>
        private void CallAcceptCompleted(IAsyncResult result)
        {
            InstantMessagingCall instantMessagingCall = result.AsyncState as InstantMessagingCall;

            Exception ex = null;
            try
            {
               
                    instantMessagingCall.EndAccept(result);
                    Debug.WriteLine("The call is now in the established state.");

                    //changed event handlers name.
                    instantMessagingCall.Flow.StateChanged += new EventHandler<MediaFlowStateChangedEventArgs>(this.InstantMessageFlow_StateChanged);

                    if (!this.messageCalls.ContainsKey(instantMessagingCall.Flow) && !this.conversationFlowMisunderstandingAlerts.ContainsKey(instantMessagingCall.Flow))
                    {
                        this.messageCalls.Add(instantMessagingCall.Flow, instantMessagingCall);
                        this.conversationFlowMisunderstandingAlerts.Add(instantMessagingCall.Flow, false);
                    }
               
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the call to the remote party.
                // It is left to the developer to write real error handling code.
                ex = opFailEx;
            }
            catch (RealTimeException rte)
            {
                // Other errors may cause other RealTimeExceptions to be thrown.
                ex = rte;
            }
            finally
            {
                if (ex != null)
                {
                    string originator = string.Format("Error when establishing call.");
                    this.RaiseErrorOccured(originator, ex);
                }
            }
        }


        /// <summary>
        /// Handles the StateChanged event of the InstantMessageFlow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Rtc.Collaboration.MediaFlowStateChangedEventArgs"/> instance containing the event data.</param>
        private void InstantMessageFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            if (e.State == MediaFlowState.Terminated)
            {
                InstantMessagingFlow instantMessagingFlow = (InstantMessagingFlow)sender;

                if (bots.ContainsKey(instantMessagingFlow))
                {
                    bots.Remove(instantMessagingFlow);
                }
            }
        }


        /// <summary>
        /// SendMessageCompleted callback.
        /// </summary>
        /// <param name="result">The result.</param>
        private void SendMessageCompleted(IAsyncResult result)
        {
            InstantMessagingFlow instantMessagingFlow = result.AsyncState as InstantMessagingFlow;
            Exception ex = null;
            try
            {
                instantMessagingFlow.EndSendInstantMessage(result);
                Debug.WriteLine("The message has been sent.");
            }
            catch (OperationTimeoutException opTimeEx)
            {
                // OperationFailureException: Indicates failure to connect the IM to the remote party due to timeout (called party failed to respond within the expected time).
                // It is left to the developer to write real error handling code.
                ex = opTimeEx;
            }
            catch (RealTimeException rte)
            {
                // Other errors may cause other RealTimeExceptions to be thrown.
                ex = rte;
            }
            finally
            {
                if (ex != null)
                {
                    string originator = "Error sending message.";
                    this.RaiseErrorOccured(originator, ex);
                }
            }
        }

        /// <summary>
        /// Shuts down the platform.
        /// </summary>
        private void ShutdownPlatform()
        {
            this.collabPlatform.BeginShutdown(this.EndPlatformShutdown, this.collabPlatform);
        }

        /// <summary>
        /// EndPlatformShutdown callback.
        /// </summary>
        /// <param name="result">The result.</param>
        private void EndPlatformShutdown(IAsyncResult result)
        {
            CollaborationPlatform collabPlatform = result.AsyncState as CollaborationPlatform;

            // Shutdown actions will not throw.
            collabPlatform.EndShutdown(result);
            Debug.WriteLine("The platform is now shutdown.");
            this.applicationCompletedEvent.Set();
        }

        /// <summary>
        /// Raises the ErrorOccured event.
        /// </summary>
        /// <param name="originator">The originating message (or context) that caused the error.</param>
        /// <param name="ex">The exception.</param>
        private void RaiseErrorOccured(string originator, Exception ex)
        {
            if (this.ErrorOccurred != null)
            {
                this.ErrorOccurred(this, new ErrorEventArgs(originator, ex));
            }

            Debug.WriteLine(originator);
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
        }

        /// <summary>
        /// Raises the conference created event.
        /// </summary>
        /// <param name="conferenceSession">The conference session.</param>
        private void RaiseConferenceCreatedEvent(ConferenceSession conferenceSession)
        {
            if (this.ConferenceCreated != null)
            {
                this.ConferenceCreated(conferenceSession, null);
            }
        }
    }
}
