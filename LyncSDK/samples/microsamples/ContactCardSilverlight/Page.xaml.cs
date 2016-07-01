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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Lync.Controls;

namespace ContactCardSilverlight
{
    public partial class Page : UserControl
    {
        public List<string> users = new List<string>();

        public Page()
        {
            InitializeComponent();

            users.Add("sip:kim@contoso.com");
            users.Add("sip:dan@contoso.com");
            users.Add("sip:david@contoso.com");

            foreach (string user in users)
            {
                ContactCard contactCard = new ContactCard();
                contactCard.Source = user;
                listBox1.Items.Add(contactCard);

            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
           
            
        }
    }
}
