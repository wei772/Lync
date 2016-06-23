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
        private ShareableContent whiteBoardContent;
        private ShareableContent pptContent;
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
                // User should be signed into Lync application
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

        private void buttonShareWhiteBoard_Click(object sender, RoutedEventArgs e)
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
                    whiteBoardContent = _contentModality.EndCreateContent(result);
                    whiteBoardContent.StateChanged += new EventHandler<ShareableContentStateChangedEventArgs>(whiteBoardContent_StateChanged);

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
                Log("Title already exists. Please enter a unique title and try again.");
            }
            catch (NullReferenceException)
            {
                Log("Null Reference Exception: Did you create the conversation first? If yes, you can try restarting the sample.");
            }
        }

        private void buttonSharePPT_Click(object sender, RoutedEventArgs e)
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
                    pptContent = _contentModality.EndCreateContentFromFile(result);
                    pptContent.StateChanged += new EventHandler<ShareableContentStateChangedEventArgs>(pptContent_StateChanged);

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
                Log("Null Reference Exception: Did you create the conversation first? If yes, you can try restarting the sample.");
            }
            catch (Exception excep)
            {
                Log(excep.ToString());
            }
        }

        private void buttonForward_Click(object sender, RoutedEventArgs e)
        {
            if (pptContent != null)
            {
                if (pptContent.State == ShareableContentState.Active)
                {
                    PowerPointContent pptC =  (PowerPointContent) pptContent;
                    pptC.GoForward();
                }
            }
        }

        private void buttonBackward_Click(object sender, RoutedEventArgs e)
        {
            if (pptContent != null)
            {
                if (pptContent.State == ShareableContentState.Active)
                {
                    PowerPointContent pptC = (PowerPointContent)pptContent;
                    pptC.GoBackward();
                }
            }
        }

        private void buttonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (_contentModality.State == ModalityState.Connected)
            {
                IAsyncResult result = _contentModality.BeginDisconnect(ModalityDisconnectReason.None, null, null);
                _contentModality.EndDisconnect(result);
            }
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
        }

        #endregion

        #region Lync Event Handlers.

        void ConversationManager_ConversationAdded(object sender, ConversationManagerEventArgs e)
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

        void Conversation_ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
        {
            // if this is the remote participant then lets connect the ContentSharing modality.
            if (e.Participant.IsSelf != true)
            {
                _contentModality = (ContentSharingModality)((Conversation)sender).Modalities[ModalityTypes.ContentSharing];

                if (_contentModality.CanInvoke(ModalityAction.Connect))
                {
                    try
                    {
                        _contentModality.BeginConnect(HandleCallBacks, "Connect");
                    }
                    catch (Exception ec)
                    {
                        dispatcher.BeginInvoke(new Action<string>(Log), ec.ToString());
                    }
                }
            }

        }

        void whiteBoardContent_StateChanged(object sender, ShareableContentStateChangedEventArgs e)
        {
            if ((e.OldState == ShareableContentState.Connecting) && (e.NewState == ShareableContentState.Online))
            {
                // Lets make the content visible.
                whiteBoardContent.Present();
            }
        }

        void pptContent_StateChanged(object sender, ShareableContentStateChangedEventArgs e)
        {
            if ((e.OldState == ShareableContentState.Connecting) && (e.NewState == ShareableContentState.Online))
            {
                // Lets make the content visible.
                pptContent.Present();
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

        #endregion

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
