/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.ComponentModel;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.ViewModels
{
	public enum MessageColor
	{
		Black = 0,
		White,
		Blue,
		Red,
		Green,
		Gray,
	}

	public class MessageViewModel : ViewModel
	{
		#region Constructors

		internal MessageViewModel(ParticipantViewModel participantViewModel, string displayMessage, MessageColor displayColor, DateTime time)
		{
            this.MessageSource = participantViewModel;
			this.DisplayMessage = displayMessage;
			this.DisplayColor = displayColor;
			this.Time = time;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the display name for the message.
		/// </summary>
		public ParticipantViewModel MessageSource { get; private set; }

		/// <summary>
		/// Gets the display message
		/// </summary>
		public String DisplayMessage { get; private set; }

		/// <summary>
		/// Gets the message color.
		/// </summary>
		public MessageColor DisplayColor { get; private set; }

		/// <summary>
		/// Gets the time at which the message posted.
		/// </summary>
		public DateTime? Time { get; private set; }

		#endregion
	}
}