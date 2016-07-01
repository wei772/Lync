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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using MessageBox = System.Windows.MessageBox;
using System.IO;
using System.Windows.Media.Imaging;
using System.Text;

namespace StartConversation
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum CheckedRadio
        {
            HTML = 1,
            RTF = 2,
            PlainText = 3,
        }
        Microsoft.Lync.Model.LyncClient client = null;
        Microsoft.Lync.Model.Extensibility.Automation automation = null;
        InstantMessageModality _ConversationImModality;
        string RemoteUserUri = "";

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                //Start the conversation
                automation = LyncClient.GetAutomation();
                client = LyncClient.GetClient();
                
            
            }
            catch (LyncClientException lyncClientException)
            {
                MessageBox.Show("Failed to connect to Lync." + lyncClientException.Message);
                Console.Out.WriteLine(lyncClientException);
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    MessageBox.Show("Failed to connect to Lync." + systemException.Message);
                    Console.WriteLine("Error: " + systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }
        }

        /// <summary>
        /// Async callback method invoked by InstantMessageModality instance when SendMessage completes
        /// </summary>
        /// <param name="_asyncOperation">IAsyncResult The operation result</param>
        /// 
        private void SendMessageCallback(IAsyncResult ar)
        {
            InstantMessageModality imModality = (InstantMessageModality)ar.AsyncState;

            try
            {
                imModality.EndSendMessage(ar);
            }
            catch (LyncClientException lce)
            {
                MessageBox.Show("Lync Client Exception on EndSendMessage " + lce.Message);
            }
        }


        void conversationManager_ConversationAdded(object sender, ConversationManagerEventArgs e)
        {
            try
            {
                ModalityState newState = e.Conversation.Modalities[ModalityTypes.InstantMessage].State;
                if (newState == ModalityState.Connected || newState == ModalityState.Notified)
                {
                    _ConversationImModality = (InstantMessageModality)e.Conversation.Modalities[ModalityTypes.InstantMessage];
                }

                e.Conversation.ParticipantAdded += new EventHandler<ParticipantCollectionChangedEventArgs>(Conversation_ParticipantAdded);
               // e.Conversation.AddParticipant(client.ContactManager.GetContactByUri(RemoteUserUri));
            }
            catch (LyncClientException lce)
            {
                System.Diagnostics.Debug.WriteLine("LYnc client exception on conversationadded " + lce.Message);
            }
        }

        void Conversation_ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
        {
            // add event handlers for modalities of participants other than self participant:
            if (e.Participant.IsSelf == false)
            {
                if (((Conversation)sender).Modalities.ContainsKey(ModalityTypes.InstantMessage))
                {
                    ((InstantMessageModality)e.Participant.Modalities[ModalityTypes.InstantMessage]).InstantMessageReceived += ConversationTest_InstantMessageReceived;
                }
               
            }
        }

        void ConversationTest_IsTypingChanged(object sender, IsTypingChangedEventArgs e)
        {

        }

        void ConversationTest_InstantMessageReceived(object sender, MessageSentEventArgs e)
        {
            ShowNewMessage(e);
            try
            {
                if (_ConversationImModality.CanInvoke(ModalityAction.SendInstantMessage))
                {
                    _ConversationImModality.BeginSendMessage(
                        "Got your message",
                        (ar) => 
                        {
                            _ConversationImModality.EndSendMessage(ar);
                        }
                        ,
                        null);
                }
            }
            catch (LyncClientException ex)
            {
                txtErrors.Text = ex.Message;
            }

        }

        private void ShowNewMessage(MessageSentEventArgs e)
        {
            string rtfString = string.Empty;
            if (e.Contents.TryGetValue(InstantMessageContentType.RichText, out rtfString))
            {
                //display the rtf in an image control
                this.Dispatcher.Invoke(new FillRichTextBoxDelegate(FillRichTextBox), new object[] { richTextBox1, rtfString });

            }

            string htmlString = string.Empty;
            if (e.Contents.TryGetValue(InstantMessageContentType.Html, out htmlString))
            {
                //display the html in an image control
                //webBrowser1.NavigateToString(htmlString);
                this.Dispatcher.Invoke(new FillWebBrowserDelegate(FillWebBrowser),new object[] {webBrowser1, htmlString});
            }

            string plainString = string.Empty;
            if (e.Contents.TryGetValue(InstantMessageContentType.PlainText, out plainString))
            {
                //display the plain text  in an image control
                this.Dispatcher.Invoke(new FillTextBoxDelegate(FillTextBox), new object[] { textBox1, plainString });
                
            }
        }

        private delegate void FillWebBrowserDelegate(System.Windows.Controls.WebBrowser webBrowser, string body);
        private delegate void FillTextBoxDelegate(System.Windows.Controls.TextBox textBox, string text);
        private delegate void FillRichTextBoxDelegate(System.Windows.Controls.RichTextBox textBox, string text);

        private void FillRichTextBox(System.Windows.Controls.RichTextBox richTextBox, string richText)
        {
            FlowDocument doc = new FlowDocument();
            TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
            byte[] byteArray = Encoding.ASCII.GetBytes( richText );
            MemoryStream stream = new MemoryStream( byteArray );
         
            range.Load(stream, DataFormats.Rtf);
            richTextBox.Document = doc;
        }

        private void FillTextBox(System.Windows.Controls.TextBox textBox, string text)
        {
            textBox.Text = text;
        }

        private void FillWebBrowser(System.Windows.Controls.WebBrowser webBrowser, string body)
        {
                webBrowser.NavigateToString(body);

        }

        /// <summary>
        /// Identify if a particular SystemException is one of the exceptions which may be thrown
        /// by the Lync Model API.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private bool IsLyncException(SystemException ex)
        {
            return
                ex is NotImplementedException ||
                ex is ArgumentException ||
                ex is NullReferenceException ||
                ex is NotSupportedException ||
                ex is ArgumentOutOfRangeException ||
                ex is IndexOutOfRangeException ||
                ex is InvalidOperationException ||
                ex is TypeLoadException ||
                ex is TypeInitializationException ||
                ex is InvalidComObjectException ||
                ex is InvalidCastException;
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            client.ConversationManager.ConversationAdded += conversationManager_ConversationAdded;
        }
    }
}
