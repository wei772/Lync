using Lync.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlueOfficeSkype.ViewModel
{
	public class VideoConversationViewModel: ConversationViewModelBase
	{
		public void OnNavigateTo(object args)
		{
			SkypeConversation = args as VideoAudioConversation;
		}
	}
}
