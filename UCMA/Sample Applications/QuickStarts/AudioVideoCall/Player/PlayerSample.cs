/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   	*
*                                                       *
********************************************************/

using System;
using System.Threading;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.PlayerBasic
{
    // This demo establishes a basic audio video connection between the user provided below, and the target user, intended to demonstrate the 
    // usage of an automatic player on a flow. First, this application places a basic outbound audio video call from the user provided to the 
    // far end URI designated below. Note: This sample only represents an outbound call. it is necessary that there be a recieving client
    // logged in as the far end participant (callee). Once the call is connected, and the flow goes active, the player automatically starts.
    // From there, basic player control actions are undergone (Start, Stop, Pause, PlaybackSpeed), and then the platform and associate objects are torn down.
    // (We suggest you use Microsoft Lync as the target of this application.)

    // This application requires the credentials of Microsoft Lync Server users, enabled for voice, and that UCMA be present on this machine.
    // Warning: Though the code below makes use of UserEndpoint/user credentials, this is a simplification for ease of use of the sample. For all trusted operations, use ApplicationEndpoint.
    public class UCMAPlayerBasic
    {
        private CollaborationPlatform _collabPlatform;
        private AudioVideoFlow _audioVideoFlow;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _waitForShutdownEventCompleted = new AutoResetEvent(false);
        private AutoResetEvent _waitForPrepareSourceCompleted = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            UCMAPlayerBasic p = new UCMAPlayerBasic();
            p.Run();
        }

        public void Run()
        {
            // Create AudioVideoFlow
            AudioVideoFlowHelper audioVideoFlowHelper = new AudioVideoFlowHelper();
            _audioVideoFlow = audioVideoFlowHelper.CreateAudioVideoFlow(
                null,
                audioVideoFlow_StateChanged);

            // Create a player and attach it to a AudioVideoFlow
            Player player = new Player();
            player.AttachFlow(_audioVideoFlow);

            //Subscribe to the player's state changed event, including the play completed event. 
            player.StateChanged += new EventHandler<PlayerStateChangedEventArgs>(player_StateChanged);

            //Load the file into memory
            WmaFileSource source = new WmaFileSource("music.wma");
            source.BeginPrepareSource(MediaSourceOpenMode.Buffered, source_PrepareSourceCompleted, source);
            _waitForPrepareSourceCompleted.WaitOne();

            //in automatic mode, player will start playing only when the flow is in the active state.
            //in manual mode, player will start playing immediately.
            player.SetMode(PlayerMode.Automatic);

            player.SetSource(source);

            //Start playing the file
            player.Start();
            Console.WriteLine("Start playing for 10 seconds");

            //Allow the player to play for 10 seconds by waiting for 10 seconds
            Thread.Sleep(10000);

            //Pauses player
            player.Pause();
            Console.WriteLine("Pausing for 5 seconds");

            //Wait 5 seconds
            Thread.Sleep(5000);

            //Change playback speed to half of the regular speed
            player.PlaybackSpeed = PlaybackSpeed.Half;
            Console.WriteLine("Playback speed change to half of the regular speed");

            //Resume playing from where we paused the player at previously
            player.Start();
            Console.WriteLine("Resume playing for 10 seconds");

            Thread.Sleep(10000);

            //Stop the player and reset position to the beginning
            player.Stop();
            Console.WriteLine("Stopping the player");

            // Source must be closed after it is no longer needed, otherwise memory will not be released even after garbage collection.
            source.Close();

            //player must be detached from the flow, otherwise if the player is rooted, it will keep the flow in memory.
            player.DetachFlow(_audioVideoFlow);

            // Shutdown the platform
            ShutdownPlatform();

            //Wait for shutdown to occur.
            _waitForShutdownEventCompleted.WaitOne();
        }

        void player_StateChanged(object sender, PlayerStateChangedEventArgs e)
        {
            Console.WriteLine("Player state changed from " + e.PreviousState + " to " + e.State + " with reason " + e.TransitionReason);
        }

        private void source_PrepareSourceCompleted(IAsyncResult result)
        {
            WmaFileSource source = (WmaFileSource)result.AsyncState;
            source.EndPrepareSource(result);

            _waitForPrepareSourceCompleted.Set();
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
