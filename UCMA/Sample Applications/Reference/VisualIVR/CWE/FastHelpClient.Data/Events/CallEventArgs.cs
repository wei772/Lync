/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data.Events
{
    using System;

    /// <summary>
    ///  Event args for passing menu name to the caller 
    /// </summary>
    public class CallEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallEventArgs"/> class.
        /// </summary>
        /// <param name="menuName">Name of the menu.</param>
        public CallEventArgs(string menuName)
        {
            this.MenuName = menuName;
        }

        /// <summary>
        /// Gets or sets the name of the menu.
        /// </summary>
        /// <value>
        /// The name of the menu.
        /// </value>
        public string MenuName { get; set; }
    }
}
