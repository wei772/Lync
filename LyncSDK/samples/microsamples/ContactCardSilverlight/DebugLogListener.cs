/*=====================================================================
  This file is part of the Microsoft Unified Communications Code Samples.

  Copyright (C) 2012 Microsoft Corporation.  All rights reserved.

This source code is intended only as a supplement to Microsoft
Development Tools and/or on-line documentation.  See these other
materials for detailed information regarding Microsoft code samples.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
=====================================================================*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Lync.Utilities.Logging;

namespace ContactCardSilverlight
{
    /// <summary>
    /// DebugLogListener provides a listener that writes to the debug output
    /// </summary>    
    public class DebugLogListener : LogListener
    {
        /// <summary>
        /// Write a LogEntry instance
        /// </summary>
        /// <param name="logEntry"></param>
        public override void Write(LogEntry logEntry)
        {
            if (!Filter(logEntry))
            {
                Debug.WriteLine(logEntry.ToString());
            }
        }
    }
}