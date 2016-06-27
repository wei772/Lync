/*=====================================================================
  File:      AsyncResultNoResult.cs

  Summary:   Asynchronous Programming Model that handles asynchronous
             operations.

---------------------------------------------------------------------
This file is part of the Microsoft Lync SDK Code Samples

  Copyright (C) 2010 Microsoft Corporation.  All rights reserved.

This source code is intended only as a supplement to Microsoft
Development Tools and/or on-line documentation.  See these other
materials for detailed information regarding Microsoft code samples.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
=====================================================================*/
using System;
using System.Threading;

namespace Microsoft.Lync.Samples.ContactCenterExtension.Models
{
    internal class AsyncResultNoResult : IAsyncResult
    {
        #region Fields

        private readonly AsyncCallback _asyncCallback;
        private readonly Object _asyncState;
        private Int32 _completedState;
        private ManualResetEvent _asyncWaitHandle;
        private Exception _exception;

        private const Int32 StatePending = 0;
        private const Int32 StateCompletedSynchronously = 1;
        private const Int32 StateCompletedAsynchronously = 2;

        #endregion

        #region Public Properties

        /// <summary>
        /// Exception that occured.
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }

        #endregion

        #region Constructors

        public AsyncResultNoResult(AsyncCallback asyncCallback, Object state)
        {
            _asyncCallback = asyncCallback;
            _asyncState = state;
        }

        #endregion

        #region Public Methods

        public void SetAsCompleted(Exception exception, Boolean completedSynchronously)
        {
            // Passing null for exception means no error occurred; this is the common case
            _exception = exception;

            // The m_CompletedState field MUST be set prior calling the callback
            Int32 prevState = Interlocked.Exchange(ref _completedState,
               completedSynchronously ? StateCompletedSynchronously : StateCompletedAsynchronously);
            //If the event state is not pending , return
            if (prevState != StatePending) return;
            // If the event exists, set it
            if (_asyncWaitHandle != null) _asyncWaitHandle.Set();

            // If a callback method was set, call it
            if (_asyncCallback != null) _asyncCallback(this);
        }

        public void EndInvoke()
        {
            // This method assumes that only 1 thread calls EndInvoke for this object
            if (!IsCompleted)
            {
                // If the operation isn't done, wait for it
                AsyncWaitHandle.WaitOne();
                AsyncWaitHandle.Close();
                _asyncWaitHandle = null;  // Allow early GC
            }

            // Operation is done: if an exception occured, throw it
            if (_exception != null) throw _exception;
        }

        #endregion

        #region Implementation of IAsyncResult

        public Object AsyncState { get { return _asyncState; } }

        public Boolean CompletedSynchronously
        {
            get { return _completedState == StateCompletedSynchronously; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (_asyncWaitHandle == null)
                {
                    Boolean done = IsCompleted;
                    using (ManualResetEvent manualResetEvent = new ManualResetEvent(done))
                    {
                        if (Interlocked.CompareExchange(ref _asyncWaitHandle, manualResetEvent, null) != null)
                        {
                            // Another thread created this object's event; dispose the event we just created
                            manualResetEvent.Close();
                        }
                        else
                        {
                            if (!done && IsCompleted)
                            {
                                // If the operation wasn't done when we created 
                                // the event but now it is done, set the event
                                _asyncWaitHandle.Set();
                            }
                        }
                    }
                }
                return _asyncWaitHandle;
            }
        }

        public Boolean IsCompleted
        {
            get { return _completedState != StatePending; }
        }

        #endregion
    }
}