/*=====================================================================
  File:      WebConversation.cs
 
  Summary:   Represents a conversation object.
 
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/



using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.AsyncResults;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities;
using System.ServiceModel;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{
    /// <summary>
    /// Represents a conversation object.
    /// </summary>
    [DataContract]
    public class WebConversation
    {
        #region private members

        /// <summary>
        /// Ucma conversation
        /// </summary>
        private readonly Conversation m_conversation;

        /// <summary>
        /// Callback reference.
        /// </summary>
        private IConversationCallback m_conversationCallback;

        /// <summary>
        /// web Im call associated with this web conversation.
        /// </summary>
        private WebImCall m_imCall;

        /// <summary>
        /// web av call associated with this web conversation.
        /// </summary>
        private WebAvCall m_avCall;

        /// <summary>
        /// Back to back call associated with this web conversation.
        /// </summary>
        private BackToBackCall m_b2bCall;

        /// <summary>
        /// Conversation context dictionary.
        /// </summary>
        private readonly IDictionary<string, string> m_conversationContext;

        /// <summary>
        /// Context channel.
        /// </summary>
        private readonly IContextChannel m_channel;
        #endregion

        #region constructor

        /// <summary>
        /// Creates a new conversation with given subject and id.
        /// </summary>
        /// <param name="conversation">Ucma conversation.</param>
        /// <param name="conversationCallback">Conversation callback.</param>
        /// <param name="conversationContext">Conversation context.</param>
        /// <param name="channel">Context channel.</param>
        internal WebConversation(Conversation conversation, IConversationCallback conversationCallback, IDictionary<string, string> conversationContext, IContextChannel channel)
        {
            if (conversation == null)
            {
                throw new ArgumentException("Ucma conversation cannot be null", "conversation");
            }

            this.Id = conversation.Id;
            this.Subject = conversation.Subject;
            m_conversation = conversation;
            m_conversationCallback = conversationCallback;
            if (conversationContext != null)
            {
                m_conversationContext = new Dictionary<string, string>(conversationContext);
            }
            m_channel = channel;
            m_channel.Faulted += this.ChannelClosed;
            m_channel.Closed += this.ChannelClosed;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the id of the conversation.
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Gets the subject of the conversation.
        /// </summary>
        [DataMember]
        public string Subject { get; set; }

        /// <summary>
        /// Gets the ucma conversation associated with this web conversation.
        /// </summary>
        public Conversation Conversation
        {
            get
            {
                return m_conversation;
            }
        }

        /// <summary>
        /// Gets or sets the web instant messaging call associated with this web conversation.
        /// </summary>
        public WebImCall WebImCall
        {
            get
            {
                return m_imCall;
            }
            set
            {
                m_imCall = value;
            }
        }

        /// <summary>
        /// Gets or sets the audio video call associated with this web conversation.
        /// </summary>
        public WebAvCall WebAvCall
        {
            get
            {
                return m_avCall;
            }
            set
            {
                m_avCall = value;
            }
        }

        /// <summary>
        /// Gets or sets the b2b call associated with this web conversation.
        /// </summary>
        public BackToBackCall BackToBackCall
        {
            get
            {
                return m_b2bCall;
            }
            set
            {
                m_b2bCall = value;
            }
        }

        /// <summary>
        /// Gets or sets the conversation callback reference.
        /// </summary>
        public IConversationCallback ConversationCallback
        {
            get { return m_conversationCallback; }
            set { m_conversationCallback = value; }
        }


        /// <summary>
        /// Gets the conversation context.
        /// </summary>
        public IDictionary<string, string> ConversationContext
        {
            get { return m_conversationContext; }
        }

        /// <summary>
        /// Gets the Context channel.
        /// </summary>
        public IContextChannel ContextChannel
        {
            get { return m_channel; }
        }
        #endregion


        #region private methods

        /// <summary>
        /// Handle client channel closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChannelClosed(object sender, EventArgs e)
        {
            IContextChannel contextChannel = sender as IContextChannel;
            contextChannel.Faulted -= this.ChannelClosed;
            contextChannel.Closed -= this.ChannelClosed;

            TerminateConversationRequest request = new TerminateConversationRequest();
            request.Conversation = this;
            request.RequestId = Guid.NewGuid().ToString();
            this.BeginTerminate(request, this.ConversationTerminated, null);
        }

        /// <summary>
        /// Conversation termination callback.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ConversationTerminated(IAsyncResult asyncResult)
        {
            try
            {
                this.EndTerminate(asyncResult);
            }
            catch (Exception)
            {
            }
        }

        #endregion 

        #region public methods

        /// <summary>
        /// Begin terminate method.
        /// </summary>
        /// <param name="terminationConversationRequest">Termination request.</param>
        /// <param name="asyncCallback">callback.</param>
        /// <param name="state">State.</param>
        /// <returns>IAsync result.</returns>
        public IAsyncResult BeginTerminate(TerminateConversationRequest terminationConversationRequest, AsyncCallback asyncCallback, object state)
        {

            //Create a new async result. 
            TerminateConversationAsyncResult asyncResult = new TerminateConversationAsyncResult(terminationConversationRequest, this, asyncCallback, state);
            asyncResult.Process();
            return asyncResult;

        }

        /// <summary>
        /// End terminate method.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public TerminateConversationResponse EndTerminate(IAsyncResult asyncResult)
        {
            TerminateConversationResponse response = null;
            if (asyncResult == null)
            {
                throw new ArgumentException(FailureStrings.GenericFailures.InvalidAsyncResult, "asyncResult");
            }
            else
            {
                TerminateConversationAsyncResult terminateConversationAsyncResult = asyncResult as TerminateConversationAsyncResult;
                if (terminateConversationAsyncResult == null)
                {
                    throw new ArgumentException(FailureStrings.GenericFailures.InvalidAsyncResult, "asyncResult");
                }
                else
                {
                    response = terminateConversationAsyncResult.EndInvoke();
                }
            }

            return response;
        }
        #endregion
    }
}
