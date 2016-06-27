/*=====================================================================
  File:    Program.cs  

  Summary:  Contains the main program for this application. 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/


using System;
using System.Configuration;


namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    class Program
    {
        private Application m_application;

        static void Main(string[] args)
        {
            Program myProgram = new Program();
            myProgram.Run();
        }

        public void Run()
        {
            m_application = new Application();
            m_application.Run();
            Console.WriteLine("Press any key to stop... -- Type ? for help.");
            Console.WriteLine("Current Logger Level is {0}", m_application.Logger.LogSwitch);
            string logFile = ConfigurationManager.AppSettings["LogFile"];
            if (!String.IsNullOrEmpty(logFile))
            {
                m_application.Logger.OpenLogFile(logFile);
                Console.WriteLine("Current Log File is {0}", logFile);
            }
            string line;
            bool exit = false;
            while (!exit)
            {
                line = Console.ReadLine();
                if (String.Compare(line, "quit", true) == 0 ||
                    String.Compare(line, "exit", true) == 0)
                {
                    exit = true;
                }
                else if (String.Compare(line, "?") == 0)
                {
                    this.PrintHelp();
                }
                else if (line.StartsWith("add", StringComparison.OrdinalIgnoreCase))
                {
                    string[] tokens = line.Split(' ');
                    m_application.Platform.ReverseNumberLookUp.AddEntry(tokens[1], tokens[2]);
                }
                else
                {
                    this.ProcessLoggerCommand(line);
                }
            }

            m_application.Stop();
            Console.WriteLine("Application stopped successfull. Press any key to close window.");

            Console.ReadKey();
        }

        /// <summary>
        /// Print Help Information.
        /// </summary>
        private void PrintHelp()
        {
            Console.WriteLine("quit or exit -- Stop the program.");
            Console.WriteLine("? -- Print this help.");
            Console.WriteLine("log -- Enable logging.");
            Console.WriteLine("nolog -- Diable logging.");
            Console.WriteLine("verbose|info|event|error|warning -- Add that logging level to the switch.");
            Console.WriteLine("logswitch <switchval> -- Set logging level to the value given.");
        }

        /// <summary>
        /// Process logger commands, if any, from given line. If none, no action taken.
        /// </summary>
        /// <param name="line">The input line.</param>
        private bool ProcessLoggerCommand(string line)
        {
            bool isLoggerCommand = true;
            if (String.Compare(line, "log", true) == 0)
            {
                m_application.Logger.EnableLog();
            }
            else if (String.Compare(line, "nolog", true) == 0)
            {
                m_application.Logger.DisableLog();
            }
            else if (String.Compare(line, "verbose", true) == 0)
            {
                m_application.Logger.LogSwitch = m_application.Logger.LogSwitch | Logger.LogLevel.Verbose;
            }
            else if (String.Compare(line, "info", true) == 0)
            {
                m_application.Logger.LogSwitch = m_application.Logger.LogSwitch | Logger.LogLevel.Info;
            }
            else if (String.Compare(line, "error", true) == 0)
            {
                m_application.Logger.LogSwitch = m_application.Logger.LogSwitch | Logger.LogLevel.Error;
            }
            else if (String.Compare(line, "warning", true) == 0)
            {
                m_application.Logger.LogSwitch = m_application.Logger.LogSwitch | Logger.LogLevel.Warning;
            }
            else if (String.Compare(line, "event", true) == 0)
            {
                m_application.Logger.LogSwitch = m_application.Logger.LogSwitch | Logger.LogLevel.Event;
            }
            else if (String.Compare(line, "closelogfile", true) == 0)
            {
                m_application.Logger.CloseLogFile();
            }
            else if (line.Length > 0)
            {
                String[] tokens = line.Split(' ');
                if (tokens.Length == 2 && String.Compare(tokens[0], "logswitch", true) == 0)
                {
                    int level;
                    if (int.TryParse(tokens[1], out level))
                    {
                        m_application.Logger.LogSwitch = (Logger.LogLevel)level;
                    }
                }
                else if (tokens.Length == 2 && String.Compare(tokens[0], "openlogfile", true) == 0)
                {
                    m_application.Logger.OpenLogFile(tokens[1]);
                }
            }
            else
            {
                isLoggerCommand = false;
            }
            if (isLoggerCommand)
            {
                Console.WriteLine("Current Logger Level is {0}", m_application.Logger.LogSwitch);
            }
            return isLoggerCommand;
        }

    }
}
