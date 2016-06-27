/*=====================================================================
  File:      Application.cs


***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/



using System;
using System.Configuration;
using Microsoft.Rtc.Signaling;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    internal class Application
    {
        #region Private fields
        private AppPlatform m_platform;

        #endregion

        #region Public methods

        public void Run()
        {
            this.StartPlatform(); 
        }

        public void Stop()
        {
            this.ShutdownPlatform();
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public Logger Logger
        {
            get
            {
                return m_platform.Logger;
            }
        }

        /// <summary>
        /// Gets the platform.
        /// </summary>
        public AppPlatform Platform
        {
            get
            {
                return m_platform;
            }
        }
        #endregion


        #region Implementation

        private void StartPlatform()
        {
            string userAgent;
            string applicationId;

            GetPlatformSettings(out userAgent, out applicationId);

            m_platform = new AppPlatform(userAgent, applicationId);
            bool isLogEnabled;
            Logger.LogLevel logSwitch;
            Application.GetLoggerSetting(out isLogEnabled, out logSwitch);
            if (isLogEnabled)
            {
                this.Logger.EnableLog();
            }
            else
            {
                this.Logger.DisableLog();
            }
            this.Logger.LogSwitch = logSwitch;
            try
            {
                m_platform.BeginStartup(
                    ar =>
                    {
                        try { m_platform.EndStartup(ar); }
                        catch (RealTimeException e) { Console.Write(e.ToString()); }
                    },
                    null);
            }
            catch (InvalidOperationException exp)
            {
                Logger.Log(Logger.LogLevel.Error, "Unable to start the platform.", exp);
            }
        }

        private static void GetPlatformSettings(out string userAgent, out string applicationId)
        {
            userAgent = ConfigurationManager.AppSettings["UserAgent"];
            applicationId = ConfigurationManager.AppSettings["applicationId"];

        }

        private static void GetLoggerSetting(out bool isEnabled, out Logger.LogLevel logSwitch)
        {
            string isEnabledString = ConfigurationManager.AppSettings["Log"];
            string logSwitchString = ConfigurationManager.AppSettings["LogSwitch"];
            isEnabled = true;
            if (String.Compare(isEnabledString, "Enabled", true) != 0)
            {
                isEnabled = false;
            }
            logSwitch = Logger.LogLevel.All;
            int temp;
            if (!int.TryParse(logSwitchString, out temp))
            {
                temp = (int) Logger.LogLevel.All;
            }
            logSwitch = (Logger.LogLevel)temp;
        }

        private void ShutdownPlatform()
        {
            if (m_platform != null)
            {
                m_platform.BeginShutdown(
                    delegate(IAsyncResult ar)
                    {
                        m_platform.EndShutdown(ar);
                    },
                    null);
            }
        }

        #endregion
    }

}
