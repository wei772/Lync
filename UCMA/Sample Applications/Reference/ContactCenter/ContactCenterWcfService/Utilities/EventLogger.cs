
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Globalization;


namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities
{
    /// <summary>
    /// Represents helper methods to log events.
    /// </summary>
    internal class EventLogger : Logger
    {
        #region private variables.
        /// <summary>
        /// Application name.
        /// </summary>
        private readonly string m_applicationName;
        #endregion

        #region constructor

        /// <summary>
        /// Creates a new event logger with given application name.
        /// </summary>
        /// <param name="applicationName">Application name.</param>
        internal EventLogger(string applicationName)
        {
            Debug.Assert(!String.IsNullOrEmpty(applicationName), "Application name is null or empty");
            m_applicationName = applicationName;
        }
        #endregion

        #region private properties

        /// <summary>
        /// Gets the application name to use.
        /// </summary>
        private string ApplicationName
        {
            get { return m_applicationName; }
        }
        #endregion

        /// <summary>
        /// Method to log information.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <param name="args">Args.</param>
        internal override void Info(string format, params object[] args)
        {
            string message = String.Format(CultureInfo.CurrentCulture, format, args);
            EventLog.WriteEntry(this.ApplicationName, message, EventLogEntryType.Information);
        }

        /// <summary>
        /// Method to log errors.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <param name="args">Args.</param>
        internal override void Error(string format, params object[] args)
        {
            string message = String.Format(CultureInfo.CurrentCulture, format, args);
            EventLog.WriteEntry(this.ApplicationName, message, EventLogEntryType.Error);
        }

    }
}
