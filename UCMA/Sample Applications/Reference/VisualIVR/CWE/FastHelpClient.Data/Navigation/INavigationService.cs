/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data.Navigation
{
    /// <summary>
    /// Interface with defines a Navigation Service
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        void Navigate(string url);

        /// <summary>
        /// Backs this instance.
        /// </summary>
        void Back();
    }
}