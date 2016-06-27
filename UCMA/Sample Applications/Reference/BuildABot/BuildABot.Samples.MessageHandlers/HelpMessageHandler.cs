using System;
using System.ComponentModel.Composition;
using BuildABot.Core.MessageHandlers;

namespace BuildABot.Samples.MessageHandlers
{
    /// <summary>
    /// Help message handler.
    /// </summary>
    [Export(typeof(MessageHandler))]
    public class HelpMessageHandler : MessageHandler
    {
        string ticketNumber;
        string problemDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpMessageHandler"/> class.
        /// </summary>
        public HelpMessageHandler()
            : base("help")
        {
        }

        /// <summary>
        /// Gets the initial state handler.
        /// </summary>
        /// <value>The initial state handler.</value>
        protected override StateHandler InitialStateHandler
        {
            get { return AskForTicketStatus; }
        }

        /// <summary>
        /// Asks for ticket status.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>


        public Reply AskForTicketStatus(Message message)
        {
            Reply reply = new Reply("Is this a new or an already opened ticket?");
            this.nextStateHandler = HandleTicketStatus;
            return reply;
        }

        /// <summary>
        /// Handles the ticket status.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Reply HandleTicketStatus(Message message)
        {
            Reply reply = new Reply();
            if (message.Content.Contains("new"))
            {
                this.ticketNumber = this.CreateNewTicket();
                reply.Add("This is your new ticket number: " + this.ticketNumber);
                reply.Add("What's your problem?");
                this.nextStateHandler = this.GetUserIssue;
            }
            else
            {
                reply.Add("What's the ticket number?");
                this.nextStateHandler = this.GetTicketNumber;
            }
            return reply;
        }

        /// <summary>
        /// Gets the ticket number.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Reply GetTicketNumber(Message message)
        {
            this.ticketNumber = message.Content;
            this.nextStateHandler = this.GetUserIssue;
            return new Reply("What's the update on your problem?");
        }

        /// <summary>
        /// Gets the user issue.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Reply GetUserIssue(Message message)
        {
            this.problemDescription = message.Content;
            string solution = this.GetSolution(problemDescription);
            this.nextStateHandler = this.GetSolutionFeedback;
            return new Reply("Will this work: " + solution);
        }

        /// <summary>
        /// Gets the solution feedback.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Reply GetSolutionFeedback(Message message)
        {
            Reply reply = new Reply();
            if (message.Content.Contains("yes"))
            {
                reply.Add("Great, good to know");
                this.Done = true;
            }
            else
            {
                string solution = this.GetAlternativeSolution(problemDescription);
                reply.Add("Sorry to hear that. What about this, will this work: " + solution);
                this.nextStateHandler = this.GetAlternativeSolutionFeedback;
            }
            return reply;
        }

        /// <summary>
        /// Gets the alternative solution feedback.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Reply GetAlternativeSolutionFeedback(Message message)
        {
            Reply reply = new Reply();
            if (message.Content.Contains("yes"))
            {
                reply.Add("Great, good to know");
            }
            else
            {
                reply.Add("Sorry, I'll escalate this for you.");
                // escalation code...
            }
            this.Done = true;
            return reply;
        }

        private string CreateNewTicket()
        {
            return Guid.NewGuid().ToString();
        }

        private string GetSolution(string problemDescription)
        {
            return "http://www.letmebingthatforyou.com/?q=" + problemDescription;
        }

        private string GetAlternativeSolution(string problemDescription)
        {
            return "Ask your manager about " + problemDescription;
        }
    }
}
