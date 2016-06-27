using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BuildABot.Core.MessageHandlers.QAs
{
    /// <summary>
    /// Message handler that manipulate simple question/answer (QA) pairs,
    /// that can be used in conversations that require no state and very little processing.
    /// </summary>
    public abstract class QAMessageHandler : SingleStateMessageHandler
    {       

        /// <summary>
        /// Initializes a new instance of the <see cref="QAMessageHandler"/> class.
        /// </summary>
        public QAMessageHandler()
        {
            this.QAs = new List<QA>();
            this.DefaultConfidence = 0.5;
        }


        /// <summary>
        /// Gets or sets the list of QA's.
        /// </summary>
        /// <value>The list of QA's..</value>
        protected List<QA> QAs { get; set; }

        /// <summary>
        /// Determines whether this instance can handle the specified message.
        /// </summary>
        /// <param name="message">The message info.</param>
        /// <returns></returns>
        public override MessageHandlingResponse CanHandle(Message message)
        {
            MessageHandlingResponse response = new MessageHandlingResponse();
            foreach (QA qa in QAs)
            {
                if (Regex.IsMatch(message.Content.ToLower(), qa.Question, RegexOptions.IgnoreCase))
                {
                    response.Confidence = this.DefaultConfidence;
                    break;
                }
            }
            return response;
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public override Reply Handle(Message message)
        {
            Reply reply = null;
            string messageContent = message.Content;
            foreach (QA qa in this.QAs)
            {
                if (Regex.IsMatch(message.Content.ToLower(), qa.Question, RegexOptions.IgnoreCase))
                {
                    reply = qa.GetAnswer(message);
                    break;
                }
            }

            return reply;
        }
    }
}
