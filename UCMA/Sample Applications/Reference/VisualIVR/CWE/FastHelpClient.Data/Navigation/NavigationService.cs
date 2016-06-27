/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data.Navigation
{
    using System;

    /// <summary>
    ///   Class for enabling naviagtion between views
    /// </summary>
    public class NavigationService : INavigationService
    {
        /// <summary>
        ///   Instance of Silverlight's naviagtion service 
        /// </summary>
        private readonly System.Windows.Navigation.NavigationService navigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        /// <param name="navService">The navigation service.</param>
        public NavigationService(System.Windows.Navigation.NavigationService navService)
        {
            this.navigationService = navService;
        }

        /// <summary>
        /// Navigates the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void Navigate(string url)
        {
            this.navigationService.Navigate(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Backs this instance.
        /// </summary>
        public void Back()
        {
            if (this.navigationService.CanGoBack)
            {
                this.navigationService.GoBack();
            }
        }
    }  
}
