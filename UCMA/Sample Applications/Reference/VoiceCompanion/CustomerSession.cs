/*=====================================================================
  File:    CustomerSession.cs  

  Summary:  Represents a customer interacting with the system. 

***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.Utilities;
using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.VoiceServices;
using Microsoft.Rtc.Signaling;
using System.Linq;
using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
using VoiceCompanion.AuthenticationDialog;
using VoiceCompanion.MainMenu;
using VoiceCompanion.SimpleStatementDialog;
using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{

    #region CustomerSession

    public class CustomerSession : ComponentBase
    {
        #region CustomerSession mode

        private enum Mode
        {
            /// <summary>
            /// Session created based on an incoming customer call
            /// </summary>
            IncomingSession,

            /// <summary>
            /// Session created based on initiating a callback to the customer.
            /// </summary>
            CallbackSession
        }


        #endregion

        #region Private fields

        private readonly AppFrontEnd m_appFrontEnd;
        private Customer m_customer;
        private ServiceHub m_serviceHub;
        private ServiceChannel m_customerServiceChannel;
        private MyUserEndpoint m_customerEndpoint;
        private AudioVideoCall m_customerCall;
        private Conversation m_customerConversation;
        private CustomerContactManager m_contactManager;
        private readonly Mode m_mode;
        private RosterTrackingService m_customerTracker;
        private string m_callbackTargetUri;
        private string m_callbackTargetDisplayName;
        private ConferenceService m_conferenceService;
        private bool m_customerTransfered;
        private AsyncTask m_lookAheadTask;
        #endregion
               
        #region Constructors

        /// <summary>
        /// Customer session to handle a new incoming call from a user.
        /// </summary>
        /// <param name="appFrontEnd">The application front end</param>
        /// <param name="customerCall"></param>
        internal CustomerSession(AppFrontEnd appFrontEnd, AudioVideoCall customerCall)
            :base(appFrontEnd.AppPlatform)
        {
            m_mode = Mode.IncomingSession;
            m_appFrontEnd = appFrontEnd;
            m_customerCall = customerCall;
            m_customer = new Customer();
            this.InitializeFields();
        }

        internal CustomerSession(
            AppFrontEnd appFrontEnd, 
            Customer customer, 
            string callbackTargetUri,
            string callbackTargetDisplayName)
            : base(appFrontEnd.AppPlatform)
        {
            m_mode = Mode.CallbackSession;
            m_appFrontEnd = appFrontEnd;
            m_customer = customer;
            m_callbackTargetDisplayName = callbackTargetDisplayName;
            m_callbackTargetUri = callbackTargetUri;
            this.InitializeFields();
        }

        private void InitializeFields()
        {
            m_customerTracker = new RosterTrackingService(this.AppFrontEnd, new TimeSpan(0, 0, 20));
            m_serviceHub = new ServiceHub(this, m_customerTracker);
            m_customerServiceChannel = new ServiceChannel(m_serviceHub, m_customerTracker);
            m_customerTracker.ParticipantCountChanged += RosterParticipantCountChanged;
        }

        #endregion Constructors

        public AudioVideoCall AudioVideoCall
        {
            get
            {
                return m_customerCall;
            }
        }

        public Customer Customer
        {
            get
            {
                return m_customer;
            }
        }

        public UserEndpoint CustomerEndpoint
        {
            get
            {
                return m_customerEndpoint.UserEndpoint;
            }
        }

        public ServiceHub ServiceHub
        {
            get
            {
                return m_serviceHub;
            }
        }

        public ServiceChannel CustomerServiceChannel
        {
            get
            {
                return m_customerServiceChannel;
            }
        }

        public RosterTrackingService RosterTrackingService
        {
            get
            {
                return m_customerTracker;
            }
        }

        public CustomerContactManager ContactManager
        {
            get
            {
                return m_contactManager;
            }
        }

        public MusicProvider MusicOnHoldProvider
        {
            get
            {
                return m_appFrontEnd.MusicOnHoldProvider;
            }
        }

        public Conversation CustomerConversation
        {
            get
            {
                return m_customerConversation;
            }
        }

        internal AppFrontEnd AppFrontEnd
        {
            get
            {
                return m_appFrontEnd;
            }
        }

        public override void CompleteStartup(Exception exception)
        {
            try
            {
                base.CompleteStartup(exception);
                if (exception != null)
                {
                    this.Logger.Log(Logger.LogLevel.Info, string.Format(CultureInfo.InvariantCulture, "CustomerSession failed to start for {0}", m_customer.DisplayName), exception);
                }
                else
                {
                    this.Logger.Log(Logger.LogLevel.Info, string.Format(CultureInfo.InvariantCulture, "CustomerSession successfully started for {0}", m_customer.DisplayName));
                }
            }
            finally
            {
                if (exception != null)
                {
                    this.Shutdown();
                }
            }
        }

        public override void CompleteShutdown()
        {
            base.CompleteShutdown();
            this.Logger.Log(Logger.LogLevel.Info, string.Format(CultureInfo.InvariantCulture, "CustomerSession for {0} has been shutdown.", m_customer.DisplayName));
            m_appFrontEnd.ReportSessionShutdown(this);
        }


        protected override void StartupCore()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(HandleStartup);     
        }

        private void RosterParticipantCountChanged(object sender, ParticipantCountChangedEventArgs e)
        {
            if (!this.RosterTrackingService.IsTargetInRoster)
            {
                this.Shutdown();
            }
        }

        private void HandleStartup(object state)
        {
            if (m_mode == Mode.IncomingSession)
            {
                this.HandleIncomingCall();
            }
            else
            {
                this.HandleCallback();
            }
        }

        protected override void ShutdownCore()
        {
            Helpers.DetachFlowFromAllDevices(m_customerServiceChannel.ServiceChannelCall);
            AsyncTaskSequenceSerial shutdownActions = new AsyncTaskSequenceSerial(this);
            shutdownActions.Name = "ShutdownCustomerSession";
            shutdownActions.AddTask(new AsyncTask(this.ShutdownCustomerCall));
            shutdownActions.AddTask(new AsyncTask(this.ShutdownConferenceService));
            shutdownActions.AddTask(new AsyncTask(this.ShutdownContactManager));
            shutdownActions.AddTask(new AsyncTask(this.ShutdownServiceChannel));
            shutdownActions.AddTask(new AsyncTask(this.ShutdownServiceHub));
            shutdownActions.AddTask(new AsyncTask(this.RemoveInACallPresence));
            shutdownActions.AddTask(new AsyncTask(this.ShutdownUserEndpoint));
            shutdownActions.Start();
        }


        private void HandleIncomingCall()
        {                        
            m_customerCall.StateChanged += this.CustomerCallStateChanged;
            
            ConversationParticipant caller = m_customerCall.RemoteEndpoint.Participant;
            // Default both corporate phone uri and callback uri to what we learn from the incoming call for now.
            // We should prompt the user for these two values and override.
            m_customer.PhoneUri = caller.PhoneUri;
            if (String.IsNullOrEmpty(m_customer.PhoneUri))
            {
                m_customer.PhoneUri = caller.OtherPhoneUri;
            }
            m_customer.CallbackPhoneUri = caller.OtherPhoneUri;
            if (String.IsNullOrEmpty(m_customer.CallbackPhoneUri))
            {
                m_customer.CallbackPhoneUri = caller.PhoneUri;
            }
            m_customer.Uri = caller.Uri;
            RealTimeAddress uriAddress = new RealTimeAddress(caller.Uri);
            m_customer.IsUriPhone = uriAddress.IsPhone;
            if (!uriAddress.IsPhone && m_customerCall.RemoteEndpoint.IsParticipantIdAsserted)
            {
                m_customer.UserUri = caller.Uri; // This could be the case if we got directly from Microsoft Lync.
            }
            else
            {
                string phone;
                if (Helpers.TryExtractCleanPhone(caller.Uri, out phone))
                {
                    m_customer.CleanNumber = phone;
                    this.Logger.Log(Logger.LogLevel.Info, string.Format(CultureInfo.InvariantCulture, "Handling call from {0}", m_customer.CleanNumber));
                    // Look up the user and wait for the look up to complete.
                    AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
                    sequence.Name = "RNLLookup";
                    sequence.SuccessCompletionReportHandlerDelegate = this.ReverseLookupCompleted;
                    sequence.FailureCompletionReportHandlerDelegate = this.ReverseLookupCompleted;
                    sequence.AddTask(new AsyncTask( this.ReverseLookup));
                    sequence.Start();
                    return; // Wait for ReverseLookup to complete.
                }
            }
            this.ResumeHandleIncomingCall();
        }

        private void ReverseLookupCompleted(Exception exception)
        {
            if (exception != null)
            {
                this.Logger.Log(Logger.LogLevel.Info, "RNL Look up failed for customer " + m_customer.PhoneUri ?? "null");
                this.CompleteStartup(exception); // Failed to to RNL.
            }
            else
            {
                this.Logger.Log(Logger.LogLevel.Info, 
                        String.Format("RNL Look up completed for customer. User Uri = {0} Phone Uri = {1}", 
                        m_customer.UserUri??"null", m_customer.PhoneUri??"null"));
                this.ResumeHandleIncomingCall();
            }
        }

        private void ResumeHandleIncomingCall()
        {
            ConversationParticipant caller = m_customerCall.RemoteEndpoint.Participant;

            if (String.IsNullOrEmpty(m_customer.UserUri))
            {
                // We can't determine the user uri of the caller. Let us simply reject it.
                // One could start a different workflow to ask user to enter phone number manually. This sample does not do this.
                this.RejectCall();
            }
            else
            {
                SipUriParser userUriParser = new SipUriParser(m_customer.UserUri);
                m_customer.DisplayName = userUriParser.User; // Initialize. Will be overrriden by UserEndpoint after establishment.
                m_customerTracker.TargetUri = m_customer.UserUri;
                AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
                sequence.Name = "HandleCustomerCall";
                sequence.FailureCompletionReportHandlerDelegate = this.CompleteStartup;
                // We do not want to complete on success. We need to wait until pin auth completes.
                //sequence.AddTask(new AsyncTask(this.AcceptCustomerCall));
                sequence.AddTask(new AsyncTask( this.StartupInvokeAuthenticationDialog));
                
                // Before starting the sequence, we can start other operations that are needed if the authentication succeeds.
                AsyncTaskSequenceSerial lookAheadsTaskSequence = new AsyncTaskSequenceSerial(this);
                lookAheadsTaskSequence.AddTask(new AsyncTask(this.StartupUserEndpoint));
                lookAheadsTaskSequence.AddTask(new AsyncTask(this.StartupServiceHub));
                lookAheadsTaskSequence.AddTask(new AsyncTask(this.StartupServiceChannel));
                lookAheadsTaskSequence.AddTask(new AsyncTask(this.StartupCustomerConferenceJoin));
                m_lookAheadTask = new AsyncTask(AsyncTask.SequenceStartingMethod, lookAheadsTaskSequence);
                lookAheadsTaskSequence.FailureCompletionReportHandlerDelegate = m_lookAheadTask.Complete;
                lookAheadsTaskSequence.SuccessCompletionReportHandlerDelegate = m_lookAheadTask.Complete;
                m_lookAheadTask.StartTask(); 

                sequence.Start();
            }
        }

        public void AcceptCustomerCall(AsyncTask task, object state)
        {
            if (m_customerCall == null || m_customerCall.State != CallState.Incoming)
            {
                task.Complete(new InvalidOperationException("Customer call is missing or no longer in incoming state."));
                return;
            }
            task.DoOneStep(
                delegate()
                {
                    m_customerCall.BeginAccept(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                               delegate()
                               {
                                   m_customerCall.EndAccept(ar);
                               });
                        },
                        null);
                });
        }

        public void StartupCustomerTransfer(AsyncTask task, object state)
        {
            if (m_serviceHub == null || m_serviceHub.Conversation == null || m_serviceHub.Conversation.ConferenceSession == null)
            {
                task.Complete(new InvalidOperationException("Cannot transfer customer call without conference in service hub."));
            }
            task.DoOneStep(
                delegate()
                {
                    AudioVideoMcuSession mcuSession = m_serviceHub.Conversation.ConferenceSession.AudioVideoMcuSession;
                    McuTransferOptions options = new McuTransferOptions();
                    options.ParticipantUri = m_customer.UserUri;
                    options.ParticipantDisplayName = m_customer.DisplayName;
                    m_customerTransfered = true;
                    mcuSession.BeginTransfer(m_customerCall, options,
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    mcuSession.EndTransfer(ar);
                                });
                        },
                        null);
                });
        }

        public void ShutdownCustomerCall(AsyncTask task, object state)
        {
            if (m_customerCall != null)
            {
                task.DoOneStep(
                    delegate()
                    {
                        Logger.Log(Logger.LogLevel.Info, "Terminating customer call.");
                        m_customerCall.BeginTerminate(
                            delegate(IAsyncResult ar)
                            {
                                task.DoFinalStep(
                                    delegate()
                                    {
                                        m_customerCall.EndTerminate(ar);
                                        Logger.Log(Logger.LogLevel.Info, "Terminated customer call.");
                                    });
                            },
                            null);
                    });
            }
            else
            {
                // Eject user from the conference, if present
                ConversationParticipant customerParticipant = null;
                if (m_customerTracker != null && m_customerTracker.TargetUriEndpoint != null)
                {
                    customerParticipant = m_customerTracker.TargetUriEndpoint.Participant;
                }                
                if (customerParticipant != null && m_customerTracker.IsTargetInRoster && 
                     m_serviceHub != null && m_serviceHub.Conversation != null)
                {
                    task.DoOneStep(
                        delegate()
                        {
                            ConferenceSession confSession = m_serviceHub.Conversation.ConferenceSession;
                            confSession.BeginEject(customerParticipant,
                                delegate(IAsyncResult ar)
                                {
                                    task.DoFinalStep(
                                        delegate()
                                        {
                                            confSession.EndEject(ar);
                                        });
                                },
                                null);
                        });

                }
                else
                {
                    task.Complete(null);
                }
            }
        }

        /// <summary>
        /// Looks up the customer number to find the matching user uri. 
        /// </summary>
        /// <returns>True if user was found. False, otherwise.</returns>
        private void ReverseLookup(AsyncTask task, object state)
        {
            Exception exception = null;
            try
            {
                var reverseLookup = m_appFrontEnd.AppPlatform.ReverseNumberLookUp;

                reverseLookup.BeginLookup(
                    m_customer.CleanNumber,
                    ar =>
                    {
                        Exception ex = null;
                        try
                        {
                            var result = reverseLookup.EndLookup(ar);
                            if (result.WasNumberFound)
                            {
                                m_customer.UserUri = result.Uri;
                            }
                        }
                        catch (LookupFailureException e)
                        {
                            ex = e;
                        }
                        finally
                        {
                            task.Complete(ex);
                        }
                    }, null);
            }
            catch (InvalidOperationException e)
            {
                exception = e;
            }
            finally
            {
                if (exception != null)
                {
                    task.Complete(exception);
                }
            }
        }

        private void RejectCall()
        {
            this.Logger.Log(Logger.LogLevel.Info, string.Format(CultureInfo.InvariantCulture, "Rejecting the call."));

            try
            {
                m_customerCall.Decline();
            }
            catch (InvalidOperationException ioe)
            {
                this.Logger.Log(Logger.LogLevel.Error,ioe);
            }
            catch (RealTimeException rte)
            {
                this.Logger.Log(Logger.LogLevel.Error,rte);
            }
            finally
            {
                this.CompleteStartup(null);
            }
        }

        private void StartupServiceChannel(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    //Logger.Log(Logger.LogLevel.Info, "Starting customer service channel(TCU).");

                    m_customerServiceChannel.BeginStartup(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_customerServiceChannel.EndStartup(ar);
                                    //Logger.Log(Logger.LogLevel.Info, "Started customer service channel(TCU).");
                                });
                        }, 
                        null);
                });
        }

        private void ShutdownServiceChannel(AsyncTask task, object state)
        {
            if (m_customerServiceChannel == null)
            {
                task.Complete(null);
                return;
            }
            task.DoOneStep(
                delegate()
                {
                    //Logger.Log(Logger.LogLevel.Info, "Terminating the authenitcation service channel(TCU).");

                    m_customerServiceChannel.BeginShutdown(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_customerServiceChannel.EndShutdown(ar);
                                    //Logger.Log(Logger.LogLevel.Info, "Terminated the authentication service channel(TCU).");
                                });
                        },
                        null);
                });
        }

        private void StartupInvokeMainMenuDialog(AsyncTask task, object state)
        {
            Exception exception = null;
            try
            {
                this.Logger.Log(Logger.LogLevel.Info, "Main dialog detaching previous devices from call, if any.");
                Helpers.DetachFlowFromAllDevices(m_customerServiceChannel.ServiceChannelCall);

                //Start main menu dialog.    
                MainMenuDialog mainMenuDialog = new MainMenuDialog(ApplicationConfiguration.GetMainMenuConfiguration(), m_customerServiceChannel.ServiceChannelCall, this.Logger);
                mainMenuDialog.Completed += new EventHandler<DialogCompletedEventArgs>(this.MainMenuDialogCompleted);
                mainMenuDialog.Run();
                task.Complete(null);
             }
            catch (InvalidOperationException ivo)
            {
                exception = ivo;
            }
            finally
            {
                if (exception != null)
                {
                    task.Complete(exception);
                }
            }
        }

        private void StartupInvokeAuthenticationDialog(AsyncTask task, object state)
        {
            Exception exception = null;
            try
            {             

                //Start authentication dialog to get the Pin of user. 
                AuthenticationDialog authenticationDialog = new AuthenticationDialog(this, ApplicationConfiguration.GetAuthenticationConfiguration());
                authenticationDialog.Completed += new EventHandler<DialogCompletedEventArgs>(this.AuthenticationDialogCompleted);
                authenticationDialog.Run();
                task.Complete(null);
                Logger.Log(Logger.LogLevel.Info, "Started authentication dialog.");
            }
            catch (InvalidOperationException ivo)
            {
                exception = ivo;
            }
            finally
            {
                if (exception != null)
                {
                    task.Complete(exception);
                }
            }
        }

        private void MainMenuDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            string serviceId = e.Output.ContainsKey("ServiceId") ? e.Output["ServiceId"] as string : string.Empty;
            this.MainMenuCompleted(serviceId);
        }

        private void MainMenuCompleted(string selectedServiceId)
        {
            if (string.IsNullOrEmpty(selectedServiceId))
            {
                //If no service was selected, then terminate
                this.Shutdown();
            }
            else
            {
                // Use thread pool queue so that we can return back to the main work flow fast to let it shutdown.
                System.Threading.ThreadPool.QueueUserWorkItem(this.LoadVoiceService, selectedServiceId);
            }

        }

        private void AuthenticationDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            bool userWasAuthenticated = (e.Output.ContainsKey("UserWasAuthenticated")) ? (bool)e.Output["UserWasAuthenticated"] : false;
            AuthenticationCompleted(userWasAuthenticated);
        }

        private void AuthenticationCompleted(bool userWasAuthenticated)
        {
            if (userWasAuthenticated)
            {
                AsyncTaskSequenceSerial finalStartupActions = new AsyncTaskSequenceSerial(this);
                finalStartupActions.Name = "HandleCustomerCallAfterPin";
                finalStartupActions.SuccessCompletionReportHandlerDelegate = this.CompleteStartup;
                finalStartupActions.FailureCompletionReportHandlerDelegate = this.CompleteStartup;
                finalStartupActions.AddTask(new AsyncTask(this.WaitForLookAheadTasks));
                finalStartupActions.AddTask(new AsyncTask(this.StartupCustomerTransfer));
                AsyncTask trackingAction = new AsyncTask(m_customerTracker.StartupWaitForSpecificTarget, m_customer.UserUri);
                finalStartupActions.AddTask(trackingAction);
                ServiceChannel.InteractionMode interactiveMode =
                    ServiceChannel.InteractionMode.AnnounceSpeech | ServiceChannel.InteractionMode.ListenSpeechAndDtmf;
                AsyncTask updateInteractiveModeAction =
                    new AsyncTask(m_customerServiceChannel.UpdateInteractiveMode,
                                  new ArgumentTuple(true /* Add Route */, interactiveMode));
                finalStartupActions.AddTask(updateInteractiveModeAction);
                finalStartupActions.AddTask(new AsyncTask(this.PublishInACallPresence));
                finalStartupActions.AddTask(new AsyncTask(this.StartupContactManager));
                finalStartupActions.AddTask(new AsyncTask(this.StartupInvokeMainMenuDialog));
                finalStartupActions.Start();
            }
            else
            {
                //Terminate session.
                this.CompleteStartup(new VoiceCompanionException("Invalid pin"));
            }
        }

        private void WaitForLookAheadTasks(AsyncTask task, object state)
        {
            if (m_lookAheadTask == null)
            {
                task.Complete(new InvalidOperationException("Look ahead task is expected to have started. It is missing."));
                return;
            }

            m_lookAheadTask.TaskCompleted +=
                delegate(object sender, AsyncTaskCompletedEventArgs e)
                {
                    task.Complete(e.ActionResult.Exception);
                };

            if (m_lookAheadTask.IsCompleted)
            {
                // it is already complete. No need to wait anymore.
                task.Complete(m_lookAheadTask.Exception);
            }
        }

        private void StartupServiceHub(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    //Logger.Log(Logger.LogLevel.Info, "Starting service hub.");
                    m_serviceHub.BeginStartup(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_serviceHub.EndStartup(ar);
                                    //Logger.Log(Logger.LogLevel.Info, "Started service hub.");
                                });
                        }, null);
                });
        }

        private void ShutdownServiceHub(AsyncTask task, object state)
        {
            if (m_serviceHub == null)
            {
                task.Complete(null);
                return;
            }
            task.DoOneStep(
                delegate()
                {
                    Logger.Log(Logger.LogLevel.Info, "Terminating the service hub.");
                    m_serviceHub.BeginShutdown(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_serviceHub.EndShutdown(ar);
                                    Logger.Log(Logger.LogLevel.Info, "Terminated the service hub.");
                                });
                        }, null);
                });
        }

        private void StartConferenceService(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    ConferenceService service = new ConferenceService(this);
                    m_conferenceService = service;
                    service.BeginStartup(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    service.EndStartup(ar);
                                });
                        },
                        null);
                });
        }

        private void ShutdownConferenceService(AsyncTask task, object state)
        {
            ConferenceService service = m_conferenceService;
            if (service == null)
            {
                task.Complete(null);
                return;
            }
            task.DoOneStep(
                delegate()
                {
                    service.BeginShutdown(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    service.EndShutdown(ar);
                                });
                        },
                        null);
                });
        }

        private void StartupUserEndpoint(AsyncTask task, object state)
        {
            AsyncTask proxyTask = new AsyncTask(m_appFrontEnd.CreateOrGetUserEndpoint, m_customer.UserUri);
            proxyTask.TaskCompleted +=
                delegate(object sender, AsyncTaskCompletedEventArgs e)
                {
                    UserEndpointCreationActionResult result = proxyTask.TaskResult as UserEndpointCreationActionResult;
                    if (result != null)
                    {
                        m_customerEndpoint = result.MyUserEndpoint;
                    }
                    task.Complete(e.ActionResult.Exception);
                };
            proxyTask.StartTask();
        }


        private void ShutdownUserEndpoint(AsyncTask task, object state)
        {
            AsyncTask proxyTask = new AsyncTask(m_appFrontEnd.RelaseUserEndpoint, m_customerEndpoint);
            proxyTask.TaskCompleted +=
                delegate(object sender, AsyncTaskCompletedEventArgs e)
                {
                    task.Complete(e.ActionResult.Exception);
                };
            proxyTask.StartTask();
        }

        private void StartupContactManager(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    m_contactManager = new CustomerContactManager(this);
                    //Logger.Log(Logger.LogLevel.Info, "Starting contact manager.");
                    m_contactManager.BeginStartup(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_contactManager.EndStartup(ar);
                                   // Logger.Log(Logger.LogLevel.Info, "Started contact manager.");
                                });
                        }, 
                        null);
                });
        }

        private void ShutdownContactManager(AsyncTask task, object state)
        {
            if (m_contactManager == null)
            {
                task.Complete(null);
                return;
            }
            task.DoOneStep(
                delegate()
                {
                    Logger.Log(Logger.LogLevel.Info, "Terminating contact manager.");
                    m_contactManager.BeginShutdown(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_contactManager.EndShutdown(ar);
                                    Logger.Log(Logger.LogLevel.Info, "Terminated contact manager.");
                                });
                        },
                        null);
                });
        }

        private void PublishInACallPresence(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    var uri = m_customer.PhoneUri;
                    if (String.IsNullOrEmpty(uri))
                    {
                        uri = m_customer.UserUri;
                    }
                    if (String.IsNullOrEmpty(uri))
                    {
                        task.Complete(null);
                        return;
                    }
                    // If endpoint is enabled for automatic presence, then it is not needed to publish machine state explicitly.
                    // If not, the following commented line indicates how it is done.
                    var categories = new List<PresenceCategory>{ PresenceState.EndpointOnline };
                    categories.Add(PresenceState.PhoneInACall(uri));
                    //Logger.Log(Logger.LogLevel.Info, "Starting presence publishing for \"In a call\" state.");
                    this.CustomerEndpoint.LocalOwnerPresence.BeginPublishPresence(
                        categories,
                        delegate(IAsyncResult ar)                        
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    this.CustomerEndpoint.LocalOwnerPresence.EndPublishPresence(ar);
                                    //Logger.Log(Logger.LogLevel.Info, "Published \"In a call\" state.");
                                });
                        }, 
                        null);
                });
        }

        private void RemoveInACallPresence(AsyncTask task, object state)
        {
            UserEndpoint endpoint = this.CustomerEndpoint;
            if (endpoint == null || endpoint.State == LocalEndpointState.Terminating || endpoint.State == LocalEndpointState.Terminated)
            {
                task.Complete(null);
                return;
            }
            task.DoOneStep(
                delegate()
                {
                    var uri = m_customer.PhoneUri;
                    if (String.IsNullOrEmpty(uri))
                    {
                        uri = m_customer.UserUri;
                    }
                    if (String.IsNullOrEmpty(uri))
                    {
                        task.Complete(null);
                        return;
                    }
                    var categories = new List<PresenceCategory>{PresenceState.EndpointOnline};
                    categories.Add(PresenceState.PhoneInACall(uri));
                    //Logger.Log(Logger.LogLevel.Info, "Starting presence publishing for \"In a call\" state.");
                    this.CustomerEndpoint.LocalOwnerPresence.BeginDeletePresence(
                        categories,
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    this.CustomerEndpoint.LocalOwnerPresence.EndDeletePresence(ar);
                                    //Logger.Log(Logger.LogLevel.Info, "Published \"In a call\" state.");
                                });
                        },
                        null);
                });
        }

        internal void CallBackFailureDialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            this.Shutdown();
        }

        public void Shutdown()
        {
            this.BeginShutdown(ar => this.EndShutdown(ar), null);
        }

        private void HandleCallback()
        {
            Debug.Assert(m_mode == Mode.CallbackSession, "This is not the correct call mode");
            // Set the cusomer data on the tracker first.
            m_customerTracker.TargetUri = m_customer.UserUri; 

            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.Name = "HandleCallback";
            sequence.SuccessCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.FailureCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.AddTask(new AsyncTask(this.StartupUserEndpoint));
            sequence.AddTask(new AsyncTask(this.StartupServiceHub));
            sequence.AddTask(new AsyncTask(this.StartupContactManager));
            sequence.AddTask(new AsyncTask(this.StartupServiceChannel));
            sequence.AddTask(new AsyncTask(this.StartupCustomerConferenceJoin));
            ArgumentTuple args = new ArgumentTuple(m_customer.CallbackPhoneUri, m_customer.UserUri, m_customer.DisplayName);
            AsyncTask dialoutAction = new AsyncTask(this.DialOut, args);
            sequence.AddTask(dialoutAction);
            AsyncTask trackingAction = new AsyncTask(m_customerTracker.StartupWaitForSpecificTarget, m_customer.UserUri);
            sequence.AddTask(trackingAction);
            ServiceChannel.InteractionMode mode =
                ServiceChannel.InteractionMode.AnnounceSpeech | ServiceChannel.InteractionMode.ListenSpeechAndDtmf;
            AsyncTask updateInteractiveModeAction = 
                new AsyncTask(m_customerServiceChannel.UpdateInteractiveMode,
                                new ArgumentTuple(true /* Add Route */, mode));
            sequence.AddTask(updateInteractiveModeAction);
            // If endpoint was enabled for automatic presence publishing, it is already subscribed. No need to do the following step.
            // If automatic presence is not done, then the following code is needed before any publishing happens.
            //callbackSequence.AddAction(new Action(callbackSequence, this.StartupLocalOwnerPresenceSubscription));
            sequence.AddTask(new AsyncTask(this.PublishInACallPresence));
            sequence.AddTask(new AsyncTask(this.StartupCallbackGreetingDialog));
            sequence.Start();
        }
        
        private void StartupCustomerConferenceJoin(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    m_customerConversation = new Conversation(this.CustomerEndpoint);
                    //Logger.Log(Logger.LogLevel.Info, "Starting conference join for callback customer.");
                    m_customerConversation.ConferenceSession.BeginJoin(
                        m_serviceHub.ConferenceUri,
                        null,
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_customerConversation.ConferenceSession.EndJoin(ar);
                                    //Logger.Log(Logger.LogLevel.Info, "Started conference join for callback customer.");
                                });
                        }, 
                        null);
                });
        }

        private void StartupCallbackGreetingDialog(AsyncTask task, object state)
        {
            task.DoFinalStep(
                delegate()
                {
                   

                    var greetingConfig = ApplicationConfiguration.GetCallbackGreetingConfiguration();
                    string prompt = string.Format(CultureInfo.InvariantCulture, greetingConfig.GreetingStatement.MainPrompt, m_callbackTargetDisplayName);

                    //Start simple  dialog  which speaks a greeting for callback.  
                    SimpleStatementDialog simpleStatDialog = new SimpleStatementDialog(prompt, this.m_customerServiceChannel.ServiceChannelCall);
                    simpleStatDialog.Completed += new EventHandler<DialogCompletedEventArgs>(this.CallbackGreetingDialogCompleted);
                    simpleStatDialog.Run();
                    //Logger.Log(Logger.LogLevel.Info, "Started Greeting workflow for callback customer.");
                });
        }

        private void CallbackSequenceCompleted(Exception exception)
        {
            this.StopMusic();
            if (exception != null)
            {
                bool userDeclined = false;
                if (exception is FailureResponseException)
                {
                    userDeclined = true;
                }
                AsyncTask task = new AsyncTask(this.ShutdownConferenceService);
                task.TaskCompleted +=
                    delegate(object sender, AsyncTaskCompletedEventArgs e)
                    {
                        this.InvokeCallbackFailureDialog(userDeclined);
                    };
                task.StartTask();
            }
            else
            {
                // Load the conference service to provide conference services.
                m_conferenceService = new ConferenceService(this);
                AsyncTask task = new AsyncTask(this.StartConferenceService);
                task.StartTask();
            }
        }

        private void SendConferenceInvitation(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    var settings = new ConferenceInvitationSettings();
                    settings.AvailableMediaTypes.Add("audio");
                    settings.AvailableMediaTypes.Add("chat");
                    settings.ConferenceUri = m_serviceHub.ConferenceUri;

                    var confInv = new ConferenceInvitation(m_customerConversation, settings);
                    confInv.BeginDeliver(
                        m_callbackTargetUri,
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    confInv.EndDeliver(ar);
                                });
                        }, 
                        null);
                });
        }

        private void InvokeCallbackFailureDialog(bool userDeclined)
        {
            var dictionary = new Dictionary<string, object>();

            var greetingConfig = ApplicationConfiguration.GetCallbackGreetingConfiguration();

            string prompt = null;
            if (userDeclined)
            {
                prompt = string.Format(CultureInfo.InvariantCulture, greetingConfig.UserDeclined.MainPrompt, m_callbackTargetDisplayName);
            }
            else
            {
                prompt = string.Format(CultureInfo.InvariantCulture, greetingConfig.CannotReachUser.MainPrompt, m_callbackTargetDisplayName);
            }
            try
            {
                //Start simple dialog which speaks call back faiure message.             
                SimpleStatementDialog simpleStatDialog = new SimpleStatementDialog(prompt, this.m_customerServiceChannel.ServiceChannelCall);
                simpleStatDialog.Completed += new EventHandler<DialogCompletedEventArgs>(this.CallBackFailureDialogCompleted);
                simpleStatDialog.Run();
            }
            catch (InvalidOperationException)
            {
                this.Shutdown();
            }
        }

        internal void CallbackGreetingDialogCompleted(object sender, DialogCompletedEventArgs e)
        {          
            Helpers.DetachFlowFromAllDevices(m_customerServiceChannel.ServiceChannelCall);
            this.ConnectionCustomerWithContact();
        }

        private void ConnectionCustomerWithContact()
        {
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.Name = "ConnectCustomers";
            sequence.FailureCompletionReportHandlerDelegate = this.CallbackSequenceCompleted;
            sequence.SuccessCompletionReportHandlerDelegate = this.CallbackSequenceCompleted;
            sequence.AddTask(new AsyncTask(this.StartMusic));
            sequence.AddTask(new AsyncTask(this.SendConferenceInvitation));
            AsyncTask waitAction = new AsyncTask(this.RosterTrackingService.StartupWaitForSpecificTarget, m_callbackTargetUri);
            sequence.AddTask(waitAction);
            sequence.Start();
        }

        private void StartMusic(AsyncTask task, object state)
        {
            Debug.Assert(task != null);
            m_appFrontEnd.MusicOnHoldProvider.StartMusic(m_customerServiceChannel.ServiceChannelCall);
            task.Complete(null);
        }

        private void StopMusic(AsyncTask task, object state)
        {
            Debug.Assert(task != null);
            m_appFrontEnd.MusicOnHoldProvider.StopMusic(m_customerServiceChannel.ServiceChannelCall);
            task.Complete(null);
        }

        private void StartMusic()
        {
            m_appFrontEnd.MusicOnHoldProvider.StartMusic(m_customerServiceChannel.ServiceChannelCall);
        }

        private void StopMusic()
        {
            m_appFrontEnd.MusicOnHoldProvider.StopMusic(m_customerServiceChannel.ServiceChannelCall);
        }

        private void LoadVoiceService(object state)
        {
            string serviceId = (string)state;
            VoiceService voiceService = null;
            bool result = VoiceServiceFactory.TryGetVoiceServiceInstance(serviceId, this, out voiceService);          
            if (!result)
            {
                this.Logger.Log(Logger.LogLevel.Error, "Internal error occured. Cannot load service: " + serviceId);
                this.Shutdown();
                return;
            }

            try
            {
                voiceService.BeginStartup(
                    ar =>
                    {
                        voiceService.EndStartup(ar);
                    }, 
                    null);
            }
            catch (InvalidOperationException ioe)
            {
                this.Logger.Log(Logger.LogLevel.Error,ioe);
                this.Shutdown();
            }
        }

       
        internal void VoiceServiceCompleted(VoiceService service)
        {
            string message = string.Format(CultureInfo.InvariantCulture,"Voice service (id={0}) completed.", service.Id);
            this.Logger.Log(Logger.LogLevel.Verbose, message);
            if (!this.IsTerminatingTerminated && this.RosterTrackingService.ParticipantCount == 1 && this.RosterTrackingService.IsTargetInRoster)
            {
                // Only customer exists. Start with main work flow.               
                AsyncTaskSequenceSerial mainFlowSequence = new AsyncTaskSequenceSerial(this);
                mainFlowSequence.Name = "MainDialog";
                mainFlowSequence.AddTask(new AsyncTask(this.StartupInvokeMainMenuDialog));
                mainFlowSequence.FailureCompletionReportHandlerDelegate =
                        delegate(Exception exception)
                        {
                            if (exception != null)
                            {
                                this.Shutdown();
                            }
                        };
                mainFlowSequence.Start();
            }
            else if (!this.IsTerminatingTerminated && this.RosterTrackingService.ParticipantCount > 1 && this.RosterTrackingService.IsTargetInRoster)
            {                
                // Customer and others exist in conference. Load the conference service to provide conference services for customer.
                m_conferenceService = new ConferenceService(this);
                AsyncTask task = new AsyncTask(this.StartConferenceService);
                task.StartTask();
            }
            else if (!this.RosterTrackingService.IsTargetInRoster)
            {
                // Customer left the session. Terminate the customer session.
                this.Shutdown();
            }
        }

        private void CustomerCallStateChanged(object o, CallStateChangedEventArgs e)
        {
            if (e.State == CallState.Terminating)
            {
                if (!m_customerTransfered)
                {
                    this.Shutdown();
                }
            }
            else if (e.State == CallState.Terminated)
            {
                m_customerCall.StateChanged -= this.CustomerCallStateChanged;
            }
        }

        public void DialOut(AsyncTask task, object state)
        {
            ArgumentTuple args = (ArgumentTuple) state;
            task.DoOneStep(
                delegate()
                {
                    string destinationUri = (string) args.One;
                    string rosterUri = (string) args.Two;
                    string displayName = (string) args.Three;
                    // Start the dialout and complete task when the dial out operation completes.
                    this.DialOut(destinationUri, rosterUri, displayName, exp => task.Complete(exp));
                });
        }

        private void DialOut(string destinationUri, string rosterUri, string displayName, CompletionDelegate completionDelegate)
        {
            try
            {
                var avmcuSession = this.CustomerConversation.ConferenceSession.AudioVideoMcuSession;
                AudioVideoMcuDialOutOptions options = new AudioVideoMcuDialOutOptions();
                options.PrivateAssistantDisabled = true;
                options.ParticipantUri = rosterUri; // uri that is shown in Roster.
                options.ParticipantDisplayName = displayName;
                avmcuSession.BeginDialOut(
                    destinationUri, // Uri to send the dial out to.
                    options,
                    ar =>
                    {
                        try
                        {
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


    }

    #endregion


    #region Customer

    public class Customer
    {
        private string mUserUri;
        private string mUri;
        private string mPhoneUri;
        private string mCallbackPhoneUri;
        private string mDisplayName = String.Empty;
        private bool mIsUriPhone;
        private string mCleanNumber;

        /// <summary>
        /// Gets the sip user uri of the caller. This is the discovered corporate uri of the user.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "This is a SIP uri.")]
        public string UserUri
        {
            get
            {
                return mUserUri;
            }
            set
            {
                mUserUri = value;
            }
        }

        /// <summary>
        /// Gets the sip uri of the caller. Could be a phone uri.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "This is a SIP uri.")]
        public string Uri
        {
            get
            {
                return mUri;
            }
            set
            {
                mUri = value;
            }
        }

        /// <summary>
        /// Gets whether the Uri discovered from incoming call represents a phone uri.
        /// </summary>
        public bool IsUriPhone
        {
            get
            {
                return mIsUriPhone;
            }
            set
            {
                mIsUriPhone = value;
            }
        }

        /// <summary>
        /// Gets the corporate phone uri of the caller.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "This is a SIP uri.")]
        public string PhoneUri
        {
            get
            {
                return mPhoneUri;
            }
            set
            {
                mPhoneUri = value;
            }
        }

        /// <summary>
        /// Gets the call back phone uri of the caller.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "This is a SIP uri.")]
        public string CallbackPhoneUri
        {
            get
            {
                return mCallbackPhoneUri;
            }
            set
            {
                mCallbackPhoneUri = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return mDisplayName;
            }
            set
            {
                mDisplayName = value;
            }
        }

        public string CleanNumber
        {
            get
            {
                return mCleanNumber;
            }
            set
            {
                mCleanNumber = value;
            }
        }
    }

    #endregion
    
    public class ParticipantCountChangedEventArgs : EventArgs
    {
        private int m_prevCount;
        private int m_currCount;
        public ParticipantCountChangedEventArgs(int prevCount, int currCount)
        {
            m_prevCount = prevCount;
            m_currCount = currCount;
        }

        /// <summary>
        /// Gets the previous count of participants.
        /// </summary>
        public int PreviousCount
        {
            get
            {
                return m_prevCount;
            }
        }

        /// <summary>
        /// Gets the current count of participants.
        /// </summary>
        public int CurrentCount
        {
            get
            {
                return m_currCount;
            }
        }

    }

    /// <summary>
    /// Tracks participants in AV MCU roster and provides services related to the roster.
    /// </summary>
    /// <remarks>This class should be hooked up before joining the MCU to avoid any potential race conditions with roster monitoring.</remarks>
    public class RosterTrackingService
    {
        private AppFrontEnd m_appFrontEnd;
        private Dictionary<RealTimeAddress, ParticipantEndpoint> m_mcuEndpoints;
        private RealTimeAddress m_targetUriAddress;
        private Dictionary<RealTimeAddress, AsyncTask> m_pendingActions;
        private List<AsyncTask> m_pendingActionsForNewParticipant;
        private TimeSpan m_waitTimeSpan; // To timeout actions.
        private object m_syncRoot = new object();

        public RosterTrackingService(AppFrontEnd appFrontEnd, TimeSpan waitTimeSpan)
        {
            Debug.Assert(appFrontEnd != null);
            if (waitTimeSpan.TotalMinutes > 10)
            {
                waitTimeSpan = new TimeSpan(0, 10, 0); // Cap it.
            }
            m_appFrontEnd = appFrontEnd;
            m_waitTimeSpan = waitTimeSpan;
            m_pendingActions = new Dictionary<RealTimeAddress, AsyncTask>();
            m_mcuEndpoints = new Dictionary<RealTimeAddress, ParticipantEndpoint>();
            m_pendingActionsForNewParticipant = new List<AsyncTask>();
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public Logger Logger
        {
            get
            {
                return m_appFrontEnd.Logger;
            }
        }

        /// <summary>
        /// Gets value that indicates if the original target is in Roster.
        /// </summary>
        public bool IsTargetInRoster
        {
            get
            {
                lock (m_syncRoot)
                {
                    if (m_targetUriAddress != null && m_mcuEndpoints.ContainsKey(m_targetUriAddress))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the target uri to be tracked by this class.
        /// </summary>
        public string TargetUri
        {
            get
            {
                string uri = null;
                if (m_targetUriAddress != null)
                {
                    uri = m_targetUriAddress.ToString();
                }
                return uri;
            }
            set
            {
                m_targetUriAddress = new RealTimeAddress(value);
            }
        }

        /// <summary>
        /// Gets the participant endpoint of the target uri that joined the conference. Can be null, if target never joined the conference.
        /// </summary>
        public ParticipantEndpoint TargetUriEndpoint
        {
            get
            {
                ParticipantEndpoint endpoint = null;
                lock (m_syncRoot)
                {
                    if (m_targetUriAddress != null && m_mcuEndpoints.ContainsKey(m_targetUriAddress))
                    {
                        endpoint = m_mcuEndpoints[m_targetUriAddress];
                    }
                }
                return endpoint;
            }
        }

        /// <summary>
        /// Gets the current count of endpoints (visible) in the mcu.
        /// </summary>
        public int ParticipantCount
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_mcuEndpoints.Count;
                }
            }
        }

        public void StartupWaitForNewParticipant(AsyncTask task, object state)
        {
            int currCount = (int)state;
            if (this.ParticipantCount > currCount)
            {
                task.Complete(null);
            }
            else
            {
                m_pendingActionsForNewParticipant.Add(task);
                TimerItem timerItem = new TimerItem(m_appFrontEnd.TimerWheel, m_waitTimeSpan);
                timerItem.Expired +=
                    delegate(object sender, EventArgs e)
                    {
                        lock (m_syncRoot)
                        {
                            m_pendingActionsForNewParticipant.Remove(task);
                        }
                        if (!task.IsCompleted)
                        {
                            task.Complete(new OperationTimeoutException("WaitForNewParticipant task timed out."));
                        }
                    };
                timerItem.Start();
            }
        }

        /// <summary>
        /// Waits for the two events tracked by this class, namely, the cusomter has joined the conference and the remote media flow is connected.
        /// </summary>
        /// <param name="task">The task to be performed.</param>
        public void StartupWaitForSpecificTarget(AsyncTask task, object state)
        {
            string uriToTrack = (string)state;
            RealTimeAddress addressToTrack = null;
            try
            {
                addressToTrack = new RealTimeAddress(uriToTrack);
            }
            catch (ArgumentException)
            {
                // Bad uri passed. Fail task.
                task.Complete(new InvalidOperationException("Invalid Uri passed to RosterService for tracking."));
                return;
            }
            lock (m_syncRoot)
            {
                //this.Logger.Log(Logger.LogLevel.Info, "Starting Customer Tracking.");
                if (m_mcuEndpoints.ContainsKey(addressToTrack))
                {
                    this.Logger.Log(Logger.LogLevel.Info, "Customer already in conference. Stopped Customer Tracking.");
                    task.Complete(null);
                }
                else
                {
                    m_pendingActions.Add(addressToTrack, task);
                    this.Logger.Log(Logger.LogLevel.Info, 
                        String.Format("Customer {0} is not conference yet. Starting Timer for Customer Tracking.", uriToTrack));
                    TimerItem timerItem = new TimerItem(m_appFrontEnd.TimerWheel, m_waitTimeSpan);
                    timerItem.Expired +=
                        delegate(object sender, EventArgs e)
                        {
                            lock (m_syncRoot)
                            {
                                m_pendingActions.Remove(addressToTrack);
                            }
                            if (!task.IsCompleted && m_mcuEndpoints.ContainsKey(addressToTrack))
                            {
                                this.Logger.Log(Logger.LogLevel.Info, "Customer joined the conference. Stopped Customer Tracking.");
                                task.Complete(null);
                            }
                            else if (!task.IsCompleted)
                            {
                                task.Complete(new OperationTimeoutException("Customer tracking timed out."));
                            }
                        };
                    // We have not seen both events yet. Let us start the timer.
                    timerItem.Start();
                }
            }
        }

        public void ParticipantEndpointAttendanceChanged(
            object sender,
            ParticipantEndpointAttendanceChangedEventArgs<AudioVideoMcuParticipantEndpointProperties> e)
        {
            int prevCount = 0;
            int currCount = 0;
            lock(m_syncRoot)
            {
                prevCount = m_mcuEndpoints.Count;
                bool someoneJoined = false;
                foreach (KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties> pair in e.Joined)
                {
                    if (pair.Key.Participant.RosterVisibility == ConferencingRosterVisibility.Hidden)
                    {
                        continue; // Skip hidden participants.
                    }
                    someoneJoined = true; // Some visible participant joined.
                    RealTimeAddress participantAddress = new RealTimeAddress(pair.Key.Participant.Uri);
                    if (!m_mcuEndpoints.ContainsKey(participantAddress))
                    {
                        m_mcuEndpoints.Add(participantAddress, pair.Key);
                        if (m_pendingActions.ContainsKey(participantAddress))
                        {
                            AsyncTask task = m_pendingActions[participantAddress];
                            if (!task.IsCompleted)
                            {
                                task.Complete(null);
                            }
                            m_pendingActions.Remove(participantAddress);
                        }
                   }                   
                }
                if (someoneJoined)
                {
                    foreach (AsyncTask task in m_pendingActionsForNewParticipant)
                    {
                        int oldCount = (int)task.State;
                        if (this.ParticipantCount > oldCount)
                        {
                            task.Complete(null);
                        }
                    }
                }

                foreach (KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties> pair in e.Left)
                {
                    if (pair.Key.Participant.RosterVisibility == ConferencingRosterVisibility.Hidden)
                    {
                        continue; // Skip hidden participants.
                    }
                    RealTimeAddress address = new RealTimeAddress(pair.Key.Participant.Uri);
                    if (m_mcuEndpoints.ContainsKey(address))
                    {
                        m_mcuEndpoints.Remove(address);
                    }
                }
                currCount = m_mcuEndpoints.Count;
            }
            if (prevCount != currCount)
            {
                // Raise event.
                EventHandler<ParticipantCountChangedEventArgs> countChangedEventHandler = this.ParticipantCountChanged;
                if (countChangedEventHandler != null)
                {
                    countChangedEventHandler(this, new ParticipantCountChangedEventArgs(prevCount, currCount));
                }
            }
        }

        /// <summary>
        /// Raised when the roster count goes down to 1 or 0 from above. It is up to the caller to terminate the conference if inactive.
        /// </summary>
        public event EventHandler<ParticipantCountChangedEventArgs> ParticipantCountChanged;
    }
}
