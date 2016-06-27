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

namespace Microsoft.Rtc.Collaboration.Sample.CallTransferBasic
{
    public class UCMASampleCallTransferBasic
    {
        #region Locals
        // The difference between attended and unattended transfer is that 
        // unattended transfers begin the transfer (send the REFER to the far end)
        // and terminate the Call on receipt of the transfer request (202-Reply 
        // to the caller). Attended transfers wait to terminate the call until 
        // the subsequent call either succeeds or fails.
        private static CallTransferOptions _transferType = new CallTransferOptions(CallTransferType.Attended);

        private UserEndpoint _transferorEndpoint;
        
        private string _transferTargetUri;
        
        private AudioVideoCall _audioVideoCall;
        
        private UCMASampleHelper _helper;

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _waitForCallAccept = new AutoResetEvent(false);
        
        private AutoResetEvent _waitForTransferComplete = new AutoResetEvent(false);
        #endregion

        #region Methods
        // <summary>
        /// Instantiate and run the CallTransferBasic quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleCallTransferBasic ucmaSampleCallTransferBasic = new UCMASampleCallTransferBasic();
            ucmaSampleCallTransferBasic.Run();
        }

        private void Run()
        {
            // A helper class to take care of platform and endpoint setup and 
            // cleanup. This has been abstracted from this sample to focus on 
            // Call Control.
            _helper = new UCMASampleHelper();

            // Create a user endpoint, using the network credential object 
            // defined above.
            _transferorEndpoint = _helper.CreateEstablishedUserEndpoint("Transferor" /*endpointFriendlyName*/);


            // Get the URI of the user to transfer the call to.
            // Prepend URI with "sip:" if not present.
            _transferTargetUri = UCMASampleHelper.PromptUser("Enter a URI of the user to transfer the call to: ", "TransferTargetUri");
            if (!(_transferTargetUri.ToLower().StartsWith("sip:") || _transferTargetUri.ToLower().StartsWith("tel:")))
                _transferTargetUri = "sip:" + _transferTargetUri;


            // Here, we are accepting an AudioVideo call only. If the incoming 
            // call is not an AudioVideo call then it will not get raised to the
            // application. UCMA 3.0 handles this silently by having the call 
            // types register for various modalities (as part of the extensibility
            // framework). The appropriate action (here, accepting the call) 
            // will be handled in the handler assigned to the method call below.
            _transferorEndpoint.RegisterForIncomingCall<AudioVideoCall>(On_AudioVideoCall_Received);

            // Wait for the call to complete accept.
            Console.WriteLine(String.Empty);
            Console.WriteLine("Transferor waiting for incoming call...");
            _waitForCallAccept.WaitOne();
            Console.WriteLine("Initial call accepted by Transferor.");

            // Then transfer the call to another user, as designated above. 
            _audioVideoCall.BeginTransfer(_transferTargetUri, EndTransferCall, _audioVideoCall);
            Console.WriteLine("Waiting for transfer to complete...");
            
            // Wait for the call to complete the transfer.
            _waitForTransferComplete.WaitOne();
            Console.WriteLine("Transfer completed.");

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to shutdown and exit.");

            // Now that the call has completed, shutdown the platform.
            _helper.ShutdownPlatform();

        }

        void On_AudioVideoCall_Received(object sender, CallReceivedEventArgs<AudioVideoCall> e)
        {
            // Type checking was done by the platform; no risk of this being any 
            // type other than the type expected.
            _audioVideoCall = e.Call;

            // Call: StateChanged: Only hooked up for logging, to show the call 
            // state transitions.
            _audioVideoCall.StateChanged += this.AudioVideoCall_StateChanged;

            // Remote Participant URI represents the far end (caller) in this 
            // conversation. Toast is the message set by the caller as the 'greet'
            // message for this call. In Microsoft Lync, the toast will 
            // show up in the lower-right of the screen.
            Console.WriteLine("Call Received! From: " + e.RemoteParticipant.Uri);

            // Now, accept the call.
            // EndAcceptCall will be raised on the same thread.
            _audioVideoCall.BeginAccept(EndAcceptCall, _audioVideoCall);

        }

        private void EndAcceptCall(IAsyncResult ar)
        {
            AudioVideoCall audioVideoCall = ar.AsyncState as AudioVideoCall;

            try
            {

                // End accepting the incoming call.
                audioVideoCall.EndAccept(ar);
            }
            catch (OperationFailureException OpFailEx)
            {
                // Operation failure exception can occur when the far end transfer
                // does not complete successfully, usually due to the transferee 
                // failing to pick up. 
                Console.WriteLine(OpFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // Real time exception can occur when the far end transfer does 
                // not complete successfully, usually due to a link-layer or 
                // transport failure (i.e: Link dead, or failure response.).
                Console.WriteLine(realTimeEx.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _waitForCallAccept.Set();
            }

        }

        private void EndTransferCall(IAsyncResult ar)
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
                // does not complete successfully, usually due to the transferee failing to pick up.
                Console.WriteLine(OpFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // Real time exception can occur when the far end transfer does 
                // not complete successfully, usually due to a link-layer or 
                // transport failure (i.e: Link dead, or failure response.).
                Console.WriteLine(realTimeEx.ToString());
            }
            finally
            {
                //Again, just to sync the completion of the code.
                _waitForTransferComplete.Set();
            }
        }

        //Just to record the state transitions in the console.
        void AudioVideoCall_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Console.WriteLine("Call has changed state. The previous call state was: " + e.PreviousState + 
                " and the current state is: " + e.State);
        }
        #endregion
    }
}
