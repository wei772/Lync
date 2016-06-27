namespace ExcelBot.Util
{
    using System.IO;
    using System.Reflection;
    using System.Security;
    using BuildABot.UC;
    using BuildABot.Util;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Helper methods for initializing the Excel bot.
    /// </summary>
    public class InitializationHelper
    {
        /// <summary>
        /// Output path for the bot configuration file that gets generated after the Excel spreadsheet is loaded.
        /// </summary>
        public static string BotConfigFilePath { get; set; }

        /// <summary>
        /// Initializes the excel bot.
        /// </summary>
        public static void Initialize()
        {
            Debug.Listeners.Add(new ConsoleTraceListener());

            string currentDirectory = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().Location));
            string inputPath = Path.Combine(currentDirectory, "BotConfig.xlsx");
            InitializationHelper.BotConfigFilePath = Path.Combine(currentDirectory, "BotConfig.xml");
            BotInfo botInfo = InteropHelper.LoadExcelSpreadsheet(inputPath, InitializationHelper.BotConfigFilePath);

            Logger.Log("Spreadhseet loaded, xml generated at " + InitializationHelper.BotConfigFilePath);

            UCBotHost ucBotHost = new UCBotHost(botInfo.ApplicationUserAgent, botInfo.ApplicationUserAgent,  "Sorry, I was not able to understand you.".EncloseRtf());
            ucBotHost.Run();

            ucBotHost.ErrorOccurred += new BuildABot.Core.ErrorEventHandler(ucBotHost_ErrorOccurred);

            Logger.Log("UCBotHost created, now initializing...");
            ucBotHost.Run();
        }

        static void ucBotHost_ErrorOccurred(object sender, BuildABot.Core.ErrorEventArgs e)
        {
            // This will work OK for Console launchers, but we need to probably log events for Windows Service launchers.
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("An error has occurred:");
            Console.WriteLine(e.Exception.Message);
            Console.ForegroundColor = temp;
        }
    }
}
