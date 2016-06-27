using System.ComponentModel.Composition;
using BuildABot.Core.MessageHandlers;
using System;

namespace BuildABot.Samples.MessageHandlers
{
    /// <summary>
    /// Sample message handler for handling time-related questions.
    /// </summary>
    [Export(typeof(MessageHandler))]
    public class TimeMessageHandler : SingleStateMessageHandler
    {
        // average response time, in seconds
        static int averageResponseTime = 60;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeMessageHandler"/> class.
        /// </summary>
        public TimeMessageHandler()
            : base("time", "Please wait while I get the current time", true)
        {
            
        }

        /// <summary>
        /// Gets the initial handling text. By this time, you can already call the inputMatcher index to request regex group values.
        /// </summary>
        /// <returns></returns>
        protected override string GetInitialHandlingText()
        {
            int numberOfRequests = this.GetNumberOfRequests();
            System.DateTime expectedReplyTime = DateTime.Now.AddSeconds(numberOfRequests * averageResponseTime);

            string initialHandlingText = String.Format(
                "I'm handling {0} requests right now so I expect you will get this reponse at {1}...",
                numberOfRequests, expectedReplyTime);

            return initialHandlingText;
        }

        /// <summary>
        /// Determines whether this instance can handle the specified message.
        /// </summary>
        /// <param name="message">The message info.</param>
        /// <returns></returns>
        public override MessageHandlingResponse CanHandle(Message message)
        {
            MessageHandlingResponse response = new MessageHandlingResponse();
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday
                || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                response.Confidence = 0;
            }
            else
            {
                // we could have implemented more complex/specific
                // logic for handling the message here.
                response = base.CanHandle(message);
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
            Reply reply = new Reply();
            reply.Add("The current time is: " + DateTime.Now.ToShortTimeString());
            reply.Add("The current date is: " + DateTime.Now.ToShortDateString());
            return reply;
        }

        private int GetNumberOfRequests()
        {
            // TODO: implement real logic
            return 3;
        }
    }
}
