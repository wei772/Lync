/*=====================================================================
  File:      RemoteComposingStatusNotification.cs
 
  Summary:   composing status notification received.
 
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
    /// Represents remote composing status notification message..
    /// </summary>
    [DataContract]
    public class RemoteComposingStatusNotification
    {
        #region private variables

        /// <summary>
        /// RemoteComposingStatus.
        /// </summary>
        private RemoteComposingStatus m_remoteComposingStatus;

        /// <summary>
        /// Remote participant uri.
        /// </summary>
        private string m_remoteParticipantUri;
        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets the im call associated with this notification.
        /// </summary>
        [DataMember]
        public WebImCall ImCall { get; set; }

        /// <summary>
        /// Gets or sets the remote composing status.
        /// </summary>
        [DataMember]
        public RemoteComposingStatus RemoteComposingStatus
        {
            get { return m_remoteComposingStatus; }
            set { m_remoteComposingStatus = value; }
        }

        /// <summary>
        /// Gets or sets the remote participant uri.
        /// </summary>
        [DataMember]
        public String Participant
        {
            get { return m_remoteParticipantUri; }
            set { m_remoteParticipantUri = value; }
        }

        #endregion

    }

    /// <summary>
    /// Represents remote composing status.
    /// </summary>
    public enum RemoteComposingStatus
    {
        /// <summary>
        /// Currently, user is not typing.
        /// </summary>
        Idle = 0,
        /// <summary> 
        /// Currently, user is actively typing.
        /// </summary>
        Active
    }
}
