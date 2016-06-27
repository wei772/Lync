/*=====================================================================
 File:      AudioVideoCallTerminationNotification.cs
 
 Summary:   Audio video Call termination notification received.
 
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
    /// Reprsents audio video call termination notification.
    /// </summary>
    [DataContract]
    public class AudioVideoCallTerminationNotification
    {
        #region private variables
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the av call associated with this notification.
        /// </summary>
        [DataMember]
        public WebAvCall AvCall { get; set; }
        #endregion
    }
}
