/*=====================================================================
  File:      SalesLeaderBoardControl.xaml.cs

  Summary:   Backend class for SalesLeaderBoardControl.xaml.

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
using ProposalTracker.Models;

namespace ProposalTracker.Controls
{
    /// <summary>
    /// This control displays the PresenceIndicator along the person who is associated with it. 
    /// A static Sales Team provides a list of Sales Person that is bound to the Listbox.
    /// </summary>
    public partial class SalesLeaderBoardControl : UserControl
    {
        public SalesLeaderBoardControl()
        {
            InitializeComponent();

            //Bind the listbox to a list of Sales People. List is retrieved from the static class Sales Team.
            SalesPersonsListbox.ItemsSource = SalesTeam.SalesPeople;
        }
    }
}
