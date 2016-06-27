/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

// .NET namespaces
using System;
using System.Collections.Generic;
using System.Threading;

// UCMA namespaces
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration.ConferenceManagement;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.BasicConferencing
{
    public class UCMASampleBasicConferencing
    {
        #region Locals
        // The IM to send upon joining the MCU.
        private static String _messageToSend = "Hello, World!";

        private Conference _conference;
        
        private UserEndpoint _callerEndpoint, _calleeEndpoint;
        
        private UCMASampleHelper _helper;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _waitForCallEstablish = new AutoResetEvent(false);
        
        private AutoResetEvent _waitForConferenceScheduling = new AutoResetEvent(false);
        
        private AutoResetEvent _waitForConferenceJoin = new AutoResetEvent(false);
        
        private AutoResetEvent _waitForMessageReceived = new AutoResetEvent(false);
        
        private AutoResetEvent _waitForMessage2Received = new AutoResetEvent(false);
        
        private AutoResetEvent _waitForShutdown = new AutoResetEvent(false);
        
        private AutoResetEvent _waitForConversationInviteRemoteParticipants = new AutoResetEvent(false);
        
        private InstantMessagingFlow _IMFlow;
        
        private InstantMessagingFlow _IMFlow2;
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the BasicConferencing quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleBasicConferencing ucmaSampleBasicConferencing = new UCMASampleBasicConferencing();
            ucmaSampleBasicConferencing.Run();
        }

        private void Run()
        {
            // A helper class to take care of platform and endpoint setup and
            // cleanup. This has been abstracted from this sample to focus on 
            // Call Control.
            _helper = new UCMASampleHelper();

            // Create a user endpoint, using the network credential object
            // defined above.
            _callerEndpoint = _helper.CreateEstablishedUserEndpoint(
                "Conference Leader" /* friendly name for conference leader endpoint */);

            // Create a second user endpoint, using the network credential object
            // defined above.
            _calleeEndpoint = _helper.CreateEstablishedUserEndpoint(
                "Conference Attendee" /* friendly name for conference attendee endpoint */);

            // Get the URI for the user logged onto Microsoft Lync
            String _ocUserURI = "sip:" + UCMASampleHelper.PromptUser(
                "Enter the URI for the user logged onto Microsoft Lync, in the User@Host format => ",
                "RemoteUserURI" /* key to specify remote user's URI in app.config */);

            // One of the endpoints schedules the conference in advance. At 
            // schedule time, all the conference settings are set.

            // The base conference settings object, used to set the policies for the conference.
            ConferenceScheduleInformation conferenceScheduleInformation = new ConferenceScheduleInformation();
            // An open meeting (participants can join who are not on the list), 
            // but requiring authentication (no anonymous users allowed.)
            conferenceScheduleInformation.AccessLevel = ConferenceAccessLevel.SameEnterprise;
            // The below flag determines whether or not the passcode is optional
            // for users joining the conference.
            conferenceScheduleInformation.IsPasscodeOptional = true; 
            conferenceScheduleInformation.Passcode = "1357924680";
            // The verbose description of the conference
            conferenceScheduleInformation.Description = "Interesting Description";
            // The below field indicates the date and time after which the conference can be deleted.
            conferenceScheduleInformation.ExpiryTime = System.DateTime.Now.AddHours(5); 

            // These two lines assign a set of modalities (here, only 
            // InstantMessage) from the available MCUs to the conference. Custom
            // modalities (and their corresponding MCUs) may be added at this 
            // time as part of the extensibility model.
            ConferenceMcuInformation instantMessageMCU = new ConferenceMcuInformation(McuType.InstantMessaging);
            conferenceScheduleInformation.Mcus.Add(instantMessageMCU); 

            // Now that the setup object is complete, schedule the conference 
            // using the conference services off of Endpoint. Note: the conference
            // organizer is considered a leader of the conference by default.
            _callerEndpoint.ConferenceServices.BeginScheduleConference(conferenceScheduleInformation,
                EndScheduleConference, _callerEndpoint.ConferenceServices);

            // Wait for the scheduling to complete.
            _waitForConferenceScheduling.WaitOne();

            // Now that the conference is scheduled, it's time to join it. As we
            // already have a reference to the conference object populated from
            // the EndScheduleConference call, we do not need to get the 
            // conference first. Initialize a conversation off of the endpoint, 
            // and join the conference from the uri provided above.
            Conversation callerConversation = new Conversation(_callerEndpoint);
            callerConversation.ConferenceSession.StateChanged += new 
                EventHandler<StateChangedEventArgs<ConferenceSessionState>>(ConferenceSession_StateChanged);

            // Join and wait, again forcing synchronization.
            callerConversation.ConferenceSession.BeginJoin(_conference.ConferenceUri, null /*joinOptions*/,
                EndJoinConference, callerConversation.ConferenceSession);
            _waitForConferenceJoin.WaitOne();

            // Placing the calls on the conference-connected conversation 
            // connects to the respective MCUs. These calls may then be used to
            // communicate with the conference/MCUs.
            InstantMessagingCall instantMessagingCall = new InstantMessagingCall(callerConversation);

            // Hooking up event handlers and then placing the call.
            instantMessagingCall.InstantMessagingFlowConfigurationRequested += 
                this.instantMessagingCall_InstantMessagingFlowConfigurationRequested;
            instantMessagingCall.StateChanged += this._call_StateChanged;
            instantMessagingCall.BeginEstablish(EndCallEstablish, instantMessagingCall);

            //Synchronize to ensure that call has completed.
            _waitForCallEstablish.WaitOne();

            //send conf invite
            ConferenceInvitationDeliverOptions deliverOptions = new ConferenceInvitationDeliverOptions();
            deliverOptions.ToastMessage = new ToastMessage("Welcome to my conference");

            ConferenceInvitation invitation = new ConferenceInvitation(callerConversation);
            invitation.BeginDeliver(_ocUserURI, deliverOptions, EndDeliverInvitation, invitation);

            // Synchronize to ensure that invitation is complete
            _waitForConversationInviteRemoteParticipants.WaitOne();

            //And from the other endpoint's perspective:
            //Initialize a conversation off of the endpoint, and join the 
            //conference from the uri provided above.
            Conversation calleeConversation = new Conversation(_calleeEndpoint);
            calleeConversation.ConferenceSession.StateChanged += new 
                EventHandler<StateChangedEventArgs<ConferenceSessionState>>(ConferenceSession_StateChanged);

            // Join and wait, again forcing synchronization.
            calleeConversation.ConferenceSession.BeginJoin(_conference.ConferenceUri, null /*joinOptions*/, 
                EndJoinConference, calleeConversation.ConferenceSession);
            _waitForConferenceJoin.WaitOne();

            // Placing the calls on the conference-connected conversation 
            // connects to the respective MCUs. These calls may then be used to
            //communicate with the conference/MCUs.
            InstantMessagingCall instantMessagingCall2 = new InstantMessagingCall(calleeConversation);

            //Hooking up event handlers and then placing the call.
            instantMessagingCall2.InstantMessagingFlowConfigurationRequested += 
                this.instantMessagingCall2_InstantMessagingFlowConfigurationRequested;
            instantMessagingCall2.StateChanged += this._call_StateChanged;
            instantMessagingCall2.BeginEstablish(EndCallEstablish, instantMessagingCall2);

            //Synchronize to ensure that call has completed.
            _waitForCallEstablish.WaitOne();

            //Synchronize to ensure that all messages are sent and received
            _waitForMessageReceived.WaitOne();

            //Wait for shutdown initiated by user
            _waitForShutdown.WaitOne();

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to shutdown and exit.");
        }

        //Just to record the state transitions in the console.
        void ConferenceSession_StateChanged(object sender, StateChangedEventArgs<ConferenceSessionState> e)
        {
            ConferenceSession confSession = sender as ConferenceSession;

            //Session participants allow for disambiguation.
            Console.WriteLine("The conference session with Local Participant: " + 
                confSession.Conversation.LocalParticipant + " has changed state. " + 
                "The previous conference state was: " + e.PreviousState + 
                " and the current state is: " + e.State);
        }

        // Flow created indicates that there is a flow present to begin media 
        // operations with, and that it is no longer null.
        public void instantMessagingCall_InstantMessagingFlowConfigurationRequested
            (object sender, InstantMessagingFlowConfigurationRequestedEventArgs e)
        {
            InstantMessagingFlow instantMessagingFlow = sender as InstantMessagingFlow;
            Console.WriteLine("Caller's Flow Created.");
            instantMessagingFlow = e.Flow;
            _IMFlow = instantMessagingFlow;

            // Now that the flow is non-null, bind the event handlers for State
            // Changed and Message Received. When the flow goes active, (as 
            // indicated by the state changed event) the program will send the 
            // IM in the event handler.
            instantMessagingFlow.StateChanged += this.instantMessagingFlow_StateChanged;

            // Message Received is the event used to indicate that a message has
            // been received from the far end.
            instantMessagingFlow.MessageReceived += this.instantMessagingFlow_MessageReceived;
        }

        // Flow created indicates that there is a flow present to begin media
        // operations with, and that it is no longer null.
        public void instantMessagingCall2_InstantMessagingFlowConfigurationRequested(
            object sender, InstantMessagingFlowConfigurationRequestedEventArgs e)
        {
            InstantMessagingFlow instantMessagingFlow = sender as InstantMessagingFlow;
            Console.WriteLine("Callee's Flow Created.");
            instantMessagingFlow = e.Flow;
            _IMFlow2 = instantMessagingFlow;

            // Now that the flow is non-null, bind the event handlers for State 
            // Changed and Message Received. When the flow goes active, the 
            // program will send the IM in the event handler.
            instantMessagingFlow.StateChanged += this.instantMessagingFlow2_StateChanged;

            // Message Received is the event used to indicate that a message 
            // from the far end has been received.
            instantMessagingFlow.MessageReceived += this.instantMessagingFlow2_MessageReceived;
        }

        private void instantMessagingFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            InstantMessagingFlow instantMessagingFlow = sender as InstantMessagingFlow;

            Console.WriteLine("Flow state changed from " + e.PreviousState + " to " + e.State);

            //When flow is active, media operations (here, sending an IM) may begin.
            if (e.State == MediaFlowState.Active)
            {
                _IMFlow = instantMessagingFlow;
                Console.WriteLine("Please type the message to send...");
                string msg = Console.ReadLine();
                //Send the message on the InstantMessagingFlow.
                instantMessagingFlow.BeginSendInstantMessage(msg, EndSendMessage, instantMessagingFlow);
            }
        }

        private void instantMessagingFlow2_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            InstantMessagingFlow instantMessagingFlow = sender as InstantMessagingFlow;

            Console.WriteLine("Flow state changed from " + e.PreviousState + " to " + e.State);

            //When flow is active, media operations (here, sending an IM) may begin.
            if (e.State == MediaFlowState.Active)
            {
                _IMFlow2 = instantMessagingFlow;
            }
        }

        private void EndSendMessage(IAsyncResult ar)
        {
            InstantMessagingFlow instantMessagingFlow = ar.AsyncState as InstantMessagingFlow;
            try
            {
                instantMessagingFlow.EndSendInstantMessage(ar);
                Console.WriteLine("The message has been sent.");
            }
            catch (OperationTimeoutException opTimeEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // IM to the remote party due to timeout (called party failed 
                // to respond within the expected time).
                // TODO (Left to the reader): Add error handling code
                Console.WriteLine(opTimeEx.ToString());
            }
        }
        private void instantMessagingFlow_MessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            InstantMessagingFlow instantMessagingFlow = sender as InstantMessagingFlow;
            //On an incoming Instant Message, print the contents to the console.
            Console.WriteLine("In caller's message handler: " + e.Sender.DisplayName + " said: " + e.TextBody);
            _waitForMessageReceived.Set();
        }

        private void instantMessagingFlow2_MessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            InstantMessagingFlow instantMessagingFlow = sender as InstantMessagingFlow;
            //On an incoming Instant Message, print the contents to the console.
            Console.WriteLine("In callee's message handler: " + e.Sender.DisplayName + " said: " + e.TextBody);

            //Shutdown the platform
            if (e.TextBody.Equals("bye", StringComparison.OrdinalIgnoreCase))
            {
                _helper.ShutdownPlatform();
                _waitForShutdown.Set();
                return;
            }

            Console.WriteLine("Message received will be echoed");
            _messageToSend = "echo: " + e.TextBody;
            //Send the message on the InstantMessagingFlow.
            if (_IMFlow2 != null && _IMFlow2.State == MediaFlowState.Active)
            {
                _IMFlow2.BeginSendInstantMessage(_messageToSend, EndSendMessage, instantMessagingFlow);
            }
            else
                Console.WriteLine("Could not echo message because flow was either null or inactive");

            _waitForMessage2Received.Set();
        }

        private void EndCallEstablish(IAsyncResult ar)
        {
            Call call = ar.AsyncState as Call;
            try
            {
                call.EndEstablish(ar);
                Console.WriteLine("The call with Local Participant: " + call.Conversation.LocalParticipant + 
                    " and Remote Participant: " + call.RemoteEndpoint.Participant + 
                    " is now in the established state.");
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer 
                //failures.
                // TODO (Left to the reader): Add error handling code
                Console.WriteLine(exception.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _waitForCallEstablish.Set();
            }
        }

        private void EndDeliverInvitation(IAsyncResult ar)
        {
            ConferenceInvitation invitation = ar.AsyncState as ConferenceInvitation;

            try
            {
                invitation.EndDeliver(ar);
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // TODO (Left to the reader): Add error handling code
                Console.WriteLine(exception.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _waitForConversationInviteRemoteParticipants.Set();
            }
        }

        private void EndScheduleConference(IAsyncResult ar)
        {
            ConferenceServices confSession = ar.AsyncState as ConferenceServices;
            try
            {   
                //End schedule conference returns the conference object, which 
                // contains the vast majority of the data relevant to that
                // conference.
                _conference = confSession.EndScheduleConference(ar);
                Console.WriteLine("");
                Console.WriteLine(" The conference is now scheduled.");
                Console.WriteLine("");

            }
            catch (ConferenceFailureException confFailEx)
            {
                // ConferenceFailureException may be thrown on failures to 
                // schedule due to MCUs being absent or unsupported, or due to
                // malformed parameters.
                // TODO (Left to the reader): Add error handling code
                Console.WriteLine(confFailEx.ToString());
            }

            //Again, for sync. reasons.
            _waitForConferenceScheduling.Set();
        }
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
                // TODO (Left to the reader): Add error handling code
                Console.WriteLine(confFailEx.ToString());
            }
            catch (RealTimeException rTEx)
            {
                // TODO (Left to the reader): Add error handling code
                Console.WriteLine(rTEx.ToString());
            }
            finally
            {
                //Again, for sync. reasons.
                _waitForConferenceJoin.Set();             
            }
        }

        //Just to record the state transitions in the console.
        void _call_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Call call = sender as Call;

            //Call participants allow for disambiguation.
            Console.WriteLine("The call with Local Participant: " + call.Conversation.LocalParticipant +
                " has changed state. The previous call state was: " + e.PreviousState + 
                " and the current state is: " + e.State);
        }
        #endregion
    }
    
}
