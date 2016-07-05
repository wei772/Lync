using Microsoft.Lync.Model.Conversation.AudioVideo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lync.Repository
{
	public class ConversationRepository
	{
		private List<VideoParticipantItem> _videoParticipantItems;

		private static ConversationRepository _instance;

		public static ConversationRepository Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ConversationRepository();
				}
				return _instance;
			}
		}

		private ConversationRepository()
		{
			_videoParticipantItems = new List<VideoParticipantItem>();
		}

		public void AddVideoParticipantItem(VideoParticipantItem videoParticipantItem)
		{
			_videoParticipantItems.Add(videoParticipantItem);
		}


		public void UpdateVideoWindow(VideoChannel videoChannel, VideoWindow videoWindow, bool isCapture)
		{
			var item = _videoParticipantItems.Where(p => p.IsMatch(videoChannel)).SingleOrDefault();
			if (item != null)
			{
				if (isCapture)
				{
					if (item.CaptureVideoWindow == null)
					{
						item.CaptureVideoWindow = videoWindow;
						item.CaptureVideoWindowOriginHeight = videoWindow.Height;
						item.CaptureVideoWindowOriginWidth = videoWindow.Width;
					}
				}
				else
				{
					if (item.RenderVideoWindow == null)
					{
						item.RenderVideoWindow = videoWindow;
						item.RenderVideoWindowOriginHeight = videoWindow.Height;
						item.RenderVideoWindowOriginWidth = videoWindow.Width;
					}
				}
			}
		}

		public void Clear()
		{
			_videoParticipantItems.Clear();
		}


	}
}
