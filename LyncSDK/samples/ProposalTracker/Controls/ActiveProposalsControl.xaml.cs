/*=====================================================================
  File:      ActiveProposalsControl.xaml.cs

  Summary:   Backend class for ActiveProposalsControl.xaml.

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
using ProposalTracker.Models;

namespace ProposalTracker.Controls
{
    /// <summary>
    /// ActiveProposalsControl is a control which displays a bound collection of Proposal objects. 
    /// </summary>
    /// <remarks>
    /// This control shows a ListBox bound to a proposals collection, a textbox that displays additional
    /// information on the selected proposal, and a “More” button which opens a child window 
    /// to show details about the selected proposal.
    /// </remarks>
    public partial class ActiveProposalsControl : UserControl
    {
        //Create a child window instance. Will have to be instantiated in the constructor
        private ProposalDetails _proposalDetails;

        public ActiveProposalsControl()
        {
            InitializeComponent();

            //Initialize the child window
            _proposalDetails = new ProposalDetails();

            //Bound the ListBox to a list of Proposals. The static class SalesTeam provides the list of proposals.
            proposalsListBox.ItemsSource = SalesTeam.Proposals;
        }

        #region Private Events and Methods
        /// <summary>
        /// Event fired when an element is selected from the listbox. Since the listbox is bound to the proposals 
        /// collection, when the selected item is changed, the description displayed in txtProposalsDetails also changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProposalsListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Grab the proposal that is bound to the selected item.
            Proposal proposal = proposalsListBox.SelectedItem as Proposal;

            //Check if the selected proposal is null. If proposal is null, txtProposalDetails will be hidden.
            if (proposal != null)
            {
                //Set the datacontext of the txtProposalsDetails textbox to the proposal selected in the listbox
                txtProposalsDetails.DataContext = proposal;

                //Make the whole ProposalsDetailsSection visible
                ProposalsDetailsSection.Visibility = Visibility.Visible;
            }
            else
            {
                //hide the txtProposalsDetails textblock
                ProposalsDetailsSection.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Event to open the ProposalDetails Control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoreButtonClick(object sender, RoutedEventArgs e)
        {
            Proposal proposal = proposalsListBox.SelectedItem as Proposal;
            if (proposal != null)
            {
                _proposalDetails.DataContext = proposal;
                _proposalDetails.Show();
            }
        }

        #endregion
    }
}
