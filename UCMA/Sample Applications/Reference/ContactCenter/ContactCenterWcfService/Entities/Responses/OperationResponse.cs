/*=====================================================================
  File:      OperationResponse.cs
 
  Summary:   Base class from which the response classes are derived.
 
/*******************************************************************************
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
    /// Base class from which the response classes are derived.
    /// </summary>
    [DataContract]
    [KnownType(typeof(CreateConversationResponse))]
    [KnownType(typeof(EstablishAudioVideoCallResponse))]
    [KnownType(typeof(EstablishInstantMessagingCallResponse))]
    [KnownType(typeof(SendInstantMessageResponse))]
    [KnownType(typeof(TerminateConversationResponse))]
    public class OperationResponse
    {

        #region private variables
        /// <summary>
        /// Operation request.
        /// </summary>
        private OperationRequest m_operationRequest;
        #endregion


        #region protected constructors

        /// <summary>
        /// Operation response.
        /// </summary>
        /// <param name="request">Operation request.</param>
        protected OperationResponse(OperationRequest request)
        {
            m_operationRequest = request;
        }

        #endregion

        #region public properties


        /// <summary>
        /// Gets or sets original request that was passed into the service.
        /// </summary>
        [DataMember]
        public OperationRequest Request
        {
            get
            {
                return m_operationRequest;
            }
            set
            {
                Debug.Assert(null != value, "Request value is null");
                m_operationRequest = value;
            }
        }

        #endregion


    }
}
