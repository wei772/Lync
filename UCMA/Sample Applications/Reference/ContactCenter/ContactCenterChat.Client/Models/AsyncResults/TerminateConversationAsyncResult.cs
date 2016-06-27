/*=====================================================================
  File:      EstablishConversationAsyncResult.cs
 
  Summary:   Async result to perform conversation establishment.
 
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Collections.Generic;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;


///////////////////////////////////////////////////////////////////////////////

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.Models.AsyncResults
{

    #region internal class TerminateConversationAsyncResult

    internal class TerminateConversationAsyncResult : AsyncResultWithProcess<TerminateConversationResult>
    {

        #region private variables
        /// <summary>
        /// Conversation model.
        /// </summary>
        private readonly ConversationModel m_conversationModel;
        #endregion

        #region constructors
        /// <summary>
        /// Constructor to create new EstablishConversationAsyncResult.
        /// </summary>
        /// <param name="destinationQueue">Destination queue.</param>
        /// <param name="localParticipant">Local participant name.</param>
        /// <param name="userCallback">User callback.</param>
        /// <param name="state">User state.</param>
        internal TerminateConversationAsyncResult(ConversationModel conversationModel,
                                                    AsyncCallback userCallback,
                                                    object state)
            : base(userCallback, state)
        {
            Debug.Assert(null != conversationModel, "Conversation model is null.");
            m_conversationModel = conversationModel;
        }
        #endregion

        #region private properties

        /// <summary>
        /// Gets the Conversation model.
        /// </summary>
        private ConversationModel ConversationModel
        {
            get { return m_conversationModel; }
        }
        #endregion

        #region overridden methods

        /// <summary>
        /// Overridden process method.
        /// </summary>
        public override void Process()
        {
            bool unhandledExceptionDetected = true;
            Exception caughtException = null;
            try
            {
                WebConversation webConversation = this.ConversationModel.WebConversation;
                if (webConversation != null)
                {
                    this.ConversationModel.ContactCenterService.BeginTerminateConversation(webConversation,
                                                                                        this.ConversationTerminationCompleted,
                                                                                        null /*state*/);
                }
                else
                {
                    this.CompleteConversationTermination();
                }
                unhandledExceptionDetected = false;
            }
            catch (FaultException<OperationFault> fex)
            {
                caughtException = fex;
                unhandledExceptionDetected = false;
            }
            finally
            {
                if (unhandledExceptionDetected)
                {
                    Debug.Assert(null == caughtException, "Caught exception is not null");
                    caughtException = new Exception(ExceptionResource.UnhandledExceptionOccured);
                }

                if (caughtException != null)
                {
                    //Even though exception occured terminate the conversation because app might not be ready to handle exceptions during termination.
                    this.CompleteConversationTermination();
                }
            }
        }
        #endregion

        #region private methods

        /// <summary>
        /// Completes this operation.
        /// </summary>
        private void CompleteConversationTermination()
        {
            TerminateConversationResult result = new TerminateConversationResult();
            this.ConversationModel.TryUpdateState(ConversationModelState.Terminated);
            this.Complete(result);
        }

        /// <summary>
        /// conv termination completed callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void ConversationTerminationCompleted(IAsyncResult asyncResult)
        {
            Exception caughtException = null;
            bool unhandledExceptionDetected = true;
            try
            {
                this.ConversationModel.ContactCenterService.EndTerminateConversation(asyncResult);
                unhandledExceptionDetected = false;
            }
            catch (FaultException<OperationFault> fex)
            {
                caughtException = fex;
                unhandledExceptionDetected = false;
            }
            finally
            {
                if (unhandledExceptionDetected)
                {
                    Debug.Assert(null == caughtException, "Caught exception is not null");
                    caughtException = new Exception(ExceptionResource.UnhandledExceptionOccured);
                }

                this.CompleteConversationTermination();
            }
        }

        #endregion
    }

    #endregion
}
//////////////////////////////// End of File //////////////////////////////////
