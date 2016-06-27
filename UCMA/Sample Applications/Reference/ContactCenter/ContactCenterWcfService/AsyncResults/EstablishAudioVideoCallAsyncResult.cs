/*=====================================================================
  File:      EstablishAudioVideoCallAsyncResult.cs
 
  Summary:   Async result to perform AV call establishment.
 
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

    internal class EstablishAudioVideoCallAsyncResult : AsyncResultWithProcess<EstablishAudioVideoCallResponse>
    {

        #region private variables
        /// <summary>
        /// Im call to establish.
        /// </summary>
        private readonly AudioVideoCall m_avCall;

        /// <summary>
        /// Custom Mime part to use.
        /// </summary>
        private readonly MimePartContentDescription m_cutomMimePart;

        /// <summary>
        /// Establish av call request.
        /// </summary>
        private readonly EstablishAudioVideoCallRequest m_establishAvCallRequest;

        /// <summary>
        /// Web conversation to use.
        /// </summary>
        private readonly WebConversation m_webConversation;

        /// <summary>
        /// Destination uri. Can be null or empty.
        /// </summary>
        private readonly string m_destinationUri;
        #endregion

        #region constructors
        /// <summary>
        /// Constructor to create new EstablishAudioVideoCallAsyncResult.
        /// </summary>
        /// <param name="request">Establish Av call request. cannot be null.</param>
        /// <param name="avCall">Audio Video call to establish. Cannot be null.</param>
        /// <param name="destinationUri">Destination uri.</param>
        /// <param name="webConversation">Web conversation to use.</param>
        /// <param name="customMimePart">Custom MIME part to use. Can be null.</param>
        /// <param name="userCallback">User callback.</param>
        /// <param name="state">User state.</param>
        internal EstablishAudioVideoCallAsyncResult(EstablishAudioVideoCallRequest request,
                                                            AudioVideoCall avCall,
                                                            string destinationUri,
                                                            WebConversation webConversation,
                                                            MimePartContentDescription customMimePart,
                                                            AsyncCallback userCallback,
                                                            object state)
            : base(userCallback, state)
        {
            Debug.Assert(null != avCall, "Av call is null");
            Debug.Assert(null != request, "Request is null");
            Debug.Assert(null != webConversation, "Web conversation to use is null.");

            m_cutomMimePart = customMimePart;
            m_establishAvCallRequest = request;
            m_avCall = avCall;
            m_webConversation = webConversation;
            m_destinationUri = destinationUri;
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
                //If callback number is not specified then establish audio call directly to the destination.
                this.EstablishAvCallDirectly();
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
        private void CompleteEstablishOperationSuccessfully(EstablishAudioVideoCallResponse result)
        {
            this.Complete(result);
        }

        /// <summary>
        /// Establishes av call directly to the destination.
        /// </summary>
        private void EstablishAvCallDirectly()
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
                m_avCall.BeginEstablish(m_destinationUri, establishOptions, this.AudioVideoCallEstablishCompleted, null /*state*/);
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
        /// Av call establish completed callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void AudioVideoCallEstablishCompleted(IAsyncResult asyncResult)
        {
            Exception exceptionCaught = null;
            bool exceptionEncountered = true;
            try
            {
                m_avCall.EndEstablish(asyncResult);
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
                    AudioVideoCallContext avCallContext = m_avCall.ApplicationContext as AudioVideoCallContext;
                    Debug.Assert(null != avCallContext, "Av call context is null");
                    Debug.Assert(null != avCallContext.WebAvcall, "Av call in av call context is null");

                    m_webConversation.WebAvCall = avCallContext.WebAvcall;

                    //If no exception occured, create a web av call.
                    EstablishAudioVideoCallResponse response = new EstablishAudioVideoCallResponse(m_establishAvCallRequest, avCallContext.WebAvcall);
                    this.CompleteEstablishOperationSuccessfully(response);
                }
            }
        }

        #endregion
    }
}
//////////////////////////////// End of File //////////////////////////////////
