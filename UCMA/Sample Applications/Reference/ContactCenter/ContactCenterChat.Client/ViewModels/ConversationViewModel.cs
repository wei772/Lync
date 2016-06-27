/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows.Browser;
using System.Globalization;
using System.ServiceModel;
using System.Collections.Generic;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.Models;
using System.Text;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.ViewModels
{
    public class ConversationViewModel : ViewModel, IConversationViewModel
    {
        #region private variables
        /// <summary>
        /// Simple Regex for html tag parsing.
        /// </summary>
        private readonly Regex HTMLTag = new Regex(@"(<\/?[^>]+>)");

        /// <summary>
        /// Local participant display name message color.
        /// </summary>
        private const MessageColor LocalParticipantDisplayNameMessageColor = MessageColor.Red;

        /// <summary>
        /// Local participant message color.
        /// </summary>
        private const MessageColor LocalParticipantMessageColor = MessageColor.Gray;

        /// <summary>
        /// Remote participant display name message color.
        /// </summary>
        private const MessageColor RemoteParticipantDisplayNameMessageColor = MessageColor.Red;

        /// <summary>
        /// Remote participant message color.
        /// </summary>
        private const MessageColor RemoteParticipantMessageColor = MessageColor.Gray;


        /// <summary>
        /// Html tag start character.
        /// </summary>
        private const string HTMLTagStart = "<";
     
        /// <summary>
        /// Status message.
        /// </summary>
        private String m_status;
     

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly ObservableCollection<MessageViewModel> m_messages = new ObservableCollection<MessageViewModel>();

        /// <summary>
        /// Local participant view model.
        /// </summary>
        private readonly ParticipantViewModel m_localParticipant;

        /// <summary>
        /// Product id.
        /// </summary>
        private readonly string m_productId;

        /// <summary>
        /// Bool to denote if call me command is enabled.
        /// </summary>
        private bool m_isCallMeCommandEnabled;

        /// <summary>
        /// Bool to denote if send message command is enabled.
        /// </summary>
        private bool m_isSendMessageCommandEnabled;

        /// <summary>
        /// Terminate conversation command enabled.
        /// </summary>
        private bool m_isTerminateConversationCommandEnabled;

        /// <summary>
        /// Conversation model.
        /// </summary>
        private ConversationModel m_conversationModel;

        /// <summary>
        /// Captures the last message sender.
        /// </summary>
        private string m_lastMessageSender;

        /// <summary>
        /// Sync root.
        /// </summary>
        private readonly object m_syncRoot = new object();

        /// <summary>
        /// Empty message iew model.
        /// </summary>
        private static readonly MessageViewModel EmptyMessageViewModel = new MessageViewModel(new ParticipantViewModel(string.Empty, string.Empty, MessageColor.White), string.Empty, MessageColor.White, DateTime.Now);
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConversationViewModel(Dispatcher dispatcher, string endpointUri, string userName, string phoneNumber, string queueName, string productId)
        {
            m_localParticipant = new ParticipantViewModel(userName, phoneNumber, ConversationViewModel.LocalParticipantDisplayNameMessageColor);
            m_productId = productId;

            this.Dispatcher = dispatcher;

            this.Initialize(queueName, endpointUri, productId, userName);
        }

        #endregion

        #region protected methods

        /// <summary>
        /// Intialize commands.
        /// </summary>
        protected override void OnInitializeCommands()
        {
            base.OnInitializeCommands();
            this.SendMessageCommand = new Command(this.ExecuteSendMessageCommand, this.CanExecuteSendMessageCommand);
            this.CallMeCommand = new Command(this.ExecuteCallMeCommand, this.CanExecuteCallMeCommand);
            this.TerminateConversationCommand = new Command(this.ExecuteTerminateConversationCommand, this.CanExecuteTerminateConversationCommand);
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the local participant
        /// </summary>
        public ParticipantViewModel LocalParticipant 
        {
            get { return m_localParticipant; }
        }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public String Status
        {
            get
            {
                return m_status;
            }
            set
            {
                if (m_status != value)
                {
                    m_status = value;
                    NotifyPropertyChanged("Status");
                }
            }
        }

      
        /// <summary>
        /// Bool to denote if send message command is enabled.
        /// </summary>
        public bool IsSendMessageCommandEnabled
        {
            get
            {
                return m_isSendMessageCommandEnabled;
            }
            set
            {
                if (m_isSendMessageCommandEnabled != value)
                {
                    m_isSendMessageCommandEnabled = value;
                    NotifyPropertyChanged("IsSendMessageCommandEnabled");
                }
            }
        }

        /// <summary>
        /// Is call me command enabled.
        /// </summary>
        public bool IsCallMeCommandEnabled
        {
            get
            {
                return m_isCallMeCommandEnabled;
            }
            set
            {
                if (m_isCallMeCommandEnabled != value)
                {
                    m_isCallMeCommandEnabled = value;
                    NotifyPropertyChanged("IsCallMeCommandEnabled");
                }
            }
        }

        /// <summary>
        /// Is terminate conversation command enabled.
        /// </summary>
        public bool IsTerminateConversationCommandEnabled
        {
            get
            {
                return m_isTerminateConversationCommandEnabled;
            }
            set
            {
                if (m_isTerminateConversationCommandEnabled != value)
                {
                    m_isTerminateConversationCommandEnabled = value;
                    NotifyPropertyChanged("IsTerminateConversationCommandEnabled");
                }
            }
        }

        /// <summary>
        /// Gets the messages collection.
        /// </summary>
        public ObservableCollection<MessageViewModel> Messages 
        {
            get { return m_messages; }
        }

        /// <summary>
        /// Gets or sets the working message.
        /// </summary>
        public String WorkingMessage
        {
            get
            {
                return _workingMessage;
            }
            set
            {
                if (_workingMessage != value)
                {
                    _workingMessage = value;
                    //NotifyPropertyChanged("WorkingMessage");
                    ((Command)this.SendMessageCommand).NotifyCanExecuteChanged();
                }
            }
        }
        private String _workingMessage;

        #endregion

        #region private properties

        /// <summary>
        /// Gets the last message sender.
        /// </summary>
        private string LastMessageSender
        {
            get { return m_lastMessageSender; }
        }

        /// <summary>
        /// Gets the conversation model.
        /// </summary>
        private ConversationModel ConversationModel
        {
            get { return m_conversationModel; }
        }
        #endregion

        #region Command implementations

        /// <summary>
        /// Can execute send message command.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <returns>True if can execute send message command. False otherwise.</returns>
        public bool CanExecuteSendMessageCommand(Object argument)
        {
            string workingMessage = this.WorkingMessage;
            ConversationModelState convModelState = this.ConversationModel.State;
            return ((!String.IsNullOrEmpty(workingMessage)) && (convModelState == ConversationModelState.Established));
        }

        /// <summary>
        /// Send message execute method.
        /// </summary>
        /// <param name="argument">Argument.</param>
        public void ExecuteSendMessageCommand(Object argument)
        {
            string messageToSend = this.WorkingMessage;
            if(!String.IsNullOrEmpty(messageToSend)) 
            {
                try
                {
                    //Send Im message.
                    ConversationModel conversationModel = this.ConversationModel;
                    conversationModel.BeginSendImMessage(messageToSend, this.ImMessageSendCompleted, conversationModel/*state*/);
                }
                catch (Exception e)
                {
                    this.Status = e.Message;
                }
                this.AddMessageToMessageQueue(new MessageViewModel(new ParticipantViewModel(this.LocalParticipant), messageToSend, ConversationViewModel.LocalParticipantMessageColor, DateTime.Now));
                this.WorkingMessage = String.Empty;
            }
        }

        /// <summary>
        /// Can execute call me command.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <returns>True if we can execute call me command.</returns>
        public bool CanExecuteCallMeCommand(Object argument)
        {
            string phoneNumber = this.LocalParticipant.PhoneNumber;
            ConversationModelState convModelState = this.ConversationModel.State;
            return ((!String.IsNullOrEmpty(phoneNumber)) && (convModelState == ConversationModelState.Established));
        }

        /// <summary>
        /// Execute call me command.
        /// </summary>
        /// <param name="argument">Argument.</param>
        public void ExecuteCallMeCommand(Object argument)
        {
            string callbackNumber = this.LocalParticipant.PhoneNumber;
            if (!String.IsNullOrEmpty(callbackNumber))
            {
                try
                {
                    //Establish av call.
                    ConversationModel conversationModel = this.ConversationModel;
                    conversationModel.BeginAddClickToCall(callbackNumber, this.AvCallEstablishCompleted, conversationModel /*state*/);
                    //Disable till the command is done. We can reenable it once the command is complete.
                    this.IsCallMeCommandEnabled = false;
                }
                catch (Exception e)
                {
                    this.Status = e.Message;
                }
            }
        }

        /// <summary>
        /// Can execute terminate conversation command.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <returns>True if we can execute terminate conversation command.</returns>
        public bool CanExecuteTerminateConversationCommand(Object argument)
        {
            ConversationModelState convModelState = this.ConversationModel.State;
            return (convModelState != ConversationModelState.Terminating && convModelState != ConversationModelState.Terminated);
        }

        /// <summary>
        /// Execute terminate conversation command.
        /// </summary>
        /// <param name="argument">Argument.</param>
        public void ExecuteTerminateConversationCommand(Object argument)
        {
            try
            {
                //Establish av call.
                ConversationModel conversationModel = this.ConversationModel;
                conversationModel.SessionTerminated();
            }
            catch (Exception e)
            {
                this.Status = e.Message;
            }
        }

        #endregion

        #region Commands
        /// <summary>
        /// Gets the send message command.
        /// </summary>
        public ICommand SendMessageCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the call me command.
        /// </summary>
        public ICommand CallMeCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the terminate conversation command.
        /// </summary>
        public ICommand TerminateConversationCommand
        {
            get;
            private set;
        }
        #endregion

        #region private methods

        /// <summary>
        /// Initialize.
        /// </summary>
        private void Initialize(string queueName, string endpointUri, string productId, string userName)
        {
            ContactCenterService contactCenterService = new ContactCenterService(endpointUri);
            ConversationModel conversationModel = new ConversationModel(contactCenterService, productId/*subject of the conversation*/, userName);
            this.RegisterConversationModelEventHandlers(conversationModel);
            m_conversationModel = conversationModel;

            try
            {
                //Start conversation.
                conversationModel.BeginEstablishConversationAndImCall(queueName, productId, this.ImCallEstablished, conversationModel/*state*/);
            }
            catch (Exception e)
            {
                this.Status = e.Message;
            }
        }

        /// <summary>
        /// av call establish callback.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void AvCallEstablishCompleted(IAsyncResult asyncResult)
        {
            ConversationModel convModel = asyncResult.AsyncState as ConversationModel;
            try
            {
                convModel.EndAddClickToCall(asyncResult);
                //Disable click to call buttong.
                this.IsCallMeCommandEnabled = false;
            }
            catch (Exception e)
            {
                this.Status = e.Message;
                this.IsCallMeCommandEnabled = true; //In case of failure enable the button so that user can retry.
            }
        }

        /// <summary>
        /// Im message send callback..
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void ImMessageSendCompleted(IAsyncResult asyncResult)
        {
            ConversationModel convModel = asyncResult.AsyncState as ConversationModel;
            try
            {
                convModel.EndSendImMessage(asyncResult);
            }
            catch (Exception e)
            {
                this.Status = e.Message;
            }
        }

        /// <summary>
        /// Im call established.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void ImCallEstablished(IAsyncResult asyncResult)
        {
            ConversationModel convModel = asyncResult.AsyncState as ConversationModel;
            try
            {
                convModel.EndEstablishConversationAndImCall(asyncResult);
                //On successful im call establishment light up send message button.
                this.IsSendMessageCommandEnabled = true;
                //On successful im call establishment light up click to call button.
                if (!String.IsNullOrEmpty(this.LocalParticipant.PhoneNumber))
                {
                    this.IsCallMeCommandEnabled = true;
                }
            }
            catch (Exception e)
            {
                this.Status = e.Message;
            }
        }

        /// <summary>
        /// Register conversation model event handlers.
        /// </summary>
        /// <param name="conversationModel">Conversation model.</param>
        private void RegisterConversationModelEventHandlers(ConversationModel conversationModel)
        {
            conversationModel.StateChanged += this.ConversationModelStateChanged;
            conversationModel.ImMessageReceived += this.ImMessageReceived;
            conversationModel.ImTypingNotificationReceived += this.ImTypingNotificationReceived;
        }

        /// <summary>
        /// Unregister conversation model event handlers.
        /// </summary>
        /// <param name="conversationModel">Conversation model.</param>
        private void UnregisterConversationModelEventHandlers(ConversationModel conversationModel)
        {
            conversationModel.StateChanged -= this.ConversationModelStateChanged;
            conversationModel.ImMessageReceived -= this.ImMessageReceived;
            conversationModel.ImTypingNotificationReceived -= this.ImTypingNotificationReceived;
        }


        /// <summary>
        /// Conversation model state changed event handler.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ConversationModelStateChanged(object sender, ConversationStateChangedEventArgs e)
        {
            this.Status = String.Format("{0} {1}", StatusResource.ConversationStateChange, e.CurrentState);
            if (e.CurrentState == ConversationModelState.Terminated)
            {
                this.IsCallMeCommandEnabled = false;
                this.IsSendMessageCommandEnabled = false;
                ConversationModel conversationModel = sender as ConversationModel;
                this.UnregisterConversationModelEventHandlers(conversationModel);
            }
        }

        /// <summary>
        /// Im message received.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ImMessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            string message = this.ParseHtmlMessage(e.Message);
            MessageViewModel msgViewModel = new MessageViewModel(new ParticipantViewModel(e.MessageSender, null/*phoneNumber*/, ConversationViewModel.RemoteParticipantDisplayNameMessageColor), 
                                                                    message,
                                                                    ConversationViewModel.RemoteParticipantMessageColor, 
                                                                    DateTime.Now);
            this.AddMessageToMessageQueue(msgViewModel);
        }

        /// <summary>
        /// Im typing notification received.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ImTypingNotificationReceived(object sender, InstantMessageTypingNotificationReceivedEventArgs e)
        {
            string typingMessage = ConversationViewModel.GetTypingStatusFromRemoteComposingStatus(e.RemoteComposingStatus);   
            Dispatcher dispatcher = this.Dispatcher;
            if (dispatcher != null)
            {
                Action method = () => this.Status = typingMessage;
                dispatcher.BeginInvoke(method);
            }
            else
            {
                this.Status = typingMessage;
            }
        }

        /// <summary>
        /// Static method to convert remote composing status to string.
        /// </summary>
        /// <param name="remoteComposingStatus">Remote composing status.</param>
        /// <returns>string.</returns>
        private static string GetTypingStatusFromRemoteComposingStatus(RemoteComposingStatus remoteComposingStatus)
        {
            string typingText = string.Empty;
            if (remoteComposingStatus == RemoteComposingStatus.Active)
            {
                typingText = StatusResource.Typing;
            }
            else if (remoteComposingStatus == RemoteComposingStatus.Idle)
            {
                //Do nothing. this will reset the typing value.
            }
            return typingText;
        }

        /// <summary>
        /// Helper method to remove html tags if necessary.
        /// </summary>
        /// <param name="message">Mesasge with html</param>
        /// <returns>Message without html.</returns>
        private string ParseHtmlMessage(string message)
        {
            StringBuilder parsedMessage = new StringBuilder();
            foreach (string str in this.HTMLTag.Split(message))
            {
                if (!str.StartsWith(ConversationViewModel.HTMLTagStart))
                {
                    //Not a html tag. Append to the display message.
                    parsedMessage.Append(str);
                }
            }
            return parsedMessage.ToString();
        }

        /// <summary>
        /// Adds message to a message queue.
        /// </summary>
        /// <param name="msgViewModel">Message view model.</param>
        private void AddMessageToMessageQueue(MessageViewModel msgViewModel)
        {
            ParticipantViewModel newParticipant = null;
            lock (m_syncRoot)
            {
                if (msgViewModel != null)
                {
                    if (msgViewModel.MessageSource != null && !String.IsNullOrEmpty(msgViewModel.MessageSource.DisplayName))
                    {
                        if (!String.Equals(m_lastMessageSender, msgViewModel.MessageSource.DisplayName))
                        {
                            //Different display name. Store the new display name as last known display name.
                            m_lastMessageSender = msgViewModel.MessageSource.DisplayName;
                            newParticipant = new ParticipantViewModel(msgViewModel.MessageSource);
                        }
                    }
                }
            }

            if (newParticipant != null)
            {
                //Add an empty line and then
                this.AddMessage(ConversationViewModel.EmptyMessageViewModel);
                //Add a seperate display name line.
                this.AddMessage(new MessageViewModel(newParticipant, string.Empty, MessageColor.White, DateTime.Now));
            }
            //Add message after stripping out the display name.
            msgViewModel.MessageSource.DisplayName = string.Empty;
            this.AddMessage(msgViewModel);
        }

        /// <summary>
        /// Adds message to the message queue.
        /// </summary>
        /// <param name="msgViewModel"></param>
        private void AddMessage(MessageViewModel msgViewModel)
        {
            if (msgViewModel != null)
            {
                Dispatcher dispatcher = this.Dispatcher;
                if (dispatcher != null)
                {
                    Action method = () => this.Messages.Add(msgViewModel);
                    dispatcher.BeginInvoke(method);
                }
                else
                {
                    this.Messages.Add(msgViewModel);
                }
            }
        }


        #endregion
    }
}
