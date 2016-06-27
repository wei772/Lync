/*=====================================================================
  File:      StartMonitoringProcessRequestAsyncResult.cs

  Summary:   Model for starting supervisor monitoring

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
using Microsoft.Lync.Samples.ContactCenterExtension.ViewModels;

namespace Microsoft.Lync.Samples.ContactCenterExtension.Models
{
    internal class StartMonitoringProcessRequestAsyncResult : ProcessRequestAsyncResult
    {
        #region Public Properies

        public SupervisorMonitoringChannel SupervisorMonitoringChannel { get; private set; }

        #endregion

        #region Public Constructors

        public StartMonitoringProcessRequestAsyncResult(SupervisorMonitoringChannel monitoringChannel,
                                                        requestType request,
                                                        RequestProcessor requestProcessor,
                                                        AsyncCallback userCallback, object state)
            : base(request, requestProcessor, monitoringChannel.SupervisorDashboardChannel.Conversation,
                   monitoringChannel.SupervisorDashboardChannel.ApplicationId, userCallback, state)
        {
            SupervisorMonitoringChannel = monitoringChannel;
        }

        #endregion

    }
}