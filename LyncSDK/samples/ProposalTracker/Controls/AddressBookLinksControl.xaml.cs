/*=====================================================================
  File:      AddressBookLinksControl.xaml.cs

  Summary:   Backend class for AddressBookLinksControl.xaml.

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
using System.Windows.Controls;

namespace ProposalTracker.Controls
{
    /// <summary>
    /// This control displays the AddressBook. Doesn't have any communicator controls at this point. It consists of buttons that could be connected
    /// to other controls at any point. Data is directly entered into the xaml code.
    /// </summary>
    public partial class AddressBookLinksControl : UserControl
    {
        public AddressBookLinksControl()
        {
            InitializeComponent();
        }
    }
}
