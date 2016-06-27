/*=====================================================================
  File:      TermianteConversationAsyncResult.cs
 
  Summary:   Async result to terminate a web conversation.
 
/*******************************************************************************
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


///////////////////////////////////////////////////////////////////////////////

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.AsyncResults
{

    internal class TerminateConversationAsyncResult : AsyncResultWithProcess<TerminateConversationResponse>
    {

        #region private variables
        /// <summary>
        /// Web conversation to terminate.
        /// </summary>
        private readonly WebConversation m_webConversation;

        /// <summary>
        /// Terminate conversation request.
        /// </summary>
        private readonly TerminateConversationRequest m_terminateConversationRequest;

        #endregion

        #region constructors
        /// <summary>
        /// Constructor to create new TerminateConversationAsyncResult.
        /// </summary>
        /// <param name="request">Terminate conversation request. cannot be null.</param>
        /// <param name="webConversation">Web conversation to terminate. Cannot be null.</param>
        /// <param name="userCallback">User callback.</param>
        /// <param name="state">User state.</param>
        internal TerminateConversationAsyncResult(TerminateConversationRequest request,
                                                WebConversation webConversation,
                                                AsyncCallback userCallback,
                                                object state)
            : base(userCallback, state)
        {
            Debug.Assert(null != webConversation, "Web conversation is null");
            Debug.Assert(null != request, "Request is null");

            m_webConversation = webConversation;
            m_terminateConversationRequest = request;
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
            try
            {
                //First terminate all calls and then terminate the conversation.
                this.TerminateInstantMessagingCall();
                exceptionEncountered = false;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    this.CompleteTerminateConversationOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }
        #endregion

        #region private methods

        /// <summary>
        /// Completes the terminate conversation operation.
        /// </summary>
        private void CompleteTerminateConversationOperationWithException(Exception exception)
        {
            this.Complete(exception);
        }

        /// <summary>
        /// Completes the terminate conversation operation with a valid result.
        /// </summary>
        /// <param name="result">Result. Cannot be null.</param>
        private void CompleteTerminateConversationOperationSuccessfully(TerminateConversationResponse result)
        {
            this.Complete(result);
        }

        /// <summary>
        /// Terminate Im call.
        /// </summary>
        private void TerminateInstantMessagingCall()
        {
            bool exceptionEncountered = true;
            try
            {
                InstantMessagingCall imCall = null;
                WebImCall webImCall = m_webConversation.WebImCall;
                if (webImCall != null)
                {
                    imCall = webImCall.ImCall;
                }
                if (imCall != null)
                {
                    imCall.BeginTerminate(this.InstantMessagingCallTerminated, imCall);
                }
                else
                {
                    //Go to next step of terminating av call.
                    this.TerminateAudioVideoCall();
                }

                exceptionEncountered = false;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    this.CompleteTerminateConversationOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }

        /// <summary>
        /// Terminate av call.
        /// </summary>
        private void TerminateAudioVideoCall()
        {
            bool exceptionEncountered = true;
            try
            {
                AudioVideoCall avCall = null;
                if (m_webConversation.WebAvCall != null)
                {
                    avCall = m_webConversation.WebAvCall.AvCall;
                }
                if (avCall != null)
                {
                    avCall.BeginTerminate(this.AudioVideoCallTerminated, avCall);
                }
                else
                {
                    //Go to next step of terminating b2b call.
                    this.TerminateBackToBackCall();
                }

                exceptionEncountered = false;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    this.CompleteTerminateConversationOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }


        /// <summary>
        /// Terminate b2b call.
        /// </summary>
        private void TerminateBackToBackCall()
        {
            bool exceptionEncountered = true;
            try
            {

                BackToBackCall b2bCall = m_webConversation.BackToBackCall;
                if (b2bCall != null)
                {
                    b2bCall.BeginTerminate(this.BackToBackCallTerminated, b2bCall);
                }
                else
                {
                    //Go to next step of terminating conversation.
                    this.TerminateConversation();
                }

                exceptionEncountered = false;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    this.CompleteTerminateConversationOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }

        /// <summary>
        /// Terminate conversation.
        /// </summary>
        private void TerminateConversation()
        {
            bool exceptionEncountered = true;
            try
            {
                Conversation conversation = m_webConversation.Conversation;
                if (conversation != null)
                {
                    conversation.BeginTerminate(this.ConversationTerminated, conversation);
                }
                else
                {
                    this.TerminationWorkCompleted();
                }

                exceptionEncountered = false;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    this.CompleteTerminateConversationOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }

        /// <summary>
        /// Termination work completed handler.
        /// </summary>
        private void TerminationWorkCompleted()
        {
            TerminateConversationResponse response = new TerminateConversationResponse(m_terminateConversationRequest);
            this.CompleteTerminateConversationOperationSuccessfully(response);
        }


        /// <summary>
        /// Im call establish completed callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void InstantMessagingCallTerminated(IAsyncResult asyncResult)
        {
            bool exceptionEncountered = true;
            try
            {
                InstantMessagingCall imCall = asyncResult.AsyncState as InstantMessagingCall;
                imCall.EndTerminate(asyncResult);

                //Now try to terminate av call.
                this.TerminateAudioVideoCall();

                exceptionEncountered = false;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    this.CompleteTerminateConversationOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }

        /// <summary>
        /// Av call establish completed callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void AudioVideoCallTerminated(IAsyncResult asyncResult)
        {
            bool exceptionEncountered = true;
            try
            {
                AudioVideoCall avCall = asyncResult.AsyncState as AudioVideoCall;
                avCall.EndTerminate(asyncResult);

                //Now try to terminate b2bcall.
                this.TerminateBackToBackCall();

                exceptionEncountered = false;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    this.CompleteTerminateConversationOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }


        /// <summary>
        /// B2B call terminate completed callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void BackToBackCallTerminated(IAsyncResult asyncResult)
        {
            Exception exceptionCaught = null;
            bool exceptionEncountered = true;
            try
            {
                BackToBackCall b2bCall = asyncResult.AsyncState as BackToBackCall;
                b2bCall.EndTerminate(asyncResult);

                //Now try to terminate conversation.
                this.TerminateConversation();

                exceptionEncountered = false;
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
                    this.CompleteTerminateConversationOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }


        /// <summary>
        /// Conversation terminated callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void ConversationTerminated(IAsyncResult asyncResult)
        {
            bool exceptionEncountered = true;
            try
            {
                Conversation conv = asyncResult.AsyncState as Conversation;
                conv.EndTerminate(asyncResult);

                //Invoke all done.
                this.TerminationWorkCompleted();

                exceptionEncountered = false;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    this.CompleteTerminateConversationOperationWithException(new FaultException<OperationFault>(operationFault));
                }
            }
        }


        #endregion
    }
}
//////////////////////////////// End of File //////////////////////////////////
