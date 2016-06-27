/*=====================================================================
  File:      EstablishAvCallAsyncResult.cs
 
  Summary:   Async result to perform establish av call.
 
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

    #region internal class EstablishAvCallAsyncResult

    internal class AddClickToCallAsyncResult : AsyncResultWithProcess<AddClickToCallResult>
    {

        #region private variables

        /// <summary>
        /// Conversation model.
        /// </summary>
        private readonly ConversationModel m_conversationModel;

        /// <summary>
        /// Callback number.
        /// </summary>
        private string m_callbackNumber;
        #endregion

        #region constructors
        /// <summary>
        /// Constructor to create new EstablishConversationAsyncResult.
        /// </summary>
        /// <param name="conversationModel">Conversation model.</param>
        /// <param name="callbackNumber">Callback number.</param>
        /// <param name="userCallback">User callback.</param>
        /// <param name="state">User state.</param>
        internal AddClickToCallAsyncResult(ConversationModel conversationModel,
                                                    string callbackNumber,
                                                    AsyncCallback userCallback,
                                                    object state)
            : base(userCallback, state)
        {
            Debug.Assert(null != conversationModel, "Conversation model is null.");
            Debug.Assert(!String.IsNullOrEmpty(callbackNumber), "Callback number cannot be empty or null here");
            m_conversationModel = conversationModel;
            m_callbackNumber = callbackNumber;
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

        /// <summary>
        /// Callback number.
        /// </summary>
        private string CallbackNumber
        {
            get { return m_callbackNumber; }
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
            WebConversation webConversation = this.ConversationModel.WebConversation;
            try
            {
                this.ConversationModel.ContactCenterService.BeginEstablishAvCall(webConversation,
                                                                                        null /*destination*/,
                                                                                        this.CallbackNumber,
                                                                                        this.AvCallEstablishmentCompleted,
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
        /// av call establish completed callback method.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void AvCallEstablishmentCompleted(IAsyncResult asyncResult)
        {
            Exception caughtException = null;
            bool unhandledExceptionDetected = true;
            WebAvCall webAvCall = null;
            try
            {
                webAvCall = this.ConversationModel.ContactCenterService.EndEstablishAvCall(asyncResult);
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
                    Debug.Assert(null != webAvCall, "For successful case av call should not be null");
                    AddClickToCallResult result = new AddClickToCallResult(webAvCall.WebConversation);
                    this.Complete(result);
                }
            }
        }

        #endregion
    }

    #endregion
}
//////////////////////////////// End of File //////////////////////////////////
