/*=====================================================================
  This file is part of the Microsoft Lync Code Samples.

  Copyright (C) 2010 Microsoft Corporation.  All rights reserved.

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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Documents;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Extensibility;
using MessageBox = System.Windows.MessageBox;
using Microsoft.Lync.Model.Conversation;

namespace StartConversation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Microsoft.Lync.Model.LyncClient client = null;
        Microsoft.Lync.Model.Extensibility.Automation automation = null;
        
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
                MessageBox.Show("Failed to connect to Lync.");
                Console.Out.WriteLine(lyncClientException);
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    MessageBox.Show("Failed to connect to Lync.");
                    Console.WriteLine("Error: " + systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client.ConversationManager.ConversationAdded += new EventHandler<Microsoft.Lync.Model.Conversation.ConversationManagerEventArgs>(ConversationManager_ConversationAdded);
        }

        /// <summary>
        /// Handles ConversationAdded state change event raised on ConversationsManager
        /// </summary>
        /// <param name="source">ConversationsManager The source of the event.</param>
        /// <param name="data">ConversationsManagerEventArgs The event data. The incoming Conversation is obtained here.</param>
        void ConversationManager_ConversationAdded(object sender, ConversationManagerEventArgs data)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            if (data.Conversation.Modalities[ModalityTypes.InstantMessage].State == ModalityState.Connected)
            {
                sb.Append("Incoming IM from ");
            }
            string callerName = data.Conversation.Participants[1].Contact.GetContactInformation(ContactInformationType.DisplayName).ToString();
            sb.Append(callerName);
            sb.Append(System.Environment.NewLine);
            sb.Append("Do you want to Ignore the invitiation? IM P2P is always auto-accepted");
            if (System.Windows.Forms.MessageBox.Show(
                sb.ToString()
                , "Incoming Invitation"
                , System.Windows.Forms.MessageBoxButtons.YesNo
                , System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
            {
                data.Conversation.ParticipantAdded += Conversation_ParticipantAdded;
                data.Conversation.StateChanged += new EventHandler<ConversationStateChangedEventArgs>(Conversation_StateChanged);
            }
            else
            {
                data.Conversation.End();
            }
        }
           
        void MainWindow_ModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {
            if (e.NewState == ModalityState.Connected)
                MessageBox.Show("IM Modality Connected"); 
        }

        void Conversation_StateChanged(object sender, Microsoft.Lync.Model.Conversation.ConversationStateChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Conversation_ParticipantAdded(object sender, Microsoft.Lync.Model.Conversation.ParticipantCollectionChangedEventArgs e)
        {
            if (e.Participant.IsSelf == false)
            {
                if (((Conversation)sender).Modalities.ContainsKey(ModalityTypes.InstantMessage))
                {
                    //((Microsoft.Lync.Model.Conversation.InstantMessageModality)e.Participant.Modalities[ModalityTypes.InstantMessage]).InstantMessageReceived += new EventHandler<MessageSentEventArgs>(MainWindow_InstantMessageReceived);
                    //((Microsoft.Lync.Model.Conversation.InstantMessageModality)e.Participant.Modalities[ModalityTypes.InstantMessage]).IsTypingChanged += new EventHandler<IsTypingChangedEventArgs>(MainWindow_IsTypingChanged);
                }
            }

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
    }
}
