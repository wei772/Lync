/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FastHelp.Logging;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

namespace FastHelpServer
{

    /// <summary>
    /// Represents a customer.
    /// </summary>
    public class Customer
    {
        #region private variables

        /// <summary>
        /// Uri of the customer.
        /// </summary>
        private string uri;
        #endregion

        /// <summary>
        /// To create a new customer.
        /// </summary>
        public Customer(string uri)
        {
            if (String.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("uri is null or empty");
            }
            this.uri = uri;
        }

        /// <summary>
        /// Gets the uri of this customer.
        /// </summary>
        public string Uri
        {
            get { return this.uri; }
        }

        /// <summary>
        /// Overridden equals method.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return this.uri.Equals(obj);
        }

        /// <summary>
        /// Overridden hashcode method.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.uri.GetHashCode();
        }

        /// <summary>
        /// Overridden tostring method.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Customer = ");
            sb.Append(this.uri);
            return sb.ToString();
        }
    }



    /// <summary>
    /// Represents a customer session.
    /// </summary>
    public class CustomerSession
    {
        #region private variables

        /// <summary>
        /// Instant messaging IVR.
        /// </summary>
        private InstantMessagingIVR imIvr;

        /// <summary>
        /// Audio IVR.
        /// </summary>
        private AudioIVR audioIvr;

        /// <summary>
        /// Conversation with customer.
        /// </summary>
        private Conversation conversation;

        /// <summary>
        /// Conversation context channel.
        /// </summary>
        private ConversationContextChannel conversationContextChannel;

        /// <summary>
        /// Customer.
        /// </summary>
        private Customer customer;

        /// <summary>
        /// Logger.
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Menu object.
        /// </summary>
        private Menu menu;

        /// <summary>
        /// Application.
        /// </summary>
        private FastHelpServerApp application;

        /// <summary>
        /// Call anchor
        /// </summary>
        private CustomerCallAnchor callAnchor;

        /// <summary>
        /// Lock object.
        /// </summary>
        private object syncRoot = new object();
        #endregion

        #region constructors

        /// <summary>
        /// To create a new customer session.
        /// </summary>
        public CustomerSession(Customer customer, Menu menu, FastHelpServerApp application, ILogger logger)
        {
            if (customer == null)
            {
                throw new ArgumentNullException("customer");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (menu == null)
            {
                throw new ArgumentNullException("menu");
            }

            this.customer = customer;
            this.menu = menu;
            this.menu.MenuLevelChanged += this.MenuLevelChanged;
            this.application = application;
            this.logger = logger;
            Console.WriteLine("New customer session created for {0}", this.customer);
            this.logger.Log("New customer session created for {0}", this.customer);

        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the customer uri.
        /// </summary>
        public string CustomerUri
        {
            get { return this.customer.Uri; }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Handle incoming IM call from the customer.
        /// </summary>
        /// <param name="imCall">IM call.</param>
        public void HandleIncomingInstantMessagingCall(InstantMessagingCall imCall)
        {
            lock (syncRoot)
            {
                if (this.conversation == null)
                {
                    this.conversation = imCall.Conversation;
                    this.RegisterConversationEventHandlers(this.conversation);
                }

                if (this.conversationContextChannel == null && !String.IsNullOrEmpty(this.application.CweGuid))
                {
                    this.conversationContextChannel = new ConversationContextChannel(this.conversation, imCall.RemoteEndpoint);
                    this.RegisterContextChannelHandlers(this.conversationContextChannel);
                }


                this.imIvr = new InstantMessagingIVR(this, imCall, this.conversationContextChannel, this.menu, this.logger);

                this.imIvr.isUserEndpointFirstMessage = true;

                try
                {
                    imCall.BeginAccept((asyncResult) =>
                    {
                        try
                        {
                            imCall.EndAccept(asyncResult);
                            if (this.conversationContextChannel != null && this.conversationContextChannel.State == ConversationContextChannelState.Idle)
                            {
                                ConversationContextChannelEstablishOptions channelOptions = new ConversationContextChannelEstablishOptions();
                                channelOptions.ApplicationName = this.application.Name;
                                channelOptions.ContextualData = "Context channel is open.";
                                channelOptions.Toast = "Please check the accompaining CWE window for Graphical Experience";
                                Guid appGuid = new Guid(this.application.CweGuid);

                                try
                                {
                                    this.conversationContextChannel.BeginEstablish(appGuid,
                                        channelOptions,
                                        (contextChannelAsyncResult) =>
                                        {
                                            try
                                            {
                                                this.conversationContextChannel.EndEstablish(contextChannelAsyncResult);
                                            }
                                            catch (RealTimeException rte)
                                            {
                                                Console.WriteLine("Error establishing conversation context channel {0}", rte);
                                                this.logger.Log("Error establishing conversation context channel {0}", rte);
                                            }
                                        },
                                        null);
                                }
                                catch (InvalidOperationException ioe)
                                {
                                    Console.WriteLine("Error establishing conversation context channel {0}", ioe);
                                    this.logger.Log("Error establishing conversation context channel {0}", ioe);
                                }
                            }
                        }
                        catch (RealTimeException rte)
                        {
                            Console.WriteLine("Error accepting incoming IM call {0}", rte);
                            this.logger.Log("Error accepting incoming IM call {0}", rte);
                        }
                    },
                    null);
                }
                catch (InvalidOperationException ioe)
                {
                    Console.WriteLine("Error accepting incoming IM call {0}", ioe);
                    this.logger.Log("Error accepting incoming IM call {0}", ioe);
                }
            }
        }


        /// <summary>
        /// Handle incoming audio call from the customer.
        /// </summary>
        /// <param name="audioCall">Audio call.</param>
        public void HandleIncomingAudioCall(AudioVideoCall audioCall)
        {
            lock (syncRoot)
            {
                if (this.conversation == null)
                {
                    this.conversation = audioCall.Conversation;
                    this.RegisterConversationEventHandlers(this.conversation);

                }

                this.audioIvr = new AudioIVR(audioCall, this.application.XmlParser, this.logger);

                try
                {
                    audioCall.BeginAccept((asyncResult) =>
                    {
                        try
                        {
                            audioCall.EndAccept(asyncResult);
                        }
                        catch (RealTimeException rte)
                        {
                            Console.WriteLine("Error accepting incoming AV call {0}", rte);
                            this.logger.Log("Error accepting incoming AV call {0}", rte);
                        }
                    },
                    null);
                }
                catch (InvalidOperationException ioe)
                {
                    Console.WriteLine("Error accepting incoming AV call {0}", ioe);
                    this.logger.Log("Error accepting incoming AV call {0}", ioe);
                }
            }
        }

        /// <summary>
        /// Handle incoming av call from the avmcu.
        /// </summary>
        /// <param name="imCall">IM call.</param>
        public void HandleIncomingDialOutCall(AudioVideoCall avCall)
        {
            lock (syncRoot)
            {
                if (this.callAnchor != null)
                {
                    this.callAnchor.ProcessIncomingDialOutCall(avCall);
                }
                else
                {
                    try
                    {
                        Console.WriteLine("No pending estalblishment process. Declining call");
                        this.logger.Log("No pending estalblishment process. Declining call");
                        avCall.Decline();
                    }
                    catch (RealTimeException rte)
                    {
                        Console.WriteLine("Decline failed with {0}", rte);
                        this.logger.Log("Decline failed with {0}", rte);
                    }
                    catch (InvalidOperationException ioe)
                    {
                        Console.WriteLine("Decline failed with {0}", ioe);
                        this.logger.Log("Decline failed with {0}", ioe);
                    }
                }
            }
        }
        #endregion

        #region private methods

        /// <summary>
        /// Terminates the customer session.
        /// </summary>
        /// <returns></returns>
        private void Terminate()
        {
            Console.WriteLine("Terminating customer session for {0}", this.customer);
            this.logger.Log("Terminating customer session for {0}", this.customer);

            this.application.UnbindCustomerSesssion(this);

            var conv = this.conversation;
            if (conv != null)
            {
                conv.BeginTerminate((asyncResult) =>
                {
                    conv.EndTerminate(asyncResult);
                },
                null);
            }

            var callAnchor = this.callAnchor;
            if (callAnchor != null)
            {
                callAnchor.BeginTerminate((result) =>
                {
                    callAnchor.EndTerminate(result);
                },
                null);
            }
        }


        /// <summary>
        /// State changed event handler for the channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChannelStateChanged(object sender, ConversationContextChannelStateChangedEventArgs e)
        {
            ConversationContextChannel convChannel = sender as ConversationContextChannel;
            if (e.State == ConversationContextChannelState.Terminated)
            {
                this.UnregisterContextChannelHandlers(convChannel);
            }
        }


        /// <summary>
        /// Registers conversation context channel handlers.
        /// </summary>
        /// <param name="channel">Channel.</param>
        private void RegisterContextChannelHandlers(ConversationContextChannel channel)
        {
            channel.StateChanged += this.ChannelStateChanged;
        }

        /// <summary>
        /// Unregisters conversation context channel handlers.
        /// </summary>
        /// <param name="channel">Channel.</param>
        private void UnregisterContextChannelHandlers(ConversationContextChannel channel)
        {
            channel.StateChanged -= this.ChannelStateChanged;
        }

        /// <summary>
        /// Registers conversation event handlers.
        /// </summary>
        private void RegisterConversationEventHandlers(Conversation conversation)
        {
            conversation.StateChanged += this.ConversationStateChanged;
        }

        /// <summary>
        /// Registers conversation event handlers.
        /// </summary>
        private void UnregisterConversationEventHandlers(Conversation conversation)
        {
            conversation.StateChanged -= this.ConversationStateChanged;
        }

        #endregion

        #region event handlers


        /// <summary>
        /// Menu level changed handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuLevelChanged(object sender, MenuLevelChangedEventArgs e)
        {
            if (e.Level == MenuLevel.HelpDeskRequested)
            {
                lock (this.syncRoot)
                {
                    if (this.callAnchor == null)
                    {
                        this.callAnchor = new CustomerCallAnchor(this, this.logger, this.conversation);
                    }

                    try
                    {
                        var helpdeskNumber = e.HelpdeskNumber;
                        if (String.IsNullOrEmpty(helpdeskNumber))
                        {
                            Console.WriteLine("Null or empty help desk number");
                            this.logger.Log("Null or empty help desk number");
                        }
                        else
                        {
                            try
                            {
                                RealTimeAddress address = new RealTimeAddress(helpdeskNumber);

                                this.callAnchor.BeginEstablish(address,
                                    (asyncResult) =>
                                    {
                                        try
                                        {
                                            this.callAnchor.EndEstablish(asyncResult);
                                        }
                                        catch (Exception ex)
                                        {
                                            this.callAnchor.BeginTerminate((terminateAsyncResult) => { this.callAnchor.EndTerminate(terminateAsyncResult); }, null);
                                            Console.WriteLine("Call anchor failed with {0}", ex);
                                            this.logger.Log("Call anchor failed with {0}", ex);
                                        }
                                    },
                                    null);
                            }
                            catch (ArgumentException ae)
                            {
                                Console.WriteLine("Invalid help desk number {0}, Exception ={1}", helpdeskNumber, ae);
                                this.logger.Log("Invalid help desk number {0}, Exception ={1}", helpdeskNumber, ae);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Call anchor failed with {0}", ex);
                        this.logger.Log("Call anchor failed with {0}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Conversation event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConversationStateChanged(object sender, Microsoft.Rtc.Signaling.StateChangedEventArgs<ConversationState> e)
        {
            Conversation conversation = sender as Conversation;

            if (e.State == ConversationState.Terminating)
            {
                this.UnregisterConversationEventHandlers(conversation);
                this.Terminate();
            }
        }

        #endregion
    }
}
