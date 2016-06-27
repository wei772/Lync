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
    public class HelloWorldMessageHandler : SingleStateMessageHandler
    {
        public HelloWorldMessageHandler() : base ("hello", "Hi, let me think... ", true)
        {
        }

        public override Reply Handle(Message message)
        {
            Reply reply = new Reply();
            reply.Add("Hello world!");

            string boldWorld = "world".EncloseRtfBold();
            reply.AddRtfMessage("This is a beautiful rich text " + boldWorld);
            return reply;
        }
    }
}
