/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   	*
*                                                       *
********************************************************/

using System;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.ToneControllerBasic
{
    // This demo represents some basic DTMF tone reception and recognition. It begins by establishing an outbound audio connection with the called party, 
    // (Which must be a currently logged-on client), then keeps the call alive receiving tones until a zero or fax tone is received.
    // For each tone received an equal tone will be sent it back, except for zero and the fax tone which will disconnect the call.
    // After completing that operation, it will hang up, then teardown the platform and end.

    // This application requires the credentials of Microsoft Lync Server users, enabled for voice, and that UCMA be present on this machine.
    // Also, be sure that the users in question can log in to Microsoft Lync Server using Microsoft Lync with the credentials provided, and from the machine that is running this code.
    // Warning: Though the code below makes use of UserEndpoint/user credentials, this is a simplification for ease of use of the sample. For all trusted operations, use ApplicationEndpoint.
    public class UCMAToneControllerBasic
    {
        private CollaborationPlatform _collabPlatform;
        private AudioVideoFlow _audioVideoFlow;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _waitForShutdownEventCompleted = new AutoResetEvent(false);
        private AutoResetEvent _waitForToneReceivedEventCompleted = new AutoResetEvent(false);
        
        static void Main(string[] args)
        {
            UCMAToneControllerBasic BasicAVCall = new UCMAToneControllerBasic();
            BasicAVCall.Run();
        }

        public void Run()
        {
            // Create AudioVideoFlow
            AudioVideoFlowHelper audioVideoFlowHelper = new AudioVideoFlowHelper();
            _audioVideoFlow = audioVideoFlowHelper.CreateAudioVideoFlow(
                null,
                audioVideoFlow_StateChanged);

            // Create a ToneController and attach to AVFlow
            ToneController toneController = new ToneController();
            toneController.AttachFlow(_audioVideoFlow);

            // Subscribe to callback to receive tones
            toneController.ToneReceived += new EventHandler<ToneControllerEventArgs>(toneController_ToneReceived);

            // Subscribe to callback to receive fax tones
            toneController.IncomingFaxDetected += new EventHandler<IncomingFaxDetectedEventArgs>(toneController_IncomingFaxDetected);

            Console.WriteLine("ToneController attached. Send Zero or a Fax Tone to disconnect the call.");

            //Sync; wait for ToneReceivedEvent
            _waitForToneReceivedEventCompleted.WaitOne();

            // Shutdown the platform
            ShutdownPlatform();

            //Wait for shutdown to occur.
            _waitForShutdownEventCompleted.WaitOne();
        }

        // Callback that handles when a tone is received
        void toneController_ToneReceived(object sender, ToneControllerEventArgs e)
        {
            Console.WriteLine("Tone Received: " + (ToneId)e.Tone + " (" + e.Tone + ")");
            if ((ToneId)e.Tone == ToneId.Tone0)
            {
                _waitForToneReceivedEventCompleted.Set();
            }
            else
            {
                ToneController tc = (ToneController)sender;
                tc.Send(e.Tone);
            }
        }

        // Callback that handles when a fax tone is received
        void toneController_IncomingFaxDetected(object sender, IncomingFaxDetectedEventArgs e)
        {
            Console.WriteLine("Fax Tone Received");
            _waitForToneReceivedEventCompleted.Set();
        }

        // Callback that handles when the state of an AudioVideoFlow changes
        private void audioVideoFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            //When flow is active, media operations can begin
            if (e.State == MediaFlowState.Terminated)
            {
                // Detach ToneController since AVFlow will not work anymore
                AudioVideoFlow avFlow = (AudioVideoFlow)sender;
                if (avFlow.ToneController != null)
                {
                    avFlow.ToneController.DetachFlow();
                }
            }
        }

        private void ShutdownPlatform()
        {
            // Shutdown the platform.     
            _collabPlatform = _audioVideoFlow.Call.Conversation.Endpoint.Platform;
            _collabPlatform.BeginShutdown(EndPlatformShutdown, _collabPlatform);
        }

        private void EndPlatformShutdown(IAsyncResult ar)
        {
            CollaborationPlatform collabPlatform = ar.AsyncState as CollaborationPlatform;

            //Shutdown actions will not throw.
            collabPlatform.EndShutdown(ar);
            Console.WriteLine("The platform is now shutdown.");

            //Again, just to sync the completion of the code and the platform teardown.
            _waitForShutdownEventCompleted.Set();
        }
    }
}
