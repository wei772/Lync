/*=====================================================================
  File:      AudioVideoCallContext.cs
 
  Summary:   Represents the internal context to be maintained for an audio video call.
 

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
    /// This class encapsulates internal context information to be stored on an audio video call.
    /// </summary>
    internal class AudioVideoCallContext
    {

        #region private variables

        /// <summary>
        /// Web avcall object reference.
        /// </summary>
        private readonly WebAvCall m_webAvCall;
        #endregion


        #region constructors

        /// <summary>
        /// Constructor to create audio video call context for a given web avcall.
        /// </summary>
        /// <param name="webAvCall">Web avcall.</param>
        internal AudioVideoCallContext(WebAvCall webAvCall)
        {
            m_webAvCall = webAvCall;
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets the web avcall.
        /// </summary>
        internal WebAvCall WebAvcall
        {
            get { return m_webAvCall; }
        }

        #endregion
    }
}
