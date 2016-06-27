/*=====================================================================
  File:      ConversationOperatinoRequest.cs
 
  Summary:   Base class to wrap input parameters for all conversation specific operations..

**********************************************************************************
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
    /// Wraps all the input parameters to conversation specific operations.
    /// </summary>
    [DataContract]
    [KnownType(typeof(EstablishAudioVideoCallRequest))]
    [KnownType(typeof(EstablishInstantMessagingCallRequest))]
    [KnownType(typeof(SendInstantMessageRequest))]
    [KnownType(typeof(TerminateConversationRequest))]
    [KnownType(typeof(LocalComposingStateRequest))]
    public class ConversationOperationRequest : OperationRequest
    {

        #region constructors
        /// <summary>
        /// Protected constructor.
        /// </summary>
        protected ConversationOperationRequest()
        {
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the conversation associated with this request.
        /// </summary>
        [DataMember]
        public WebConversation Conversation { get; set; }
        #endregion

    }
}
