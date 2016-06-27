/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClientView.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using FastHelpClient.Data;
    using FastHelpClient.Data.Navigation;
    using FastHelpClient.View.Utils;
    using FastHelpCore;
    using System.Windows.Navigation;
    using System.Globalization;

    /// <summary>
    /// Sub Menu Page
    /// </summary>
    public partial class MenuOptions : Page
    {
        /// <summary>
        /// View model for sub menu page
        /// </summary>
        private MenuOptionsViewModel viewModel = null;

      
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuOptions"/> class.
        /// </summary>
        public MenuOptions()
        {
            InitializeComponent();
            this.viewModel = Resources["MenuOptionsViewModel"] as MenuOptionsViewModel;
            this.viewModel.ErrorLoading += new EventHandler(this.ViewModel_ErrorLoading);
            this.viewModel.LoadComplete += new EventHandler(this.ViewModel_LoadComplete);
            Navigator.SetSource(this, this.viewModel);
            Loaded += new RoutedEventHandler(this.Page_Loaded);
            this.KeyUp += new KeyEventHandler(this.OnKeyUp);
        }

        /// <summary>
        /// Handles the LoadComplete event of the ViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ViewModel_LoadComplete(object sender, EventArgs e)
        {
            Console.Write("Options load here");

            string selectedOptionId = this.NavigationContext.QueryString["Id"].Trim();

            // GET top option name by id;
            var option = this.viewModel.GetTopLevelOptionById(selectedOptionId);

           

            menuName.Text = option.GraphicalText.ToLower(CultureInfo.InvariantCulture);

            string selectedOption = option.Name;

            string hostName = Application.Current.Host.Source.AbsoluteUri;
            int index = hostName.IndexOf("Clientbin", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                index = hostName.IndexOf("bin/Debug", StringComparison.OrdinalIgnoreCase);
            }

            hostName = hostName.Substring(0, index);
            selectedOption = selectedOption.ToLower().Replace(' ', '_');
            string menuImagePath = string.Format(CultureInfo.CurrentCulture,
                "{0}{1}/{2}/{3}.png", hostName, "Images", selectedOption, selectedOption);
            Uri uri = new Uri(menuImagePath, UriKind.RelativeOrAbsolute);
            menuImage.Source = new BitmapImage(uri);
        }

        /// <summary>
        /// Handles the ErrorLoading event of the ViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ViewModel_ErrorLoading(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.NavigationContext.QueryString.ContainsKey("Option"))
            {
                string selectedOptionId = this.NavigationContext.QueryString["Id"].Trim();
                this.viewModel.GetOptionsForLevel(selectedOptionId);
                
            }
           
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
          // Retrieve element binding / datacontext            
            FastHelpMenuOption selectedOption = (sender as FrameworkElement).DataContext as FastHelpMenuOption;
            if (selectedOption != null && !string.IsNullOrEmpty(selectedOption.Name.Trim()))
            {
                this.viewModel.SendIM(selectedOption.Name.Trim());
            }
            else 
            {
                Console.Write("Something went wrong.Check binding");
            }
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the Image control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object sender, MouseButtonEventArgs e)
        {
            this.viewModel.CurrentNavigationLevel = 1;
            this.viewModel.NavigateToMainMenu(true);
        }

        /// <summary>
        /// Called when [key up].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/>
        /// instance containing the event data.</param>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            // For backspace or left arrow key go back 
            if (e.Key == Key.Left || e.Key == Key.Back) 
            {
                this.viewModel.CurrentNavigationLevel = 1;
                this.viewModel.NavigateToMainMenu(true);
            }
        }

        private void OnSearchClick(object sender, MouseButtonEventArgs e)
        {
            this.viewModel.NavigateToSearchMenu();
        }
    }
}
