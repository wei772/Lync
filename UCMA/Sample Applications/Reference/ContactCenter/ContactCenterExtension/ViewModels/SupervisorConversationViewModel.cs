/*=====================================================================
  File:      SupervisorConversationViewModel.cs

  Summary:   View Model for the conversation being monitored by supervisor.

 * ---------------------------------------------------------------------
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
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Lync.Samples.ContactCenterExtension.Models;

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    #region Interfaces

    public interface ISupervisorConversationViewModel<TParticipantVm,TAgentVm>
        where TParticipantVm : IParticipantViewModel
        where TAgentVm : IAgentViewModel
    {
        ObservableCollection<TParticipantVm> Customers { get; }

        ObservableCollection<TAgentVm> Agents { get; }

        ObservableCollection<TParticipantVm> Participants { get; }

        Boolean IsMonitoringReady { get; set; }

        Boolean IsBargedIn { get; }

        ICommand BargeInCommand { get; }

        ICommand TerminateCommand { get; }

    }

    #endregion

    public class SupervisorConversationViewModel : ViewModelBase, ISupervisorConversationViewModel<ParticipantViewModel, MonitoredAgentViewModel>
    {
        #region private members

        private readonly SupervisorMonitoringChannel _monitoringChannel;
        private readonly ObservableCollection<MonitoredAgentViewModel> _agents;
        private readonly ObservableCollection<ParticipantViewModel> _participants;
        private readonly ObservableCollection<ParticipantViewModel> _customers;

        private bool _isBargedIn;
        private bool _isBargeInInProgress;
        private bool _isMonitoringReady;

        private readonly Command _bargeInCommand;

        #endregion

        #region Properties

        private bool IsBargeinInProgress
        {
            get { return _isBargeInInProgress; }
            set
            {
                _isBargeInInProgress = value;
                ((Command)BargeInCommand).NotifyCanExecuteChanged();
            }
        }


        #endregion

        #region Events

        public event EventHandler Terminated;

        #endregion

        #region Constructors

        public SupervisorConversationViewModel(SupervisorMonitoringChannel monitoringChannel)
        {
            _monitoringChannel = monitoringChannel;

            monitoringChannel.AgentsChanged += MonitoringChannelAgentsChanged;
            monitoringChannel.ParticipantsChanged += MonitoringChannelParticipantsChanged;
            monitoringChannel.CustomersChanged += MonitoringChannelCustomersChanged;

            _agents = new ObservableCollection<MonitoredAgentViewModel>();
            _participants = new ObservableCollection<ParticipantViewModel>();
            _customers = new ObservableCollection<ParticipantViewModel>();

            PopulateAgentsAndParticipants();

            _bargeInCommand = new Command(ExecuteBargeInCommand, CanExecuteBargeInCommand);
            TerminateCommand = new Command(ExecuteTerminateCommand, CanExecuteTerminateCommand);
        }


        #endregion

        #region Methods

        private void PopulateAgentsAndParticipants()
        {
            foreach (agentType agent in _monitoringChannel.Agents)
            {
                MonitoredAgentViewModel avm = CreateAgentViewModel(agent);

                Agents.Add(avm);
            }

            foreach (participantType participant in _monitoringChannel.Participants)
            {
                ParticipantViewModel pvm = CreateParticipantViewModel(participant);

                Participants.Add(pvm);
            }

            foreach (participantType customer in _monitoringChannel.Customers)
            {
                ParticipantViewModel pvm = CreateParticipantViewModel(customer);

                Customers.Add(pvm);
            }

            if (Agents.Count > 0 || Customers.Count > 0 || Participants.Count > 0)
            {
                IsMonitoringReady = true;
            }
        }

        public bool CanExecuteBargeInCommand(Object obj)
        {
            return !IsBargedIn && !IsBargeinInProgress;
        }

        public void ExecuteBargeInCommand(Object obj)
        {
            try
            {
                _monitoringChannel.BeginBargeIn(ar =>
                {
                    try
                    {
                        _monitoringChannel.EndBargeIn(ar);
                        IsBargedIn = true;
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        IsBargeinInProgress = false;
                    }
                }, null);

                IsBargeinInProgress = true;
            }
            catch (Exception)
            {
                IsBargeinInProgress = false;
            }
        }

        public bool CanExecuteTerminateCommand(Object obj)
        {
            return true;
        }

        public void ExecuteTerminateCommand(Object obj)
        {
            if (IsBargedIn)
            {
                // TODO: undo barge-in
            }

            try
            {
                IsMonitoringReady = false;
                _monitoringChannel.SupervisorDashboardChannel.BeginStopMonitoringSession(ar =>
                {
                    try
                    {
                        _monitoringChannel.SupervisorDashboardChannel.EndStopMonitoringSession(ar);
                    }
                    catch (Exception)
                    {

                    }
                }, null);

            }
            catch (Exception)
            {
                
            }

            OnTerminated(new EventArgs());
        }

        protected void OnTerminated(EventArgs e)
        {
            if( Terminated != null )
            {
                Terminated(this, e);
            }
        }

        #endregion

        #region Event Handlers

        void MonitoringChannelParticipantsChanged(object sender, ParticipantsChangedEventArgs e)
        {
            foreach (string uri in e.ParticipantsRemoved)
            {
                Participants.Remove(new ParticipantViewModel { Uri = uri });
            }

            foreach (participantType participant in e.ParticipantsUpdated)
            {
                ParticipantViewModel avm = CreateParticipantViewModel(participant);

                int index;

                if ((index = _participants.IndexOf(avm)) != -1)
                {
                    _participants[index].DisplayName = avm.DisplayName;

                    _participants[index].MediaTypes.Clear();

                    if (avm.MediaTypes != null)
                    {
                        foreach (string m in avm.MediaTypes)
                        {
                            _participants[index].MediaTypes.Add(m);
                        }
                    }
                }
                else
                {
                    Participants.Add(avm);
                }

            }

            if (!IsMonitoringReady &&
                (Agents.Count > 0 || Customers.Count > 0 || Participants.Count > 0))
            {
                IsMonitoringReady = true;
            }
        }

        void MonitoringChannelAgentsChanged(object sender, AgentsChangedEventArgs e)
        {
            foreach (string uri in e.AgentsRemoved)
            {
                Agents.Remove(new MonitoredAgentViewModel(_monitoringChannel) { Uri = uri });
            }

            foreach (agentType agent in e.AgentsUpdated)
            {
                MonitoredAgentViewModel avm = CreateAgentViewModel(agent);

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
                    Agents.Add(avm);
                }

            }

            if (!IsMonitoringReady &&
                (Agents.Count > 0 || Customers.Count > 0 || Participants.Count > 0))
            {
                IsMonitoringReady = true;
            }
        }

        void MonitoringChannelCustomersChanged(object sender, ParticipantsChangedEventArgs e)
        {
            foreach (string uri in e.ParticipantsRemoved)
            {
                Customers.Remove(new ParticipantViewModel { Uri = uri });
            }

            foreach (participantType participant in e.ParticipantsUpdated)
            {
                ParticipantViewModel avm = CreateParticipantViewModel(participant);

                int index;

                if ((index = _customers.IndexOf(avm)) != -1)
                {
                    _customers[index].DisplayName = avm.DisplayName;

                    _customers[index].MediaTypes.Clear();

                    if (avm.MediaTypes != null)
                    {
                        foreach (string m in avm.MediaTypes)
                        {
                            _customers[index].MediaTypes.Add(m);
                        }
                    }
                }
                else
                {
                    Customers.Add(avm);
                }

            }

            if (!IsMonitoringReady &&
                (Agents.Count > 0 || Customers.Count > 0 || Participants.Count > 0))
            {
                IsMonitoringReady = true;
            }
        }
        private MonitoredAgentViewModel CreateAgentViewModel(agentType agent)
        {
            MonitoredAgentViewModel avm = new MonitoredAgentViewModel(_monitoringChannel);
            avm.DisplayName = agent.displayname;
            //TODO:Ask stephane to send a number, ticks

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

        private static ParticipantViewModel CreateParticipantViewModel(participantType participant)
        {
            ParticipantViewModel pvm = new ParticipantViewModel();
            pvm.DisplayName = participant.displayname;
            pvm.IsCustomer = participant.iscustomer;



            if (participant.mediatypes != null)
            {
                foreach (string m in participant.mediatypes)
                {
                    pvm.MediaTypes.Add(m);
                }
            }

            pvm.Uri = participant.uri;

            return pvm;
        }
        #endregion

        #region ISupervisorConversationViewModel<ParticipantViewModel,AgentViewModel> Members

        public ObservableCollection<MonitoredAgentViewModel> Agents
        {
            get { return _agents; }
        }

        public ObservableCollection<ParticipantViewModel> Customers
        {
            get
            {
                return _customers;
            }
        }

        public ObservableCollection<ParticipantViewModel> Participants
        {
            get
            {
                return _participants;
            }
        }

        public bool IsBargedIn
        {
            get { return _isBargedIn; }
            private set
            {
                _isBargedIn = value;
                NotifyPropertyChanged("IsBargedIn");
            }
        }

        public bool IsMonitoringReady
        {
            get { return _isMonitoringReady; }
            set
            {
                _isMonitoringReady = value;
                NotifyPropertyChanged("IsMonitoringReady");
            }
        }

        public ICommand BargeInCommand
        {
            get { return _bargeInCommand; }
        }

        public ICommand TerminateCommand { get; set; }

        #endregion

    }

}

