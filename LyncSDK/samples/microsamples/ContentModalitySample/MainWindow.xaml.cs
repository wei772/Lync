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
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

// Used by this sample.
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.Sharing;

namespace ContentModalitySample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region private fields
        
        // used for managing sample's user interface. 
        private Dispatcher dispatcher;
        // related to Lync APIs
        private LyncClient _lyncClient;
        private Conversation _conversation;
        private ContentSharingModality _contentModality;
        // Used for creating conversation and content.
        private string targetUri;
        private string title;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        #region Window related events.

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // get the Lync client.
            try
            {
                _lyncClient = LyncClient.GetClient();
            }
            catch (ClientNotFoundException exception)
            {
                // Lync application should be running.
                Log(exception.ToString());
                return;
            }
            catch (NotSignedInException exception)
            {
                // This sample require user to be signed into Lync application
                // Sign in can be initiated through Lync APIs as well. See SignIn
                // sample for that code.
                Log(exception.ToString());
                return;
            }
            catch (Exception exception)
            {
                Log(exception.ToString());
                return;
            }

            // Subscribe to ConversationAdded event to know when someone tries to call you to share content.
            _lyncClient.ConversationManager.ConversationAdded += new EventHandler<ConversationManagerEventArgs>(ConversationManager_ConversationAdded);
            _lyncClient.ConversationManager.ConversationRemoved += new EventHandler<ConversationManagerEventArgs>(ConversationManager_ConversationRemoved);

        }

        private void buttonCreateConversation_Click(object sender, RoutedEventArgs e)
        {
            // this sample only creates and handles one conversation
            if (_conversation == null)
            {
                targetUri = textBoxShareWith.Text;

                // Conversation has not been created yet, lets create it
                _lyncClient.ConversationManager.AddConversation();
                // above statement will create conversation and result in ConversationAdded Event
                // participant will be added in that event handler.
            }
        }

        private void buttonCreateWhiteBoard_Click(object sender, RoutedEventArgs e)
        {

            // title of each content should be different
            title = textBoxTitle.Text;

            try
            {
                if (_contentModality.CanInvoke(ModalityAction.CreateShareableWhiteboardContent))
                {
                    // This way of calling will block the app until Content is created. To create a more responsive app you can
                    // use an AsyncCallback Delegate to end this asynchoronous opertation. More information about this C# concept
                    // can be found on MSDN
                    IAsyncResult result = _contentModality.BeginCreateContent(ShareableContentType.Whiteboard, title, null, null);
                    ShareableContent whiteBoardContent = _contentModality.EndCreateContent(result);

                    // The newly created content is only present on local computer. Lets upload it to conference
                    // where other users can also see the content listed in their content bin. 
                    int reason; // if can invoke returns false, you can get the reason from here.
                    if (whiteBoardContent.CanInvoke(ShareableContentAction.Upload, out reason))
                    {
                        whiteBoardContent.Upload();
                        // above call will start uploading the content to conference. Once the content is ready its 
                        // state will change from Offline to Online thats when Present() can be called.
                    }
                }
            }
            catch (ContentTitleExistException)
            {
                Log("Content Title Exists Exception: Please enter a unique title and try again.");
            }
            catch (NullReferenceException)
            {
                Log("Null Reference Exception: Did you create the conversation first? If yes, try restarting the sample.");
            }
            catch (Exception excep)
            {
                Log(excep.ToString());
            }
        }

        private void buttonCreatePPT_Click(object sender, RoutedEventArgs e)
        {
            // title of each content should be different
            title = textBoxTitle.Text;

            try
            {
                if (_contentModality.CanInvoke(ModalityAction.CreateShareablePowerPointContent))
                {
                    // This way of calling will block the app until Content is created. To create a more responsive app you can
                    // use an AsyncCallback Delegate to end this asynchoronous opertation. More information about this C# concept
                    // can be found on MSDN
                    IAsyncResult result = _contentModality.BeginCreateContentFromFile(ShareableContentType.PowerPoint, title, textBoxPPTFilePath.Text, true, null, null);
                    ShareableContent pptContent = _contentModality.EndCreateContentFromFile(result);

                    // The newly created content is only present on local computer. Lets upload it to conference
                    // where other users can also see the content listed in their content bin. 
                    int reason; // if can invoke returns false, you can get the reason from here.
                    if (pptContent.CanInvoke(ShareableContentAction.Upload, out reason))
                    {
                        pptContent.Upload();
                        // above call will start uploading the content to conference. Once the content is ready its 
                        // state will change from Offline to Online thats when Present() can be called.
                    }
                }
            }
            catch (NullReferenceException)
            {
                Log("Null Reference Exception: Did you create the conversation first? If yes, try restarting the sample.");
            }
            catch (ContentTitleExistException)
            {
                Log("Content Title Exists Exception: Please enter a unique title and try again.");
            }
            catch (LyncClientException)
            {
                Log("Enter a valid path to the PowerPoint presentation.");
            }
            catch (Exception excep)
            {
                Log(excep.ToString());
            }
        }

        private void buttonForward_Click(object sender, RoutedEventArgs e)
        {
            ShareableContent selectedContent = (ShareableContent)listBoxContentBin.SelectedItem;

            if ( selectedContent != null)
            {
                if (selectedContent.Type == ShareableContentType.PowerPoint)
                {
                    if (selectedContent.State == ShareableContentState.Active)
                    {
                        // Only call this method if its PPT content and its currently being presented.
                        PowerPointContent pptC = (PowerPointContent)selectedContent;
                        pptC.GoForward();
                    }
                    else Log("Present this PowerPoint before calling this action");
                }
                else Log("Forward action is only supported for PowerPoint Content. Present PowerPoint content and then call this action.");
            }
            else Log("Select a content from Content Bin first");
        }

        private void buttonBackward_Click(object sender, RoutedEventArgs e)
        {
            ShareableContent selectedContent = (ShareableContent)listBoxContentBin.SelectedItem;

            if (selectedContent != null)
            {
                if (selectedContent.Type == ShareableContentType.PowerPoint)
                {
                    if (selectedContent.State == ShareableContentState.Active)
                    {
                        // Only call this method if its PPT content and its currently being presented.
                        PowerPointContent pptC = (PowerPointContent)selectedContent;
                        pptC.GoBackward();
                    }
                    else Log("Present this PowerPoint before calling this action"); 
                }
                else Log("Backward action is only supported for PowerPoint Content. Present PowerPoint content and then call this action.");
            }
            else Log("Select a content from Content Bin first");
        }

        private void buttonPresent_Click(object sender, RoutedEventArgs e)
        {
            ShareableContent selectedContent = (ShareableContent)listBoxContentBin.SelectedItem;

            if (selectedContent != null)
            {
                int reason; // if can invoke returns false, you can get the reason from here.
                if (selectedContent.CanInvoke(ShareableContentAction.Present, out reason))
                {
                    selectedContent.Present();
                }
            }
            else
                Log("Select a content from Content Bin first");
        }

        private void buttonStopPresenting_Click(object sender, RoutedEventArgs e)
        {
            ShareableContent selectedContent = (ShareableContent)listBoxContentBin.SelectedItem;

            if (selectedContent != null)
            {
                int reason; // if can invoke returns false, you can get the reason from here.
                if (selectedContent.CanInvoke(ShareableContentAction.StopPresenting, out reason))
                {
                    selectedContent.StopPresenting();
                }
            }
            else
                Log("Select a content from Content Bin first");
        }

        private void buttonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (_contentModality != null)
            {
                if (_contentModality.State == ModalityState.Connected)
                {
                    IAsyncResult result = _contentModality.BeginDisconnect(ModalityDisconnectReason.None, null, null);
                    _contentModality.EndDisconnect(result);
                }
            }
            else
                Log("No conversation to disconnect.");
        }

        private void buttonAccept_Click(object sender, RoutedEventArgs e)
        {
            if (_contentModality != null)
            {
                if (_contentModality.CanInvoke(ModalityAction.Accept))
                {
                    _contentModality.Accept();
                }
            }
            else
                Log("No conversation to Accept.");
        }

        #endregion

        #region Lync Event Handlers.

        void ConversationManager_ConversationAdded(object sender, ConversationManagerEventArgs e)
        {
            try
            {
                // this sample will only handle one conversation
                if (_conversation == null)
                {
                    _conversation = e.Conversation;

                    if (_conversation.Modalities[ModalityTypes.ContentSharing].State == ModalityState.Notified)
                    {
                        // Conversation orignated with remote SIP user. Lets set the modality.
                        // Accept will happen when user clicks Accept button in UI.
                        _contentModality = (ContentSharingModality)_conversation.Modalities[ModalityTypes.ContentSharing];
                        // Following event will help maintain the Content Bin 
                        _contentModality.ContentAdded += new EventHandler<ContentCollectionChangedEventArgs>(_contentModality_ContentAdded);

                    }
                    else
                    {
                        // lets add a participant
                        if (_conversation.CanInvoke(ConversationAction.AddParticipant))
                        {
                            _conversation.ParticipantAdded += new EventHandler<ParticipantCollectionChangedEventArgs>(Conversation_ParticipantAdded);
                            _conversation.AddParticipant(_lyncClient.ContactManager.GetContactByUri(targetUri));
                            // above call will result in ParticipantAdded event.
                            // In that event handler, lets Connect the ContentModality.
                        }
                    }
                }
            }
            catch (ArgumentException)
            {
                dispatcher.BeginInvoke(new Action<string>(Log), "Argument Exception: Is the SIP address a valid URI?");
            }
        }

        void Conversation_ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
        {
            // if this is the remote participant then lets connect the ContentSharing modality.
            if (e.Participant.IsSelf != true)
            {
                _contentModality = (ContentSharingModality)((Conversation)sender).Modalities[ModalityTypes.ContentSharing];
                // Following events will help maintain the Content Bin 
                _contentModality.ContentAdded += new EventHandler<ContentCollectionChangedEventArgs>(_contentModality_ContentAdded);

                if (_contentModality.CanInvoke(ModalityAction.Connect))
                {
                    try
                    {
                        // This will send a notification to remote party.
                        _contentModality.BeginConnect(HandleCallBacks, "Connect");
                    }
                    catch (Exception ec)
                    {
                        dispatcher.BeginInvoke(new Action<string>(Log), ec.ToString());
                    }
                }
            }
        }

        void ConversationManager_ConversationRemoved(object sender, ConversationManagerEventArgs e)
        {
            if (_conversation != null)
            {
                if (e.Conversation.Equals(_conversation))
                {
                    _conversation = null;
                }
            }
        }

        void _contentModality_ContentAdded(object sender, ContentCollectionChangedEventArgs e)
        {
            // lets be aware of when state of this content changes.
            e.Item.StateChanged += new EventHandler<ShareableContentStateChangedEventArgs>(Content_StateChanged);
        }

        void Content_StateChanged(object sender, ShareableContentStateChangedEventArgs e)
        {
            // Lets update content bin. Some content may have moved to Online state or could be removed.
            // This call can be optimized to only happen if e.NewState is ShareableContentState.Online
            // Active or Unusable
            dispatcher.BeginInvoke(new Action(UpdateContentBin));
        }

        #endregion

        // Updates the Content Bin ListBox
        public void UpdateContentBin()
        {
            if (_contentModality != null)
            {
                try
                {
                    // clear the list box.
                    listBoxContentBin.Items.Clear();

                    foreach (ShareableContent c in _contentModality.ContentCollection)
                    {
                        // Content is local to the user unless Upload methods has been called to upload it to the server. 
                        // Then its state is Online and its in content bin of all Participants. A content in Content Bin 
                        // which is being presented will have Active state so it should also be part of Content Bin.
                        if (c.State == ShareableContentState.Online || c.State == ShareableContentState.Active)
                        {
                            listBoxContentBin.Items.Add(c);
                            // Ensure Title is listed in the UI to help user distinguish between different contents.
                            listBoxContentBin.DisplayMemberPath = "Title";
                        }
                    }
                }
                catch (Exception e)
                {
                    dispatcher.BeginInvoke(new Action<string>(Log), e.ToString());
                }
            }
        }

        public void HandleCallBacks(IAsyncResult result)
        {
            string callBackFor = (string)result.AsyncState;

            switch (callBackFor)
            {

                // Content Modality
                case "CreateContent":
                    break;
                case "CreateContentFromFile":
                    break;
                case "Connect":
                    _contentModality.EndConnect(result);
                    break;

                default:
                    break;
            }
        }

        #region Helper methods

        /// <summary>
        /// Used to share information about errors.
        /// </summary>
        private void Log(string msg)
        {
            textBoxLog.Text = DateTime.Now.ToString("T") + " - " + msg + "\n" + textBoxLog.Text;
        }

        #endregion

    }
}
