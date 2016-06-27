/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data
{
    using System;
    using FastHelpCore;

    /// <summary>
    ///  Interface for the Option loading service for the view models.
    /// </summary>
    public interface IOptionService
    {
        /// <summary>
        /// Occurs when options are loaded
        /// </summary>
        event EventHandler<OptionServiceLoadingEventArgs> OptionLoadingComplete;

        /// <summary>
        /// Occurs when an error occurs while loading the options
        /// </summary>
        event EventHandler<OptionServiceErrorEventArgs> OptionLoadingError;

        /// <summary>
        /// Occurs when service has loaded the Option XML
        /// </summary>
        event EventHandler ServiceLoadComplete;


        /// <summary>
        /// Gets the top level options.
        /// </summary>
        void GetTopLevelOptions();

        /// <summary>
        /// Gets the options for level.
        /// </summary>
        /// <param name="levelname">The levelname.</param>
        FastHelpMenuOption GetTopLevelOptionById(string id);


        /// <summary>
        /// Gets the options for level.
        /// </summary>
        /// <param name="levelname">The levelname.</param>
        void GetOptionsForLevel(string levelname);

        /// <summary>
        /// Gets the top level options with levels.
        /// </summary>
        void GetTopLevelOptionsWithLevels();

        bool IsLoaded
        {
            get;
            set;
        }
    }
}
