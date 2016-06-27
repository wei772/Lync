namespace ExcelBot.MessageHandlers
{
    using System.ComponentModel.Composition;
    using BuildABot.Core.MessageHandlers;
    using BuildABot.Core.MessageHandlers.QAs;
    using ExcelBot.Util;

    /// <summary>
    /// Static single state message handler for the Excel bot.
    /// </summary>
    [Export(typeof(MessageHandler))]
    public class ExcelBotStaticSingleStateMessageHandler : StaticSingleStateMessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelBotStaticSingleStateMessageHandler"/> class.
        /// </summary>
        public ExcelBotStaticSingleStateMessageHandler()
            : base(InitializationHelper.BotConfigFilePath)
        {
        }
    }
}
