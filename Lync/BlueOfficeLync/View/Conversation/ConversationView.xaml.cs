using BlueOfficeSkype.ViewModel;
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

namespace BlueOfficeSkype.View
{
	/// <summary>
	/// Interaction logic for VideoConversation.xaml
	/// </summary>
	public partial class ConversationView : UserControl
	{
		public ConversationViewModel ViewModel { get; set; }


		public ConversationView()
		{
			InitializeComponent();
		}

		public void OnNavigateTo(object args)
		{
			ViewModel = new ConversationViewModel();
			DataContext = ViewModel;
			ViewModel.OnNavigateTo(args);
		}

	}
}
