/*=====================================================================
  File:      Program.cs

  Summary:   Application entry point.
******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterExe
{
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            AcdLogger logger = null;
            try
            {
                logger = new AcdLogger();
                AcdPlatform platform = new AcdPlatform();

                string configXMLDoc = "";
                using (TextReader reader = new StreamReader("Config.xml"))
                {
                    configXMLDoc = reader.ReadToEnd();
                }

                Console.WriteLine("Starting the ContactCenter.");

                platform.BeginStartUp(configXMLDoc, 
                delegate(IAsyncResult ar)
                {
                    try
                    {
                        platform.EndStartUp(ar);
                        Console.WriteLine("Main: AcdPlatform started");    
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Main: AcdPlatform failed to start.");
                                          
                    }
                                                                    
                }, 
                platform);

                Console.WriteLine("Main: Starting the AcdPlatform");
                Console.WriteLine("Main: Press a Key to close the Application");
                
                bool condition= true;

                while (condition)
                {
                    String command = Console.ReadLine();

                    if (String.IsNullOrEmpty(command))
                    {
                        platform.EndShutdown(platform.BeginShutdown(null, null));
                        condition = false;
                    }
                    else
                    {
                        if (command.Equals("gcCollect", StringComparison.CurrentCultureIgnoreCase))
                        {
                            GC.AddMemoryPressure(int.MaxValue);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            GC.Collect();
                            Console.WriteLine("Force Collection");
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log("Unhandled exception",ex);
                Console.ReadLine();
            }
        }

    }
}
