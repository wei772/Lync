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
    public partial class Search : Page
    {
        /// <summary>
        /// View model for sub menu page
        /// </summary>
        private SearchViewModel viewModel = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuOptions"/> class.
        /// </summary>
        public Search()
        {
            InitializeComponent();
            this.viewModel = Resources["SearchViewModel"] as SearchViewModel;
            Navigator.SetSource(this, this.viewModel);
            Loaded += new RoutedEventHandler(this.Page_Loaded);
            this.KeyUp += new KeyEventHandler(this.OnKeyUp);
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

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the Image control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object sender, MouseButtonEventArgs e)
        {
            this.viewModel.NavigateToMainMenu(true);         
        }

        private void OnSearchClick(object sender, MouseButtonEventArgs e)
        {  
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
                this.viewModel.NavigateToMainMenu(true);     
            }
        }

    }
}
