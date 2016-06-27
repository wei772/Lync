using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildABot.Core.MessageHandlers.QAs
{
    /// <summary>
    /// A QA entity that represents a question and a set of random answers.
    /// </summary>
    public class RandomQA : QA
    {
        private Random random = new Random();
       

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomQA"/> class.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <param name="answers">The answers.</param>
        public RandomQA(string question, params string[] answers)
            : base(question)
        {
            this.Answers = answers;
        }


        /// <summary>
        /// Gets or sets the answers.
        /// </summary>
        /// <value>The answers.</value>
        public string[] Answers { get; set; }

        /// <summary>
        /// Gets the answer. The answer is randomly selected from this RandomQA's set of answers.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        internal override Reply GetAnswer(Message message)
        {
            return new Reply(Answers[random.Next(Answers.Length)]);
        }
    }
}
