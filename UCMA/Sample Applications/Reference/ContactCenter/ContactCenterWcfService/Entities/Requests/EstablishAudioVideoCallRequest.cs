/*=====================================================================
  File:      EstablishAudioVideoCallRequest.cs
 
  Summary:   Wraps input parameters to create a new audio video call.
 
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
    /// Wraps all the input parameters to create a new call and establish.
    /// </summary>
    [DataContract]
    public class EstablishAudioVideoCallRequest : ConversationOperationRequest
    {
        #region private variables

        /// <summary>
        /// Destination uri.
        /// </summary>
        private string m_destination = string.Empty;

        /// <summary>
        /// Callback uri.
        /// </summary>
        private string m_callbackPhoneNumber = string.Empty;

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the destination uri to use when establishing the IM call.
        /// </summary>
        [DataMember]
        public string Destination
        {
            get { return m_destination; }
            set { m_destination = value; }
        }

        /// <summary>
        /// Gets or sets the callback phone number of the customer. If this is null/empty/or invalid, av call establishment will fail.
        /// </summary>
        [DataMember]
        public string CallbackPhoneNumber
        {
            get { return m_callbackPhoneNumber; }
            set { m_callbackPhoneNumber = value; }
        }
        #endregion

    }
}
