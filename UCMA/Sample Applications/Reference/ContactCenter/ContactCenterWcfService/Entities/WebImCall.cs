/*=====================================================================
  File:      WebImCall.cs
 
  Summary:   Represents an im call object.
 
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{
    /// <summary>
    /// Represents a web im call.
    /// </summary>
    [DataContract]
    public class WebImCall
    {
        #region private variables

        /// <summary>
        /// Ucma im call.
        /// </summary>
        private readonly InstantMessagingCall m_imCall;
        #endregion

        #region internal constructor

        /// <summary>
        /// Web im call.
        /// </summary>
        /// <param name="conversation">Web conversation to which this call belongs to.</param>
        internal WebImCall(InstantMessagingCall imCall, WebConversation conversation)
        {
            Debug.Assert(null != imCall, "imCall is null");
            Debug.Assert(null != conversation, "conversation is null");
            m_imCall = imCall;
            this.WebConversation = conversation;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the web conversation.
        /// </summary>
        [DataMember]
        public WebConversation WebConversation { get; set; }


        /// <summary>
        /// Gets the Im call associated with this web im call.
        /// </summary>
        public InstantMessagingCall ImCall
        {
            get { return m_imCall; }
        }
        #endregion
    }
}
