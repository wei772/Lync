namespace BuildABot.Samples.CommandPrompt
{
    using System;
    using System.ComponentModel.Composition;
    using BuildABot.Core;
    using BuildABot.Core.MessageHandlers;
    using BuildABot.Core.Feedback;

    /// <summary>
    /// Launches a command prompt bot.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot();

            // Changing bot's default feedback request question
            bot.GiveUpOnNegativeFeedback = true;
            bot.Replied += new ReplyEventHandler(bot_Replied);
            bot.FailedToUnderstand += new MessageEventHandler(bot_FailedToUnderstand);
            
            bot.FeedbackEngine.FeedbackCollected += new FeedbackEventHandler(FeedbackEngine_FeedbackCollected);
            bot.FeedbackEngine.FeedbackRequest = new Reply("how did I do?");

            // Changing bot's default feedback answer. Notice those are regular
            // expressions patterns that are matched against user input.
            bot.FeedbackEngine.PositiveFeedbackPattern = "well";
            bot.FeedbackEngine.NegativeFeedbackPattern = "bad";

            string userMessage = Console.ReadLine();
            while (userMessage != "exit")
            {
                bot.ProcessMessage(userMessage);
                userMessage = Console.ReadLine();
            }
        }

        static Reply FeedbackEngine_FeedbackCollected(object sender, FeedbackCollectedEventArgs e)
        {
            Reply reply = new Reply();
            switch (e.FeedbackType)
            {
                case FeedbackType.Positive:
                    reply.Add("Great, good to know!");
                    // store positive feedback
                    break;

                case FeedbackType.Negative:
                    reply.Add("Sorry for that...");
                    // store negative feedback
                    break;

                case FeedbackType.NotProvided:
                    // probably don't do anything in this case.
                    break;
            }

            return reply;
        }

        /// <summary>
        /// Handles the FailedToUnderstand event of the bot control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="BuildABot.Core.MessageEventArgs"/> instance containing the event data.</param>
        static void bot_FailedToUnderstand(object sender, MessageEventArgs e)
        {
            Console.WriteLine("Bot says: sorry but I didn't get you");
        }

        /// <summary>
        /// Handles the Replied event of the bot control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="BuildABot.Core.ReplyEventArgs"/> instance containing the event data.</param>
        static void bot_Replied(object sender, ReplyEventArgs e)
        {
            foreach (ReplyMessage replyMessage in e.Reply.Messages)
            {
                Console.WriteLine("Bot says: " + replyMessage.Content);
            }
        }
    }
}
