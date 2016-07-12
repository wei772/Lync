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
using System.Windows.Threading;

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


		public static DependencyProperty VideoOriginWidthProperty = DependencyProperty.Register("VideoOriginWidth", typeof(int), typeof(VideoWindowHost), new PropertyMetadata());

		public int VideoOriginWidth
		{
			get { return (int)GetValue(VideoOriginWidthProperty); }
			set
			{
				SetValue(VideoOriginWidthProperty, value);
			}
		}



		public static DependencyProperty VideoOriginHeightProperty = DependencyProperty.Register("VideoOriginHeight", typeof(int), typeof(VideoWindowHost), new PropertyMetadata());

		public int VideoOriginHeight
		{
			get { return (int)GetValue(VideoOriginHeightProperty); }
			set
			{
				SetValue(VideoOriginHeightProperty, value);
			}
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

		private long _windowStyle;


		#endregion



		public VideoWindowHost()
		{
			InitializeComponent();
			videoPanel.Layout += OnVideoPanelLayout;


		}




		//#region Event callbacks

		//void model_VideoAvailabilityChanged(object sender, VideoAvailabilityChangedEventArgs e)
		//{
		//	SetVideoControlProperties(e);
		//}

		//#endregion

		#region UI Events

		private void OnVideoPanelLayout(object sender, System.Windows.Forms.LayoutEventArgs e)
		{
			try
			{
				if (VideoWindowFeed != null)
				{
					SetWindowPosition(VideoWindowFeed);
				}
			}
			catch (Exception exception)
			{
				_log.ErrorException("OnVideoPanelLayout", exception);
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
			var oldVideo = (VideoWindow)args.OldValue;
			if (oldVideo != null)
			{
				thisControl.videoPanel.Controls.Clear();
			}

			if (videoWindow != null)
			{
				thisControl.ShowVideo(thisControl.videoPanel, videoWindow);
			}

		}


		private void SetWindowPosition(VideoWindow videoWindow)
		{
			var point = new Point(0, 0);
			var size = new Size(VideoWidth, VideoHeight);

			if (videoWindow.Height != VideoHeight)
			{
				VideoOriginHeight = videoWindow.Height;
			}

			if (videoWindow.Width != VideoWidth)
			{
				VideoOriginWidth = videoWindow.Width;
			}

			videoWindow.SetWindowPosition((int)point.X, (int)point.Y, (int)size.Width, (int)size.Height);

			if (VideoOriginHeight == 0 || VideoOriginWidth == 0)
			{
			}
			else
			{
				var actualHeight = (int)Math.Floor(new decimal((VideoOriginHeight * VideoWidth / VideoOriginWidth))) + 12;
				size.Height = actualHeight;
				point.Y = (VideoHeight - actualHeight) / 2;
			}
			//videoWindow.SetWindowPosition((int)point.X, (int)point.Y, (int)size.Width, (int)size.Height);

		}



		private void ShowVideo(System.Windows.Forms.Panel videoPanel, VideoWindow videoWindow)
		{





			//Win32 constants:                  WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS|	WS_VISIBLE;
			const long lEnableWindowStyles = 0x40000000L | 0x02000000L | 0x04000000L | 0x10000000L;
			//Win32 constants:                   WS_POPUP| WS_CAPTION | WS_SIZEBOX
			const long lDisableWindowStyles = 0x80000000 | 0x00C00000 | 0x00040000L;
			const int OATRUE = -1;

			//gets the current window style to modify it
			if (_windowStyle == 0)
			{
				long currentStyle = videoWindow.WindowStyle;
				//disables borders, sizebox, close button
				currentStyle = currentStyle & ~lDisableWindowStyles;

				//enables styles for a child window
				currentStyle = currentStyle | lEnableWindowStyles;
				_windowStyle = currentStyle;
			}

			_log.Debug("ShowVideo  videoPanel:{0}  windowStyle：{1}", videoPanel.Handle.ToInt32(), _windowStyle);


			try
			{
				videoWindow.Owner = videoPanel.Handle.ToInt32();

				SetWindowPosition(videoWindow);

				//WindowStyle的文章太多，出错就是在这里，默认0不错。
				//videoWindow.WindowStyle = 0;
				videoWindow.WindowStyle = (int)_windowStyle;

				videoWindow.Visible = OATRUE;


			}
			catch (Exception exception)
			{
				_log.ErrorException("ShowVideo", exception);
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
