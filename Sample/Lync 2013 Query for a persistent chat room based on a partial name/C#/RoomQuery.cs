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

namespace RoomQuery
{
    public partial class RoomQuery : Form
    {

        /// <summary>
        /// The LyncClient class instance that encapsulates the Lync client platform
        /// </summary>
        private LyncClient _client=null;

        /// <summary>
        /// A dictionary of all Room instances that are returned from a query for rooms whose title matches (or partially matches)
        /// a query string provided by the user.
        /// </summary>
        private Dictionary<string, Room> _roomQueryResults = new Dictionary<string, Room>();

        /// <summary>
        /// Simple chat window constructor
        /// </summary>
        public RoomQuery()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Form constructor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoomQuery_Load(object sender, EventArgs e)
        {
            
            try
            {
                //Get the API entry point
                _client = LyncClient.GetClient();

                //Register for the state changed event on the lync client platform.
                _client.StateChanged += new EventHandler<ClientStateChangedEventArgs>(client_StateChanged);
                _client.ClientDisconnected += new EventHandler(client_ClientDisconnected);

                //Set the enable state of the query start button based on the state of the RoomManager.
                EnableDisableStartQueryButton();

                //Register for room manager state event. If room manager is disabled, disable the "Go" button on the UI so that
                //a user does not attempt to query the room manager for a room.
                if (_client.RoomManager.State == RoomManagerState.Disabled)
                {
                    MessageBox.Show("Persistent Chat Server is not reachable. Quitting");
                }
                _client.RoomManager.RoomManagerStateChanged += roomManager_RoomManagerStateChanged;

                //Display the current sign in state of the Lync client.
                RefreshClientStateLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            
        }

        /// <summary>
        /// Handles the event raised if the state of the RoomManager changes.
        /// Response to state change i
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void roomManager_RoomManagerStateChanged(object sender, RoomManagerStateChangedEventArgs e)
        {
            //Invoke a delegate on UI thread to enable or disable room query start button
            StartQuery_Button.Invoke(new EnableDisableStartQueryButtonDelegate(EnableDisableStartQueryButton));
        }


        private delegate void EnableDisableStartQueryButtonDelegate();

        
        /// <summary>
        /// Enables or disables the room query button based on the current state of the RoomManager
        /// </summary>
        private void EnableDisableStartQueryButton()
        {
            if (_client.RoomManager.State == RoomManagerState.Enabled)
            {
                StartQuery_Button.Enabled = true;
            }
            else
            {
                StartQuery_Button.Enabled = false;
            }
        }


        /// <summary>
        /// Handles the client state change event
        /// Updates the client state label on the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void client_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            //Invoke delegate on UI thread that updates client state label
            this.Invoke(new RefreshClientStateLabelDelegate(RefreshClientStateLabel));

            //Invoke a delegate on UI thread to enable or disable room query start button
            StartQuery_Button.Invoke(new EnableDisableStartQueryButtonDelegate(EnableDisableStartQueryButton));

        }

        public void client_ClientDisconnected(object sender, EventArgs e)
        {
            //Invoke delegate on UI thread that updates client state label
            this.Invoke(new RefreshClientStateLabelDelegate(RefreshClientStateLabel));
        }

        private delegate void RefreshClientStateLabelDelegate();
        /// <summary>
        /// Updates client state label based on current state of the client
        /// </summary>
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

        /// <summary>
        /// Handles the click event on the StartRoomQuery button
        /// If the RoomManager.State is not RoomManagerState.Enbled, the application logic
        /// disables the StartRoomQuery button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartQuery_Button_Click(object sender, EventArgs e)
        {
            //Clear the current contents of the room listbox
             QueriedRooms_ListBox.Items.Clear();

            //Start asynchronous room query operation with the room name or partial room name 
            //entered by the user.
             _client.RoomManager.BeginQueryRooms(RoomQueryString_TextBox.Text, RoomSearchModeType.Regular, RoomQueryCallback, null);
        }


        /// <summary>
        /// Invoked by the platform when the room query operation finishes. 
        /// </summary>
        /// <param name="ar"></param>
        private void RoomQueryCallback(System.IAsyncResult ar)
        {
            //Get the results of the query.
           IList<Room> roomResults = _client.RoomManager.EndQueryRooms(ar);

            //Load the resulting room collection into the room list on the UI
           foreach (Room r in roomResults)
           {
               //If this room is not already in the results dictionary then add it to the dictionary.
               if (!_roomQueryResults.ContainsKey(r.Properties[RoomProperty.Title].ToString()))
               {
                   //Room title and room are key/value pair. Added to dictionary to be retrieved based on 
                   //room title when user selects a room title from the UI list.
                   _roomQueryResults.Add(r.Properties[RoomProperty.Title].ToString(), r);

                   //Invoke a delegate on the UI thread to add a room to the room list on the UI
                   QueriedRooms_ListBox.Invoke(new AddToQueriedRoomsListDelegate(this.AddToQueriedRoomsList), r.Properties[RoomProperty.Title].ToString());
               }
           }
        }

        private delegate void AddToQueriedRoomsListDelegate(string roomTitle);

        /// <summary>
        /// Adds the title of a room to the room list on the UI
        /// </summary>
        /// <param name="roomTitle"></param>
        private void AddToQueriedRoomsList(string roomTitle)
        {
            QueriedRooms_ListBox.Items.Add(roomTitle);
        }
    }
}
