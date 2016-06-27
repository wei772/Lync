using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuildABot.Core.MessageHandlers;
using System.ComponentModel.Composition;

namespace BuildABot.Samples.MessageHandlers
{
    /// <summary>
    /// Sample message handler representing home temperature messages.
    /// </summary>
    [Export(typeof(MessageHandler))]
    public class HomeAutomationTemperatureMessageHandler : MessageHandler
    {
        /// <summary>
        /// Gets the initial state handler.
        /// </summary>
        /// <value>
        /// The initial state handler.
        /// </value>
        protected override StateHandler InitialStateHandler
        {
            get { return this.InitialState; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeAutomationTemperatureMessageHandler"/> class.
        /// </summary>
        public HomeAutomationTemperatureMessageHandler()
            : base("abnormal temperature")
        {
        }

        /// <summary>
        /// Initials the state.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Reply InitialState(Message message)
        {
            this.nextStateHandler = CollectUserResponse;
            return null;
        }

        /// <summary>
        /// Collects the user response.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Reply CollectUserResponse(Message message)
        {
            Reply reply = new Reply();
            if (message.Content.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                reply.Messages.Add("OK, adjusting temperature!");
                this.Done = true;
            }
            else if (message.Content.Equals("no", StringComparison.InvariantCultureIgnoreCase))
            {
                reply.Messages.Add("Alright, keeping temperature.");
                this.Done = true;
            }
            else
            {
                reply.Messages.Add("Sorry I didn't get you. Do you want to adjust the temperature? (yes/no)");
            }

            return reply;
        }
    }
}
