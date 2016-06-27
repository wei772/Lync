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

namespace Microsoft.Rtc.Collaboration.Sample.DeclineIncomingCall
{
    public class UCMASampleDeclineIncomingCall
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
        /// Instantiate and run the DeclineIncomingCall quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleDeclineIncomingCall ucmaSampleDeclineIncomingCall = new UCMASampleDeclineIncomingCall();
            ucmaSampleDeclineIncomingCall.Run();
        }
        
        private void Run()
        {
            // A helper class to take care of platform and endpoint setup and 
            // cleanup. This has been abstracted from this sample to focus on 
            // Call Control.
            _helper = new UCMASampleHelper();

            // Create a user endpoint, using the network credential object 
            // defined above.
            _userEndpoint = _helper.CreateEstablishedUserEndpoint("DeclineCall Sample User" /*endpointFriendlyName*/);
       
         
            // Here, we are Declining an Instant Messaging call only. If the 
            // incoming call is not an Instant Messaging call (for example, an 
            // AudioVideo call, or a custom (Foo) Call, then it will not get 
            // raised to the application. UCMA 3.0 handles this silently by 
            // having the call types register for various modalities (as part of
            // the extensibility framework). The appropriate action (here, 
            // declining the call) will be handled in the handler assigned to the
            // method call below.
            _userEndpoint.RegisterForIncomingCall<InstantMessagingCall>(On_InstantMessagingCall_Received);

            // Wait for the call to complete Decline, then shutdown the platform.
            Console.WriteLine("Waiting for incoming call...");
            _autoResetEvent.WaitOne();

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to shutdown and exit.");

            // Shut down the sample.
            _helper.ShutdownPlatform();

        }

        void On_InstantMessagingCall_Received(object sender, CallReceivedEventArgs<InstantMessagingCall> e)
        {
            //Type checking was done by the platform; no risk of this being any 
            // type other than the type expected.
            _instantMessagingCall = e.Call;

            // Call: StateChanged: Only hooked up for logging, to show the call 
            // state transitions.
            _instantMessagingCall.StateChanged += new 
                EventHandler<CallStateChangedEventArgs>(_instantMessagingCall_StateChanged);
            
            // Remote Participant URI represents the far end (caller) in this 
            // conversation. Toast is the message set by the caller as the 'greet'
            // message for this call. In Microsoft Lync, the toast will 
            // show up in the lower-right of the screen.
            Console.WriteLine("Call Received! From: " + e.RemoteParticipant.Uri + " Toast is: " + 
                e.ToastMessage.Message);
            
            // Now, decline the call. CallDeclineOptions can be used to supply a
            // response code, here, 486 (Busy Here). Decline is asynchronous, as
            // no reply will be received from the far end.
            _instantMessagingCall.Decline(new CallDeclineOptions(486));

            _autoResetEvent.Set();
        }

        //Just to record the state transitions in the console.
        void _instantMessagingCall_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Console.WriteLine("Call has changed state. The previous call state was: " + e.PreviousState + 
                " and the current state is: " + e.State);
        }
        #endregion
    }
}
