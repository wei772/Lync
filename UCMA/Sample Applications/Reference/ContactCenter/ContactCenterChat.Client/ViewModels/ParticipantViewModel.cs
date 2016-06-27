/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.ComponentModel;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.ViewModels
{
  
    /// <summary>
    /// Represents participant view model.
    /// </summary>
    public partial class ParticipantViewModel : ViewModel
    {
        #region Constructors

        /// <summary>
        /// Internal constructor to create a new participant.
        /// </summary>
        /// <param name="displayName">Diplay name. Cannot be null or empty.</param>
        /// <param name="phoneNumber">Phone number. Can be null or empty.</param>
        /// <param name="displayNameColor">Display name color.</param>
        internal ParticipantViewModel(string displayName, string phoneNumber, MessageColor displayNameColor)
        {
            this.DisplayName = displayName;
            if (!String.IsNullOrEmpty(this.DisplayName))
            {
                this.DisplayName += ": ";
            }
            this.DisplayNameColor = displayNameColor;
            this.PhoneNumber = phoneNumber;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="copy">Copy.</param>
        internal ParticipantViewModel(ParticipantViewModel copy)
        {
            this.DisplayName = copy.DisplayName;
            this.DisplayNameColor = copy.DisplayNameColor;
            this.PhoneNumber = copy.PhoneNumber;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the display name for the message.
        /// </summary>
        public String DisplayName { get; internal set; }

        /// <summary>
        /// Gets the phone number
        /// </summary>
        public String PhoneNumber { get; private set; }

        /// <summary>
        /// Gets the display name color.
        /// </summary>
        public MessageColor DisplayNameColor { get; private set; }
        #endregion

        #region events

        #endregion
    }
}