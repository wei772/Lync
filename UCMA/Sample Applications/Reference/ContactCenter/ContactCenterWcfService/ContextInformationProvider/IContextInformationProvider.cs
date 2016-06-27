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
    /// Represents context information interface.
    /// </summary>
    public interface IContextInformationProvider
    {
        MimePartContentDescription GenerateContextMimePartContentDescription(IDictionary<string, string> incomingContext);
    }
}
