﻿/*=====================================================================
  File:      MonitoredEventArgs.cs

  Summary:   Helper for handling monitoring.

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

namespace Microsoft.Lync.Samples.ContactCenterExtension.Models
{
    public class MonitoredEventArgs : EventArgs
    {
        #region Public Properties

        public SupervisorMonitoringChannel MonitoringChannel { get; private set; }

        #endregion

        #region Public Constructors

        public MonitoredEventArgs(SupervisorMonitoringChannel monitoringChannel)
        {
            MonitoringChannel = monitoringChannel;
        }

        #endregion
    }
}