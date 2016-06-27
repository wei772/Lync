
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
    /// Represents Logger interface.
    /// </summary>
    internal abstract class Logger   
    {
        /// <summary>
        /// Method to log information.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <param name="args">Args.</param>
        internal abstract void Info(string format, params object[] args);

        /// <summary>
        /// Method to log errors.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <param name="args">Args.</param>
        internal abstract void Error(string format, params object[] args);

        /// <summary>
        /// Helper method to extract exception details.
        /// </summary>
        /// <param name="e">Exception.</param>
        /// <returns>Exception details as a string.</returns>
        internal static string ToString(Exception e)
        {
            string retVal = null;
            if (e != null)
            {
                StringBuilder sb = new StringBuilder(e.Message);
                sb.AppendLine(e.StackTrace);
                Exception innerException = e.InnerException;

                while (innerException != null)
                {

                    sb.AppendLine("Inner Exception:");
                    sb.AppendLine(innerException.Message);
                    sb.AppendLine(innerException.StackTrace);
                    innerException = innerException.InnerException;
                }
                retVal = sb.ToString();
            }
            else
            {
                retVal = "<NULL>";
            }
            return retVal;
        }
        
    }


    /// <summary>
    /// Represents helper methods to log messages to console.
    /// </summary>
    internal class ConsoleLogger : Logger
    {
        #region string consts

        private const string INFO = "INFO:";
        private const string ERROR = "ERRROR:";
        #endregion

        #region private variables

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
        internal ConsoleLogger(string applicationName)
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
            Console.WriteLine("{0} {1}", ConsoleLogger.INFO, message);
        }

        /// <summary>
        /// Method to log errors.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <param name="args">Args.</param>
        internal override void Error(string format, params object[] args)
        {
            string message = String.Format(CultureInfo.CurrentCulture, format, args);
            Console.WriteLine("{0} {1}", ConsoleLogger.ERROR, message);
        }

    }
}
