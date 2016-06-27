/*=====================================================================
  File:      InstantMessagingCallContext.cs
 
  Summary:   Represents the internal context to be maintained for an instant messaging call.
 

/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Collections.Generic;

using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities
{

    /// <summary>
    /// This class encapsulates internal context information to be stored on an instant messaging call.
    /// </summary>
    internal class InstantMessagingCallContext
    {

        #region private variables

        /// <summary>
        /// Web imcall object reference.
        /// </summary>
        private readonly WebImCall m_webImCall;
        #endregion

        #region constructors

        /// <summary>
        /// Constructor to create instant messaging call context for a given web imcall.
        /// </summary>
        /// <param name="webImCall">Web imcall.</param>
        internal InstantMessagingCallContext(WebImCall webImCall)
        {
            m_webImCall = webImCall;
        }

        #endregion

        #region internal properties
        /// <summary>
        /// Gets the web imcall.
        /// </summary>
        internal WebImCall WebImcall
        {
            get { return m_webImCall; }
        }

        #endregion
    }
}