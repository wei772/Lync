/*=====================================================================
  File:      ProcessRequestAsyncResult.cs

  Summary:   Helper class for receiving, processing and sending conversation
  data.

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
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Samples.ContactCenterExtension.ViewModels;

namespace Microsoft.Lync.Samples.ContactCenterExtension.Models
{
    internal class ProcessRequestAsyncResult : AsyncResultNoResult
    {
        #region Private Fields

        private readonly RequestProcessor _requestProcessor;

        #endregion

        #region Public Properties

        public Conversation Conversation { get; private set; }
        public string ApplicationId { get; private set; }
        public requestType Request { get; private set; }

        #endregion

        #region Public Constructors

        public ProcessRequestAsyncResult(requestType request,
            RequestProcessor requestProcessor,
            Conversation conversation,
            string applicationId,
            AsyncCallback userCallback, object state)
            : base(userCallback, state)
        {
            Request = request;
            _requestProcessor = requestProcessor;
            Conversation = conversation;
            ApplicationId = applicationId;
        }

        #endregion

        #region Public Methods

        public void Process()
        {
            bool succeeded = false;
            Exception exception = null;
            try
            {
                Conversation.BeginSendContextData(ApplicationId, WireHelpers.Request, WireHelpers.Serialize(Request), RequestCompleted, null);
                succeeded = true;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                if (!succeeded)
                {
                    if (exception == null)
                    {
                        exception = new Exception("Request failed with unhandled exception.");
                    }
                    Complete(exception, true);
                }
            }
        }

        public void ProcessResponse(responseType response)
        {
            Complete(
                string.Equals(response.result, "success", StringComparison.OrdinalIgnoreCase)
                    ? null
                    : new Exception("Operation Failed"), false);
        }

        #endregion

        #region Private Methods

        private void RequestCompleted(IAsyncResult result)
        {
            bool succeeded = false;
            Exception exception = null;
            try
            {
                Conversation.EndSendContextData(result);
                succeeded = true;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                if (!succeeded)
                {
                    if (exception == null)
                    {
                        exception = new Exception("Request failed with unhandled exception.");
                    }
                    Complete(exception, false);
                }
            }
        }

        protected virtual void Complete(Exception ex, bool completedSynchronously)
        {
            SetAsCompleted(ex, completedSynchronously);
            _requestProcessor.RemovePendingRequest(Request);
        }

        #endregion
    }
}
