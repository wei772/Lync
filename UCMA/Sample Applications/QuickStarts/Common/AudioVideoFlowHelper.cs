/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

using System;
using System.Configuration;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

namespace Microsoft.Rtc.Collaboration.Sample.Common
{
    class AudioVideoFlowHelper
    {
        private static String _conversationSubject = "The Microsoft Lync Server!";
        private static String _conversationPriority = ConversationPriority.Urgent;
        private static String _calledParty;

        private AutoResetEvent _waitForAudioVideoCallEstablishCompleted = new AutoResetEvent(false);
        private AutoResetEvent _waitForAudioVideoFlowStateChangedToActiveCompleted = new AutoResetEvent(false);
        private AutoResetEvent _waitForPrepareSourceCompleted = new AutoResetEvent(false);

        private AudioVideoFlow _audioVideoFlow;
        private EventHandler<AudioVideoFlowConfigurationRequestedEventArgs> _audioVideoFlowConfigurationRequestedEventHandler;
        private EventHandler<MediaFlowStateChangedEventArgs> _audioVideoFlowStateChangedEventHandler;

        public AudioVideoFlow CreateAudioVideoFlow(EventHandler<AudioVideoFlowConfigurationRequestedEventArgs> audioVideoFlowConfigurationRequestedEventHandler, EventHandler<MediaFlowStateChangedEventArgs> audioVideoFlowStateChangedEventHandler)
        {
            _audioVideoFlowConfigurationRequestedEventHandler = audioVideoFlowConfigurationRequestedEventHandler;
            _audioVideoFlowStateChangedEventHandler = audioVideoFlowStateChangedEventHandler;

            UCMASampleHelper UCMASampleHelper = new UCMASampleHelper();
            UserEndpoint userEndpoint = UCMASampleHelper.CreateEstablishedUserEndpoint("AudioVideoFlowHelper");

            // If application settings are provided via the app.config file, then use them
            if (ConfigurationManager.AppSettings.HasKeys() == true)
            {
                _calledParty = "sip:" + ConfigurationManager.AppSettings["CalledParty"];
            }
            else
            {
                // Prompt user for user URI
                string prompt = "Please enter the Called Party URI in the User@Host format => ";
                _calledParty = UCMASampleHelper.PromptUser(prompt, "Remote User URI");
                _calledParty = "sip:" + _calledParty;
            }

            // Setup the conversation and place the call.
            ConversationSettings convSettings = new ConversationSettings();
            convSettings.Priority = _conversationPriority;
            convSettings.Subject = _conversationSubject;

            // Conversation represents a collection of modes of communication (media types)in the context of a dialog with one or multiple callees.
            Conversation conversation = new Conversation(userEndpoint, convSettings);
            AudioVideoCall audioVideoCall = new AudioVideoCall(conversation);

            //Call: StateChanged: Only hooked up for logging.
            audioVideoCall.StateChanged += new EventHandler<CallStateChangedEventArgs>(audioVideoCall_StateChanged);

            //Subscribe for the flow configuration requested event; the flow will be used to send the media.
            //Ultimately, as a part of the callback, the media will be sent/recieved.
            audioVideoCall.AudioVideoFlowConfigurationRequested += new EventHandler<AudioVideoFlowConfigurationRequestedEventArgs>(audioVideoCall_FlowConfigurationRequested);

            //Place the call to the remote party, with the default call options.
            audioVideoCall.BeginEstablish(_calledParty, null, EndCallEstablish, audioVideoCall);

            //Sync; wait for the call to complete.
            _waitForAudioVideoCallEstablishCompleted.WaitOne();

            //Sync; wait for the AudioVideoFlow goes Active
            _waitForAudioVideoFlowStateChangedToActiveCompleted.WaitOne();

            return _audioVideoFlow;
        }

        public void AttachAndStartPlayer(AudioVideoFlow audioVideoFlow, bool loop)
        {
            // Create a player and attach it to a AudioVideoFlow
            Player player = new Player();
            player.AttachFlow(audioVideoFlow);

            //Subscribe to the player's state changed event, including the play completed event. 
            player.StateChanged += delegate(object sender, PlayerStateChangedEventArgs args)
            {
                if (loop && args.TransitionReason == PlayerStateTransitionReason.PlayCompleted)
                {
                    // Restart player as soon as it completes playing the file.
                    ((Player)sender).Start();
                }
            };

            //Load the file into memory
            WmaFileSource source = new WmaFileSource("music.wma");
            source.BeginPrepareSource(MediaSourceOpenMode.Buffered, source_PrepareSourceCompleted, source);
            _waitForPrepareSourceCompleted.WaitOne();

            player.SetSource(source);

            //Start playing the file
            player.Start();
        }

        private void source_PrepareSourceCompleted(IAsyncResult result)
        {
            WmaFileSource source = (WmaFileSource)result.AsyncState;
            source.EndPrepareSource(result);

            _waitForPrepareSourceCompleted.Set();
        }

        //Just to record the state transitions in the console.
        void audioVideoCall_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Console.WriteLine("Call has changed state. The previous call state was: " + e.PreviousState + " and the current state is: " + e.State);
        }

        //Flow configuration requested indicates that there is a flow present to begin media operations with that it is no longer null, and is ready to be configured.
        public void audioVideoCall_FlowConfigurationRequested(object sender, AudioVideoFlowConfigurationRequestedEventArgs e)
        {
            Console.WriteLine("Flow Configuration Requested.");
            _audioVideoFlow = e.Flow;

            //Now that the flow is non-null, bind the event handler for State Changed.
            // When the flow goes active, (as indicated by the state changed event) the program will perform media related actions..
            _audioVideoFlow.StateChanged += new EventHandler<MediaFlowStateChangedEventArgs>(audioVideoFlow_StateChanged);

            // call sample event handler
            if (_audioVideoFlowConfigurationRequestedEventHandler != null)
            {
                _audioVideoFlowConfigurationRequestedEventHandler(sender, e);
            }
        }

        // Callback that handles when the state of an AudioVideoFlow changes
        private void audioVideoFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            Console.WriteLine("Flow state changed from " + e.PreviousState + " to " + e.State);

            //When flow is active, media operations can begin
            if (e.State == MediaFlowState.Active)
            {
                // Flow-related media operations normally begin here.
                _waitForAudioVideoFlowStateChangedToActiveCompleted.Set();
            }

            // call sample event handler
            if (_audioVideoFlowStateChangedEventHandler != null)
            {
                _audioVideoFlowStateChangedEventHandler(sender, e);
            }
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
                // It is left to the application to perform real error handling here.
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // It is left to the application to perform real error handling here.
                Console.WriteLine(exception.ToString());
            }
            finally
            {
                //Again, just for sync. reasons.
                _waitForAudioVideoCallEstablishCompleted.Set();
            }
        }
    }
}
