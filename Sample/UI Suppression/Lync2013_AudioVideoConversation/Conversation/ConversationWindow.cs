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
using System.Diagnostics;
using System.Windows.Forms;

using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;

namespace AudioVideoConversation
{
    /// <summary>
    /// Implements a conversation window for the AvModality.
    /// 
    /// Each button represents one possible action that may be taken on the Conversation, 
    /// AvModality, Audio or Video channels.
    /// 
    /// The buttons state (Enabled true/false) is controlled by the ActionAvailability events.
    /// All the buttons start with Enabled false. Over the course of the conversation life-cycle
    /// the buttons will become enabled / disabled.
    /// 
    /// Most of the actions take a callback method as an argument. This is used to notify the caller 
    /// asynchronously that an operation has finished. This implementation does not use the callback
    /// option since this window only shows very simple status of the conversation state.
    /// 
    /// The state of the conversation life-cycle is obtained through StateChanged events. Those will
    /// be raised independently by the Conversation, AvModality, AudioChannel and VideoChannel objects.
    /// </summary>
    public partial class ConversationWindow : Form
    {
        #region Fields

        //holds the Lync client instance
        private LyncClient client;

        //holds the reference to the conversation associated with this window
        private Conversation conversation;

        //self participant's AvModality
        private AVModality avModality;

        //self participant's channels
        private AudioChannel audioChannel;
        private VideoChannel videoChannel;

        #endregion

        #region Constructor

        /// <summary>
        /// Initiates the window for the specified conversation.
        /// </summary>
        public ConversationWindow(Conversation conversation, LyncClient client)
        {
            InitializeComponent();

            //saves the client reference
            this.client = client;

            //saves the conversation reference
            this.conversation = conversation;

            //saves the AVModality, AudioChannel and VideoChannel, just for the sake of readability
            avModality = (AVModality) conversation.Modalities[ModalityTypes.AudioVideo];
            audioChannel = avModality.AudioChannel;
            videoChannel = avModality.VideoChannel;

            //show the current conversation and modality states in the UI
            toolStripStatusLabelConvesation.Text = conversation.State.ToString();
            toolStripStatusLabelModality.Text = avModality.State.ToString();

            //enables and disables the checkbox associated with the ConversationProperty.AutoTerminateOnIdle property
            //based on whether the Lync client is running in InSuppressedMode
            //se more details in the checkBoxAutoTerminateOnIdle_CheckStateChanged() method
            checkBoxAutoTerminateOnIdle.Enabled = client.InSuppressedMode;

            //registers for conversation state updates
            conversation.StateChanged += conversation_StateChanged;

            //registers for participant events
            conversation.ParticipantAdded += conversation_ParticipantAdded;
            conversation.ParticipantRemoved += conversation_ParticipantRemoved;

            //subscribes to the conversation action availability events (for the ability to add/remove participants)
            conversation.ActionAvailabilityChanged += conversation_ActionAvailabilityChanged;

            //subscribes to modality action availability events (all audio button except DTMF)
            avModality.ActionAvailabilityChanged += avModality_ActionAvailabilityChanged;

            //subscribes to the modality state changes so that the status bar gets updated with the new state
            avModality.ModalityStateChanged += avModality_ModalityStateChanged;

            //subscribes to the audio channel action availability events (DTMF only)
            audioChannel.ActionAvailabilityChanged += audioChannel_ActionAvailabilityChanged;

            //subscribes to the video channel state changes so that the status bar gets updated with the new state
            audioChannel.StateChanged += audioChannel_StateChanged;

            //subscribes to the video channel action availability events
            videoChannel.ActionAvailabilityChanged += videoChannel_ActionAvailabilityChanged;

            //subscribes to the video channel state changes so that the video feed can be presented
            videoChannel.StateChanged += videoChannel_StateChanged;
        }

        #endregion

        /// <summary>
        /// Ends the conversation if the user closes the window.
        /// </summary>
        private void ConversationWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            //need to remove event listeners otherwide events may be received after the form has been unloaded
            conversation.StateChanged -= conversation_StateChanged;
            conversation.ParticipantAdded -= conversation_ParticipantAdded;
            conversation.ParticipantRemoved -= conversation_ParticipantRemoved;
            conversation.ActionAvailabilityChanged -= conversation_ActionAvailabilityChanged;
            avModality.ActionAvailabilityChanged -= avModality_ActionAvailabilityChanged;
            avModality.ModalityStateChanged -= avModality_ModalityStateChanged;
            audioChannel.ActionAvailabilityChanged -= audioChannel_ActionAvailabilityChanged;
            audioChannel.StateChanged -= audioChannel_StateChanged;
            videoChannel.ActionAvailabilityChanged -= videoChannel_ActionAvailabilityChanged;
            videoChannel.StateChanged -= videoChannel_StateChanged;

