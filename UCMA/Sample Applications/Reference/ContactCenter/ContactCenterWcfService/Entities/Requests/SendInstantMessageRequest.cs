/*=====================================================================
  File:      SendInstantMessageRequest.cs
 
  Summary:   Wraps input parameters to send an instant message.
 
 ********************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Runtime.Serialization;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{
    /// <summary>
    /// Wraps all the input parameters to send a IM message.
    /// </summary>
    [DataContract]
    public class SendInstantMessageRequest : ConversationOperationRequest
    {
        #region private variables

        /// <summary>
        /// Message.
        /// </summary>
        private string m_instantMessage = string.Empty;
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the message to be sent.
        /// </summary>
        [DataMember]
        public string Message
        {
            get { return m_instantMessage; }
            set { m_instantMessage = value; }
        }

        #endregion

    }
}
