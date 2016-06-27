/*=====================================================================
  File:      CreateConversationResponse.cs
 
  Summary:   Wraps the results returned from creating a conversation.
 
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
    /// Wraps the results returned from an CreateConversation method call.
    /// </summary>
    [DataContract]
    public class CreateConversationResponse : OperationResponse
    {
     
        #region constructors

        /// <summary>
        /// Creates a converssation creation operation response with given conversation.
        /// </summary>
        /// <param name="request">Original request.</param>
        /// <param name="conversation">Conversation that was created, if any.</param>
        public CreateConversationResponse(CreateConversationRequest request, WebConversation conversation)
            : base(request)
        {
            this.Conversation = conversation;
        }
    

        #endregion

        #region public properties

        /// <summary>
        /// Gets the conversation that was created.
        /// </summary>
        [DataMember]
        public WebConversation Conversation { get; set; }
        #endregion
    }
}
