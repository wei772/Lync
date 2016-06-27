using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuildABot.Core.MessageHandlers;
using System.ComponentModel.Composition;

namespace BuildABot.Tests.TestMessageHandlers
{
    [Export(typeof(MessageHandler))]
    class DefinitionMessageHandler : SingleStateMessageHandler
    {
        public static string ReplyMessageContent = "Got it, but I can't define anything because I'm a just test message handler";
        public DefinitionMessageHandler()
            : base("what is (?<from>(.)*)", "Let me get a definition for this...", true)
        {

        }

        public override Reply Handle(Message message)
        {
            string term = this["term"];
            return new Reply(ReplyMessageContent);
        }
    }
}
