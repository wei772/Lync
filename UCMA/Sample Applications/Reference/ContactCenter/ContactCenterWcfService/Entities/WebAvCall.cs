/*=====================================================================
  File:      WebAvCall.cs
 
  Summary:   Represents an av call object.
 
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
using Microsoft.Rtc.Collaboration.AudioVideo;
using System.Diagnostics;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{
    /// <summary>
    /// Represents a web av call.
    /// </summary>
    [DataContract]
    public class WebAvCall
    {
        #region private variables

        /// <summary>
        /// Audio video call.
        /// </summary>
        private readonly AudioVideoCall m_avCall;
        #endregion

        #region public constructor
        /// <summary>
        /// Creates a new web av call with given web conversation.
        /// </summary>
        /// <param name="avCall">Av call.</param>
        /// <param name="conversation">Web conversation.</param>
        internal WebAvCall(AudioVideoCall avCall, WebConversation conversation)
        {
            Debug.Assert(null != conversation, "conversation is null");
            Debug.Assert(null != avCall, "av call is null");
            this.WebConversation = conversation;
            m_avCall = avCall;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the web conversation.
        /// </summary>
        [DataMember]
        public WebConversation WebConversation { get; set; }

        /// <summary>
        /// Gets the av call associated with this object.
        /// </summary>
        public AudioVideoCall AvCall
        {
            get { return m_avCall; }
        }
        #endregion
    }
}
