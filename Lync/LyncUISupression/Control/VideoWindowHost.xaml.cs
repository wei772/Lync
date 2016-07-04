/* Copyright (C) 2012 Modality Systems - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the Microsoft Public License, a copy of which 
 * can be seen at: http://www.microsoft.com/en-us/openness/licenses.aspx
 * 
 * http://www.LyncAutoAnswer.com
*/

using Lync.Enum;
using Lync.EventArg;
using Lync.Model;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lync.Control
{
	public partial class VideoWindowHost : UserControl
	{

		private static ILog _log = LogManager.GetLog(typeof(VideoWindowHost));

		#region Properties

		public static DependencyProperty PlayingProperty = DependencyProperty.Register("Playing", typeof(bool), typeof(VideoWindowHost), new PropertyMetadata(OnPlayingPropertyChanged));
		public bool Playing
		{
			get { return (bool)GetValue(PlayingProperty); }
			set { SetValue(PlayingProperty, value); }
		}

		public static DependencyProperty VideoWindowProperty = DependencyProperty.Register("VideoWindowFeed", typeof(VideoWindow), typeof(VideoWindowHost), new PropertyMetadata(OnVideoWindowPropertyChanged));
		public VideoWindow VideoWindowFeed
		{
			get { return (VideoWindow)GetValue(VideoWindowProperty); }
			set { SetValue(VideoWindowProperty, value); }
		}

		public static DependencyProperty DirectionProperty = DependencyProperty.Register("Direction", typeof(VideoDirection), typeof(VideoWindowHost), new PropertyMetadata());
		public VideoDirection Direction
		{
			get { return (VideoDirection)GetValue(DirectionProperty); }
			set
			{
				SetValue(DirectionProperty, value);
			}
		}


		public static DependencyProperty VideoHeightProperty = DependencyProperty.Register("VideoHeight", typeof(int), typeof(VideoWindowHost), new PropertyMetadata());
		public int VideoHeight
		{
			get { return (int)GetValue(VideoHeightProperty); }
			set
			{
				SetValue(VideoHeightProperty, value);
			}
		}


		public static DependencyProperty VideoWidthProperty = DependencyProperty.Register("VideoWidth", typeof(int), typeof(VideoWindowHost), new PropertyMetadata());
		public int VideoWidth
		{
			get { return (int)GetValue(VideoWidthProperty); }
			set
			{
				SetValue(VideoWidthProperty, value);
			}
		}

		#endregion

		#region Constructor

		public VideoWindowHost()
		{
			InitializeComponent();
			videoPanel.Layout += OnVideoPanelLayout;

		}

		#endregion

		//#region Event callbacks

		//void model_VideoAvailabilityChanged(object sender, VideoAvailabilityChangedEventArgs e)
		//{
		//	SetVideoControlProperties(e);
		//}

		//#endregion

		#region UI Events

		private void OnVideoPanelLayout(object sender, System.Windows.Forms.LayoutEventArgs e)
		{
			if (VideoWindowFeed != null)
			{
				VideoWindowFeed.SetWindowPosition(0, 0, VideoWidth, VideoHeight);
			}
		}

		#endregion

		#region Property setter callbacks

		private static void OnPlayingPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			var thisControl = (VideoWindowHost)sender;
			var isPlaying = (bool)args.NewValue;

		}

		private static void OnVideoWindowPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			var thisControl = (VideoWindowHost)sender;
			var videoWindow = (VideoWindow)args.NewValue;

			


			if (videoWindow != null)
			{
				ShowVideo(thisControl.videoPanel, videoWindow, thisControl.VideoWidth, thisControl.VideoHeight);
			}
		}


		private static void ShowVideo(System.Windows.Forms.Panel videoPanel, VideoWindow videoWindow, int videoWidth, int videoHeight)
		{

			_log.Debug("ShowVideo  videoPanel:{0}", videoPanel.Handle.ToInt32());

			//Win32 constants:                  WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS;
			const long lEnableWindowStyles = 0x40000000L | 0x02000000L | 0x04000000L;
			//Win32 constants:                   WS_POPUP| WS_CAPTION | WS_SIZEBOX
			const long lDisableWindowStyles = 0x80000000 | 0x00C00000 | 0x00040000L;
			const int OATRUE = -1;

			try
			{
				videoPanel.Width = videoWidth;
				videoPanel.Height = videoHeight;
				videoWindow.Width = videoWidth;
				videoWindow.Height = videoHeight;

				//sets the properties required for the native video window to draw itself
				videoWindow.Owner = videoPanel.Handle.ToInt32();
				videoWindow.SetWindowPosition(0, 0, videoWidth, videoHeight);
				//videoWindow.SetWindowForeground(new SolidColorBrush());

				//gets the current window style to modify it
				long currentStyle = videoWindow.WindowStyle;

				//disables borders, sizebox, close button
				currentStyle = currentStyle & ~lDisableWindowStyles;

				//enables styles for a child window
				currentStyle = currentStyle | lEnableWindowStyles;

				//updates the current window style
				videoWindow.WindowStyle = (int)currentStyle;

				//updates the visibility
				videoWindow.Visible = OATRUE;
			}
			catch (Exception exception)
			{
				_log.ErrorException("",exception);
			}
		}


		#endregion

		#region Private Methods



		private void SetVideoControlProperties(VideoAvailabilityChangedEventArgs e)
		{
			Dispatcher.Invoke((Action)delegate ()
			{
				if (e.Direction == Direction)
				{
					Playing = e.IsAvailable;
					VideoWindowFeed = e.IsAvailable ? e.VideoWindow : null;
				}
			}
			);
		}



		#endregion
	}
}
