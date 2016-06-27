/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data.Navigation
{
    /// <summary>
    /// Interface to be implmented by navigable content
    /// </summary>
    public interface INavigable
    {
        /// <summary>
        /// Gets or sets the navigation service.
        /// </summary>
        /// <value>
        /// The navigation service.
        /// </value>
        INavigationService NavigationService { get; set; }
    }
}
