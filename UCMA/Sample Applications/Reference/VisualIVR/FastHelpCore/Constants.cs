/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    public sealed class Constants
    {
        /// <summary>
        /// URL of REST service
        /// </summary>

        public const string RestServiceUrl = "http://ServerName:8082/FastHelpRestService/IVROptions";

        /// <summary>
        /// Prevents a default instance of the <see cref="Constants"/> class from being created.
        /// </summary>
        private Constants()
        {
        }
    }
}
