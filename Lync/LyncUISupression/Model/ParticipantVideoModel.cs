using GalaSoft.MvvmLight;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lync.Model
{
	public class ParticipantVideoModel : ObservableObject
	{
		public string Id { get; set; }

		private string _displayName;

		public string DisplayName
		{
			set
			{
				Set("DisplayName", ref _displayName, value);
			}
			get
			{
				return _displayName;
			}
		}


		private bool _isPined;

		public bool IsPined
		{
			set
			{
				Set("IsPined", ref _isPined, value);
			}
			get
			{
				return _isPined;
			}
		}


		private VideoWindow _view;

		public VideoWindow View
		{
			set
			{
				Set("View", ref _view, value);
			}
			get
			{
				return _view;
			}
		}

		public AVModality Modality { get; set; }

		public VideoChannel VideoChannel { get; set; }

		public int VideoChannelKey { get; set; }
	}
}
