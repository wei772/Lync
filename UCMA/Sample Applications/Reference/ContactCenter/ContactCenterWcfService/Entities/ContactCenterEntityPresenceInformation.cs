/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{
    [DataContract]
    public class ContactCenterEntityPresenceInformation
    {

        #region constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="presenceInformation">Presence information. Cannot be null.</param>
        internal ContactCenterEntityPresenceInformation(string entityName, PresenceInformation presenceInformation)
        {
            this.EntityName = entityName;
            this.Status = presenceInformation.Status;
            this.Availability = presenceInformation.Availability;
            this.ActivityStatus = presenceInformation.ActivityStatus;
        }
        #endregion

        #region Data members
        /// <summary>
        /// Gets or sets the contact center entity name.
        /// </summary>
        [DataMember]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the current availabilty.
        /// </summary>
        [DataMember]
        public long Availability { get; set; }

        /// <summary>
        /// Gets or sets the current status.
        /// </summary>
        [DataMember]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the current activity status.
        /// </summary>
        [DataMember]
        public string ActivityStatus { get; set; }
        #endregion
    }
}
