/*=====================================================================
  File:      ConferenceService.cs

  Summary:   Represents a conferencing service.

***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/




using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.Utilities;
using Microsoft.Rtc.Collaboration.AudioVideo;
using System.Globalization;
using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
using VoiceCompanion.SimpleStatementDialog;
using VoiceCompanion.ConferenceService.DtmfMenuDialog;
using VoiceCompanion.DialupDialog;
using VoiceCompanion.GetContactService.GetBuddyDialog;
using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.VoiceServices
{
    public class ConferenceService : VoiceService
    {
        #region Private fields
        private ConferenceServiceConfiguration m_configuration;       
        private const int c_OutOfDefaultRoutingDurationSecs = 12000000; 
        private ContactInformation m_selectedContact;
        //private WorkflowInstance m_dtmfInstance;
        private object m_syncRoot = new object();

        #endregion

        public ConferenceService(CustomerSession customerSession) :
            base(customerSession)
        {
            m_configuration = ApplicationConfiguration.GetConferenceServiceConfiguration();
        }

        public override string Id
        {
            get
            {
                return "01C400CB-E2DF-4970-A4D5-0FEEA4F46E3F";
                // return "047F245D-FFDD-4f7b-ABF7-D96046D46F4F"; // TODO: Check why this id was different from what is given in voiceservices.xml file.
            }
        }

        protected override void StartupCore()
        {
            this.CustomerSession.RosterTrackingService.ParticipantCountChanged += RosterParticipantCountChanged;
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.SuccessCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.FailureCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.AddTask(new AsyncTask(this.StartupInstructiondialog));
            sequence.Start();
        }

        protected override void ShutdownCore()
        {
            this.CustomerSession.RosterTrackingService.ParticipantCountChanged -= RosterParticipantCountChanged;
            this.CompleteShutdown();
        }

        private void StartupInstructiondialog(AsyncTask task, object state)
        {
            task.DoFinalStep(
                delegate()
                {
                    this.StartInstructionDialog(null);
                });
        }

        private void StartInstructionDialog(object state)
        {
            try
            {
                //Start simple dialog which speaks a  instruction statement.
                this.StartSimpleDialog(m_configuration.InstructionsStatement.MainPrompt, new EventHandler<DialogCompletedEventArgs>(this.StartDtmfMenuDialog));                                                 
            }
            catch (InvalidOperationException exp)
            {
                this.EndService(exp);
            }
        }

        void RosterParticipantCountChanged(object sender, ParticipantCountChangedEventArgs e)
        {
            if (e.CurrentCount > 1)
            {
                // No music should play if there are more than one participants. It could be off already. Just to be sure, do it here.
                this.StopMusic(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
            }
            else 
            {
                // No one exists or just one person left. If customer is still there, we need to end this so that we can start main flow.
                this.EndService(null);
            }
        }

        /// <summary>
        /// Start Dtmf menu dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void StartDtmfMenuDialog(object sender, DialogCompletedEventArgs e)
        {
            this.StartDtmfMenuDialog();
        }

        public void StartDtmfMenuDialog()
        {
            try
            {              
                //Start dtmf menu dialog.             
                DtmfMenuDialog dtmfMenuDialog = new DtmfMenuDialog(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall, m_configuration,this.Logger);
                dtmfMenuDialog.Completed += new EventHandler<DialogCompletedEventArgs>(this.DtmfDialogCompleted);
                dtmfMenuDialog.Run();
            }
            catch (InvalidOperationException exp)
            {
                this.EndService(exp);
            }
        }

        /// <summary>
        /// DtmfDialog completed handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtmfDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            DtmfMenuDialog.SelectedMenuItem selectedMenuItem = (DtmfMenuDialog.SelectedMenuItem)e.Output["SelectedMenuItem"];
            switch (selectedMenuItem)
            {
                case DtmfMenuDialog.SelectedMenuItem.None:
                    this.RestartDtmfCommand();
                    break;
                case DtmfMenuDialog.SelectedMenuItem.AddNumber:
                    this.AddNumberToConference();
                    break;
                case DtmfMenuDialog.SelectedMenuItem.AddContact:
                    this.AddNewContactToConference();
                    break;
               //Added case of dialog is failed, do nothing.
                case DtmfMenuDialog.SelectedMenuItem.Failed:                   
                    break;
                default:
                    Debug.Fail("Unexpected state");
                    break;
            }
        }

        private void AddNumberToConference()
        {
            try
            {
                this.BeginRemoveCustomerFromDefaultRouting(
                    ar =>
                    {
                        try
                        {
                            this.EndRemoveCustomerFromDefaultRouting(ar);
                            this.StartDialupDialog();
                        }
                        catch (RealTimeException rte)
                        {
                            this.Logger.Log(Logger.LogLevel.Error,rte);
                            this.RestartDtmfCommand();
                        }
                    }, null);
            }
            catch (InvalidOperationException ioe)
            {
                this.Logger.Log(Logger.LogLevel.Error,ioe);
                this.RestartDtmfCommand();
            }
        }

        private void StartDialupDialog()
        {
            if (!this.CustomerSession.IsTerminatingTerminated)
            {
                var configuration = ApplicationConfiguration.GetDialupConfiguration();

                try
                {
                    //Start Dialup dialog which gets the number user wants to dial.
                    DialupDialog dialupDialog = new DialupDialog(CustomerSession.CustomerServiceChannel.ServiceChannelCall, configuration);
                    dialupDialog.Completed += new EventHandler<DialogCompletedEventArgs>(this.DialupDialogCompleted);
                    dialupDialog.Run();
                }
                catch (InvalidOperationException exp)
                {
                    this.EndService(exp);
                }
            }
        }


        /// <summary>
        /// Dialup dialog completion event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DialupDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            this.ReturnCustomerToDefaultRouting();
            string number = e.Output.ContainsKey("Number") ? e.Output["Number"] as string : string.Empty;
            if (!string.IsNullOrEmpty(number))
            {
                string telUri = string.Format(CultureInfo.InvariantCulture, "tel:+{0}", number);
                this.DialOut(telUri, telUri, telUri, this.UserInvitationWorkCompleted);
            }            
            else
            {
                this.StartDtmfMenuDialog();
            }   

        }

        private void DialOut(string destinationUri, string rosterUri, string displayName, CompletionDelegate completionDelegate)
        {
            try
            {
                var avmcuSession = this.CustomerSession.CustomerConversation.ConferenceSession.AudioVideoMcuSession;
                AudioVideoMcuDialOutOptions options = new AudioVideoMcuDialOutOptions();
                options.PrivateAssistantDisabled = true;
                options.ParticipantUri = rosterUri; // uri that is shown in Roster.
                options.ParticipantDisplayName = displayName;
                bool stopMusic = false;
                if (this.CustomerSession.RosterTrackingService.ParticipantCount == 1)
                {
                    this.StartMusic(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
                    stopMusic = true;
                }

                avmcuSession.BeginDialOut(
                    destinationUri, // Uri to send the dial out to.
                    options,
                    ar =>
                    {
                        try
                        {
                            if (stopMusic)
                            {
                                this.StopMusic(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
                            }
                            avmcuSession.EndDialOut(ar);
                            completionDelegate(null);
                        }
                        catch (RealTimeException rte)
                        {
                            completionDelegate(rte);
                        }
                    }, 
                    null);
            }
            catch (InvalidOperationException ioe)
            {
                completionDelegate(ioe);
            }
        }

        private void UserInvitationWorkCompleted(Exception exception)
        {
            Helpers.DetachFlowFromAllDevices(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
            if (exception != null)
            {
                Helpers.PlayCallWasDeclined(
                    this.CustomerSession.CustomerServiceChannel.ServiceChannelCall,              
                 new EventHandler<DialogCompletedEventArgs> (this.StartDtmfMenuDialog)); //Replaced workflow completed delegate with dialog completed delegate
            }
            else
            {
                this.StartDtmfMenuDialog();
            }
        }

        public void RestartDtmfCommand()
        {
            this.ReturnCustomerToDefaultRouting(); // Esnure that customer is in conference
            this.StartDtmfMenuDialog();
        }

        private void AddNewContactToConference()
        {
            try
            {
                this.BeginRemoveCustomerFromDefaultRouting(
                    ar =>
                    {
                        try
                        {
                            this.EndRemoveCustomerFromDefaultRouting(ar);
                            this.StartGetContactDialog();
                        }
                        catch (RealTimeException rte)
                        {
                            this.Logger.Log(Logger.LogLevel.Error,rte);
                            this.RestartDtmfCommand();
                        }
                    }, null);

            }
            catch(InvalidOperationException ioe)
            {
                this.Logger.Log(Logger.LogLevel.Error,ioe);
                this.RestartDtmfCommand();
            }

        }

        private IAsyncResult BeginRemoveCustomerFromDefaultRouting(
            AsyncCallback userCallback,
            object state)
        {
            //First isolate the customer so that any interaction they go through
            //to select the new user is not heard by other users in the conference.
            var serviceHub = this.CustomerSession.ServiceHub;
            
            var asyncResult =
                serviceHub.BeginRemoveCustomerFromDefaultRouting(
                c_OutOfDefaultRoutingDurationSecs,
                userCallback,
                state);

            return asyncResult;
        }

        private void EndRemoveCustomerFromDefaultRouting(IAsyncResult result)
        {
            var serviceHub = this.CustomerSession.ServiceHub;
            serviceHub.EndRemoveCustomerFromDefaultRouting(result);
        }

        private void StartGetContactDialog()
        {
            if (this.CustomerSession.IsTerminatingTerminated)
            {
                this.EndService(null);
                return;
            }
            try
            {  

                //AStart get buddy dialog which gets the contact name the user wants to contact with
                GetBuddyDialog getcontactDialog = new GetBuddyDialog(CustomerSession, ApplicationConfiguration.GetBuddyConfiguration());
                getcontactDialog.Completed += new EventHandler<DialogCompletedEventArgs>(this.GetContactDialogCompleted);
                getcontactDialog.Run();
            }
            catch (InvalidOperationException exp)
            {
                this.EndService(exp);
            }
        }


        /// <summary>
        /// Get buddy dialog completion handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetContactDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            ContactInformation selectedContact;
            selectedContact = e.Output["selectedContact"] as ContactInformation;
            m_selectedContact = null;

            if (this.CustomerSession.IsTerminatingTerminated)
            {
                this.EndService(null);
                return;
            }
            if (selectedContact == null)
            {
                //No contact was selected, return the customer
                //back to default routing.
                this.RestartDtmfCommand();
            }
            else
            {
                m_selectedContact = selectedContact;

                var statement = m_configuration.InviteUserStatement;
                string mainPrompt =
                    string.Format(
                    CultureInfo.InvariantCulture,
                    statement.MainPrompt,
                    selectedContact.DisplayName);

                this.StartSimpleDialog(mainPrompt, new EventHandler<DialogCompletedEventArgs>(this.InviteUserStatementDialogCompleted));


            }
        }

        /// <summary>
        ///  Invite user dialog completion handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InviteUserStatementDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            this.InviteUserStatementCompleted();
        }

        private void InviteUserStatementCompleted()
        {
            if (this.CustomerSession.IsTerminatingTerminated)
            {
                this.EndService(null);
                return;
            }
            this.RestartDtmfCommand();

            RealTimeAddress address = new RealTimeAddress(m_selectedContact.Uri);
            if (address.IsPhone)
            {
                this.DialOut(m_selectedContact.Uri, m_selectedContact.Uri, m_selectedContact.Uri, this.UserInvitationWorkCompleted);
            }
            else
            {
                this.InviteToConference(m_selectedContact.Uri, this.UserInvitationWorkCompleted); // Conf          
            }
        }

        private void InviteToConference(string uri, CompletionDelegate completionDelegate)
        {
            Debug.Assert(!string.IsNullOrEmpty(uri), "New user could not be null.");

            ConferenceInvitationSettings convSettings = new ConferenceInvitationSettings();
            convSettings.AvailableMediaTypes.Add(MediaType.Audio);
            var confInvitation = new ConferenceInvitation(this.CustomerSession.CustomerConversation, convSettings);

            try
            {
                if (this.CustomerSession.RosterTrackingService.ParticipantCount == 1)
                {
                    this.StartMusic(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
                }
                confInvitation.BeginDeliver(
                    uri,
                    ar =>
                    {
                        try
                        {
                            confInvitation.EndDeliver(ar);
                            completionDelegate(null);
                        }
                        catch (RealTimeException rte)
                        {
                            this.StopMusic(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
                            completionDelegate(rte);
                        }
                    }, null);
            }
            catch (InvalidOperationException ioe)
            {
                this.Logger.Log(Logger.LogLevel.Error,ioe);                
                this.StopMusic(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
                completionDelegate(ioe);
            }
        }

        
        /// <summary>
        ///Start simple dialog which only speaks a statement and dialog completion event is handled by passed event handler.
        /// </summary>
        /// <param name="mainPrompt"></param>
        /// <param name="dialogCompleted"></param>
        private void StartSimpleDialog(string mainPrompt,EventHandler<DialogCompletedEventArgs> dialogCompleted)
        {
            try
            {               
             
                SimpleStatementDialog simpleStatDialog = new SimpleStatementDialog(mainPrompt, this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
                simpleStatDialog.Completed += dialogCompleted;
                simpleStatDialog.Run();
            }
            catch (InvalidOperationException exp)
            {
                this.EndService(exp);
            }
        }
        
        private void ReturnCustomerToDefaultRouting()
        {
            var serviceHub = this.CustomerSession.ServiceHub;

            try
            {
                serviceHub.BeginAddCustomerToDefaultRouting(
                    ar =>
                    {
                        try
                        {
                            serviceHub.EndAddCustomerToDefaultRouting(ar);
                        }
                        catch (RealTimeException rte)
                        {
                            this.Logger.Log(Logger.LogLevel.Error,rte);
                        }
                    }, null);
            }
            catch (InvalidOperationException ioe)
            {
                this.Logger.Log(Logger.LogLevel.Error,ioe);
                //TODO:Handle failure to put customer back into default routing.
            }
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