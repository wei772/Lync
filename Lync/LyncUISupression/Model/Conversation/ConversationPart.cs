using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using Lync.Repository;
using Lync.Service;
using Microsoft.Lync.Model.Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lync.Model
{
	public class ConversationPart : ObservableObject
	{
		internal LyncConversation LyncConversation { get; set; }

		internal Conversation Conversation { get; set; }

		internal ContactService ContactService { get; set; }

		internal ConversationRepository Repository { get; set; }


		protected Action<Action> RunAtUI = DispatcherHelper.CheckBeginInvokeOnUI;


		internal virtual void CloseInternal()
		{

		}

		internal virtual void HandleAddedInternal()
		{

		}

		internal virtual void ConversationParticipantAddedInternal(Participant participant)
		{

		}

		internal virtual void ConversationParticipantRemovedInternal(Participant participant)
		{

		}


	}
}
