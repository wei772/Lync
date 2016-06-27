using System.Collections.Generic;
using System.Net.Mime;

namespace BuildABot.Core.MessageHandlers
{
    /// <summary>
    /// Reply message.
    /// </summary>
    public class ReplyMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplyMessage"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        public ReplyMessage(string content)
            : this(content, PlainTextContent)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplyMessage"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentType">Type of the content.</param>
        public ReplyMessage(string content, ContentType contentType)
        {
            this.Content = content;
            this.ContentType = contentType;
        }


        /// <summary>
        /// Plain text message content type.
        /// </summary>
        public static readonly ContentType PlainTextContent = new ContentType("text/plain");

        /// <summary>
        /// RTF message content type.
        /// </summary>
        public static readonly ContentType RtfTextContent = new ContentType("text/rtf");

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        /// <value>The content.</value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the type of the message content.
        /// </summary>
        /// <value>The type of the content.</value>
        public ContentType ContentType { get; set; }


    }

    /// <summary>
    /// A collection of reply messages.
    /// </summary>
    public class ReplyMessageCollection : List<ReplyMessage>
    {
        /// <summary>
        /// Adds the specified content to this collection of reply messages.
        /// </summary>
        /// <param name="content">The content.</param>
        public void Add(string content)
        {
            this.Add(new ReplyMessage(content));
        }

        /// <summary>
        /// Adds the specified content to this collection of reply messages.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentType">Type of the content.</param>
        public void Add(string content, ContentType contentType)
        {
            this.Add(new ReplyMessage(content, contentType));
        }
    }
}
