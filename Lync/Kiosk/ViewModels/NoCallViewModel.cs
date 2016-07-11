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


	

		private string SipUriOfRealPerson = Properties.Settings.Default.sipEmailAddress;



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


		private string _meetUrl;
		public string MeetUrl
		{
			get
			{
				return _meetUrl;
			}
			set
			{
				_meetUrl = value;
				NotifyPropertyChanged("MeetUrl");
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


		private Command _startConversationCommand;

		public ICommand StartConversationCommand
		{
			get
			{
				if (_startConversationCommand == null)
					_startConversationCommand = new Command { Execute = StartConversation };
				return _startConversationCommand;
			}
		}



		private Command _createConversationCommand;

		public ICommand CreateConversationCommand
		{
			get
			{
				if (_createConversationCommand == null)
					_createConversationCommand = new Command { Execute = CreateConversation };
				return _createConversationCommand;
			}
		}



		private Command _joinConversationCommand;

		public ICommand JoinConversationCommand
		{
			get
			{
				if (_joinConversationCommand == null)
					_joinConversationCommand = new Command { Execute = JoinConversation };
				return _joinConversationCommand;
			}
		}

		#endregion


		public NoCallViewModel()
		{

		}


		private void StartConversation(object obj)
		{
			var service = new BlueOfficeSkypeService();
			service.GetSkypeMeeting(StartSkypeMeetingHander);
		}

		private void StartSkypeMeetingHander(bool sus, GetSkypeMeetingResult result)
		{
			var share = new LyncConversation();
			share.ExternalId = result.TalkId;
			ConversationService.Instance.CreateConversationUseExternalUrl(result.Url, share);
			var videoView = new ConversationView();
			videoView.OnNavigateTo(share);
			var window = new Window();
			window.Content = videoView;
			window.Show();
		}


		private void CreateConversation(object obj)
		{
			var share = new LyncConversation();
			share.SipUriOfRealPerson = SipUriOfRealPerson;
			ConversationService.Instance.AddConversation(share);
			var videoView = new ConversationView();
			videoView.OnNavigateTo(share);
			var window = new Window();
			window.Content = videoView;
			window.Show();
		}


		private void JoinConversation(object obj)
		{
			var share = new LyncConversation();

			ConversationService.Instance.CreateConversationUseExternalUrl(MeetUrl, share);
			var videoView = new ConversationView();
			videoView.OnNavigateTo(share);
			var window = new Window();
			window.Content = videoView;
			window.Show();
		}

	}

}
