using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lync.Model
{
	public class ConversationFactory
	{
		public static LyncConversation CreateLyncConversation(Conversation conversation)
		{
			if (conversation.Modalities.ContainsKey(ModalityTypes.AudioVideo))
			{
				var avModality = (AVModality)conversation.Modalities[ModalityTypes.AudioVideo];
				var audioChannel = avModality.AudioChannel;
				var videoChannel = avModality.VideoChannel;
				if (videoChannel != null)
				{
					return new VideoConversation();
				}
				else
				{
					return new VideoConversation();
				}
			}

			else if (conversation.Modalities.ContainsKey(ModalityTypes.ContentSharing))
			{
				return new ShareResourceConversation();
			}

			//show the current conversation and modality states in the UI

			return null;
		}
	}
}
