/*=====================================================================
  File:      AgentDashboardView.xaml.cs

  Summary:   View for displaying the Agent Dashboard.

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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Lync.Samples.ContactCenterExtension.ViewModels;

namespace Microsoft.Lync.Samples.ContactCenterExtension.Views
{
    public partial class AgentDashboardView : UserControl
    {
        #region Constructors

        public AgentDashboardView(AgentDashboard agentDashboard)
            : this()
        {
            DataContext = agentDashboard;

            ShowEscalationViewCommand = new Command
            {
                Execute = ExecuteShowEscalationViewCommand,
                CanExecute = CanExecuteShowEscalationViewCommand
            };
        }



        public AgentDashboardView()
        {
            InitializeComponent();
        }

        #endregion



        #region Commands

        #region ShowEscalationViewCommand

        public ICommand ShowEscalationViewCommand { get; private set; }

        private void ExecuteShowEscalationViewCommand(Object parameter)
        {
            EscalationView.Visibility = Visibility.Visible;
        }

        private bool CanExecuteShowEscalationViewCommand(Object parameter)
        {
            return true;
        }

        #endregion

        #endregion
    }
}
