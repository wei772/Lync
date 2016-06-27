/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

// .NET namespaces
using System;
using System.Collections.Generic;

// UCMA namespaces
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.SubscribePresenceView
{
    public class UCMASampleSubscribePresenceView
    {
        #region Locals
        #region UCMA 3.0 Core Classes
        /// <summary>
        /// A helper class to abstract out code that is not pertinent to this
        /// application.
        /// </summary>
        private UCMASampleHelper _helper;

        /// <summary>
        /// The endpoint designed to be used for client operations.
        /// </summary>
        private UserEndpoint _userEndpoint;

        /// <summary>
        /// The target that will be subscribed to.
        /// </summary>
        private RemotePresentitySubscriptionTarget _target;

        /// <summary>
        /// The view which maintains a persistent subscription to the
        /// RemotePresentitySubscriptionTarget.
        /// </summary>
        private RemotePresenceView _persistentView;

        /// <summary>
        /// The view which will periodically poll the
        /// RemotePresentitySubscriptionTarget.
        /// </summary>
        private RemotePresenceView _pollingView;
        #endregion

        #region Configuration Settings
        /// <summary>
        /// The Uri of the remote user being subscribed to.
        /// </summary>
        private static String _remoteUserUri;
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the SubscribePresenceView quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleSubscribePresenceView ucmaSampleSubscribePresenceView
                = new UCMASampleSubscribePresenceView();
            ucmaSampleSubscribePresenceView.Run();
        }

        /// <summary>
        /// Retrieves the application configuration and begins running the
        /// sample.
        /// </summary>
        private void Run()
        {
            // Prepare and instantiate the platform and an endpoint.
            _helper = new UCMASampleHelper();
            _userEndpoint = _helper.CreateEstablishedUserEndpoint(
                "SubscribePresenceView Sample User" /*endpointFriendlyName*/);

            // Get the Uri of the remote user to subscribe to.
            _remoteUserUri = "sip:" +
                UCMASampleHelper.PromptUser(
                "Please enter the URI, in the format User@Host, of the user to subscribe to => ",
                "RemoteUserURI");

            Console.WriteLine("\nChanging PresenceSubscriptionCategory Filter to only include ContactCard " +
                "and PresenceState");
            // Set category filter. This is a global filter for all persistent
            // subscriptions and can only be changed before any subscriptions
            // are active.
            // BUGBUG: error CS0618: 'Microsoft.Rtc.Collaboration.LocalEndpoint.RemotePresence' is obsolete: 'This property will be removed from future Versions. Please see RemotePresenceView and PresenceServices instead.'
            // _userEndpoint.RemotePresence.PresenceSubscriptionCategories =
            //    new string[] { PresenceCategoryNames.ContactCard, PresenceCategoryNames.State };

            // RemotePresencView objects can be used to group subscriptions.
            // When a RemotePresenceView is created, it is created with the
            // specified RemotePresenceViewSettings and associated with the
            // specified LocalEndpoint. The views can then be accessed via the
            // LocalEndpoint setting: RemotePresenceViews.

            // RemotePresenceView.ApplicationContext can be used to pass or
            // store information related to the view (seen below).

            // Create a RemotePresenceView with a persistent subscription mode.
            // This type of view can represent a contact list, for example.
            // Note: The Default SubscriptionMode will start a subscription as
            // Persistent and downgrade to Polling if an error occurs.
            var persistentSettings = new RemotePresenceViewSettings();
            persistentSettings.SubscriptionMode = RemotePresenceViewSubscriptionMode.Default;
            _persistentView = new RemotePresenceView(_userEndpoint, persistentSettings);
            _persistentView.ApplicationContext = "Persistent View";

            // Wire up event handlers for the view
            this.WireUpHandlersForView(_persistentView);

            // Create a RemotePresenceView with a polling subscription mode
            // This type of view can represent a list of people
            // on the To: line of an e-mail, for example.
            var pollingSettings = new RemotePresenceViewSettings();
            pollingSettings.SubscriptionMode = RemotePresenceViewSubscriptionMode.Polling;
            // The line below is not necessary; PollingInterval has a default
            // (and minimum) value of 5 minutes.
            pollingSettings.PollingInterval = TimeSpan.FromMinutes(5);
            _pollingView = new RemotePresenceView(_userEndpoint, pollingSettings);
            _pollingView.ApplicationContext = "Polling View";

            // Wire up event handlers for the view
            this.WireUpHandlersForView(_pollingView);

            Console.WriteLine("\nChanging Polling View's category filter to only include Note.");
            _pollingView.SetPresenceSubscriptionCategoriesForPolling(
                new string[] { PresenceCategoryNames.Note });

            try
            {
                // This constructor does very basic validation on the uri
                _target = new RemotePresentitySubscriptionTarget(_remoteUserUri);
            }
            catch (ArgumentException argumentException)
            {
                // ArgumentException will be thrown if the parameter used to
                // create the RemotePresentitySubscriptionTarget is an
                // invalid sip Uri.

                // TODO (Left to the reader): Error handling code to either
                // retry creating the target with corrected parameters, log
                // the error for debugging or gracefully exit the program.
                Console.WriteLine(argumentException.ToString());
                throw;
            }

            Console.WriteLine("\nInitiating subscriptions for both Views to user: " + _remoteUserUri);

            // Both Views will try to subscribe to the specified user
            // Note: if the target is a not a real user, these operations will
            // "succeed", but the StateChanged notifications will indicate the
            // subscription went to a Terminated state.
            _persistentView.StartSubscribingToPresentities(new RemotePresentitySubscriptionTarget[] { _target });
            _pollingView.StartSubscribingToPresentities(new RemotePresentitySubscriptionTarget[] { _target });

            // There is no callback for the StartSubscribingToPresentities
            // operation because subscriptions to multiple targets can
            // complete at different times. Completion or failure of the
            // subscription can be monitored through the
            // SubscriptionStateChanged event handler,
            // RemotePresenceView_NotificationReceived.

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to unsubscribe.");

            Console.WriteLine("\nBoth Views are terminating any subscriptions to user: " + _remoteUserUri);

            _persistentView.StartUnsubscribingToPresentities(new string[] { _remoteUserUri });
            _pollingView.StartUnsubscribingToPresentities(new string[] { _remoteUserUri });

            UnWireHandlersForView(_persistentView);
            UnWireHandlersForView(_pollingView);

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to shutdown and exit.");

            // Shutdown Platform
            _helper.ShutdownPlatform();
        }

        /// <summary>
        /// Helper to wire up important event handlers for a RemotePreenceView.
        /// </summary>
        /// <param name="view">View to add event handlers to.</param>
        private void WireUpHandlersForView(RemotePresenceView view)
        {
            Console.WriteLine("\nWiring up handlers for view: " + view.ApplicationContext);
            view.SubscriptionStateChanged += RemotePresenceView_SubscriptionStateChanged;
            view.PresenceNotificationReceived += RemotePresenceView_NotificationReceived;
        }

        /// <summary>
        /// Helper to unwire important event handlers for a RemotePresenceView.
        /// </summary>
        /// <param name="view">View to remove event handlers from.</param>
        private void UnWireHandlersForView(RemotePresenceView view)
        {
            Console.WriteLine("\nUn-wiring handlers for view: " + view.ApplicationContext);
            view.SubscriptionStateChanged -= RemotePresenceView_SubscriptionStateChanged;
            view.PresenceNotificationReceived -= RemotePresenceView_NotificationReceived;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Log the subscription states that have changed.
        /// </summary>
        /// <param name="sender">
        /// View that saw subscription state changes.
        /// </param>
        /// <param name="e">Data about the state changes</param>
        private void RemotePresenceView_SubscriptionStateChanged(
            object sender,
            RemoteSubscriptionStateChangedEventArgs e)
        {
            // Extract the view that raised the event.
            RemotePresenceView view = sender as RemotePresenceView;
            // The event args can contain multiple StateChanged notifications
            foreach (KeyValuePair<RealTimeAddress, RemotePresentityStateChange> stateChanged in
                e.SubscriptionStateChanges)
            {
                Console.WriteLine("\nView: " + view.ApplicationContext
                    + "; SubscriptionState for user: "
                    + stateChanged.Key /* uri of subscription target */
                    + " has changed from: " + stateChanged.Value.PreviousState
                    + " to: " + stateChanged.Value.State + ".");
            }
        }

        /// <summary>
        /// Log the presence notification for the remote user.
        /// </summary>
        /// <param name="sender">View that received the notification.</param>
        /// <param name="e">Data about the notifications received.</param>
        private void RemotePresenceView_NotificationReceived(object sender,
            RemotePresentitiesNotificationEventArgs e)
        {
            // Extract the RemotePresenceView that received the notification.
            RemotePresenceView view = sender as RemotePresenceView;
            // A RemotePresentityNotification will contain all the
            // categories for one user; Notifications can contain notifications
            // for multiple users.
            foreach (RemotePresentityNotification notification in e.Notifications)
            {
                Console.WriteLine("\nView: " + view.ApplicationContext
                    + " Received a Notification for user "
                    + notification.PresentityUri + ".");

                // If a category on notification is null, the category
                // was not present in the notification. This means there were no
                // changes in that category.
                if(notification.AggregatedPresenceState != null)
                {
                    Console.WriteLine("Aggregate State = " + notification.AggregatedPresenceState.Availability + ".");
                }

                if(notification.PersonalNote != null)
                {
                    Console.WriteLine("PersonalNote: " + notification.PersonalNote.Message + ".");
                }

                if (notification.ContactCard != null)
                {
                    // A ContactCard contains many properties; only display
                    // some.
                    ContactCard contactCard = notification.ContactCard;
                    Console.WriteLine("ContactCard Company: " + contactCard.Company + ".");
                    Console.WriteLine("ContactCard DisplayName: " + contactCard.DisplayName + ".");
                    Console.WriteLine("ContactCard EmailAddress: " + contactCard.EmailAddress + ".");
                }
            }
        }
        #endregion
    }
}
