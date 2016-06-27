/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   	*
*                                                       *
********************************************************/

using System;
using System.IO;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Synthesis;
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.SpeechSynthesisConnectorBasic
{
    // This demo establishes a basic audio video connection between the user provided below, and the target user, intended to demonstrate the 
    // usage of a speech synthesis connector on a flow. First, this application places a basic outbound audio video call from the user provided to the 
    // far end URI designated below. Note: This sample only represents an outbound call. it is necessary that there be a receiving client
    // logged in as the far end participant (callee). Once the call is connected, and the flow goes active, the speech synthesis connector automatically starts.
    // From there, speech synthesis connector will play audio for small period of time before stopping, and then the platform and associate objects are torn down.
    // (We suggest you use Microsoft Lync as the target of this application.)

    // This application requires the credentials of Microsoft Lync Server users, enabled for voice, and that UCMA be present on this machine.
    // Warning: Though the code below makes use of UserEndpoint/user credentials, this is a simplification for ease of use of the sample. For all trusted operations, use ApplicationEndpoint.
    public class UCMASpeechSynthesisConnectorBasic
    {
        private CollaborationPlatform _collabPlatform;
        private AudioVideoFlow _audioVideoFlow;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _waitForShutdownEventCompleted = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            UCMASpeechSynthesisConnectorBasic p = new UCMASpeechSynthesisConnectorBasic();
            p.Run();
        }

        public void Run()
        {
            // Create AudioVideoFlow
            AudioVideoFlowHelper audioVideoFlowHelper = new AudioVideoFlowHelper();
            _audioVideoFlow = audioVideoFlowHelper.CreateAudioVideoFlow(
                null,
                audioVideoFlow_StateChanged);

            // Create a speech synthesis connector and attach it to an AudioVideoFlow
            SpeechSynthesisConnector speechSynthesisConnector = new SpeechSynthesisConnector();
            speechSynthesisConnector.AttachFlow(_audioVideoFlow);

            // Create a speech synthesis and set connector to it
            SpeechSynthesizer speechSynthesis = new SpeechSynthesizer();
            SpeechAudioFormatInfo audioformat = new SpeechAudioFormatInfo(16000, AudioBitsPerSample.Sixteen, Microsoft.Speech.AudioFormat.AudioChannel.Mono);
            speechSynthesis.SetOutputToAudioStream(speechSynthesisConnector, audioformat);

            //Load readme file as the source
            Console.WriteLine();
            Console.Write("Please enter the source file => ");
            string filename = Console.ReadLine();

            string msg = "";
            try
            {
                StreamReader objReader = new StreamReader(filename);
                msg = objReader.ReadToEnd();
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("\r\nFile doesn't exist.");
                ShutdownPlatform();
            }

            //Start connector
            speechSynthesisConnector.Start();
            Console.WriteLine("\r\nStreaming source file for 15 seconds.");

            //Start streaming from speech synthesis.
            speechSynthesis.SpeakAsync(new Prompt(msg));

            //Allow the connector to stream 15 seconds by waiting for 15 seconds
            Thread.Sleep(15000);

            //Stop the connector
            speechSynthesisConnector.Stop();
            Console.WriteLine("\r\nSpeech synthesis connector stopped.");

            //speech synthesis connector must be detached from the flow, otherwise if the connector is rooted, it will keep the flow in memory.
            speechSynthesisConnector.DetachFlow();

            // Shutdown the platform
            ShutdownPlatform();

            _waitForShutdownEventCompleted.WaitOne();
        }

        // Callback that handles when the state of an AudioVideoFlow changes
        private void audioVideoFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            //When flow is active, media operations can begin
            if (e.State == MediaFlowState.Terminated)
            {
                // Detach SpeechRecognitionConnector since AVFlow will not work anymore
                AudioVideoFlow avFlow = (AudioVideoFlow)sender;
                if (avFlow.SpeechSynthesisConnector != null)
                {
                    avFlow.SpeechSynthesisConnector.DetachFlow();
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
