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

using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Lync.Model;

namespace AudioVideoConversation
{

    /// <summary>
    /// Called when contacts are selected by the user.
    /// </summary>
    public delegate void ContactSelected(object source, Contact contact);

    /// <summary>
    /// Implements a contact list that shows the display-name only of your contacts.
    /// </summary>
    public partial class SimpleContactFinderControl : UserControl
    {
        //holds the Lync client instance
        private ContactManager lyncContactManager;

        public SimpleContactFinderControl(ContactManager lyncContactManager, string buttonText)
        {
            InitializeComponent();

            //saves the client instance
            Debug.Assert(lyncContactManager != null);
            this.lyncContactManager = lyncContactManager;

            //changes the button text
            buttonSelect.Text = buttonText;
        }

        /// <summary>
        /// Occurs when contacts are selected by the user.
        /// </summary>
        public event ContactSelected ContactSelected;

        /// <summary>
        /// Obtains a contact using the URI provided.
        /// </summary>
        private void buttonSelect_Click(object sender, EventArgs e)
        {
            //gets the typed uri
            string uri = textBoxAddress.Text;

            //simple validation: ignores empty string
            if (string.IsNullOrEmpty(uri))
            {
                Console.WriteLine("No URI specified");
                return;
            }

            //*****************************************************************************************
            //                              contactManager.GetContactByUri
            //
            // This method will may return a contact, even if it doesn't match a phone number or a 
            // SIP URI in the database. 
            //
            // Calls placed using contact object that does not match a valid number or an actual contact
            // will fail after the modality starts connecting.
            //
            //*****************************************************************************************

            Contact contact = null;

            //Finds the contact using the provided URI (synchronously)
            try
            {
                contact = lyncContactManager.GetContactByUri(uri);
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine("Contact not found.  Did you use the sip: or tel: prefix? " + lyncClientException);
            }
            catch (SystemException systemException)
            {
                if (LyncModelExceptionHelper.IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    Console.WriteLine("Error: " + systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }

            //if there was somebody listening to the event
            ContactSelected onContactSelected = ContactSelected;
            if (contact != null && onContactSelected != null)
            {
                //notify that the contact was selected
                onContactSelected(this, contact);
            }
        }
    }


}
