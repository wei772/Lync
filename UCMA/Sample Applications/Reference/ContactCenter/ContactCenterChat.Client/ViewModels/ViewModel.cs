/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.ViewModels
{
	
	public abstract class ViewModel: INotifyPropertyChanged
	{
		#region Constructors

		public ViewModel()
		{
			OnInitializeCommands();
		}

		#endregion

		#region Properties

        /// <summary>
        /// Gets or sets the dispatcher to use.
        /// </summary>
        public Dispatcher Dispatcher
        {
            get; set;
        }

		public static Boolean IsDesignTime
		{
			get
			{
				return DesignerProperties.GetIsInDesignMode(Application.Current.RootVisual);
			}
		}

		#endregion

		#region Methods

		protected void NotifyPropertyChanged(string propertyName)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		protected void NotifyPropertyChanged(Enum property)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(property.ToString()));
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
            PropertyChangedEventHandler eventHandler = this.PropertyChanged;
			if (eventHandler != null)
			{
                Dispatcher dispatcher = this.Dispatcher;
                if (dispatcher == null)
                {
                    eventHandler(this, e);
                }
                else
                {
                    Action method = () => eventHandler(this, e);
                    dispatcher.BeginInvoke(method);
                }
			}
		}

		protected virtual void OnInitializeCommands()
		{
		}

		protected virtual void OnDesignTime()
		{
		}

		#endregion

		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}

}