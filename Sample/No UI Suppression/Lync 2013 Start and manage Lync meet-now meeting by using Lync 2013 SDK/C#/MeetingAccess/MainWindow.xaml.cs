using System.Windows;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Extensibility;
using Microsoft.Lync.Model.Room;
using System.Text;
using System.Security.Principal;
using System.Collections.Generic;

namespace MeetingAccess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Automation _Automation;
        Conversation _Conversation;
        ConversationWindow _ConversationWindow;
        StringBuilder _MeetingKey;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Starts a new meeting by using automation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartMeeting_Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (_Automation != null)
            {
                _Automation.BeginMeetNow((ar) => 
                {
                  _ConversationWindow = _Automation.EndMeetNow(ar);
                  _Conversation = _ConversationWindow.Conversation;
                  //Watch for changes in conference properties
                  _Conversation.PropertyChanged += _Conversation_PropertyChanged;

                  //MeetingRoster_Listbox
                  //Add the user's name to the lobby listbox
                  this.Dispatcher.Invoke(
                      new LoadListBoxDelegate(LoadListBox),
                      new object[] {MeetingRoster_Listbox, 
                        _Conversation.SelfParticipant.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString()});

                  //Watch for participants added to meeting (Lobby)
                  _Conversation.ParticipantAdded += _Conversation_ParticipantAdded;

                },
                null);
            }
        }

        /// <summary>
        /// Handles event raised when a participant is added to meeting. If waiting in lobby, adds
        /// user name to lobby list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _Conversation_ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
        {
            //Watch for changes in participant property (IsInLobby)
            e.Participant.PropertyChanged += Participant_PropertyChanged;
            bool isInLobby = false;
            try
            {
                isInLobby = (bool)e.Participant.Properties[ParticipantProperty.IsInLobby];
            }
            catch (System.NullReferenceException) { }
            //Check to see if participant is in lobby now
            if (isInLobby == true)
            {
                //Add the user's name to the lobby listbox
                this.Dispatcher.Invoke(
                    new LoadListBoxDelegate(LoadListBox),
                    new object[] {Lobby_ListBox, 
                        e.Participant.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString()});
            }
            else
            { 
                //MeetingRoster_Listbox
                //Add the user's name to the lobby listbox
                this.Dispatcher.Invoke(
                    new LoadListBoxDelegate(LoadListBox),
                    new object[] {MeetingRoster_Listbox, 
                        e.Participant.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString()});
            }
        }

        /// <summary>
        /// Handles event raised when participant property (IsInLobby) changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Participant_PropertyChanged(object sender, ParticipantPropertyChangedEventArgs e)
        {
            Participant participant = (Participant)sender;

            //If the IsInLobby property changes and user is no longer in the lobby
            //then remove the user's name from the lobby list
            if (e.Property == ParticipantProperty.IsInLobby && (bool)e.Value == false)
            {
                string displayName = participant.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString();
                this.Dispatcher.Invoke(new RemoveListItemDelegate(RemoveListItem), new object[] { Lobby_ListBox, displayName });


                //MeetingRoster_Listbox
                //Add the user's name to the lobby listbox
                this.Dispatcher.Invoke(
                    new LoadListBoxDelegate(LoadListBox),
                    new object[] {MeetingRoster_Listbox, 
                        displayName});

            }
        }

        /// <summary>
        /// Handles event raised when interesting conversation properties changed. 
        /// Handler is only interested in conference properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _Conversation_PropertyChanged(object sender, ConversationPropertyChangedEventArgs e)
        {
            Conversation conference = (Conversation)sender;
            LoadTextBlockDelegate textBlockDelegate = new LoadTextBlockDelegate(LoadTextBlock);
            LoadTextBoxDelegate textBoxDelegate = new LoadTextBoxDelegate(LoadTextBox);

            switch (e.Property)
            {
                case ConversationProperty.ConferenceAcceptingParticipant:
                    Contact acceptingContact = (Contact)e.Value;
                    this.Dispatcher.Invoke(
                        textBlockDelegate,
                        new object[] { ConferenceAcceptingParticipant_block, "AcceptingParticipant:" + acceptingContact.GetContactInformation(ContactInformationType.DisplayName).ToString() });
                    break;
                case ConversationProperty.ConferencingUri:
                    if (_MeetingKey == null)
                    {
                        _MeetingKey = new StringBuilder();
                    }
                    _MeetingKey.Append("Meeting Uri: " + "conf:" + e.Value.ToString() + "?conversation-id=null");
                    _MeetingKey.Append(System.Environment.NewLine);
                    break;
                case ConversationProperty.ConferenceAccessInformation:

                    try
                    {
                        if (_MeetingKey == null)
                        {
                            _MeetingKey = new StringBuilder();
                        }

                        _MeetingKey.Append(CreateConferenceKey());


                        this.Dispatcher.Invoke(
                            new EnableDisableButtonDelegate(EnableDisableButton),
                            new object[] { PostMeetingKey_Button, true });

                    }
                    catch (System.NullReferenceException nr)
                    {
                        System.Diagnostics.Debug.WriteLine("Null ref Eception on ConferenceAccessInformation changed " + nr.Message);
                    }
                    catch (LyncClientException lce)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception on ConferenceAccessInformation changed " + lce.Message);
                    }
                    break;
            }

            this.Dispatcher.Invoke(
                textBoxDelegate,
                new object[] { ConferenceAccessInformation_block, _MeetingKey.ToString() });
            _MeetingKey.Clear();
        }

        /// <summary>
        /// Returns the meet-now meeting access key as a string
        /// </summary>
        /// <returns></returns>
        private string CreateConferenceKey()
        {
            string returnValue = string.Empty;
            try
            {
                StringBuilder MeetingKey = new StringBuilder();

                //These properties are used to invite people by creating an email (or text message, or IM)
                //and adding the dial in number, external Url, internal Url, and conference Id
                ConferenceAccessInformation conferenceAccess = (ConferenceAccessInformation)_Conversation.Properties[ConversationProperty.ConferenceAccessInformation];

                if (conferenceAccess.Id.Length > 0)
                {
                    MeetingKey.Append("Meeting Id: " + conferenceAccess.Id);
                    MeetingKey.Append(System.Environment.NewLine);
                }

                if (conferenceAccess.AdmissionKey.Length > 0)
                {
                    MeetingKey.Append(conferenceAccess.AdmissionKey);
                    MeetingKey.Append(System.Environment.NewLine);
                }

                string[] attendantNumbers = (string[])conferenceAccess.AutoAttendantNumbers;

                StringBuilder sb2 = new StringBuilder();
                sb2.Append(System.Environment.NewLine);
                foreach (string aNumber in attendantNumbers)
                {
                    sb2.Append("\t\t" + aNumber);
                    sb2.Append(System.Environment.NewLine);
                }
                if (sb2.ToString().Trim().Length > 0)
                {
                    MeetingKey.Append("Auto attendant numbers:" + sb2.ToString());
                    MeetingKey.Append(System.Environment.NewLine);
                }

                if (conferenceAccess.ExternalUrl.Length > 0)
                {
                    MeetingKey.Append("External Url: " + conferenceAccess.ExternalUrl);
                    MeetingKey.Append(System.Environment.NewLine);
                }

                if (conferenceAccess.InternalUrl.Length > 0)
                {
                    MeetingKey.Append("Inner Url: " + conferenceAccess.InternalUrl);
                    MeetingKey.Append(System.Environment.NewLine);
                }

                MeetingKey.Append("Meeting access type: " + ((ConferenceAccessType)_Conversation.Properties[ConversationProperty.ConferencingAccessType]).ToString());
                MeetingKey.Append(System.Environment.NewLine);
                returnValue = MeetingKey.ToString();

            }
            catch (System.NullReferenceException nr)
            {
                System.Diagnostics.Debug.WriteLine("Null ref Eception on ConferenceAccessInformation changed " + nr.Message);
            }
            catch (LyncClientException lce)
            {
                System.Diagnostics.Debug.WriteLine("Exception on ConferenceAccessInformation changed " + lce.Message);
            }
            return returnValue;
        }

        private delegate void LoadTextBoxDelegate(System.Windows.Controls.TextBox blockToLoad, string newItem);
        private void LoadTextBox(System.Windows.Controls.TextBox boxToLoad, string newItem)
        {
            boxToLoad.Text += newItem;
        }

        private delegate void LoadTextBlockDelegate(System.Windows.Controls.TextBlock blockToLoad, string newItem);
        private void LoadTextBlock(System.Windows.Controls.TextBlock blockToLoad, string newItem)
        {
            blockToLoad.Text = newItem;
        }
        private delegate void LoadListBoxDelegate(System.Windows.Controls.ListBox listToLoad, string newItem);
        private void LoadListBox(System.Windows.Controls.ListBox listToLoad, string newItem)
        {
            if (listToLoad.Items.Contains(newItem))
            {
                return;
            }
            listToLoad.Items.Add(newItem);
        }
        private delegate void ClearListBoxDelegate(System.Windows.Controls.ListBox listToClear);
        private void ClearListBox(System.Windows.Controls.ListBox listToClear)
        {
            listToClear.Items.Clear();
        }

        private delegate void RemoveListItemDelegate(System.Windows.Controls.ListBox listToClear, string listItem);
        private void RemoveListItem(System.Windows.Controls.ListBox listToClear, string listItem)
        {
            listToClear.Items.Remove(listItem);
        }

        private delegate void EnableDisableButtonDelegate(System.Windows.Controls.Button buttonToSet, bool enableState);
        private void EnableDisableButton(System.Windows.Controls.Button buttonToSet, bool enableState)
        {
            buttonToSet.IsEnabled = enableState;
        }

        /// <summary>
        /// Handles the window loaded event and gets the Lync client 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            try
            {
                _Automation = LyncClient.GetAutomation();

                //Load a list of the user's followed chat rooms
                LoadRoomList();
                
            }
            catch (ClientNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("Client is not running");
            }
            catch (LyncClientException lce) 
            {
                System.Diagnostics.Debug.WriteLine("LyncClientException on getClient(): " + lce.Message);
            }
        }

        //Ends the current meeting
        private void EndMeeting_Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_ConversationWindow != null)
                {
                    _ConversationWindow.Close();
                }
                ConferenceAccessInformation_block.Text = "";
                MeetingRoster_Listbox.Items.Clear();

            }
            catch (NotInitializedException){}

        }

        /// <summary>
        /// Sets the conference access type property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetAccessType_Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!_Conversation.CanSetProperty(ConversationProperty.ConferencingAccessType))
            {
                return;
            }
            if (Anon_Radio.IsChecked == true)
            {
                _Conversation.BeginSetProperty(
                    ConversationProperty.ConferencingAccessType, 
                    ConferenceAccessType.Anonymous, (ar) => 
                    {
                        _Conversation.EndSetProperty(ar);
                    },
                    null);
            }
            if (Open_Radio.IsChecked == true)
            {
                _Conversation.BeginSetProperty(
                    ConversationProperty.ConferencingAccessType,
                    ConferenceAccessType.Open, (ar) =>
                    {
                        _Conversation.EndSetProperty(ar);
                    },
                    null);
            }

            //All invited users must wait in lobby for admission to a closed meeting
            if (Closed_Radio.IsChecked == true)
            {
                _Conversation.BeginSetProperty(
                    ConversationProperty.ConferencingAccessType,
                    ConferenceAccessType.Closed, (ar) =>
                    {
                        _Conversation.EndSetProperty(ar);
                    },
                    null);
            }
            if (Locked_Radio.IsChecked == true)
            {
                _Conversation.BeginSetProperty(
                    ConversationProperty.ConferencingAccessType,
                    ConferenceAccessType.Locked, (ar) =>
                    {
                        _Conversation.EndSetProperty(ar);
                    },
                    null);
            }
        }

        /// <summary>
        /// Locks the meeting so that only the presenter can get in.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LockMeeting_Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!_Conversation.CanSetProperty(ConversationProperty.ConferencingLocked))
            {
                return;
            }

            _Conversation.BeginSetProperty(
                ConversationProperty.ConferencingLocked,
                true, (ar) =>
                {
                    _Conversation.EndSetProperty(ar);
                },
                null);

        }

        /// <summary>
        /// Admits all people waiting in the meeting lobby
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AdmitAll_Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Collections.Generic.List<Participant> participants = new System.Collections.Generic.List<Participant>();
            foreach (Participant participant in _Conversation.Participants)
            {
                if ((bool)participant.Properties[ParticipantProperty.IsInLobby] == true)
                {
                    participants.Add(participant);
                }
            }
            _Conversation.BeginAdmitParticipants(
                participants, 
                (ar) => 
                {
                    _Conversation.EndAdmitParticipants(ar);
                },
                null);
        }

        /// <summary>
        /// Admits the selected person from the meeting lobby
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AdmitOne_Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (Participant participant in _Conversation.Participants)
            {
                if (participant.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString() == Lobby_ListBox.SelectedItem.ToString())
                {
                    if (participant.CanAdmit())
                    {
                        participant.BeginAdmit(
                            (ar) => 
                            {
                                participant.EndAdmit(ar);
                            },
                            null);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Denies meeting admission to the person selected in the lobby
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DenyOne_Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (Participant participant in _Conversation.Participants)
            {
                if (participant.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString() == Lobby_ListBox.SelectedItem.ToString())
                {
                    if (participant.CanDeny())
                    {
                        participant.BeginDeny(
                            (ar) =>
                            {
                                participant.EndDeny(ar);
                            },
                            null);
                    }
                    break;
                }
            }

        }

        /// <summary>
        /// Denies meeting admission to all people waiting in the meeting lobby
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DenyAll_Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Collections.Generic.List<Participant> participants = new System.Collections.Generic.List<Participant>();
            foreach (Participant participant in _Conversation.Participants)
            {
                if ((bool)participant.Properties[ParticipantProperty.IsInLobby] == true)
                {
                    participants.Add(participant);
                }
            }
            _Conversation.BeginDenyParticipants(
                participants,
                (ar) =>
                {
                    _Conversation.EndDenyParticipants(ar);
                },
                null);

        }

        /// <summary>
        /// Pins or unpins the video stream of the selected participant.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PinVideo_Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (Participant participant in _Conversation.Participants)
            {

                Contact contact = participant.Contact;
                string displayName;

                displayName = contact.GetContactInformation(
                    ContactInformationType.DisplayName).ToString();

                if (displayName == MeetingRoster_Listbox.SelectedItem.ToString())
                {

                    //The local participant cannot pin themselves in their
                    //video gallery view.
                    if (participant.IsSelf == true)
                    {
                        return;
                    }

                    if ((bool)participant.Properties[ParticipantProperty.IsPinned])
                    {
                        try
                        {
                            participant.BeginUnPinVideo((ar) =>
                            {
                                try
                                {
                                    participant.EndUnPinVideo(ar);
                                }
                                catch (LyncClientException) { };
                            }, null);
                        }
                        catch (System.ArgumentException) { };
                    }
                    
                    else
                    {
                        try
                        {
                            participant.BeginPinVideo((ar) =>
                            {
                                try
                                {
                                    participant.EndPinVideo(ar);
                                }
                                catch (LyncClientException) { };
                            }, null);
                        }
                        catch (System.ArgumentException) { };
                    }
                    break;
                }
            }
        }


        /// <summary>
        /// Locks or unlocks the selected participant video in the video gallery so 
        /// that other conversation participants cannot remove the participant's 
        /// video stream from their view of the video gallery.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LockVideo_Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Only presenters can lock participant video
            if (!(bool)_Conversation.SelfParticipant.Properties[ParticipantProperty.IsPresenter])
            {
                return;
            }

            foreach (Participant participant in _Conversation.Participants)
            {
                Contact contact = participant.Contact;
                string displayName = contact.GetContactInformation(
                    ContactInformationType.DisplayName).ToString();

                if (displayName == MeetingRoster_Listbox.SelectedItem.ToString())
                {
                    if ((bool)participant.Properties[ParticipantProperty.IsLocked])
                    {
                        participant.BeginUnLockVideo((ar) => 
                        { participant.EndUnLockVideo(ar); }, null);
                    }
                    else
                    {
                        participant.BeginLockVideo((ar) => 
                        { participant.EndLockVideo(ar); }, null);
                    }
                    break;
                }
            }
        }

        private void MakePresenter_Button_Click_1(object sender, RoutedEventArgs e)
        {
            if ((bool)_Conversation.SelfParticipant.Properties[ParticipantProperty.IsPresenter] == false)
            {
                return;
            }
            foreach (Participant participant in _Conversation.Participants)
            {
                string displayName = participant.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString();
                if (displayName == MeetingRoster_Listbox.SelectedItem.ToString())
                {
                    if (!(bool)participant.Properties[ParticipantProperty.IsPresenter])
                    {
                        participant.BeginSetProperty(
                            ParticipantProperty.IsPresenter,
                            true,
                            (ar) => 
                            {
                                participant.EndSetProperty(ar); 
                            },
                            null);
                    }
                    break;
                }
            }

        }

        private void MakeParticipant_Button_Click_1(object sender, RoutedEventArgs e)
        {
            if ((bool)_Conversation.SelfParticipant.Properties[ParticipantProperty.IsPresenter] == false)
            {
                return;
            }
            foreach (Participant participant in _Conversation.Participants)
            {
                string displayName = participant.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString();
                if (displayName == MeetingRoster_Listbox.SelectedItem.ToString())
                {
                    if ((bool)participant.Properties[ParticipantProperty.IsPresenter])
                    {
                        participant.BeginSetProperty(
                            ParticipantProperty.IsPresenter,
                            false,
                            (ar) =>
                            {
                                participant.EndSetProperty(ar);
                            },
                            null);
                    }
                    break;
                }
            }

        }

        private void PostMeetingKey_Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (RoomList_ListBox.SelectedItem == null)
            {
                MessageBox.Show("You must select a chat room");
                return;
            }
            try
            {
                Room myRoom = ((RoomWrapper)RoomList_ListBox.SelectedItem).ChatRoom;
                    
                Dictionary<RoomMessageFormat, string> messageDictionary = new Dictionary<RoomMessageFormat, string>();
                messageDictionary.Add(RoomMessageFormat.PlainText, CreateConferenceKey());
                myRoom.BeginSendStoryMessage(
                    messageDictionary,
                    RoomMessageType.Alert
                    ,
                    "Meeting has started",
                    (ar) =>
                    {
                        try
                        {
                            myRoom.EndSendStoryMessage(ar);
                            MessageBox.Show(
                                "Meeting access key successfully posted to " +
                                myRoom.Properties[RoomProperty.Title].ToString(),"Chat room post");
                        }
                        catch (LyncClientException lce)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                "Lync client exception on post story message to " + 
                                myRoom.Properties[RoomProperty.Title].ToString() + 
                                " " + 
                                lce.Message);
                        }
                    }
                    ,
                    null);
            }
            catch (LyncClientException lce)
            {
                System.Diagnostics.Debug.WriteLine("Lync client exception on most message to room() " + lce.Message);
            }

        }

        /// <summary>
        /// Loads the user's followed chat rooms into a UI listbox.
        /// </summary>
        private void LoadRoomList()
        {
            foreach (Room room in LyncClient.GetClient().RoomManager.FollowedRooms)
            {
                RoomList_ListBox.Items.Add(new RoomWrapper(room));
            }
        }
    }

    internal class RoomWrapper
    {
        Room _Room;
        public override string ToString()
        {
            return _Room.Properties[RoomProperty.Title].ToString();
        }
        public Room ChatRoom
        {
            get
            {
                return _Room;
            }
            set
            {
                _Room = value;
            }
        }
        public RoomWrapper(Room roomToWrap)
        {
            _Room = roomToWrap;
        }
    }
}
