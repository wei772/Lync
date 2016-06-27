namespace BuildABot.Core
{
    using System;
    using BuildABot.Core.MessageHandlers;

    /// <summary>
    /// ReplyEventHandler delegate.
    /// </summary>
    public delegate void ReplyEventHandler(object sender, ReplyEventArgs e);

    /// <summary>
    /// MessageEventHandler delegate.
    /// </summary>
    public delegate void MessageEventHandler(object sender, MessageEventArgs e);

    /// <summary>
    /// ErrorEventHandler delegate.
    /// </summary>
    public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);

    /// <summary>
    /// ConferenceCreatedEventHandler delegate.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    public delegate void ConferenceCreatedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// The type of the reply.
    /// </summary>
    public enum ReplyContext
    {
        /// <summary>
        /// The reply is a regular reply message.
        /// </summary>
        RegularReplyMessage,

        /// <summary>
        /// The reply is a request for feedback.
        /// </summary>
        FeedbackRequest,

        /// <summary>
        /// The reply is for acknowledging a response to feedback.
        /// </summary>
        FeedbackResponse,

        /// <summary>
        /// The reply is initial text prior to handling a request.
        /// </summary>
        InitialHandlingText
    }

    /// <summary>
    /// Event arguments for the reply class.
    /// </summary>
    public class ReplyEventArgs : EventArgs
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplyEventArgs"/> class.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <param name="message">The message.</param>
        /// <param name="replyContext">The reply context.</param>
        /// <param name="conversationReplyCount">The conversation reply count.</param>
        /// <param name="messageHandler">The message handler.</param>
        public ReplyEventArgs(Reply reply, Message message, ReplyContext replyContext, int conversationReplyCount, MessageHandler messageHandler)
        {
            this.Reply = reply;
            this.Message = message;
            this.ReplyContext  = replyContext;
            this.ConversationReplyCount = conversationReplyCount;
            this.MessageHandler = messageHandler;
        }

        /// <summary>
        /// Gets or sets the reply.
        /// </summary>
        /// <value>The reply.</value>
        public Reply Reply { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public Message Message { get; set; }

        /// <summary>
        /// Gets or sets the reply context.
        /// </summary>
        /// <value>The reply context.</value>
        public ReplyContext ReplyContext { get; set; }

        /// <summary>
        /// Gets or sets the conversation reply count.
        /// </summary>
        /// <value>The conversation reply count.</value>
        public int ConversationReplyCount { get; set; }

        /// <summary>
        /// Gets or sets the message handler that handled the message. It will be null if the reply is sent
        /// not as a result of a message handler (such as when feedback is being requested or responded).
        /// </summary>
        /// <value>The message handler.</value>
        public MessageHandler MessageHandler { get; set; }
    }

    /// <summary>
    /// Event args for events handling messages.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {        
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MessageEventArgs(Message message)
        {
            this.Message = message;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public Message Message { get; set; }
    }


    /// <summary>
    /// Event args for events handling errors.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEventArgs"/> class.
        /// </summary>
        /// <param name="originator">The originator.</param>
        /// <param name="exception">The originating message (or context) that caused the error.</param>
        public ErrorEventArgs(string originator, Exception exception)
        {
            this.Originator = originator;
            this.Exception = exception;
        }

        /// <summary>
        /// Gets or sets the originating message (or context) that caused the error.
        /// </summary>
        /// <value>The originating message (or context) that caused the error.</value>
        public string Originator { get; set; }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; set; }
    }
}
