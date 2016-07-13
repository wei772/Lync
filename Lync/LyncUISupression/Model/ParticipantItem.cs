using GalaSoft.MvvmLight;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using Microsoft.Lync.Model.Conversation.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lync.Model
{
	public class ParticipantItem : ObservableObject
	{
		private ILog _log = LogManager.GetLog(typeof(ParticipantItem));

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

		private bool _isMute;

		public bool IsMute
		{
			get
			{
				return _isMute;
			}
			set
			{
				Set("IsMute", ref _isMute, value);
			}
		}

		private bool _isPresenter;

		public bool IsPresenter
		{
			get
			{
				return _isPresenter;
			}
			set
			{
				Set("IsPresenter", ref _isPresenter, value);
			}
		}


		public AVModality Modality { get; set; }

		public VideoChannel VideoChannel { get; set; }

		public ApplicationSharingModality ApplicationSharingModality { get; set; }

		public int VideoChannelKey { get; set; }


		private Participant _participant;
		public Participant Participant
		{

			get { return _participant; }
			set
			{
				_participant = value;

				IsPresenter = _participant.Properties[ParticipantProperty.IsPresenter] != null ? (bool)_participant.Properties[ParticipantProperty.IsPresenter] : false;
				IsMute = _participant.IsMuted;
				Participant.IsMutedChanged += OnIsMutedChanged;
				Participant.PropertyChanged += OnPropertyChanged;
			}
		}



		public VideoWindow CaptureVideoWindow { get; set; }

		public int CaptureVideoWindowOriginWidth { get; set; }

		public int CaptureVideoWindowOriginHeight { get; set; }


		public VideoWindow RenderVideoWindow { get; set; }

		public int RenderVideoWindowOriginWidth { get; set; }

		public int RenderVideoWindowOriginHeight { get; set; }


		public bool IsMatch(VideoChannel channel)
		{
			return channel.GetHashCode() == VideoChannel.GetHashCode();
		}

		private void OnPropertyChanged(object sender, ParticipantPropertyChangedEventArgs e)
		{
			_log.Debug("OnPropertyChanged  DisplayName:{2}  Type:{0}  Value:{1}", e.Property, e.Value, DisplayName);
			if (e.Property == ParticipantProperty.IsPresenter)
			{
				IsPresenter = (bool)e.Value;
			}
		}

		private void OnIsMutedChanged(object sender, MutedChangedEventArgs e)
		{
			_log.Debug("OnIsMutedChanged DisplayName:{1}  IsMuted:{0}  ", e.IsMuted, DisplayName);
			IsMute = e.IsMuted;
		}

	}
}
