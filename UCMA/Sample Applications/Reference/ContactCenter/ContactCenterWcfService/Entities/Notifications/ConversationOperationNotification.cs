/*=====================================================================
  File:      ConversationOperationNotification.cs
 
  Summary:   Base class from which the notification classes are derived.
 
 ********************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{
    /// <summary>
    /// Base class from which the notification classes are derived.
    /// </summary>
    [DataContract]
    [KnownType(typeof(ConversationTerminationNotification))]
    public class ConversationOperationNotification
    {

        #region constructors
        /// <summary>
        /// Protected constructor.
        /// </summary>
        protected ConversationOperationNotification()
        {
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the conversation associated with this notification.
        /// </summary>
        [DataMember]
        public WebConversation Conversation { get; set; }
        #endregion
    }
}
