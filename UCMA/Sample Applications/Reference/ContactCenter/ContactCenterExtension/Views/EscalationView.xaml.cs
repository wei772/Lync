/*=====================================================================
  File:      EscalationView.xaml.cs

  Summary:   View for displaying Escalation options.

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
using System.Windows;
using System.Windows.Controls;
using Microsoft.Lync.Samples.ContactCenterExtension.ViewModels;

namespace Microsoft.Lync.Samples.ContactCenterExtension.Views
{
    public partial class EscalationView : UserControl
    {
        public EscalationView()
        {
            InitializeComponent();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void EscalateButtonClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}