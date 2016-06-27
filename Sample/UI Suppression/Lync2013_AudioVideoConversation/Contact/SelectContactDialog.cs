/*===================================================================== 
  This file is part of the Microsoft Unified Communications Code Samples. 

  Copyright (C) 2010 Microsoft Corporation.  All rights reserved. 

This source code is intended only as a supplement to Microsoft 
Development Tools and/or on-line documentation.  See these other 
materials for detailed information regarding Microsoft code samples. 

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
PARTICULAR PURPOSE. 
=====================================================================*/

using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Lync.Model;

namespace AudioVideoConversation
{
    /// <summary>
    /// Implements a simple dialog to allow the user to type a SIP URI.
    /// </summary>
    public partial class SelectContactDialog : Form
    {
        private Contact contact;

        public SelectContactDialog(ContactManager lyncContactManager)
        {
            InitializeComponent();
            Debug.Assert(lyncContactManager != null);

            //adds a contact list to itself
            SimpleContactFinderControl contactListControl = 
                new SimpleContactFinderControl(lyncContactManager, "Find contact by URI");
            contactListControl.ContactSelected += new ContactSelected(contactListControl_ContactsSelected);

            //adds the contact list control to the UI
            contactListControl.Dock = DockStyle.Fill;
            this.Controls.Add(contactListControl);
        }

        /// <summary>
        /// Called when a contact is selected.
        /// </summary>
        void contactListControl_ContactsSelected(object source, Contact selectedContact)
        {
            Debug.Assert(selectedContact != null);
            //saves the selected contact);
            this.contact = selectedContact;

            //marks the result of the dialog as OK and close
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Gets the selected contact.
        /// </summary>
        public Contact Contact
        {
            get
            {
                return contact;
            }
        }
    }
}
