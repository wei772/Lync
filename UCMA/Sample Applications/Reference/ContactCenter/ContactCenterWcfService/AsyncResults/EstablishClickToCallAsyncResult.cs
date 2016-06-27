/*=====================================================================
  File:      EstablishClickToCallAsyncResult.cs
 
  Summary:   Async result to perform av call establishment by doing self transfer with callback number.
 
/********************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities;
using Microsoft.Rtc.Collaboration;
using System.Diagnostics;
using System.ServiceModel;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.AudioVideo;
using System.Collections.Generic;


///////////////////////////////////////////////////////////////////////////////

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.AsyncResults
{

    internal class EstablishClickToCallAsyncResult : AsyncResultWithProcess<EstablishAudioVideoCallResponse>
    {

        #region private variables

        /// <summary>
        /// Establish av call request.
        /// </summary>
        private readonly EstablishAudioVideoCallRequest m_establishAvCallRequest;

        /// <summary>
        /// Callback call.
        /// </summary>
        private AudioVideoCall m_callbackCall;

        /// <summary>
        /// Completing delegate. Will be invoked just before completion.
        /// </summary>
        private Action<EstablishClickToCallAsyncResult> m_completingDelegate;

        /// <summary>
        /// Web conversation to use.
        /// </summary>
        private readonly WebConversation m_webConversation;
        #endregion

        #region constructors
        /// <summary>
        /// Constructor to create new EstablishClickToCallAsyncResult.
        /// </summary>
        /// <param name="request">Establish Av call request. cannot be null.</param>
        /// <param name="callbackAvCall">Callback Audio Video call to establish. Cannot be null.</param>
        /// <param name="webConversation">Web conversation to use.</param>
        /// <param name="completingActionDelegate">Delegate to be invoked just before completion of this async result.</param>
        /// <param name="userCallback">User callback.</param>
        /// <param name="state">User state.</param>
        internal EstablishClickToCallAsyncResult(EstablishAudioVideoCallRequest request,
                                                            AudioVideoCall callbackAvCall,
                                                            WebConversation webConversation,
                                                            Action<EstablishClickToCallAsyncResult> completingActionDelegate,
                                                            AsyncCallback userCallback,
                                                            object state)
            : base(userCallback, state)
        {
            Debug.Assert(null != callbackAvCall, "Av call is null");
            Debug.Assert(null != request, "Request is null");
            Debug.Assert(!String.IsNullOrEmpty(request.CallbackPhoneNumber), "Callback number is null or empty");
            Debug.Assert(null != webConversation, "Web conversation is null");

            m_establishAvCallRequest = request;
            m_callbackCall = callbackAvCall;
            m_completingDelegate = completingActionDelegate;
            m_webConversation = webConversation;
        }
        #endregion


        #region internal properties

        /// <summary>
        /// Gets the callback call. Can be null if there is no callback call yet.
        /// </summary>
        internal AudioVideoCall CallbackCall
        {
            get { return m_callbackCall; }
        }

        /// <summary>
        /// Gets the Web conversation to use.
        /// </summary>
        internal WebConversation WebConversation
        {
            get { return m_webConversation; }
        }

        /// <summary>
        /// Gets the destination uri from Request.
        /// </summary>
        internal string DestinationFromRequest
        {
            get { return m_establishAvCallRequest.Destination; }
        }
        #endregion

        #region overridden methods

        /// <summary>
        /// Overridden process method.
        /// </summary>
        public override void Process()
        {
            bool exceptionEncountered = true;
            try
            {
                this.EstablishClickToCall();

                exceptionEncountered = false;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    this.CompleteEstablishOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }
        #endregion

        #region internal methods

        /// <summary>
        /// Establishes back to back call due to incoming self transfer.
        /// </summary>
        /// <param name="b2bCall">Pre populated b2b call. Cannot be null.</param>
        internal void EstablishBackToBackCall(BackToBackCall b2bCall)
        {
            Debug.Assert(null != b2bCall, "B2B call is null");
            Exception exceptionCaught = null;
            bool exceptionEncountered = true;
            try
            {
                b2bCall.BeginEstablish(this.BackToBackCallEstablishCompleted, b2bCall /*state*/);
                exceptionEncountered = false;
            }
            catch (InvalidOperationException ioe)
            {
                Helper.Logger.Info("Exception = {0}", EventLogger.ToString(ioe));
                exceptionCaught = ioe;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = null;
                    if (exceptionCaught != null)
                    {
                        operationFault = FaultHelper.CreateClientOperationFault(exceptionCaught.Message, exceptionCaught.InnerException);
                    }
                    else
                    {
                        operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    }
                    this.CompleteEstablishOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }



        #endregion

        #region private methods

        /// <summary>
        /// Completes the establish operation.
        /// </summary>
        private void CompleteEstablishOperationWithException(Exception exception)
        {
            this.InvokeCompletingDelegateIfNeeded();
            this.CleanupCallbackCallIfNeeded();
            this.Complete(exception);
        }

        /// <summary>
        /// Completes the establish operation with a valid result.
        /// </summary>
        /// <param name="result">Result. Cannot be null.</param>
        private void CompleteEstablishOperationSuccessfully(EstablishAudioVideoCallResponse result)
        {
            this.InvokeCompletingDelegateIfNeeded();
            this.CleanupCallbackCallIfNeeded();
            this.Complete(result);
        }

        /// <summary>
        /// Method to clean up callback call.
        /// </summary>
        private void CleanupCallbackCallIfNeeded()
        {
            //In exception cases terminate temporary callback av call.
            AudioVideoCall callbackCall = m_callbackCall;
            if (callbackCall != null)
            {
                callbackCall.BeginTerminate(
                                delegate(IAsyncResult result)
                                {
                                    callbackCall.EndTerminate(result);
                                },
                                callbackCall);
            }
        }

        /// <summary>
        /// Invoke completing delegate if needed.
        /// </summary>
        private void InvokeCompletingDelegateIfNeeded()
        {
            Action<EstablishClickToCallAsyncResult> completionDelegate = m_completingDelegate;
            if (completionDelegate != null)
            {
                completionDelegate(this);
            }
        }

        /// <summary>
        /// Establish call by first calling the phone number and then doing self transfer and then back to backing with the destination.
        /// </summary>
        private void EstablishClickToCall()
        {
            bool exceptionEncountered = true;
            Exception exceptionCaught = null;
            try
            {
                string callbackPhoneUri = Helper.GetCallbackPhoneUri(m_establishAvCallRequest.CallbackPhoneNumber);
                m_callbackCall.BeginEstablish(callbackPhoneUri, null /*establishOptions*/, this.CallbackAudioVideoCallEstablishCompleted, null /*state*/);
                exceptionEncountered = false;
            }
            catch (ArgumentException ae)
            {
                Helper.Logger.Error("Exception = {0}", EventLogger.ToString(ae));
                exceptionCaught = ae;
            }
            catch (InvalidOperationException ioe)
            {
                Helper.Logger.Info("Exception = {0}", EventLogger.ToString(ioe));
                exceptionCaught = ioe;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = null;
                    if (exceptionCaught != null)
                    {
                        operationFault = FaultHelper.CreateClientOperationFault(exceptionCaught.Message, exceptionCaught.InnerException);
                    }
                    else
                    {
                        operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    }

                    this.CompleteEstablishOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }

        /// <summary>
        /// Callback Av call establish completed callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void CallbackAudioVideoCallEstablishCompleted(IAsyncResult asyncResult)
        {
            Exception exceptionCaught = null;
            bool exceptionEncountered = true;
            try
            {
                m_callbackCall.EndEstablish(asyncResult);
                //If establish operation succeeds immediately trigger a self transfer.
                m_callbackCall.BeginTransfer(m_callbackCall, null/*transferOptions*/, this.SelfTransferCompleted, null /*asyncState*/);
                exceptionEncountered = false;
            }
            catch (InvalidOperationException ioe)
            {
                Helper.Logger.Info("Exception = {0}", EventLogger.ToString(ioe));
                exceptionCaught = ioe;
            }
            catch (RealTimeException rte)
            {
                Helper.Logger.Info("Exception = {0}", EventLogger.ToString(rte));
                exceptionCaught = rte;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = null;
                    if (exceptionCaught != null)
                    {
                        operationFault = FaultHelper.CreateClientOperationFault(exceptionCaught.Message, exceptionCaught.InnerException);
                    }
                    else
                    {
                        operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    }
                    this.CompleteEstablishOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }

        /// <summary>
        /// self transfer has completed.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void SelfTransferCompleted(IAsyncResult asyncResult)
        {
            Exception exceptionCaught = null;
            bool exceptionEncountered = true;
            try
            {
                m_callbackCall.EndTransfer(asyncResult);
                //Self Transfer has completed. successfully.
                exceptionEncountered = false;
            }
            catch (RealTimeException rte)
            {
                Helper.Logger.Info("Exception = {0}", EventLogger.ToString(rte));
                exceptionCaught = rte;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = null;
                    if (exceptionCaught != null)
                    {
                        operationFault = FaultHelper.CreateClientOperationFault(exceptionCaught.Message, exceptionCaught.InnerException);
                    }
                    else
                    {
                        operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    }

                    this.CompleteEstablishOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }

        /// <summary>
        /// B2B call establish completed callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void BackToBackCallEstablishCompleted(IAsyncResult asyncResult)
        {
            var b2bCall = asyncResult.AsyncState as BackToBackCall;
            Debug.Assert(null != b2bCall, "Async state is null");

            Exception exceptionCaught = null;
            bool exceptionEncountered = true;
            try
            {
                b2bCall.EndEstablish(asyncResult);
                //If the call establishment is successful, stamp the b2b call in the web conversation.
                this.WebConversation.BackToBackCall = b2bCall;
                exceptionEncountered = false;
            }
            catch (RealTimeException rte)
            {
                Helper.Logger.Info("Exception = {0}", EventLogger.ToString(rte));
                exceptionCaught = rte;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = null;
                    if (exceptionCaught != null)
                    {
                        operationFault = FaultHelper.CreateClientOperationFault(exceptionCaught.Message, exceptionCaught.InnerException);
                    }
                    else
                    {
                        operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    }
                    this.CompleteEstablishOperationWithException(new FaultException<OperationFault>(operationFault));
                }
                else
                {
                    AudioVideoCallContext avCallContext = b2bCall.Call1.ApplicationContext as AudioVideoCallContext;
                    Debug.Assert(null != avCallContext, "Av call context is null");
                    Debug.Assert(null != avCallContext.WebAvcall, "Av call in av call context is null");
                    EstablishAudioVideoCallResponse response = new EstablishAudioVideoCallResponse(m_establishAvCallRequest, avCallContext.WebAvcall);
                    this.CompleteEstablishOperationSuccessfully(response);
                }
            }
        }

        #endregion
    }
}
//////////////////////////////// End of File //////////////////////////////////
