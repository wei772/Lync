using BuildABot.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using BuildABot.Core.MessageHandlers;
using BuildABot.Tests.TestMessageHandlers;

namespace BuildABot.Tests
{
    
    
    /// <summary>
    ///This is a test class for BotTest and is intended
    ///to contain all BotTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BotTest
    {


        private TestContext testContextInstance;

        private static Bot bot;
        private static bool failedToUnderstand;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Bot.UseEmoticons = true;
            bot = new Bot();
            
            // this is just to ensure the but will always fire Reply events
            bot.Replied += new ReplyEventHandler(DefaultReplyHandle);
        }

        static void DefaultReplyHandle(object sender, ReplyEventArgs e)
        {
        }
        
        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            bot.Replied -= new ReplyEventHandler(DefaultReplyHandle);
        }
        
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for a regular bot conversation.
        ///</summary>
        [TestMethod()]
        public void RegularConversationTest()
        {
            bot.Replied += RepliedToHello;
            bot.ProcessMessage(new Message("hello"));
            bot.Replied -= RepliedToHello;

        }

        private void RepliedToHello(object sender, ReplyEventArgs e)
        {
            if (e.ReplyContext == ReplyContext.RegularReplyMessage)
            {
                bool foundHelloWorld = e.Reply.Messages.Any(message => message.Content == "Hello world!");
                Assert.IsTrue(foundHelloWorld, "Verifying whether bot replied with hello world");
            }
        }

        /// <summary>
        ///A test for a regular bot conversation.
        ///</summary>
        [TestMethod()]
        public void FailedToUnderstandTest()
        {
            failedToUnderstand = false;
            bot.FailedToUnderstand += Bot_FailedToUnderstand;
            bot.ProcessMessage(new Message("blah"));
            Assert.IsTrue(failedToUnderstand, "Verifying whether bot failed to understand message, as expected.");
            bot.FailedToUnderstand -= Bot_FailedToUnderstand;
            failedToUnderstand = false;
        }

        private void Bot_FailedToUnderstand(object sender, MessageEventArgs e)
        {
            failedToUnderstand = true;
        }

        /// <summary>
        ///A test for a regular bot conversation.
        ///</summary>
        [TestMethod()]
        public void DontGiveupOnFeedbackTest()
        {
            bot.Replied += RepliedToWeather;
            try
            {
                bot.ProcessMessage(new Message("what is weather"));
                Assert.AreEqual(3, bot.ConversationReplyCount, "Verifying the bot has the right conversation iteration index");
                bot.ProcessMessage(new Message("no"));
                Assert.IsFalse(failedToUnderstand, "Verifying the bot didn't fail to understand message, as expected.");
                Assert.AreEqual(6, bot.ConversationReplyCount, "Verifying the bot has the right conversation iteration index");
            }
            finally
            {
                bot.Replied -= RepliedToWeather;
            }
        }

        /// <summary>
        ///A test for a regular bot conversation.
        ///</summary>
        [TestMethod()]
        public void GiveupOnFeedbackTest()
        {
            bot.GiveUpOnNegativeFeedback = true;
            
            bot.ProcessMessage(new Message("what is weather"));
            Assert.AreEqual(3, bot.ConversationReplyCount, "Verifying the bot has the right conversation iteration index");
            bot.ProcessMessage(new Message("no"));
            Assert.AreEqual(3, bot.ConversationReplyCount, "Verifying the bot has the right conversation iteration index");
            
            // cleanup
            bot.GiveUpOnNegativeFeedback = false;
        }

        private void RepliedToWeather(object sender, ReplyEventArgs e)
        {
            if (e.ReplyContext == ReplyContext.RegularReplyMessage)
            {
                switch (e.ConversationReplyCount)
                {
                    case 2:
                        Assert.AreEqual(DefinitionMessageHandler.ReplyMessageContent, e.Reply[0].Content, "Verifying the bot replies with the highest-ranking message handler first");
                        break;
                    case 4:
                        Assert.AreEqual(WeatherMessageHandler.ReplyMessageContent, e.Reply[0].Content, "Verifying the bot replies with the second highest-ranking message handler later on");
                        break;
                    default:
                        break;
                }
            }
        }

        [TestMethod()]
        public void ConversationIterationTest()
        {
            bot.ProcessMessage(new Message("add"));
            Assert.AreEqual(1, bot.ConversationReplyCount, "Verifying the bot has the right conversation iteration index");
            bot.ProcessMessage(new Message("34"));
            Assert.AreEqual(2, bot.ConversationReplyCount, "Verifying the bot has the right conversation iteration index");
            bot.ProcessMessage(new Message("55"));
            Assert.AreEqual(4, bot.ConversationReplyCount, "Verifying the bot has the right conversation iteration index");
            bot.ProcessMessage(new Message("add"));
            Assert.AreEqual(1, bot.ConversationReplyCount, "Verifying the bot has the right conversation iteration index");
            bot.ProcessMessage(new Message("34"));
            Assert.AreEqual(2, bot.ConversationReplyCount, "Verifying the bot has the right conversation iteration index");
            bot.ProcessMessage(new Message("55"));
            Assert.AreEqual(4, bot.ConversationReplyCount, "Verifying the bot has the right conversation iteration index");
        }

        [TestMethod()]
        public void InputMatcherGroupTest()
        {
            bot.ProcessMessage(new Message("who is afurtado"));
            Assert.AreEqual("afurtado", AliasMessageHandler.alias, "Verifying the alias message handler got the right alias");
        }
    }
}
