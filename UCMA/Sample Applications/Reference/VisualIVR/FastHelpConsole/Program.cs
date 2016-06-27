/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpConsole
{
    using System;
    using FastHelpServer;

    /// <summary>
    /// Main Program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Mains the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
            public static void Main(string[] args)
            {
                FastHelpServerApp ucmabot = new FastHelpServerApp();
                ucmabot.Start();
                Console.ReadLine();
                ucmabot.Stop();
            }
    }
}
