using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildABot.Core.MessageHandlers.QAs
{
    /// <summary>
    /// Delegate for QA's, that defines an action for creating an answer from a question.
    /// </summary>
    public delegate Reply ActionQAHandler(Message messageInfo);

    /// <summary>
    /// QA that performs an action to get an answer from a question.
    /// </summary>
    public class ActionQA : QA
    {
        private ActionQAHandler actionQAHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionQA"/> class.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <param name="actionQAHandler">The action QA handler.</param>
        public ActionQA(string question, ActionQAHandler actionQAHandler)
            : base(question)
        {
            this.actionQAHandler = actionQAHandler;
        }

        /// <summary>
        /// Gets the answer.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        internal override Reply GetAnswer(Message message)
        {
            return this.actionQAHandler(message);
        }
    }
}
