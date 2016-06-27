/*===================================================================== 
  This file is part of the Microsoft Office 2013 Lync Code Samples. 

  Copyright (C) 2013 Microsoft Corporation.  All rights reserved. 

This source code is intended only as a supplement to Microsoft 
Development Tools and/or on-line documentation.  See these other 
materials for detailed information regarding Microsoft code samples. 

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
PARTICULAR PURPOSE. 
=====================================================================*/

#define DEBUG

using System;
using System.Windows.Forms;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Group;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.Sharing;
using System.Collections.Generic;


namespace ShareResources
{
    internal struct ControlDimensions
    {
        /// <summary>
        /// Form and panel starting dimensions 
        /// </summary>
        internal int _originalFormWidth;
        internal int _originalFormHeight;
        internal int _horizontalMargin;
        internal int _verticalMargin;
        internal int _originalPanelWidth;
        internal int _originalPanelHeight;

    }
    public partial class ShareResources_Form : Form
    {

        #region class field declarations

        /// <summary>
        /// Dimensions of the parent form and container control that
        /// will dock the application sharing view
        /// </summary>
        /// 
        ControlDimensions _windowSize;
        string _UserUri;


        /// <summary>
        /// Lync client platform. The entry point to the API
        /// </summary>
        Microsoft.Lync.Model.LyncClient _LyncClient;

        /// <summary>
        /// Represents a Lync conversation
        /// </summary>
        Conversation _conversation;

        /// <summary>
        /// A Contact instance representing the participant selected to be granted control of a resource
        /// </summary>
        Contact _ResourceControllingContact;

        /// <summary>
        /// Dictionary of all contacts selected from the multi-select enabled contact list on UI
        /// </summary>
        Dictionary<string, Contact> _selectedContacts = new Dictionary<string, Contact>();

        /// <summary>
        /// Collection of all participants application sharing modalities, keyed by Contact.Uri.
        /// See _controllingContact class field...
        /// </summary>
        Dictionary<string, ApplicationSharingModality> _participantSharingModalities = new Dictionary<string, ApplicationSharingModality>();

        /// <summary>
        /// The Application sharing modality of the conversation itself
        /// </summary>
        ApplicationSharingModality _sharingModality;

        /// <summary>
        /// The Application sharing modality of the local participant.
        /// </summary>
        ApplicationSharingModality _LocalParticipantSharingModality;

        UserSignIn _userSignIn;

        /// <summary>
        /// Winform used to create bright border around a locally shared resource.
        /// </summary>
        ChromeRunner _chromeRunner;

        #endregion

        #region Sign in related methods
        void _userSignIn_ClientStateChanged(string newState)
        {
            this.Invoke(new ChangeLabelTextDelegate(ChangeLabelText), new object[] { ClientStateString_Label, newState });
            if (newState == ClientState.SigningOut.ToString())
            {
                _userSignIn.ClientStateChanged -= _userSignIn_ClientStateChanged;
            }
        }

        void _userSignIn_SetWindowCursor(Cursor newCursor)
        {
            this.Invoke(new SetCursorForUserDelegate(SetCursorForUser), new object[] { newCursor });
        }

        void _userSignIn_UserIsSignedIn(LyncClient lyncClient)
        {
            _LyncClient = lyncClient;
            _LyncClient.ConversationManager.ConversationAdded += new EventHandler<ConversationManagerEventArgs>(ConversationManager_ConversationAdded);
            _LyncClient.ConversationManager.ConversationRemoved += ConversationManager_ConversationRemoved;

            this.Invoke(new NoParamDelegate(LoadAllContacts));
            this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { Start_Button, true });
            this.Invoke(new UpdateSharedResourcesListboxDelegate(UpdateSharedResourcesListbox));
            try
            {
                string userName = _LyncClient.Self.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString();
                this.Invoke(new UpdateControlContentDelegate(UpdateControlContent), new object[] { UserName_Label, userName });
            }
            catch (NullReferenceException)
            {
                this.Invoke(new UpdateControlContentDelegate(UpdateControlContent), new object[] { UserName_Label, "user name is not available" });
            }


            this.Invoke(new SetCursorForUserDelegate(SetCursorForUser), new object[] { System.Windows.Forms.Cursors.Arrow });

        }

        void _userSignIn_CloseAppConditionHit()
        {
            this.Invoke(new NoParamDelegate(CloseWindow));
        }

        /// <summary>
        /// Signs a user in to Lync as one of two possible users. User that is
        /// signed in depends on whether side-by-side client is chosen.
        /// </summary>
        private void SignUserIn()
        {
            //Set the display cursor to indicate that user must wait for
            //sign in to complete
            this.Invoke(new SetCursorForUserDelegate(SetCursorForUser), new object[] { System.Windows.Forms.Cursors.WaitCursor });

            //Set the sign in credentials of the user to the
            //appropriate credentials for the endpoint mode
            string userUri = string.Empty;
            string userPassword = string.Empty;


            SignInCreds getCreds;
            getCreds = new SignInCreds("Sign in");
            if (getCreds.ShowDialog() == DialogResult.OK)
            {
                userUri = getCreds.UserName;
                userPassword = getCreds.Password;
                getCreds.Close();
            }

            _UserUri = userUri;

            this.Invoke(new ClearAllContactsDelegate(ClearAllContacts));
            this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { Start_Button, false });

