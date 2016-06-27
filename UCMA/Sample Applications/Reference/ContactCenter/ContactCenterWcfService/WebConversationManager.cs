/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities;
using System.Diagnostics;
using System.Text;
using System.ServiceModel;
using Microsoft.Rtc.Signaling;
using System.Collections.ObjectModel;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.AsyncResults;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService
{
    /// <summary>
    /// Represents helper class to manage web conversations.
    /// </summary>
    internal class WebConversationManager
    {


        #region private const strings.

        /// <summary>
        /// Default user name to be used.
        /// </summary>
        private const string DefaultUserName = "WebUser";
        #endregion


        #region private variables

        /// <summary>
        /// Dictionary of currently active conversations. Key for this dictionary is the conversation id.
        /// </summary>
        private readonly Dictionary<string, WebConversation> m_conversationDictionary = new Dictionary<string, WebConversation>();

        /// <summary>
        /// Application endpoint to use.
        /// </summary>
        private readonly ApplicationEndpoint m_applicationEndpoint;

        /// <summary>
        /// Conversation poller.
        /// </summary>
        private readonly WebConversationPoller m_webConversationPoller;
        #endregion

        #region constructors

        /// <summary>
        /// Constructs a new web conversation manager.
        /// </summary>
        /// <param name="applicationEndpoint">Application endpoint to use.</param>
        internal WebConversationManager(ApplicationEndpoint applicationEndpoint, TimerWheel timerWheel)
        {
            Debug.Assert(null != applicationEndpoint, "Application endpoint cannot be null");
            m_applicationEndpoint = applicationEndpoint;
            m_webConversationPoller = new WebConversationPoller(this, timerWheel);
            m_webConversationPoller.Start();
        }
        #endregion

        #region private properties

        /// <summary>
        /// Gets the application endpoint to use.
        /// </summary>
        private ApplicationEndpoint ApplicationEndpoint 
        {
            get { return m_applicationEndpoint; }
        }


        /// <summary>
        /// Gets the Web conversation poller.
        /// </summary>
        private WebConversationPoller ConversationPoller
        {
            get { return m_webConversationPoller; }
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Clean up method.
        /// </summary>
        internal void Stop()
        {
            WebConversationPoller poller = this.ConversationPoller;
            if (poller != null)
            {
                poller.Stop();
            }
        }

        /// <summary>
        /// Helper method to create new web conversation.
        /// </summary>
        /// <param name="request">Create conversation request.</param>
        /// <param name="conversationCallback">Conversation callback.</param>
        /// <param name="contextChannel">Context channel.</param>
        /// <returns>WebConversation.</returns>
        internal WebConversation CreateNewWebConversation(CreateConversationRequest request, IConversationCallback conversationCallback, IContextChannel contextChannel)
        {
            WebConversation webConversation = null;

            ConversationSettings convSettings = new ConversationSettings();
            convSettings.Subject = request.ConversationSubject;

            //First create a ucma conversation.
            Conversation ucmaConversation = new Conversation(this.ApplicationEndpoint, convSettings);
            ucmaConversation.Impersonate(WebConversationManager.CreateUserUri(this.ApplicationEndpoint.DefaultDomain), null /*phoneUri*/, request.DisplayName);

            //Register for state changes.
            ucmaConversation.StateChanged += this.UcmaConversation_StateChanged;

            //Now create a web conversation.
            webConversation = new WebConversation(ucmaConversation, conversationCallback, request.ConversationContext, contextChannel);

            //Add conversation to local cache.
            lock (m_conversationDictionary)
            {
                m_conversationDictionary.Add(webConversation.Id, webConversation);
            }

            return webConversation;
        }


        /// <summary>
        /// Gets the mathcing web conversation, if any, based on given id.
        /// </summary>
        /// <param name="id">Conversation id.</param>
        /// <returns>Matching web conversation if any.</returns>
        internal WebConversation GetWebConversationFromId(string id)
        {
            WebConversation webConversation = null;
            string conversationId = id;

            if (!String.IsNullOrEmpty(conversationId))
            {
                lock (m_conversationDictionary)
                {
                    if (m_conversationDictionary.ContainsKey(conversationId))
                    {
                        webConversation = m_conversationDictionary[conversationId];
                    }
                }
            }
            return webConversation;
        }

        /// <summary>
        /// Gets all web conversations.
        /// </summary>
        /// <returns>All existing web conversations.</returns>
        internal Collection<WebConversation> GetAllWebConversations()
        {
            Collection<WebConversation> allConversations = new Collection<WebConversation>();
            lock (m_conversationDictionary)
            {
                foreach (WebConversation webConversation in m_conversationDictionary.Values)
                {
                    allConversations.Add(webConversation);
                }
            }
            return allConversations;
        }



        #endregion

        #region conversation event handlers

        /// <summary>
        /// Ucma conversation state changed handler.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void UcmaConversation_StateChanged(object sender, Microsoft.Rtc.Signaling.StateChangedEventArgs<ConversationState> e)
        {
            if (e.State == ConversationState.Terminating)
            {
                Conversation conversation = sender as Conversation;
                WebConversation webConversation = null;
                //If conversation is terminating remove it from local dictionary.
                lock (m_conversationDictionary)
                {
                    conversation.StateChanged -= this.UcmaConversation_StateChanged;
                    if (!m_conversationDictionary.TryGetValue(conversation.Id, out webConversation))
                    {
                        webConversation = null;
                    }
                    m_conversationDictionary.Remove(conversation.Id);

                }

                if (webConversation != null)
                {
                    ConversationTerminationNotification conversationTerminationNotification = new ConversationTerminationNotification();
                    conversationTerminationNotification.Conversation = webConversation;

                    IConversationCallback convCallback = WebConversationManager.GetActiveConversationCallback(webConversation);
                    if (convCallback != null)
                    {
                        try
                        {
                            convCallback.ConversationTerminated(conversationTerminationNotification);
                        }
                        catch (System.TimeoutException)
                        {
                        }
                    }
                }
            }
        }

        #endregion

        #region private static methods


        /// <summary>
        /// Helper method to construct user uri.
        /// </summary>
        /// <param name="defaultDomain">Default domain to use.</param>
        /// <returns>User uri.</returns>
        private static string CreateUserUri(string defaultDomain)
        {
            string userNameToUse = WebConversationManager.DefaultUserName;

            StringBuilder uriBuilder = new StringBuilder(Helper.Sip);
            uriBuilder.Append(userNameToUse);
            uriBuilder.Append("@");
            uriBuilder.Append(defaultDomain);

            return uriBuilder.ToString();
        }


        /// <summary>
        /// Returns active conversation callback handler. Can return null if the callback handler is not active.
        /// </summary>
        /// <param name="webConversation">Web conversation for which we need an active callback handler.</param>
        /// <returns>Active conversation callback.</returns>
        internal static IConversationCallback GetActiveConversationCallback(WebConversation webConversation)
        {
            IConversationCallback callback = null;

            Debug.Assert(null != webConversation, "Web conversation cannot be null");
            ICommunicationObject communicationObject = webConversation.ConversationCallback as ICommunicationObject;
            if (communicationObject != null && (communicationObject.State == CommunicationState.Opened))
            {
                callback = webConversation.ConversationCallback;
            }
            return callback;
        }



        #endregion
    }


    /// <summary>
    /// Represents poller class to keep polling the client callbacks to check if they are alive.
    /// </summary>
    internal class WebConversationPoller
    {
        #region private variables

        private readonly TimerWheel m_timerWheel;

        private static readonly TimeSpan DefaultPollingTimeSpan = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Sync root object.
        /// </summary>
        private readonly object m_syncRoot = new object();

        /// <summary>
        /// Timer item.
        /// </summary>
        private TimerItem m_timerItem;

        /// <summary>
        /// Conv manager.
        /// </summary>
        private readonly WebConversationManager m_conversationManager;

        #endregion

        #region constructor
        /// <summary>
        /// Constructor to create the poller.
        /// </summary>
        internal WebConversationPoller(WebConversationManager webConversationManager, TimerWheel timerWheel)
        {
            m_conversationManager = webConversationManager;
            m_timerWheel = timerWheel;
        }
        #endregion

        #region private properties

      
        #endregion

        #region internal properties

        #endregion

        #region internal methods

        /// <summary>
        /// Starts the poller.
        /// </summary>
        internal void Start()
        {
            lock (m_syncRoot)
            {
                this.TerminateUnnecessaryConversations();
                if (m_timerItem == null)
                {
                    m_timerItem = new TimerItem(m_timerWheel, WebConversationPoller.DefaultPollingTimeSpan);
                    m_timerItem.Expired += this.TimerItem_Expired;
                    m_timerItem.Start();
                }
                else if (!m_timerItem.IsStarted)
                {
                    m_timerItem.Start();
                }
            }
        }

        /// <summary>
        /// Stops the poller.
        /// </summary>
        internal void Stop()
        {
            lock (m_syncRoot)
            {
                if (m_timerItem != null)
                {
                    m_timerItem.Stop();
                    m_timerItem.Expired -= this.TimerItem_Expired;
                    m_timerItem = null;
                }
            }
        }
        #endregion

        #region private methods

        /// <summary>
        /// Timer expired callback.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void TimerItem_Expired(object sender, EventArgs e)
        {
            //Request for new data.
            lock (m_syncRoot)
            {
                this.TerminateUnnecessaryConversations();
                m_timerItem.Reset();
            }
        }

        /// <summary>
        /// Method to start the work of retrieving new data.
        /// </summary>
        private void TerminateUnnecessaryConversations()
        {
            foreach (WebConversation webConversation in m_conversationManager.GetAllWebConversations())
            {
                CommunicationState channelState = webConversation.ContextChannel.State;
                if (channelState != CommunicationState.Created && channelState != CommunicationState.Opened && channelState != CommunicationState.Opening)
                {
                    //Create a new async result.
                    TerminateConversationRequest convRequest = new TerminateConversationRequest();
                    convRequest.Conversation = webConversation;
                    convRequest.RequestId = Guid.NewGuid().ToString();
                    webConversation.BeginTerminate(convRequest, this.WebConversationTerminationCompleted, webConversation/*state*/);
                }
            }
        }

        /// <summary>
        /// Callback.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void WebConversationTerminationCompleted(IAsyncResult asyncResult)
        {
            WebConversation webConveration = asyncResult.AsyncState as WebConversation;
            webConveration.EndTerminate(asyncResult);
        }

        #endregion

    }
}
