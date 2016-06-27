/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   	*
*                                                       *
********************************************************/


using System;
using System.Threading;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.AudioVideoFlowApplyChanges
{
    // This demo establishes a basic audio video connection between the user provided below, and the target user, and is intended to demonstrate the usage of BeginApplyChanges
    // on a flow. (BeginApplyChanges is used to change the properties of a flow, usually to change/limit the direction, or to halt the flow of media entirely.)
    // First, this application places a basic outbound audio video call from the user provided to the far end URI designated below.
    // Note: This sample represents only an outbound call. It is necessary that there be a receiving client logged in as the remote participant (callee).
    // Then, the application waits for the audio video flow to become active (signifying that media operations may commence).
    // The application plays a file so the callee can hear the quality of the sound. Then it applies a change to the sampling rate property, to use only 8Khz, the callee side will be able to tell the change happened due to the change of the audio quality.
    // After that, the application tears down the platform, and ends.
    // (We suggest that you use Microsoft Lync as the target of this application.)

    // This application requires the credentials of Microsoft Lync Server users, enabled for voice, and that UCMA Core SDK be present on this machine.
    // Also, be sure that the users in question can log in to Microsoft Lync Server using Microsoft Lync with the credentials provided, and from the machine that is running this code.
    // Warning: Though the code below makes use of UserEndpoint/user credentials, this is a simplification for ease of use of the sample. For all trusted operations, use ApplicationEndpoint.
    public class UCMAAudioVideoFlowApplyChanges
    {
        private CollaborationPlatform _collabPlatform;
        private AudioVideoFlow _audioVideoFlow;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _waitForShutdownEventCompleted = new AutoResetEvent(false);
        private AutoResetEvent _waitForApplyChangesCompleted = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            UCMAAudioVideoFlowApplyChanges BasicAVCall= new UCMAAudioVideoFlowApplyChanges();
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

            // Check allowed direction. 
            Console.WriteLine("AudioVideoFlow using sampling rate: " + _audioVideoFlow.Audio.GetChannels()[ChannelLabel.AudioMono].SamplingRate);

            Thread.Sleep(10000);

            Console.WriteLine("Call ApplyChanges changing sampling rate from 8Khz or 16Khz to only 8Khz.");
            
            AudioVideoFlowTemplate template = new AudioVideoFlowTemplate(_audioVideoFlow);
            AudioChannelTemplate audioChannelTemplate = template.Audio.GetChannels()[ChannelLabel.AudioMono];
            audioChannelTemplate.SamplingRate = AudioSamplingRate.EightKhz;

            // Change allowed direction to SendOnly.
            _audioVideoFlow.BeginApplyChanges(template, audioVideoFlow_ApplyChangesCompleted, _audioVideoFlow);
            _waitForApplyChangesCompleted.WaitOne();

            Console.WriteLine("AudioVideoFlow using sampling rate: " + _audioVideoFlow.Audio.GetChannels()[ChannelLabel.AudioMono].SamplingRate);

            Thread.Sleep(10000);

            // Shutdown the platform
            ShutdownPlatform();

            //Wait for shutdown to occur.
            _waitForShutdownEventCompleted.WaitOne();
        }

        private void audioVideoFlow_ApplyChangesCompleted(IAsyncResult result)
        {
            try
            {
                AudioVideoFlow avFlow = (AudioVideoFlow)result.AsyncState;
                avFlow.EndApplyChanges(result);
            }
            catch (RealTimeException e)
            {
                throw e;
            }

            _waitForApplyChangesCompleted.Set();
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
