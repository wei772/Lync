/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

// .NET namespaces
using System;
using System.Threading;

// UCMA namespaces
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.BasicInstantMessagingCall
{
    public class UCMASampleInstantMessagingCall
    {
        #region Locals
        // The information for the conversation and the far end participant.        
        
        // Subject of the conversation; will appear in the center of the title bar of the 
        // conversation window if Microsoft Lync is the far end client.
        private static String _conversationSubject = "The Microsoft Lync Server!"; 
        
        // Priority of the conversation will appear in the left corner of the title bar of the
        // conversation window if Microsoft Lync is the far end client.
        private static String _conversationPriority = ConversationPriority.Urgent;

        // The Instant Message that will be sent to the far end.
        private static String _messageToSend = "Hello World! I am a bot, and will echo whatever you type. " + 
            "Please send 'bye' to end this application."; 

        private InstantMessagingCall _instantMessagingCall;
        
        private InstantMessagingFlow _instantMessagingFlow;
        
        private UserEndpoint _userEndpoint;
        
        private UCMASampleHelper _helper;

        // Event to notify application main thread on completion of the sample.
        private AutoResetEvent _sampleCompletedEvent = new AutoResetEvent(false);
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the InstantMessagingCall quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleInstantMessagingCall ucmaSampleInstantMessagingCall = 
                                                                        new UCMASampleInstantMessagingCall();
            ucmaSampleInstantMessagingCall.Run();
        }

        private void Run()
        {

            // Initialize and startup the platform.
            Exception ex = null;
            try
            {

                // Create the UserEndpoint

                _helper = new UCMASampleHelper();
                _userEndpoint = _helper.CreateEstablishedUserEndpoint(
                    "IMCall Sample User" /*endpointFriendlyName*/);

                Console.Write("The User Endpoint owned by URI: ");
                Console.Write(_userEndpoint.OwnerUri);
                Console.WriteLine(" is now established and registered.");

                // Setup the conversation and place the call.
                ConversationSettings convSettings = new ConversationSettings();
                convSettings.Priority = _conversationPriority;
                convSettings.Subject = _conversationSubject;

                // Conversation represents a collection of modes of communication
                // (media types)in the context of a dialog with one or multiple 
                // callees.
                Conversation conversation = new Conversation(_userEndpoint, convSettings);
                _instantMessagingCall = new InstantMessagingCall(conversation);

                // Call: StateChanged: Only hooked up for logging. Generally, 
                // this can be used to surface changes in Call state to the UI
                _instantMessagingCall.StateChanged += this.InstantMessagingCall_StateChanged;

                // Subscribe for the flow created event; the flow will be used to
                // send the media (here, IM).
                // Ultimately, as a part of the callback, the messages will be 
                // sent/received.
                _instantMessagingCall.InstantMessagingFlowConfigurationRequested += 
                    this.InstantMessagingCall_FlowConfigurationRequested;

                // Get the sip address of the far end user to communicate with.
                String _calledParty = "sip:" + 
                    UCMASampleHelper.PromptUser(
                    "Enter the URI of the user logged onto Microsoft Lync, in the User@Host format => ", 
                    "RemoteUserURI");

                // Place the call to the remote party, without specifying any 
                // custom options. Please note that the conversation subject 
                // overrides the toast message, so if you want to see the toast 
                // message, please set the conversation subject to null.
                _instantMessagingCall.BeginEstablish(_calledParty, new ToastMessage("Hello Toast"), null, 
                    CallEstablishCompleted, _instantMessagingCall);
            }
            catch (InvalidOperationException iOpEx)
            {
                // Invalid Operation Exception may be thrown if the data provided
                // to the BeginXXX methods was invalid/malformed.
                // TODO (Left to the reader): Write actual handling code here.
                ex = iOpEx;
            }
            finally
            {
                if (ex != null)
                {
                    // If the action threw an exception, terminate the sample, 
                    // and print the exception to the console.
                    // TODO (Left to the reader): Write actual handling code here.
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Shutting down platform due to error");
                    _helper.ShutdownPlatform();
                }
            }

            // Wait for sample to complete
            _sampleCompletedEvent.WaitOne();

        }

        // Just to record the state transitions in the console.
        void InstantMessagingCall_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Console.WriteLine("Call has changed state. The previous call state was: " + e.PreviousState + 
                "and the current state is: " + e.State);
        }

        // Flow created indicates that there is a flow present to begin media 
        // operations with, and that it is no longer null.
        public void InstantMessagingCall_FlowConfigurationRequested(object sender, 
            InstantMessagingFlowConfigurationRequestedEventArgs e)
        {
            Console.WriteLine("Flow Created.");
            _instantMessagingFlow = e.Flow;
            
            // Now that the flow is non-null, bind the event handlers for State 
            // Changed and Message Received. When the flow goes active, 
            // (as indicated by the state changed event) the program will send 
            // the IM in the event handler.
            _instantMessagingFlow.StateChanged += this.InstantMessagingFlow_StateChanged;

            // Message Received is the event used to indicate that a message has
            // been received from the far end.
            _instantMessagingFlow.MessageReceived += this.InstantMessagingFlow_MessageReceived;

            // Also, here is a good place to bind to the 
            // InstantMessagingFlow.RemoteComposingStateChanged event to receive
            // typing notifications of the far end user.
            _instantMessagingFlow.RemoteComposingStateChanged += 
                                                    this.InstantMessagingFlow_RemoteComposingStateChanged;
        }

        private void InstantMessagingFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            Console.WriteLine("Flow state changed from " + e.PreviousState + " to " + e.State);

            // When flow is active, media operations (here, sending an IM) 
            // may begin.
            if (e.State == MediaFlowState.Active)
            {
                // Send the message on the InstantMessagingFlow.
                _instantMessagingFlow.BeginSendInstantMessage(_messageToSend, SendMessageCompleted, 
                    _instantMessagingFlow);
            }
        }

        private void InstantMessagingFlow_RemoteComposingStateChanged(object sender, 
                                                                        ComposingStateChangedEventArgs e)
        {
            // Prints the typing notifications of the far end user.
            Console.WriteLine("Participant " 
                                + e.Participant.Uri.ToString() 
                                + " is " 
                                + e.ComposingState.ToString()
                                );
        }

        private void InstantMessagingFlow_MessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            // On an incoming Instant Message, print the contents to the console.
            Console.WriteLine(e.Sender.Uri + " said: " + e.TextBody);

            // Shutdown if the far end tells us to.
            if (e.TextBody.Equals("bye", StringComparison.OrdinalIgnoreCase))
            {
                // Shutting down the platform will terminate all attached objects.
                // If this was a production application, it would tear down the 
                // Call/Conversation, rather than terminating the entire platform.
                _instantMessagingFlow.BeginSendInstantMessage("Shutting Down...", SendMessageCompleted, 
                    _instantMessagingFlow);
                _helper.ShutdownPlatform();
                _sampleCompletedEvent.Set();
            }
            else
            {
                // Echo the instant message back to the far end (the sender of 
                // the instant message).
                // Change the composing state of the local end user while sending messages to the far end.
                // A delay is introduced purposely to demonstrate the typing notification displayed by the 
                // far end client; otherwise the notification will not last long enough to notice.
                _instantMessagingFlow.LocalComposingState = ComposingState.Composing;
                Thread.Sleep(2000);

                //Echo the message with an "Echo" prefix.
                _instantMessagingFlow.BeginSendInstantMessage("Echo: " + e.TextBody, SendMessageCompleted, 
                    _instantMessagingFlow);
            }

        }

        private void CallEstablishCompleted(IAsyncResult result)
        {
            InstantMessagingCall instantMessagingCall = result.AsyncState as InstantMessagingCall;
            Exception ex = null;
            try
            {
                instantMessagingCall.EndEstablish(result);
                Console.WriteLine("The call is now in the established state.");
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Write real error handling code.
                ex = opFailEx;
            }
            catch (RealTimeException rte)
            {
                // Other errors may cause other RealTimeExceptions to be thrown.
                // TODO (Left to the reader): Write real error handling code.
                ex = rte;
            }
            finally
            {
                if (ex != null)
                {
                    // If the action threw an exception, terminate the sample, 
                    // and print the exception to the console.
                    // TODO (Left to the reader): Write real error handling code.
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Shutting down platform due to error");
                    _helper.ShutdownPlatform();
                }
            }
        }

        private void SendMessageCompleted(IAsyncResult result)
        {
            InstantMessagingFlow instantMessagingFlow = result.AsyncState as InstantMessagingFlow;
            Exception ex = null;
            try
            {
                instantMessagingFlow.EndSendInstantMessage(result);
                Console.WriteLine("The message has been sent.");
            }
            catch (OperationTimeoutException opTimeEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // IM to the remote party due to timeout (called party failed to
                // respond within the expected time).
                // TODO (Left to the reader): Write real error handling code.
                ex = opTimeEx;
            }
            catch (RealTimeException rte)
            {
                // Other errors may cause other RealTimeExceptions to be thrown.
                // TODO (Left to the reader): Write real error handling code.
                ex = rte;
            }
            finally
            {
                // Reset the composing state of the local end user so that the typing notifcation as seen 
                // by the far end client disappears.
                _instantMessagingFlow.LocalComposingState = ComposingState.Idle;
                if (ex != null)
                {
                    // If the action threw an exception, terminate the sample, 
                    // and print the exception to the console.
                    // TODO (Left to the reader): Write real error handling code.
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Shutting down platform due to error");
                    _helper.ShutdownPlatform();
                }
            }
        }
        #endregion
    }
}
