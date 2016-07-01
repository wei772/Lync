/* Copyright (C) 2012 Modality Systems - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the Microsoft Public License, a copy of which 
 * can be seen at: http://www.microsoft.com/en-us/openness/licenses.aspx
 * 
 * http://www.LyncAutoAnswer.com
*/

using Microsoft.Lync.Model;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System;
using Lync;
using Lync.Service;
using Lync.Model;
using BlueOfficeSkype.Service;
using Lync.Enum;
using BlueOfficeSkype.View;
using System.Windows;

namespace SuperSimpleLyncKiosk.ViewModels
{
    class NoCallViewModel : ViewModelBase
    {
        #region Fields


        private bool subscribingToInformationUpdates = false;

		private string SipUriOfRealPerson = Properties.Settings.Default.sipEmailAddress;

		private VideoAudioConversation _audioConversation;

        #endregion

        #region Properties

        private string _presence;
        public string Presence
        {
            get
            {
                return _presence;
            }
            set
            {
                _presence = value;
                NotifyPropertyChanged("Presence");
            }
        }

        private string _displayName;
        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                NotifyPropertyChanged("DisplayName");
            }
        }

        private string _activity;
        public string Activity
        {
            get
            {
                return _activity;
            }
            set
            {
                _activity = value;
                NotifyPropertyChanged("Activity");
            }
        }


        private BitmapImage _photo;
        public BitmapImage Photo
        {
            get
            {
                return _photo;
            }
            set
            {
                _photo = value;
                NotifyPropertyChanged("Photo");
            }
        }

		#endregion

		#region Commands


		private Command _startAudioCommand;

		public ICommand StartAudioCommand
		{
            get
            {
                if (this._startAudioCommand == null)
                    this._startAudioCommand = new Command { Execute = StartAudio };
                return this._startAudioCommand;
            }
        }


		private Command _startVideoCommand;

		public ICommand StartVideoCommand
		{
			get
			{
				if (this._startVideoCommand == null)
					this._startVideoCommand = new Command { Execute = StartVideo };
				return this._startVideoCommand;
			}
		}



		private Command _shareResourceCommand;

		public ICommand ShareResourceCommand
		{
			get
			{
				if (this._shareResourceCommand == null)
					this._shareResourceCommand = new Command { Execute = ShareResource };
				return this._shareResourceCommand;
			}
		}


		private Command _sharePPTCommand;

		public ICommand SharePPTCommand
		{
			get
			{
				if (this._sharePPTCommand == null)
					this._sharePPTCommand = new Command { Execute = SharePPT };
				return this._sharePPTCommand;
			}
		}







		#endregion


		public NoCallViewModel()
        {

        }


		private void StartAudio(object obj)
		{

			var service = new BlueOfficeSkypeService();
			service.GetSkypeMeeting(StartAudioSkypeMeetingHander);

			//var audio = new AudioConversation();
			//audio.Init(SipUriOfRealPerson);
			//audio.CreateConversation();

			//var share = new ShareResourceConversation();
			//share.Init(SipUriOfRealPerson);
			//share.CreateConversation();
		}

		private void StartAudioSkypeMeetingHander(bool sus, GetSkypeMeetingResult result)
		{
			//var audio = new AudioConversation();
			//audio.Init(SipUriOfRealPerson);
			//audio.ExternalId = result.TalkId;


			var share = new VideoAudioConversation();
			share.Type = ConversationType.Audio;
			// share.Init(SipUriOfRealPerson);
			share.ExternalId = result.TalkId;

			ConversationService.Instance.CreateConversationUseExternalUrl(result.Url, share);
		}


		private void StartVideo(object obj)
		{
			var service = new BlueOfficeSkypeService();
			service.GetSkypeMeeting(StartVideoSkypeMeetingHander);
		}

		private void StartVideoSkypeMeetingHander(bool sus, GetSkypeMeetingResult result)
		{
			var share = new VideoAudioConversation();
			share.Type = ConversationType.Video;
			// share.Init(SipUriOfRealPerson);
			share.ExternalId = result.TalkId;

			ConversationService.Instance.CreateConversationUseExternalUrl(result.Url, share);

			var videoView = new VideoConversationView();
			videoView.OnNavigateTo(share);
			var window = new Window();
			window.Content = videoView;
			window.Show();
		}

		private void ShareResource(object  obj)
		{
			var service = new BlueOfficeSkypeService();
			service.GetSkypeMeeting(ShareResourceSkypeMeetingHander);
		}

		private void ShareResourceSkypeMeetingHander(bool sus, GetSkypeMeetingResult result)
		{
			var share = new ApplicationSharingConversation();
			share.ExternalId = result.TalkId;

			ConversationService.Instance.CreateConversationUseExternalUrl(result.Url, share);
		}


		private void SharePPT(object obj)
		{
			var service = new BlueOfficeSkypeService();
			service.GetSkypeMeeting(SharePPTSkypeMeetingHander);
		}

		private void SharePPTSkypeMeetingHander(bool sus, GetSkypeMeetingResult result)
		{
			var share = new ContentSharingConversation();
			share.ExternalId = result.TalkId;
			ConversationService.Instance.CreateConversationUseExternalUrl(result.Url, share);
		}
	}




}
