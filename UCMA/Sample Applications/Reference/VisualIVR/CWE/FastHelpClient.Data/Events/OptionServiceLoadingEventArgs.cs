/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data
{
    using System;
    using System.Collections.Generic;
    using FastHelpCore;

    /// <summary>
    /// Event arg to pass the options received to the handler
    /// </summary>
    public class OptionServiceLoadingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionServiceLoadingEventArgs"/> class.
        /// </summary>
        /// <param name="results">The results.</param>
        public OptionServiceLoadingEventArgs(IEnumerable<FastHelpMenuOption> results)
        {                         
            this.Results = results;
        }

        /// <summary>
        /// Gets the results.
        /// </summary>
        public IEnumerable<FastHelpMenuOption> Results { get; private set; }
    }
}