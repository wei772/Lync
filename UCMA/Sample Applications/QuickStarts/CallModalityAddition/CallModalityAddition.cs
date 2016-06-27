/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

// .NET namespaces
using System;
using System.Configuration;
using System.Threading;

// UCMA namespaces
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;


namespace Microsoft.Rtc.Collaboration.Sample.CallModalityAddition
{
    public class UCMASampleCallModalityAddition
    {
        #region Locals
        // Reference to UCMASampleHelper.
        private UCMASampleHelper _helper;

        //Wait handles are only present to keep things synchronous and easy to read.        
        private AutoResetEvent _sampleCompleted = new AutoResetEvent(false);

        //Conversation for multiple modality
        private Conversation _conversation = null;
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the CallModalityAddition quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleCallModalityAddition ucmaSampleCallModalityAddition = new UCMASampleCallModalityAddition();
            ucmaSampleCallModalityAddition.Run();
        }

        private void Run()
        {
            // A helper class to take care of platform and endpoint setup and 
            // cleanup. This has been abstracted from this sample to focus on 
            // Call Control.
            _helper = new UCMASampleHelper();

            // Create a user endpoint, using the network credential object 
            // defined above. 
            UserEndpoint callerEndpoint = _helper.CreateEstablishedUserEndpoint(
                "Caller" /*endpointFriendlyName*/);

            // Create a second user endpoint, using the network credential object
            // defined above.
            UserEndpoint calleeEndpoint = _helper.CreateEstablishedUserEndpoint(
                "Callee" /*endpointFriendlyName*/);

            // Here, we are accepting an Instant Messaging call only. If the 
            // incoming call is not an Instant Messaging call (for example, an 
            // AudioVideo call or a custom Call, then it will not get raised to 
            // the application. UCMA 3.0 handles this silently by having the call
            // types register for various modalities (as part of the extensibility
            // framework). The appropriate action (here, accepting the call) will
            // be handled in the handler assigned to the method call below.
            calleeEndpoint.RegisterForIncomingCall<InstantMessagingCall>(On_InstantMessagingCall_Received);
            
            // Setup the call and conversation objects for the initial call (IM)
            // and place the call (synchronously).
            _conversation = new Conversation(callerEndpoint);
            InstantMessagingCall instantMessagingCall = new InstantMessagingCall(_conversation);


            // Add registration for the AudioVideo modality, before placing the 
            // second call. This could have been done at any time before placing
            // the audio video call. This handler could choose to accept, deflect
            // or drop this portion of the call entirely.
            calleeEndpoint.RegisterForIncomingCall<AudioVideoCall>(On_AudioVideoCall_Received);

            // Place the call to the remote party, without specifying any custom 
            // options.
            instantMessagingCall.BeginEstablish(calleeEndpoint.OwnerUri, new ToastMessage("Sample Toast Message"),
                null, CallEstablishCompleted, instantMessagingCall);

            // Force synchronization to ensure that the both AVCall and IMCall 
            // are now complete.
            _sampleCompleted.WaitOne();

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to shutdown and exit.");

            //And shutdown (synchronously).
            Console.WriteLine("Shutting down the sample...");
            _helper.ShutdownPlatform();

        }

         void On_InstantMessagingCall_Received(object sender, CallReceivedEventArgs<InstantMessagingCall> e)
        {
            // Type checking was done by the platform; no risk of this being any 
            // type other than the type expected.
            InstantMessagingCall instantMessagingCall = e.Call;

            // Call: StateChanged: Only hooked up for logging, to show the call 
            // state transitions.
            instantMessagingCall.StateChanged += new EventHandler<CallStateChangedEventArgs>(Call_StateChanged);
            
            // Remote Participant URI represents the far end (caller) in this 
            // conversation. Toast is the message set by the caller as the 'greet'
            // message for this call. In Microsoft Lync, the toast will 
            // show up in the lower-right of the screen. Conversation ID 
            // demonstrates that the two calls inhabit the same conversation 
            // container on both ends of the call.
            Console.WriteLine("");
            Console.WriteLine(" Instant Messaging Call Received! From: " + e.RemoteParticipant.Uri);
            Console.WriteLine(" Toast is: " + e.ToastMessage.Message);
            Console.WriteLine(" Conversation ID is: " + e.Call.Conversation.Id);
            Console.WriteLine("");
            
            // Now, accept the call.
            // AcceptCallCompleted will be raised on the same thread.
            instantMessagingCall.BeginAccept(AcceptCallCompleted, instantMessagingCall);
        }

