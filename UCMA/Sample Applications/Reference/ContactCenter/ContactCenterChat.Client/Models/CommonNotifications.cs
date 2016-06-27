/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.Models
{

    #region Conversation Notification
    /// <summary>
    /// Represents base class for all conversation related notifications.
    /// </summary>
    public class ConversationNotification
    {
        #region private variables
        /// <summary>
        /// conversation id.
        /// </summary>
        private readonly WebConversation m_webConversation;
        #endregion

        #region constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="webConversation">web conversation. Cannot be null or empty.</param>
        internal ConversationNotification(WebConversation webConversation)
        {
            Debug.Assert(webConversation != null, "web Conversation cannot be null or empty");
            m_webConversation = webConversation;
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets the web conversation.
        /// </summary>
        public WebConversation WebConversation
        {
            get
            {
                return m_webConversation;
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents ConversationNotificationReceivedEventArgs
    /// </summary>
    /// <typeparam name="TConversationNotification">Conversation notification type.</typeparam>
    public class ConversationNotificationReceivedEventArgs<TConversationNotification> : EventArgs where TConversationNotification : ConversationNotification
    {
        #region private variables

        /// <summary>
        /// Actual notification.
        /// </summary>
        private TConversationNotification m_conversationNotification;
        #endregion

        #region internal constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="conversationNotification">Conversation notification.</param>
        internal ConversationNotificationReceivedEventArgs(TConversationNotification conversationNotification)
        {
            Debug.Assert(null != conversationNotification, "Conversation notification is null");
            m_conversationNotification = conversationNotification;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the actual notification.
        /// </summary>
        public TConversationNotification Notification
        {
            get { return m_conversationNotification; }
        }
        #endregion
    }
    #endregion

    #region event args

    #region StateChangedEventArgs

    /// <summary>
    /// Generic state changed event args.
    /// </summary>
    /// <typeparam name="TState">Type of state.</typeparam>
    public class StateChangedEventArgs<TState> : EventArgs where TState : struct
    {
        #region private variables

        /// <summary>
        /// Previous state.
        /// </summary>
        private readonly TState m_prevState;

        /// <summary>
        /// Current state.
        /// </summary>
        private readonly TState m_currentState;
        #endregion

        #region constructor
        /// <summary>
        /// State changed event args.
        /// </summary>
        /// <param name="prevState">Prev state.</param>
        /// <param name="currentState">Current state.</param>
        internal StateChangedEventArgs(TState prevState, TState currentState)
        {
            m_prevState = prevState;
            m_currentState = currentState;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the previous state.
        /// </summary>
        public TState PreviousState
        {
            get { return m_prevState; }
        }

        /// <summary>
        /// Gets the current state.
        /// </summary>
        public TState CurrentState
        {
            get { return m_currentState; }
        }
        #endregion


    }
    #endregion

    #region InstantMessageTypingNotificationReceivedEventArgs

    /// <summary>
    /// InstantMessageTypingNotificationReceivedEventArgs
    /// </summary>
    public class InstantMessageTypingNotificationReceivedEventArgs : EventArgs
    {
        #region constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageSender"></param>
        internal InstantMessageTypingNotificationReceivedEventArgs(RemoteComposingStatus remoteComposingStatus)
            : base()
        {
            this.RemoteComposingStatus = remoteComposingStatus;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the message.
        /// </summary>
        public RemoteComposingStatus RemoteComposingStatus { get; private set; }
        #endregion
    }
    #endregion

    #region InstantMessageReceivedEventArgs

    /// <summary>
    /// InstantMessageReceivedEventArgs
    /// </summary>
    public class InstantMessageReceivedEventArgs : EventArgs
    {
        #region constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageSender"></param>
        internal InstantMessageReceivedEventArgs(string message, string messageSender)
            : base()
        {
            this.Message = message;
            this.MessageSender = messageSender;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the message sender.
        /// </summary>
        public string MessageSender { get; private set; }
        #endregion
    }
    #endregion

    #region ConversationStateChangedEventArgs

    /// <summary>
    /// Conversation state changed event args.
    /// </summary>
    public class ConversationStateChangedEventArgs : StateChangedEventArgs<ConversationModelState>
    {
        #region constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prevState"></param>
        /// <param name="currentState"></param>
        internal ConversationStateChangedEventArgs(ConversationModelState prevState, ConversationModelState currentState)
            : base(prevState, currentState)
        {
        }
        #endregion
    }
    #endregion

    #region ConversationTerminatedNotification

    /// <summary>
    /// Represents conversation terminated notification.
    /// </summary>
    public class ConversationTerminatedNotification : ConversationNotification
    {

        #region private variables
        #endregion

        #region constructor

        /// <summary>
        /// Internal constructor to create conversation terminated notification.
        /// </summary>
        /// <param name="webConversation">Conversation id.</param>
        internal ConversationTerminatedNotification(WebConversation webConversation)
            : base(webConversation)
        {
        }
        #endregion
    }
    #endregion

    #region ImCallTerminatedNotification

    /// <summary>
    /// Represents im call terminated notification.
    /// </summary>
    public class ImCallTerminatedNotification : ConversationNotification
    {

        #region private variables
        #endregion

        #region constructor

        /// <summary>
        /// Internal constructor to create im call terminated event args.
        /// </summary>
        /// <param name="webConversation">Web conversation.</param>
        internal ImCallTerminatedNotification(WebConversation webConversation)
            : base(webConversation)
        {
        }
        #endregion
    }
    #endregion

    #region AvCallTerminatedNotification

    /// <summary>
    /// Represents av call terminated notification.
    /// </summary>
    public class AvCallTerminatedNotification : ConversationNotification
    {

        #region private variables

        #endregion

        #region constructor

        /// <summary>
        /// Internal constructor to create av call terminated notification.
        /// </summary>
        /// <param name="webConversation">Web conversation.</param>
        internal AvCallTerminatedNotification(WebConversation webConversation)
            : base(webConversation)
        {
        }
        #endregion

        #region public properties
        #endregion
    }
    #endregion

    #region ImMessageReceivedNotification

    /// <summary>
    /// Represents im message received notification.
    /// </summary>
    public class ImMessageReceivedNotification : ConversationNotification
    {

        #region private variables

        /// <summary>
        /// Actual message.
        /// </summary>
        private readonly string m_message;

        /// <summary>
        /// Message sender.
        /// </summary>
        private readonly string m_messageSender;
        #endregion

        #region constructor

        /// <summary>
        /// Internal constructor to create im message received event args.
        /// </summary>
        /// <param name="webConversation">Web conversation.</param>
        /// <param name="message">Message</param>
        /// <param name="messageSender">Message sender.</param>
        internal ImMessageReceivedNotification(WebConversation webConversation, string message, string messageSender)
            : base(webConversation)
        {
            m_message = message ?? String.Empty;
            m_messageSender = messageSender ?? String.Empty;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message
        {
            get { return m_message; }
        }

        /// <summary>
        /// Gets the message sender.
        /// </summary>
        public string MessageSender
        {
            get { return m_messageSender; }
        }
        #endregion
    }
    #endregion

    #region ImComposingStatusNotification

    /// <summary>
    /// Represents im message composing status notification.
    /// </summary>
    public class ImComposingStatusNotification : ConversationNotification
    {

        #region private variables

        /// <summary>
        /// Actual message.
        /// </summary>
        private readonly RemoteComposingStatus m_composingStatus;
        #endregion

        #region constructor

        /// <summary>
        /// Internal constructor to create im composing status notification.
        /// </summary>
        /// <param name="webConversation">Web conversation.</param>
        /// <param name="remoteComposingStatus">Remote composing status.</param>
        internal ImComposingStatusNotification(WebConversation webConversation, RemoteComposingStatus remoteComposingStatus)
            : base(webConversation)
        {
            m_composingStatus = remoteComposingStatus;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the remote composing status
        /// </summary>
        public RemoteComposingStatus RemoteComposingStatus
        {
            get { return m_composingStatus; }
        }
        #endregion
    }
    #endregion
    #endregion

    #region results

    #region ConversationResult

    /// <summary>
    /// Represents conversation result.
    /// </summary>
    internal class ConversationResult
    {
        #region private variables

        /// <summary>
        /// Web conversation.
        /// </summary>
        private readonly WebConversation m_webConversation;
        #endregion

        #region constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="webConversation">Web conversation. Cannot be null.</param>
        internal ConversationResult(WebConversation webConversation)
        {
            Debug.Assert(null != webConversation, "Web conversation is null");
            m_webConversation = webConversation;
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets the web conversation associated with this result.
        /// </summary>
        internal WebConversation WebConversation
        {
            get { return m_webConversation; }
        }

        #endregion
    }
    #endregion

    #region EstablishConversationResult

    /// <summary>
    /// Represents establish conversation result.
    /// </summary>
    internal class EstablishConversationResult : ConversationResult
    {
        #region constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="webConversation">Web conversation. Cannot be null.</param>
        internal EstablishConversationResult(WebConversation webConversation)
            : base(webConversation)
        {
        }

        #endregion
    }

    #endregion

    #region EstablishConversationAndImCallResult

    /// <summary>
    /// Represents establish conversation and im call result.
    /// </summary>
    internal class EstablishConversationAndImCallResult : ConversationResult
    {
        #region constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="webConversation">Web conversation. Cannot be null.</param>
        internal EstablishConversationAndImCallResult(WebConversation webConversation)
            : base(webConversation)
        {
        }

        #endregion
    }

    #endregion

    #region EstablishAvCallResult

    /// <summary>
    /// Represents establish click to av call result.
    /// </summary>
    internal class AddClickToCallResult : ConversationResult
    {
        #region constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="webConversation">Web conversation. Cannot be null.</param>
        internal AddClickToCallResult(WebConversation webConversation)
            : base(webConversation)
        {
        }

        #endregion
    }

    #endregion

    #region SendImMessageResult

    /// <summary>
    /// Represents send im message.
    /// </summary>
    internal class SendImMessageResult
    {
        #region constructor
        #endregion
    }

    #endregion

    #region TerminateConversationResult

    /// <summary>
    /// Represents terminate conversation result.
    /// </summary>
    internal class TerminateConversationResult
    {
        #region constructor
        #endregion
    }

    #endregion

    #endregion
}
