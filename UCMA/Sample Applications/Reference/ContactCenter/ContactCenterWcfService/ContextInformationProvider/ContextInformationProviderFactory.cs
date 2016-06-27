/********************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.ContactCenterWcfService.ContextInformation
{
    /// <summary>
    /// Factory class to create a new context information provider.
    /// </summary>
    public class ContextInformationProviderFactory
    {
        /// <summary>
        /// Gets the default context information provider.
        /// </summary>
        /// <returns>Context information provider.</returns>
        public static IContextInformationProvider GetDefaultContextInformationProvider()
        {
            //This behavior can be changed by implementing other context information providers.
            return new ProductContextContextInformationProvider();
        }
    }
}
