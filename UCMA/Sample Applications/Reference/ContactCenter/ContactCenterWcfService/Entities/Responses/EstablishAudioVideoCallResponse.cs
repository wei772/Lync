/*=====================================================================
  File:      EstablishAudioVideoCallResponse.cs
 
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
    /// Wraps the results returned from establishing an Av call.
    /// </summary>
    [DataContract]
    public class EstablishAudioVideoCallResponse : OperationResponse
    {

        #region constructors

        /// <summary>
        /// Constructor to create new call establishment response.
        /// </summary>
        /// <param name="request">Original request.</param>
        /// <param name="avCall">Av call.</param>
        internal EstablishAudioVideoCallResponse(EstablishAudioVideoCallRequest request, WebAvCall avCall)
            : base(request)
        {
            this.AvCall = avCall;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the web av call that was created.
        /// </summary>
        [DataMember]
        public WebAvCall AvCall { get; set; }
        #endregion
    }
}
