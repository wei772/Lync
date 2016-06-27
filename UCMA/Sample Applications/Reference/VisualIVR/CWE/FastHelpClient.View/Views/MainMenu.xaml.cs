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
    using FastHelpClient.Data;
    using FastHelpClient.Data.Navigation;
    using FastHelpCore;
    using System.Windows.Navigation;

    /// <summary>
    /// Main Menu page
    /// </summary>
    public partial class MainMenu : Page
    {
        /// <summary>
        /// ViewModel for Mainmenu page
        /// </summary>
        private MainMenuViewModel viewModel = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainMenu"/> class.
        /// </summary>
        public MainMenu()
        {
            this.InitializeComponent();

            this.viewModel = Resources["MainMenuViewModel"] as MainMenuViewModel;
            this.viewModel.ErrorLoading += new EventHandler(this.ViewModel_ErrorLoading);
            this.viewModel.LoadComplete += new EventHandler(this.ViewModel_LoadComplete);
            Navigator.SetSource(this, this.viewModel);
            Loaded += new RoutedEventHandler(this.Page_Loaded);
        }

        /// <summary>
        /// Handles the LoadComplete event of the viewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ViewModel_LoadComplete(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles the ErrorLoading event of the viewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ViewModel_ErrorLoading(object sender, EventArgs e)
        {
        }


        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
                
            if (this.NavigationContext.QueryString.ContainsKey("Back"))
            {
                string selectedOption = this.NavigationContext.QueryString["Back"];
                if (!string.IsNullOrEmpty(selectedOption) && selectedOption == "true")
                {
                    this.viewModel.GetTopLevelOptions();
                    this.viewModel.CurrentNavigationLevel = 1;
                }
            }
        }
       
        
        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
           
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
                Console.Write("Something went wrong.Check binding!!");
            }
        }
        
        private void OnSearchClick(object sender, MouseButtonEventArgs e)
        {
            this.viewModel.NavigateToSearchMenu();
        }
    }
}