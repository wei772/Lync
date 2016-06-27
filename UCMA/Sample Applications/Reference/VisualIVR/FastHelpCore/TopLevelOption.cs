/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpCore
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;


    /// <summary>
    ///  Represents Top level menu
    /// </summary>
    public class TopLevelOption : FastHelpMenuOption
    {
        /// <summary>
        /// Options associated with a top level menu
        /// </summary>
        private Collection<FastHelpMenuOption> options= null;

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public Collection<FastHelpMenuOption> Options 
        {
            get 
            { 
                return this.options; 
            }
        }
    }
}
