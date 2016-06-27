/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   	*
*                                                       *
********************************************************/

using System;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.AudioVideoFlowMute
{
    // This demo establishes a basic audio video connection between the user provided below, and the target user, intended to demonstrate the usage of Mute on an audio video call.
    // First, this application places a basic outbound audio video call from the user provided to the far end URI designated below.
    // Note: This sample only represents an outbound call. it is necessary that there be a recieving client logged in as the far end participant (callee).
    // Then, the application waits for the audio video flow to become active (signifying that media operations may commence).
    // The application then places the call on mute, waits until the mute completes, then unmutes the call.
    // After completing that operation, it will hang up, then teardown the platform and end.
    // (We suggest you  use Microsoft Lync as the target of this application.)

    // This application requires the credentials of Microsoft Lync Server users, enabled for voice, and that UCMA v 2.0 be present on this machine.
    // Also, be sure that the users in question can log in to Microsoft Lync Server using Microsoft Lync with the credentials provided, and from the machine that is running this code.    
    // Warning: Though the code below makes use of UserEndpoint/user credentials, this is a simplification for ease of use of the sample. For all trusted operations, use ApplicationEndpoint.
    public class UCMABasicAVCall
    {
        private CollaborationPlatform _collabPlatform;
        private AudioVideoFlow _audioVideoFlow;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _waitForShutdownEventCompleted = new AutoResetEvent(false);

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

            // Check direction muted
            Console.WriteLine("AudioVideoFlow's direction muted: " + _audioVideoFlow.Audio.DirectionMuted);

            Thread.Sleep(10000);

            // Mute both directions
            _audioVideoFlow.Audio.Mute(MuteDirection.SendReceive);

            // Check direction muted
            Console.WriteLine("AudioVideoFlow's direction muted: " + _audioVideoFlow.Audio.DirectionMuted);

            Thread.Sleep(10000);

            // Unmute both directions
            _audioVideoFlow.Audio.Unmute(MuteDirection.SendReceive);

            // Check direction muted
            Console.WriteLine("AudioVideoFlow's direction muted: " + _audioVideoFlow.Audio.DirectionMuted);

            Thread.Sleep(10000);

            // Shutdown the platform
            ShutdownPlatform();

            //Wait for shutdown to occur.
            _waitForShutdownEventCompleted.WaitOne();
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
