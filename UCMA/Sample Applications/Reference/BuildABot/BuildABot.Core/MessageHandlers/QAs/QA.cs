using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BuildABot.Core.MessageHandlers.QAs
{
    /// <summary>
    /// A question/answer (QA) pair, that can be used in conversations that require no state and very little processing.
    /// </summary>
    public abstract class QA
    {        

        /// <summary>
        /// Initializes a new instance of the <see cref="QA"/> class, replacing special regular expression characters in the
        /// provided question by their literals.
        /// </summary>
        /// <param name="question">The question.</param>
        internal QA(string question)
        {
            this.Question = @"\b" + Regex.Escape(question) + @"\b";
        }


        /// <summary>
        /// Gets or sets the question.
        /// </summary>
        /// <value>The question.</value>
        public string Question { get; set; }
        
        /// <summary>
        /// Gets the answer.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        internal abstract Reply GetAnswer(Message message);
    }
}