            this.Invoke(new ChangeLabelTextDelegate(ChangeLabelText), new object[] { ClientStateString_Label, _LyncClient.State.ToString() });
            this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { EndConversation_Button, false });
            this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { StartSharingResource_Button, false });


            _LyncClient.BeginSignIn(
                userUri,
                userUri,
                userPassword,
                (ar) =>
                {
                    try
                    {
                        _LyncClient.EndSignIn(ar);

                        this.Invoke(
                            new UpdateControlContentDelegate(UpdateControlContent),
                            new object[] { this.ClientStateString_Label, "Signed in" });
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show("exception on endsignin: " + exc.Message);
                    }
                },
                null);

        }

        private delegate void UpdateControlContentDelegate(object controlToUpdate, string newContent);

        private void UpdateControlContent(object controlToUpdate, string newContent)
        {
            switch (controlToUpdate.GetType().Name.ToLower())
            {
                case "label":
                    System.Windows.Forms.Label label = (System.Windows.Forms.Label)controlToUpdate;
                    label.Text = newContent;
                    break;
                case "textbox":
                    System.Windows.Forms.TextBox textBox = (System.Windows.Forms.TextBox)controlToUpdate;
                    textBox.Text = newContent;
                    break;
            }
        }
        private delegate void SetCursorForUserDelegate(System.Windows.Forms.Cursor cursorToSet);
        private void SetCursorForUser(System.Windows.Forms.Cursor cursorToSet)
        {
            this.Cursor = cursorToSet;
            this.Refresh();

        }
        #endregion

        #region conversation manager event handlers
        /// <summary>
        /// Handles the event raised when the active conversation is terminated and removed from the collection of conversations
        /// on ConversationManager.Conversations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void ConversationManager_ConversationRemoved(object sender, ConversationManagerEventArgs e)
        {
            this.Invoke(new SetContactSelectionModeDelegate(SetContactSelectionMode), new object[] { SelectionMode.MultiSimple });
            this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { EndConversation_Button, false });
            this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { Disconnect_Button, false });
            this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { Request_Button, false });
            this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { StartSharingResource_Button, false });
            this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { Start_Button, true });
            this.Invoke(new ChangeLabelTextDelegate(ChangeLabelText), new object[] { ContactList_Label, "1) Choose contacts" });
            this.Invoke(new UpdateSharedResourcesListboxDelegate(UpdateSharedResourcesListbox));

            this.Invoke(new NoParamDelegate(LoadAllContacts));

            if (_conversation != null)
            {
                //Un-register for participant events on the conversation that is terminated.
                _conversation.ParticipantAdded -= _conversation_ParticipantAdded;
                _conversation.ParticipantRemoved -= _conversation_ParticipantRemoved;

                //If the removed conversation is the conversation hosted by this window then resume listening for new conversations.
                if (_conversation == e.Conversation)
                {
                    //Resume listening for new conversations
                    _LyncClient.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;
                }
            }

            if (_sharingModality != null)
            {
                //Unregister for events on the terminated conversation's sharing modality events.
                _sharingModality.ModalityStateChanged -= _sharingModality_ModalityStateChanged;
                _sharingModality.ControlRequestReceived -= _sharingModality_ControlRequestReceived;
                _sharingModality.LocalSharedResourcesChanged -= _sharingModality_LocalSharedResourcesChanged;
                _sharingModality.ControllerChanged -= _sharingModality_ControllerChanged;
                _sharingModality.ActionAvailabilityChanged -= _sharingModality_ActionAvailabilityChanged;
                if (_chromeRunner != null)
                {
                    this.Invoke(new NoParamDelegate(_chromeRunner.CloseTheChrome));
                }

            }

            //Unregister for the application sharing events on each and every participant in the terminated 
            //conversation
            foreach (string contactUri in _selectedContacts.Keys)
            {
                if (_participantSharingModalities.ContainsKey(contactUri))
                {
                    ApplicationSharingModality participantSharingModality = (ApplicationSharingModality)_participantSharingModalities[contactUri];
                    participantSharingModality.ActionAvailabilityChanged -= _sharingModality_ActionAvailabilityChanged;
                }
            }



            //de-reference the class state
            _conversation = null;
            _ResourceControllingContact = null;
            _sharingModality = null;
            _participantSharingModalities.Clear();



        }

        /// <summary>
        /// Handles the event raised when a new conversation is added. This sample only hosts one conversation
        /// at a time. Once this event is handled, the sample un-registers for this event. The event is registered again when
        /// this conversation is removed from the ContactManager.Conversations collection upon termination of this conversation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void ConversationManager_ConversationAdded(object sender, Microsoft.Lync.Model.Conversation.ConversationManagerEventArgs e)
        {
            Boolean addedByThisProcess = true;

            try
            {
                //Suspend hosting new conversations until this conversation is ended
                _LyncClient.ConversationManager.ConversationAdded -= ConversationManager_ConversationAdded;
            }

            catch (Exception ex) { MessageBox.Show("Exception on de-register for ConversationAdded: " + ex.Message); }

            //If this class field is null then the new conversation was not started by this running process. It was
            //Started by the Lync client or by a remote user.
            if (_conversation == null)
            {
                addedByThisProcess = false;
                _conversation = e.Conversation;
            }



            //Register for the application sharing modality event on the conversation itself
            _sharingModality = (ApplicationSharingModality)_conversation.Modalities[ModalityTypes.ApplicationSharing];

            //Register for state changes like connecting->connected
            _sharingModality.ModalityStateChanged += _sharingModality_ModalityStateChanged;

            //Register to catch requests from other participants for control of the locally owned sharing resource.
            _sharingModality.ControlRequestReceived += _sharingModality_ControlRequestReceived;
            _sharingModality.ControllerChanged += _sharingModality_ControllerChanged;


            //Register to catch changes in the list of local sharable resources such as a process that starts up or terminates.
            _sharingModality.LocalSharedResourcesChanged += _sharingModality_LocalSharedResourcesChanged;

            //Register for changes in the availbility of resource controlling actions such as grant and revoke.
            _sharingModality.ActionAvailabilityChanged += _sharingModality_ActionAvailabilityChanged;

            //Register for changes in the local participant's mode of sharing participation
            // such as Viewing->Sharing, Requesting Control->Controlling.
            _sharingModality.ParticipationStateChanged += _sharingModality_ParticipationStateChanged;

            //Register for participant added events on the new conversation
            //The next important action in the chain of conversation intiating action happens in the ParticipantAdded event handler.
            _conversation.ParticipantAdded += _conversation_ParticipantAdded;
            _conversation.ParticipantRemoved += _conversation_ParticipantRemoved;


            if (addedByThisProcess == true)
            {

                //Clear out the contact list on the UI to be replaced with only the contacts selected for the current conversation.
                this.Invoke(new ClearAllContactsDelegate(ClearAllContacts));

                //Iterate on the contact dictionary that is filled from the contact Sip addresses selected from the 
                //contact list UI control. Add each selected contact to the conversation, causing an invitation to be
                //sent to each contact.
                foreach (Contact selectedContact in _selectedContacts.Values)
                {
                    Participant newParticipant = _conversation.AddParticipant(selectedContact);
                }
            }

            //Update the list of local sharable resources on the UI
            this.Invoke(new UpdateSharedResourcesListboxDelegate(UpdateSharedResourcesListbox));

            //Set the enabled state of the resource sharing button, end conversation button, and start conversation button.
            this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { StartSharingResource_Button, true });
            this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { EndConversation_Button, true });
            this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { Start_Button, false });

            if (((Modality)_conversation.Modalities[ModalityTypes.InstantMessage]).CanInvoke(ModalityAction.SendInstantMessage))
            {
                ((InstantMessageModality)_conversation.Modalities[ModalityTypes.InstantMessage]).BeginSendMessage("hi", (ar) =>
                {
                    if (((InstantMessageModality)_conversation.Modalities[ModalityTypes.InstantMessage]).CanInvoke(ModalityAction.SendInstantMessage))
                    {
                        ((InstantMessageModality)_conversation.Modalities[ModalityTypes.InstantMessage]).EndSendMessage(ar);
                    }
                }, null);
            }

        }

        #endregion

        #region Conversation event handlers


        /// <summary>
        /// Handles the participant added event. Registers for events on the application sharing modality for the 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _conversation_ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
        {
            try
            {
                //Is this added participant the local user?
                if (e.Participant.IsSelf)
                {
                    //Store the application sharing modality of the local user so that
                    //the user can request or release control of a remotely owned and shared resource.
                    _LocalParticipantSharingModality = (ApplicationSharingModality)e.Participant.Modalities[ModalityTypes.ApplicationSharing];

                    //Enable or disable the Start Resource Sharing button according to the role of the local participant.
                    //Roles can be Presenter or Attendee.

                    if (e.Participant.Properties[ParticipantProperty.IsPresenter] != null)
                    {
                        this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { StartSharingResource_Button, (Boolean)e.Participant.Properties[ParticipantProperty.IsPresenter] });
                    }
                    //Register for the particpant property changed event to be notified when the role of the local user changes.
                    e.Participant.PropertyChanged += Participant_PropertyChanged;

                    this.Invoke(new ChangeLabelTextDelegate(ChangeLabelText), new object[] { SharingParticipationStateString_Label, "not sharing" });

                }
                else
                {
                    //The self participant application sharing modality is also the conversation application sharing modality.
                    //to avoid registering an event handler for the conversation modality twice, we do not register it on the
                    //self participant

                    //Get the application sharing modality of the added participant and store it in a class dictionary field for easy access later.
                    ApplicationSharingModality participantSharingModality = (ApplicationSharingModality)e.Participant.Modalities[ModalityTypes.ApplicationSharing];
                    _participantSharingModalities.Add(e.Participant.Contact.Uri, participantSharingModality);

                    //register for important events on the application sharing modality of the new participant.
                    participantSharingModality.ActionAvailabilityChanged += _sharingModality_ActionAvailabilityChanged;
                    participantSharingModality.ModalityStateChanged += _sharingModality_ModalityStateChanged;
                }



            }
            catch (ArgumentException ae)
            {
                System.Diagnostics.Debug.WriteLine("argument exception: " + ae.Message);
            }

        }

        /// <summary>
        /// Handles the event raised when a particpant is removed from the conversation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _conversation_ParticipantRemoved(object sender, ParticipantCollectionChangedEventArgs e)
        {

            //get the application sharing modality of the removed participant out of the class modalty dicitonary
            ApplicationSharingModality removedModality = _participantSharingModalities[e.Participant.Contact.Uri];

            //Un-register for modality events on this participant's application sharing modality.
            removedModality.ActionAvailabilityChanged -= _sharingModality_ActionAvailabilityChanged;

            //Remove the modality from the dictionary.
            _participantSharingModalities.Remove(e.Participant.Contact.Uri);
            this.Invoke(new RemoveAContactDelegate(RemoveAContact), new object[] { e.Participant.Contact.Uri });
        }
        #endregion

        #region Participant event handlers
        /// <summary>
        /// Called when a participant property is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Participant_PropertyChanged(object sender, ParticipantPropertyChangedEventArgs e)
        {
            if (e.Property == ParticipantProperty.IsPresenter)
            {
                //Enable or disable the Start Sharing Resource button according to the participant role
                this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { StartSharingResource_Button, (Boolean)e.Value });
            }
        }

        #endregion

        #region application sharing modality event handlers

        /// <summary>
        /// Handles the event raised when the local user has started or ended a shareable process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _sharingModality_LocalSharedResourcesChanged(object sender, LocalSharedResourcesChangedEventArgs e)
        {
            //Update the shareable resources list box with the currently available shareable resources.
            if (e.ResourceList.Count > 0)
            {
                this.Invoke(new ChangeLabelTextDelegate(ChangeLabelText), new object[] { SharedResource_Label, e.ResourceList[0].Name });
            }
            else
            {
                this.Invoke(new ChangeLabelTextDelegate(ChangeLabelText), new object[] { SharedResource_Label, "None" });
            }
        }

        /// <summary>
        /// Handles the even raised when the state of an application sharing modality changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _sharingModality_ModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {

            //Modality will be connected for each particpant whethere they have accepted the sharing invite or not.
            ApplicationSharingModality thisModality = (ApplicationSharingModality)sender;
            if (e.NewState == ModalityState.Connected)
            {
                this.Invoke(new AddAContactDelegate(AddAContact), new object[] { thisModality.Participant.Contact.Uri });
                this.Invoke(new ChangeLabelTextDelegate(ChangeLabelText), new object[] { ContactList_Label, "5) Pick a participant" });

                if (thisModality == _conversation.Modalities[ModalityTypes.ApplicationSharing])
                {
                    //ShowStage_Button
                    //If the local user is not resource sharer, then dock the view to see
                    //the resource shared by a remote user
                    if (thisModality.Sharer != _conversation.SelfParticipant)
                    {
                        this.Invoke(new ChangeButtonTextDelegate(ChangeButtonText), new object[] { Disconnect_Button, "Hide sharing stage" });
                    }
                    else
                    {
                        this.Invoke(new ChangeButtonTextDelegate(ChangeButtonText), new object[] { Disconnect_Button, "Stop sharing" });
                    }
                }

            }
            if (e.NewState == ModalityState.Disconnected)
            {
                if (thisModality == _conversation.Modalities[ModalityTypes.ApplicationSharing])
                {
                    this.Invoke(new NoParamDelegate(RemoveDockingPanel));
                }

            }
        }

        /// <summary>
        /// Handles the event raised when the participant that is controlling the shared conversation application resource
        /// changes. This event is raised locally even when the shared resource is not locally owned.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _sharingModality_ControllerChanged(object sender, ControllerChangedEventArgs e)
        {
            this.Invoke(new ChangeLabelTextDelegate(ChangeLabelText), new object[] { ResourceControllerName_Label, e.ControllerName });

            //Store the Contact object for the conversation participant that now controls the shared conversation resource.
            if (((ApplicationSharingModality)sender).Controller != null)
            {
                _ResourceControllingContact = ((ApplicationSharingModality)sender).Controller.Contact;
            }

        }

        /// <summary>
        /// Handles the event raised when a conversation participant requests control of a locally owned resource.
        /// These requests always go to the resource owner, and not the current controller of the resource.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _sharingModality_ControlRequestReceived(object sender, ControlRequestReceivedEventArgs e)
        {
            //Get the name of the participant that is requesting control of the locally owned resource.
            string displayRequesterName = e.Requester.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString();


            //Update the text of the Accept button to include the name of the requester.
            this.Invoke(new ChangeButtonTextDelegate(ChangeButtonText),
                new object[] { Accept_Button, "Accept " + displayRequesterName + "  Request" });

            //Store the Contact object for the requesting participant.
            _ResourceControllingContact = e.Requester.Contact;
        }

        /// <summary>
        /// Handles the event raised when a participant participation state changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _sharingModality_ParticipationStateChanged(object sender, ParticipationStateChangedEventArgs e)
        {
            if (((ApplicationSharingModality)sender) == _sharingModality)
            {
                if (e.NewState.ToString() == "None")
                {
                    this.Invoke(new ChangeLabelTextDelegate(ChangeLabelText),
                        new object[] { SharingParticipationStateString_Label, "not sharing" });
                }
                else if (e.NewState == ParticipationState.Viewing)
                {
                    this.Invoke(new NoParamDelegate(DockAppShareView));
                }
                else
                {
                    this.Invoke(new ChangeLabelTextDelegate(ChangeLabelText),
                        new object[] { SharingParticipationStateString_Label, e.NewState.ToString() });

                }
                ApplicationSharingModality participantModality = (ApplicationSharingModality)sender;
                if (participantModality.Controller != null)
                {
                    string userName = participantModality.Controller.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString();
                    this.Invoke(new ChangeLabelTextDelegate(ChangeLabelText), new object[] { SharedResource_Label, userName });
                }
            }
        }

        /// <summary>
        /// Event handler for sharing modality action availability change
        /// This method enables or disables the modality control action buttons on the UI according to
        /// the availability of a given action.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _sharingModality_ActionAvailabilityChanged(object sender, ModalityActionAvailabilityChangedEventArgs e)
        {
            try
            {
                ApplicationSharingModality thisModality = (ApplicationSharingModality)sender;
                Button buttonToUpdate = null;

                //Enable or disable a UI action button that corresponds to the action whose availability has changed.
                switch (e.Action)
                {
                    case ModalityAction.Accept:
                        buttonToUpdate = AcceptSharing_Button;
                        break;
                    case ModalityAction.Reject:
                        buttonToUpdate = RejectSharing_Button;
                        break;
                    case ModalityAction.AcceptSharingControlRequest:
                        buttonToUpdate = Accept_Button;
                        break;
                    case ModalityAction.DeclineSharingControlRequest:
                        buttonToUpdate = Decline_Button;
                        break;
                    case ModalityAction.GrantSharingControl:
                        buttonToUpdate = Grant_Button;
                        break;
                    case ModalityAction.ReleaseSharingControl:
                        buttonToUpdate = Release_Button;
                        break;
                    case ModalityAction.RequestSharingControl:
                        buttonToUpdate = Request_Button;
                        break;
                    case ModalityAction.RevokeSharingControl:
                        buttonToUpdate = Revoke_Button;
                        break;
                    case ModalityAction.Disconnect:
                        buttonToUpdate = Disconnect_Button;
                        break;
                }

                //Not all possible cases of ActionAvailability are represented in the previous switch statement. 
                //For this reason, buttonToUpdate may be null.
                if (buttonToUpdate != null)
                {
                    this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { buttonToUpdate, e.IsAvailable });
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region UI event handlers


        /// <summary>
        /// invoked when sample form is loaded. Initializes fields, gets API entry point, 
        /// registers for events on Lync Client and ConversationManager.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShareResources_Form_Load(object sender, EventArgs e)
        {
            _windowSize = new ControlDimensions();
            _windowSize._originalFormHeight = this.Height;
            _windowSize._originalFormWidth = this.Width;
            _windowSize._originalPanelHeight = 600;
            _windowSize._originalPanelWidth = 975;


            _userSignIn = new UserSignIn();
            _userSignIn.CloseAppConditionHit += _userSignIn_CloseAppConditionHit;
            _userSignIn.UserIsSignedIn += _userSignIn_UserIsSignedIn;
            _userSignIn.SetWindowCursor += _userSignIn_SetWindowCursor;
            _userSignIn.ClientStateChanged += _userSignIn_ClientStateChanged;
        }

        /// <summary>
        /// Handles click event on sharing resource button. User must select a resource
        /// from the sharable resources list before clicking the button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartSharingResource_Button_Click(object sender, EventArgs e)
        {

            //If there is no active conversation to share this resource in, return from handler
            if (_conversation == null)
            {
                return;
            }

            //If there is no sharing modality stored locally on the active conversation, get it from the active conversation and store it.
            if (_sharingModality == null)
            {
                _sharingModality = _conversation.Modalities[ModalityTypes.ApplicationSharing] as ApplicationSharingModality;
            }


            //Process the users selected resource choice by calling this helper method
            ShareSelectedResource(SharedResources_ListBox.SelectedItem as SharingResource_Wrapper);
        }

       /// <summary>
       ///Accept another participants request to control locally owned and shared resource.
       ///The Accept button is enabled when the _sharingModality_ActionAvailabilityChanged event handler is called
       ///with the event argument that specifies the ModalityAction.AcceptSharingControlRequest action is available.
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void Accept_Button_Click(object sender, EventArgs e)
        {
            Accept_Button.Text = "Accept";

            //_selectedContact is set to the Contact object of the participant who requested control of the resource. 
            //see the _sharingModality_ControlRequestReceived method in the application sharing modality event handlers region.
            ApplicationSharingModality sharingModality = (ApplicationSharingModality)_participantSharingModalities[_ResourceControllingContact.Uri];

            //If the requesting participant application sharing modality is available and the AcceptSharingControlRequest action can be invoked
            if (sharingModality != null && sharingModality.CanInvoke(ModalityAction.AcceptSharingControlRequest))
            {
                //Accept sharing control request.
                sharingModality.BeginAcceptControlRequest((ar) => {sharingModality.EndAcceptControlRequest(ar);}, null);
            }
        }

        /// <summary>
        ///Decline another participants request to control locally owned and shared resource.
        ///The Decline button is enabled when the _sharingModality_ActionAvailabilityChanged event handler is
        ///called with the event argument that specifies the ModalityAction.DeclineSharingControlRequest action is available.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Decline_Button_Click(object sender, EventArgs e)
        {
            //_selectedContact is set to the Contact object of the participant who requested control of the resource. 
            //see the _sharingModality_ControlRequestReceived method in the application sharing modality event handlers region.
            ApplicationSharingModality sharingModality = (ApplicationSharingModality)_participantSharingModalities[_ResourceControllingContact.Uri];
            if (sharingModality != null && sharingModality.CanInvoke(ModalityAction.DeclineSharingControlRequest))
            {
                sharingModality.BeginDeclineControlRequest((ar) => { sharingModality.EndDeclineControlRequest(ar); }, null);
            }

        }

        /// <summary>
        ///Grant another participant control of a locally owned and shared resource.
        ///The Grant button is enabled when the _sharingModality_ActionAvailabilityChanged event handler is
        ///called with the event argument that specifies the ModalityAction.GrantSharingControl action is available.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grant_Button_Click(object sender, EventArgs e)
        {
            if (Contact_ListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a conversation participant");
                return;
            }
            //Get the sharing modality of the participant which the local user has selected to control the locally owned resource.
            try
            {
                ApplicationSharingModality sharingModality = (ApplicationSharingModality)_participantSharingModalities[Contact_ListBox.SelectedItem.ToString()];

                //If the application sharing modality is available and the resource can still be granted then grant
                //control of the resource.
                if (sharingModality != null && sharingModality.CanInvoke(ModalityAction.GrantSharingControl))
                {
                    sharingModality.BeginGrantControl((ar) => { sharingModality.EndGrantControl(ar); }
                        , null);
                }
            }
            catch (KeyNotFoundException) { MessageBox.Show("Chosen participant does not have an application sharing modality.", "Grant control error"); }
            catch (NullReferenceException) { MessageBox.Show("Chosen participant does not have an application sharing modality.", "Grant control error"); }

        }

        /// <summary>
        ///Release control of a remotely owned resource and shared resource.
        ///The Release button is enabled when the _sharingModality_ActionAvailabilityChanged event handler is
        ///called with the event argument that specifies the ModalityAction.ReleaseSharingControl action is available.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Release_Button_Click(object sender, EventArgs e)
        {
            if (_LocalParticipantSharingModality != null && _LocalParticipantSharingModality.CanInvoke(ModalityAction.ReleaseSharingControl))
            {
                _LocalParticipantSharingModality.BeginReleaseControl((ar) => { _LocalParticipantSharingModality.EndReleaseControl(ar); }, null);
            }
        }

        /// <summary>
        ///Request control of a remotely owned resource and shared resource.
        ///The Request button is enabled when the _sharingModality_ActionAvailabilityChanged event handler is called
        ///with the event argument that specifies the ModalityAction.RequestSharingControl action is available.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Request_Button_Click(object sender, EventArgs e)
        {
            if (_LocalParticipantSharingModality != null && _LocalParticipantSharingModality.CanInvoke(ModalityAction.RequestSharingControl))
            {
                _LocalParticipantSharingModality.BeginRequestControl((ar) => 
                {
                    try
                    {
                        _LocalParticipantSharingModality.EndRequestControl(ar);
                    }
                    catch (LyncClientException lce)
                    {
                        System.Diagnostics.Debug.WriteLine("Lync client exception on request control: " + lce.Message);
                    }
                }
                , null);
            }
        }

        /// <summary>
        ///Revoke control of a remotely controlled resource and locally owned shared resource.
        ///The Revoke button is enabled when the _sharingModality_ActionAvailabilityChanged event handler is called
        ///with the event argument that specifies the ModalityAction.RevokeSharingControl action is available.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Revoke_Button_Click(object sender, EventArgs e)
        {
            if (_ResourceControllingContact == null)
            {
                return;
            }
            ApplicationSharingModality sharingModality = (ApplicationSharingModality)_participantSharingModalities[_ResourceControllingContact.Uri];
            if (sharingModality != null && sharingModality.CanInvoke(ModalityAction.RevokeSharingControl))
            {
                sharingModality.BeginRevokeControl((ar) => 
                {
                    try
                    {
                        sharingModality.EndRevokeControl(ar);
                    }
                    catch (OperationException oe) { MessageBox.Show("Operation exception " + oe.Message); }
                }
                , null);
            }

        }

        private void EndConversation_Button_Click(object sender, EventArgs e)
        {
            if (_conversation == null)
            {
                return;
            }
            if (_conversation.State != ConversationState.Terminated)
            {
                _conversation.End();
            }
        }

        /// <summary>
        /// Starts a new conversation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_Button_Click(object sender, EventArgs e)
        {
            if (_conversation == null)
            {
                foreach (Object selectedObject in Contact_ListBox.SelectedItems)
                {
                    if (!_selectedContacts.ContainsKey(selectedObject.ToString()))
                    {
                        _selectedContacts.Add(selectedObject.ToString(), _LyncClient.ContactManager.GetContactByUri(selectedObject.ToString()));
                    }
                }
                _conversation = _LyncClient.ConversationManager.AddConversation();
            }

        }

        /// <summary>
        /// Disconnects the conversation application sharing modality so that the user
        /// is no longer sharing or viewing a resource.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopSharing_Button_Click(object sender, EventArgs e)
        {
            if (_sharingModality != null && _sharingModality.CanInvoke(ModalityAction.Disconnect))
            {
                _sharingModality.BeginDisconnect(ModalityDisconnectReason.None, (ar) => 
                {
                    _sharingModality.EndDisconnect(ar);
                    if (_chromeRunner != null)
                    {
                        this.Invoke(new NoParamDelegate(_chromeRunner.CloseTheChrome));
                    }
                }, null);

            }
        }


        /// <summary>
        /// Accepts an invitation to connect to a resource sharing modality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcceptSharing_Button_Click(object sender, EventArgs e)
        {
            if (_sharingModality != null)
            {
                //Register for the application sharing modality event on the conversation itself
                _sharingModality = (ApplicationSharingModality)_conversation.Modalities[ModalityTypes.ApplicationSharing];
            }
            _sharingModality.Accept();
            
        }

        /// <summary>
        /// Rejects an invitiation to connect to a resource sharing modality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RejectSharing_Button_Click(object sender, EventArgs e)
        {
            _sharingModality.Reject(ModalityDisconnectReason.Decline);
        }

        /// <summary>
        /// Updates the locally shareable resource list with the newest
        /// list of locally shareable resources
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshResource_Button_Click(object sender, EventArgs e)
        {
            UpdateSharedResourcesListbox();
        }

        /// <summary>
        /// Initializes the Lync.exe process and signs the user in to Lync if in UI suppression mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartLync_Button_Click(object sender, EventArgs e)
        {
            ClearAllContacts();
            Start_Button.Enabled = false;
            EndConversation_Button.Enabled = false;
            StartSharingResource_Button.Enabled = false;
            _userSignIn.StartUpLync(SideCheck.Checked);
            _LyncClient = _userSignIn.Client;
            ClientStateString_Label.Text = _LyncClient.State.ToString();
        }

        /// <summary>
        /// Signs the user out of Lync and cancels the form close op.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShareResources_Form_FormClosing(object sender, FormClosingEventArgs e)
        {

            try
            {
                if (_userSignIn.ThisProcessInitializedLync == true)
                {
                    if (_LyncClient.State == ClientState.SignedIn)
                    {
                        _LyncClient.BeginSignOut((ar) => { _LyncClient.EndSignOut(ar); }, null);
                        e.Cancel = true;
                    }
                }
            }
            catch (LyncClientException)
            {
            }

        }

        /// <summary>
        /// Signs out of Lync and uninitializes the Lync.exe process if this application
        /// process initialized the Lync.exe process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopLync_Button_Click(object sender, EventArgs e)
        {
            try
            {
                if (_userSignIn.ThisProcessInitializedLync == true)
                {
                    if (_LyncClient.State == ClientState.SignedIn)
                    {
                        _LyncClient.BeginSignOut((ar) => { _LyncClient.EndSignOut(ar); }, null);
                    }
                }
            }
            catch (LyncClientException)
            {
            }

        }

        /// <summary>
        /// Changes the view mode of the application sharing view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoSizeView_Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (_sharingModality == null || _sharingModality.View.DisplayMode == ApplicationSharingViewDisplayMode.Invalid)
            {
                return;
            }
            CheckBox fitToParent_checkBox = (CheckBox)sender;
            if (fitToParent_checkBox.Checked == true)
            {
                _sharingModality.View.DisplayMode = ApplicationSharingViewDisplayMode.FitToParent;
            }
            else
            {
                _sharingModality.View.DisplayMode = ApplicationSharingViewDisplayMode.ActualSize;
            }
            _sharingModality.View.SyncRectangle();
        }


        #endregion

        #region Client event handlers

        private void CloseWindow()
        {
            this.Close();
        }

        #endregion

        #region API Operation callback methods


        /// <summary>
        /// Completes the desktop sharing operation.
        /// </summary>
        /// <param name="ar"></param>
        private void ShareDesktopCallback(System.IAsyncResult ar)
        {
            try
            {
                ApplicationSharingModality sharingModality = (ApplicationSharingModality)ar.AsyncState;
                sharingModality.EndShareDesktop(ar);
            }
            catch (LyncClientException )  {}
            catch (InvalidCastException) { };
        }

        /// <summary>
        /// Completes the process sharing operation
        /// </summary>
        /// <param name="ar"></param>
        private void ShareResourcesCallback(System.IAsyncResult ar)
        {
            try
            {
                ((ApplicationSharingModality)ar.AsyncState).EndShareResources(ar);
            }
            catch (OperationException) { }
            catch (LyncClientException ) {}
            catch (InvalidCastException) { };

        }

        /// <summary>
        /// Completes the monitor sharing operation
        /// </summary>
        /// <param name="ar"></param>
        private void ShareMonitorCallback(System.IAsyncResult ar)
        {
            try
            {
                ApplicationSharingModality sharingModality = (ApplicationSharingModality)ar.AsyncState;
            }
            catch (InvalidCastException ) {}
            catch (LyncClientException )  {}

        }


        #endregion

        #region UI update helper methods and their delegates

        /// <summary>
        /// Shares the resource selected by the user.
        /// </summary>
        private void ShareSelectedResource(SharingResource_Wrapper selectedResource)
        {
            if (MessageBox.Show(
                "You have chosen to share " 
                + selectedResource 
                + ". Is this correct?", 
                "Confirm Share Selection",
                MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            SharingResourceType selectedResourceType = SharingResourceType.Invalid;
            SharingResource sharingResource = null;

            foreach (SharingResource s in _sharingModality.ShareableResources)
            {
                if (s.Id == selectedResource.ResourceId)
                {
                    //Get the type of resource selected by the user
                    selectedResourceType = s.Type;
                    sharingResource = s;
                    break;
                }
            }

            CanShareDetail sharingDetail;
            if (!_sharingModality.CanShare(selectedResourceType, out sharingDetail))
            {
                switch (sharingDetail)
                {
                    case CanShareDetail.DisabledByOrganizerPolicy:
                        MessageBox.Show("The conversation organizer has disallowed sharing" );
                        break;
                    case CanShareDetail.DisabledByPolicy:
                        MessageBox.Show("Sharing resources is not allowed ");
                        break;
                    case CanShareDetail.DisabledByRole:
                        MessageBox.Show("Conference attendees cannot share resources" );
                        break;
                }
                return;
            }
            this.Invoke(new ChangeButtonTextDelegate(ChangeButtonText), new object[] { Disconnect_Button, "Stop sharing" });
            if (selectedResourceType == SharingResourceType.Desktop)
            {
                _sharingModality.BeginShareDesktop((ar) =>
                {
                    _sharingModality.EndShareDesktop(ar);
                }
                    , null);

                if (ShowChrome_Checkbox.Checked == true)
                {
                    Screen[] screens;
                    screens = Screen.AllScreens;
                    _chromeRunner = new ChromeRunner();
                    foreach (Screen screen in screens)
                    {
                        _chromeRunner.ShowDesktopChrome(screen);
                    }
                }

            }
            else if (selectedResourceType == SharingResourceType.Monitor)
            {
                _sharingModality.BeginShareResources(sharingResource, (ar) =>
                    {
                        _sharingModality.EndShareResources(ar);
                    }, null);

                if (ShowChrome_Checkbox.Checked == true)
                {
                    _chromeRunner = new ChromeRunner();
                    if (sharingResource.Name.Contains("Primary"))
                    {
                        _chromeRunner.ShowDesktopChrome(Screen.PrimaryScreen);
                    }
                    else
                    {
                        _chromeRunner.ShowDesktopChrome(Screen.AllScreens[1]);
                    }
                }
            }
            else
            {
                _sharingModality.BeginShareResources(sharingResource, (ar) =>
                {
                    try
                    {
                        _sharingModality.EndShareResources(ar);

                    }
                    catch (OperationException oe) { throw oe; }
                    catch (LyncClientException lce) { throw lce; }
                }
                , null);

                if (ShowChrome_Checkbox.Checked == true)
                {
                    _chromeRunner = new ChromeRunner();
                    _chromeRunner.ShowProcessChrome(sharingResource);
                }
            }
        }

        private delegate void NoParamDelegate();
        private delegate void TwoIntParamDelegate(int height, int width);
        private delegate void EnableDisableButtonDelegate(Button buttonToUpdate, Boolean newButtonEnableState);

        /// <summary>
        /// Enables or disables a UI button based on the actionAvailability
        /// </summary>
        /// <param name="buttonToUpdate"></param>
        /// <param name="newButtonEnableState"></param>
        private void EnableDisableButton(Button buttonToUpdate, Boolean newButtonEnableState)
        {
            buttonToUpdate.Enabled = newButtonEnableState;
        }

        /// <summary>
        /// Iterates on all groups and adds an Uri string for each contact in a group to a list on the UI
        /// </summary>
        private void LoadAllContacts()
        {
            //Set the selection mode of the UI list to multi-select
            Contact_ListBox.SelectionMode = SelectionMode.MultiSimple;
            Contact_ListBox.Items.Clear();
            try
            {
                foreach (Group group in _LyncClient.ContactManager.Groups)
                {
                    foreach (Contact contact in group)
                    {
                        if (!Contact_ListBox.Items.Contains(contact.Uri))
                            Contact_ListBox.Items.Add(contact.Uri);
                    }
                }
            }
            catch (LyncClientException lce)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(
                    System.DateTime.Now.Hour.ToString() +
                    ":" +
                    System.DateTime.Now.Minute.ToString() +
                    ":" +
                    System.DateTime.Now.Second.ToString() +
                    ":" +
                    System.DateTime.Now.Millisecond.ToString() +
                    " LoadAllContacts, Lync Client Exception: " + lce.Message);
#endif
            }
        }

        private delegate void ClearAllContactsDelegate();
        private void ClearAllContacts()
        {
            Contact_ListBox.Items.Clear();
        }

        private delegate void AddAContactDelegate(string ContactUri);
        private void AddAContact(string ContactUri)
        {
            Contact_ListBox.SelectionMode = SelectionMode.One;
            if (!Contact_ListBox.Items.Contains(ContactUri))
            {
                Contact_ListBox.Items.Add(ContactUri);
            }
        }


        private delegate void RemoveAContactDelegate(string ContactUri);
        private void RemoveAContact(string ContactUri)
        {
            Contact_ListBox.Items.Remove(ContactUri);
        }

        private delegate void UpdateSharedResourcesListboxDelegate();

        /// <summary>
        /// Fills a UI list with the names of all local resources that can be shared.
        /// </summary>
        private void UpdateSharedResourcesListbox()
        {
            SharedResources_ListBox.Items.Clear();
            if (_sharingModality == null || _sharingModality.ShareableResources == null)
            {
                return;
            }
            if (_sharingModality.ShareableResources.Count > 0)
            {
                SharingResource sharingResource;
                for (int j = 0; j < _sharingModality.ShareableResources.Count; j++)
                {
                    sharingResource = _sharingModality.ShareableResources[j];
                    SharingResource_Wrapper wrap = new SharingResource_Wrapper(sharingResource);
                    SharedResources_ListBox.Items.Add(wrap);
                }
            }
        }


        private delegate void ChangeButtonTextDelegate(Button buttonToChange, string newText);
        private void ChangeButtonText(Button buttonToChange, string newText)
        {
            buttonToChange.Text = newText;
        }

        private delegate void SetContactSelectionModeDelegate(SelectionMode newMode);
        /// <summary>
        /// Changes the contact list on the UI from single-select to multi-select mode
        /// </summary>
        /// <param name="newMode"></param>
        private void SetContactSelectionMode(SelectionMode newMode)
        {
            Contact_ListBox.SelectionMode = newMode;
        }

        private delegate void ChangeLabelTextDelegate(Label labelToUpdate, string newText);
        /// <summary>
        /// Replaces the text of any label control on the UI with new text
        /// </summary>
        /// <param name="labelToUpdate"></param>
        /// <param name="newText"></param>
        private void ChangeLabelText(Label labelToUpdate, string newText)
        {
            labelToUpdate.Text = newText;
        }

        #endregion

        #region constructors
        public ShareResources_Form()
        {
            InitializeComponent();
        }
        #endregion


    }
}
