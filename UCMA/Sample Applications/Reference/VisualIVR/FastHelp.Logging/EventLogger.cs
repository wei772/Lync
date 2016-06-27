/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace FastHelp.Logging
{
    public class EventLogger : ILogger
    {
        private string eventSource;
        private const string LOGNAME = "Application";

        public EventLogger(string eventSource)
        {
            this.eventSource = eventSource;
            if (!EventLog.SourceExists(eventSource))
            {
                var data = new EventSourceCreationData(eventSource, LOGNAME);
                EventLog.CreateEventSource(data);
            }
        }

        public void Log(string message)
        {
            EventLog.WriteEntry(eventSource, message, EventLogEntryType.Information,9897);
        }

        public void Log(string message, params object[] args)
        {
            EventLog.WriteEntry(eventSource, string.Format(message, args), EventLogEntryType.Information, 9898);
        }

        public void Log(string message, Exception ex)
        {
            EventLog.WriteEntry(eventSource, string.Format(message, ex), EventLogEntryType.Error, 9899);
        }

       
    }
}
