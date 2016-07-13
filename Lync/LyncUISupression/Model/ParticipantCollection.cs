using GalaSoft.MvvmLight;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Lync.Model
{
	public class ParticipantCollection : ObservableObject
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

		private static ParticipantCollection _instance;

		public static ParticipantCollection Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ParticipantCollection();
				}
				return _instance;
			}
		}

		private ParticipantCollection()
		{
			ParticipantItems = new ObservableCollection<ParticipantItem>();
		}

		public void AddItem(ParticipantItem participantItem)
		{
			ParticipantItems.Add(participantItem);
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
