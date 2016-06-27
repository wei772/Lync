using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuildABot.Core.MessageHandlers;
using System.ComponentModel.Composition;

namespace BuildABot.Tests.TestMessageHandlers
{
    [Export(typeof(MessageHandler))]
    class WeatherMessageHandler : SingleStateMessageHandler
    {
        internal static string ReplyMessageContent = "Got it, but I can't tell the weather because I'm a just test message handler";

        internal WeatherMessageHandler()
            : base(".*weather.*", "Let me check the weather...", true)
        {
            this.DefaultConfidence = 0.5f;
        }

        public override Reply Handle(Message message)
        {
            return new Reply(ReplyMessageContent);
        }
    }
}
