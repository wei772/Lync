/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using FastHelp.Logging;
using FastHelpCore;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Signaling;

namespace FastHelpServer
{

    /// <summary>
    /// Menu level changed event args.
    /// </summary>
    public class MenuLevelChangedEventArgs : EventArgs 
    {

        private MenuLevel previous;
        private MenuLevel current;
        private string helpdeskNumber;

        public MenuLevelChangedEventArgs(MenuLevel previous, MenuLevel current) 
        {
            this.previous = previous;
            this.current = current;
        }

        public MenuLevel PreviousLevel { get { return this.previous; } }
        public MenuLevel Level { get { return this.current; } }
        public string HelpdeskNumber { get { return this.helpdeskNumber; } set { this.helpdeskNumber = value; } }
    }

    /// <summary>
    /// Represents enumeration of different menu levels.
    /// </summary>
    public enum MenuLevel
    {
        None = 0,
        TopLevel = 1,
        SubLevel = 2,
        HelpDeskRequested = 3,
    }

    /// <summary>
    /// Represents the IVR menu.
    /// </summary>
    public class Menu
    {
        #region private consts

        /// <summary>
        /// Back space string in the menu.
        /// </summary>
        private const string Backspace = "#";
        #endregion

        #region private variables

        /// <summary>
        /// Current menu level
        /// </summary>
        private MenuLevel level;

        /// <summary>
        /// Xml parser.
        /// </summary>
        private XmlParser xmlParser;

        /// <summary>
        /// More information message.
        /// </summary>
        private string moreInformationMessage;

        /// <summary>
        /// More information link.
        /// </summary>
        private string moreInformationLink;

        /// <summary>
        /// Conversation window extension message.
        /// </summary>
        private string cweMessage;

        /// <summary>
        /// Registry file path.
        /// </summary>
        private string registryFilePath;

        /// <summary>
        /// Top level menu name.
        /// </summary>
        private string topLevelMenuName;

        /// <summary>
        /// Sync root object.
        /// </summary>
        private object syncRoot = new object();

        /// <summary>
        /// Const string.
        /// </summary>
        private const string UnableToUnderstand = "Sorry! We could not understand your request. Please try again";
        #endregion

        #region events

        /// <summary>
        /// Menu level changed event.
        /// </summary>
        public event EventHandler<MenuLevelChangedEventArgs> MenuLevelChanged;
        #endregion

        #region constructors

