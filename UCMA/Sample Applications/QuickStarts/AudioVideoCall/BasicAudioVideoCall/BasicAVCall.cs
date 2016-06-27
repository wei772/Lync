/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/


using System;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.BasicAudioVideoCall
{
    // This program represents a simple outbound audio video call to a remote user.
    // This program will log in as the users given below, and place an audio call to the target user URI designated below.
    // After this program hangs up, it will tear down the platform and end, and then pause the console for display of the logs.
    // (We suggest that you  use Microsoft Lync to place the calls to this application.)

    // This application requires the credentials of Microsoft Lync Server users, enabled for voice.
    // Also, be sure that the users in question can log in to Microsoft Lync Server using Microsoft Lync with the credentials provided, and from the machine that is running this code.
    // Warning: Though the code below makes use of UserEndpoint/user credentials, this is a simplification for ease of use of the sample. For all trusted operations, use ApplicationEndpoint.
    public class UCMABasicAVCall
    {
        // Some necessary instance variables
        private UCMASampleHelper _helper;
        private AudioVideoCall _audioVideoCall;
        private AudioVideoFlow _audioVideoFlow;
        private UserEndpoint _userEndpoint;

        //The information for the conversation and the far end participant.
        private static String _calledParty; // The target of the call in the format sip:user@host (should be logged on when the application is run). This could also be in the format tel:+1XXXYYYZZZZ
        private static String _conversationSubject = "The Microsoft Lync Server!"; // Subject of the convertsation; will appear at the head of the conversation window if Microsoft Lync is the far end client.
        private static String _conversationPriority = ConversationPriority.Urgent;

        //Wait handles are present only to keep things synchronous and easy to read.
        private AutoResetEvent _waitForConversationToTerminate = new AutoResetEvent(false);
        private AutoResetEvent _waitForCallToEstablish = new AutoResetEvent(false);
        private AutoResetEvent _waitForCallToTerminate = new AutoResetEvent(false);
        
        static void Main(string[] args)
        {
            UCMABasicAVCall BasicAVCall= new UCMABasicAVCall();
            BasicAVCall.Run();
        }

        public void Run()
        {
 
                                  
            //Initialize and register the endpoint, using the credentials of the user the application will be acting as.
            _helper = new UCMASampleHelper();
            _userEndpoint = _helper.CreateEstablishedUserEndpoint("AVCall Sample User" /*endpointFriendlyName*/);

            //Set up the conversation and place the call.
            ConversationSettings convSettings = new ConversationSettings();
            convSettings.Priority = _conversationPriority;
            convSettings.Subject = _conversationSubject;

            //Conversation represents a collection of modalities in the context of a dialog with one or multiple callees.
            Conversation conversation = new Conversation(_userEndpoint, convSettings);

            _audioVideoCall = new AudioVideoCall(conversation);

            //Call: StateChanged: Only hooked up for logging.
            _audioVideoCall.StateChanged += new EventHandler<CallStateChangedEventArgs>(audioVideoCall_StateChanged);

            //Subscribe for the flow configuration requested event; the flow will be used to send the media.
            //Ultimately, as a part of the callback, the media will be sent/received.
            _audioVideoCall.AudioVideoFlowConfigurationRequested += this.audioVideoCall_FlowConfigurationRequested;
            
            // Prompt for called party
            _calledParty = UCMASampleHelper.PromptUser("Enter the URI for the user logged onto Microsoft Lync, in the sip:User@Host format or tel:+1XXXYYYZZZZ format => ", "RemoteUserURI");

            //Place the call to the remote party.
            _audioVideoCall.BeginEstablish(_calledParty, null, EndCallEstablish, _audioVideoCall);

            //Sync; wait for the call to complete.
            Console.WriteLine("Calling the remote user...");
            _waitForCallToEstablish.WaitOne();

            // Terminate the call, and then the conversation.
            // Terminating these additional objects individually is made redundant by shutting down the platform right after, but in the multiple call case, 
            // this is needed for object hygene. Terminating a Conversation terminates all it's associated calls, and terminating an endpoint will terminate 
            // all conversations on that endpoint.
            _audioVideoCall.BeginTerminate(EndTerminateCall, _audioVideoCall);
            Console.WriteLine("Waiting for the call to get terminated...");
            _waitForCallToTerminate.WaitOne();
            _audioVideoCall.Conversation.BeginTerminate(EndTerminateConversation, _audioVideoCall.Conversation);
            Console.WriteLine("Waiting for the conversation to get terminated...");
            _waitForConversationToTerminate.WaitOne();

            //Now, cleanup by shutting down the platform.
            Console.WriteLine("Shutting down the platform...");
            _helper.ShutdownPlatform();

            // Pause the console to allow for easier viewing of logs.
            Console.WriteLine("Please hit any key to end the sample.");
            Console.ReadKey();
        }

        //Event handler to record the state transitions in the console.
        void audioVideoCall_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Console.WriteLine("Call has changed state. The previous call state was: " + e.PreviousState + " and the current state is: " + e.State);
        }

        //Flow created indicates that there is a flow present to begin media operations with, and that it is no longer null.
        public void audioVideoCall_FlowConfigurationRequested(object sender, AudioVideoFlowConfigurationRequestedEventArgs e)
        {
            Console.WriteLine("Flow Created.");
            _audioVideoFlow = e.Flow;
            
            // Now that the flow is non-null, bind the event handler for State Changed.
            // When the flow goes active, (as indicated by the state changed event) the program can choose to take media-related actions on the flow.
            _audioVideoFlow.StateChanged += new EventHandler<MediaFlowStateChangedEventArgs>(audioVideoFlow_StateChanged);

        }

        private void audioVideoFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            Console.WriteLine("Flow state changed from " + e.PreviousState + " to " + e.State);

            //When flow is active, media operations can begin.
            if (e.State == MediaFlowState.Active)
            {
                // Other samples demonstrate uses for an active flow.
            }
        }

        private void EndTerminateCall(IAsyncResult ar)
        {
            AudioVideoCall audioVideoCall = ar.AsyncState as AudioVideoCall;

            // End terminating the incoming call.
            audioVideoCall.EndTerminate(ar);

            //Again, just to sync the completion of the code.
            _waitForCallToTerminate.Set();
        }

        private void EndTerminateConversation(IAsyncResult ar)
        {
            Conversation conv = ar.AsyncState as Conversation;

            // End terminating the conversation.
            conv.EndTerminate(ar);

            //Again, just to sync the completion of the code.
            _waitForConversationToTerminate.Set();
        }
        
        private void EndCallEstablish(IAsyncResult ar)
        {
            Call call = ar.AsyncState as Call;
            try
            {
                call.EndEstablish(ar);
                Console.WriteLine("The call with Local Participant: " + call.Conversation.LocalParticipant + " and Remote Participant: " + call.RemoteEndpoint.Participant + " is now in the established state.");
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the call to the remote party.
                // It is left to the application to perform real error-handling here.
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // It is left to the application to perform real error-handling here.
                Console.WriteLine(exception.ToString());
            }
            finally
            {
                //Again, just for sync. reasons.
                _waitForCallToEstablish.Set();
            }

        }

    }
}
