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
    public class TicketsViewModel : BaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuOptionsViewModel"/> class.
        /// </summary>
        public TicketsViewModel() :
            base(OptionService.Instance)
        {

        }       
    }
}
