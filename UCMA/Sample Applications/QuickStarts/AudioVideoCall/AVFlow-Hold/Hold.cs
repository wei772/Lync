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

namespace Microsoft.Rtc.Collaboration.Sample.AudioVideoFlowHold
{
    // This demo establishes a basic audio video connection between the user provided below, and the target user, intended to demonstrate the usage of Hold on an audio video call.
    // First, this application places a basic outbound audio video call from the user provided to the far end URI designated below.
    // Note: This sample represents only an outbound call. It is necessary that there be a receiving client logged in as the remote participant (callee).
    // Then, the application waits for the audio video flow to become active (signifying that media operations may commence).
    // The application then places the call on hold, waits until the hold completes, then takes the call off hold.
    // After completing that operation, it will hang up, then tear down the platform and end.
    // (We suggest that you use Microsoft Lync as the target of this application.)

    // This application requires the credentials of Microsoft Lync Server users, enabled for voice, and that UCMA Core SDK be present on this machine.
    // Also, be sure that the users in question can log in to Microsoft Lync Server using Microsoft Lync with the credentials provided, and from the machine that is running this code.
    // Warning: Though the code below makes use of UserEndpoint/user credentials, this is a simplification for ease of use of the sample. For all trusted operations, use ApplicationEndpoint.
    public class UCMABasicAVCall
    {
        private CollaborationPlatform _collabPlatform;
        private AudioVideoFlow _audioVideoFlow;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _waitForShutdownEventCompleted = new AutoResetEvent(false);
        private AutoResetEvent _waitForHoldRetrieveCompleted = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            UCMABasicAVCall BasicAVCall= new UCMABasicAVCall();
            BasicAVCall.Run();
        }

        public void Run()
        {
            // Create AudioVideoFlow
            AudioVideoFlowHelper audioVideoFlowHelper = new AudioVideoFlowHelper();
            _audioVideoFlow = audioVideoFlowHelper.CreateAudioVideoFlow(
                null,
                audioVideoFlow_StateChanged);

            // When something happens involving negotiation this event will be triggered.
            _audioVideoFlow.ConfigurationChanged += new EventHandler<AudioVideoFlowConfigurationChangedEventArgs>(audioVideoFlow_ConfigurationChanged);

            // Attaches a player with a source and starts it in constant loop.
            audioVideoFlowHelper.AttachAndStartPlayer(_audioVideoFlow, true);

            // Check Hold Status.
            Console.WriteLine("AudioVideoFlow's HoldStatus: " + _audioVideoFlow.HoldStatus);
            
            Thread.Sleep(10000);

            // Hold both endpoints synchronously.
            _audioVideoFlow.BeginHold(HoldType.BothEndpoints, audioVideoFlow_HoldCompleted, _audioVideoFlow);
            _waitForHoldRetrieveCompleted.WaitOne();

            // Check Hold Status.
            Console.WriteLine("AudioVideoFlow's HoldStatus: " + _audioVideoFlow.HoldStatus);

            Thread.Sleep(10000);

            // Retrieve AudioVideoFlow synchronously.
            _audioVideoFlow.BeginRetrieve(audioVideoFlow_RetrieveCompleted, _audioVideoFlow);
            _waitForHoldRetrieveCompleted.WaitOne();

            // Check Hold Status.
            Console.WriteLine("AudioVideoFlow's HoldStatus: " + _audioVideoFlow.HoldStatus);

            Thread.Sleep(10000);

            // Shutdown the platform
            ShutdownPlatform();

            //Wait for shutdown to occur.
            _waitForShutdownEventCompleted.WaitOne();        
        }

        private void audioVideoFlow_HoldCompleted(IAsyncResult result)
        {
            try
            {
                AudioVideoFlow avFlow = (AudioVideoFlow)result.AsyncState;
                avFlow.EndHold(result);
            }
            catch (RealTimeException e)
            {
                throw e;
            }

            _waitForHoldRetrieveCompleted.Set();
        }

        private void audioVideoFlow_RetrieveCompleted(IAsyncResult result)
        {
            try
            {
                AudioVideoFlow avFlow = (AudioVideoFlow)result.AsyncState;
                avFlow.EndRetrieve(result);
            }
            catch (RealTimeException e)
            {
                throw e;
            }

            _waitForHoldRetrieveCompleted.Set();
        }

        // Callback that handles when the state of an AudioVideoFlow changes
        private void audioVideoFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            //When flow is active, media operations can begin
            if (e.State == MediaFlowState.Terminated)
            {
                // Detach Player since AVFlow will not work anymore
                AudioVideoFlow avFlow = (AudioVideoFlow)sender;
                if (avFlow.Player != null)
                {
                    avFlow.Player.DetachFlow(avFlow);
                }
            }
        }

        // Callback that is called when the configuration of an AudioVideoFlow changes.
        private void audioVideoFlow_ConfigurationChanged(object sender, AudioVideoFlowConfigurationChangedEventArgs e)
        {
            // application can check AudioVideoFlow values to see the result of the negotiation.
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
