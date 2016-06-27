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

    #region internal class EstablishConversationAsyncResult

    internal class EstablishConversationAsyncResult : AsyncResultWithProcess<EstablishConversationResult>
    {

        #region private variables
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
        /// <param name="localParticipant">Local participant name.</param>
        /// <param name="userCallback">User callback.</param>
        /// <param name="state">User state.</param>
        internal EstablishConversationAsyncResult(ConversationModel conversationModel,
                                                    Dictionary<string, string> context,
                                                    AsyncCallback userCallback,
                                                    object state)
            : base(userCallback, state)
        {
            Debug.Assert(null != conversationModel, "Conversation model is null.");
            m_conversationModel = conversationModel;
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
            catch(FaultException<OperationFault> fex) 
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

                if(caughtException != null) 
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
                unhandledExceptionDetected = false;
            }
            catch(FaultException<OperationFault> fex) 
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
                    Debug.Assert(null != webConversation, "For successful case web conversation should not be null");
                    EstablishConversationResult result = new EstablishConversationResult(webConversation);
                    this.Complete(result);
                }
            }
        }

        #endregion
    }

    #endregion
}
//////////////////////////////// End of File //////////////////////////////////
