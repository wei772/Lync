
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities;
using System.Diagnostics;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities
{
    /// <summary>
    /// Represents UcPresence Provider class.
    /// </summary>
    internal class UcPresenceProvider
    {
        #region private variables

        /// <summary>
        /// Reference to application endpoint.
        /// </summary>
        private readonly ApplicationEndpoint m_applicationEndpoint;
        
        /// <summary>
        /// Reference to remote presence view.
        /// </summary>
        private RemotePresenceView m_remotePresenceView;

        #endregion

        #region internal events

        /// <summary>
        /// Event raised when prsence information changes.
        /// </summary>
        internal event EventHandler<PresenceChangedEventArgs> PresenceChanged;


        #endregion

        #region constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="endpoint">Application endpoint. Cannot be null.</param>
        internal UcPresenceProvider(ApplicationEndpoint endpoint)
        {
            Debug.Assert(null != endpoint, "Endpoint is null");
            m_applicationEndpoint = endpoint;
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Starts the provider.
        /// </summary>
        /// <returns>True if uc presence prvovider was started successfully.</returns>
        internal void Start()
        {
            // RemotePresenceView provides a unified way to request presence for a set of presentities
            m_remotePresenceView = new RemotePresenceView(
                m_applicationEndpoint,
                new RemotePresenceViewSettings() { SubscriptionMode = RemotePresenceViewSubscriptionMode.Default });

            this.RegisterEventHandlers(m_remotePresenceView);
            Helper.Logger.Info("UcPresenceProvider started");
        }

        /// <summary>
        /// Stops the provider.
        /// </summary>
        internal void Stop()
        {
            this.UnRegisterEventHandlers(m_remotePresenceView);
            Helper.Logger.Info("UcPresenceProvider stopped");
        }

        /// <summary>
        /// Subscribes to presence of a specific uri.
        /// </summary>
        /// <param name="sipUri">Uri. Cannot be null or empty.</param>
        internal void SubscribeToPresence(string sipUri)
        {
            Debug.Assert(!String.IsNullOrEmpty(sipUri), "Sip uri is null or empty");
            if (!this.IsAlreadySubscribed(sipUri))
            {
                this.Subscribe(sipUri);
            }
        }

        /// <summary>
        /// UnSubscribes to presence of a specific uri.
        /// </summary>
        /// <param name="sipUri">Uri. Cannot be null or empty.</param>
        internal void UnsubscribeToPresence(string sipUri)
        {
            Debug.Assert(!String.IsNullOrEmpty(sipUri), "Sip uri is null or empty");
            if (this.IsAlreadySubscribed(sipUri))
            {
                this.Unsubscribe(sipUri);
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Registers event handlers.
        /// </summary>
        /// <param name="remotePresenceView">Remote presence view.</param>
        private void RegisterEventHandlers(RemotePresenceView remotePresenceView) 
        {
            if(remotePresenceView != null)
            {
                // Wire up NotificationReceived before adding targets
                remotePresenceView.PresenceNotificationReceived += this.RemotePresenceView_PresenceNotificationReceived;
            }
        }

        /// <summary>
        /// UnRegisters event handlers.
        /// </summary>
        /// <param name="remotePresenceView">Remote presence view.</param>
        private void UnRegisterEventHandlers(RemotePresenceView remotePresenceView) 
        {
            if(remotePresenceView != null)
            {
                // Wire up NotificationReceived before adding targets
                remotePresenceView.PresenceNotificationReceived -= this.RemotePresenceView_PresenceNotificationReceived;
            }
        }

        /// <summary>
        /// Checks if a given uri is already subscribed or not.
        /// </summary>
        /// <param name="sipUri">Uri. Cannot be null or empty.</param>
        /// <returns>True if the uri is already subscribed to. False otherwise.</returns>
        private bool IsAlreadySubscribed(string sipUri)
        {
            Debug.Assert(!String.IsNullOrEmpty(sipUri), "Sip uri is null or empty");
            RemotePresenceView remotePresenceView = m_remotePresenceView;
            bool retVal = false;
            if (remotePresenceView != null)
            {
                var subscriptions = remotePresenceView.GetPresentities();
                retVal = subscriptions.Any(s => s.Equals(sipUri, StringComparison.InvariantCultureIgnoreCase));
            }
            return retVal;
        }

        /// <summary>
        /// Subscribe to a specific uri.
        /// </summary>
        /// <param name="sipUri">Sip uri to subscribe to. Cannot be null or empty.</param>
        private void Subscribe(string sipUri)
        {
            Debug.Assert(!String.IsNullOrEmpty(sipUri), "Sip uri is null or empty");
            SipUriParser uriParser;
            if (SipUriParser.TryParse(sipUri, out uriParser))
            {
                var subscriptionTargets = new List<RemotePresentitySubscriptionTarget>();
                subscriptionTargets.Add(new RemotePresentitySubscriptionTarget(uriParser.ToString()));
                // Immediately fires NotificationReceived with current presence of targets
                m_remotePresenceView.StartSubscribingToPresentities(subscriptionTargets);
            }
        }

        /// <summary>
        /// Unsubscribe from a specific uri.
        /// </summary>
        /// <param name="sipUri">Sip uri to unsubscribe from. Cannot be null or empty.</param>
        private void Unsubscribe(string sipUri)
        {
            Debug.Assert(!String.IsNullOrEmpty(sipUri), "Sip uri is null or empty");
            SipUriParser uriParser;
            if (SipUriParser.TryParse(sipUri, out uriParser))
            {
                m_remotePresenceView.StartUnsubscribingToPresentities(new List<string> { sipUri });
            }
        }

        /// <summary>
        /// Presence notification received.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void RemotePresenceView_PresenceNotificationReceived(object sender, RemotePresentitiesNotificationEventArgs e)
        {
            List<PresenceInformation> presenceSubscriptions = new List<PresenceInformation>();
            foreach (var notification in e.Notifications)
            {
                presenceSubscriptions.Add(ConvertNotificationToPresence(notification));
            }
            var presenceChanged = this.PresenceChanged;
            if (presenceChanged != null)
            {
                presenceChanged(this, new PresenceChangedEventArgs { PresenceSubscriptions = presenceSubscriptions });
            }
        }

        /// <summary>
        /// Helper method to convery notification to presence.
        /// </summary>
        /// <param name="notification">Notification</param>
        /// <returns>Presence subscription</returns>
        private PresenceInformation ConvertNotificationToPresence(RemotePresentityNotification notification)
        {
            //after the first notification for a contact, you only get the categories that changed next time
            //so after the first notification if presence changes, you only get presence; if name changes you only get contact card
            long availability = PresenceInformation.NoChangeInAvailability;
            string status = PresenceInformation.NoChange;
            string contactName = PresenceInformation.NoChange;
            string activityStatus = PresenceInformation.NoChange;

            var presence = notification.AggregatedPresenceState;
            if (presence != null)
            {
                //Get availability.
                availability = presence.AvailabilityValue;
                status = AvailabilityToStatusConverter.Convert(availability);
                //Get activity status.
                PresenceActivity activity = presence.Activity;
                if (activity != null && activity.CustomTokens != null && activity.CustomTokens.Count > 0)
                {
                    //For now just take the first token.
                    activityStatus = activity.CustomTokens[0].Value ?? PresenceInformation.NoChange;
                }
            }
            
            var contactCard = notification.ContactCard;
            if (contactCard != null)
            {
                contactName = contactCard.DisplayName;
            }


            return new PresenceInformation
            {
                Availability = availability,
                ContactName = contactName,
                SipUri = notification.PresentityUri,
                Status = status,
                ActivityStatus = activityStatus
            };
        }

        private bool InitializeRemotePresenceView()
        {
            try
            {
                // RemotePresenceView provides a unified way to request presence for a set of presentities
                m_remotePresenceView = new RemotePresenceView(
                    m_applicationEndpoint,
                    new RemotePresenceViewSettings() { SubscriptionMode = RemotePresenceViewSubscriptionMode.Default });


                // Wire up NotificationReceived before adding targets
                m_remotePresenceView.PresenceNotificationReceived +=
                    new EventHandler<RemotePresentitiesNotificationEventArgs>(RemotePresenceView_PresenceNotificationReceived);
                Helper.Logger.Info("UcPresenceProvider started");
                return true;
            }
            catch (Exception ex)
            {
                Helper.Logger.Error("UCPresenceProvider failed: Could not initialize remote presence view {0}", ex);
                return false;
            }
           
        }

        #endregion

    }
}
