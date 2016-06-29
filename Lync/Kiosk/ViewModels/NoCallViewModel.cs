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
using BlueOfficeLync.Service;

namespace SuperSimpleLyncKiosk.ViewModels
{
	class NoCallViewModel : ViewModelBase
	{
		#region Fields


		private bool subscribingToInformationUpdates = false;
		private string SipUriOfRealPerson = Properties.Settings.Default.sipEmailAddress;
		private Command _placeCallCommand;

		private AudioConversation _audioConversation;

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

		public ICommand PlaceCallCommand
		{
			get
			{
				if (this._placeCallCommand == null)
					this._placeCallCommand = new Command { Execute = ExecutePlaceCall };
				return this._placeCallCommand;
			}
		}

		private void ExecutePlaceCall(object obj)
		{
            var service = new BlueOfficeLyncService();
            service.GetSkypeMeeting(null);
            //var audio = new AudioConversation();
            //audio.Start(SipUriOfRealPerson);
        }

		#endregion


		public NoCallViewModel()
		{
		
		}

	}




}
