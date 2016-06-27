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
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Lync.Model.Conversation;

namespace AudioVideoConversation
{
    /// <summary>
    /// Implements a simple dialog to allow the user to select a conversation.
    /// </summary>
    public partial class ConversationPickerDialog : Form
    {
        public ConversationPickerDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the selected conversation in the dialog.
        /// </summary>
        public Conversation Conversation
        {
            get;
            private set;
        }

        /// <summary>
        /// Shows the conversations in a combobox to allow the user to select one.
        /// </summary>
        public void ShowConversations(IList<Conversation> allConversations, Conversation currentConversation)
        {
            //iterates through all conversations...
            foreach (Conversation conversation in allConversations)
            {
                //... ignoring the one associated with the conversation window
                if (conversation.Equals(currentConversation))
                {
                    continue;
                }

                //adds a new item to the comboBox
                comboBoxConversations.Items.Add(new ConversationItem(conversation));
            }
        }

        /// <summary>
        /// Gets the selected conversation and validates it.
        /// </summary>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            //gets the selected conversation
            ConversationItem item = comboBoxConversations.SelectedItem as ConversationItem;

            //checks if there was a valid selection
            if (item != null)
            {
                //marks the result of the dialog as OK
                this.DialogResult = DialogResult.OK;

                //saves the selected conversation
                Conversation = item.Conversation;

                //closes
                this.Close();
            }
        }

        /// <summary>
        /// Cancels and closes the dialog.
        /// </summary>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            //marks the result of the dialog as Cancel and close it
            Conversation = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
