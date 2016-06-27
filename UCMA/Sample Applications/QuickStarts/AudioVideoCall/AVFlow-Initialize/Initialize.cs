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

namespace Microsoft.Rtc.Collaboration.Sample.AudioVideoFlowInitialize
{
    // This demo establishes a basic audio video connection between the user provided below, and the target user, intended to demonstrate the usage of Initalization on a flow.
    // First, this application places a basic outbound audio video call from the user provided to the far end URI designated below.
    // Note: This sample only represents an outbound call. it is necessary that there be a recieving client logged in as the far end participant (callee).
    // Once the call is connected, and before the flow goes active, Initalize is used to change the allowed direction do Inactive.
    // The application then continues as normal, to establish an active AudioVideoFlow, and then the application applies a change renegotiating the direction to SendReceive, only then it tears down the platform, and ends..
    // (We suggest you use Microsoft Lync as the target of this application.)

    // This application requires the credentials of Microsoft Lync Server users, enabled for voice, and that UCMA be present on this machine.
    // Warning: Though the code below makes use of UserEndpoint/user credentials, this is a simplification for ease of use of the sample. For all trusted operations, use ApplicationEndpoint.
    public class UCMABasicAVCall
    {
        private CollaborationPlatform _collabPlatform;
        private AudioVideoFlow _audioVideoFlow;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _waitForShutdownEventCompleted = new AutoResetEvent(false);
        private AutoResetEvent _waitForApplyChangesCompleted = new AutoResetEvent(false);
        
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
                audioVideoCall_FlowConfigurationRequested,
                audioVideoFlow_StateChanged);

            // When something happens involving negotiation this event will be triggered.
            _audioVideoFlow.ConfigurationChanged += new EventHandler<AudioVideoFlowConfigurationChangedEventArgs>(audioVideoFlow_ConfigurationChanged);

            // Attaches a player with a source and starts it in constant loop.
            audioVideoFlowHelper.AttachAndStartPlayer(_audioVideoFlow, true);

            // Check allowed direction. 
            Console.WriteLine("AudioVideoFlow audio channel direction: " + _audioVideoFlow.Audio.GetChannels()[ChannelLabel.AudioMono].Direction);

            Thread.Sleep(10000);

            Console.WriteLine("Call ApplyChanges changing audio direcion to send and receive.");

            AudioVideoFlowTemplate template = new AudioVideoFlowTemplate(_audioVideoFlow);
            AudioChannelTemplate audioChannelTemplate = template.Audio.GetChannels()[ChannelLabel.AudioMono];
            audioChannelTemplate.AllowedDirection = MediaChannelDirection.SendReceive;

            // Change allowed direction to SendOnly.
            _audioVideoFlow.BeginApplyChanges(template, audioVideoFlow_ApplyChangesCompleted, _audioVideoFlow);
            _waitForApplyChangesCompleted.WaitOne();

            Console.WriteLine("AudioVideoFlow audio channel direction: " + _audioVideoFlow.Audio.GetChannels()[ChannelLabel.AudioMono].Direction);

            Thread.Sleep(5000);

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

        //Flow configuration requested indicates that there is a flow present to begin media operations with that it is no longer null, and is ready to be configured.
        public void audioVideoCall_FlowConfigurationRequested(object sender, AudioVideoFlowConfigurationRequestedEventArgs e)
        {
            // Change the default behavior before the negotiation is completed
            
            // Create a template based on the current AudioVideoFlow
            AudioVideoFlowTemplate template = new AudioVideoFlowTemplate(e.Flow);
            
            // Accept only 8Khz audio sampling rate codecs
            template.Audio.GetChannels()[ChannelLabel.AudioMono].AllowedDirection = MediaChannelDirection.Inactive;
            Console.WriteLine("AudioVideoFlow initialized as Inactive.");

            // Change _audioVideoFlow settings according to the template
            e.Flow.Initialize(template);
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
