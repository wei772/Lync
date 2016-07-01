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

namespace Lync.Control
{
	public partial class VideoWindowHost : UserControl
	{

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


		}

		#endregion

		#region Event callbacks

		void model_VideoAvailabilityChanged(object sender, VideoAvailabilityChangedEventArgs e)
		{
			SetVideoControlProperties(e);
		}

		#endregion

		#region UI Events

		private void videoPanel_Layout(object sender, System.Windows.Forms.LayoutEventArgs e)
		{
			if (VideoWindowFeed != null && Playing)
				VideoWindowFeed.SetWindowPosition(0, 0, VideoWidth, VideoHeight);
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
				videoWindow.Owner = thisControl.videoPanel.Handle.ToInt32();
				thisControl.videoPanel.Width = thisControl.VideoWidth;
				thisControl.videoPanel.Height = thisControl.VideoHeight;
				videoWindow.SetWindowPosition(0, 0, thisControl.VideoWidth, thisControl.VideoHeight);
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
