using BlueOfficeSkype.ViewModel;
using Lync.Model;
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
using Microsoft.Lync.Model.Conversation.Sharing;
using Microsoft.Lync.Model.Conversation.AudioVideo;

namespace BlueOfficeSkype.View
{
	/// <summary>
	/// Interaction logic for VideoConversation.xaml
	/// </summary>
	public partial class ConversationView : UserControl
	{
		public ConversationViewModel ViewModel { get; set; }

		private LyncConversation _lyncConversation;

		public ConversationView()
		{
			InitializeComponent();
		}

		public void OnNavigateTo(object args)
		{
			_lyncConversation = args as LyncConversation;
			_lyncConversation.ApplicationSharingPart.ShowApplicationSharingView = ShowApplicationSharingView;
			_lyncConversation.ApplicationSharingPart.HideApplicationSharingView = HideApplicationSharingView;

			_lyncConversation.VideoAudioPart.ShowVideoPartView = ShowVideoPartView;

			ViewModel = new ConversationViewModel();
			DataContext = ViewModel;
			ViewModel.OnNavigateTo(args);

		}

		private void ShowVideoPartView()
		{
			showVideoPartView.Visibility = Visibility.Visible;
		}

		private void HideApplicationSharingView()
		{
			showSharingDesktopPartView.Visibility = Visibility.Collapsed;
			showSharingDesktopPartView.DataContext = null;
		}

		private void ShowApplicationSharingView(ApplicationSharingView obj)
		{
			showSharingDesktopPartView.Visibility = Visibility.Visible;
			showSharingDesktopPartView.DataContext = ViewModel;
		}
	}
}
