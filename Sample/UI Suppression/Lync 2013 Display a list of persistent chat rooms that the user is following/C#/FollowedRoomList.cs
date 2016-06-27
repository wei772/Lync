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

namespace FollowedRoomList
{
    public partial class FollowedRoomList : Form
    {

        /// <summary>
        /// The LyncClient class instance that encapsulates the Lync client platform
        /// </summary>
        private LyncClient _client=null;

        /// <summary>
        /// A Dictionary of Room instances that represent the chat rooms which a user is following.
        /// </summary>
        private Dictionary<string, Room> _followedRoomsList = new Dictionary<string, Room>();

        /// <summary>
        /// Simple chat window constructor
        /// </summary>
        public FollowedRoomList()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Main form load event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FollowedRoomList_Load(object sender, EventArgs e)
        {
            try
            {
                //Get the lync client platform, the entry point for all group chat room related code.
                _client = LyncClient.GetClient();

                if (_client.RoomManager.State == RoomManagerState.Disabled)
                {
                    MessageBox.Show("Persistent Chat Server is not reachable. Quitting");
                }

                //Register for sign in/sign out events on the client
                _client.StateChanged += new EventHandler<ClientStateChangedEventArgs>(client_StateChanged);

                //Register for an unexpected network disconnect event
                _client.ClientDisconnected += new EventHandler(client_ClientDisconnected);

                //Register for events on the followed room list
                _client.RoomManager.FollowedRoomAdded += new EventHandler<FollowedRoomsChangedEventArgs>(roomManager_FollowedRoomAdded);
                _client.RoomManager.FollowedRoomRemoved += new EventHandler<FollowedRoomsChangedEventArgs>(roomManager_FollowedRoomRemoved);

                //Register for a room manager state changed (enabled/disabled) event.
                _client.RoomManager.RoomManagerStateChanged += new EventHandler<RoomManagerStateChangedEventArgs>(roomManager_RoomManagerStateChanged);

                //If the client is signed in then load the followed room list
                if (_client.State == ClientState.SignedIn)
                {
                     LoadInitialFollowedRoomList();
                }

                //Set the lync client state label to show the current state of the client
                RefreshClientStateLabel();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                if (_client.RoomManager.State == RoomManagerState.Enabled &&  _client.RoomManager.FollowedRooms != null)
                {

                    //Iterate on the collection of Room instances that are followed by the user
                    foreach (Room followedRoom in _client.RoomManager.FollowedRooms)
                    {
                        //Add a room to the Dictionary<string,Microsoft.Lync.Model.Room.Room>() 
                        //Dictionary entry key is room title and value is room instance
                        _followedRoomsList.Add(followedRoom.Properties[RoomProperty.Title].ToString(), followedRoom);

                        //Add a followed room title to the UI list box of followed room titles.
                        //User selects a room title from the list box and the corresponding Room instance is obtained
                        //from the _followedRoomsList dictonary.
                        FollowedRooms_listbox.Items.Add(followedRoom.Properties[RoomProperty.Title].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Handles the event raised when the room manager is enabled or disabled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void roomManager_RoomManagerStateChanged(object sender, RoomManagerStateChangedEventArgs e)
        {
           Console.WriteLine("Room Manager state changed. New state is: " + e.NewState.ToString() );
        }

        /// <summary>
        /// Handles the event raised when a user removes a chat room from the contact list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void roomManager_FollowedRoomRemoved(object sender, FollowedRoomsChangedEventArgs e)
        {
            //Invoke a delegate on the UI thread to re-fill the followed rooms list
            FollowedRooms_listbox.Invoke(new UpdatedFollowedRoomListDelegate(this.UpdateFollowedRoomList), e.Room);
        }

        /// <summary>
        /// Handles the event raised when a user adds a chat room to the contact list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void roomManager_FollowedRoomAdded(object sender, FollowedRoomsChangedEventArgs e)
        {
            //Invoke a delegate on the UI thread to re-fill the followed rooms list
            FollowedRooms_listbox.Invoke(new UpdatedFollowedRoomListDelegate(this.UpdateFollowedRoomList), e.Room);
        }


        //Delegate that is invoked by the Room Manager followed room collection events
        private delegate void UpdatedFollowedRoomListDelegate(Room followedRoom);

        /// <summary>
        /// Clears and re-fills the followed room list.
        /// This helper method is called when a room is added to the contact list or when a room is removed
        /// from the contact list.
        /// 
        /// If called on FollowedRoomAdded event, then the room is not in the list box and must
        /// be added to the list box.
        /// 
        /// If called on FollowedRoomRemoved event, then the room is in the list box and must be
        /// removed from the list box
        /// </summary>
        /// <param name="followedRoom"></param>
        private void UpdateFollowedRoomList(Room followedRoom)
        {
            if (_client.State == ClientState.SignedIn)
            {
                //If the followed room list box DOES NOT contain the title of the room in param 1 then 
                if (!FollowedRooms_listbox.Items.Contains(followedRoom.Properties[RoomProperty.Title].ToString()))
                {
                    //Add the room to the list box
                    FollowedRooms_listbox.Items.Add(followedRoom.Properties[RoomProperty.Title].ToString());

                    //Add the room title/room to the dictionary as key/value
                    _followedRoomsList.Add(followedRoom.Properties[RoomProperty.Title].ToString(), followedRoom);
                }

                 //The followed room IS IN the list box already
                else
                {
                    //Remove the room from the list box
                    FollowedRooms_listbox.Items.Remove(followedRoom.Properties[RoomProperty.Title].ToString());

                    //Remove the room from the dictionary
                    _followedRoomsList.Remove(followedRoom.Properties[RoomProperty.Title].ToString());
                }
            }
            else
            {
                //Client is signed out and the contact list is empty. Therefore, clear the followed room list
                FollowedRooms_listbox.Items.Clear();
                _followedRoomsList.Clear();
            }
        }


        /// <summary>
        /// Handles the client state change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void client_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            //If the client has signed out then un-register for events on the room manager
            if (e.NewState == ClientState.SignedOut)
            {
                _client.RoomManager.FollowedRoomAdded -= roomManager_FollowedRoomAdded;
                _client.RoomManager.FollowedRoomRemoved -= roomManager_FollowedRoomRemoved;
                _client.RoomManager.RoomManagerStateChanged -= roomManager_RoomManagerStateChanged;
            }

            //If the client has signed in then register for room mananger events
            if (e.NewState == ClientState.SignedIn)
            {
                _client.RoomManager.FollowedRoomAdded += roomManager_FollowedRoomAdded;
                _client.RoomManager.FollowedRoomRemoved += roomManager_FollowedRoomRemoved;
                _client.RoomManager.RoomManagerStateChanged += roomManager_RoomManagerStateChanged;
            }

            //Update the form label with the current state of the client
            RefreshClientStateLabel();
        }

        public void client_ClientDisconnected(object sender, EventArgs e)
        {
            RefreshClientStateLabel();
        }

        public void RefreshClientStateLabel()
        {
            if (_client == null)
            {
                LyncClientState_Label.Text = "Null";
            }
            else
            {
                LyncClientState_Label.Text = _client.State.ToString();

            }
        }


        private void lstFollowedRooms_SelectedValueChanged(object sender, EventArgs e)
        {
            ListBox box = (ListBox)sender;
            Room selectedRoom;
            if (box.SelectedItem != null)
            {
                //Get the selected room from the Dictionary<string,Room> object
                if (_followedRoomsList.TryGetValue(box.SelectedItem.ToString(), out selectedRoom))
                {
                    Console.WriteLine(selectedRoom.Properties[RoomProperty.Title].ToString() + " is selected");

                }
            }
        }
    }
}
