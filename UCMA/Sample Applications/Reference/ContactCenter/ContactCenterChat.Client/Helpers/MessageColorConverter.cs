/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.ViewModels;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.Helpers
{
	public class MessageColorConverter : IValueConverter
	{

		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is MessageColor)
			{
				switch ((MessageColor)value)
				{
					case MessageColor.White:
						return Application.Current.Resources["WhiteMessageBrush"];
					case MessageColor.Blue:
						return Application.Current.Resources["BlueMessageBrush"];
					case MessageColor.Red:
						return Application.Current.Resources["RedMessageBrush"];
					case MessageColor.Green:
						return Application.Current.Resources["GreenMessageBrush"];
					case MessageColor.Gray:
						return Application.Current.Resources["GrayMessageBrush"];
					case MessageColor.Black:
					default:
						return Application.Current.Resources["BlackMessageBrush"];
				}
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
