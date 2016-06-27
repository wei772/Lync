namespace ExcelBot.Util
{
    using System.IO;
    using System;
    using System.Diagnostics;
    using System.Reflection;

    /// <summary>
    /// Primary logger.
    /// </summary>
    public class Logger
    {
        private static string logFilePath;
        /// <summary>
        /// The default log file path.
        /// </summary>
        public static string DefaultLogFilePath { get; private set; }

        /// <summary>
        /// Gets or sets the log file path.
        /// </summary>
        /// <value>The log file path.</value>
        public static string LogFilePath
        {
            get { return Logger.logFilePath; }
            set
            {

                string directoryPath = Path.GetDirectoryName(value);

                bool directoryExists = Directory.Exists(directoryPath);
                if (!directoryExists)
                {
                    try
                    {
                        Directory.CreateDirectory(directoryPath);
                        directoryExists = true;
                    }
                    catch
                    {
                        Debug.WriteLine(String.Format("Failed to create log directory {0}. Default log file path will be used: {1}", directoryPath, Logger.DefaultLogFilePath));
                    }
                }

                if (directoryExists)
                {
                    Logger.logFilePath = value;
                }
            }
        }

        /// <summary>
        /// Initializes the <see cref="Logger"/> class.
        /// </summary>
        static Logger()
        {
            string currentDirectory = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().Location));
            Logger.DefaultLogFilePath = Path.Combine(currentDirectory, "ExcelBotLog.txt");
            Logger.LogFilePath = Logger.DefaultLogFilePath;
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Log(string message)
        {
            try
            {
                StreamWriter sw = new StreamWriter(Logger.LogFilePath, true);
                sw.WriteLine(String.Format("[{0}]: {1}", DateTime.Now, message));
                sw.Close();
            }
            catch { }
        }
    }
}
