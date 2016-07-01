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

namespace PresenceIndicatorDesktop
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {

        public List<string> users = new List<string>();

        public Window1()
        {
            InitializeComponent();

            users.Add("sip:kim@contoso.com");
            users.Add("sip:dan@contoso.com");
            users.Add("sip:david@contoso.com");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string user in users)
            {
                PresenceIndicatorItem presenceIndicatorItem = new PresenceIndicatorItem();
                presenceIndicatorItem.SourceUri = user;
                listBox1.Items.Add(presenceIndicatorItem);
            }
        }
    }
}
