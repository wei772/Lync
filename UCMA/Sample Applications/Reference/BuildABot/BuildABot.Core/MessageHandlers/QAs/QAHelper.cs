using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildABot.Core.MessageHandlers.QAs
{
    /// <summary>
    /// Helper methods for QA's.
    /// </summary>
    public static class QAHelper
    {
        /// <summary>
        /// Adds a batch of RandomQA's to a list of QA's.
        /// </summary>
        /// <param name="qas">The QA's.</param>
        /// <param name="questions">The questions.</param>
        /// <param name="answers">The answers.</param>
        public static void AddBatchRandomQAs(this List<QA> qas, string[] questions, params string[] answers)
        {
            foreach (string question in questions)
            {
                qas.Add(new RandomQA(question, answers));
            }
        }

        /// <summary>
        /// Adds a batch of ActionQA's to a list of QA's.
        /// </summary>
        /// <param name="qas">The QA's.</param>
        /// <param name="actionQAHandler">The action QA handler.</param>
        /// <param name="question">The question.</param>
        public static void AddBatchActionQAs(this List<QA> qas, ActionQAHandler actionQAHandler, params string[] question)
        {
            foreach (string message in question)
            {
                qas.Add(new ActionQA(message, actionQAHandler));
            }
        }
    }
}
