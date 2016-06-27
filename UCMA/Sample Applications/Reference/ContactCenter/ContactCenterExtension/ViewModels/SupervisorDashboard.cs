/*=====================================================================
  File:      SupervisorDashboard.cs

  Summary:   View Model for the supervisor's dashboard view.

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.Lync.Samples.ContactCenterExtension.Models;

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    #region Interfaces

    public interface ISupervisorDashboard<TAgentVm>
        where TAgentVm : IAgentViewModel
    {
        ObservableCollection<TAgentVm> Agents { get; }

        Boolean IsMonitoring { get; }

        SupervisorConversationViewModel MonitoredConversation { get; }
    }

    #endregion

    public class SupervisorDashboard : ViewModel<SupervisorDashboardChannel>, ISupervisorDashboard<SupervisedAgentViewModel>
    {
        #region Fields

        private enum Properties
        {
            IsMonitoring,
            MonitoredConversation
        }

        private const string ApplicationGuid = "{63D37F02-47B3-4B9E-AA8E-FEF3665298DD}";
        private readonly ObservableCollection<SupervisedAgentViewModel> _agents;

        #region Sample Data

        public List<Order> Orders
        {
            get
            {
                return new List<Order>
                           {
                            new Order
                                {
                                Date = "08/29/2012",
                                OrderNumber = "8777",
                                Product = "NOKIA LUMIA 900 (CYAN)",
                                Status = "Pending"
                                },
                            new Order
                                {
                                Date = "05/25/2010",
                                OrderNumber = "4734",
                                Product = "SCREEN PROTECTOR",
                                Status = "Returned"
                                },
                            new Order
                                {
                                Date = "05/15/2010",
                                OrderNumber = "4168",
                                Product = "CAR CHARGER",
                                Status = "Returned"
                                },
                            new Order
                                {
                                Date = "04/14/2010",
                                OrderNumber = "9856",
                                Product = "HTC TOUCH PRO2",
                                Status = "Delivered"
                                }
                           };
            }
        }

        #endregion

        #endregion

        #region Constructors

        public SupervisorDashboard()
            : base(new SupervisorDashboardChannel(ApplicationGuid))
        {

            Model.AgentsChanged += ModelAgentsChanged;

            _agents = new ObservableCollection<SupervisedAgentViewModel>();
            PopulateAgents();
        }



        #endregion

        #region ISupervisorDashboard<AgentViewModel,TConverstationViewModel> Members

        public ObservableCollection<SupervisedAgentViewModel> Agents
        {
            get { return _agents; }
        }

        public Boolean IsMonitoring
        {
            get
            {
                return _isMonitoring;
            }
            private set
            {
                if (_isMonitoring != value)
                {
                    _isMonitoring = value;
                    NotifyPropertyChanged(Properties.IsMonitoring);
                }
            }
        }
        private Boolean _isMonitoring;

        public SupervisorConversationViewModel MonitoredConversation
        {
            get
            {
                return _monitoredConversation;
            }
            private set
            {
                if (_monitoredConversation != value)
                {
                    _monitoredConversation = value;
                    NotifyPropertyChanged(Properties.MonitoredConversation);
                }
            }
        }
        private SupervisorConversationViewModel _monitoredConversation;

        #endregion

        #region Event Handlers

        private void ModelAgentsChanged(object sender, AgentsChangedEventArgs e)
        {
            foreach (string uri in e.AgentsRemoved)
            {
                Agents.Remove(new SupervisedAgentViewModel(Model) { Uri = uri });
            }

            foreach (agentType agent in e.AgentsUpdated)
            {
                SupervisedAgentViewModel avm = CreateAgentViewModel(agent);

                int index;

                if ((index = _agents.IndexOf(avm)) != -1)
                {
                    _agents[index].DisplayName = avm.DisplayName;

                    _agents[index].StartTime = avm.StartTime;
                    _agents[index].Status = avm.Status;

                    _agents[index].MediaTypes.Clear();

                    if (avm.MediaTypes != null)
                    {
                        foreach (string m in avm.MediaTypes)
                        {
                            _agents[index].MediaTypes.Add(m);
                        }
                    }
                }
                else
                {
                    avm.PropertyChanged += AvmPropertyChanged;
                    avm.Monitored += AvmMonitored;

                    Agents.Add(avm);
                }

            }
        }

        private void AvmPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, SupervisedAgentViewModel.Properties.IsMonitoring.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                IsMonitoring = _agents.Any(a => a.IsMonitoring);
            }
        }

        private void AvmMonitored(object sender, MonitoredEventArgs e)
        {
            MonitoredConversation = new SupervisorConversationViewModel(e.MonitoringChannel);
            MonitoredConversation.Terminated += MonitoredConversationTerminated;
        }

        void MonitoredConversationTerminated(object sender, EventArgs e)
        {
            IsMonitoring = false;
            foreach (var agent in _agents)
            {
                agent.IsMonitoring = false;
            }
            MonitoredConversation.Terminated -= MonitoredConversationTerminated;
            MonitoredConversation = null;
        }

        private SupervisedAgentViewModel CreateAgentViewModel(agentType agent)
        {
            SupervisedAgentViewModel avm = new SupervisedAgentViewModel(Model);
            avm.DisplayName = agent.displayname;

            Int64 startTime;
            if (Int64.TryParse(agent.statuschangedtime, out startTime))
            {
                avm.StartTime = new DateTime(startTime).ToLocalTime();
            }


            if (agent.mediatypes != null)
            {
                foreach (string m in agent.mediatypes)
                {
                    avm.MediaTypes.Add(m);
                }
            }

            avm.Status = agent.status;
            avm.Uri = agent.uri;

            return avm;
        }
        #endregion

        #region Private Methods

        private void PopulateAgents()
        {
            foreach (agentType agent in Model.Agents)
            {
                SupervisedAgentViewModel avm = CreateAgentViewModel(agent);

                avm.PropertyChanged += AvmPropertyChanged;
                avm.Monitored += AvmMonitored;

                Agents.Add(avm);
            }
        }

        #endregion
    }

}
