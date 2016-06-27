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
    /// <summary>
    /// Represents presence subscription.
    /// </summary>
    public class PresenceInformation
    {

        #region static const strings
        /// <summary>
        /// No chance const string.
        /// </summary>
        public const string NoChange = "No change";

        /// <summary>
        /// No change in availability.
        /// </summary>
        public const long NoChangeInAvailability = -99;

        #endregion

        #region internal properties
        /// <summary>
        /// Gets or sets the contact name.
        /// </summary>
        internal string ContactName { get; set; }

        /// <summary>
        /// Gets or sets the current availabilty.
        /// </summary>
        internal long Availability { get; set; }

        /// <summary>
        /// Gets or sets the current status.
        /// </summary>
        internal string Status { get; set; }

        /// <summary>
        /// Gets or sets the activity status.
        /// </summary>
        internal string ActivityStatus { get; set; }

        /// <summary>
        /// Gets or sets the sip uri that corresponds to the contact.
        /// </summary>
        internal string SipUri { get; set; }

        /// <summary>
        /// Gets or sets the subscription create time.
        /// </summary>
        internal DateTime SubscriptionCreateTime { get; set; }

        #endregion

        #region overridden methods
        /// <summary>
        /// Overridden equals method.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var compareObj = obj as PresenceInformation;
            if (compareObj == null)
            {
                return false;
            }
            else
            {
                return compareObj.SipUri == this.SipUri;
            }
        }

        /// <summary>
        /// Overriddent get hashcode method.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return SipUri.GetHashCode();
        }


        /// <summary>
        /// Overridden to string method.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                string.Format("PresenceSubscription => [ContactName = {0}, Availability = {1}, Status = {2}, SubscriptionCreated = {3}, ActivityStatus = {4} ]",
                    this.ContactName, this.Availability, this.Status, this.SubscriptionCreateTime, this.ActivityStatus);
        }

        #endregion
    }
}
