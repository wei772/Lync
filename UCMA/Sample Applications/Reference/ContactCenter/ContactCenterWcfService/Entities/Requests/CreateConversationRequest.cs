/*=====================================================================
  File:      CreateConversationRequest.cs
 
  Summary:   Wraps input parameters to create a new conversation.
 
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/




using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{
    /// <summary>
    /// Wraps all the input parameters to create a new conversation.
    /// </summary>
    [DataContract]
    public class CreateConversationRequest : OperationRequest
    {
        #region private members

        /// <summary>
        /// Conversation subject.
        /// </summary>
        private string m_conversationSubject = string.Empty;

        /// <summary>
        /// Display name to be used.
        /// </summary>
        private string m_displayName = string.Empty;
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the subject of the new conversation to be created.
        /// </summary>
        [DataMember]
        public string ConversationSubject 
        {
            get { return m_conversationSubject; }
            set { m_conversationSubject = value; }
        }

        /// <summary>
        /// Gets or sets the customer display name.
        /// </summary>
        [DataMember]
        public string DisplayName 
        {
            get { return m_displayName; }
            set { m_displayName = value; }
        }

        /// <summary>
        /// Gets or sets the conversation context.
        /// </summary>
        [DataMember]
        public Dictionary<string, string> ConversationContext { get; set; }

        #endregion
    }
}
