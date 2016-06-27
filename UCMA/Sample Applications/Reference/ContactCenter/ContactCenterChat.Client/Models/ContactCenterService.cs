/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Net;
using System.ServiceModel;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.Models
{
    /// <summary>
    /// Implementation of contact center service.
    /// </summary>
    public class ContactCenterService : IDisposable
    {

        #region private variables
        /// <summary>
        /// Client proxy.
        /// </summary>
        private ContactCenterWcfServiceClient m_contactCenterWcfServiceClient;

        /// <summary>
        /// Is disposed.
        /// </summary>
        private bool m_isDisposed;
        #endregion

        #region constructor

        /// <summary>
        /// Creates a new contact center service based on the endpoint uri.
        /// </summary>
        /// <param name="endpointUri">Endpoint uri. Cannot be null or empty.</param>
        public ContactCenterService(string endpointUri)
        {
            if (String.IsNullOrEmpty(endpointUri))
            {
                throw new ArgumentException("Endpoint uri is not valid", endpointUri);
            }

            EndpointAddress address = new EndpointAddress(endpointUri);
            PollingDuplexHttpBinding binding = new PollingDuplexHttpBinding();

            m_contactCenterWcfServiceClient = new ContactCenterWcfServiceClient(binding, address);

            this.RegisterEventHandlers(this.WcfClient);

        }
        #endregion

        #region private properties

        /// <summary>
        /// Gets the wcf client.
        /// </summary>
        private ContactCenterWcfServiceClient WcfClient
        {
            get { return m_contactCenterWcfServiceClient; }
        }
        #endregion

        #region public events
        /// <summary>
        /// Conversation terminated notification.
        /// </summary>
        public event EventHandler<ConversationNotificationReceivedEventArgs<ConversationTerminatedNotification>> ConversationTerminated;

        /// <summary>
        /// Im message received.
        /// </summary>
        public event EventHandler<ConversationNotificationReceivedEventArgs<ImMessageReceivedNotification>> ImMessageReceived;

        /// <summary>
        /// Composing status notification received.
        /// </summary>
        public event EventHandler<ConversationNotificationReceivedEventArgs<ImComposingStatusNotification>> ImComposingStatusNotificationReceived;
        #endregion

        #region public methods

        /// <summary>
        /// Dispose method.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true /*disposing*/);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose method.
        /// </summary>
        /// <param name="disposing">Disposing flag.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_isDisposed)
            {
                if (disposing)
                {
                    ContactCenterWcfServiceClient wcfClient = this.WcfClient;
                    if (wcfClient != null)
                    {
                        this.UnregisterEventHandlers(wcfClient);
                        CommunicationState commState = wcfClient.State;
                        if (commState == CommunicationState.Faulted)
                        {
                            wcfClient.Abort();
                        }
                        else
                        {
                            wcfClient.CloseAsync();
                        }
                    }

                    m_contactCenterWcfServiceClient = null;
                    m_isDisposed = true;
                }
            }
        }


        /// <summary>
        /// Initiates an operation to establish a new conversation with the contact center service.
        /// </summary>
        /// <param name="localParticipant">Local participant name. Can be null or empty.</param>
        /// <param name="subject">Subject of the conversation. Can be null or empty.</param>
        /// <param name="conversationContext">Conversation context. Can be null or empty.</param>
        /// <param name="callback">Callback method.</param>
        /// <param name="state">State.</param>
        /// <returns>Async result reference.</returns>
        public IAsyncResult BeginEstablishConversation(string localParticipant, string subject, Dictionary<string, string> conversationContext, AsyncCallback callback, object state)
        {
            //Create new request.
            CreateConversationRequest request = new CreateConversationRequest();
            request.RequestId = ContactCenterService.GenerateNewRequestId();
            request.DisplayName = localParticipant;
            request.ConversationSubject = subject;
            request.ConversationContext = conversationContext;
            return ((IContactCenterWcfService)this.WcfClient).BeginCreateConversation(request, callback, state);
        }

        /// <summary>
        /// Completes the corresponding begin operation.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        public WebConversation EndEstablishConversation(IAsyncResult asyncResult)
        {
            CreateConversationResponse response = ((IContactCenterWcfService)this.WcfClient).EndCreateConversation(asyncResult);
            return response.Conversation;
        }

        /// <summary>
        /// Initiates an operation to establish a new im call with the contact center service.
        /// </summary>
        /// <param name="webConversation">Web conversation.</param>
        /// <param name="destination">Destination.</param>
        /// <param name="callback">Callback method.</param>
        /// <param name="state">State.</param>
        /// <returns>Async result reference.</returns>
        public IAsyncResult BeginEstablishImCall(WebConversation webConversation, string destination, AsyncCallback callback, object state)
        {
            //Create new request.
            EstablishInstantMessagingCallRequest request = new EstablishInstantMessagingCallRequest();
            request.RequestId = ContactCenterService.GenerateNewRequestId();
            request.Conversation = webConversation;
            request.Destination = destination;
            return ((IContactCenterWcfService)this.WcfClient).BeginEstablishInstantMessagingCall(request, callback, state);
        }

        /// <summary>
        /// Completes the corresponding begin operation.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        public WebImCall EndEstablishImCall(IAsyncResult asyncResult)
        {
            EstablishInstantMessagingCallResponse response = ((IContactCenterWcfService)this.WcfClient).EndEstablishInstantMessagingCall(asyncResult);
            return response.ImCall;
        }

        /// <summary>
        /// Initiates an operation to establish a new av call with the contact center service.
        /// </summary>
        /// <param name="webConversation">Web conversation.</param>
        /// <param name="destination">Destination.</param>
        /// <param name="callback">Callback method.</param>
        /// <param name="state">State.</param>
        /// <returns>Async result reference.</returns>
        public IAsyncResult BeginEstablishAvCall(WebConversation webConversation, string destination, string callbackPhoneNumber, AsyncCallback callback, object state)
        {
            //Create new request.
            EstablishAudioVideoCallRequest request = new EstablishAudioVideoCallRequest();
            request.RequestId = ContactCenterService.GenerateNewRequestId();
            request.Conversation = webConversation;
            request.Destination = destination;
            request.CallbackPhoneNumber = callbackPhoneNumber;
            return ((IContactCenterWcfService)this.WcfClient).BeginEstablishAudioVideoCall(request, callback, state);
        }

        /// <summary>
        /// Completes the corresponding begin operation.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        public WebAvCall EndEstablishAvCall(IAsyncResult asyncResult)
        {
            EstablishAudioVideoCallResponse response = ((IContactCenterWcfService)this.WcfClient).EndEstablishAudioVideoCall(asyncResult);
            return response.AvCall;
        }

        /// <summary>
        /// Initiates an operation to establish a new im call with the contact center service.
        /// </summary>
        /// <param name="webConversation">Web conversation.</param>
        /// <param name="message">Message.</param>
        /// <param name="callback">Callback method.</param>
        /// <param name="state">State.</param>
        /// <returns>Async result reference.</returns>
        public IAsyncResult BeginSendImMessage(WebConversation webConversation, string message, AsyncCallback callback, object state)
        {
            //Create new request.
            SendInstantMessageRequest request = new SendInstantMessageRequest();
            request.RequestId = ContactCenterService.GenerateNewRequestId();
            request.Conversation = webConversation;
            request.Message = message;
            return ((IContactCenterWcfService)this.WcfClient).BeginSendInstantMessage(request, callback, state);
        }

        /// <summary>
        /// Completes the corresponding begin operation.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        public void EndSendImMessage(IAsyncResult asyncResult)
        {
            SendInstantMessageResponse response = ((IContactCenterWcfService)this.WcfClient).EndSendInstantMessage(asyncResult);
        }

        /// <summary>
        /// Initiates a conversation termination operation.
        /// </summary>
        /// <param name="webConversation">Web conversation.</param>
        /// <param name="asyncCallback">Async callback.</param>
        /// <param name="state">State.</param>
        /// <returns>Async result reference</returns>
        public IAsyncResult BeginTerminateConversation(WebConversation webConversation, AsyncCallback callback, object state)
        {
            //Create new request.
            if (webConversation == null)
            {
                throw new ArgumentNullException("webConversation", "Web conversation cannot be null");
            }
            TerminateConversationRequest request = new TerminateConversationRequest();
            request.RequestId = ContactCenterService.GenerateNewRequestId();
            request.Conversation = webConversation;
            return ((IContactCenterWcfService)this.WcfClient).BeginTerminateConversation(request, callback, state);
        }

        /// <summary>
        /// Completes the corresponding begin operation.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        public void EndTerminateConversation(IAsyncResult asyncResult)
        {
            ((IContactCenterWcfService)this.WcfClient).EndTerminateConversation(asyncResult);
        }

        /// <summary>
        /// Session terminated.
        /// </summary>
        /// <param name="webConversation">Web conversation.</param>
        public void SessionTerminated(WebConversation webConversation)
        {
            if (webConversation == null)
            {
                throw new ArgumentNullException("webConversation", "Web conversation cannot be null");
            }
            SessionTerminationRequest request = new SessionTerminationRequest();
            request.RequestId = ContactCenterService.GenerateNewRequestId();
            request.Conversations = new System.Collections.ObjectModel.ObservableCollection<WebConversation>();
            request.Conversations.Add(webConversation);
            ((IContactCenterWcfService)this.WcfClient).BeginSessionTerminated(request, this.SessionTerminatedCompleted, null);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Session termination completed callback.
        /// </summary>
        private void SessionTerminatedCompleted(IAsyncResult asyncResult)
        {
            try
            {
                ((IContactCenterWcfService)this.WcfClient).EndSessionTerminated(asyncResult);
            }
            catch (Exception)
            {
                //ignore exceptions during session termianted.
            }
        }

        /// <summary>
        /// Generates new request id.
        /// </summary>
        /// <returns>New request id.</returns>
        private static string GenerateNewRequestId()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Raises event handler.
        /// </summary>
        /// <param name="eventHandler">Event handler.</param>
        /// <param name="eventArgs">Event args to raise.</param>
        private void RaiseEventHandler<TEventArgs>(EventHandler<TEventArgs> eventHandler, TEventArgs eventArgs) where TEventArgs : EventArgs
        {
            Debug.Assert(null != eventArgs, "Event args is null");
            if (eventHandler != null)
            {
                eventHandler(this, eventArgs);
            }
        }

        /// <summary>
        /// Registers all event handlers.
        /// </summary>
        private void RegisterEventHandlers(ContactCenterWcfServiceClient wcfClient)
        {
            if (wcfClient != null)
            {
                wcfClient.ConversationTerminatedReceived += this.ConversationTerminatedReceived;
                wcfClient.InstantMessageReceivedReceived += this.InstantMessageReceivedReceived;
                wcfClient.RemoteComposingStatusReceived += this.RemoteComposingStatusReceived;
            }
        }

        /// <summary>
        /// Unregisters all event handlers.
        /// </summary>
        private void UnregisterEventHandlers(ContactCenterWcfServiceClient wcfClient)
        {
            if (wcfClient != null)
            {

                wcfClient.ConversationTerminatedReceived -= this.ConversationTerminatedReceived;
                wcfClient.InstantMessageReceivedReceived -= this.InstantMessageReceivedReceived;
                wcfClient.RemoteComposingStatusReceived -= this.RemoteComposingStatusReceived;
            }
        }

        #endregion

        #region private event handlers

        /// <summary>
        /// Remote composing status received.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void RemoteComposingStatusReceived(object sender, RemoteComposingStatusReceivedEventArgs e)
        {
            if (e.Error == null)
            {
                var imComposingStatusNotification = new ImComposingStatusNotification(e.remoteComposingStatusNotification.ImCall.WebConversation, e.remoteComposingStatusNotification.RemoteComposingStatus);
                var eventArgs = new ConversationNotificationReceivedEventArgs<ImComposingStatusNotification>(imComposingStatusNotification);
                this.RaiseEventHandler<ConversationNotificationReceivedEventArgs<ImComposingStatusNotification>>(this.ImComposingStatusNotificationReceived, eventArgs);
            }
        }

        /// <summary>
        /// Instant message received.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void InstantMessageReceivedReceived(object sender, InstantMessageReceivedReceivedEventArgs e)
        {
            if (e.Error == null)
            {
                var imReceivedNotification = new ImMessageReceivedNotification(e.imReceivedNotification.ImCall.WebConversation, e.imReceivedNotification.MessageReceived, e.imReceivedNotification.MessageSender);
                var eventArgs = new ConversationNotificationReceivedEventArgs<ImMessageReceivedNotification>(imReceivedNotification);
                this.RaiseEventHandler<ConversationNotificationReceivedEventArgs<ImMessageReceivedNotification>>(this.ImMessageReceived, eventArgs);
            }
        }

        /// <summary>
        /// Conversation termination received.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ConversationTerminatedReceived(object sender, ConversationTerminatedReceivedEventArgs e)
        {
            if(e.Error == null) 
            {
                var conversationTerminatedNotification = new ConversationTerminatedNotification(e.conversationTerminationNotification.Conversation);
                var eventArgs = new ConversationNotificationReceivedEventArgs<ConversationTerminatedNotification>(conversationTerminatedNotification);
                this.RaiseEventHandler<ConversationNotificationReceivedEventArgs<ConversationTerminatedNotification>>(this.ConversationTerminated, eventArgs);
            }
        }

/*        /// <summary>
        /// AudioVideo call terminated.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void AudioVideoCallTerminatedReceived(object sender, AudioVideoCallTerminatedReceivedEventArgs e)
        {
            if (e.Error == null)
            {
                Debug.Assert(null != e.audioVideoCallTerminationNotification, "Notification is null");
                ConversationModel conversationModel = this.GetConversationModelFromConversation(e.audioVideoCallTerminationNotification.AvCall.WebConversation);
                if (conversationModel != null)
                {
                    conversationModel.HandleAvCallTerminatedNotification();
                }
            }
        }

        /// <summary>
        /// Im call terminated.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void InstantMessageCallTerminatedReceived(object sender, InstantMessageCallTerminatedReceivedEventArgs e)
        {
            if (e.Error == null)
            {
                Debug.Assert(null != e.imCallTerminationNotification, "Notification is null");
                ConversationModel conversationModel = this.GetConversationModelFromConversation(e.imCallTerminationNotification.ImCall.WebConversation);
                if (conversationModel != null)
                {
                    conversationModel.HandleImCallTerminatedNotification();
                }
            }
        }*/

        #endregion
    }
}
