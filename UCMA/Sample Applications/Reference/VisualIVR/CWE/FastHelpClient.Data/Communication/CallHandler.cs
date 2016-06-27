/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data.Communication
{
    using System;
    using Microsoft.Lync.Model;
    using Microsoft.Lync.Model.Conversation;
    using Microsoft.Lync.Model.Conversation.AudioVideo;
    using FastHelpClient.Data.Events;

    /// <summary>
    /// CallHandler for CWE
    /// </summary>
    public class CallHandler
    {
        /// <summary>
        /// CallHandler object
        /// </summary>
        private static CallHandler callHandler = null;

        /// <summary>
        /// Lynclient conains reference of logged in user
        /// </summary>
        private LyncClient lyncClient;

        /// <summary>
        /// Conversation of user with remote user
        /// </summary>
        private Conversation conversation;

        /// <summary>
        /// AudioVideo Modality of loggedin user
        /// </summary>
        private AVModality audioVideoModality;

        /// <summary>
        /// Audio Channel of loggedin user
        /// </summary>
        private AudioChannel audioChannel;

        /// <summary>
        /// IM modlity of loggedin user
        /// </summary>
        private InstantMessageModality instantMessageModality;

        /// <summary>
        /// IM modality of remote user
        /// </summary>
        private InstantMessageModality remoteModality;

        /// <summary>
        /// Prevents a default instance of the <see cref="CallHandler"/> class from being created.
        /// </summary>
        private CallHandler()
        {
            this.lyncClient = LyncClient.GetClient();

            // FOR CWE WINDOW,use current hosting conversation 
            this.conversation = (Conversation)Microsoft.Lync.Model.LyncClient.GetHostingConversation();
            if (this.conversation == null)
            {
                this.conversation = this.lyncClient.ConversationManager.Conversations[this.lyncClient.ConversationManager.Conversations.Count - 1];
            }

            this.audioVideoModality = (AVModality)this.conversation.Modalities[ModalityTypes.AudioVideo];
            this.audioChannel = this.audioVideoModality.AudioChannel;

            this.conversation.StateChanged += this.Conversation_StateChangedEvent;
            this.conversation.BeginSendContextData("{553C1CE2-0C73-51B6-81C7-75F2D071FCD2}", @"plain/text", "hi", SendContextDataCallBack, null);
            
            this.audioVideoModality.ModalityStateChanged += new EventHandler<ModalityStateChangedEventArgs>(this.AudioVideoModality_ModalityStateChanged);
            
            this.instantMessageModality = (InstantMessageModality)this.conversation.Modalities[ModalityTypes.InstantMessage];

            this.remoteModality = this.conversation.Participants[1].Modalities[ModalityTypes.InstantMessage] as InstantMessageModality;

            this.remoteModality.InstantMessageReceived += new EventHandler<MessageSentEventArgs>(this.RemoteModality_InstantMessageReceived);
            
            this.instantMessageModality.InstantMessageReceived += new EventHandler<MessageSentEventArgs>(this.ImModality_InstantMessageReceived);
            
            this.CurrentMenuLevel = 0;
        }

        /// <summary>
        /// Occurs when [I m_ received].
        /// </summary>
        public event EventHandler<Events.CallEventArgs> IM_Received;

        /// <summary>
        /// Gets or sets the current menu level.
        /// </summary>
        /// <value>
        /// The current menu level.
        /// </value>
        public int CurrentMenuLevel { get; set; }

        /// <summary>
        /// Gets or sets the menu request.
        /// </summary>
        /// <value>
        /// The menu request.
        /// </value>
        public string MenuRequest { get; set; }

        /// <summary>
        /// Gets the call handler.
        /// </summary>
        /// <returns>callhandler object</returns>
        public static CallHandler GetCallHandler()
        {
            if (callHandler == null)
            {
                callHandler = new CallHandler();
            }

            return callHandler;
        }

        /// <summary>
        /// Sends the IM.
        /// </summary>
        /// <param name="instantMessageText">The im text.</param>
        public void SendIM(string instantMessageText)
        {
            this.instantMessageModality.BeginSendMessage(instantMessageText, modalityCallback: this.InstantMessageCallBack, state: null);
        }
       
        /// <summary>
        /// Checks the call status.
        /// </summary>
        /// <returns>Status of Audio call</returns>
        public bool CheckCallStatus()
        {
            return this.audioVideoModality.State == ModalityState.Disconnected;
        }

        /// <summary>
        /// Ims the call back.
        /// </summary>
        /// <param name="result">The result.</param>
        private void InstantMessageCallBack(IAsyncResult result)
        {
        }

        /// <summary>
        /// Handles the InstantMessageReceived event of the remoteModality control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Lync.Model.Conversation.MessageSentEventArgs"/> instance containing the event data.</param>
        private void RemoteModality_InstantMessageReceived(object sender, MessageSentEventArgs e)
        {
            if (!(e.Text.Contains("Sorry") || e.Text.Contains("Connecting") || e.Text.Contains("Calling")))
            {
                this.IM_Received(this, new CallEventArgs(this.MenuRequest));
            }
        }

        /// <summary>
        /// Handles the ModalityStateChanged event of the avModality control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Lync.Model.Conversation.ModalityStateChangedEventArgs"/> instance containing the event data.</param>
        private void AudioVideoModality_ModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {
        }

        /// <summary>
        /// Handles the StateChangedEvent event of the Conversation control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="data">The <see cref="Microsoft.Lync.Model.Conversation.ConversationStateChangedEventArgs"/> instance containing the event data.
        /// </param>
        /// 
        private void Conversation_StateChangedEvent(object source, ConversationStateChangedEventArgs data)
        {
        }

        /// <summary>
        /// Handles the InstantMessageReceived event of the imModality control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Lync.Model.Conversation.MessageSentEventArgs"/> instance containing the event data.</param>
        private void ImModality_InstantMessageReceived(object sender, MessageSentEventArgs e)
        {
            this.MenuRequest = e.Text.Trim();
        }

        private void SendContextDataCallBack(IAsyncResult asyncResult)
        {
            if (asyncResult.IsCompleted)
            {
                this.conversation.EndSendContextData(asyncResult);
            }
            else
            {
            }
        }
    }
 }
