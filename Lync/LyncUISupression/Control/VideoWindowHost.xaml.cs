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
using System;
using System.Windows;
using System.Windows.Controls;

namespace LyncUI.Control
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

        public static DependencyProperty VideoWindowProperty = DependencyProperty.Register("VideoWindowFeed", typeof(LyncVideoWindow), typeof(VideoWindowHost), new PropertyMetadata(OnVideoWindowPropertyChanged));
        public LyncVideoWindow VideoWindowFeed
        {
            get { return (LyncVideoWindow)GetValue(VideoWindowProperty); }
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

        #endregion

        #region Constructor

        public VideoWindowHost()
        {
            InitializeComponent();

            VisualStateManager.GoToElementState(grdControl, Idle.Name, true);

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
                VideoWindowFeed.SetWindowPosition(0, 0, videoPanel.Width, videoPanel.Height);
        }

        #endregion

        #region Property setter callbacks

        private static void OnPlayingPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var thisControl = (VideoWindowHost)sender;
            var isPlaying = (bool)args.NewValue;
            string stateName = GetStateName(thisControl, isPlaying);

            VisualStateManager.GoToElementState(thisControl.grdControl, stateName, true);
        }

        private static void OnVideoWindowPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var thisControl = (VideoWindowHost)sender;
            var videoWindow = (LyncVideoWindow)args.NewValue;

            if (videoWindow != null)
            {
                videoWindow.Owner = thisControl.videoPanel.Handle.ToInt32();
                videoWindow.SetWindowPosition(0, 0, thisControl.videoPanel.Width, thisControl.videoPanel.Height);
            }
        }

        #endregion

        #region Private Methods

        private static string GetStateName(VideoWindowHost window, bool isPlaying)
        {
            if (isPlaying)
            {
                return window.PlayingVideo.Name;
            }
            else
            {
                return window.Idle.Name;
            }
        }

        private void SetVideoControlProperties(VideoAvailabilityChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)delegate()
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
