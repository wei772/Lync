/*=====================================================================
  File:      AcdLogger.cs

  Summary:   Exposes the ACD logging primitives.

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

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
	public class AcdLogger
    {       
        //private static StringBuilder _eventLogMessages = new StringBuilder();

        public void Log(string message)
        {
            string timeStamp = System.DateTime.Now.ToString("hh:mm:ss.ff");
            message = message + " : " + timeStamp;
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
            //_eventLogMessages.AppendLine(message);
        }

        public void Log(string message, EventLogEntryType eventType)
        {
            if (!System.Diagnostics.EventLog.SourceExists("ACD"))
                System.Diagnostics.EventLog.CreateEventSource(
                   "ACD", "Application");
            EventLog EventLog1 = new EventLog();
            EventLog1.Source = "ACD";
            
            EventLog1.WriteEntry(message,eventType);
        }

        public  void Log(Exception ex)
        {
            if (null != ex)
            {
                Log(ex.ToString(), EventLogEntryType.Error);
            }
        }

        public  void Log(string message, Exception ex)
        {
            if (null == ex)
                Log(message);
            else
                Log(message + ": " + ex.ToString() );
        }

        public void ShutDown()
        {
            //UNDONE: Nice to Have - If the string builder gets larger than the max length of a eventlog entry,
            //split it into multiple event log entries.
            //_eventLogMessages.AppendLine("End of Log");
        }        
	}
}
