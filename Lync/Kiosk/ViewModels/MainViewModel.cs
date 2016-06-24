/* Copyright (C) 2012 Modality Systems - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the Microsoft Public License, a copy of which 
 * can be seen at: http://www.microsoft.com/en-us/openness/licenses.aspx
 * 
 * http://www.LyncAutoAnswer.com
*/

using Lync.Service;
using System;

namespace SuperSimpleLyncKiosk.ViewModels
{
	class MainViewModel : ViewModelBase
	{
		private LyncService _lyncService = LyncService.Instance;

		private string _currentVisualState;

		public string CurrentVisualState
		{
			get { return _currentVisualState; }
			private set
			{
				_currentVisualState = value;
				NotifyPropertyChanged("CurrentVisualState");
			}
		}

		public MainViewModel()
		{
			_lyncService.ClientStateChanged += OnLyncServiceClientStateChanged;

			_lyncService.Connect(
				Properties.Settings.Default.LyncAccountEmail
				, Properties.Settings.Default.LyncAccountPassword
				,false);

			ConversationService.Instance.InitializeConversationEvent();
		}

		private void   OnLyncServiceClientStateChanged(string newState)
		{
			if (newState == "SignedIn")
			{
				CurrentVisualState = "NoCall";
			}
			else  if(newState== "SigningIn")
			{
				CurrentVisualState = "SigningIn";
			}

		}

	}

}
