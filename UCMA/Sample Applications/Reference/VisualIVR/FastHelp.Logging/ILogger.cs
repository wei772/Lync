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
    public interface ILogger
    {
        void Log(string message);
        void Log(string message, params object[] args);
        void Log(string message, Exception ex);
    }
}
