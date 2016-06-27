/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

// .NET namespaces
using System;
using System.Configuration;
using System.Threading;

// UCMA namespaces
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;


namespace Microsoft.Rtc.Collaboration.Sample.ConferenceEscalation
{
    public class UCMASampleConferenceEscalation
    {
        #region Locals
        // Reference to the UCMASampleHelper.
        private UCMASampleHelper _helper;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _waitForCallerConferenceEscalation = new AutoResetEvent(false);

        private AutoResetEvent _waitForCalleeConferenceEscalation = new AutoResetEvent(false);

        private AutoResetEvent _waitForCallAcceptByCallee = new AutoResetEvent(false);

        private AutoResetEvent _waitForCallerCallEstablish = new AutoResetEvent(false);

        private AutoResetEvent _waitForRoleModification = new AutoResetEvent(false);

        private AutoResetEvent _waitForConferenceLock = new AutoResetEvent(false);

        private AutoResetEvent _waitForParticipantEject = new AutoResetEvent(false);
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the ConferenceEscalation quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleConferenceEscalation ucmaSampleConferenceEscalation = new UCMASampleConferenceEscalation();
            ucmaSampleConferenceEscalation.Run();
        }

        private void Run()
        {
            // A helper class to take care of platform and endpoint setup and 
            // cleanup. This has been abstracted from this sample to focus on 
            // Call Control.
            _helper = new UCMASampleHelper();

            // Create a user endpoint, using the network credential object 
            // defined above.
            UserEndpoint callerEndpoint = _helper.CreateEstablishedUserEndpoint("Caller" /*endpointFriendlyName*/);

            // Create a second user endpoint, using the network credential object
            // defined above.
            UserEndpoint calleeEndpoint = _helper.CreateEstablishedUserEndpoint("Callee" /*endpointFriendlyName*/);

            // Here, we are accepting an InstantMessaging call only.
            // If the incoming call is not an InstantMessaging call (for example,
            // an AudioVideo call) then it will not get raised to the application.
            // UCMA 3.0 handles this silently by having the call types register 
            // for various modalities (as part of the extensibility framework). 
            // The appropriate action (here, accepting the call) will be handled
            // in the handler assigned to the method call below.
            calleeEndpoint.RegisterForIncomingCall<InstantMessagingCall>
                (On_InstantMessagingCall_Received_ByCallee);

            // Setup the call and conversation objects for the initial call (IM)
            // and place the call (synchronously).
            Conversation callerConversation = new Conversation(callerEndpoint);
            InstantMessagingCall callerInstantMessagingCall = new InstantMessagingCall(callerConversation);
            callerInstantMessagingCall.BeginEstablish(calleeEndpoint.OwnerUri , null, EndCallerCallEstablish, 
                callerInstantMessagingCall);

            // Force synchronization to ensure that the call is now complete.
            _waitForCallAcceptByCallee.WaitOne();
            _waitForCallerCallEstablish.WaitOne();

            // Now that the call is established, we can begin the escalation.
            // First, we bind the conference state changed event handler, largely
            // for logging reasons.
            callerConversation.ConferenceSession.StateChanged += new 
                EventHandler<StateChangedEventArgs<ConferenceSessionState>>(ConferenceSession_StateChanged);

            Console.WriteLine("");
            Console.WriteLine(" Beginning conference creation and escalation..." );
            Console.WriteLine("");

            // Next, the initiator of the escalation creates an ad-hoc conference
            // (using the current modalities present in the conversation) by calling 
            // ConferenceSession.BeginJoin on a conversation, without providing 
            // a conference URI. This prepares the calls for actual escalation by
            // binding the appropriate conference multipoint Control Unit (MCU) 
            // sessions. When EndJoinConference is called, it will kick off the 
            // subsequent escalation. As part of the escalation, the remote party
            // will receive an escalation request, via the event on the far end 
            // conversation, EscalateToConferenceRequested. Also, the existing 
            // calls will be shifted to the MCUs.
            callerConversation.ConferenceSession.BeginJoin(default(ConferenceJoinOptions), 
                EndCallerJoinConference, callerConversation.ConferenceSession);
        
            //Wait for both sides to fully escalate to the new conference.
            _waitForCallerConferenceEscalation.WaitOne();
            _waitForCalleeConferenceEscalation.WaitOne();

            Console.WriteLine("");
            Console.WriteLine(" Beginning conference command and control..." );
            Console.WriteLine("");

            // Promote a participant to leader; Leaders can lock and unlock the 
            // conference, as well as possessing the ability to eject and control
            // other participants, and mute participants (in an Audio conference).
            // For purposes of the demonstration, choose an arbitrary conference
            // participant here, the first.

            ConversationParticipant target = null;

            if (callerConversation.RemoteParticipants.Count >= 1)
            {
                target = callerConversation.RemoteParticipants[0];
            }
            else
            {
                // TODO (Left to the reader): Add error handling code.
            }

            Console.WriteLine("User " + target.UserAtHost + " is currently an " + target.Role + ".");
            
            // Note: This is the naive synch implementation, and is not generally
            // suitable for production code. It is only used here for brevity.
            callerConversation.ConferenceSession.BeginModifyRole(target, ConferencingRole.Leader, 
                EndModifyRole, callerConversation.ConferenceSession);
            _waitForRoleModification.WaitOne();
            Console.WriteLine("User " + target.UserAtHost + " is now a " + target.Role + ".");

            Console.WriteLine("The conference access level is currently " + 
                callerConversation.ConferenceSession.AccessLevel + ".");
            // Locking the conference prevents new users from joining the 
            // conference, unless explicitly called into the conference through 
            // the dialout API.
            callerConversation.ConferenceSession.BeginLockConference(EndLockConference, 
                callerConversation.ConferenceSession);
            _waitForConferenceLock.WaitOne();
            Console.WriteLine("The conference accesslevel is now " + 
                callerConversation.ConferenceSession.AccessLevel + ".");

            // Now, eject the participant, and then shut down the platform.
            
            Console.WriteLine("The conference currently has " + 
                callerConversation.ConferenceSession.GetRemoteParticipantEndpoints().Count + " attendees.");
            // Ejection can only be performed by leaders of the conference.
            callerConversation.ConferenceSession.BeginEject(target, EndEject, callerConversation.ConferenceSession);
            _waitForParticipantEject.WaitOne();
            Console.WriteLine("The conference now has " + 
                callerConversation.ConferenceSession.GetRemoteParticipantEndpoints().Count + " attendees.");
                       
            _helper.ShutdownPlatform();

            Console.ReadLine();
        }

