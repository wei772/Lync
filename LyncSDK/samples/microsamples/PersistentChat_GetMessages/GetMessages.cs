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
using System.Windows.Forms;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Room;
using System.Collections;

namespace GetMessages
{
    public partial class GetMessages : Form
    {

        /// <summary>
        /// The LyncClient class instance that encapsulates the Lync client platform
        /// </summary>
        private LyncClient _client=null;

        /// <summary>
        /// The Room instance that a user joins from the list of rooms that are queried, searched for, or gotten from the followed rooms list
        /// </summary>
        private Room _room = null;

        /// <summary>
        /// A Dictionary of Room instances that represent the chat rooms which a user is following.
        /// </summary>
        private Dictionary<string, Room> _followedRoomsList = new Dictionary<string, Room>();

        /// <summary>
        /// the number of messages that have been sent to a room which have not been read by a user
        /// </summary>
        private uint _unreadMessageCount = 0;

        /// <summary>
        /// Simple chat window constructor
        /// </summary>
        public GetMessages()
        {
            InitializeComponent();

          
        }


        private void GetMessages_Load(object sender, EventArgs e)
        {
            
            try
            {
                //Get the client. The entry point to all further sample application functionality.
                _client = LyncClient.GetClient();

                if (_client.RoomManager.State == RoomManagerState.Disabled)
                {
                    MessageBox.Show("Persistent Chat Server is not reachable. Quitting");
                }

                //Register for state change on the client (sign in, sign out)
                _client.StateChanged += new EventHandler<ClientStateChangedEventArgs>(client_StateChanged);

                //Register for accidental loss of network connectivity event
                _client.ClientDisconnected += new EventHandler(client_ClientDisconnected);

                //If the user is signed in to Lync, get the group chat rooms that are on the Lync contact list and register
                //for events raised when a user adds another room to the contact list or removes a room from the contact list.
                if (_client.State == ClientState.SignedIn)
                {
                    //Check the state of the room manager. If the room manager is not enabled, then the Lync client is not signed in.
                    //Room manager is enabled by the platform after the Lync client has signed in
                    if (_client.RoomManager.State == RoomManagerState.Enabled && _client.RoomManager.FollowedRooms != null)
                    {
                        //Load all followed rooms that are currently in the users contact list.
                        LoadInitialFollowedRoomList();

                        //Register for event that is raised when another room is added to the user's contact list.
                        _client.RoomManager.FollowedRoomAdded += RoomManager_FollowedRoomAdded;

                        //Register for event that is raised when a room is removed from the user's contact list.
                        _client.RoomManager.FollowedRoomRemoved += RoomManager_FollowedRoomRemoved;
                    }
                }
                RefreshSelectedRoomProperties();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        #region Followed Room related Methods
        /// <summary>
        /// Room removed event handler. Removes a room from the followed room list box on the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoomManager_FollowedRoomRemoved(object sender, FollowedRoomsChangedEventArgs e)
        {
            this.Invoke(new UpdateFollowedRoomListDelegate(RemoveAFollowedRoomFromList), new object[] { e.Room });
        }

        /// <summary>
        /// Room added to contact list event handler. Adds a room to the followed room list box on the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoomManager_FollowedRoomAdded(object sender, FollowedRoomsChangedEventArgs e)
        {
            this.Invoke(new UpdateFollowedRoomListDelegate(AddAFollowedRoomToList), new object[] { e.Room });
        }

        private delegate void UpdateFollowedRoomListDelegate(Room roomToAdd);
        private void AddAFollowedRoomToList(Room roomToAdd)
        {

            //Add a room to the Dictionary<string,Microsoft.Lync.Model.Room.Room>() 
            //Dictionary entry key is room title and value is room instance
            _followedRoomsList.Add(roomToAdd.Properties[RoomProperty.Title].ToString(), roomToAdd);

            //Add a followed room title to the UI list box of followed room titles.
            //User selects a room title from the list box and the corresponding Room instance is obtained
            //from the _followedRoomsList dictonary.
            FollowedRooms_listbox.Items.Add(roomToAdd.Properties[RoomProperty.Title].ToString());

        }

        /// <summary>
        /// Removes a room from the followed room list on the UI
        /// </summary>
        /// <param name="roomToRemove"></param>
        private void RemoveAFollowedRoomFromList(Room roomToRemove)
        {
            //If the followed room is in the followed room dictionary, remove the room from
            //both the dictionary and the followed room list on the UI
            if (_followedRoomsList.ContainsKey(roomToRemove.Properties[RoomProperty.Title].ToString()))
            {
                _followedRoomsList.Remove(roomToRemove.Properties[RoomProperty.Title].ToString());
                FollowedRooms_listbox.Items.Remove(roomToRemove.Properties[RoomProperty.Title].ToString());
            }
        }

        /// <summary>
        /// Loads a UI list box with the room title strings of all group chat rooms that
        /// the user has "followed"... added to the contact list.
        /// These followed rooms are locally cached by the Lync client platform.
        /// </summary>
        private void LoadInitialFollowedRoomList()
        {
            try
            {
                //Update a UI label with the current state of the Lync client platform (signed out, signing in, signed in... etc)
                LyncClientState_Label.Text = _client.State.ToString();

                //Check the state of the room manager. If the room manager is not enabled, then the Lync client is not signed in.
                //Room manager is enabled by the platform after the Lync client has signed in
                if (_client.RoomManager.State == RoomManagerState.Enabled && _client.RoomManager.FollowedRooms != null)
                {

                    //Iterate on the collection of Room instances that are followed by the user
                    foreach (Room followedRoom in _client.RoomManager.FollowedRooms)
                    {
                        //Add a room to the followed room UI list
                        AddAFollowedRoomToList(followedRoom);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Load Followed Room List Exception: " + ex.Message);
            }

        }

        /// <summary>
        /// Handles the event raised when a user selects a followed room from the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FollowedRoomsList_SelectedValueChanged(object sender, EventArgs e)
        {
            ListBox box = (ListBox)sender;
            Room selectedRoom;

            //Make sure the user has selected a room.
            if (box.SelectedItem != null)
            {
                //Get the Room object from the followed room dictionary that is filled when the form is opened.
                if (_followedRoomsList.TryGetValue(box.SelectedItem.ToString(), out selectedRoom))
                {
                    //Clear the room properties and unregister for events on the previously chosen room.
                    ClearRoomProperties(_room);

                    //Fill the room properties and register for events on the currently chosen room.
                    FillRoomProperties(selectedRoom);
                }
            }
        }

        #endregion

        #region Message Related methods
        /// <summary>
        /// Gets a set of previously posted messages.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRetrieveAdditionalMessages_Click(object sender, EventArgs e)
        {
            try
            {
                //Empty the message list box of all messages
                listBoxMessages.Items.Clear();

                if (_room != null)
                {

                    //A user can get messages from a room if they are a member of the room as defined by the room managers. 
                    //User must join a room to get messages from the room.

                    //If not joined to the room, attempt to join the room.
                    if (_room.JoinedState != RoomJoinState.Success)
                    {
                        _room.EndJoin(_room.BeginJoin(null, null));
                    }

                    //If joined to the room then request messages posted to the room.
                    if (_room.JoinedState == RoomJoinState.Success)
                    {
                        //Get the number of messages to return
                        uint count = Convert.ToUInt32(NumberOfMessagesToGet_Text.Text);

                        //Declare field that holds the id of the oldest message currently displayed
                        uint OldestMessageIdDisplayed = 0;

                        try
                        {
                            //If a non-numeric string is held in this text field, a FormatException is raised
                            //when the Convert method is called.
                            OldestMessageIdDisplayed = Convert.ToUInt32(txtLastMessageID.Text);
                        }
                        catch (FormatException) { }


                        //If an oldest current message Id is specified then get a set of messages that are older than the oldest current message.
                        if (OldestMessageIdDisplayed != 0)
                        {
                            //Retrieve messages with Ids up to (but not including) the message Id specified in the first argument
                            _room.BeginRetrieveAdditionalMessages(OldestMessageIdDisplayed, count, GetMessagesCallback, "Additional");
                        }
                        else
                        {
                            //Retrieve messages in chronological order from newest to oldest up to the number of messages specified in the first argument.
                            _room.BeginRetrieveLatestMessages(count, GetMessagesCallback, "Latest");
                        }
                    }

                }
            }
            catch (JoinRoomUnauthorizedException)
            {
                MessageBox.Show("Not authorized to join room");
            }
            catch (JoinRoomFailException)
            {
                MessageBox.Show("Failed to join the room");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Invoked by platform thread when the message retrieve operation is completed.
        /// </summary>
        /// <param name="ar"></param>
        private void GetMessagesCallback(IAsyncResult ar)
        {

            //Declare a list to contain the collection of room messages returned by the operation
            IList<RoomMessage> messages;
            try
            {

                //Call the EndRetrieveXXXMessages operation that corresponds to the
                //BeginRetrieveXXXMessges method call that started the operation
                if (ar.AsyncState.ToString() == "Latest")
                {
                    //Get the collection of the latest messages sent to the room
                    messages = _room.EndRetrieveLatestMessages(ar);
                }
                else
                {
                    //Get the collection of messages up to the specified message
                    messages = _room.EndRetrieveAdditionalMessages(ar);
                }

                //Invoke a delegate on the UI thread that loads the message collection in the message list box.                
                listBoxMessages.Invoke(new LoadPostedMessagesDelegate(LoadPostedMessages), new object[] { messages, false });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception on EndRetrieveXXMessages " + ex.Message);
            }
        }

        private delegate void LoadPostedMessagesDelegate(IList<RoomMessage> messages, Boolean invokeByMessagesReceivedEvent);
        /// <summary>
        /// Loads a list box with a collection of messages
        /// </summary>
        /// <param name="messages"></param>
        private void LoadPostedMessages(IList<RoomMessage> messages, Boolean invokeByMessagesReceivedEvent)
        {
            if (invokeByMessagesReceivedEvent == false)
            {
                //Add a header row to the list box with colunm names separated by TAB characters.
                listBoxMessages.Items.Add("Messages retrieved: " + messages.Count.ToString());
                listBoxMessages.Items.Add("Id\tSent Time\t\tSender");

                //Store the message id of the oldest message in this collection. Subsequent calls to
                //BeginGetAdditionalMessages() uses this Id to specify the oldest message that is already
                //displayed in the message list.
                if (messages.Count > 0)
                {
                    txtLastMessageID.Text = messages[0].Id.ToString();
                }
            }
            //Declare Contact object to be initialized with the object returned from GetContactByUri
            Contact senderContact;


            foreach (RoomMessage message in messages)
            {
                object messageObject = null;

                //Get the sender as a Contact instance
                senderContact = _client.ContactManager.GetContactByUri(message.SenderUri);

                //Get the display name of the message sender
                string senderName = senderContact.GetContactInformation(ContactInformationType.DisplayName).ToString();

                //Get the plain text version of a message
                if (message.MessageDictionary.TryGetValue(RoomMessageFormat.PlainText, out messageObject))
                {
                    string messageContent = (string)messageObject;

                    //create an item in the list box constructed of message sent time - TAB character - message type - sender display name
                    listBoxMessages.Items.Add(message.Id.ToString() +
                        "\t" + message.SentTime.ToString() +
                        "\t" + senderName +
                        "\t" + messageContent);
                }
                messageObject = null;
            }
        }


        /// <summary>
        /// Room event handler for the UnreadRoomMessageCount changed event. Raised when a user posts a message to a room. 
        /// This event handler can be used to cause your application to request the most recent messages.
        /// In this case, the event handler just updates a UI label that displays the number of unread messages.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void room_UnreadMessageCountChanged(object sender, UnreadMessageCountChangedEventArgs e)
        {
            uint count = e.NewUnreadMessageCount;

            txtUnreadMessageCountChanged.Invoke(new UpdateUnreadMessageCountDelegate(this.UpdateUnreadMessageCount), count);

        }

        private delegate void UpdateUnreadMessageCountDelegate(uint count);
        /// <summary>
        /// Updates a UI label with the current number of unread messages
        /// </summary>
        /// <param name="count"></param>
        private void UpdateUnreadMessageCount(uint count)
        {
            txtUnreadMessageCountChanged.Text = count.ToString();
        }



        /// <summary>
        /// This event is raised when any room member has posted a message to the room. This includes
        /// the local user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void room_MessagesReceived(object sender, RoomMessagesEventArgs e)
        {

            //Get the collection of newly posted messages.
            IList<RoomMessage> receivedMessages = e.Messages;

            //Invoke a delegate on the UI thread that updates the message list with the contents of the new message.
            listBoxMessages.Invoke(new LoadPostedMessagesDelegate(LoadPostedMessages), new object[] { receivedMessages, true });

        }

        #endregion

        #region Room related methods
        private delegate void RefreshObjectsDelegate();
        /// <summary>
        /// Updates the client state label, unread message count, and message list
        /// when the state of the client changes.
        /// </summary>
        public void RefreshSelectedRoomProperties()
        {
            LyncClientState_Label.Text = _client.State.ToString();
            if (_client.State != ClientState.SignedIn)
            {
                FollowedRooms_listbox.Items.Clear();
                _followedRoomsList.Clear();
            }
            ClearRoomProperties(_room);
            if (_room != null)
            {
                FillRoomProperties(_room);
            }

            if (_room == null)
            {
                RoomDescription_label.Text = "null";
                txtUnreadMessageCountChanged.Text = "null";
            }
        }



        /// <summary>
        /// Clears the messagebox and un-registers for events on a selected room
        /// </summary>
        /// <param name="room"></param>
        private void ClearRoomProperties(Room room)
        {
            //Clear the messages from a previous room choice out of the message listbox
            listBoxMessages.Items.Clear();

            if (room == null)
            {
                return;
            }

            //Register for event raised when a new message is posted to the room or the user has read a message.
            room.UnreadMessageCountChanged -= room_UnreadMessageCountChanged;

            //Register for the event raised when users have posted messages to the room.
            room.MessagesReceived -= room_MessagesReceived;
        }

        /// <summary>
        /// Registers for message, state, and participant related events on a room.
        /// </summary>
        private void FillRoomProperties(Room room)
        {

            //If the LyncClient is signed in then any Room object is valid and can be queried for properties. 
            //If LyncClient is signed out, you may have a non-null Room object, but it's properties are null.
            if (_client.State == ClientState.SignedIn)
            {
                _room = room;

                //Register for event raised when a new message is posted to the room or the user has read a message.
                _room.UnreadMessageCountChanged += room_UnreadMessageCountChanged;

                //Register for the event raised when users have posted messages to the room.
                _room.MessagesReceived += room_MessagesReceived;

                if (_room.Properties[RoomProperty.Description] != null)
                {
                    RoomDescription_label.Text = _room.Properties[RoomProperty.Description].ToString();
                }

                txtUnreadMessageCountChanged.Text = _room.UnreadRoomMessageCount.ToString();

                uint numberOfMessagesToGet = 0;
                try
                {
                    numberOfMessagesToGet = Convert.ToUInt32(NumberOfMessagesToGet_Text.Text);
                }
                catch (FormatException) { }

                //Get the most recent messages.
                _room.BeginRetrieveLatestMessages(numberOfMessagesToGet, GetMessagesCallback, "Latest");

            }
        }

        #endregion

        #region Client related methods
        public void client_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            if (_client.State != ClientState.SignedIn)
            {
                _room = null;
            }
            this.Invoke(new RefreshObjectsDelegate(RefreshSelectedRoomProperties));
        }

        public void client_ClientDisconnected(object sender, EventArgs e)
        {
            this.Invoke(new RefreshObjectsDelegate(RefreshSelectedRoomProperties));
        }
        #endregion

    }
}
