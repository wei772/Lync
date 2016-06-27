/*=====================================================================
  File:      SupervisedAgentViewModel.cs

  Summary:   View Model for handling views of supervised Agents.

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
using Microsoft.Lync.Samples.ContactCenterExtension.Models;

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    public interface ISupervisedAgentViewModel : IAgentViewModel
    {
        Boolean IsMonitoring { get; set; }
    }

    public class SupervisedAgentViewModel : AgentViewModel, ISupervisedAgentViewModel
    {
        #region Fields

        public new enum Properties
        {
            IsMonitoring,
            CanMonitor
        }

        private readonly SupervisorDashboardChannel _supervisorDashboardChannel;

        #endregion

        #region Constructors

        public SupervisedAgentViewModel(SupervisorDashboardChannel supervisorDashboardChannel)
        {
            _supervisorDashboardChannel = supervisorDashboardChannel;
        }

        #endregion

        #region Public Events

        public event EventHandler<MonitoredEventArgs> Monitored;

        #endregion

        #region Private methods

        protected virtual void OnMonitored(MonitoredEventArgs e)
        {
            if (Monitored != null)
            {
                Monitored(this, e);
            }
        }

        private void ExecuteMonitor()
        {
            try
            {
                _supervisorDashboardChannel.BeginStartMonitoringSession(new Uri(Uri), ar =>
                {
                    try
                    {
                        SupervisorMonitoringChannel monitoringChannel = _supervisorDashboardChannel.EndStartMonitoringSession(ar);
                        OnMonitored(new MonitoredEventArgs(monitoringChannel));
                    }
                    catch (Exception)
                    {
                        IsMonitoring = false;
                    }
                }, null);
            }
            catch (Exception)
            {
                IsMonitoring = false;
            }
        }

        protected override void NotifyPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case "IsActive":
                    NotifyPropertyChanged(Properties.CanMonitor);
                    break;
            }
            base.NotifyPropertyChanged(propertyName);
        }

        #endregion

        #region ISupervisorAgentViewModel members

        public bool IsMonitoring
        {
            get { return _isMonitoring; }
            set
            {
                _isMonitoring = value;
                if (value)
                {
                    ExecuteMonitor();
                }
                NotifyPropertyChanged(Properties.IsMonitoring);
            }
        }

        private bool _isMonitoring;
        
        public bool CanMonitor
        {
            get
            {
                return IsActive;
            }
        }

        #endregion
    }
}