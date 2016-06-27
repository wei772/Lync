/*=====================================================================
  File:      SessionTerminationRequest.cs
 
  Summary:   Wraps input parameters of session termination notification
             from clients.
 
 ********************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{
    /// <summary>
    /// Wraps all the input parameters session termination request.
    /// </summary>
    [DataContract]
    public class SessionTerminationRequest : OperationRequest
    {
        #region private members

        /// <summary>
        /// List of all client conversations.
        /// </summary>
        private Collection<WebConversation> m_clientConversations;
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the list of client conversations.
        /// </summary>
        [DataMember]
        public Collection<WebConversation> Conversations
        {
            get { return m_clientConversations; }
            set { m_clientConversations = value; }
        }

        #endregion
    }
}