        /// <summary>
        /// Creates a new menu.
        /// </summary>
        public Menu(string moreInformationMessage, 
            string moreInformationLink, 
            string cweMessage,
            string registryFilePath,
            XmlParser xmlParser)
        {
            

            this.moreInformationMessage = moreInformationMessage;
            this.moreInformationLink = moreInformationLink;
            this.cweMessage = cweMessage;
            this.registryFilePath = registryFilePath;
            this.xmlParser = xmlParser;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the current menu level.
        /// </summary>
        public MenuLevel Level 
        {
            get {return level;}
        }
        #endregion

        #region public methods

        /// <summary>
        /// Gets message at current level.
        /// </summary>
        /// <returns></returns>
        public MimePartContentDescription GetMessage()
        {
            return Menu.PackageText(this.GetMessageAtCurrentLevel());
        }

        /// <summary>
        /// Handles user input.
        /// </summary>
        /// <param name="userInput">User input.</param>
        public MimePartContentDescription HandleUserInput(string userInput)
        {
            string response = " ";
            MimePartContentDescription mimeResponse = null;
            if (!String.IsNullOrEmpty(userInput))
            {
                lock (syncRoot)
                {
                    if (String.Equals(userInput, Menu.Backspace, StringComparison.OrdinalIgnoreCase))
                    {
                        response = this.MoveBackward(userInput);
                    }
                    else
                    {
                        response = this.MoveForward(userInput);
                    }
                }
            }

            if (!string.IsNullOrEmpty(response))
            {
                mimeResponse = Menu.PackageText(response);
            }
            return mimeResponse; 
        }



    
        #endregion

        #region private methods

        /// <summary>
        /// Moves the menu forward based on userInput
        /// </summary>
        /// <param name="userInput">User input.</param>
        /// <returns>Response to the user input.</returns>
        private string MoveForward(string userInput)
        {
            string response = Menu.UnableToUnderstand;
            lock (syncRoot)
            {
                switch (this.level)
                {
                    case MenuLevel.None:
                        this.level = MenuLevel.TopLevel;
                        this.RaiseEvent(MenuLevel.None, MenuLevel.TopLevel);
                        response = this.GetMessageAtCurrentLevel();
                        break;
                    case MenuLevel.TopLevel:
                        if (this.xmlParser.ValidateInput(userInput, (int)this.Level, null /* menu name*/))
                        {
                            this.topLevelMenuName = userInput;
                            this.level = MenuLevel.SubLevel;
                            this.RaiseEvent(MenuLevel.TopLevel, MenuLevel.SubLevel);
                            response = this.GetMessageAtCurrentLevel();
                        }
                        break;
                    case MenuLevel.SubLevel:
                        if (this.xmlParser.ValidateInput(userInput, (int)this.Level, this.topLevelMenuName))
                        {
                            this.level = MenuLevel.HelpDeskRequested;
                            var helpdeskNumber = this.xmlParser.HelpdeskNumber(this.topLevelMenuName, userInput);
                            this.RaiseEvent(MenuLevel.SubLevel, MenuLevel.HelpDeskRequested, helpdeskNumber);
                            response = "Calling help desk";
                        }
                        break;
                    case MenuLevel.HelpDeskRequested:

                        if (this.level == MenuLevel.HelpDeskRequested)
                            this.level = MenuLevel.SubLevel;

                        if (this.xmlParser.ValidateInput(userInput, (int)this.Level, this.topLevelMenuName))
                        {   
                            this.level = MenuLevel.HelpDeskRequested;
                            var helpdeskNumber = this.xmlParser.HelpdeskNumber(this.topLevelMenuName, userInput);
                            this.RaiseEvent(MenuLevel.SubLevel, MenuLevel.HelpDeskRequested, helpdeskNumber);
                            response = "Calling help desk";
                        }
                        break;
                    default:
                        break;
                }
            }

            return response;
        }

        /// <summary>
        /// Moves the menu backward based on userInput
        /// </summary>
        /// <param name="userInput">User input.</param>
        /// <returns>Response to the user input.</returns>
        private string MoveBackward(string userInput)
        {
            string response = " ";
            lock (syncRoot)
            {
                switch (this.level)
                {
                    case MenuLevel.None:
                        // Nothing to do since we are at the first level and there 
                        // no more back levels.
                        break;
                    case MenuLevel.TopLevel:
                        // Stay in top level.
                        response = this.GetMessageAtCurrentLevel();
                        break;
                    case MenuLevel.SubLevel:
                        this.level = MenuLevel.TopLevel;
                        this.RaiseEvent(MenuLevel.SubLevel, MenuLevel.TopLevel);
                        response = this.GetMessageAtCurrentLevel();
                        break;
                    case MenuLevel.HelpDeskRequested:
                       
                        this.level = MenuLevel.TopLevel;
                        this.RaiseEvent(MenuLevel.SubLevel, MenuLevel.TopLevel);
                            response = this.GetMessageAtCurrentLevel();
                        break;
                    default:
                        break;
                }
            }

            return response;
        }

        /// <summary>
        /// Raise menu level changed event.
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        private void RaiseEvent(MenuLevel previous, MenuLevel current, string helpdeskNumber = null)
        {
            var eventHandler = this.MenuLevelChanged;
            if (eventHandler != null)
            {
                MenuLevelChangedEventArgs eventArgs = new MenuLevelChangedEventArgs(previous, current);
                eventArgs.HelpdeskNumber = helpdeskNumber;
                eventHandler(this, eventArgs);
            }
        }

        /// <summary>
        /// Gets message at current menu level.
        /// </summary>
        /// <returns>Message at current menu level.</returns>
        private string GetMessageAtCurrentLevel()
        {
            string message = " ";
            lock (syncRoot)
            {
                switch (this.level)
                {
                    case MenuLevel.TopLevel:
                        message = this.GetTopLevelMessage();
                        break;
                    case MenuLevel.SubLevel:
                        message = this.GetSubLevelMessage(this.topLevelMenuName);
                        break;
                    case MenuLevel.None:
                    default:
                        break;
                }
            }

            return message;
        }

        /// <summary>
        /// Gets the sub level message.
        /// </summary>
        /// <returns>Sub level message.</returns>
        private string GetSubLevelMessage(string topLevelMenuName)
        {
            StringBuilder response = new StringBuilder();

            if (!String.IsNullOrEmpty(topLevelMenuName))
            {
                var options = this.xmlParser.SubOptions(topLevelMenuName);
                string selectedOption = string.Format("The options for {0} are : \n <br>", topLevelMenuName);
                response = new StringBuilder(selectedOption);
                response.Append(Menu.CreateOptionAsString(options));
                response.Append("Press # to go back\n <br/>");
            }

            return response.ToString();
        }

        /// <summary>
        /// Gets the welcome message
        /// </summary>
        /// <returns>Welcome message.</returns>
        private string GetTopLevelMessage()
        {

            // Flow is active. Send welcome message.
            // Send the message on the InstantMessagingFlow.
            StringBuilder response = new StringBuilder();
            IList<FastHelpMenuOption> topLevelOptions = this.xmlParser.TopLevelMenuOptions();
            if (topLevelOptions != null)
            {
                response.AppendLine("Welcome to HelpDesk.\n <br/>");
                response.Append(Menu.CreateOptionAsString(topLevelOptions));
                response.AppendLine("<br/>");
                if (!String.IsNullOrEmpty(this.moreInformationLink) && !String.IsNullOrEmpty(this.moreInformationMessage))
                {
                    response.AppendFormat(this.moreInformationMessage, this.moreInformationLink);
                    response.AppendLine("<br/>");
                }
                if (!String.IsNullOrEmpty(this.cweMessage) && !String.IsNullOrEmpty(this.registryFilePath))
                {
                    response.AppendFormat(this.cweMessage, this.registryFilePath);
                }
            }
            else
            {
                response.AppendLine("\n <br/> Sorry something went wrong. We are looking into this.");
            }

            return response.ToString();
        }
        #endregion


        #region private static methods

        /// <summary>
        /// Pmethod wraps two versions of the message into a single 'package' 
        /// that will degrade gracefully, providing the plain text version to 
        /// clients that cannot handle the HTML, or will provide the HTML if the client supports it.
        /// </summary>
        /// <param name="textToConvert">The text to convert.</param>
        /// <returns></returns>
        private static MimePartContentDescription PackageText(string textToConvert)
        {
            MimePartContentDescription plainText = null;
            MimePartContentDescription htmlText = null;
            MimePartContentDescription package = null;

            plainText = new MimePartContentDescription(
                new ContentType("text/plain"),
                Encoding.UTF8.GetBytes(textToConvert));

            htmlText = new MimePartContentDescription(
                new ContentType("text/html"),
                Encoding.UTF8.GetBytes(textToConvert));

            package = new MimePartContentDescription(
                new ContentType("multipart/alternative"), null
            );

            package.Add(htmlText);
            package.Add(plainText);
            return package;
        }

        /// <summary>
        /// Create option as a string.
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Stringified options/</returns>
        private static string CreateOptionAsString(IList<FastHelpMenuOption> options)
        {
            StringBuilder response = new StringBuilder();
            foreach (var opt in options)
            {
                response.AppendFormat("{0}.{1} \n <br/> ", opt.Id, opt.WrittenText);
            }

            return response.ToString();
        }
        #endregion



    }