            //if the conversation is active, will end it
            if (conversation.State != ConversationState.Terminated)
            {
                //ends the conversation which will disconnect all modalities
                try
                {
                    conversation.End();
                }
                catch (LyncClientException lyncClientException)
                {
                    Console.WriteLine(lyncClientException);
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
            }
        }


        #region Conversation related actions

        //*****************************************************************************************
        //                              Conversation Related Actions
        //*****************************************************************************************

        /// <summary>
        /// Changes the ConversationProperty.AutoTerminateOnIdle based on the checkBox state.
        /// </summary>
        private void checkBoxAutoTerminateOnIdle_CheckStateChanged(object sender, EventArgs e)
        {
            //reads the value of the property
            bool autoTerminateOnIdle = checkBoxAutoTerminateOnIdle.Checked;

            //*****************************************************************************************
            //                              ConversationProperty.AutoTerminateOnIdle
            //
            // By default this property will be set to false, which will cause the conversation to be
            // terminated when all modalities are disconnected (after at least one has been active).
            //
            // When the Lync client is running, there's no need to set this property since the Lync
            // user interface will set it to true.
            // 
            // If this application is running in UISuppressionMode, it needs to set this property to
            // allow the conversation to live after all modalities are disconnected.
            //
            //*****************************************************************************************

            //setting the property for Lync terminate the conversation once it's disconnected
            try
            {
                conversation.BeginSetProperty(ConversationProperty.AutoTerminateOnIdle, autoTerminateOnIdle, null, null);
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
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
        }

        /// <summary>
        /// Removes a participant from the conversation.
        /// </summary>
        private void buttonRemoveRosterContact_Click(object sender, EventArgs e)
        {
            //validates if there's a participant selected
            if (listBoxRosterContacts.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a participant (other than self).");
                return;
            }

            //get the selected participant for removal
            ParticipantItem item = listBoxRosterContacts.SelectedItem as ParticipantItem;

            //removes the participant from the conversation
            try
            {
                if (item != null)
                {
                    conversation.RemoveParticipant(item.Participant);
                }
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
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
        }

        /// <summary>
        /// Adds a participant to the conversation.
        /// </summary>
        private void buttonAddRosterContact_Click(object sender, EventArgs e)
        {
            //obtains the contact
            SelectContactDialog contactDialog = new SelectContactDialog(client.ContactManager);
            contactDialog.ShowDialog(this);

            //if a contact is selected
            if (contactDialog.DialogResult == DialogResult.OK && contactDialog.Contact != null)
            {
                //add a participant to the conversation
                try
                {
                    conversation.AddParticipant(contactDialog.Contact);
                }
                catch (LyncClientException lyncClientException)
                {
                    Console.WriteLine(lyncClientException);
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
            }
        }

        #endregion


        #region Conversation event handling

        //*****************************************************************************************
        //                              Conversation Event Handling
        //
        // The ability to add or remove a participant to/from the conversation will change over the  
        // life-cycle of the conversation. This will informed through the ActionAvailabilityChanged
        // events.
        //
        // Whenever a participant joins or exits the conversation, events ParticipantAdded or 
        // ParticipantRemoved will be raised. Those will occur either by an action taken at this
        // endpoint or any other participant in the conversation. For example, a ParticipantAdded 
        // may be fired because the other side of the conversation added a new contact to the 
        // conversation.
        //
        //*****************************************************************************************

        /// <summary>
        /// Called when the availability of an action changes.
        /// 
        /// Will Enable/Disable buttons based off the availability.
        /// </summary>
        void conversation_ActionAvailabilityChanged(object sender, ConversationActionAvailabilityEventArgs e)
        {
            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {
                //each action is mapped to a button in the UI
                switch (e.Action)
                {
                    case ConversationAction.AddParticipant:
                        buttonAddRosterContact.Enabled = e.IsAvailable;
                        break;

                    case ConversationAction.RemoveParticipant:
                        buttonRemoveRosterContact.Enabled = e.IsAvailable;
                        break;
                }

            }));
        }

        /// <summary>
        /// Called when the conversation state changes.
        /// 
        /// Updates the status bar.
        /// </summary>
        void conversation_StateChanged(object sender, ConversationStateChangedEventArgs e)
        {
            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {
                //show the current conversation state
                toolStripStatusLabelConvesation.Text = e.NewState.ToString();
            }));
        }

