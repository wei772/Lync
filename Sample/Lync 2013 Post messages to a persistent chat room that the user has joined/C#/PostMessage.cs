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

namespace PostMessage
{
    public partial class PostMessage : Form
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
        public PostMessage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Main form load event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PostMessage_Load(object sender, EventArgs e)
        {
            try
            {
                //Get the lync client platform, the entry point for all group chat room related code.
                _client = LyncClient.GetClient();

                //Register for sign in/sign out events on the client
                _client.StateChanged += new EventHandler<ClientStateChangedEventArgs>(client_StateChanged);

                //If the client is signed in then load the followed room list
                if (_client.State == ClientState.SignedIn)
                {
                    //Set the numeric updown control maximum property to the number of 
                    //followed rooms in the followed room collection on the room manager.
                    if (_client.RoomManager.State == RoomManagerState.Enabled)
                    {
                        if (_client.RoomManager.FollowedRooms != null)
                        {
                            FollowedRoom_Numeric.Maximum = _client.RoomManager.FollowedRooms.Count;
                            FollowedRoom_Numeric.Minimum = 1;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Persistent Chat Server is not reachable. Qutting");
                        this.Close();
                    }
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

            //RoomManager.FollowedRooms property is null when the LyncClient.State == ClientState.SignedOut.
            //A user can sign out of Lync while this sample is running. If this happens, then the
            //FollowedRoom property is set to null
            if (_client.RoomManager.FollowedRooms != null && _client.RoomManager.FollowedRooms.Count > 0)
            {
                //Get the Room from the followed rooms collection at the index specified by the user
                _FollowedRoom = _client.RoomManager.FollowedRooms[Convert.ToInt32(upDown.Value - 1)];

                //Update the room title label on the form with the Title property of the selected room.
                FollowedRoomTitle_Label.Text = _FollowedRoom.Properties[RoomProperty.Title].ToString();

            }
        }

        /// <summary>
        /// Handles the event raised when the user clicks the Send button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendButton_Click(object sender, EventArgs e)
        {
            //Clear the previous message send result.
            SendMessageResult_Label.Text = "";

            //Send a message if the user has selected a room
            if (_FollowedRoom == null || _client.State != ClientState.SignedIn)
            {
                SendMessageResult_Label.Text = "Message was not sent";
                return;
            }
            try
            {
                //If the user is not joined to the room then try to join the room
                if (_FollowedRoom.JoinedState != RoomJoinState.Success)
                {
                    _FollowedRoom.EndJoin(_FollowedRoom.BeginJoin(null, null));
                }

                //If the user is connected (joined) to the room then send the message
                if (_FollowedRoom.JoinedState == RoomJoinState.Success)
                {
                    //Post the message
                    _FollowedRoom.BeginSendMessage(Message_TextBox.Text, RoomMessageType.Regular, SendMessageCallback, null);
                }
            }
            catch (JoinRoomFailException)
            {
                SendMessageResult_Label.Text = "Failed to join room";
            }
            catch (JoinRoomUnauthorizedException)
            {
                SendMessageResult_Label.Text = "Not authorized to join room";
            }
            catch (OperationException oe)
            {
                SendMessageResult_Label.Text = "Operation Exception on post message " + oe.Message;
            }
            catch (Exception ex)
            {
                SendMessageResult_Label.Text = "Exception on post message " + ex.Message;
            }
        }

        /// <summary>
        /// Invoked by platform when the message post operation is finished.
        /// </summary>
        /// <param name="ar"></param>
        private void SendMessageCallback(IAsyncResult ar)
        {
            try
            {
                _FollowedRoom.EndSendMessage(ar);
                SendMessageResult_Label.Invoke(new UpdateMessagePostResultsDelegate(UpdateMessagePostResults), new object[] { "Sent" });
            }
            catch (OperationException oe)
            {
                SendMessageResult_Label.Invoke(new UpdateMessagePostResultsDelegate(UpdateMessagePostResults), new object[] { "Lync client failed to send the message: " + oe.Message });
            }
            catch (Exception ex)
            {
                SendMessageResult_Label.Invoke(new UpdateMessagePostResultsDelegate(UpdateMessagePostResults), new object[] { "Exception on send message: " + ex.Message });
            }
        }

        private delegate void UpdateMessagePostResultsDelegate(string Results);

        private void UpdateMessagePostResults(string Results)
        {
            SendMessageResult_Label.Text = Results;
        }
    }
}
