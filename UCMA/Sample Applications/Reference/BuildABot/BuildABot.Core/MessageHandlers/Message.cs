using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildABot.Core.MessageHandlers
{
    /// <summary>
    /// Message meta-information.
    /// </summary>
    public class Message
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        public Message(string content)
        {
            this.Sender = new Sender();
            this.Content = content;
            this.TimeStamp = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="senderDisplayName">Display name of the sender.</param>
        /// <param name="senderAlias">The sender alias.</param>
        /// <param name="timeStamp">The timestamp.</param>
        /// <param name="conversationId">The conversation id.</param>
        /// <param name="conferenceUri">The conference URI.</param>
        public Message(string content, string senderDisplayName, string senderAlias, DateTime timeStamp, string conversationId, string conferenceUri)
            : this(content)
        {
            this.Sender = new Sender(senderDisplayName, senderAlias, SenderKind.Unknown);
            this.TimeStamp = timeStamp;
            this.ConversationId = conversationId;
            this.ConferenceUri = conferenceUri;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="sender">The sender.</param>
        public Message(string content, Sender sender)
            : this(content)
        {
            this.Sender = sender;
        }


        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the sender.
        /// </summary>
        /// <value>
        /// The sender.
        /// </value>
        public Sender Sender { get; set; }

        /// <summary>
        /// Gets or sets the display name of the sender.
        /// </summary>
        /// <value>The display name of the sender.</value>
        public string SenderDisplayName
        {
            get
            {
                string result = null;
                if (this.Sender != null)
                {
                    result = this.Sender.DisplayName;
                }

                return result;
            }
            set
            {
                if (this.Sender != null)
                {
                    this.Sender.DisplayName = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the sender alias.
        /// </summary>
        /// <value>The sender alias.</value>
        public string SenderAlias
        {
            get
            {
                string result = null;
                if (this.Sender != null)
                {
                    result = this.Sender.Alias;
                }

                return result;
            }
            set
            {
                if (this.Sender != null)
                {
                    this.Sender.Alias = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the sender timestamp.
        /// </summary>
        /// <value>When the message was sent.</value>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the conversation id.
        /// </summary>
        /// <value>
        /// The conversation id.
        /// </value>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the conference URI.
        /// </summary>
        /// <value>
        /// The conference URI.
        /// </value>
        public string ConferenceUri { get; set; }
       

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Content;
        }
    }
}
