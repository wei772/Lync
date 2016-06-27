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
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.SpeechRecognitionBasic
{
    // This demo establishes a basic audio video connection between the user provided below, and the target user, intended to demonstrate the 
    // usage of a speech recognition connector on a flow. First, this application places a basic outbound audio video call from the user provided to the 
    // far end URI designated below. Note: This sample only represents an outbound call. it is necessary that there be a receiving client
    // logged in as the far end participant (callee). Once the call is connected, and the flow goes active, the speech recognition connector automatically starts.
    // From there, the speech recognition connector will try to recognize for small period of time before stopping the other call saying one digit, and then the platform and associate objects are torn down.
    // (We suggest you use Microsoft Lync as the target of this application.)

    // This application requires the credentials of Microsoft Lync Server users, enabled for voice, and that UCMA be present on this machine.
    // Warning: Though the code below makes use of UserEndpoint/user credentials, this is a simplification for ease of use of the sample. For all trusted operations, use ApplicationEndpoint.
    public class UCMASpeechRecognition
    {
        private CollaborationPlatform _collabPlatform;
        private AudioVideoFlow _audioVideoFlow;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _waitForShutdownEventCompleted = new AutoResetEvent(false);
        private AutoResetEvent _waitForXXXCompleted = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            UCMASpeechRecognition p = new UCMASpeechRecognition();
            p.Run();
        }

        public void Run()
        {
            // Create AudioVideoFlow
            AudioVideoFlowHelper audioVideoFlowHelper = new AudioVideoFlowHelper();
            _audioVideoFlow = audioVideoFlowHelper.CreateAudioVideoFlow(
                null,
                audioVideoFlow_StateChanged);

            // Create a speech recognition connector and attach it to a AudioVideoFlow
            SpeechRecognitionConnector speechRecognitionConnector = new SpeechRecognitionConnector();
            speechRecognitionConnector.AttachFlow(_audioVideoFlow);

            //Start recognizing
            SpeechRecognitionStream stream = speechRecognitionConnector.Start();

            // Create speech recognition engine and start recognizing by attaching connector to engine
            SpeechRecognitionEngine speechRecognitionEngine = new SpeechRecognitionEngine();
            speechRecognitionEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(speechRecognitionEngine_SpeechRecognized);


            string[] recognizedString = { "zero" , "one", "two", "three", "four",  "five", "six", "seven", "eight", "nine", "ten", "exit"};
            Choices numberChoices = new Choices(recognizedString);
            speechRecognitionEngine.LoadGrammar(new Grammar(new GrammarBuilder(numberChoices)));

            SpeechAudioFormatInfo speechAudioFormatInfo = new SpeechAudioFormatInfo(8000, AudioBitsPerSample.Sixteen, Microsoft.Speech.AudioFormat.AudioChannel.Mono);
            speechRecognitionEngine.SetInputToAudioStream(stream, speechAudioFormatInfo);
            Console.WriteLine("\r\nGrammar loaded from zero to ten, say exit to shutdown.");

            speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

            _waitForXXXCompleted.WaitOne();
            //Stop the connector
            speechRecognitionConnector.Stop();
            Console.WriteLine("Stopping the speech recognition connector");

            //speech recognition connector must be detached from the flow, otherwise if the connector is rooted, it will keep the flow in memory.
            speechRecognitionConnector.DetachFlow();

            // Shutdown the platform
            ShutdownPlatform();

            _waitForShutdownEventCompleted.WaitOne();
        }

        void speechRecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            RecognitionResult result = e.Result;
            if(result != null)
            {
                Console.WriteLine("Speech recognized: " + result.Text);

                if (result.Text.Equals("exit"))
                {
                    _waitForXXXCompleted.Set();
                }
            }
        }
        // Callback that handles when the state of an AudioVideoFlow changes
        private void audioVideoFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            //When flow is active, media operations can begin
            if (e.State == MediaFlowState.Terminated)
            {
                // Detach SpeechRecognitionConnector since AVFlow will not work anymore
                AudioVideoFlow avFlow = (AudioVideoFlow)sender;
                if (avFlow.SpeechRecognitionConnector != null)
                {
                    avFlow.SpeechRecognitionConnector.DetachFlow();
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
