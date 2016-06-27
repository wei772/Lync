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
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.BasicCallHandling
{
    public class UCMASampleBasicCallHandling
    {
        #region Locals
        private UCMASampleHelper _helper;

        private UserEndpoint _userEndpoint;

        private InstantMessagingCall _instantMessagingCall;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the BasicCallHandling quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleBasicCallHandling ucmaSampleBasicCallHandling = new UCMASampleBasicCallHandling();
            ucmaSampleBasicCallHandling.Run();
        }
        
        private void Run()
        {
            // A helper class to take care of platform and endpoint setup and 
            // cleanup. This has been abstracted from this sample to focus on 
            // Call Control.
            _helper = new UCMASampleHelper();

            // Create a user endpoint using the network credential object 
            // defined above. Again, the credentials used must be for a user 
            // enabled for Microsoft Lync Server, and capable of logging
            // in from the machine that is running this code.
            _userEndpoint = _helper.CreateEstablishedUserEndpoint(
                "BasicCall Sample User" /* endpointFriendlyName */);

            // Here, we are accepting an Instant Messaging call only.
            // If the incoming call is not an Instant Messaging call (for example,
            // an AudioVideo call, or a custom Call, then it will not get
            // raised to the application. UCMA 3.0 handles this silently by having
            // the call types register for various modalities (as part of the 
            // extensibility framework). The appropriate action (here, accepting the
            // call) will be handled in the handler assigned to the method call below.
            _userEndpoint.RegisterForIncomingCall<InstantMessagingCall>(On_InstantMessagingCall_Received);

            // Wait for the call to complete accept, then terminate the conversation.
            Console.WriteLine("Waiting for incoming instant messaging call...");
            _autoResetEvent.WaitOne();

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to shutdown and exit.");

            // Terminate the call, the conversation, and then unregister the 
            // endpoint from the receiving an incoming call. Terminating these 
            // additional objects individually is made redundant by shutting down
            // the platform right after, but in the multiple call case, this is 
            // needed for object hygiene. Terminating a Conversation terminates 
            // all it's associated calls, and terminating an endpoint will 
            // terminate all conversations on that endpoint.
            _instantMessagingCall.BeginTerminate(EndTerminateCall, _instantMessagingCall);
            _autoResetEvent.WaitOne();
            _instantMessagingCall.Conversation.BeginTerminate(EndTerminateConversation, 
                                            _instantMessagingCall.Conversation);
            _autoResetEvent.WaitOne();
            _userEndpoint.UnregisterForIncomingCall<InstantMessagingCall>(On_InstantMessagingCall_Received);

            //Now, cleanup by shutting down the platform.
            _helper.ShutdownPlatform();

        }

        void On_InstantMessagingCall_Received(object sender, CallReceivedEventArgs<InstantMessagingCall> e)
        {
            // Type checking was done by the platform; no risk of this being any 
            // type other than the type expected.
            _instantMessagingCall = e.Call;

            // Call: StateChanged: Only hooked up for logging, to show the call
            // state transitions.
            _instantMessagingCall.StateChanged += 
                new EventHandler<CallStateChangedEventArgs>(InstantMessagingCall_StateChanged);
            
            // Remote Participant URI represents the far end (caller) in this 
            // conversation. Toast is the message set by the caller as the 
            // 'greet' message for this call. In Microsoft Lync, the 
            // toast will show up in the lower-right of the screen.
            Console.WriteLine("Call Received! From: " + e.RemoteParticipant.Uri + " Toast is: " + 
                                                e.ToastMessage.Message);
            
            // Now, accept the call. EndAcceptCall will be raised on the 
            // same thread.
            _instantMessagingCall.BeginAccept(EndAcceptCall, _instantMessagingCall);
        }

        private void EndAcceptCall(IAsyncResult ar)
        {
            InstantMessagingCall instantMessagingCall = ar.AsyncState as InstantMessagingCall;
            try
            {
                // Determine whether the IM Call was accepted successfully.
                instantMessagingCall.EndAccept(ar);                    
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer 
                // failures. 
                // TODO: Add actual error handling code here.
                Console.WriteLine(exception.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _autoResetEvent.Set();
            }
        }

        private void EndTerminateCall(IAsyncResult ar)
        {
            InstantMessagingCall instantMessagingCall = ar.AsyncState as InstantMessagingCall;

            // End terminating the incoming call.
            instantMessagingCall.EndTerminate(ar);

            // Remove this event handler now that the call has been terminated.
            _instantMessagingCall.StateChanged -= InstantMessagingCall_StateChanged;

            //Again, just to sync the completion of the code.
            _autoResetEvent.Set();
        }

        private void EndTerminateConversation(IAsyncResult ar)
        {
            Conversation conv = ar.AsyncState as Conversation;

            // End terminating the conversation.
            conv.EndTerminate(ar);

            //Again, just to sync the completion of the code.
            _autoResetEvent.Set();
        }

        //Just to record the state transitions in the console.
        void InstantMessagingCall_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Console.WriteLine("Call has changed state. The previous call state was: " + e.PreviousState + 
                                                    " and the current state is: " + e.State);
        }
        #endregion
    }
}