    /// <summary>
    /// Represents instant messaging IVR class.
    /// </summary>
    public class InstantMessagingIVR
    {
        public Boolean isUserEndpointFirstMessage;

        #region private variables


        /// <summary>
        /// Instant messaging call with customer.
        /// </summary>
        private InstantMessagingCall imCall;

        /// <summary>
        /// Customer session.
        /// </summary>
        private CustomerSession customerSession;

        /// <summary>
        /// Logger.
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Conversation context channel.
        /// </summary>
        private ConversationContextChannel channel;

        /// <summary>
        /// Menu.
        /// </summary>
        private Menu menu;
        #endregion

        #region constructors

        /// <summary>
        /// To create a new IM IVR.
        /// </summary>
        /// <param name="customerSession">Customer session.</param>
        /// <param name="imCall">Im call.</param>
        /// <param name="menu">Menu to use.</param>
        /// <param name="conversationContextChannel">Conversation context channel.</param>
        /// <param name="logger">Logger.</param>
        public InstantMessagingIVR(CustomerSession customerSession, 
            InstantMessagingCall imCall,
            ConversationContextChannel conversationContextChannel,
            Menu menu,
            ILogger logger)
        {
            this.customerSession = customerSession;
            this.logger = logger;
            this.imCall = imCall;
            this.RegisterIMcallEventHandlers(imCall);
            this.menu = menu;
            if (conversationContextChannel != null)
            {
                this.channel = conversationContextChannel;
                this.RegisterContextChannelHandlers(this.channel);
            }
        }
        #endregion

        #region public methods


        /// <summary>
        /// Send status message to customer on the IM channel.
        /// </summary>
        public void SendStatusMessageToCustomer(ContentType contentType, byte[] body)
        {
            try
            {
                InstantMessagingFlow imFlow = this.imCall.Flow;
                imFlow.BeginSendInstantMessage(contentType,
                    body,
                    (asyncResult) =>
                    {
                        try
                        {
                            imFlow.EndSendInstantMessage(asyncResult);
                        }
                        catch (RealTimeException rte)
                        {
                            Console.WriteLine("Exception while sending message {0}", rte);
                            this.logger.Log("Exception while sending message {0}", rte);
                        }
                    },
                    null);
            }
            catch (InvalidOperationException ioe)
            {
                Console.WriteLine("Exception while sending message {0}", ioe);
                this.logger.Log("Exception while sending message {0}", ioe);
            }
        }


        #endregion


        #region private methods

