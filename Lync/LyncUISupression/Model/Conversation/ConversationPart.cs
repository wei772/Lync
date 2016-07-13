using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
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

		internal ParticipantCollection ParticipantCollection { get; set; }


		protected Action<Action> RunAtUI = DispatcherHelper.CheckBeginInvokeOnUI;


		internal virtual void CloseInternal()
		{

		}

		internal virtual void HandleAddedInternal()
		{

		}

		internal virtual void ConversationParticipantAddedInternal(ParticipantItem participant)
		{

		}

		internal virtual void ConversationParticipantRemovedInternal(ParticipantItem participant)
		{

		}


	}
}
