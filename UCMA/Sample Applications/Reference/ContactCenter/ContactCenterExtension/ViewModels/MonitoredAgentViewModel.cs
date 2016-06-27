/*=====================================================================
  File:      MonitoredAgentViewModel.cs

  Summary:   ViewModel for Monitoring Agents 

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
    #region Interfaces

    public interface IMonitoredAgentViewModel : IAgentViewModel
    {
        Boolean IsWhispering { get; set; }
    }

    #endregion

    public class MonitoredAgentViewModel : AgentViewModel, IMonitoredAgentViewModel
    {
        #region private data members

        private readonly SupervisorMonitoringChannel _monitoringChannel;
        private bool _isWhispering;

        #endregion

        #region constructors

        public MonitoredAgentViewModel(SupervisorMonitoringChannel monitoringChannel)
        {
            _monitoringChannel = monitoringChannel;
        }

        #endregion

        #region IMonitoringAgentViewModel members

        public bool CanWhisper
        {
            get
            {
                return true;
            }
        }

        public bool IsWhispering
        {
            get { return _isWhispering; }
            set
            {
                _isWhispering = value;
                if (value)
                {
                    ExecuteWhisper();
                }
                NotifyPropertyChanged("IsWhispering");
            }
        }

        #endregion

        #region private methods

        private void ExecuteWhisper()
        {
            try
            {
                _monitoringChannel.BeginWhisper(new Uri(Uri), ar =>
                                                                  {
                                                                      try
                                                                      {
                                                                          _monitoringChannel.EndWhisper(ar);
                                                                      }
                                                                      catch (Exception)
                                                                      {
                                                                          IsWhispering = false;
                                                                      }
                                                                  }, null);

            }
            catch (Exception)
            {
                IsWhispering = false;
            }
        }

        #endregion
    }
}