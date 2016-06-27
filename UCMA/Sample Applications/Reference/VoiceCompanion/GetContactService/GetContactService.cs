/*=====================================================================
  File:      GetContactService.cs

  Summary:   Implements a service that connects the user to one of his contacts.

***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.VoiceServices;
using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.Utilities;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion;
using System.Globalization;
using VoiceCompanion.GetContactService.GetBuddyDialog;
using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
using VoiceCompanion.CallbackDialog;
using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.VoiceServices
{
    public class GetContactService : VoiceService
    {
        #region Public methods, constructors and properties

        public GetContactService(CustomerSession customerSession) :
            base(customerSession)
        {

        }

        public override string Id
        {
            get
            {
                return "C979E103-B6C2-4b0d-ACAE-9A2B0B35E336";
            }
        }

        #endregion

        #region Protected methods

        protected override void StartupCore()
        {
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.FailureCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.SuccessCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.AddTask(new AsyncTask(this.StartupGetContactDialog));
            sequence.Start();
        }

        protected override void ShutdownCore()
        {
            this.CompleteShutdown();
        }

        #endregion

        private void StartupGetContactDialog(AsyncTask task, object state)
        {
            task.DoFinalStep(
                delegate()
                {
                    //Start get buddy dialog to get the contact name, user wants to contact with.
                    GetBuddyDialog getcontactDialog = new GetBuddyDialog(this.CustomerSession, ApplicationConfiguration.GetBuddyConfiguration());
                    getcontactDialog.Completed += new EventHandler<DialogCompletedEventArgs>(this.GetContactDialogCompleted);
                    getcontactDialog.Run();

                });
        }


        /// <summary>
        ///  Get buddy dialog completion event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetContactDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            ContactInformation selectedContact;
            selectedContact = e.Output["selectedContact"] as ContactInformation;
            if (selectedContact == null)
            {
                //No contact was selected, end the service to return
                //back to the main menu.
                this.EndService(null);
                return;
            }
            System.Threading.ThreadPool.QueueUserWorkItem(this.StartCallbackDialog, selectedContact);
        }
        private void StartCallbackDialog(object state)
        {
            try
            {
                ContactInformation selectedContact = (ContactInformation)state;

                //Start callback dialog set up a callback with the selected contact.
                CallbackDialog callbackDialog = new CallbackDialog(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall
                                                                    , ApplicationConfiguration.GetSetupCallbackConfiguration()
                                                                    , selectedContact
                                                                     , this);
                callbackDialog.Completed += new EventHandler<DialogCompletedEventArgs>(this.CallbackDialogCompleted);
                callbackDialog.Run();
            }
            catch (InvalidProgramException exp)
            {
                this.EndService(exp);
            }
        }

        /// <summary>
        /// callback dialog completion handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallbackDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            CallbackAction nextAction;
            string contactUri;

            nextAction = (CallbackAction)e.Output["NextAction"];
            contactUri = e.Output.ContainsKey("ContactURI") ? e.Output["ContactURI"] as string : string.Empty;
            if (nextAction == CallbackAction.None || nextAction == CallbackAction.SetupCallback)
            {
                this.EndService(null);
            }
            else
            {
                System.Threading.ThreadPool.QueueUserWorkItem(this.InviteUser, contactUri);
            }
        }

        private void StartMusic()
        {
            this.StartMusic(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
        }

        private void StopMusic()
        {
            this.StopMusic(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
        }

        private void InviteUserCompleted(Exception exception)
        {
            if (exception != null)
            {
                Helpers.PlayCallWasDeclined(
                        this.CustomerSession.CustomerServiceChannel.ServiceChannelCall, new EventHandler<DialogCompletedEventArgs>(DeclinedDialogCompleted)); //Replaced workflow completed delegate with dialog completion dialog
            }
            else
            {
                this.EndService(null);
            }
        }

        /// <summary>
        ///  Call declined dialog completion event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeclinedDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            this.EndService(null);
        }

        private void InviteUser(object state)
        {
            string contactUri = (string)state;

            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.SuccessCompletionReportHandlerDelegate = this.InviteUserCompleted;
            sequence.FailureCompletionReportHandlerDelegate = this.InviteUserCompleted;
            AsyncTask conferenceInviteAction = new AsyncTask(this.SendConferenceInvitation, contactUri);
            sequence.AddTask(conferenceInviteAction);
            AsyncTask waitAction = new AsyncTask(this.CustomerSession.RosterTrackingService.StartupWaitForNewParticipant,
                                                 this.CustomerSession.RosterTrackingService.ParticipantCount);
            sequence.AddTask(waitAction);
            sequence.Start();

        }

        private void SendConferenceInvitation(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    string uriToDialOutTo = (string)state;
                    McuDialOutOptions options = new McuDialOutOptions();
                    options.Issuer = this.CustomerSession.CustomerConversation.LocalParticipant;
                    ConferenceInvitationSettings convSettings = new ConferenceInvitationSettings();
                    convSettings.AvailableMediaTypes.Add(MediaType.Audio);
                    var confInvitation = new ConferenceInvitation(this.CustomerSession.CustomerConversation, convSettings);
                    this.StartMusic();
                    confInvitation.BeginDeliver(
                        uriToDialOutTo,
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    this.StopMusic();
                                    confInvitation.EndDeliver(ar);
                                });
                        },
                        null);
                });
        }


        private void EndService(Exception e)
        {
            if (e != null)
            {
                this.Logger.Log(Logger.LogLevel.Error, string.Format(CultureInfo.InvariantCulture, "Service {0} encountered {1}", this.Id, e.ToString()));
            }
            Helpers.DetachFlowFromAllDevices(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
            this.BeginShutdown(ar => this.EndShutdown(ar), null);
        }

    }
}
