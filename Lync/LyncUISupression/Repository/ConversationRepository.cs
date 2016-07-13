using GalaSoft.MvvmLight;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Lync.Repository
{
	public class ConversationRepository : ObservableObject
	{
		private ObservableCollection<ParticipantItem> _participantItems;

		public ObservableCollection<ParticipantItem> ParticipantItems
		{
			get
			{
				return _participantItems;
			}
			set
			{
				Set("ParticipantItems", ref _participantItems, value);
			}
		}

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
			ParticipantItems = new ObservableCollection<ParticipantItem>();
		}

		public void AddParticipantItem(ParticipantItem videoParticipantItem)
		{
			ParticipantItems.Add(videoParticipantItem);
		}


		public void UpdateVideoWindow(VideoChannel videoChannel, VideoWindow videoWindow, bool isCapture)
		{
			var item = ParticipantItems.Where(p => p.IsMatch(videoChannel)).SingleOrDefault();
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
			ParticipantItems.Clear();
		}

		internal ParticipantItem Remove(string uri)
		{
			var removeItem = ParticipantItems.Where(p => p.Id == uri).SingleOrDefault();
			if (removeItem == null)
			{
				return null;
			}
			ParticipantItems.Remove(removeItem);

			return removeItem;

		}

		internal ParticipantItem GetItem(VideoChannel channel)
		{
			var item = ParticipantItems.Where(p => p.IsMatch(channel)).SingleOrDefault();
			if (item == null)
			{
				return null;
			}
			return item;
		}
	}
}
