/*=====================================================================
  File:      InstantMessageReceivedNotification.cs
 
  Summary:   Instant message received notification message.
 
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
    /// Reprsents instant message received notification message..
    /// </summary>
    [DataContract]
    public class InstantMessageReceivedNotification
    {
        #region private variables

        /// <summary>
        /// Message.
        /// </summary>
        private string m_receivedInstantMessage = string.Empty;

        /// <summary>
        /// Message sender.
        /// </summary>
        private string m_sender = string.Empty;

        /// <summary>
        /// Message id.
        /// </summary>
        private int m_messageId;
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the im call associated with this notification.
        /// </summary>
        [DataMember]
        public WebImCall ImCall { get; set; }

        /// <summary>
        /// Gets or sets the message received.
        /// </summary>
        [DataMember]
        public string MessageReceived
        {
            get { return m_receivedInstantMessage; }
            set { m_receivedInstantMessage = value; }
        }

        /// <summary>
        /// Gets or sets the message sender.
        /// </summary>
        [DataMember]
        public string MessageSender
        {
            get { return m_sender; }
            set { m_sender = value; }
        }

        /// <summary>
        /// Gets or sets the message sender.
        /// </summary>
        [DataMember]
        public int MessageId
        {
            get { return m_messageId; }
            set { m_messageId = value; }
        }

        #endregion

    }
}
