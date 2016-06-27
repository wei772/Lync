namespace BuildABot.Core.MessageHandlers
{
    using System;
    using System.Text;
    using BuildABot.Util;

    /// <summary>
    /// Reply to a user, composed by a set of reply messages.
    /// </summary>
    public class Reply
    {
        private bool logReply = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="Reply"/> class.
        /// </summary>
        public Reply()
            : this(new ReplyMessageCollection())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reply"/> class.
        /// </summary>
        /// <param name="logReply">if set to <c>true</c> if need to log reply.</param>
        public Reply(bool logReply)
            : this(new ReplyMessageCollection())
        {
            this.logReply = logReply;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reply"/> class containing only one message.
        /// </summary>
        /// <param name="messageContent">Content of the message.</param>
        public Reply(string messageContent)
            : this(messageContent, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reply"/> class.
        /// </summary>
        /// <param name="messageContent">Content of the message.</param>
        /// <param name="logReply">if set to <c>true</c> [log reply].</param>
        public Reply(string messageContent, bool logReply)
        {
            this.Messages = new ReplyMessageCollection();
            this.Messages.Add(messageContent);
            this.logReply = logReply;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reply"/> class.
        /// </summary>
        /// <param name="replyMessages">The reply messages.</param>
        public Reply(ReplyMessageCollection replyMessages)
        {
            this.Messages = replyMessages;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to log reply.
        /// </summary>
        /// <value>
        ///   <c>true</c> if need to log reply; otherwise, <c>false</c>.
        /// </value>
        public bool LogReply
        {
            get
            {
                return logReply;
            }
            set
            {
                logReply = value;
            }
        }
        /// <summary>
        /// Gets or sets the messages of this reply.
        /// </summary>
        /// <value>The messages.</value>
        public ReplyMessageCollection Messages { get; set; }

        /// <summary>
        /// Gets the <see cref="BuildABot.Core.MessageHandlers.ReplyMessage"/> with the specified message index.
        /// </summary>
        /// <value></value>
        public ReplyMessage this[int messageIndex]
        {
            get
            {
                return this.Messages[messageIndex];
            }
        }

    

        /// <summary>
        /// Adds the specified reply message.
        /// </summary>
        /// <param name="replyMessage">The reply message.</param>
        public void Add(ReplyMessage replyMessage)
        {
            this.Messages.Add(replyMessage);
        }

        /// <summary>
        /// Adds a plain text message to this reply.
        /// </summary>
        /// <param name="content">The message content.</param>
        public void Add(string content)
        {
            this.Messages.Add(content);
        }

        /// <summary>
        /// Adds an RTF message to this reply.
        /// </summary>
        /// <param name="content">The content.</param>
        public void AddRtfMessage(string content)
        {
            this.Messages.Add(new ReplyMessage(content.EncloseRtf(), new System.Net.Mime.ContentType("text/rtf")));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            this.Messages.ForEach(message => stringBuilder.Append(message.Content + Environment.NewLine));
            return stringBuilder.ToString() ;
        }

    }
}
