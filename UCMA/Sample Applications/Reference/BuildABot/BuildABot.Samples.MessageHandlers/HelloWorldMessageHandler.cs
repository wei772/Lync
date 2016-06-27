using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using BuildABot.Core.MessageHandlers;

namespace BuildABot.Samples.MessageHandlers
{
    [Export(typeof(MessageHandler))]
    class HelloWorldMessageHandler : SingleStateMessageHandler
    {
        public HelloWorldMessageHandler()
            : base(@"hello|(how are you)")
        {
        }

        public override Reply Handle(BuildABot.Core.MessageHandlers.Message message)
        {
            Reply reply = new Reply();

            if (message.Content.Equals("hello", StringComparison.InvariantCultureIgnoreCase))
            {
                reply.Add("hi there!");
            }
            else
            {
                // "how are you" case
                reply.Add("I'm great, thanks!");
            }

            return reply;
        }
    }
}
