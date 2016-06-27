/*=====================================================================
  File:      EstablishConversationAndImCallAsyncResult.cs
 
  Summary:   Async result to perform conversation establishment and im call.
 
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

    #region internal class EstablishConversationAndImCallAsyncResult

    internal class EstablishConversationAndImCallAsyncResult : AsyncResultWithProcess<EstablishConversationAndImCallResult>
    {

        #region private variables
        /// <summary>
        /// Destination queue.
        /// </summary>
        private readonly string m_destinationQueue;

        /// <summary>
        /// Context.
        /// </summary>
        private readonly Dictionary<string, string> m_context;

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
        internal EstablishConversationAndImCallAsyncResult(ConversationModel conversationModel,
                                                    string destinationQueue,
                                                    Dictionary<string, string> context,
                                                    AsyncCallback userCallback,
                                                    object state)
            : base(userCallback, state)
        {
            Debug.Assert(null != conversationModel, "Conversation model is null.");
            m_conversationModel = conversationModel;
            m_destinationQueue = destinationQueue;
            m_context = context;
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
                this.ConversationModel.ContactCenterService.BeginEstablishConversation(this.ConversationModel.LocalParticipant,
                                                                                        this.ConversationModel.Subject,
                                                                                        m_context,
                                                                                        this.ConversationEstablishmentCompleted,
                                                                                        null /*state*/);
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
                    this.Complete(caughtException);
                }
            }
        }
        #endregion

        #region private methods

        /// <summary>
        /// Callback for begin terminate conversation.
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
            }
        }


        /// <summary>
        /// conv establish completed callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void ConversationEstablishmentCompleted(IAsyncResult asyncResult)
        {
            Exception caughtException = null;
            bool unhandledExceptionDetected = true;
            WebConversation webConversation = null;
            try
            {
                webConversation = this.ConversationModel.ContactCenterService.EndEstablishConversation(asyncResult);
                this.ConversationModel.WebConversation = webConversation;
                if (!this.ConversationModel.TryUpdateState(ConversationModelState.Established))
                {
                    caughtException = new Exception(ExceptionResource.InvalidState);
                    //Start termination operation since we cannot update state.
                    this.ConversationModel.BeginTerminateConversation(this.ConversationTerminationCompleted, null);
                }
                else
                {
                    //Proceed to establish IM call.
                    this.ConversationModel.ContactCenterService.BeginEstablishImCall(webConversation, m_destinationQueue, this.ImCallEstablishmentCompleted, webConversation /*state*/);
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
                    this.Complete(caughtException);
                }
            }
        }


        /// <summary>
        /// Im call established completed.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void ImCallEstablishmentCompleted(IAsyncResult asyncResult)
        {
            Exception caughtException = null;
            bool unhandledExceptionDetected = true;
            WebConversation webConversation = asyncResult.AsyncState as WebConversation;
            Debug.Assert(null != webConversation, "Web conversation is null");
            try
            {
                this.ConversationModel.ContactCenterService.EndEstablishImCall(asyncResult);
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
                    this.Complete(caughtException);
                }
                else
                {
                    EstablishConversationAndImCallResult result = new EstablishConversationAndImCallResult(webConversation);
                    this.Complete(result);
                }
            }
        }

        #endregion
    }

    #endregion
}
//////////////////////////////// End of File //////////////////////////////////
