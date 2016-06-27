/*=====================================================================
  File:      SendInstantMessageAsyncResult.cs
 
  Summary:   Async result to send IM message.
 
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

    internal class SendInstantMessageAsyncResult : AsyncResultWithProcess<SendInstantMessageResponse>
    {

        #region private variables
        /// <summary>
        /// Im flow to use.
        /// </summary>
        private readonly InstantMessagingFlow  m_imFlow;

        /// <summary>
        /// Send Im message request.
        /// </summary>
        private readonly SendInstantMessageRequest m_sendImMessageRequest;

        /// <summary>
        /// Text to send.
        /// </summary>
        private readonly string m_textBody;
        #endregion

        #region constructors
        /// <summary>
        /// Constructor to create new SendInstantMessageAsyncResult.
        /// </summary>
        /// <param name="request">Send Im message request. cannot be null.</param>
        /// <param name="imFlow">Instant messaging flow. Cannot be null.</param>
        /// <param name="textBody">Text body to send.</param>
        /// <param name="userCallback">User callback.</param>
        /// <param name="state">User state.</param>
        internal SendInstantMessageAsyncResult(SendInstantMessageRequest request,
                                                InstantMessagingFlow imFlow,
                                                string textBody,
                                                AsyncCallback userCallback,
                                                object state)
            : base(userCallback, state)
        {
            Debug.Assert(null != imFlow, "Im flow is null");
            Debug.Assert(null != request, "Request is null");

            m_imFlow = imFlow;
            m_sendImMessageRequest = request;
            m_textBody = textBody;
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
               
                m_imFlow.BeginSendInstantMessage(m_textBody, this.InstantMessagingSendMessageCompleted, null /*state*/);
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

                    this.CompleteSendMessageOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }
        #endregion

        #region private methods

        /// <summary>
        /// Completes the send message operation.
        /// </summary>
        private void CompleteSendMessageOperationWithException(Exception exception)
        {
            this.Complete(exception);
        }

        /// <summary>
        /// Completes the send message operation with a valid result.
        /// </summary>
        /// <param name="result">Result. Cannot be null.</param>
        private void CompleteSendMessageOperationSuccessfully(SendInstantMessageResponse result)
        {
            this.Complete(result);
        }

        /// <summary>
        /// Im call establish completed callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void InstantMessagingSendMessageCompleted(IAsyncResult asyncResult)
        {
            Exception exceptionCaught = null;
            bool exceptionEncountered = true;
            try
            {
                m_imFlow.EndSendInstantMessage(asyncResult);
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
                    this.CompleteSendMessageOperationWithException(new FaultException<OperationFault>(operationFault));
                }
                else
                {
                    SendInstantMessageResponse response = new SendInstantMessageResponse(m_sendImMessageRequest);
                    this.CompleteSendMessageOperationSuccessfully(response);
                }
            }
        }

        #endregion
    }
}
//////////////////////////////// End of File //////////////////////////////////
