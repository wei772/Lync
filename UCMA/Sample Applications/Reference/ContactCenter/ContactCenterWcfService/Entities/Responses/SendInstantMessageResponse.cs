/*=====================================================================
  File:      SendInstantMessageResponse.cs
 
  Summary:   Wraps the results returned from sending an IM message.
 
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/



using System;
using System.Runtime.Serialization;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{
    /// <summary>
    /// Wraps the results returned from sending an IM message.
    /// </summary>
    [DataContract]
    public class SendInstantMessageResponse : OperationResponse
    {

        #region constructors

        /// <summary>
        /// Constructor to create send messsage response.
        /// </summary>
        /// <param name="request">Original request.</param>
        public SendInstantMessageResponse(SendInstantMessageRequest request)
            : base(request)
        {
        }

        #endregion
    }
}
