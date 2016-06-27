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
using System.Configuration;
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.RecorderBasic
{
    // This demo establishes a basic audio video connection between the user provided below, and the target user, intended to demonstrate the 
    // usage of a recorder on a flow. First, this application places a basic outbound audio video call from the user provided to the 
    // far end URI designated below. Note: This sample only represents an outbound call. it is necessary that there be a receiving client
    // logged in as the far end participant (callee). Once the call is connected, and the flow goes active, the recorder subscribes to events (StateChanged and VoiceActivityChanged) and starts.
    // From there, basic recorder control actions are undergone (Start, Pause, Resume, Stop), and then the platform and associate objects are torn down.
    // (We suggest you use Microsoft Lync as the target of this application.)

    // This application requires the credentials of Microsoft Lync Server users, enabled for voice, and that UCMA be present on this machine.
    // Warning: Though the code below makes use of UserEndpoint/user credentials, this is a simplification for ease of use of the sample. For all trusted operations, use ApplicationEndpoint.
    public class UCMARecorderBasic
    {
        private CollaborationPlatform _collabPlatform;
        private AudioVideoFlow _audioVideoFlow;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _waitForShutdownEventCompleted = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            UCMARecorderBasic p = new UCMARecorderBasic();
            p.Run();
        }

        public void Run()
        {
            // Create AudioVideoFlow
            AudioVideoFlowHelper audioVideoFlowHelper = new AudioVideoFlowHelper();
            _audioVideoFlow = audioVideoFlowHelper.CreateAudioVideoFlow(
                null,
                audioVideoFlow_StateChanged);

            //Load readme file as the source
            Console.WriteLine();
            Console.Write("Please enter the destination wma file => ");
            string filename = Console.ReadLine();

            //setup a recorder to record the audio received from the remote side by attaching it to the AudioVideoFlow
            Recorder recorder = new Recorder();
            recorder.AttachFlow(_audioVideoFlow);

            //Subscribe to the recorder's state changed event
            recorder.StateChanged += new EventHandler<RecorderStateChangedEventArgs>(recorder_StateChanged);

            //Subscribe to voice activity changed event
            recorder.VoiceActivityChanged += new EventHandler<VoiceActivityChangedEventArgs>(recorder_VoiceActivityChanged);

            //Create the sink and give it to the recorder so the recorder knows where to record to
            WmaFileSink sink = new WmaFileSink(filename);
            recorder.SetSink(sink);

            //Start to record
            recorder.Start();
            Console.WriteLine("\r\nRecording for 10 seconds.");

            //Wait for 9 seconds to allow recording up to 10 seconds
            Thread.Sleep(10000);

            //Pauses recorder
            recorder.Pause();
            Console.WriteLine("\r\nPausing for 2 seconds.");

            //Wait 2 seconds
            Thread.Sleep(2000);

            //Resume recording from where we paused the recorder previously
            recorder.Start();
            Console.WriteLine("\r\nResume recording for 5 seconds.");

            //Wait 5 seconds
            Thread.Sleep(5000);

            //Stop the recording
            recorder.Stop();
            Console.WriteLine("\r\nRecording stopped.");

            //Detach the recorder from the AudioVideoFlow
            recorder.DetachFlow();

            // Shutdown the platform
            ShutdownPlatform();

            //Wait for shutdown to occur.
            _waitForShutdownEventCompleted.WaitOne();
        }

        void recorder_VoiceActivityChanged(object sender, VoiceActivityChangedEventArgs e)
        {
            Console.WriteLine("Recorder detected " + (e.IsVoice ? "voice" : "silence") + " at " + e.TimeStamp);
        }

        void recorder_StateChanged(object sender, RecorderStateChangedEventArgs e)
        {
            Console.WriteLine("Recorder state changed from " + e.PreviousState + " to " + e.State);
        }

        // Callback that handles when the state of an AudioVideoFlow changes
        private void audioVideoFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            //When flow is active, media operations can begin
            if (e.State == MediaFlowState.Terminated)
            {
                // Detach Recorder since AVFlow will not work anymore
                AudioVideoFlow avFlow = (AudioVideoFlow)sender;
                if (avFlow.Recorder != null)
                {
                    avFlow.Recorder.DetachFlow();
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
