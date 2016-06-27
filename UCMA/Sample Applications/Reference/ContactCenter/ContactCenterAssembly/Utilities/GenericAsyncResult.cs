
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

/******************************************************************************
Module:  GenericAsyncResult.cs
Notices: Written by Jeffrey Richter
******************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Rtc.Collaboration.Samples.Utilities
{

    #region GenericEndAsyncState class
    internal class GenericEndAsyncState
    {
        private AsyncResultNoResult _userAsyncResult;
        private AsyncCallback _endCallDelegate;

        internal GenericEndAsyncState(AsyncResult<object> userAsyncResult, AsyncCallback endCallDelegate)
        {
            if (endCallDelegate == null)
            {
                throw new InvalidOperationException("endCallDelegate argument MUST NOT be null");
            }

            _userAsyncResult = userAsyncResult;
            _endCallDelegate = endCallDelegate;
        }

        internal AsyncResultNoResult UserAsyncResult
        {
            get { return _userAsyncResult; }
        }

        internal AsyncCallback EndCallDelegate
        {
            get { return _endCallDelegate; }
        }
    }
    #endregion

}
