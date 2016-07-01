/*=====================================================================
  This file is part of the Microsoft Unified Communications Code Samples.

  Copyright (C) 2012 Microsoft Corporation.  All rights reserved.

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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Group;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Lync.Model.Extensibility;

namespace MeetNow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LyncClient LyncClient;
        Automation Automation;
        ContactManager ContactManager;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LyncClient = LyncClient.GetClient();
                Automation = LyncClient.GetAutomation();
                this.DataContext = this;
            }
            catch (Exception ex)
            {
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Automation.BeginMeetNow(MeetNow_Callback, null);
        }

        private void MeetNow_Callback(IAsyncResult ar)
        {
            ConversationWindow cw = Automation.EndMeetNow(ar);
            this.Dispatcher.Invoke(new Action(() =>
                {
                    MeetNowStatusTextBlock.Text = cw.State.ToString();
                }), null);
        }
    }
}
