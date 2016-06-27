/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data
{
    using System;
    using System.Windows;
    using FastHelpClient.Data.ViewModels;
    using FastHelpCore;
    using System.Globalization;
    using FastHelpClient.Data.Events;

    /// <summary>
    /// Viewmodel for Submenu page
    /// </summary>
    public class SearchViewModel : BaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuOptionsViewModel"/> class.
        /// </summary>
        public SearchViewModel() :
            base(OptionService.Instance)
        {

        }      
    }       
}
