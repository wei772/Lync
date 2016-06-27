using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using BuildABot.Core.MessageHandlers;
using BuildABot.Util;

namespace BuildABot.Tests.TestMessageHandlers
{
    [Export(typeof(MessageHandler))]
    class HelpMessageHandler : SingleStateMessageHandler
    {
        public HelpMessageHandler() : base("help")
        {
        }

        public override Reply Handle(Message message)
        {
            Reply reply = new Reply();
            reply.Add("Those are some of the things I can fully understand, " + message.SenderDisplayName.GetFirstName() + ":");
            reply.Add(Emoticons.QuestionMark + " hello");
            reply.Add(Emoticons.QuestionMark + " add");
            reply.Add(Emoticons.QuestionMark + " translate <term> from <lang1> to <lang2>");
            return reply;
        }
    }
}
