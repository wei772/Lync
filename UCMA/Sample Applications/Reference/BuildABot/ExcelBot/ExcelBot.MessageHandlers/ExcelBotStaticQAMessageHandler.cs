namespace ExcelBot.MessageHandlers
{
    using System.ComponentModel.Composition;
    using BuildABot.Core.MessageHandlers;
    using BuildABot.Core.MessageHandlers.QAs;
    using ExcelBot.Util;

    /// <summary>
    /// Static QA message handler for the Excel bot.
    /// </summary>
    [Export(typeof(MessageHandler))]
    public class ExcelBotStaticQAMessageHandler : StaticQAMessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelBotStaticQAMessageHandler"/> class.
        /// </summary>
        public ExcelBotStaticQAMessageHandler()
            : base(InitializationHelper.BotConfigFilePath)
        {
        }
    }
}
