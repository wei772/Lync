/*=====================================================================
 File:      InstantMessageCallTerminationNotification.cs
 
 Summary:   Instant Message Call termination notification received.
 
 ********************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Runtime.Serialization;

using Microsoft.Rtc.Collaboration;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{
    /// <summary>
    /// Reprsents instant message call termination notification.
    /// </summary>
    [DataContract]
    public class InstantMessageCallTerminationNotification
    {
        #region private variables
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the im call associated with this notification.
        /// </summary>
        [DataMember]
        public WebImCall ImCall { get; set; }
        #endregion

    }
}