        /// <summary>
        /// Called when a participant is added to the conversation.
        /// 
        /// Adds the participant to the roster listbox.
        /// </summary>
        void conversation_ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
        {
            //error case, the participant wasn't actually added
            if (e.StatusCode < 0)
            {
                Console.Out.WriteLine("Participant was not added: code=" + e.StatusCode);
                return;
            }

            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {
                //creates a new item and adds to the roster listbox
                listBoxRosterContacts.Items.Add(new ParticipantItem(e.Participant));
            }));

        }

        /// <summary>
        /// Called when a participant is removed from the conversation.
        /// 
        /// Removes the participant from the roster listbox.
        /// </summary>
        void conversation_ParticipantRemoved(object sender, ParticipantCollectionChangedEventArgs e)
        {
            //error case, the participant wasn't actually removed
            if (e.StatusCode < 0)
            {
                Console.Out.WriteLine("Participant was not removed: code=" + e.StatusCode);
                return;
            }

            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {

                //finds the position of the participant to be removed in the roster listbox
                int removePosition = -1;
                for (int i = 0; i < listBoxRosterContacts.Items.Count; i++)
                {
                    ParticipantItem item = listBoxRosterContacts.Items[i] as ParticipantItem;
                    if (item != null && item.Participant.Equals(e.Participant))
                    {
                        removePosition = i;
                        break;
                    }
                }

                //removes the participant from the roster listbox
                if (removePosition > 0)
                {
                    listBoxRosterContacts.Items.RemoveAt(removePosition);
                }
            }));
        }

        #endregion


        #region Modality related actions

        //*****************************************************************************************
        //                              Modality actions
        //
        // The actions here take effect on the whole modality. If the both the Audio and Video channels
        // are active, the actions taken on the modality will affect both.
        //
        // It's important to note that Audio is the default channel for the AvModality. This means that
        // connecting the AvModality is equivalent to starting the Audio channel. The same applies for 
        // disconnecting the modality or stopping the audio channel.
        // 
        //*****************************************************************************************

        /// <summary>
        /// Connects the modality (audio): AvModality.BeginConnect()
        /// </summary>
        private void buttonConnectAudio_Click(object sender, EventArgs e)
        {
            //starts an audio call or conference by connecting the AvModality
            try
            {
                AsyncCallback callback = new AsyncOperationHandler(avModality.EndConnect).Callback;
                avModality.BeginConnect(callback, null);
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
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
        }

        /// <summary>
        /// Disconnects the modality: AvModality.BeginDisconnect()
        /// </summary>
        private void buttonDisconnectAudio_Click(object sender, EventArgs e)
        {
            //ends an audio call or conference by connecting the AvModality
            try
            {
                AsyncCallback callback = new AsyncOperationHandler(avModality.EndDisconnect).Callback;
                avModality.BeginDisconnect(ModalityDisconnectReason.None, callback, null);
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
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
        }

        /// <summary>
        /// Holds the modality (audio + video): AvModality.BeginHold()
        /// </summary>
        private void buttonHold_Click(object sender, EventArgs e)
        {
            //puts the call on hold (applies for both audio and video)
            AsyncCallback callback = new AsyncOperationHandler(avModality.EndHold).Callback;
            try
            {
                avModality.BeginHold(callback, null);
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
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
        }

        /// <summary>
        /// Retrieves the modality (audio + video): AvModality.BeginRetrieve()
        /// </summary>
        private void buttonRetrieve_Click(object sender, EventArgs e)
        {
            //retrieves a call from hold (applies for both audio and video)
            //the video state will be remembered from before the call was held
            AsyncCallback callback = new AsyncOperationHandler(avModality.EndRetrieve).Callback;
            try
            {
                avModality.BeginRetrieve(callback, null);
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
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
        }

        /// <summary>
        /// Transfers the call to a contact: AvModality.BeginTransfer()
        /// </summary>
        private void buttonTransfer_Click(object sender, EventArgs e)
        {
            //since transfering needs a contact, will show a dialog to get user input
            SelectContactDialog contactDialog = new SelectContactDialog(client.ContactManager);
            contactDialog.ShowDialog();

            //if a contact is selected
            if (contactDialog.DialogResult == DialogResult.OK)
            {
                //*****************************************************************************************
                // The argument TransferOptions dictates whether the transfer target is able to redirect the
                // call or not. If the option is TransferOptions.DisallowRedirection and the target does not
                // accept the call, it will return to this endpoint. 
                //*****************************************************************************************

                // Declare a simple handler for the async callback before invoking Transfer...
                AsyncCallback transferCallback = ar =>
                        {
                            try
                            {
                                ModalityState state;
                                IList<string> properties;
                                avModality.EndTransfer(out state, out properties, ar);
                                Console.Out.WriteLine("Transfer state: " + state);
                            }
                            catch (Exception ex)
                            {
                                Console.Out.WriteLine(ex);
                            }
                        };

                //transfers the call (both audio and video) to the specified contact
                try
                {
                    avModality.BeginTransfer(contactDialog.Contact, TransferOptions.None, transferCallback, null);
                }
                catch (LyncClientException lyncClientException)
                {
                    Console.WriteLine(lyncClientException);
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
            }
        }

        /// <summary>
        /// Transfers the call to another conversation: 
        /// </summary>
        private void buttonConsultTransfer_Click(object sender, EventArgs e)
        {
            IList<Conversation> allConversations = client.ConversationManager.Conversations;
            Debug.Assert(allConversations != null && conversation != null);

            //obtain the conversation
            ConversationPickerDialog conversationPickerDialog = new ConversationPickerDialog();
            conversationPickerDialog.ShowConversations(allConversations, conversation);
            conversationPickerDialog.ShowDialog(this);

            //if a conversation was selected
            if (conversationPickerDialog.DialogResult == DialogResult.OK && conversationPickerDialog.Conversation != null)
            {
                AsyncCallback transferCallback = ar =>
                {
                    try
                    {
                        ModalityState state;
                        IList<string> properties;
                        avModality.EndConsultativeTransfer(out state, out properties, ar);
                        Console.Out.WriteLine("Consultative Transfer state: " + state);
                    }
                    catch (Exception ex)
                    {
                        Console.Out.WriteLine(ex);
                    }
                };

                //does a consultative transfer to the selected conversation
                //only the TransferOptions.None is allowed here
                try
                {
                    avModality.BeginConsultativeTransfer(conversationPickerDialog.Conversation, TransferOptions.None, transferCallback, null);
                }
                catch (LyncClientException lyncClientException)
                {
                    Console.WriteLine(lyncClientException);
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
            }
        }

        /// <summary>
        /// Accepts an incoming call: AvModality.Accept()
        /// </summary>
        private void buttonAccept_Click(object sender, EventArgs e)
        {
            //accepts an incoming invite (syncronous operation)
            try
            {
                avModality.Accept();
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
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
        }

        /// <summary>
        /// Rejects an incoming call: AvModality.Reject()
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReject_Click(object sender, EventArgs e)
        {
            //rejects an incoming invite (which will disconnect the call)
            //the ModalityDisconnectReason may be used to specify a reason to the caller side
            //the reason may be shown on the Lync client conversation window
            try
            {
                avModality.Reject(ModalityDisconnectReason.None);
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
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
        }

        /// <summary>
        /// Forwards the call to a contact or phone number.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonForward_Click(object sender, EventArgs e)
        {
            //since forwarding needs a contact, will show a dialog to get user input
            SelectContactDialog contactDialog = new SelectContactDialog(client.ContactManager);
            contactDialog.ShowDialog();

            //if a contact is selected
            if (contactDialog.DialogResult == DialogResult.OK && contactDialog.Contact != null)
            {
                //transfers the call (both audio and video) to the specified contact
                AsyncCallback callback = new AsyncOperationHandler(avModality.EndForward).Callback;
                try
                {
                    avModality.BeginForward(contactDialog.Contact, callback, null);
                }
                catch (LyncClientException lyncClientException)
                {
                    Console.WriteLine(lyncClientException);
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
            }
        }

        #endregion


        #region Modality events handling

        //*****************************************************************************************
        //                              Modality event handling
        //*****************************************************************************************

        void avModality_ModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {
            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {
                //updates the status bar with the video channel state
                toolStripStatusLabelModality.Text = e.NewState.ToString();
            }));
            
        }

        /// <summary>
        /// Called when the availability of an action changes.
        /// 
        /// Will Enable/Disable buttons based off the availability.
        /// </summary>
        void avModality_ActionAvailabilityChanged(object sender, ModalityActionAvailabilityChangedEventArgs e)
        {
            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {

                //each action is mapped to a button in the UI
                switch (e.Action)
                {
                    case ModalityAction.Connect:
                        buttonConnectAudio.Enabled = e.IsAvailable;
                        break;

                    case ModalityAction.Disconnect:
                        buttonDisconnectAudio.Enabled = e.IsAvailable;
                        break;

                    case ModalityAction.Hold:
                        buttonHold.Enabled = e.IsAvailable;
                        break;

                    case ModalityAction.Retrieve:
                        buttonRetrieve.Enabled = e.IsAvailable;
                        break;

                    case ModalityAction.LocalTransfer:
                        buttonTransfer.Enabled = e.IsAvailable;
                        break;

                    case ModalityAction.ConsultAndTransfer:
                        buttonConsultTransfer.Enabled = e.IsAvailable;
                        break;

                    case ModalityAction.Forward:
                        buttonForward.Enabled = e.IsAvailable;
                        break;

                    case ModalityAction.Accept:
                        buttonAccept.Enabled = e.IsAvailable;
                        break;

                    case ModalityAction.Reject:
                        buttonReject.Enabled = e.IsAvailable;
                        break;
                }

            }));
        }


        #endregion


        #region Audio channel related actions

        //*****************************************************************************************
        //                              AudioChannel related actions
        //*****************************************************************************************

        /// <summary>
        /// Shows a dialog with a dial pad and sends a DTMF tone for each pressed button.
        /// 
        /// The dialog will raise events when a button is pressed.
        /// </summary>
        private void buttonSendDTMF_Click(object sender, EventArgs e)
        {
            //shows the dial pad dialog and registers for button clicks
            DialPadDialog dialpad = new DialPadDialog();
            dialpad.DialPadPressed += new DialPadPressed(dialpad_DialPadPressed);
            dialpad.ShowDialog(this);
        }

        /// <summary>
        /// Called when a button is pressed in the dial pad dialog.
        /// 
        /// Sends the DTMF tone using AudioChannel.BeginSendDtmf()
        /// </summary>
        void dialpad_DialPadPressed(string tone)
        {
            //sends a set of characters (in this case one) as a dial tone
            try
            {
                AsyncCallback callback = new AsyncOperationHandler(audioChannel.EndSendDtmf).Callback;
                audioChannel.BeginSendDtmf(tone, callback, null);
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
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
        }

        #endregion


        #region Audio channel event handling

        //*****************************************************************************************
        //                              AudioChannel event handling
        //*****************************************************************************************

        /// <summary>
        /// Updates the status bar when the new Audio Channel state.
        /// </summary>
        void audioChannel_StateChanged(object sender, ChannelStateChangedEventArgs e)
        {
            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {
                //updates the status bar with the video channel state
                toolStripStatusLabelAudioChannel.Text = e.NewState.ToString();               
            }));
        }

        /// <summary>
        /// Called when the action availability changes for the action channel.
        /// 
        /// Will Enable/Disable buttons based off the availability.
        /// </summary>
        void audioChannel_ActionAvailabilityChanged(object sender, ChannelActionAvailabilityEventArgs e)
        {
            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {

                //only SendDtmf is used here since the button are already mapped
                //to the action availability of the modality itself
                if (e.Action == ChannelAction.SendDtmf)
                {
                    buttonSendDTMF.Enabled = e.IsAvailable;
                }

            }));

        }

        #endregion


        #region Video channel related actions

        //*****************************************************************************************
        //                              VideoChannel related actions
        //
        // The video channel action will behave differently depending on whether the audio is already
        // connected.
        //
        // If audio is not connected, starting video is equivalent to connecting the modality. If the
        // conversation already has audio, starting video will start the outgoing video stream. The 
        // other participants in the conversation also need to start their own video.
        //
        // Stopping the video channel will stop both outgoing and incoming video. It will remove video
        // from the conversation.
        //
        //*****************************************************************************************

        /// <summary>
        /// Starts the video channel: VideoChannel.BeginStart()
        /// </summary>
        private void buttonStartVideo_Click(object sender, EventArgs e)
        {
            //starts a video call or the video stream in a audio call
            AsyncCallback callback = new AsyncOperationHandler(videoChannel.EndStart).Callback;
            try
            {
                videoChannel.BeginStart(callback, null);
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
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
        }

        /// <summary>
        /// Starts the video channel: VideoChannel.BeginStop()
        /// </summary>
        private void buttonStopVideo_Click(object sender, EventArgs e)
        {
            //removes video from the conversation
            AsyncCallback callback = new AsyncOperationHandler(videoChannel.EndStop).Callback;
            try
            {
                videoChannel.BeginStop(callback, null);
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
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
        }

        #endregion


        #region Video channel event handling

        //*****************************************************************************************
        //                              VideoChannel Event Handling
        //*****************************************************************************************

        /// <summary>
        /// Called when the action availability changes for the action channel.
        /// 
        /// Will Enable/Disable buttons based off the availability.
        /// </summary>
        void videoChannel_ActionAvailabilityChanged(object sender, ChannelActionAvailabilityEventArgs e)
        {
            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {

                //each action is mapped to a button in the UI
                switch (e.Action)
                {
                    case ChannelAction.Start:
                        buttonStartVideo.Enabled = e.IsAvailable;
                        break;

                    case ChannelAction.Stop:
                        buttonStopVideo.Enabled = e.IsAvailable;
                        break;
                }

            }));
        }

        /// <summary>
        /// Called when the video state changes.
        /// 
        /// Will show Incoming/Outgoing video based on the channel state.
        /// </summary>
        void videoChannel_StateChanged(object sender, ChannelStateChangedEventArgs e)
        {
            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {
                //updates the status bar with the video channel state
                toolStripStatusLabelVideoChannel.Text = e.NewState.ToString();


                //*****************************************************************************************
                //                              Video Content
                //
                // The video content is only available when the Lync client is running in UISuppressionMode.
                //
                // The video content is not directly accessible as a stream. It's rather available through
                // a video window that can de drawn in any panel or window.
                //
                // The outgoing video is accessible from videoChannel.CaptureVideoWindow
                // The window will be available when the video channel state is either Send or SendReceive.
                // 
                // The incoming video is accessible from videoChannel.RenderVideoWindow
                // The window will be available when the video channel state is either Receive or SendReceive.
                //
                //*****************************************************************************************

                //if the outgoing video is now active, show the video (which is only available in UI Suppression Mode)
                if ((e.NewState == ChannelState.Send 
                    || e.NewState == ChannelState.SendReceive) && videoChannel.CaptureVideoWindow != null)
                {
                    //presents the video in the panel
                    ShowVideo(panelOutgoingVideo, videoChannel.CaptureVideoWindow);
                }

                //if the incoming video is now active, show the video (which is only available in UI Suppression Mode)
                if ((e.NewState == ChannelState.Receive 
                    || e.NewState == ChannelState.SendReceive) && videoChannel.RenderVideoWindow != null)
                {
                    //presents the video in the panel
                    ShowVideo(panelIncomingVideo, videoChannel.RenderVideoWindow);
                }

            }));
        }

        /// <summary>
        /// Shows the specified video window in the specified panel.
        /// </summary>
        private static void ShowVideo(Panel videoPanel, VideoWindow videoWindow)
        {
            //Win32 constants:                  WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS;
            const long lEnableWindowStyles = 0x40000000L | 0x02000000L | 0x04000000L;
            //Win32 constants:                   WS_POPUP| WS_CAPTION | WS_SIZEBOX
            const long lDisableWindowStyles = 0x80000000 | 0x00C00000 | 0x00040000L;
            const int OATRUE = -1;

            try
            {
                //sets the properties required for the native video window to draw itself
                videoWindow.Owner = videoPanel.Handle.ToInt32();
                videoWindow.SetWindowPosition(0, 0, videoPanel.Width, videoPanel.Height);

                //gets the current window style to modify it
                long currentStyle = videoWindow.WindowStyle;

                //disables borders, sizebox, close button
                currentStyle = currentStyle & ~lDisableWindowStyles;

                //enables styles for a child window
                currentStyle = currentStyle | lEnableWindowStyles;

                //updates the current window style
                videoWindow.WindowStyle = (int)currentStyle;

                //updates the visibility
                videoWindow.Visible = OATRUE;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        /// <summary>
        /// Helper class to expose exceptions thrown by asynch operations
        /// </summary>
        class AsyncOperationHandler
        {
            private Action<IAsyncResult> endOperation;
            public AsyncOperationHandler(Action<IAsyncResult> endOperation)
            {
                this.endOperation = endOperation;
            }

            public void Callback(IAsyncResult ar)
            {
                try
                {
                    // Async operations can throw exceptions.
                    // Generally, these exceptions should be handled gracefully.
                    // For the purpose of illustration, we will simply log any issues.
                    endOperation(ar);
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e);
                }
            }
        }


    }
}
