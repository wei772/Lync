using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using BuildABot.Core.MessageHandlers;

namespace BuildABot.Tests.TestMessageHandlers
{
    /// <summary>
    /// Illustrates how to use state machine message handlers, interacting with the user and
    /// collecting input from it once at a time. This sample contains some redundancy in the
    /// states to handle the first and second numbers.
    /// </summary>
    [Export(typeof(MessageHandler))]
    public class CalculatorMessageHandler : MessageHandler
    {
        int totalSum = 0;

        public CalculatorMessageHandler(): base("add",string.Empty,true)
        {
        }

        protected override StateHandler InitialStateHandler
        {
            get { return ShowInstructions; }
        }

        internal Reply ShowInstructions(Message message)
        {
            this.nextStateHandler = HandleFirstNumber;
            return new Reply("Enter first number");
        }

        internal Reply HandleFirstNumber(Message message)
        {
            Reply reply = new Reply();
            int firstNumber;
            if (int.TryParse(message.Content, out firstNumber))
            {
                totalSum += firstNumber;
                reply.Add("Enter second number");
                this.nextStateHandler = HandleSecondNumber;
            }
            else
            {
                reply.Add("Error: number not valid. Aborting.");
                this.Done = true;
            }
            return reply;
        }
        
        internal Reply HandleSecondNumber(Message message)
        {
            Reply reply = new Reply();
            int secondNumber;
            if (int.TryParse(message.Content, out secondNumber))
            {
                totalSum += secondNumber;
                reply.Add("The total is: " + totalSum);
            }
            else
            {
                reply.Add("Error: number not valid. Aborting.");
            }
            this.Done = true;
            return reply;
        }
    }
}
