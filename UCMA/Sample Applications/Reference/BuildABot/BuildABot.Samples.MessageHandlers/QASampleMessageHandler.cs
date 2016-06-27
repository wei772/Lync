using System.ComponentModel.Composition;
using BuildABot.Core.MessageHandlers;
using BuildABot.Core.MessageHandlers.QAs;
using System;

namespace BuildABot.Samples.MessageHandlers
{
    /// <summary>
    /// Sample QA message handler.
    /// </summary>
    [Export(typeof(MessageHandler))]
    public class QASampleMessageHandler : QAMessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QASampleMessageHandler"/> class.
        /// </summary>
        public QASampleMessageHandler()
        {
            // build the "qa" objects
            RandomQA qa1 = new RandomQA("Who are you", "Just a demo bot!");
            ActionQA qa3 = new ActionQA("Excuse me", this.GetSalute);

            this.QAs.Add(qa1);
            this.QAs.Add(qa3);

            string[] byeMessages = { "bye", "good bye", "goodbye", "see you", };
            string[] byeReplies = { "Bye bye!", "Take care, see ya!", "Have a good one!" };
            this.QAs.AddBatchRandomQAs(byeMessages, byeReplies);

            this.QAs.AddBatchActionQAs(GetSalute, "good morning", "good night", "good evening", "good afternoon", "good day");

        }

        private Reply GetSalute(Message message)
        {
            string salute = string.Empty;
            int hour = DateTime.Now.Hour;
            if (hour >= 0 && hour < 12)
            {
                salute = "Good morning!";
            }
            else if (hour >= 12 && hour < 18)
            {
                salute = "Good afternoon!";
            }
            else
            {
                salute = "Good evening!";
            }
            return new Reply(salute);
        }
    }
}