        //Just to record the state transitions in the console.
        void ConferenceSession_StateChanged(object sender, StateChangedEventArgs<ConferenceSessionState> e)
        {
            ConferenceSession confSession = sender as ConferenceSession;

            //Session participants allow for disambiguation.
            Console.WriteLine("The conference session with Local Participant: " + 
                confSession.Conversation.LocalParticipant + 
                " has changed state. The previous conference state was: " + e.PreviousState + 
                " and the current state is: " + e.State);
        }


        //Just to record the state transitions in the console.
        void _call_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Call call = sender as Call;

            //Call participants allow for disambiguation.
            Console.WriteLine("The call with Local Participant: " + call.Conversation.LocalParticipant + 
                " and Remote Participant: " + call.RemoteEndpoint.Participant + 
                " has changed state. The previous call state was: " + e.PreviousState + 
                " and the current state is: " + e.State);
        }

        void On_InstantMessagingCall_Received_ByCallee(object sender, CallReceivedEventArgs<InstantMessagingCall> e)
        {
            //Type checking was done by the platform; no risk of this being any 
            // type other than the type expected.
            InstantMessagingCall instantMessagingCall = e.Call;

            // Call: StateChanged: Only hooked up for logging, to show the call 
            // state transitions.
            instantMessagingCall.StateChanged += new EventHandler<CallStateChangedEventArgs>(_call_StateChanged);
            
            // Remote Participant URI represents the far end (caller) in this 
            // conversation. Toast is the message set by the caller as the 'greet'
            // message for this call. In Microsoft Lync, the toast will 
            // show up in the lower-right of the screen. Conversation ID demonstrates
            // that the two calls inhabit the same conversation container on 
            // both ends of the call.
            Console.WriteLine("");
            Console.WriteLine(" Call Received! From: " + e.RemoteParticipant.Uri);
            Console.WriteLine(" Toast is: " + e.ToastMessage.Message);
            Console.WriteLine(" Conversation ID is: " + e.Call.Conversation.Id);
            Console.WriteLine("");
            
            // Now, accept the call.
            // EndAcceptCall will be raised on the same thread.
            instantMessagingCall.BeginAccept(EndAcceptCall, instantMessagingCall);

            // When an escalation request is received on the existing call, this
            // event handler will be called.
            instantMessagingCall.Conversation.EscalateToConferenceRequested += 
                this.CalleeConversation_EscalateToConferenceRequested;
        }

        void CalleeConversation_EscalateToConferenceRequested(object sender, EventArgs e)
        {
            Conversation conversation = sender as Conversation;

            // The callee side is near identical; though, note that it's begin 
            // join will not cause any new conference to be created. First, we 
            // bind the conference state changed event handler, largely for 
            // logging reasons.
            conversation.ConferenceSession.StateChanged += new 
                EventHandler<StateChangedEventArgs<ConferenceSessionState>>(ConferenceSession_StateChanged);

            // Next, the escalatee prepares the session to escalate, by calling 
            // ConferenceSession.BeginJoin on the conversation that received the
            // escalation request. This prepares the calls for actual escalation 
            // by binding the appropriate conference multipoint Control Unit (MCU)
            // sessions. You cannot escalate directly in response to an 
            // escalation request.
            conversation.ConferenceSession.BeginJoin(default(ConferenceJoinOptions), EndCalleeJoinConference, 
                conversation.ConferenceSession);
       
        }

        private void EndModifyRole(IAsyncResult ar)
        {
            ConferenceSession confSession = ar.AsyncState as ConferenceSession;
            try
            {
                confSession.EndModifyRole(ar);

            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException may be thrown on media or link-layer failures
                // or call rejection (FailureResponseException)
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(realTimeEx.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _waitForRoleModification.Set();
            }
        }

        private void EndLockConference(IAsyncResult ar)
        {
            ConferenceSession confSession = ar.AsyncState as ConferenceSession;
            try
            {
                confSession.EndLockConference(ar);

            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException may be thrown on media or link-layer failures,
                // or call rejection (FailureResponseException)
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(realTimeEx.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _waitForConferenceLock.Set();
            }
        }

        private void EndEject(IAsyncResult ar)
        {
            ConferenceSession confSession = ar.AsyncState as ConferenceSession;
            try
            {
                confSession.EndEject(ar);

            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException may be thrown on media or link-layer failures,
                // or call rejection (FailureResponseException)
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(realTimeEx.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _waitForParticipantEject.Set();
            }
        }

        private void EndCallerJoinConference(IAsyncResult ar)
        {
            ConferenceSession confSession = ar.AsyncState as ConferenceSession;
            try
            {
               confSession.EndJoin(ar);

            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException may be thrown on media or link-layer failures,
                // or call rejection (FailureResponseException)
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(realTimeEx.ToString());
            }

            // As mentioned before, if this is the first party to escalate, the 
            // remote party will receive an escalation request, via the event on
            // the far end conversation, EscalateToConferenceRequested. Also, the
            // existing calls will be shifted to the MCUs.
            confSession.Conversation.BeginEscalateToConference(EndCallerEscalateConference, 
                confSession.Conversation);
        }

        private void EndCalleeJoinConference(IAsyncResult ar)
        {
            ConferenceSession confSession = ar.AsyncState as ConferenceSession;
            try
            {
                confSession.EndJoin(ar);

            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException may be thrown on media or link-layer failures,
                // or call rejection (FailureResponseException)
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(realTimeEx.ToString());
            }

            // As mentioned before, if this is the first party to escalate, the 
            // remote party will receive an escalation request, via the event on
            // the far end conversation, EscalateToConferenceRequested. Also, the
            // existing calls will be shifted to the MCUs.
            confSession.Conversation.BeginEscalateToConference(EndCalleeEscalateConference, 
                confSession.Conversation);
        }

        private void EndCallerEscalateConference(IAsyncResult ar)
        {
            Conversation conversation = ar.AsyncState as Conversation;

            try
            {
                conversation.EndEscalateToConference(ar);

            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException may be thrown on media or link-layer failures,
                // or call rejection (FailureResponseException)
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(realTimeEx.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _waitForCallerConferenceEscalation.Set();
            }

        }

        private void EndCalleeEscalateConference(IAsyncResult ar)
        {
            Conversation conversation = ar.AsyncState as Conversation;

            try
            {
                conversation.EndEscalateToConference(ar);

            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException may be thrown on media or link-layer failures,
                // or call rejection (FailureResponseException)
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(realTimeEx.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _waitForCalleeConferenceEscalation.Set();
            }

        }

        private void EndAcceptCall(IAsyncResult ar)
        {
            Call call = ar.AsyncState as Call;
            
            // End accepting the incoming call.
            call.EndAccept(ar);

            //Again, just to sync the completion of the code.
            _waitForCallAcceptByCallee.Set();
        }

        private void EndCallerCallEstablish(IAsyncResult ar)
        {
            Call call = ar.AsyncState as Call;
            try
            {
                call.EndEstablish(ar);
                Console.WriteLine("The call with Local Participant: " + call.Conversation.LocalParticipant + 
                    " and Remote Participant: " + call.RemoteEndpoint.Participant + 
                    " is now in the established state.");
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // TODO (Left to the reader): Add error handling code here.
                Console.WriteLine(exception.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _waitForCallerCallEstablish.Set();
            }
        }
        #endregion
    }
}