         void On_AudioVideoCall_Received(object sender, CallReceivedEventArgs<AudioVideoCall> e)
         {
             // Type checking was done by the platform; no risk of this being any
             // type other than the type expected.
             AudioVideoCall _audioVideoCall = e.Call;

             // Call: StateChanged: Only hooked up for logging, to show the call
             // state transitions. Only bound on the incoming side, to avoid 
             // printing the events twice.
             _audioVideoCall.StateChanged += this.Call_StateChanged;

             // Remote Participant URI represents the far end (caller) in this 
             // conversation. Toast is the message set by the caller as the 'greet'
             // message for this call. In Microsoft Lync, the toast will
             // show up in the lower-right of the screen. 
             Console.WriteLine("");
             Console.WriteLine(" Audio Video Call Received! From: " + e.RemoteParticipant.Uri);
             Console.WriteLine(" Toast is: " + e.ToastMessage.Message);
             Console.WriteLine(" Conversation ID is: " + e.Call.Conversation.Id);
             Console.WriteLine("");

             try
             {
                 // Now, accept the call. Threading note: AcceptCallCompleted will
                 // be raised on the same thread. Blocking this thread in this 
                 // portion of the code will cause endless waiting.
                 _audioVideoCall.BeginAccept(AcceptCallCompleted, _audioVideoCall);
             }
             catch (InvalidOperationException exception)
             {
                 // InvalidOperationException indicates that the call was 
                 // disconnected before it could be accepted.
                 Console.WriteLine(exception.ToString());
             }
         }

        private void AcceptCallCompleted(IAsyncResult ar)
        {
            Call call = ar.AsyncState as Call;
            
            // End accepting the incoming call.
            call.EndAccept(ar);
        }
        
        private void CallEstablishCompleted(IAsyncResult ar)
        {
            Call call = ar.AsyncState as Call;
            RealTimeException ex = null;
            try
            {
                call.EndEstablish(ar);
                Console.WriteLine("The call with Local Participant: " + call.Conversation.LocalParticipant + 
                    " and Remote Participant: " + call.RemoteEndpoint.Participant + 
                    " is now in the established state.");
                // Setup the call object for the second call (AV), reusing the 
                // original conversation, and place the call (synchronously).
                // Reuse of the conversation allows for effortless modality 
                // management. Either endpoint could add the new modality and 
                // place the call below.
                AudioVideoCall audioVideoCall = new AudioVideoCall(_conversation);
                audioVideoCall.BeginEstablish(AVCallEstablishCompleted, audioVideoCall);
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party. 
                // TODO (Left to the reader): Add error handling code here
                ex = opFailEx;                
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // TODO (Left to the reader): Add error handling code here
                ex = exception;                
            }
            finally
            {
                if (ex != null)
                {
                    Console.WriteLine("Completing application with error");
                    Console.WriteLine(ex.ToString());
                    _sampleCompleted.Set();
                }
            }
        }

        private void AVCallEstablishCompleted(IAsyncResult ar)
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
                // TODO (Left to the reader): Add error handling code here
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // TODO (Left to the reader): Add error handling code here
                Console.WriteLine(exception.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _sampleCompleted.Set();
            }
        }
        //Just to record the state transitions in the console.
        void Call_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Call call = sender as Call;

            //Call participants allow for disambiguation.
            Console.WriteLine("The call with Local Participant: " + call.Conversation.LocalParticipant + 
                " and Remote Participant: " + call.RemoteEndpoint.Participant + 
                " has changed state. The previous call state was: " + e.PreviousState + 
                " and the current state is: " + e.State);
        }
        #endregion
    }
}
