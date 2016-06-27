
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities
{
    internal class PresenceChangedEventArgs : EventArgs
    {
        internal List<PresenceInformation> PresenceSubscriptions { get; set; }
    }
}
