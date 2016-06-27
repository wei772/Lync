/*=====================================================================
  File:      ProposalDetails.xaml.cs

  Summary:   Backend class for ProposalDetails.xaml.

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

namespace ProposalTracker.Controls
{
    /// <summary>
    /// A child window that displays the content of the selected proposal as defined in the Proposals.cs class.
    /// Shows the description, the team Size, Supervisor and team members. 
    /// The supervisor is displayed in a Contact Card and the team members are presented in a CustomContactList.
    /// Check ProposalDetails.xaml to see the binding.
    /// </summary>
    public partial class ProposalDetails : ChildWindow
    {
        public ProposalDetails()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Click event for the "Back to Main Page" hyperlink button. Just closes the current child window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackToMainPageButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}