        /// <summary>
        /// Registers conversation context channel handlers.
        /// </summary>
        /// <param name="channel">Channel.</param>
        private void RegisterContextChannelHandlers(ConversationContextChannel channel)
        {
            channel.DataReceived += this.ChannelDataReceived;
        }

        /// <summary>
        /// Unregisters conversation context channel handlers.
        /// </summary>
        /// <param name="channel">Channel.</param>
        private void UnregisterContextChannelHandlers(ConversationContextChannel channel)
        {
            channel.DataReceived -= this.ChannelDataReceived;
        }

        /// <summary>
        /// Registers event handlers for im call.
        /// </summary>
        private void RegisterIMcallEventHandlers(InstantMessagingCall imCall)
        {
            imCall.InstantMessagingFlowConfigurationRequested += this.InstantMessagingFlowConfigurationRequested;
            imCall.StateChanged += this.ImCallStateChanged;
        }


        /// <summary>
        /// Unregisters event handlers for im call.
        /// </summary>
        private void UnregisterIMcallEventHandlers(InstantMessagingCall imCall)
        {
            imCall.InstantMessagingFlowConfigurationRequested -= this.InstantMessagingFlowConfigurationRequested;
            imCall.StateChanged -= this.ImCallStateChanged;
        }

        /// <summary>
        /// Registers event handlers for im flow.
        /// </summary>
        private void RegisterIMflowEventHandlers(InstantMessagingFlow imFlow)
        {
            imFlow.StateChanged += this.ImFlowStateChanged;
            imFlow.MessageReceived += this.ImFlowMessageReceived;
        }

        /// <summary>
        /// Unregisters event handlers for im flow.
        /// </summary>
        private void UnregisterIMflowEventHandlers(InstantMessagingFlow imFlow)
        {
            imFlow.StateChanged -= this.ImFlowStateChanged;
            imFlow.MessageReceived -= this.ImFlowMessageReceived;
        }

      
        #endregion

        #region event handlers

        /// <summary>
        /// IM Flow configuration requested event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstantMessagingFlowConfigurationRequested(object sender, InstantMessagingFlowConfigurationRequestedEventArgs e)
        {
            this.RegisterIMflowEventHandlers(e.Flow);
        }


        /// <summary>
        /// Im call state changed event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImCallStateChanged(object sender, CallStateChangedEventArgs e)
        {
            InstantMessagingCall imCall = sender as InstantMessagingCall;
            if (e.State == CallState.Terminated)
            {
                this.UnregisterIMcallEventHandlers(imCall);
            }
        }


        /// <summary>
        /// Im flow state changed event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImFlowStateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            if (e.State == MediaFlowState.Active)
            {
                InstantMessagingFlow imFlow = sender as InstantMessagingFlow;

                MimePartContentDescription package = null;
                // Get the top level menu.
                if(this.menu.Level == MenuLevel.None) 
                {
                    package = this.menu.HandleUserInput(" ");
                }
                else 
                {
                    package = this.menu.GetMessage();
                }
                if (package != null)
                {
                    this.SendStatusMessageToCustomer(package.ContentType, package.GetBody());
                }
            }
        }

        /// <summary>
        /// Im flow message received event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImFlowMessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["UseUserEndPoint"]))
            {
                if (this.isUserEndpointFirstMessage)
                {
                    this.isUserEndpointFirstMessage = false;
                    return;
                }
                    
            }
            
            InstantMessagingFlow imFlow = sender as InstantMessagingFlow;
            string userResponse = e.TextBody.Trim();

            Console.WriteLine("Received _ :" + userResponse + " from " + e.Sender.Uri);
            MimePartContentDescription package = this.menu.HandleUserInput(userResponse);
            if (package != null)
            {
                try
                {
                    imFlow.BeginSendInstantMessage(package.ContentType,
                        package.GetBody(),
                        (asyncResult) =>
                        {
                            try
                            {
                                imFlow.EndSendInstantMessage(asyncResult);
                            }
                            catch (RealTimeException rte)
                            {
                                Console.WriteLine("Exception while sending message {0}", rte);
                                this.logger.Log("Exception while sending message {0}", rte);
                            }
                        },
                        null);
                }
                catch (InvalidOperationException ioe)
                {
                    Console.WriteLine("Exception while sending message {0}", ioe);
                    this.logger.Log("Exception while sending message {0}", ioe);
                }
            }
        }



        /// <summary>
        /// Data received event handler for the channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChannelDataReceived(object sender, ConversationContextChannelDataReceivedEventArgs e)
        {
            ConversationContextChannel conversationContextChannel = sender as ConversationContextChannel;
            Byte[] body_byte = new Byte[100];
            string response = string.Empty;
            body_byte = e.ContentDescription.GetBody();
            string contextData = System.Text.ASCIIEncoding.ASCII.GetString(body_byte);
        }




        #endregion
    }
}
