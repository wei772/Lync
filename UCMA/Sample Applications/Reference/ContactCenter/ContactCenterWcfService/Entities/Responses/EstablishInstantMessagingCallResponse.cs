/*=====================================================================
  File:      EstablishInstantMessagingCallResponse.cs
 
  Summary:   Wraps the results returned from creating a new call.
 
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
    /// Wraps the results returned from establishing an IM call.
    /// </summary>
    [DataContract]
    public class EstablishInstantMessagingCallResponse : OperationResponse
    {

        #region constructors

        /// <summary>
        /// Constructor to create new call establishment response.
        /// </summary>
        /// <param name="request">Original request.</param>
        /// <param name="webImCall">Web im call.</param>
        internal EstablishInstantMessagingCallResponse(EstablishInstantMessagingCallRequest request, WebImCall webImCall)
            : base(request)
        {
            this.ImCall = webImCall;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the web im call that was created.
        /// </summary>
        [DataMember]
        public WebImCall ImCall { get; set; }
        #endregion
    }
}
