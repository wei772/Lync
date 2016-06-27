
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;
using System.Data.SqlClient;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common
{
    public class SqlHelper
    {
        #region Fields

        private Dictionary<String, DbConnection> _cachedConnections = new Dictionary<string, DbConnection>();

        #endregion

        #region Constructors

        static SqlHelper()
        {
            Current = new SqlHelper();
        }

        #endregion

        #region Properties

        public static SqlHelper Current { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a cached connection.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public DbConnection GetConnection(string connectionString)
        {
            if (!_cachedConnections.ContainsKey(connectionString))
            {
                DbConnection connection = new SqlConnection(connectionString);
                _cachedConnections.Add(connectionString, connection);
            }

            return _cachedConnections[connectionString];
        }

        #endregion
    }
}