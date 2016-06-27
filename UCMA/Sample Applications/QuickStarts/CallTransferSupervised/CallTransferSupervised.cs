/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

// .NET namespaces
using System;
using System.Threading;

// UCMA namespaces
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.CallTransferSupervised
{
    public class UCMASampleCallTransferSupervised
    {
        #region Locals
        private UCMASampleHelper _helper;

        private string _transfereeURI;

        private string _transferTargetURI;
        
        //Initial AVCall
        private AudioVideoCall _avCallTransferringtoInitial;

        private UserEndpoint _transfereeEndpoint = null;

        private UserEndpoint _transferorUserEndpoint = null;

        private UserEndpoint _transferTargetEndpoint = null;
        
        //Wait handles are only present to keep things synchronous and easy to read.                
        private AutoResetEvent _sampleCompleted = new AutoResetEvent(false);
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the CallTransferSupervised quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleCallTransferSupervised ucmaSampleCallTransferSupervised = new 
                UCMASampleCallTransferSupervised();
            ucmaSampleCallTransferSupervised.Run();
        }

        private void Run()
        {

            // A helper class to take care of platform and endpoint setup and 
            // cleanup. This has been abstracted from this sample to focus on Call Control.
            _helper = new UCMASampleHelper();

            // Create the user endpoints from the network credential objects 
            // defined above.
            _transfereeEndpoint = _helper.CreateEstablishedUserEndpoint("Transferee" /*endpointFriendlyName*/);
            _transferorUserEndpoint = _helper.CreateEstablishedUserEndpoint(
                "Transferor" /*endpointFriendlyName*/);
            _transferTargetEndpoint = _helper.CreateEstablishedUserEndpoint(
                "Transfer Target" /*endpointFriendlyName*/);
            _transfereeURI = _transfereeEndpoint.OwnerUri;
            _transferTargetURI = _transferTargetEndpoint.OwnerUri;

            // Register for incoming audio calls on the initial and final endpoints.
            _transferTargetEndpoint.RegisterForIncomingCall<AudioVideoCall>(On_AudioVideoCall_Received);
            _transfereeEndpoint.RegisterForIncomingCall<AudioVideoCall>(On_AudioVideoCall_Received);
            
            // Setup the call objects for both the call from transferring 
            // to initial and transferring to final
            Conversation convInitialandTransferring = new Conversation(_transferorUserEndpoint);
            _avCallTransferringtoInitial = new AudioVideoCall(convInitialandTransferring);            

            // Perform the first call, from the transferor user to the initial 
            // user, without specifying any custom options.
            _avCallTransferringtoInitial.BeginEstablish(_transfereeURI, null, InitialCallEstablishCompleted, 
                _avCallTransferringtoInitial);
          
            _sampleCompleted.WaitOne();

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to shutdown and exit.");

            // And shutdown.
            _helper.ShutdownPlatform();
        }

        void AVCall_TransferReceived(object sender, AudioVideoCallTransferReceivedEventArgs e)
        {
            // Accept the transfer. The parameter is null as we do not want to 
            // add any specalized signaling headers to the acceptance.
            AudioVideoCall audioVideoCall = e.Accept(null);
            audioVideoCall.BeginEstablish(CallEstablishCompleted, audioVideoCall);
        }

        void AVCall_TransferStateChanged(object sender, TransferStateChangedEventArgs e)
        {
            // Simply log the events as they occur.
            Call call = sender as Call;

            // Call participants allow for disambiguation.
            Console.WriteLine("The transfer with Local Participant: " + call.Conversation.LocalParticipant +
                " and Remote Participant: " + call.RemoteEndpoint.Participant + 
                " has changed state. The previous call state was: " + e.PreviousState + 
                " and the current state is: " + e.State);
        }

        private void TransferCallCompleted(IAsyncResult ar)
        {
            AudioVideoCall audioVideoCall = ar.AsyncState as AudioVideoCall;

            try
            {
                // End transferring the incoming call.
                audioVideoCall.EndTransfer(ar);
            }
            catch (OperationFailureException OpFailEx)
            {
                // Operation failure exception can occur when the far end transfer
                // does not complete successfully, usually due to the transferee
                // failing to pick up.
                Console.WriteLine(OpFailEx.ToString());
            }

            // Again, just to sync the completion of the code.
            _sampleCompleted.Set();
           
        }

        void On_AudioVideoCall_Received(object sender, CallReceivedEventArgs<AudioVideoCall> e)
        {
            // Type checking was done by the platform; no risk of this being any
            // type other than the type expected.
            AudioVideoCall _audioVideoCall = e.Call;

            // Call: StateChanged: Only hooked up for logging, to show the call 
            // state transitions. Only bound on the incoming side, to avoid 
            // printing the events twice.
            _audioVideoCall.StateChanged += this.Call_StateChanged;

            // Remote Participant URI represents the far end (caller) in this 
            // conversation. Toast is the message set by the caller as the 'greet'
            // message for this call. In Microsoft Lync, the toast will 
            // show up in the lower-right of the screen.
            Console.WriteLine("Call Received! From: " + e.RemoteParticipant.Uri);

            try
            {
                // Bind the event handler for the transfer received event. 
                // This binding will cause the call to reply that call transfer 
                // is supported, when the call is accepted.
                _audioVideoCall.TransferReceived += this.AVCall_TransferReceived;
                
                // Now, accept the call. Threading note: EndAcceptCall will be 
                // raised on the same thread. Blocking this thread in this portion
                // of the code will cause endless waiting.
                _audioVideoCall.BeginAccept(AcceptAVCallCompleted, _audioVideoCall);
            }
            catch (InvalidOperationException exception)
            {
                // Call was disconnected before it could be accepted.
                Console.WriteLine(exception.ToString());
            }
        }

        private void AcceptAVCallCompleted(IAsyncResult ar)
        {
            AudioVideoCall audioVideoCall = ar.AsyncState as AudioVideoCall;

            try
            {
                // End accepting the incoming call.
                audioVideoCall.EndAccept(ar);
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
                
            }
        }

        private void InitialCallEstablishCompleted(IAsyncResult ar)
        {
            Call call = ar.AsyncState as Call;
            Exception ex = null;
            try
            {
                call.EndEstablish(ar);
                Console.WriteLine("The call with Local Participant: " + call.Conversation.LocalParticipant + 
                    " and Remote Participant: " + call.RemoteEndpoint.Participant + 
                    " is now in the established state.");
                // Perform the second call, from the transferor user to the 
                // final user, without specifying any custom options.
                Conversation convFinalandTransferring = new Conversation(_transferorUserEndpoint);
                AudioVideoCall avCallTransferringtoFinal = new AudioVideoCall(convFinalandTransferring);
                // Bind the event handler to display the events as the transferor
                // endpoint begins the transfer.
                avCallTransferringtoFinal.TransferStateChanged += this.AVCall_TransferStateChanged;
                avCallTransferringtoFinal.BeginEstablish(_transferTargetURI, null, 
                    TransferringCallEstablishCompleted, avCallTransferringtoFinal);
                
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code here.
                ex = opFailEx;
                
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // TODO (Left to the reader): Add error handling code here.
                ex = exception;                
            }
            finally
            {
                if (ex != null)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Terminating the application due to failure");
                    _sampleCompleted.Set();
                }
            }
        }


        private void TransferringCallEstablishCompleted(IAsyncResult ar)
        {
            AudioVideoCall call = ar.AsyncState as AudioVideoCall;
            RealTimeException ex = null;
            try
            {
                call.EndEstablish(ar);
                Console.WriteLine("");
                Console.WriteLine("Now beginning the transfer...");
                Console.WriteLine("");

                // Now that both calls are connected and have been accepted by 
                // the called participant, we can use the transferor endpoint to 
                // transfer the call from the initial to the final. Note that the
                // transferor endpoint retains it's position of control over both
                // calls. This form of transfer is most suitable for a proxy 
                // application, where the proxying agent needs to remain in the 
                // signaling channel (For instance, for quality assurance purposes.)
                call.BeginTransfer(_avCallTransferringtoInitial, TransferCallCompleted, call);
                Console.WriteLine("The call with Local Participant: " + call.Conversation.LocalParticipant + 
                    " and Remote Participant: " + call.RemoteEndpoint.Participant + 
                    " is now in the established state.");
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the 
                // call to the remote party.
                // TODO (Left to the reader): Add error handling code here.
                ex = opFailEx;                
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // TODO (Left to the reader): Add error handling code here.
                ex = exception;                
            }
            finally
            {
                if (ex != null)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Terminating the application due to failure");
                    _sampleCompleted.Set();
                }
            }
        }

        private void CallEstablishCompleted(IAsyncResult ar)
        {
            Call call = ar.AsyncState as Call;
            RealTimeException ex = null;
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
                ex = opFailEx;
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // TODO (Left to the reader): Add error handling code here.
                ex = exception;
            }
            finally
            {
                if (ex != null)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Terminating the application due to failure");
                    _sampleCompleted.Set();
                }
            }
        }

        // Just to record the state transitions in the console.
        void Call_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Call call = sender as Call;

            // Call participants allow for disambiguation.
            Console.WriteLine("The call with Local Participant: " + call.Conversation.LocalParticipant + 
                " and Remote Participant: " + call.RemoteEndpoint.Participant + 
                " has changed state. The previous call state was: " + e.PreviousState + 
                " and the current state is: " + e.State);
        }
        #endregion
    }
}
        
