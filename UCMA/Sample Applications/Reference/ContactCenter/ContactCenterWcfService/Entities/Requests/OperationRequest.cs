/*=====================================================================
  File:      OperationRequest.cs
 
  Summary:   Base class from which the requests classes are derived.
 
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
    /// Base class from which the request classes are derived.
    /// </summary>
    [DataContract]
    [KnownType(typeof(CreateConversationRequest))]
    [KnownType(typeof(ConversationOperationRequest))]
    [KnownType(typeof(SessionTerminationRequest))]
    public class OperationRequest
    {

        #region private variables

        /// <summary>
        /// Request id.
        /// </summary>
        private string m_requestId = string.Empty;
        #endregion

        #region public properties


        /// <summary>
        /// Gets or sets the request id for this request.
        /// </summary>
        [DataMember]
        public string RequestId
        {
            get { return m_requestId; }
            set { m_requestId = value; }
        }
 
        #endregion
    }
}
