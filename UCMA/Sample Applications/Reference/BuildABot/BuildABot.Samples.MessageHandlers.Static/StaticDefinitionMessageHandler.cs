namespace BuildABot.Samples.MessageHandlers.Static
{
    using BuildABot.Core.MessageHandlers;
    using System.ComponentModel.Composition;

    /// <summary>
    /// A message handler that reads definitions from a static xml file.
    /// </summary>
    [Export(typeof(MessageHandler))]
    public class StaticDefinitionMessageHandler : StaticSingleStateMessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticDefinitionMessageHandler"/> class.
        /// </summary>
        public StaticDefinitionMessageHandler()
            : base("StaticSample.xml")
        {
        }
    }
}