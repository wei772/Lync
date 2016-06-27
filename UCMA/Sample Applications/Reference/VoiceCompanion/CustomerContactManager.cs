/*=====================================================================
  File:      CustomerContactManager.cs

  Summary:   Manages a customers contact list.

***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Rtc.Collaboration.ContactsGroups;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Signaling;
using System.Collections.ObjectModel;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    public class ContactInformation
    {
        #region Private fields

        private readonly string m_uri;
        private List<ContactCard> m_contactCards;
        private string m_displayName;
        
        #endregion

        #region Constructors

        internal ContactInformation(string uri)
        {
            Debug.Assert(!string.IsNullOrEmpty(uri));
            m_uri = uri;
            m_contactCards = new List<ContactCard>();

            SipUriParser parser = new SipUriParser(uri);
            m_displayName = parser.User;
        }

        #endregion

        #region Internal properties

        public Contact Contact
        {
            get;
            internal set;
        }

        internal void AddContactCard(ContactCard card)
        {
            if (!string.IsNullOrEmpty(card.DisplayName))
            {
                m_displayName = card.DisplayName.Trim(new char[]{' ','\n'});
            }

            m_contactCards.Add(card);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string Uri
        {
            get
            {
              return m_uri;
            }
        }

        public string DisplayName
        {
            get
            {
                return m_displayName;
            }
        }

        public PresenceAvailability Availability
        {
            get;
            internal set;
        }

        #endregion
    }
    
    public class CustomerContactManager :ComponentBase
    {
        #region Private fields
        private readonly CustomerSession m_customerSession;
        private MyUserEndpoint m_myUserEndpoint;
        private ContactGroupServices m_contactGroupServices;
        private RemotePresenceView m_remotePresenceView;
        private int m_subCount; // # of subscriptions in subscribed or terminated state.
        private readonly Dictionary<string, ContactInformation> m_contacts;
        private AsyncTask m_presenceWaitAction; // Waits for all subscriptions to happen.
        private object m_syncRoot = new object();
        #endregion

        #region Constructor

        internal CustomerContactManager(CustomerSession customerSession):base(customerSession.AppFrontEnd.AppPlatform)
        {
            Debug.Assert(customerSession != null, "customerSession should not be null");
            m_customerSession = customerSession;
            m_contacts = new Dictionary<string, ContactInformation>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Public methods

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IDictionary<string,ContactInformation> GetContacts()
        {
            lock (this.SyncRoot)
            {
                return new Dictionary<string, ContactInformation>(m_contacts);
            }
        }

        #endregion

        #region protected methods

        
        private UserEndpoint UserEndpoint
        {
            get
            {
                UserEndpoint ep = null;
                if (m_myUserEndpoint != null)
                {
                    ep = m_myUserEndpoint.UserEndpoint;
                }
                return ep;
            }
        }

        protected override void StartupCore()
        {
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.Name = "CustomerContactMangerStartup";
            sequence.SuccessCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.FailureCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.AddTask(new AsyncTask(this.StartupUserEndpoint));
            sequence.AddTask(new AsyncTask(this.StartupContactSubscription));
            // Create presence view for the contacts. This sample does not handle dynamically added/removed contact's presence.
            sequence.AddTask(new AsyncTask(this.StartPresenceView));
            sequence.AddTask(new AsyncTask(this.StartWaitForPresence));
            sequence.Start();
        }

        protected override void ShutdownCore()
        {
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.Name = "CustomerContactMangerShutdown";
            sequence.SuccessCompletionReportHandlerDelegate = this.CompleteShutdown;
            sequence.FailureCompletionReportHandlerDelegate = this.CompleteShutdown;
            sequence.AddTask(new AsyncTask(this.ShutdownPresenceView));
            sequence.AddTask(new AsyncTask(this.ShutdownContactSubscription));
            sequence.AddTask(new AsyncTask(this.ShutdownUserEndpoint));
            sequence.Start();
        }

        private void StartupUserEndpoint(AsyncTask task, object state)
        {
            AsyncTask proxyTask = new AsyncTask(m_customerSession.AppFrontEnd.CreateOrGetUserEndpoint, m_customerSession.Customer.UserUri);
            proxyTask.TaskCompleted +=
                delegate(object sender, AsyncTaskCompletedEventArgs e)
                {
                    UserEndpointCreationActionResult result = proxyTask.TaskResult as UserEndpointCreationActionResult;
                    if (result != null)
                    {
                        m_myUserEndpoint = result.MyUserEndpoint;
                        m_contactGroupServices = m_myUserEndpoint.UserEndpoint.ContactGroupServices;
                    }
                    task.Complete(e.ActionResult.Exception);
                };
            proxyTask.StartTask();
        }


        private void ShutdownUserEndpoint(AsyncTask task, object state)
        {
            AsyncTask proxyTask = new AsyncTask(m_customerSession.AppFrontEnd.RelaseUserEndpoint, m_myUserEndpoint);
            proxyTask.TaskCompleted +=
                delegate(object sender, AsyncTaskCompletedEventArgs e)
                {
                    task.Complete(e.ActionResult.Exception);
                };
            proxyTask.StartTask();
        }


        #endregion

        #region Private methods

        public override void CompleteStartup(Exception exception)
        {
            if(exception != null)
            {
                this.Logger.Log(Logger.LogLevel.Error,exception);
                this.Shutdown();
            }

            base.CompleteStartup(exception);
        }

        private void Shutdown()
        {
            this.BeginShutdown(ar => this.EndShutdown(ar), null);
        }

        private void StartupContactSubscription(AsyncTask task, object state)
        {
            if (m_contactGroupServices.CurrentState != CollaborationSubscriptionState.Idle)
            {
                task.Complete(null);
                return;
            }
            m_contactGroupServices.NotificationReceived += this.ContactGroupServices_NotificationReceived;
            task.DoOneStep(
                delegate()
                {
                    Logger.Log(Logger.LogLevel.Info, "Starting contact subscription.");
                    m_contactGroupServices.BeginSubscribe(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_contactGroupServices.EndSubscribe(ar);
                                    Logger.Log(Logger.LogLevel.Info, "Started contact subscription.");
                                });
                        },
                        null);
                });
        }

        private void ShutdownContactSubscription(AsyncTask task, object state)
        {
            if (m_contactGroupServices.CurrentState != CollaborationSubscriptionState.Subscribed)
            {
                task.Complete(null);
                return;
            }
            m_contactGroupServices.NotificationReceived -= this.ContactGroupServices_NotificationReceived;

            task.DoOneStep(
                delegate()
                {
                    Logger.Log(Logger.LogLevel.Info, "Starting contact unsubscription.");
                    m_contactGroupServices.BeginUnsubscribe(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_contactGroupServices.EndUnsubscribe(ar);
                                    Logger.Log(Logger.LogLevel.Info, "Started contact unsubscription.");
                                });
                        },
                        null);
                });
        }

        private void StartPresenceView(AsyncTask task, object state)
        {
            m_remotePresenceView = new RemotePresenceView(this.UserEndpoint, new RemotePresenceViewSettings());
            m_remotePresenceView.PresenceNotificationReceived += this.PresenceView_NotificationReceived;
            m_remotePresenceView.SubscriptionStateChanged += RemoteSubscriptionStateChanged;
            this.SubscribeToRemotePresentities();
            task.Complete(null);
        }

        void RemoteSubscriptionStateChanged(object sender, RemoteSubscriptionStateChangedEventArgs e)
        {
            bool completeAction = false;
            lock (m_syncRoot)
            {
                foreach (KeyValuePair<RealTimeAddress, RemotePresentityStateChange> pair in e.SubscriptionStateChanges)
                {
                    if (pair.Value.PreviousState == RemotePresentitySubscriptionState.Subscribing)
                    {
                        m_subCount++; // Went from subscribing to some other state. Could be Terminating or Subscribed or WaitingForRetry etc.
                    }
                }
                if (m_subCount == m_contacts.Count)
                {
                    completeAction = true;
                }
                if (completeAction && m_presenceWaitAction != null)
                {
                    m_presenceWaitAction.Complete(null);
                }
            }
        }

        private void ShutdownPresenceView(AsyncTask task, object state)
        {
            RemotePresenceView presenceView = m_remotePresenceView;
            if (presenceView == null)
            {
                task.Complete(null);
                return;
            }
            m_remotePresenceView.PresenceNotificationReceived -= this.PresenceView_NotificationReceived;
            task.DoOneStep(
                delegate()
                {
                    presenceView.BeginTerminate(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    presenceView.EndTerminate(ar);
                                });
                        },
                        null);
                });
        }

        private void StartWaitForPresence(AsyncTask task, object state)
        {
            lock(m_syncRoot)
            {
                if (m_subCount == m_contacts.Count)
                {
                    task.Complete(null);
                }
                else
                {
                    m_presenceWaitAction = task;
                }
            }
        }

        /// <summary>
        /// Handles contact notification.
        /// </summary>
        /// <param name="sender">The sender of the event. ContactGroupServices.</param>
        /// <param name="e">The event argument.</param>
        private void ContactGroupServices_NotificationReceived(object sender, ContactGroupNotificationEventArgs e)
        {
            // This handler should be called once at least before subscription operation completes.
            lock (this.SyncRoot)
            {
                if (e.IsFullNotification)
                {
                    m_contacts.Clear();
                }

                foreach (var contact in e.Contacts)
                {
                    if (contact.Operation == PublishOperation.Delete || contact.Operation == PublishOperation.Update)
                    {
                        if (m_contacts.ContainsKey(contact.Item.Uri))
                        {
                            m_contacts.Remove(contact.Item.Uri);
                        }
                    }
                    if (contact.Operation != PublishOperation.Delete)
                    {
                        ContactInformation info = new ContactInformation(contact.Item.Uri);
                        info.Contact = contact.Item;
                        m_contacts.Add(contact.Item.Uri, info);
                    }
                }
            }
        }

        private void SubscribeToRemotePresentities()
        {
            try
            {
                Collection<RemotePresentitySubscriptionTarget> remotePresentities = new Collection<RemotePresentitySubscriptionTarget>();
                lock (this.SyncRoot)
                {
                    foreach(string uri in m_contacts.Keys)
                    {
                        remotePresentities.Add(new RemotePresentitySubscriptionTarget(uri));
                    }
                }
                if (remotePresentities.Count > 0)
                {
                    m_remotePresenceView.StartSubscribingToPresentities(remotePresentities);
                }
            }
            catch (InvalidOperationException ioe)
            {
                this.Logger.Log(Logger.LogLevel.Error,ioe);
            }
        }
         
       
        private void PresenceView_NotificationReceived(
            object sender,
            RemotePresentitiesNotificationEventArgs e)
        {
            lock(this.SyncRoot)
            {                
                foreach (var notification in e.Notifications)
                {
                    ContactInformation info = null;
                    if (!m_contacts.TryGetValue(notification.PresentityUri, out info))
                    {
                        continue;
                    }

                    foreach (var category in notification.Categories)
                    {
                        if (category.Name == PresenceCategoryNames.ContactCard)
                        {
                            var card = new ContactCard(category);
                            info.AddContactCard(card);
                        }
                        else if (category.Name == PresenceCategoryNames.State)
                        {
                            string rawXml = category.Category.GetCategoryDataXml();
                            if (string.IsNullOrEmpty(rawXml))
                            {
                                continue;
                            }

                            PresenceState state = new PresenceState(category);

                            var availability = state.Availability;
                            if (availability != PresenceAvailability.None)
                            {
                                info.Availability = availability;
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
