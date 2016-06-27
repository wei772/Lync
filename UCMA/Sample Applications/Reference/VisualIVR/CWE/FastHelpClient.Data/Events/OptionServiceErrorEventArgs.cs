/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data
{
    using System;

    /// <summary>
    ///  Event arg to pass the exception occured to the handler
    /// </summary>
    public class OptionServiceErrorEventArgs : EventArgs
    {
        /// <summary>
        ///  Exception associated with the Event
        /// </summary>
        private Exception theException = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionServiceErrorEventArgs"/> class.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public OptionServiceErrorEventArgs(Exception ex)
        {
            this.theException = ex;
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public Exception Error
        {
            get { return this.theException; }
        }
    }
}
