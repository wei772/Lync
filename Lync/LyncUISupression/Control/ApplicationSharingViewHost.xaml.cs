using Microsoft.Lync.Model.Conversation.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lync.Control
{
	/// <summary>
	/// Interaction logic for ApplicationSharingViewHost.xaml
	/// </summary>
	public partial class ApplicationSharingViewHost : UserControl
	{
		private static ILog _log = LogManager.GetLog(typeof(ApplicationSharingViewHost));



		public static DependencyProperty ViewProperty = DependencyProperty.Register("View", typeof(ApplicationSharingView), typeof(ApplicationSharingViewHost), new PropertyMetadata(OnViewPropertyChanged));
		public ApplicationSharingView View
		{
			get { return (ApplicationSharingView)GetValue(ViewProperty); }
			set { SetValue(ViewProperty, value); }
		}



		public ApplicationSharingViewHost()
		{
			InitializeComponent();
		}



		#region UI Events

		//private void OnVideoPanelLayout(object sender, System.Windows.Forms.LayoutEventArgs e)
		//{
		//	if (VideoWindowFeed != null)
		//	{
		//		VideoWindowFeed.SetWindowPosition(0, 0, VideoWidth, VideoHeight);
		//	}
		//}

		#endregion

		#region Property setter callbacks



		private static void OnViewPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			var thisControl = (ApplicationSharingViewHost)sender;
			var view = (ApplicationSharingView)args.NewValue;

			if (view != null)
			{
				ShowApplicationSharingView(thisControl.applicaionViewPanel, view);
			}
		}


		private static void ShowApplicationSharingView(System.Windows.Forms.Panel applicaionViewPanel, ApplicationSharingView view)
		{

			_log.Debug("ShowApplicationSharingView  applicaionViewPanel:{0}", applicaionViewPanel.Handle.ToInt32());

			try
			{
				//sets the properties required for the native video window to draw itself
				view.SetParent(applicaionViewPanel.Handle.ToInt32());
			}
			catch (Exception exception)
			{
				_log.ErrorException("", exception);
			}
		}


		#endregion


	}
}
