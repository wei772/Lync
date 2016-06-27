/*=====================================================================
  File:      Helper.cs
 
  Summary:   Represents static helper methods..
 

/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Xml.Serialization;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using Microsoft.Rtc.Collaboration;
using System.Collections.Generic;
using System.Net.Mime;
using System.Xml;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;
using System.Data.Common;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities
{

    #region internal class Helper

    /// <summary>
    /// Represents static helper methods.
    /// </summary>
    internal static class Helper
    {
        #region consts

        /// <summary>
        /// Sip string constant.
        /// </summary>
        internal const string Sip = "sip:";

        /// <summary>
        /// Tel string constant.
        /// </summary>
        internal const string Tel = "tel:";

        /// <summary>
        /// Application name.
        /// </summary>
        private const string ApplicationName = "ContactCenterWcfService";

        /// <summary>
        /// Default logger.
        /// </summary>
        private static readonly Logger DefaultLogger = new EventLogger(Helper.ApplicationName);

        //To use console logging comment the above line and uncomment the line below.
        //private static readonly Logger DefaultLogger = new ConsoleLogger(Helper.ApplicationName);
        #endregion

        #region properties

        /// <summary>
        /// Gets the logger to use.
        /// </summary>
        internal static Logger Logger
        {
            get { return DefaultLogger; }
        }
        #endregion

        #region methods

        /// <summary>
        /// Constructs a phone uri from the phone number.
        /// </summary>
        /// <param name="appSuppliedPhoneNumber">Application supplied phone number. Cannot be null or empty.</param>
        /// <returns>Uri based on the given uri.</returns>
        internal static string GetCallbackPhoneUri(string appSuppliedPhoneNumber)
        {
            string phoneUriToReturn = appSuppliedPhoneNumber;
            Debug.Assert(!String.IsNullOrEmpty(appSuppliedPhoneNumber), "appSuppliedPhoneNumber is null or empty");
            if (!phoneUriToReturn.StartsWith(Helper.Tel, StringComparison.OrdinalIgnoreCase) &&
                !phoneUriToReturn.StartsWith(Helper.Sip, StringComparison.OrdinalIgnoreCase))
            {
                //Application did not supply a sip: or tel: uri. So try to add tel: after formatting the phone number.
                phoneUriToReturn = Helper.Tel + phoneUriToReturn;
            }
            return phoneUriToReturn;
        }

        #endregion

    }

    #endregion
}