
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
/******************************************************************************
Module:  AsyncResult.cs
Notices: Written by Jeffrey Richter
******************************************************************************/

using System;
using System.Diagnostics;


///////////////////////////////////////////////////////////////////////////////

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common
{

    internal class AsyncResult<TResult> : AsyncResultNoResult
    {
        // Field set when operation completes
        private TResult m_result = default(TResult);

        public AsyncResult(AsyncCallback asyncCallback, Object state) : base(asyncCallback, state) { }

        public void SetAsCompleted(TResult result, Boolean completedSynchronously)
        {
            // Save the asynchronous operation's result
            m_result = result;

            // Tell the base class that the operation completed sucessfully (no exception)
            base.SetAsCompleted(null, completedSynchronously);
        }

        new public TResult EndInvoke()
        {
            base.EndInvoke(); // Wait until operation has completed 
            return m_result;  // Return the result (if above didn't throw)
        }
    }

    /// <summary>
    /// Internal async result which exposes a process method to start processing.
    /// </summary>
    /// <typeparam name="TResult">Result type.</typeparam>
    internal class AsyncResultWithProcess<TResult> : AsyncResult<TResult>
    {

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="asyncCallback">Async callback.</param>
        /// <param name="state">State.</param>
        public AsyncResultWithProcess(AsyncCallback asyncCallback, Object state) : base(asyncCallback, state) { }

        /// <summary>
        /// Process method to start processing.
        /// </summary>
        public virtual void Process()
        {
            //By default the process method will do nothing. Derived classes can override this behavior.
        }

        /// <summary>
        /// Complete with a result.
        /// </summary>
        /// <param name="result">Result. Cannot be null.</param>
        public void Complete(TResult result)
        {
            Debug.Assert(result != null, "Result is null");
            this.SetAsCompleted(result, false /*completedSynchronously*/);
        }

        /// <summary>
        /// Complete with an exception.
        /// </summary>
        /// <param name="exception">Exception. Cannot be null.</param>
        public void Complete(Exception exception)
        {
            Debug.Assert(exception != null, "Exception is null");
            this.SetAsCompleted(exception, false /*completedSynchronously*/);
        }
    }
    
}
//////////////////////////////// End of File //////////////////////////////////
