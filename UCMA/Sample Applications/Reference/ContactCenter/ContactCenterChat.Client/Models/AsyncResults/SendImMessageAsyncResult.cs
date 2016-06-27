/*=====================================================================
  File:      SendImMessageAsyncResult.cs
 
  Summary:   Async result send an IM message.
 
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

    #region internal class SendImMessageAsyncResult

    internal class SendImMessageAsyncResult : AsyncResultWithProcess<SendImMessageResult>
    {

        #region private variables
        /// <summary>
        /// Message string.
        /// </summary>
        private readonly string m_message;

        /// <summary>
        /// Conversation model.
        /// </summary>
        private readonly ConversationModel m_conversationModel;
        #endregion

        #region constructors
        /// <summary>
        /// Constructor to create new SendImMessageAsyncResult.
        /// </summary>
        /// <param name="conversationModel">Conversation model.</param>
        /// <param name="message">Message.</param>
        /// <param name="userCallback">User callback.</param>
        /// <param name="state">User state.</param>
        internal SendImMessageAsyncResult(ConversationModel conversationModel,
                                                string message,
                                                AsyncCallback userCallback,
                                                object state)
            : base(userCallback, state)
        {
            Debug.Assert(null != conversationModel, "Conversation model is null.");
            m_conversationModel = conversationModel;
            m_message = message;
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
                this.ConversationModel.ContactCenterService.BeginSendImMessage(this.ConversationModel.WebConversation,
                                                                                        m_message,
                                                                                        this.SendImCompleted,
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
        /// conv establish completed callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void SendImCompleted(IAsyncResult asyncResult)
        {
            Exception caughtException = null;
            bool unhandledExceptionDetected = true;
            try
            {
                this.ConversationModel.ContactCenterService.EndSendImMessage(asyncResult);
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
                    SendImMessageResult result = new SendImMessageResult();
                    this.Complete(result);
                }
            }
        }

        #endregion
    }

    #endregion
}
//////////////////////////////// End of File //////////////////////////////////
