/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.Models.AsyncResults;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.Models
{
    public enum ConversationModelState
    {
        Idle = 0,
        Establishing = 1,
        Established = 2,
        Terminating = 3,
        Terminated = 4
    }

    /// <summary>
    /// Represents conversation model.
    /// </summary>
    public class ConversationModel
    {

        #region private const

        /// <summary>
        /// Product id.
        /// </summary>
        private const string ProductId = "ProductId";
        #endregion

        #region private variables

        /// <summary>
        /// Subject.
        /// </summary>
        private string m_subject;

        /// <summary>
        /// Initialt state is idle.
        /// </summary>
        private ConversationModelState m_state = ConversationModelState.Idle;

        /// <summary>
        /// Sync root object.
        /// </summary>
        private readonly object m_syncRoot = new object();

        /// <summary>
        /// Local participant
        /// </summary>
        private readonly string m_localParticipant;

        /// <summary>
        /// Contact Center service.
        /// </summary>
        private readonly ContactCenterService m_contactCenterservice;

        /// <summary>
        /// Web conversation associated with this model.
        /// </summary>
        private WebConversation m_webConversation;
        #endregion

        #region constructor

        /// <summary>
        /// Creates a new conversation model.
        /// </summary>
        public ConversationModel(ContactCenterService contactCenterService, string productId, string localParticipant)
        {
            Debug.Assert(null != contactCenterService, "ContactCenterService is null");
            m_subject = productId /*Make product id the subject of the conversation.*/;
            m_contactCenterservice = contactCenterService;
            m_localParticipant = localParticipant;
        }
        #endregion


        #region public event handlers

        /// <summary>
        /// Conversation state changed event args.
        /// </summary>
        public event EventHandler<ConversationStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Im message received event args.
        /// </summary>
        public event EventHandler<InstantMessageReceivedEventArgs> ImMessageReceived;

        /// <summary>
        /// Im message typing notification received.
        /// </summary>
        public event EventHandler<InstantMessageTypingNotificationReceivedEventArgs> ImTypingNotificationReceived;
    
        #endregion

        #region public properties

        /// <summary>
        /// Gets the current state.
        /// </summary>
        public ConversationModelState State
        {
            get { return m_state; }
        }

        /// <summary>
        /// Gets the subject of this conversation.
        /// </summary>
        public string Subject
        {
            get { return m_subject; }
        }

        /// <summary>
        /// Gets the local participant.
        /// </summary>
        public string LocalParticipant
        {
            get { return m_localParticipant; }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets the Contact center service.
        /// </summary>
        internal ContactCenterService ContactCenterService
        {
            get { return m_contactCenterservice; }
        }

        /// <summary>
        /// Gets the web conversation associated with this model.
        /// </summary>
        internal WebConversation WebConversation
        {
            get { return m_webConversation; }
            set 
            {
                m_webConversation = value; 
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Initiates a new conversation with the contact center service.
        /// </summary>
        /// <param name="destinationQueue">Destination queue. Cannot be null or empty.</param>
        /// <param name="localParticipant">Local participant name. Can be null or empty.</param>
        /// <param name="subject">Subject of the conversation. Can be null or empty.</param>
        /// <param name="productId">Product Id context. Can be null or empty.</param>
        /// <param name="callback">Callback method.</param>
        /// <param name="state">User state.</param>
        /// <returns>Async result reference.</returns>
        public IAsyncResult BeginEstablishConversation(string productId, AsyncCallback callback, object state)
        {
            Dictionary<string, string> convContext = null;
            lock (m_syncRoot)
            {
                if (!this.TryUpdateState(ConversationModelState.Establishing))
                {
                    throw new InvalidOperationException(ExceptionResource.InvalidState);
                }
                else
                {
                    if (!String.IsNullOrEmpty(productId))
                    {
                        convContext = ConversationModel.GetContextFromProductId(productId);
                    }
                    this.RegisterServiceEventHandlers();
                    EstablishConversationAsyncResult asyncResult = new EstablishConversationAsyncResult(this, convContext, callback, state);
                    asyncResult.Process();
                    return asyncResult;
                }
            }
        }

        /// <summary>
        /// Waits for corresponding begin operation to complete.
        /// </summary>
        /// <param name="asyncResult">Async result from the corresponding begin method.</param>
        public void EndEstablishConversation(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentException(ExceptionResource.InvalidAsyncResult);
            }
            else
            {
                EstablishConversationAsyncResult establishAsyncResult = asyncResult as EstablishConversationAsyncResult;
                if (establishAsyncResult == null)
                {
                    throw new ArgumentException(ExceptionResource.InvalidAsyncResult);
                }
                else
                {
                    establishAsyncResult.EndInvoke();
                }
            }
        }


        /// <summary>
        /// Establish
        /// </summary>
        /// <param name="destinationQueue">Destination queue. Cannot be null or empty.</param>
        /// <param name="callback">Callback method.</param>
        /// <param name="state">User state.</param>
        /// <returns>Async result reference.</returns>
        public IAsyncResult BeginEstablishConversationAndImCall(string destinationQueue, string productId, AsyncCallback callback, object state)
        {
            Dictionary<string, string> convContext = null;
            lock (m_syncRoot)
            {
                if (!this.TryUpdateState(ConversationModelState.Establishing))
                {
                    throw new InvalidOperationException(ExceptionResource.InvalidState);
                }
                else
                {
                    if (!String.IsNullOrEmpty(productId))
                    {
                        convContext = ConversationModel.GetContextFromProductId(productId);
                    }
                    this.RegisterServiceEventHandlers();
                    EstablishConversationAndImCallAsyncResult asyncResult = new EstablishConversationAndImCallAsyncResult(this, destinationQueue, convContext, callback, state);
                    asyncResult.Process();
                    return asyncResult;
                }
            }
        }

        /// <summary>
        /// Waits for corresponding begin operation to complete.
        /// </summary>
        /// <param name="asyncResult">Async result from the corresponding begin method.</param>
        public void EndEstablishConversationAndImCall(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentException(ExceptionResource.InvalidAsyncResult);
            }
            else
            {
                EstablishConversationAndImCallAsyncResult establishAsyncResult = asyncResult as EstablishConversationAndImCallAsyncResult;
                if (establishAsyncResult == null)
                {
                    throw new ArgumentException(ExceptionResource.InvalidAsyncResult);
                }
                else
                {
                    establishAsyncResult.EndInvoke();
                }
            }
        }


        /// <summary>
        /// Initiates an operation to send IM message to the remote side.
        /// </summary>
        /// <param name="callback">Callback method.</param>
        /// <param name="state">User state.</param>
        /// <returns>Async result reference.</returns>
        public IAsyncResult BeginSendImMessage(string message, AsyncCallback callback, object state)
        {
            if(this.State != ConversationModelState.Established) 
            {
                throw new InvalidOperationException(ExceptionResource.InvalidState);
            }
            SendImMessageAsyncResult asyncResult = new SendImMessageAsyncResult(this, message, callback, state);
            asyncResult.Process();
            return asyncResult;
        }

        /// <summary>
        /// Waits for corresponding begin operation to complete.
        /// </summary>
        /// <param name="asyncResult">Async result from the corresponding begin method.</param>
        public void EndSendImMessage(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentException(ExceptionResource.InvalidAsyncResult);
            }
            else
            {
                SendImMessageAsyncResult sendMessageAsyncResult = asyncResult as SendImMessageAsyncResult;
                if (sendMessageAsyncResult == null)
                {
                    throw new ArgumentException(ExceptionResource.InvalidAsyncResult);
                }
                else
                {
                    sendMessageAsyncResult.EndInvoke();
                }
            }
        }

        /// <summary>
        /// Initiates an operation to add av call to the conversation by providing a callback phone number.
        /// </summary>
        /// <param name="callback">Callback method.</param>
        /// <param name="state">User state.</param>
        /// <returns>Async result reference.</returns>
        public IAsyncResult BeginAddClickToCall(string callbackNumber, AsyncCallback callback, object state)
        {
            if (this.State != ConversationModelState.Established)
            {
                throw new InvalidOperationException(ExceptionResource.InvalidState);
            }
            if (String.IsNullOrEmpty(callbackNumber))
            {
                throw new ArgumentException(ExceptionResource.InvalidCallbackNumber);
            }
            AddClickToCallAsyncResult asyncResult = new AddClickToCallAsyncResult(this, callbackNumber, callback, state);
            asyncResult.Process();
            return asyncResult;
        }

        /// <summary>
        /// Waits for corresponding begin operation to complete.
        /// </summary>
        /// <param name="asyncResult">Async result from the corresponding begin method.</param>
        public void EndAddClickToCall(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentException(ExceptionResource.InvalidAsyncResult);
            }
            else
            {
                AddClickToCallAsyncResult addClickToCallAsyncResult = asyncResult as AddClickToCallAsyncResult;
                if (addClickToCallAsyncResult == null)
                {
                    throw new ArgumentException(ExceptionResource.InvalidAsyncResult);
                }
                else
                {
                    addClickToCallAsyncResult.EndInvoke();
                }
            }
        }

        /// <summary>
        /// Initiates a termination of this conversation with the contact center service.
        /// </summary>
        /// <param name="callback">Callback method.</param>
        /// <param name="state">User state.</param>
        /// <returns>Async result reference.</returns>
        public IAsyncResult BeginTerminateConversation(AsyncCallback callback, object state)
        {
            lock (m_syncRoot)
            {
                this.TryUpdateState(ConversationModelState.Terminating);
                TerminateConversationAsyncResult asyncResult = new TerminateConversationAsyncResult(this, callback, state);
                asyncResult.Process();
                return asyncResult;
            }
        }

        /// <summary>
        /// Waits for corresponding begin operation to complete.
        /// </summary>
        /// <param name="asyncResult">Async result from the corresponding begin method.</param>
        public void EndTerminateConversation(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentException(ExceptionResource.InvalidAsyncResult);
            }
            else
            {
                TerminateConversationAsyncResult terminateAsyncResult = asyncResult as TerminateConversationAsyncResult;
                if (terminateAsyncResult == null)
                {
                    throw new ArgumentException(ExceptionResource.InvalidAsyncResult);
                }
                else
                {
                    terminateAsyncResult.EndInvoke();
                }
            }
        }

        /// <summary>
        /// Starts conversation termination.
        /// </summary>
        public void SessionTerminated()
        {
            this.ContactCenterService.SessionTerminated(this.WebConversation);
        }


        #endregion

        #region internal methods

        
        #endregion

        #region private methods

        /// <summary>
        /// Conversation termination complete.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void ConversationTerminationComplete(IAsyncResult asyncResult)
        {
            ///Avoid throwing fault exceptions. Since this is a termination opeartion.
            this.EndTerminateConversation(asyncResult);
        }


        /// <summary>
        /// Creates context from product id.
        /// </summary>
        /// <param name="productId">Product id.</param>
        /// <returns>Context.</returns>
        private static Dictionary<string, string> GetContextFromProductId(string productId)
        {
            Debug.Assert(!String.IsNullOrEmpty(productId), "Product id is null or empty");
            Dictionary<string, string> retVal = new Dictionary<string, string>(1);
            retVal.Add(ConversationModel.ProductId, productId);
            return retVal;
        }

        /// <summary>
        /// Im typing notification handler.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ImComposingStatusNotificationReceivedHandler(object sender, ConversationNotificationReceivedEventArgs<ImComposingStatusNotification> e)
        {
            WebConversation webConversation = this.WebConversation;
            if (webConversation != null)
            {
                if (e.Notification.WebConversation.Id.Equals(webConversation.Id))
                {
                    var imTypingNotificationReceived = this.ImTypingNotificationReceived;
                    if (imTypingNotificationReceived != null)
                    {
                        var eventArgs = new InstantMessageTypingNotificationReceivedEventArgs(e.Notification.RemoteComposingStatus);
                        imTypingNotificationReceived(this, eventArgs);
                    }
                }
            }

        }

        /// <summary>
        /// Im received handler.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ImMessageReceivedHandler(object sender, ConversationNotificationReceivedEventArgs<ImMessageReceivedNotification> e)
        {
            WebConversation webConversation = this.WebConversation;
            if (webConversation != null)
            {
                if (e.Notification.WebConversation.Id.Equals(webConversation.Id))
                {
                    var imMessageReceived = this.ImMessageReceived;
                    if (imMessageReceived != null)
                    {
                        var eventArgs = new InstantMessageReceivedEventArgs(e.Notification.Message, e.Notification.MessageSender);
                        imMessageReceived(this, eventArgs);
                    }
                }
            }

        }


        /// <summary>
        /// Conversation terminated event handler.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ConversationTerminatedHandler(object sender, ConversationNotificationReceivedEventArgs<ConversationTerminatedNotification> e)
        {
            WebConversation webConversation = this.WebConversation;
            if (webConversation != null)
            {
                if (e.Notification.WebConversation.Id.Equals(webConversation.Id))
                {
                    lock (m_syncRoot)
                    {
                        this.TryUpdateState(ConversationModelState.Terminating);
                        this.TryUpdateState(ConversationModelState.Terminated);
                    }
                }
            }
        }

        private void RegisterServiceEventHandlers()
        {
            lock (m_syncRoot)
            {
                m_contactCenterservice.ConversationTerminated += this.ConversationTerminatedHandler;
                m_contactCenterservice.ImMessageReceived += this.ImMessageReceivedHandler;
                m_contactCenterservice.ImComposingStatusNotificationReceived += this.ImComposingStatusNotificationReceivedHandler;
            }
        }


        /// <summary>
        /// Unregister service event handlers.
        /// </summary>
        private void UnregisterServiceEventHandlers()
        {
            lock (m_syncRoot) 
            {
                m_contactCenterservice.ConversationTerminated -= this.ConversationTerminatedHandler;
                m_contactCenterservice.ImMessageReceived -= this.ImMessageReceivedHandler;
                m_contactCenterservice.ImComposingStatusNotificationReceived -= this.ImComposingStatusNotificationReceivedHandler;
            }
        }

        /// <summary>
        /// Conversation terminated.
        /// </summary>
        private void ConversationCleanup()
        {
            lock (m_syncRoot)
            {
                this.UnregisterServiceEventHandlers();
            }
        }

        /// <summary>
        /// State update
        /// </summary>
        /// <param name="newState">new state.</param>
        /// <returns>True if state update was successful.</returns>
        internal bool TryUpdateState(ConversationModelState newState)
        {
            bool stateUpdated = false;
            lock(m_syncRoot) 
            {
                ConversationModelState prevState = m_state;
                switch (prevState)
                {
                    case ConversationModelState.Idle:
                        stateUpdated = (newState == ConversationModelState.Establishing || newState == ConversationModelState.Terminating);
                        break;
                    case ConversationModelState.Establishing:
                        stateUpdated = (newState == ConversationModelState.Established || newState == ConversationModelState.Terminating);
                        break;
                    case ConversationModelState.Established:
                        stateUpdated = (newState == ConversationModelState.Terminating);
                        break;
                    case ConversationModelState.Terminating:
                        stateUpdated = (newState == ConversationModelState.Terminated);
                        break;
                    case ConversationModelState.Terminated:
                        this.ConversationCleanup();
                        break;
                    default:
                        break;
                }

                if (stateUpdated)
                {
                    m_state = newState;
                    EventHandler<ConversationStateChangedEventArgs> stateChangedEventHandler = this.StateChanged;
                    if (stateChangedEventHandler != null)
                    {
                        var eventArgs = new ConversationStateChangedEventArgs(prevState, newState);
                        stateChangedEventHandler(this, eventArgs);
                    }
                }
            }

            return stateUpdated;
        }

        #endregion
    }
}
