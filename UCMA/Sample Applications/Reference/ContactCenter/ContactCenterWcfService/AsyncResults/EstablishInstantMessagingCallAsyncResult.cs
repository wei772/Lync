/*=====================================================================
  File:      EstablishInstantMessagingCallAsyncResult.cs
 
  Summary:   Async result to perform IM call establishment.
 
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


///////////////////////////////////////////////////////////////////////////////

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.AsyncResults
{

    internal class EstablishInstantMessagingCallAsyncResult : AsyncResultWithProcess<EstablishInstantMessagingCallResponse>
    {

        #region private variables
        /// <summary>
        /// Im call to establish.
        /// </summary>
        private readonly InstantMessagingCall m_imCall;

        /// <summary>
        /// Custom Mime part to use.
        /// </summary>
        private readonly MimePartContentDescription m_cutomMimePart;

        /// <summary>
        /// Web conversation.
        /// </summary>
        private readonly WebConversation m_webConversation;

        /// <summary>
        /// Establish im call request.
        /// </summary>
        private readonly EstablishInstantMessagingCallRequest m_establishImCallRequest;

        /// <summary>
        /// Destination uri. Can be null or empty.
        /// </summary>
        private readonly string m_destinationUri;
        #endregion

        #region constructors
        /// <summary>
        /// Constructor to create new EstablishInstantMessagingCallAsyncResult.
        /// </summary>
        /// <param name="request">Establish Im call request. cannot be null.</param>
        /// <param name="webConversation">Web conversation.</param>
        /// <param name="destinationUri">Destination uri.</param>
        /// <param name="imCall">Instant messaging call to establish. Cannot be null.</param>
        /// <param name="customMimePart">Custom MIME part to use. Can be null.</param>
        /// <param name="userCallback">User callback.</param>
        /// <param name="state">User state.</param>
        internal EstablishInstantMessagingCallAsyncResult(EstablishInstantMessagingCallRequest request,
                                                            WebConversation webConversation,
                                                            string destinationUri,
                                                            InstantMessagingCall imCall, 
                                                            MimePartContentDescription customMimePart, 
                                                            AsyncCallback userCallback, 
                                                            object state)
            : base(userCallback, state)
        {
            Debug.Assert(null != imCall, "Im call is null");
            Debug.Assert(null != request, "Request is null");
            Debug.Assert(null != webConversation, "WebConversation is null");

            m_imCall = imCall;
            m_cutomMimePart = customMimePart;
            m_establishImCallRequest = request;
            m_webConversation = webConversation;
            m_destinationUri = destinationUri;
        }
        #endregion


        #region private properties
        #endregion

        #region overridden methods

        /// <summary>
        /// Overridden process method.
        /// </summary>
        public override void Process()
        {
            bool exceptionEncountered = true;
            Exception exceptionCaught = null;
            try
            {
                CallEstablishOptions establishOptions = new CallEstablishOptions();
                //Add custom MIME parts based on conversation context.
                if (m_cutomMimePart != null)
                {
                    establishOptions.CustomMimeParts.Add(m_cutomMimePart);
                }
                //Construct the destination uri, if needed.
                m_imCall.BeginEstablish(m_destinationUri, establishOptions, this.InstantMessagingCallEstablishCompleted, null /*state*/);
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
                    if(exceptionCaught != null) 
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
            this.Complete(exception);
        }

        /// <summary>
        /// Completes the establish operation with a valid result.
        /// </summary>
        /// <param name="result">Result. Cannot be null.</param>
        private void CompleteEstablishOperationSuccessfully(EstablishInstantMessagingCallResponse result)
        {
            this.Complete(result);
        }

        /// <summary>
        /// Im call establish completed callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void InstantMessagingCallEstablishCompleted(IAsyncResult asyncResult)
        {
            Exception exceptionCaught = null;
            bool exceptionEncountered = true;
            try
            {
                m_imCall.EndEstablish(asyncResult);
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
                    InstantMessagingCallContext imCallContext = m_imCall.ApplicationContext as InstantMessagingCallContext;
                    Debug.Assert(null != imCallContext, "Im call context is null");
                    Debug.Assert(null != imCallContext.WebImcall, "Im call in Imcall context is null");
                    //Stamp im call.
                    m_webConversation.WebImCall = imCallContext.WebImcall;
                    EstablishInstantMessagingCallResponse response = new EstablishInstantMessagingCallResponse(m_establishImCallRequest, imCallContext.WebImcall);
                    this.CompleteEstablishOperationSuccessfully(response);
                }
            }
        }

        #endregion
    }
}
//////////////////////////////// End of File //////////////////////////////////
