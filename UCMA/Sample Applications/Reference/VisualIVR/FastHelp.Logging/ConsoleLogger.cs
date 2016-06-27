/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastHelp.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void Log(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        public void Log(string message, Exception ex)
        {
            Console.WriteLine(message, ex);
        }
    }
}
