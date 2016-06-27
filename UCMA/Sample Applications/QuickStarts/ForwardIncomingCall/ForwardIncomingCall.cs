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

namespace Microsoft.Rtc.Collaboration.Sample.ForwardIncomingCall
{
    public class UCMASampleForwardIncomingCall
    {
        #region Locals
        //The URI and connection server of the user to forward the call to.
        private String _forwardUserURI;
        
        private UCMASampleHelper _helper;

        private UserEndpoint _userEndpoint;

        private AudioVideoCall _audioVideoCall;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the ForwardIncomingCall quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleForwardIncomingCall ucmaSampleForwardIncomingCall = new UCMASampleForwardIncomingCall();
            ucmaSampleForwardIncomingCall.Run();
        }
        
        private void Run()
        {
            // A helper class to take care of platform and endpoint setup and 
            // cleanup. This has been abstracted from this sample to focus on 
            // Call Control.
            _helper = new UCMASampleHelper();

            // Create a user endpoint, using the network credential object 
            // defined above.
            _userEndpoint = _helper.CreateEstablishedUserEndpoint("Forwarding User" /*endpointFriendlyName*/);

            // Enter the URI of the user to forward the call to.
            _forwardUserURI = UCMASampleHelper.PromptUser(
                "Enter the URI of the user to forward the incoming call to, in the User@Host format => ", 
                "ForwardingTargetURI");
            if (!(_forwardUserURI.ToLower().StartsWith("sip:") || _forwardUserURI.ToLower().StartsWith("tel:")))
                _forwardUserURI = "sip:" + _forwardUserURI;

            // Here, we are dealing with an Audio Video call only. If the 
            // incoming call is not of the media type expected, then it will not
            // get raised to the application. UCMA 3.0 handles this silently by 
            // having the call types register for various modalities (as part of
            // the extensibility framework). The appropriate action (here, 
            // forwarding the call) will be handled in the handler assigned to the 
            // method call below.
            _userEndpoint.RegisterForIncomingCall<AudioVideoCall>(On_AudioVideoCall_Received);

            // Wait for the call to complete forward, then shutdown the platform.
            Console.WriteLine("Waiting for incoming call...");
            _autoResetEvent.WaitOne();

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to shutdown and exit.");

            // Shut down the sample.
            _helper.ShutdownPlatform();

        }

        void On_AudioVideoCall_Received(object sender, CallReceivedEventArgs<AudioVideoCall> e)
        {
            //Type checking was done by the platform; no risk of this being any 
            // type other than the type expected.
            _audioVideoCall = e.Call;

            // Call: StateChanged: Only hooked up for logging, to show the call 
            // state transitions.
            _audioVideoCall.StateChanged += new 
                EventHandler<CallStateChangedEventArgs>(_audioVideoCall_StateChanged);
            
            // Remote Participant URI represents the far end (caller) in this 
            // conversation. Toast is the message set by the caller as the 'greet'
            // message for this call. In Microsoft Lync, the toast will 
            // show up in the lower-right of the screen.
            Console.WriteLine("Call Received! From: " + e.RemoteParticipant.Uri + " Toast is: " + 
                e.ToastMessage.Message);
            
            // Now, forward the call to the user given above. Forwarding is not 
            // an async operation; it completes as soon as the message is sent, 
            // without waiting for a reply from the far end.
            _audioVideoCall.Forward(_forwardUserURI);
            _autoResetEvent.Set();

        }

        //Just to record the state transitions in the console.
        void _audioVideoCall_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Console.WriteLine("Call has changed state. The previous call state was: " + e.PreviousState + 
                " and the current state is: " + e.State);
        }
        #endregion
    }
}
