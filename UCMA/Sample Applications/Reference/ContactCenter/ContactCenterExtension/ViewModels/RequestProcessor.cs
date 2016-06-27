/*=====================================================================
  File:      RequestProcessor.cs

  Summary:   Viewmodel for adding and creating requests 

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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Lync.Samples.ContactCenterExtension.Models;

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    internal class RequestProcessor
    {
        #region Fields

        private readonly Dictionary<uint, ProcessRequestAsyncResult> _pendingRequests;
        private int _requestId;
        private readonly string _sessionId;

        #endregion

        #region Constructor

        public RequestProcessor(string sessionId)
        {
            _pendingRequests = new Dictionary<uint, ProcessRequestAsyncResult>();
            _sessionId = sessionId;
        }

        #endregion

        #region Public Methods

        public requestType CreateRequest(ContextChannelRequestType requestType)
        {
            return CreateRequest(GetNextRequestId(), _sessionId, requestType);
        }

        #endregion

        #region Private Methods

        private int GetNextRequestId()
        {
            return Interlocked.Increment(ref _requestId);
        }

        private static requestType CreateRequest(int requestId, string sessionId,
                                                 ContextChannelRequestType requestType)
        {
            requestType request = new requestType 
                                      {
                                          requestId = (uint) requestId, 
                                          sessionId = sessionId
                                      };

            switch (requestType)
            {
                case ContextChannelRequestType.Hold:
                    request.hold = new object();
                    break;
                case ContextChannelRequestType.Escalate:
                    break;
                case ContextChannelRequestType.Retrieve:
                    request.retrieve = new object();
                    break;
                case ContextChannelRequestType.StartMonitoring:
                    break;
                case ContextChannelRequestType.StopMonitoring:
                    break;
                case ContextChannelRequestType.Whisper:
                    break;
                case ContextChannelRequestType.BargeIn:
                    request.bargein = new object();
                    break;
                default:
                    Debug.Assert(false, "invalid request type");
                    break;
            }

            return request;
        }

        #endregion

        #region Internal Methods

        internal void AddPendingRequest(requestType request, ProcessRequestAsyncResult requestAsyncResult)
        {
            lock (_pendingRequests)
            {
                _pendingRequests.Add(request.requestId, requestAsyncResult);
            }
        }

        internal void RemovePendingRequest(requestType request)
        {
            lock (_pendingRequests)
            {
                ProcessRequestAsyncResult pendingRequest;

                if (_pendingRequests.TryGetValue(request.requestId, out pendingRequest))
                {
                    _pendingRequests.Remove(request.requestId);
                }
            }
        }

        internal void ProcessResponse(responseType response)
        {
            ProcessRequestAsyncResult pendingRequest;
            lock (_pendingRequests)
            {
                _pendingRequests.TryGetValue(response.requestId, out pendingRequest);
            }

            if (pendingRequest != null)
            {
                pendingRequest.ProcessResponse(response);
            }
        }

        #endregion
    }
}