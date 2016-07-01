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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;

namespace LockVideo
{
    public partial class LockVideo : Form
    {
        /// <summary>
        /// The LyncClient class instance that encapsulates the Lync client platform
        /// </summary>
        public LyncClient client;

        /// <summary>
        /// The conversation manager that holds the conversations
        /// </summary>
        public ConversationManager conversationManager;

        /// <summary>
        /// The list of conversations from conversation manager
        /// </summary>
        public IList<Conversation> conversations;

        /// <summary>
        /// The current conversation
        /// </summary>
        public Conversation conversation;

        /// <summary>
        /// The list of participants in the conversation
        /// </summary>
        public IList<Participant> participants;

        /// <summary>
        /// The current participant
        /// </summary>
        public Participant participant;

        /// <summary>
        /// Constructor
        /// </summary>
        public LockVideo()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  When form loads, gets instance of LyncClient and ConversationManager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LockVideo_Load(object sender, EventArgs e)
        {
            try
            {
                client = LyncClient.GetClient();
                conversationManager = client.ConversationManager;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Gets the list of conversations from conversation manager and lists them in the conversation listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGetConversations_Click(object sender, EventArgs e)
        {
            try
            {
                listBoxConversations.Items.Clear();
                conversations = conversationManager.Conversations;
                foreach (Conversation conversationItem in conversations)
                {
                    listBoxConversations.Items.Add(conversationItem.Properties[ConversationProperty.Id].ToString());
                }

                //If there is at least one conversation, select the first one in the listbox
                if (listBoxConversations.Items.Count > 0)
                {
                    listBoxConversations.SelectedItem = listBoxConversations.Items[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Gets a list of participants from the conversation and adds them to the participant listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGetParticipants_Click(object sender, EventArgs e)
        {
            try
            {
                listBoxParticipants.Items.Clear();
                conversation = null;
                int index = listBoxConversations.SelectedIndex;
                conversation = conversations[index];

                if (conversation != null)
                {
                    participants = conversation.Participants;
                    foreach (Participant participant in participants)
                    {
                        string name = participant.Properties[ParticipantProperty.Name].ToString();
                        listBoxParticipants.Items.Add(name);
                    }
                }

                //Selects the first participant in the listbox
                if (listBoxParticipants.Items.Count > 0)
                {
                    listBoxParticipants.SelectedItem = listBoxParticipants.Items[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Returns the Participant item based on the selected item in the participant listbox
        /// </summary>
        /// <returns></returns>
        private Participant GetParticipant()
        {
            try
            {
                string participantName = listBoxParticipants.SelectedItem.ToString();
                foreach (Participant participantItem in participants)
                {
                    if (participantItem.Properties[ParticipantProperty.Name].ToString() == participantName)
                    {
                        return participantItem;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Locks the video of the current selected participant in the video gallery.  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLockVideo_Click(object sender, EventArgs e)
        {
            try
            {
                participant.BeginLockVideo(LockVideoCallback, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Callback for BeginLockVideo
        /// </summary>
        /// <param name="ar"></param>
        private void LockVideoCallback(IAsyncResult ar)
        {
            try
            {
                participant.EndLockVideo(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// UnLocks the current selected participant from the video gallery
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonUnLockVideo_Click(object sender, EventArgs e)
        {
            try
            {
                participant.BeginUnLockVideo(UnLockVideoCallback, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Callback for BeginUnLockVideo
        /// </summary>
        /// <param name="ar"></param>
        private void UnLockVideoCallback(IAsyncResult ar)
        {
            try
            {
                participant.EndUnLockVideo(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Delegate to update the locked state of the participant (is needed because the update happens in seperate thread)
        /// </summary>
        /// <param name="o"></param>
        private delegate void UpdateLockedStateDelegate(object o);

        /// <summary>
        /// Updates the locked state of the participant
        /// </summary>
        /// <param name="o"></param>
        private void UpdateLockedState(object o)
        {
            try
            {
                labelLockedState.Text = ((bool)o).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Participant Property Changed Event.  We want to know when the LockedState is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void participant_PropertyChanged(object sender, ParticipantPropertyChangedEventArgs e)
        {
            try
            {
                //If the lockned state changes, update the locked state label.  A delegate is needed because this happens on a seperate thread
                if (e.Property == ParticipantProperty.IsLocked)
                {
                    labelLockedState.BeginInvoke(new UpdateLockedStateDelegate(this.UpdateLockedState), e.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Gets the currently selected participant from the participant listbox and subscribes to the participant property changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGetParticipant_Click(object sender, EventArgs e)
        {
            try
            {
                participant = GetParticipant();
                labelParticipantName.Text = participant.Properties[ParticipantProperty.Name].ToString();
                participant.PropertyChanged += new EventHandler<ParticipantPropertyChangedEventArgs>(participant_PropertyChanged);
                labelLockedState.BeginInvoke(new UpdateLockedStateDelegate(this.UpdateLockedState), participant.Properties[ParticipantProperty.IsLocked]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
       
    }
}
