/*=====================================================================
  File:      Logger.cs

  Summary:   Implements basic logging.
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/


using System;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Globalization;


namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    public class Logger
    {
        private bool m_isEnabled = true;
        private LogLevel m_logSwitch = LogLevel.All;
        private System.IO.StreamWriter m_fileStreamWriter;
        private object m_syncRoot = new object();

        [Flags]
        public enum LogLevel
        {        
            Error       = 1,
            Warning     = 2,
            Info        = 4,
            Verbose     = 8,
            Event       = 16,
            All         = Error | Warning | Info | Verbose | Event
        }


        public void EnableLog()
        {
            m_isEnabled = true;
        }

        public void DisableLog()
        {
            m_isEnabled = false;
        }

        public LogLevel LogSwitch
        {
            get
            {
                return m_logSwitch;
            }
            set
            {
                m_logSwitch = value;
            }
        }

        public void OpenLogFile(string filePath)
        {
            lock (m_syncRoot)
            {
                if (m_fileStreamWriter != null)
                {
                    m_fileStreamWriter.Close();
                    m_fileStreamWriter = null; 
                }
                try
                {
                    m_fileStreamWriter = new System.IO.StreamWriter(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to open log file. Exception = {0}", ex.ToString());
                }
            }
        }

        public void CloseLogFile()
        {
            lock (m_syncRoot)
            {
                m_fileStreamWriter.Close();
                m_fileStreamWriter = null;
            }
        }

        public string Pointer(object obj)
        {
            string s = String.Empty;

            if (obj != null)
            {
                // For tracking specific object instances, use the commented line (i.e. uncomment it and comment the next line).
                s = String.Concat("<",
                                    //String.Concat(obj.GetType().Name, "_", obj.GetHashCode().ToString(CultureInfo.InvariantCulture)),
                                    obj.GetType().Name,
                                    ">");
            }

            return s;
        }

        public void Log(LogLevel logLevel, string message)
        {
            if (m_isEnabled && (logLevel & m_logSwitch) != 0)
            {
                message = this.GetTimeStampedMessage(message);
                Console.WriteLine(message);
                System.Diagnostics.Debug.WriteLine(message);
                if (m_fileStreamWriter != null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_fileStreamWriter != null)
                        {
                            m_fileStreamWriter.WriteLine(message);
                        }
                    }
                }
            }
        }

        public void Log(string message, EventLogEntryType eventType)
        {
            if (m_isEnabled && (LogLevel.Event & m_logSwitch) != 0)
            {
                if (!System.Diagnostics.EventLog.SourceExists("VoiceCompanionSample"))
                    System.Diagnostics.EventLog.CreateEventSource(
                       "VoiceCompanionSample", "Application");
                EventLog EventLog1 = new EventLog();
                EventLog1.Source = "VoiceCompanionSample";
                message = this.GetTimeStampedMessage(message);
                EventLog1.WriteEntry(message, eventType);

            }
            if (m_isEnabled && ((LogLevel.Warning & m_logSwitch) != 0))
            {
                Console.WriteLine(message);
                System.Diagnostics.Debug.WriteLine(message);
                if (m_fileStreamWriter != null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_fileStreamWriter != null)
                        {
                            m_fileStreamWriter.WriteLine(message);
                        }
                    }
                }
            }
        }

        public void Log(Exception ex)
        {
            LogLevel logLevel = LogLevel.Error;
            if (ex == null)
            {
                logLevel = LogLevel.Warning;
            }
            Log(logLevel, ex);
        }

        public void Log(LogLevel logLevel, Exception ex)
        {
            Log(logLevel, ex.ToString());
        }

        public void Log(LogLevel logLevel, string message, Exception ex)
        {
            Log(logLevel, message + "\r\nException: " + ex.ToString());
        }

        private string GetTimeStampedMessage(string message)
        {
            string timeStamp = System.DateTime.Now.ToString("hh:mm:ss.ff");
            return (timeStamp + ": " + message);
        }
    }
}
