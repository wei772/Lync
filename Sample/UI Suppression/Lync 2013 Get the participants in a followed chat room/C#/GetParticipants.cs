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
using System.Windows.Forms;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Room;

namespace GetParticipants
{
    public partial class GetParticipants : Form
    {

        /// <summary>
        /// The LyncClient class instance that encapsulates the Lync client platform
        /// </summary>
        private LyncClient _client=null;

        /// <summary>
        /// The Room selected by a user when a numeric value changes on the NumericUpDown control
        /// </summary>
        private Room _FollowedRoom;

        /// <summary>
        /// Simple chat window constructor
        /// </summary>
        public GetParticipants()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Main form load event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetParticipants_Load(object sender, EventArgs e)
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

                //If the client is signed in then load the followed room list
                if (_client.State == ClientState.SignedIn)
                {
                    //Set the numeric updown control maximum property to the number of 
                    //followed rooms in the followed room collection on the room manager.
                    FollowedRoom_Numeric.Maximum = _client.RoomManager.FollowedRooms.Count;
                    FollowedRoom_Numeric.Minimum = 1;
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
        /// Handles the client state change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void client_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            //If the client has signed out then un-register for events on the room manager
            if (e.NewState == ClientState.SignedOut)
            {
                //Refresh the participant list with zero participants
                this.Invoke(new RefreshParticipantListDelegate(RefreshParticipantList));
            }

            //If the client has signed in then register for room mananger events
            if (e.NewState == ClientState.SignedIn)
            {

                //Set the NumericUpdown maximum to the count of rooms in the followed room list
                FollowedRoom_Numeric.Maximum = _client.RoomManager.FollowedRooms.Count;
                FollowedRoom_Numeric.Minimum = 1;
            }

            //Update the form label with the current state of the client
            this.Invoke(new RefreshClientStateLabelDelegate(RefreshClientStateLabel));
        }


        private delegate void RefreshClientStateLabelDelegate();


        /// <summary>
        /// Updates the client state label on the form to the current state of the client
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
        /// Handles the event raised when a user changes the numeric value of the
        /// followed room index NumericUpDown control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FollowedRoom_Numeric_ValueChanged(object sender, EventArgs e)
        {
            //Get the control
            NumericUpDown upDown = (NumericUpDown)sender;

            //The _FollowedRoom is null when the form opens and a user has not
            //selected a room index from the control.
            if (_FollowedRoom != null)
            {
                //Unregister for participant events on the last room selected
                _FollowedRoom.ParticipantAdded -= _FollowedRoom_ParticipantAdded;
                _FollowedRoom.ParticipantRemoved -= _FollowedRoom_ParticipantRemoved;
            }
            //RoomManager.FollowedRooms property is null when the LyncClient.State == ClientState.SignedOut.
            //A user can sign out of Lync while this sample is running. If this happens, then the
            //FollowedRoom property is set to null
            if (_client.RoomManager.FollowedRooms == null)
            {
                return;
            }
            //Get the Room from the followed rooms collection at the index specified by the user
            _FollowedRoom = _client.RoomManager.FollowedRooms[Convert.ToInt32(upDown.Value - 1)];

            //Update the room title label on the form with the Title property of the selected room.
            FollowedRoomTitle_Label.Text = _FollowedRoom.Properties[RoomProperty.Title].ToString();

            //Register for participant events on the room
            _FollowedRoom.ParticipantAdded += _FollowedRoom_ParticipantAdded;
            _FollowedRoom.ParticipantRemoved += _FollowedRoom_ParticipantRemoved;

            //Refresh the participant list with the current participants of the selected room.
            RefreshParticipantList();
        }


        /// <summary>
        /// Handles the event raised when a user removes a room from their contact list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _FollowedRoom_ParticipantRemoved(object sender, RoomParticipantsEventArgs e)
        {
            //Invoke the room participant list refresh delegate on the UI thread.
            Participants_ListBox.Invoke(new RefreshParticipantListDelegate(RefreshParticipantList));
        }

        /// <summary>
        /// Handles the event raised when a user adds a room to their contact list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _FollowedRoom_ParticipantAdded(object sender, RoomParticipantsEventArgs e)
        {
           
            //Invoke the room participant list refresh delegate on the UI thread.
            Participants_ListBox.Invoke(new RefreshParticipantListDelegate(RefreshParticipantList));
        }

        private delegate void RefreshParticipantListDelegate();


        /// <summary>
        /// Refreshes the room participant list box on the UI
        /// </summary>
        private void RefreshParticipantList()
        {
            //Clear the contents of the room participant list box
            Participants_ListBox.Items.Clear();

            //verify that the FollowedRooms property is not null. 
            //When LyncClient.State == ClientState.SignedOut, FollowedRooms is null
            if (_client.RoomManager.FollowedRooms == null)
            {
                return;
            }

            //Declare a Contact that represents a room participant so that contact properties
            //can be read.
            Contact aParticipant;

            //Iterate on the collection of RoomUsers (particpiants)
            foreach (RoomUser user in _FollowedRoom.Participants)
            {
                //Get a Contact object by passing the room user's Uri to the contact manager.
                aParticipant = _client.ContactManager.GetContactByUri(user.Uri);
                if (_client.Self.Contact.Uri == user.Uri)
                {
                    //This is the local user
                    //Add the display name of the room user to the room participant list box.
                    Participants_ListBox.Items.Add("Self: " + aParticipant.GetContactInformation(ContactInformationType.DisplayName).ToString());

                }
                else
                {
                    //Add the display name of the room user to the room participant list box.
                    Participants_ListBox.Items.Add(aParticipant.GetContactInformation(ContactInformationType.DisplayName).ToString());
                }
            }
        }
    }
}
