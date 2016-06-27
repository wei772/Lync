using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using BuildABot.Core.MessageHandlers;

namespace BuildABot.Tests.TestMessageHandlers
{
    [Export(typeof(MessageHandler))]
    class FakeTranslationMessageHandler : SingleStateMessageHandler
    {
        public FakeTranslationMessageHandler()
            : base("translate( )+(?<term>(.)*)( )+from( )+(?<from>(.)*)( )+to( )+(?<to>(.)*)", "Let me check this translation... ",true)
        {
        }

        public override Reply Handle(Message message)
        {
            Reply reply = new Reply();
            string term = this["term"];
            string fromLanguage = this["from"];
            string toLanguage = this["to"];

            reply.Add(string.Format("You want me to translate the term '{0}' from '{1}' to '{2}'. Got it, but won't!", term, fromLanguage, toLanguage));

            return reply;
        }
    }
}
