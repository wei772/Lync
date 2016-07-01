using GalaSoft.MvvmLight;
using Lync.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlueOfficeSkype.ViewModel
{
	public class ConversationViewModelBase : ObservableObject
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
	}
}
