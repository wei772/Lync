/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

// .NET namespaces
using System;
using System.IO;
using System.Threading;
using System.Xml;

// UCMA namespaces
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.ContactsGroups;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.SubscribePresence
{
    public class UCMASampleSubscribePresence
    {
        #region Locals
        // The remote user being subscribed to.
        private static String _remoteUserUri;

        // Helper class instance.
        private UCMASampleHelper _helper;

        #region UCMA 3.0 Core Classes
        private UserEndpoint _userEndpoint;

        // Instance handle to the presence of the remote user.
        private RemotePresenceView _remotePresenceView;

        // Instance handle to the presentity of the remote user.
        private RemotePresentitySubscriptionTarget _target;
        #endregion

        // Id of the ContactGroup that will house the subscription
        // to the remote user.
        private int _groupId;

        // Name of the ContactGroup.
        private string _groupName = "MyGroup";

        private AutoResetEvent _waitForGroupIdSet = new AutoResetEvent(false);

        private AutoResetEvent _waitForSubscribedToContactsGroupsCompleted = new AutoResetEvent(false);

        private AutoResetEvent _waitForGroupDeleted = new AutoResetEvent(false);
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the SubscribePresence quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleSubscribePresence ucmaSampleSubscribePresence = new UCMASampleSubscribePresence();
            ucmaSampleSubscribePresence.Run();
        }

        /// <summary>
        /// Retrieves the application configuration and runs the sample.
        /// </summary>
        private void Run()
        {
            // Prepare and instantiate the platform.
            _helper = new UCMASampleHelper();
            _userEndpoint = _helper.CreateEstablishedUserEndpoint(
                "SubscribePresence Sample User" /*endpointFriendlyName*/);

            // Get the URI of the remote user whose presence to subscribe to.
            _remoteUserUri = "sip:" + UCMASampleHelper.PromptUser(
                "Please enter the URI, in the format user@host, of the user whose Presence to subscribe to "
                + "=> ", "RemoteUserURI");

            // RemotePresenceView is the class to be used to subscribe to
            // another entity's presence.
            _remotePresenceView = new RemotePresenceView(_userEndpoint);

            // Wire up event handlers to receive the incoming notifications of
            // the remote user being subscribed to.
            _remotePresenceView.SubscriptionStateChanged += new EventHandler<
                RemoteSubscriptionStateChangedEventArgs>(
                RemotePresence_SubscriptionStateNotificationReceived);
            _remotePresenceView.PresenceNotificationReceived += new EventHandler<
                RemotePresentitiesNotificationEventArgs>(RemotePresence_PresenceNotificationReceived);

            try
            {
                // Subscribe to target user.
                _target = new RemotePresentitySubscriptionTarget(_remoteUserUri);
                _remotePresenceView.StartSubscribingToPresentities(
                                        new RemotePresentitySubscriptionTarget[] {_target });
            
                // Subscribe to ContactGroupServices.
                // This is done so that the user can add/delete groups and
                // add/delete contacts, among other operations.
                _userEndpoint.ContactGroupServices.NotificationReceived += new EventHandler
                    <Microsoft.Rtc.Collaboration.ContactsGroups.ContactGroupNotificationEventArgs>(
                    ContactGroupServices_NotificationReceived);
                _userEndpoint.ContactGroupServices.SubscriptionStateChange += new EventHandler
                    <PresenceSubscriptionStateChangedEventArgs>(ContactGroupServices_SubscriptionStateChange);
                _userEndpoint.ContactGroupServices.BeginSubscribe(EndSubscribeCompleted,
                    _userEndpoint.ContactGroupServices);

                // Wait for subscription to ContactsGroups to be completed.
                _waitForSubscribedToContactsGroupsCompleted.WaitOne();
                Console.WriteLine("Subscription to ContactsGroups completed.");

                // Create a new group.
                _userEndpoint.ContactGroupServices.BeginAddGroup(_groupName,
                                                                    null /* group data */,
                                                                    EndAddGroupCompleted,
                                                                    _userEndpoint.ContactGroupServices);

                // Wait for group to be created.
                _waitForGroupIdSet.WaitOne();

                // Add the remote user to the newly created group.
                ContactsGroups.ContactAddOptions addOptions
                                = new Microsoft.Rtc.Collaboration.ContactsGroups.ContactAddOptions();
                addOptions.GroupIds.Add(_groupId);
                _userEndpoint.ContactGroupServices.BeginAddContact(_remoteUserUri,
                                                                    addOptions,
                                                                    EndAddContactCompleted,
                                                                    _userEndpoint.ContactGroupServices);

                UCMASampleHelper.PauseBeforeContinuing("You are now subscribed to the presence of the remote "
                    + "user. \nPlease toggle the user state of the remote user to get the appropriate "
                    + "notifications. \nPress ENTER to delete the contact, delete the group, and unsubscribe"
                    + "to the presence of the remote user.");

                // Remove contact from group, and delete group.
                _userEndpoint.ContactGroupServices.BeginDeleteContact(_remoteUserUri,
                                                                        EndDeleteContactCompleted,
                                                                        _userEndpoint.ContactGroupServices);

            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Could not subscribe to the presence of the remote user: " + ex.ToString());
            }

            // Exit application.
            _waitForGroupDeleted.WaitOne();
            // Pause the console to for easier viewing of logs.
            Console.WriteLine("\n\n********************");
            Console.WriteLine("Press ENTER to shutdown and exit.");
            Console.WriteLine("********************\n\n");
            Console.ReadLine();


            // Shutdown Platform
            _helper.ShutdownPlatform();
        }

        // Event handler to process remote target's presence subscription state
        // changes
        private void RemotePresence_SubscriptionStateNotificationReceived(object sender,
            RemoteSubscriptionStateChangedEventArgs e)
        {
            foreach (RealTimeAddress address in e.SubscriptionStateChanges.Keys)
            {                
                if (address.Uri == _remoteUserUri)
                {
                    Console.WriteLine("Target {0} subscription state has changed from {1} to {2}",
                        _remoteUserUri,
                        e.SubscriptionStateChanges[address].PreviousState,
                        e.SubscriptionStateChanges[address].State);
                }
            }
        }

        // Event handler to process remote target's presence notifications
        private void RemotePresence_PresenceNotificationReceived(object sender,
            RemotePresentitiesNotificationEventArgs e)
        {
            Console.WriteLine(String.Format("Presence notifications received for target {0}",
                _remoteUserUri));
            // Notifications contain all the notifications for one user.
            foreach (RemotePresentityNotification notification in e.Notifications)
            {
                if (notification.AggregatedPresenceState != null)
                {
                    Console.WriteLine("Aggregate State = "
                        + notification.AggregatedPresenceState.Availability);
                }
            }
            Console.WriteLine("Press ENTER to delete the contact, delete the group, and unsubscribe to the "
                + "presence of the remote user.");

        }

        // Event handler to process ContactGroupServices notifications
        private void ContactGroupServices_NotificationReceived(object sender,
            ContactsGroups.ContactGroupNotificationEventArgs e)
        {

            foreach (NotificationItem<ContactsGroups.Contact> contactNotification in e.Contacts)
            {
                Console.WriteLine("Notification received for contact = {0} for operation {1}.",
                    contactNotification.Item.Name,
                    contactNotification.Operation);
            }
            foreach (NotificationItem<ContactsGroups.Group> groupNotification in e.Groups)
            {
                Console.WriteLine("Notification received for group = {0} with id = {1} for operation {2}.",
                    groupNotification.Item.Name,
                    groupNotification.Item.GroupId,
                    groupNotification.Operation);

                if (groupNotification.Item.Name == _groupName && groupNotification.Operation
                    == PublishOperation.Add)
                {
                    // Store the groupId for group created
                    // Note: This sample stores it as a scalar value, but it is
                    // highly recommended that you store the groupIds as a list
                    // for all groups you create
                    _groupId = groupNotification.Item.GroupId;

                    // Signal that the groupId was set
                    _waitForGroupIdSet.Set();
                }
            }
        }

        // Event handler to process ContactGroupServices subscription state
        // changes
        private void ContactGroupServices_SubscriptionStateChange(object sender,
            PresenceSubscriptionStateChangedEventArgs e)
        {
            Console.WriteLine("ContactsGroups subscription state changed from {0} to {1}",
                e.PreviousState,
                e.State);
        }

        // Callback triggered when subscription to ContactGroupServices
        // completes
        private void EndSubscribeCompleted(IAsyncResult ar)
        {
            ContactGroupServices services = ar.AsyncState as ContactGroupServices;
            try
            {
              services.EndSubscribe(ar);
              _waitForSubscribedToContactsGroupsCompleted.Set();
            }
            catch(RealTimeException exception)
            {
                Console.WriteLine("Contact Group Subscription failed due to exception: {0}",
                    exception.ToString());
            }
        }

        // Callback triggered when group is added
        private void EndAddGroupCompleted(IAsyncResult ar)
        {
            ContactGroupServices services = ar.AsyncState as ContactGroupServices;

            try
            {
                services.EndAddGroup(ar);
                Console.WriteLine("Group with name {0} was created.", _groupName);
            }
            catch (PublishSubscribeException psex)
            {
                if (psex.DiagnosticInformation.Reason.Contains("Duplicate group name"))
                {
                    Console.WriteLine("Group {0} was not added because it already exists.", _groupName);
                }
                else
                {
                    Console.WriteLine("Add Group failed due to exception: {0}", psex.ToString());
                }
            }
            catch (InvalidOperationException ioex)
            {
                Console.WriteLine("Add Group failed due to exception: {0}", ioex.ToString());
            }
            catch (OperationFailureException ofex)
            {
                Console.WriteLine("Add Group failed due to exception: {0}", ofex.ToString());
            }
            catch (RealTimeException rtex)
            {
                Console.WriteLine("Add Group failed due to exception: {0}", rtex.ToString());
            }
        }

        // Callback triggered when group is deleted
        private void EndDeleteGroupCompleted(IAsyncResult ar)
        {

            ContactGroupServices services = ar.AsyncState as ContactGroupServices;

            try
            {
                services.EndDeleteGroup(ar);
                Console.WriteLine("Group {0} was successfully deleted.", _groupName);
                _waitForGroupDeleted.Set();
            }
            catch (PublishSubscribeException psex)
            {
                Console.WriteLine("Group {0} could not be deleted due to exception: {1}",
                    _groupName, psex.ToString());
            }
            catch (InvalidOperationException ioex)
            {
                Console.WriteLine("Delete Group failed due to exception: {0}", ioex.ToString());
            }
            catch (OperationFailureException ofex)
            {
                Console.WriteLine("Delete Group failed due to exception: {0}", ofex.ToString());
            }
            catch (RealTimeException rtex)
            {
                Console.WriteLine("Delete Group failed due to exception: {0}", rtex.ToString());
            }
            finally
            {
                // Unsubscribe to target user
                _remotePresenceView.StartUnsubscribingToPresentities(new string[] { _target.Address.Uri });
            }
        }

        // Callback triggered when contact is added
        private void EndAddContactCompleted(IAsyncResult ar)
        {
            ContactGroupServices services = ar.AsyncState as ContactGroupServices;

            try
            {
                services.EndAddContact(ar);
                Console.WriteLine("Contact " + _remoteUserUri + " was added to group " + _groupName);
            }
            catch (PublishSubscribeException psex)
            {
                Console.WriteLine("Could not add contact due to exception: {0}", psex.ToString());
            }
            catch (InvalidOperationException ioex)
            {
                Console.WriteLine("Add Contact failed due to exception: {0}", ioex.ToString());
            }
            catch (OperationFailureException ofex)
            {
                Console.WriteLine("Add Contact failed due to exception: {0}", ofex.ToString());
            }
            catch (RealTimeException rtex)
            {
                Console.WriteLine("Add Contact failed due to exception: {0}", rtex.ToString());
            }
        }

        // Callback triggered when contact is deleted
        private void EndDeleteContactCompleted(IAsyncResult ar)
        {

            ContactGroupServices services = ar.AsyncState as ContactGroupServices;
            
            try
            {
                services.EndDeleteContact(ar);
                Console.WriteLine("Contact {0} was successfully deleted.", _remoteUserUri);
            }
            catch (PublishSubscribeException psex)
            {
                Console.WriteLine("Delete Contact failed due to exception: {0}", psex.ToString());
            }
            catch (InvalidOperationException ioex)
            {
                Console.WriteLine("Delete Contact failed due to exception: {0}", ioex.ToString());
            }
            catch (OperationFailureException ofex)
            {
                Console.WriteLine("Delete Contact failed due to exception: {0}", ofex.ToString());
            }
            catch (RealTimeException rtex)
            {
                Console.WriteLine("Delete Contact failed due to exception: {0}", rtex.ToString());
            }
            finally
            {
                services.BeginDeleteGroup(_groupId, EndDeleteGroupCompleted,
                    _userEndpoint.ContactGroupServices);
            }
        }
        #endregion
    }
}
