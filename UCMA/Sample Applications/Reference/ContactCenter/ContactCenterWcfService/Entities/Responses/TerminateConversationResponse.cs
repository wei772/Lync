/*=====================================================================
  File:      TerminateConversationResponse.cs
 
  Summary:   Wraps input parameters to terminate a conversation.
 
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
    /// Wraps the results returned from terminating a conversation.
    /// </summary>
    [DataContract]
    public class TerminateConversationResponse : OperationResponse
    {

        #region constructors

        /// <summary>
        /// Constructor to create a new terminate conversation response.
        /// </summary>
        /// <param name="request">Original request.</param>
        public TerminateConversationResponse(TerminateConversationRequest request)
            : base(request)
        {
        }


        #endregion
    }
}
