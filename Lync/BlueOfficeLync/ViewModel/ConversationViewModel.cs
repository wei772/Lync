using GalaSoft.MvvmLight;
using Lync.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Lync.Model.Conversation;

namespace BlueOfficeSkype.ViewModel
{
	public class ConversationViewModel : ObservableObject
	{
		private LyncConversation _skypeConversation;

		public LyncConversation SkypeConversation
		{
			get
			{
				return _skypeConversation;
			}
			set
			{
				Set("SkypeConversation", ref _skypeConversation, value);
			}
		}

		public void OnNavigateTo(object args)
		{
			SkypeConversation = args as LyncConversation;
			SkypeConversation.CreateParticipantModel = CreateParticipantModel;
		}

		private ParticipantItem CreateParticipantModel(Participant arg)
		{
			var part = new ParticipantViewModel();
			part.Participant = arg;
			return part;
		}
	}
}
