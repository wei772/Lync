/*=====================================================================
  File:      DialupService.cs

  Summary:   Represents the a service to dial up numbers.

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
using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration;
using System.Timers;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
using VoiceCompanion.DialupDialog;
using VoiceCompanion.SimpleStatementDialog;
using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.VoiceServices
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Calling Begin/EndShutdown will clean up. This pattern is preferred, among other things, because the shutdown operation is asynchronous.")]
    class DialupService : VoiceService
    {
        private DialupConfiguration m_configuration;

        private Timer m_timer;

        private int c_intervalMins = 1;

        private int m_count = 1;

        private bool m_enableConferenceService = true; // Set to false to hear time elapses every c_intervalMins.

        public DialupService(CustomerSession customerSession)
            : base(customerSession)
        {
        }

        public override string Id
        {
            get { return "95C8AAE9-427F-4580-A49D-A7FA814676CB"; }
        }

        /// <summary>
        /// Controls whether the customer should hear elapsed minutes or get back to conference service mode.
        /// </summary>
        /// <remarks>True indicates that dialout service should finish so that conference service can be started while the two parties 
        /// are in communication. This allows the customer to call more parties into the conference.</remarks>
        public bool EnableConferenceServiceAfterDialout
        {
            get
            {
                return m_enableConferenceService;
            }
            set
            {
                m_enableConferenceService = value;
            }
        }
        protected override void StartupCore()
        {
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.FailureCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.SuccessCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.AddTask(new AsyncTask(this.StartupDialupDialog));
            sequence.Start();
        }

        private void StartupDialupDialog(AsyncTask task, object state)
        {
            task.DoFinalStep(
                delegate()
                {
                    m_configuration = ApplicationConfiguration.GetDialupConfiguration();                  

                    //Start Dialup dialog to get the number user wants to dial.
                    DialupDialog dialupDialog = new DialupDialog(CustomerSession.CustomerServiceChannel.ServiceChannelCall, m_configuration);
                    dialupDialog.Completed += new EventHandler<DialogCompletedEventArgs>(this.DialupDialogCompleted);
                    dialupDialog.Run();
                });
        }

        protected override void ShutdownCore()
        {
            this.CompleteShutdown();
        }
   

        /// <summary>
        /// Dialup dialog complettion event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DialupDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            string number = e.Output.ContainsKey("Number") ? e.Output["Number"] as string : string.Empty;
            if (string.IsNullOrEmpty(number))
            {
                this.BeginShutdown(ar => this.EndShutdown(ar), null);
                return;
            }

            string telUri = string.Format(CultureInfo.InvariantCulture, "tel:+{0}", number);
            System.Threading.ThreadPool.QueueUserWorkItem(this.Dialup, telUri);

        }
        private void StartTimer()
        {
            m_timer = new Timer(c_intervalMins * 60 * 1000);
            m_timer.AutoReset = true;
            m_timer.Elapsed += this.TimerElapsed;

            m_timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (this.CustomerSession.RosterTrackingService.ParticipantCount <= 0)
            {
                this.EndService(null); // Only customer is there or everyone has left.
            }
            try
            {

                string mainPrompt = string.Format(CultureInfo.InvariantCulture, "{0} minute", c_intervalMins * m_count++);

                //Start simple dialog which only speaks a  given statement.
                SimpleStatementDialog simpleDialog = new SimpleStatementDialog(mainPrompt, this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
                simpleDialog.Completed += new EventHandler<DialogCompletedEventArgs>(this.TimeOutDialogCompleted);
                simpleDialog.Run();
            }
            catch (InvalidOperationException exp)
            {
                this.EndService(exp);
            }
        }

        /// <summary>
        /// Dialog completion event handler for time out statement dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeOutDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            //No code is provided as nothing has to do after completion of this.
        }

        private void DialoutCompleted(Exception exception)
        {
            this.StopMusic(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
            if (exception != null)
            {
                bool userDeclined = false;
                ConferenceFailureException cfe = exception as ConferenceFailureException;
                if (cfe != null)
                {
                    userDeclined = HasUserDeclined(cfe.Reason);
                }
                if (userDeclined)
                {
                    this.StartDialOutFailureDialog(true);
                }
                else
                {
                    this.StartDialOutFailureDialog(false);
                }
            }
            else if (this.EnableConferenceServiceAfterDialout)
            {
                this.EndService(null);
            }
            else
            {
                // Timer Announcement needed.
                this.StartTimer();
            }
        }

        private void Dialup(object state)
        {
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.SuccessCompletionReportHandlerDelegate = this.DialoutCompleted;
            sequence.FailureCompletionReportHandlerDelegate = this.DialoutCompleted;
            AsyncTask dialupAction = new AsyncTask(this.DialupNumber, state);
            sequence.AddTask(dialupAction);
            AsyncTask waitAction = new AsyncTask(this.CustomerSession.RosterTrackingService.StartupWaitForNewParticipant, this.CustomerSession.RosterTrackingService.ParticipantCount);
            sequence.AddTask(waitAction);
            sequence.Start();
        }

        private void DialupNumber(AsyncTask task, object state)
        {
            string number = (string)state;
            task.DoOneStep(
                delegate()
                {
                    this.StartMusic(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
                    var avmcuSession = this.CustomerSession.CustomerConversation.ConferenceSession.AudioVideoMcuSession;
                    avmcuSession.BeginDialOut(
                        number,
                        ar =>
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    avmcuSession.EndDialOut(ar);
                                });
                        },
                        null);
                });
        }

        private static bool HasUserDeclined(string reason)
        {
            if (reason.Equals(CommandFailureReasons.UserDeclined, StringComparison.OrdinalIgnoreCase) ||
                reason.Equals(CommandFailureReasons.UserDenied, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void StartDialOutFailureDialog(bool didUserDecline)
        {
            try
            {

                string mainPrompt;
                if (didUserDecline)
                {
                    mainPrompt = m_configuration.UserDeclinedStatement.MainPrompt;
                }
                else
                {
                    mainPrompt = m_configuration.DialupFailedStatement.MainPrompt;
                }

                this.Logger.Log(Logger.LogLevel.Info, "DialupService: Dialup was unsuccessful. Preparing statement for customer.");

                //Start simple dialog which only speaks a given statement.
                SimpleStatementDialog simpleDialog = new SimpleStatementDialog(mainPrompt, this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
                simpleDialog.Completed += new EventHandler<DialogCompletedEventArgs>(this.DeclinedDialogCompleted);
                simpleDialog.Run();

            }
            catch (InvalidOperationException exp)
            {
                this.EndService(exp);
            }
        }

  
        /// <summary>
        /// Dialog completion handler of user declined call speaking dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeclinedDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            this.EndService(null);
        }

        private void EndService(Exception e)
        {
            if (e != null)
            {
                this.Logger.Log(Logger.LogLevel.Error, string.Format(CultureInfo.InvariantCulture, "Service {0} encountered {1}", this.Id, e.ToString()));
            }

            this.StopMusic(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
            Helpers.DetachFlowFromAllDevices(this.CustomerSession.CustomerServiceChannel.ServiceChannelCall);
            Timer timer = m_timer;
            m_timer = null;
            if (timer != null)
            {
                timer.Stop();
            }

            this.BeginShutdown(ar => this.EndShutdown(ar), null);
        }
    }
}
