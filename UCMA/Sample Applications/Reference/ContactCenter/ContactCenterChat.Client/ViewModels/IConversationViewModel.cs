/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.ViewModels;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.ViewModels
{
	interface IConversationViewModel
	{
        /// <summary>
        /// Local participant.
        /// </summary>
        ParticipantViewModel LocalParticipant { get; }

		/// <summary>
		/// A status message.
		/// </summary>
		string Status { get; set; }

		/// <summary>
		/// The message that is currently being entered/edited by the user.
		/// </summary>
		string WorkingMessage { get; set; }

		/// <summary>
		/// All of the messages in the conversation.
		/// </summary>
		ObservableCollection<MessageViewModel> Messages { get; }

        /// <summary>
        /// Bool to denote if send message command is enabled.
        /// </summary>
        bool IsSendMessageCommandEnabled { get; set; }

        /// <summary>
        /// Is call me command enabled.
        /// </summary>
        bool IsCallMeCommandEnabled { get; set; }

        /// <summary>
        /// Is terminate conversation command enabled.
        /// </summary>
        bool IsTerminateConversationCommandEnabled { get; set; }

		/// <summary>
		/// Sends a message.
		/// </summary>
		ICommand SendMessageCommand { get; }

		/// <summary>
		/// Send a call request.
		/// </summary>
		ICommand CallMeCommand { get; }

        /// <summary>
        /// Terminate conversation command.
        /// </summary>
        ICommand TerminateConversationCommand { get; }

		
	}
}
