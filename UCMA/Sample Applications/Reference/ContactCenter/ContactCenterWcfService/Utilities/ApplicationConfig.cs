
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.Configuration;
using System.Globalization;


namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities
{
    /// <summary>
    /// Class to load and populate the configuration setting.
    /// </summary>
    internal static class Configuration
    {
        /// <summary>
        /// Application id.
        /// </summary>
        private readonly static string applicationId = ConfigurationManager.AppSettings["ApplicationId"];

        /// <summary>
        /// Application user agent.
        /// </summary>
        private readonly static string applicationUserAgent = ConfigurationManager.AppSettings["ApplicationUserAgent"];

        /// <summary>
        /// Contact center application id.
        /// </summary>
        private readonly static string contactCenterApplicationId = ConfigurationManager.AppSettings["ContactCenterApplicationId"];

        /// <summary>
        /// Product guid key.
        /// </summary>
        private readonly static string productGuidKey = ConfigurationManager.AppSettings["ProductGuidKey"];

        /// <summary>
        /// Db connection string.
        /// </summary>
        private readonly static string dbConnectionString = ConfigurationManager.AppSettings["DbConnectionString"];
        

        /// <summary>
        /// Gets the application id from the application configuration
        /// </summary>
        public static string ApplicationId
        {
            get
            {
                return Configuration.applicationId;
            }
        }

        /// <summary>
        /// Gets the application user agent from the application configuration
        /// </summary>
        public static string ApplicationUserAgent
        {
            get
            {
                return Configuration.applicationUserAgent;
            }
        }

        /// <summary>
        /// Gets the key to look for to extract product guid.
        /// </summary>
        public static string ProductGuidKey
        {
            get
            {
                return Configuration.productGuidKey;
            }
        }

        /// <summary>
        /// Gets the connection string for the DB.
        /// </summary>
        public static string DbConnectionString
        {
            get
            {
                return Configuration.dbConnectionString;
            }
        }

        /// <summary>
        /// Gets the contact center application id from the application configuration
        /// </summary>
        public static string ContactCenterApplicationId
        {
            get
            {
                return Configuration.contactCenterApplicationId;
            }
        }
    }
}