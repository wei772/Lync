/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

// .NET namespaces
using System;
using System.IO;
using System.Xml;

// UCMA namespaces
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.PublishPresence
{
    public class UCMASamplePublishPresence
    {
        #region Locals
        // This is an example of Note category being specified via XML.
        // A category can either be specified via XML or directly through a 
        // property of the Presence object of an endpoint. The below is an 
        // example of specifying a category via XML. The value of the Note 
        // category is displayed in the Microsoft Lync user interface. This value 
        // will be displayed while the sample is running; and will be reset to 
        // its original value upon exit from this sample code.
        private static String _noteXml = "<note xmlns=\"http://schemas.microsoft.com/2006/09/sip/note\" >"
            + "<body type=\"personal\" uri=\"\" >{0}</body></note>";
        private static string _noteValue = "Gone Fishing";

        // Category Note published using raw xml.
        private CustomPresenceCategory _note;

        // This variable stores the helper class instance.
        private UCMASampleHelper _helper;

        #region UCMA 3.0 Core Classes
        // This variable stores the user endpoint created on behalf of the user 
        // that the sample logs-in as.
        private UserEndpoint _userEndpoint;

        // This class encapsulates the presence of the sample's user endpoint.
        private LocalOwnerPresence _localOwnerPresence;

        // This variable is used to publish the availability of the sample's 
        // logged-in user.
        private PresenceState _userState; 

        // This variable is used to publish the state of the samples's logged-in
        // user's phone.
        private PresenceState _phoneState;

        // This variable is used to publish the state of the sample's logged-in 
        // user's computer.
        private PresenceState _machineState;

        // This variable is used to publish the ContactCard of the sample's 
        // logged-in user.
        private ContactCard _contactCard;

        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the PublishPresence quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASamplePublishPresence ucmaSamplePublishPresence = new UCMASamplePublishPresence();
            ucmaSamplePublishPresence.Run();
        }

        /// <summary>
        /// Retrieves the application configuration and begins running the 
        /// sample.
        /// </summary>
        private void Run()
        {
            try
            {
                // Prepare and instantiate the platform.
                _helper = new UCMASampleHelper();
                UserEndpointSettings userEndpointSettings = _helper.ReadUserSettings(
                    "PublishPresence Sample User" /*friendly name of the sample's user endpoint*/);

                // Set auto subscription to LocalOwnerPresence.
                userEndpointSettings.AutomaticPresencePublicationEnabled = true;
                _userEndpoint = _helper.CreateUserEndpoint(userEndpointSettings);

                // LocalOwnerPresence is the main class to manage the 
                // sample user's presence data.
                _localOwnerPresence = _userEndpoint.LocalOwnerPresence;

                // Wire up handlers to receive presence notifications to self.
                _localOwnerPresence.PresenceNotificationReceived
                    += LocalOwnerPresence_PresenceNotificationReceived;

                // Establish the endpoint.
                _helper.EstablishUserEndpoint(_userEndpoint);

                // Publish presence categories with the new values that 
                // are outlined in the sample.
                PublishPresenceCategories(true /* publish presence categories */);
                Console.WriteLine("Note, AggregateState, and ContactCard published.");

                // Wait for user to continue.
                UCMASampleHelper.PauseBeforeContinuing(
                    "Press ENTER to continue and delete the published presence.");

                // Delete presence categories, returning them to the original 
                // values before the sample was run.
                PublishPresenceCategories(false /* delete presence categories */);

                // Wait for user to continue.
                UCMASampleHelper.PauseBeforeContinuing("Press ENTER to shutdown and exit.");

                // Un-wire the presence notification event handler.
                _localOwnerPresence.PresenceNotificationReceived
                    -= LocalOwnerPresence_PresenceNotificationReceived;
            }
            finally
            {
                // Shut down platform before exiting the sample.
                _helper.ShutdownPlatform();
            }
        }

        // AsyncCallback to publish presence state.
        private void PublishPresenceCompleted(IAsyncResult result)
        {
            try
            {
                // Since the same call back function is used to publish
                // presence categories and to delete presence categories,
                // retrieve the flag indicating which operation is desired.
                bool isPublishOperation;
                bool.TryParse(result.AsyncState.ToString(), out isPublishOperation);

                if (isPublishOperation)
                {
                    // Complete the publishing of presence categories.
                    _localOwnerPresence.EndPublishPresence(result);
                    Console.WriteLine("Presence state has been published.");
                }
                else
                {
                    // Complete the deleting of presence categories.
                    _localOwnerPresence.EndDeletePresence(result);
                    Console.WriteLine("Presence state has been deleted.");
                }
            }
            catch (PublishSubscribeException pse)
            {
                // PublishSubscribeException is thrown when there were
                // exceptions during the publication of this category such as
                // badly formed sip request, duplicate publications in the same
                // request etc
                // TODO (Left to the reader): Include exception handling code
                // here
                Console.WriteLine(pse.ToString());
            }
            catch (RealTimeException rte)
            {
                // RealTimeException is thrown when SIP Transport, SIP
                // Authentication, and credential-related errors are
                // encountered.
                // TODO (Left to the reader): Include exception handling code
                // here.
                Console.WriteLine(rte.ToString());
            }
        }

        // Publish note, state, contact card presence categories
        private void PublishPresenceCategories(bool publishFlag)
        {
            try
            {
                if (publishFlag == true)
                {
                    // The CustomPresenceCategory class enables creation of a
                    // category using XML. This allows precise crafting of a
                    // category, but it is also possible to create a category
                    // in other, more simple ways, shown below.
                    _note = new CustomPresenceCategory("note", String.Format(_noteXml, _noteValue));

                    // The PresenceState class has several static properties
                    // and methods which will provide standard states such as
                    // online, busy, and on-the-phone, for example.
                    _userState = PresenceState.UserBusy;

                    // It is possible to create and publish state with a
                    // custom availablity string, shown below. "In a call" will
                    // be shown in Microsoft Lync.
                    LocalizedString localizedCallString = new LocalizedString(
                        "In a call" /* The string to be displayed. */);

                    // Create a PresenceActivity indicating the
                    // "In a call" state.
                    PresenceActivity inACall = new PresenceActivity(
                        localizedCallString);

                    // Set the Availability of the "In a call" state to Busy.
                    inACall.SetAvailabilityRange((int)PresenceAvailability.Busy,
                        (int)PresenceAvailability.IdleBusy);

                    // Microsoft Lync will also show the Busy presence icon.
                    _phoneState = new PresenceState(
                        (int)PresenceAvailability.Busy,
                        inACall,
                        PhoneCallType.Voip,
                        "phone uri");

                    // Machine or Endpoint states must always be published to
                    // indicate the endpoint is actually online, otherwise it is
                    // assumed the endpoint is offline, and no presence
                    // published from that endpoint will be displayed.
                    _machineState = PresenceState.EndpointOnline;

                    // It is also possible to create presence categories such
                    // as ContactCard, Note, PresenceState, and Services with
                    // their constructors.
                    // Here we create a ContactCard and change the title.
                    _contactCard = new ContactCard(3);
                    LocalizedString localizedTitleString = new LocalizedString(
                        "The Boss" /* The title string to be displayed. */);
                    _contactCard.JobTitle = localizedTitleString.Value;

                    // Publish a photo
                    // If the supplied value for photo is null or empty, then set value of IsAllowedToShowPhoto to false
                    _contactCard.IsAllowedToShowPhoto = true;
                    string photoUri = UCMASampleHelper.PromptUser("Please enter a Photo Uri in the form of http://mysite/photo1.jpg", "PhotoURI1");
                    if (String.IsNullOrEmpty(photoUri))
                    {
                        photoUri = null;
                        _contactCard.IsAllowedToShowPhoto = false;
                    }
                    _contactCard.PhotoUri = photoUri;

                    // Publish all presence categories with new values.
                    _localOwnerPresence.BeginPublishPresence(
                        new PresenceCategory[]
                        {
                            _userState,
                            _phoneState,
                            _machineState,
                            _note,
                            _contactCard
                        },
                        PublishPresenceCompleted, /* async callback when publishing operation completes. */
                        publishFlag /* value TRUE indicates that presence to be published with new values. */);
                }
                else
                {
                    // Delete all presence categories.
                    _localOwnerPresence.BeginDeletePresence(
                        new PresenceCategory[]
                        {
                            _userState,
                            _phoneState,
                            _machineState,
                            _note,
                            _contactCard
                        },
                        PublishPresenceCompleted,
                        publishFlag /* value FALSE indicates that presence reverted to original values. */);
                }
            }
            catch (PublishSubscribeException pse)
            {
                // PublishSubscribeException is thrown when there were
                // exceptions during this presence operation such as badly
                // formed sip request, duplicate publications in the same
                // request etc.
                // TODO (Left to the reader): Include exception handling code
                // here.
                Console.WriteLine(pse.ToString());
            }
            catch (RealTimeException rte)
            {
                // RealTimeException is thrown when SIP Transport, SIP
                // Authentication, and credential-related errors are 
                // encountered.
                // TODO (Left to the reader): Include exception handling code
                // here.
                Console.WriteLine(rte.ToString());
            }
        }

        /// <summary>
        /// The event handler for the Category Notification. Notifications come
        /// in the form of a list of items. We are only interested in state
        /// publications here, so we will only process those.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocalOwnerPresence_PresenceNotificationReceived(object sender,
            LocalPresentityNotificationEventArgs e)
        {
            Console.WriteLine("Presence notifications received for target {0}.", 
                                this._userEndpoint.OwnerUri);
            // Notifications contain all the notifications for one user.
            // Each user will send a list of updated categories. We will choose
            // the ones we are interested in and process them.
          
            // Display to console the value of the Note category.
            if (e.PersonalNote != null)
            {
                Console.WriteLine("Note type {0} with message {1} received.",
                    NoteType.Personal,
                    e.PersonalNote.Message);
            }

            // Display to console the value of the Aggregate Presence category.
            if (e.AggregatedPresenceState != null)
            {
                Console.WriteLine("Aggregate State = " + e.AggregatedPresenceState.Availability);
            }
            
            // Display to console the value of the capabilities of the 
            // sample's user endpoint.
            if (e.ServiceCapabilities != null)
            {
                Console.WriteLine("Is IM enabled: ", e.ServiceCapabilities.InstantMessagingEnabled
                    == ServiceCapabilitySupport.Enabled);
                Console.WriteLine("Is Audio enabled: ", e.ServiceCapabilities.AudioEnabled
                    == ServiceCapabilitySupport.Enabled);
                Console.WriteLine("Is AppSharing enabled: ", e.ServiceCapabilities.ApplicationSharingEnabled
                    == ServiceCapabilitySupport.Enabled);
            }
            
            // Display to console the value of the ContactCard category.
            if (e.ContactCard != null)
            {
                Console.WriteLine("Title = {0}", e.ContactCard.JobTitle);
            }

        }
        #endregion
    }
}
