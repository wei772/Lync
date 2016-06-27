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
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.SubscribePresence
{
    public class UCMASamplePresenceContainerMembership
    {
        #region Locals
        // The remote user being subscribed to.
        static String _subscriberUri;

        private static String _sipPrefix = "sip:";

        private static String _subscriberUriKey = "SubscriberURI";

        private static AutoResetEvent _waitEvent = new AutoResetEvent(false);

        private static int _blockedContainer = 32000;

        UCMASampleHelper _helper;

        #region UCMA 3.0 Core Classes
        private UserEndpoint _subscribee;
        #endregion
        #endregion

        #region Methods
        public static void Main(string[] args)
        {
            UCMASamplePresenceContainerMembership ucmaSamplePresenceContainerMembership
                = new UCMASamplePresenceContainerMembership();
            ucmaSamplePresenceContainerMembership.Run();
        }

        private void Run()
        {

            // Prepare and instantiate the platform.
            _helper = new UCMASampleHelper();
            UserEndpointSettings userEndpointSettings = _helper.ReadUserSettings(
                "PresenceContainerMembership Sample subscribee" /*endpointFriendlyName*/);

            // Set auto subscription to LocalOwnerPresence
            userEndpointSettings.AutomaticPresencePublicationEnabled = true;
            _subscribee = _helper.CreateUserEndpoint(userEndpointSettings);

            // Establish the endpoint
            _helper.EstablishUserEndpoint(_subscribee);

            _subscriberUri = UCMASampleHelper.PromptUser("Please Enter the subscriber's Uri in the form "
                + "sip:User@Hostuser. Please ensure that the uri is in the same domain as "
                + _subscriberUriKey,
                _subscriberUriKey);

            if (!_subscriberUri.StartsWith(_sipPrefix, StringComparison.OrdinalIgnoreCase))
            {
                _subscriberUri = _sipPrefix + _subscriberUri;
            }

            Console.WriteLine("{0} will block {1}, then unblock him. Please login to Microsoft Lync as {1}.",
                _subscribee.OwnerUri,
                _subscriberUri);

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to continue.");

            // First publish MachineStateOnline using default grammar. UCMA SDK
            // will publish to the correct containers.
            _subscribee.LocalOwnerPresence.BeginPublishPresence(
                new PresenceCategory[] { PresenceState.EndpointOnline, PresenceState.UserAvailable },
                    HandleEndPublishPresence,
                    null);

            Console.WriteLine("{0} has published 'Available'. ", _subscribee.OwnerUri);
            Console.WriteLine("Using Microsoft Lync, please subscribe to {0} when logged in as {1}. ",
                _subscribee.OwnerUri,
                _subscriberUri);
            Console.WriteLine("You should see that {0} is online and Available. ", _subscribee.OwnerUri);

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to continue.");

            ContainerUpdateOperation operation = new ContainerUpdateOperation(_blockedContainer); 
            operation.AddUri(_subscriberUri);
            _subscribee.LocalOwnerPresence.BeginUpdateContainerMembership(
                new ContainerUpdateOperation[] { operation },
                HandleEndUpdateContainerMembership, null);

            Console.WriteLine("{0} has added {1} to container {2} - the blocked container.",
                _subscribee.OwnerUri,
                _subscriberUri,
                _blockedContainer);
            Console.WriteLine("Microsoft Lync should display 'Offline' for user {0} now. ",
                _subscribee.OwnerUri);

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to continue.");

            operation = new ContainerUpdateOperation(_blockedContainer);
            operation.DeleteUri(_subscriberUri);
            _subscribee.LocalOwnerPresence.BeginUpdateContainerMembership(
                new ContainerUpdateOperation[] { operation },
                HandleEndUpdateContainerMembership, null);

            Console.WriteLine("{0} has removed {1} from the blocked container. Microsoft Lync should display "
                + "'online' for user {0} now. ",
                _subscribee.OwnerUri,
                _subscriberUri); 
            Console.WriteLine(" Sample complete. ");

            UCMASampleHelper.PauseBeforeContinuing("Press ENTER to shutdown and exit.");

            // Shutdown Platform
            _helper.ShutdownPlatform();
        }

        private void HandleEndUpdateContainerMembership(IAsyncResult result)
        {
            try
            {
                _subscribee.LocalOwnerPresence.EndUpdateContainerMembership(result);
            }
            catch (PublishSubscribeException ex)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An exception of type {0} was caught in {1}",
                    ex.GetType().Name,
                    "HandleEndUpdateContainerMembership");
            }
            catch (OperationFailureException ex)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An exception of type {0} was caught in {1}",
                    ex.GetType().Name,
                    "HandleEndUpdateContainerMembership");
            }
            // UCMA SDK exceptions should all derive from RealTimeException.
            catch (RealTimeException ex)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An exception of type {0} was caught in {1}",
                    ex.GetType().Name, "HandleEndUpdateContainerMembership");
            }
        }

        private void HandleEndPublishPresence(IAsyncResult result)
        {
            try
            {
                _subscribee.LocalOwnerPresence.EndPublishPresence(result);
            }
            catch (PublishSubscribeException ex)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An exception of type {0} was caught in {1}",
                    ex.GetType().Name,
                    "HandleEndPublishPresence");
            }
            catch (OperationFailureException ex)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An exception of type {0} was caught in {1}",
                    ex.GetType().Name,
                    "HandleEndPublishPresence");
            }
            // UCMA SDK exceptions should all derive from RealTimeException.
            catch (RealTimeException ex)
            {
                // TODO (Left to the reader): Write actual handling code for the
                // occurrence.
                Console.WriteLine("An exception of type {0} was caught in {1}",
                    ex.GetType().Name,
                    "HandleEndUpdateContainerMembership");
            }
        }

        // Event handler to process remote target's presence notifications
        private void RemotePresence_PresenceNotificationReceived(object sender,
            RemotePresentitiesNotificationEventArgs e)
        {
            // Notifications contain all the notifications for one user.
            foreach (RemotePresentityNotification notification in e.Notifications)
            {
                Console.WriteLine(String.Format("Presence notifications received from {0}",
                    notification.PresentityUri));
                if (notification.AggregatedPresenceState != null)
                {
                    Console.WriteLine("Aggregate State = "
                        + notification.AggregatedPresenceState.Availability);
                }
            }
        }
        #endregion
    }
}
