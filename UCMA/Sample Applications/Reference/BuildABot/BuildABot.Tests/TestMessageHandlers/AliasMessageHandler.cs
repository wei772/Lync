using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuildABot.Core.MessageHandlers;
using BuildABot.Util;
using System.ComponentModel.Composition;

namespace BuildABot.Tests.TestMessageHandlers
{
    [Export(typeof(MessageHandler))]
    class AliasMessageHandler : SingleStateMessageHandler
    {
        internal static string alias;
        internal static string ReplyMessageContent = "Got it, but I can't tell who this person is because I'm a just test message handler";

        internal AliasMessageHandler()
            : base("(who is|whois) (?<alias>(.)*)", "Let me check who this person is... " + Emoticons.Thinking, true)
        {
        }

        public override Reply Handle(Message message)
        {
            alias = this["alias"];
            return new Reply(ReplyMessageContent);
        }
    }
}
