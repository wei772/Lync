namespace BuildABot.Samples.MessageHandlers.Static
{
    using System.ComponentModel.Composition;
    using BuildABot.Core.MessageHandlers;
    using BuildABot.Core.MessageHandlers.QAs;

    /// <summary>
    /// Message handler for handling greetings statically (defined in a xml file).
    /// </summary>
    [Export(typeof(MessageHandler))]
    public class StaticGreetingMessageHandler : StaticQAMessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticGreetingMessageHandler"/> class, reading greeting
        /// questions and answers from a Greetings.xml file.
        /// </summary>
        public StaticGreetingMessageHandler()
            :base("StaticSample.xml")
        {
        }
    }
}
