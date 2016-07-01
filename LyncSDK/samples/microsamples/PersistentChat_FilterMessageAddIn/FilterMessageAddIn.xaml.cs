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
using System.Windows.Controls;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Room;

namespace FilterMessageAddIn
{
    public partial class FilterMessageAddIn : UserControl
    {
        /// <summary>
        /// The hosting group chat room
        /// </summary>
        Room _HostingRoom;

        /// <summary>
        /// Class constructor
        /// </summary>
        public FilterMessageAddIn()
        {
            InitializeComponent();

            //Get the group chat room that is hosting this add-in
            _HostingRoom = LyncClient.GetHostingRoom();
            if (_HostingRoom != null)
            {
                //If the hosting room outgoing message filter is not enabled then enable it.
                if (_HostingRoom.IsOutgoingMessageFilterEnabled == false)
                {
                    _HostingRoom.EnableOutgoingMessageFilter();
                }

                //Register for outgoing message post events.
                _HostingRoom.IsSendingMessage += new EventHandler<RoomMessageEventArgs>(_HostingRoom_IsSendingMessage);
            }
        }


        /// <summary>
        /// Invoked when a local user is posting a message to the hosting chat room
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _HostingRoom_IsSendingMessage(object sender, RoomMessageEventArgs e)
        {

            /// <summary>
            /// The message filtering action taken by the application after filter/format logic is run
            /// </summary>
            RoomMessageFilteringAction messageAction;


            //Get the pending message
            RoomMessage messageToFilter = e.Message;

            //Extract the plain-text version of the message.
            string pendingPostText = e.Message.MessageDictionary[RoomMessageFormat.PlainText].ToString();
             
            //Update UI with the text of the pending message.
            this.Dispatcher.BeginInvoke(new ShowPendingMessageDelegate(ShowPendingMessage), new object[] { pendingPostText });

            //Check message string to see if the message contains the sub-string entered on the add-in UI
            if (FilterMessagePost(pendingPostText, FilterTextString.Text))
            {
                //The message string contains the sub-string so the message action is canceled. 
                //Update the UI with the message filtering action and do not update the UpdatedMessage text block.
                this.Dispatcher.BeginInvoke(new ShowFilterMessageActionDelegate(ShowFilterMessageAction), new object[] { RoomMessageFilteringAction.Canceled });
                messageAction = RoomMessageFilteringAction.Canceled;
            }
            else
            {

                //The message string DOES NOT contain the filter sub-string.

                //If the pending message starts with "do not reformat" in any letter case then 
                //update the UI with the message filter action, update the UI updated message box with the original message text,
                //and set the message filtering action to Passed
                if (pendingPostText.ToUpper().StartsWith("DO NOT REFORMAT:"))
                {
                    this.Dispatcher.BeginInvoke(new ShowUpdatedMessageDelegate(ShowUpdatedMessage), new object[] { pendingPostText });
                    this.Dispatcher.BeginInvoke(new ShowFilterMessageActionDelegate(ShowFilterMessageAction), new object[] { RoomMessageFilteringAction.Passed });
                    messageAction = RoomMessageFilteringAction.Passed;

                }
                else
                {
                    //Reformat the original message text by setting all characters to upper case.
                    //Update the UI with the message filter action, update the UI updated message box with the reformatted message text
                    //and set the message filtering action to Replaced
                    this.Dispatcher.BeginInvoke(new ShowUpdatedMessageDelegate(ShowUpdatedMessage), new object[] { FormatMessagePost(pendingPostText) });
                    this.Dispatcher.BeginInvoke(new ShowFilterMessageActionDelegate(ShowFilterMessageAction), new object[] { RoomMessageFilteringAction.Replaced });
                    messageAction = RoomMessageFilteringAction.Replaced;
                }
            }

            //Update the pending message with the updated message text UI text block
            messageToFilter.MessageDictionary[RoomMessageFormat.PlainText] = UpdatedPost.Text;

            //Send the filtered message with whatever message filtering action was set in the previous code.
            _HostingRoom.SendFilteredMessage(messageToFilter, messageAction);
        }


        /// <summary>
        /// Message filter method searches the pending message text for the sub-string entered
        /// on the main UI. If the substring is found, true is returned.  Otherwse, false is returned.
        /// true = message is filtered and must be canceled.
        /// false = message passes filter and can be posted.
        /// </summary>
        /// <param name="pendingMessagString"></param>
        /// <returns></returns>
        private Boolean FilterMessagePost(string pendingMessagString, string messageFilter)
        {
            if (pendingMessagString.Contains(messageFilter))
            {
                //Cancel message.
                return true;
            }
            //Post message.
            return false;
        }
        
        /// <summary>
        /// Message text formatter. sets all message text to upper case.
        /// This simplistic example works well for plain-text messages. If your application will 
        /// format rtf or Html messages, be sure to apply equivalent formatting logic to all formats of a message's text before 
        /// posting.
        /// </summary>
        /// <param name="pendingMessageString"></param>
        /// <returns></returns>
        private string FormatMessagePost(string pendingMessageString)
        {
            return pendingMessageString.ToUpper();
        }


        private delegate void ShowPendingMessageDelegate(string pendingMessage);

        /// <summary>
        /// Updates the main UI pending message block
        /// </summary>
        /// <param name="pendingMessage"></param>
        private void ShowPendingMessage(string pendingMessage)
        {
            PendingPost.Text = pendingMessage;
        }

        private delegate void ShowUpdatedMessageDelegate(string updatedMessage);
        /// <summary>
        /// Updates the main UI updated message text block with the reformatted message text.
        /// </summary>
        /// <param name="updatedMessage"></param>
        private void ShowUpdatedMessage(string updatedMessage)
        {
            UpdatedPost.Text = updatedMessage;
        }

        private delegate void ShowFilterMessageActionDelegate(RoomMessageFilteringAction action);
        /// <summary>
        /// Updates the main UI filter action label content with the action to be applied to the
        /// pending message post.
        /// </summary>
        /// <param name="action"></param>
        private void ShowFilterMessageAction(RoomMessageFilteringAction action)
        {
            FilterAction.Content = action.ToString();
        }
      
    }
}
