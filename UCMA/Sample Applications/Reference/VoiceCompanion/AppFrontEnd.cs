/*=====================================================================
  File:      AppFrontEnd.cs

  Summary:   Implements an application front end.

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
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;
using System.Collections.ObjectModel;
using System.Text;


namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    public class MyUserEndpoint
    {
        private UserEndpoint m_userEndpoint;
        private Presence.ContactCard m_contactCard;

        public MyUserEndpoint(UserEndpoint userEndpoint)
        {
            m_userEndpoint = userEndpoint;
            userEndpoint.LocalOwnerPresence.PresenceNotificationReceived += LocalOwnerPresencePresenceNotificationReceived;
        }

        void LocalOwnerPresencePresenceNotificationReceived(object sender, Presence.LocalPresentityNotificationEventArgs e)
        {
            if (e.ContactCard != null)
            {
                m_contactCard = e.ContactCard; // Store the latest contact card for self.
            }
        }

        /// <summary>
        /// Gets the user endpoint.
        /// </summary>
        public UserEndpoint UserEndpoint
        {
            get
            {
                return m_userEndpoint;
            }
        }

        /// <summary>
        /// Gets the contact card of owner of the endpoint.
        /// </summary>
        public Presence.ContactCard ContactCard
        {
            get
            {
                return m_contactCard;
            }
        }
    }

    public class UserEndpointCreationActionResult : AsyncTaskResult
    {
        MyUserEndpoint m_myUserEndpoint;
        public UserEndpointCreationActionResult(MyUserEndpoint myUserEndpoint, Exception exception)
            : base(exception)
        {
            m_myUserEndpoint = myUserEndpoint;
        }

        public MyUserEndpoint MyUserEndpoint
        {
            get
            {
                return m_myUserEndpoint;
            }
        }
    }

    public class AppFrontEnd : ComponentBase
    {
        #region Private fields
        private readonly AppPlatform m_parent;
        private readonly ApplicationEndpointSettings m_settings;
        private readonly LinkedList<CustomerSession> m_customerSessions;
        private ApplicationEndpoint m_endpoint;
        private MusicProvider m_mohProvider;
        private CallbackManager m_callbackManager;
        private TimerWheel m_timerWheel = new TimerWheel();

        // Maintain customer endpoints to avoid creating multiple customer endpoints.
        private Dictionary<RealTimeAddress, MyUserEndpoint> m_userEndpoints;
        private Dictionary<RealTimeAddress, int> m_userEndpointReferenceCounts;
        private List<AsyncTask> m_pendingUserEndpointCreationTasks; // Stores tasks that are waiting for user endpoint to establish first.
        #endregion Private fields

        #region Public interface

        public AppFrontEnd(AppPlatform parent, ApplicationEndpointSettings settings):base(parent)
        {
            Debug.Assert(parent != null);
            m_parent = parent;
            m_settings = settings;
            m_customerSessions = new LinkedList<CustomerSession>();
            m_userEndpoints = new Dictionary<RealTimeAddress, MyUserEndpoint>();
            m_userEndpointReferenceCounts = new Dictionary<RealTimeAddress, int>();
            m_pendingUserEndpointCreationTasks = new List<AsyncTask>();
        }

        public TimerWheel TimerWheel
        {
            get { return m_timerWheel; }
        }

        public AppPlatform AppPlatform
        {
            get { return m_parent; }
        }

        public ApplicationEndpoint Endpoint
        {
            get { return m_endpoint; }
        }

        public MusicProvider MusicOnHoldProvider
        {
            get { return m_mohProvider; }
        }

        public ApplicationEndpointSettings EndpointSettings
        {
            get { return m_settings; }
        }

        public CallbackManager CallbackManager
        {
            get { return m_callbackManager; }
        }
        
        public void ReportSessionShutdown(CustomerSession session)
        {
            lock (this.SyncRoot)
            {
                m_customerSessions.Remove(session);
            }
        }

        public void InitiateCallback(
            Customer customer,
            string callbackTargetUri,
            string callbackTargetDisplayName)
        {
            string logMessage = 
                string.Format(
                CultureInfo.InvariantCulture,
                "Callback Initiation: Customer {0} Target {1}",
                callbackTargetUri,
                callbackTargetDisplayName);

            this.Logger.Log(Logger.LogLevel.Info, logMessage);

            CustomerSession newSession = null;
            lock (this.SyncRoot)
            {
                if (this.IsTerminatingTerminated)
                {
                    return;
                }
                
                newSession = new CustomerSession(this, customer, callbackTargetUri, callbackTargetDisplayName);
                this.AddNewSession(newSession);
            } 

            Debug.Assert(newSession != null, "newSession should not be null");
            
            // Start up the customer session outside of the lock to avoid deadlocks
            this.StartupCustomerSession(newSession);
        }

        public void CreateOrGetUserEndpoint(AsyncTask task, object state)
        {
            string ownerUri = state as string;
            if (String.IsNullOrEmpty(ownerUri))
            {
                task.Complete(new InvalidOperationException("OwnerUri is needed to request a user endpoint."));
                return;
            }
            RealTimeAddress ownerAddress = null;
            try
            {
                ownerAddress = new RealTimeAddress(ownerUri);
            }
            catch (ArgumentException exception)
            {
                task.Complete(exception);
                return;
            }
            MyUserEndpoint myUserEndpoint = null;
            lock (this.SyncRoot)
            {
                if (m_userEndpoints.ContainsKey(ownerAddress))
                {
                    myUserEndpoint = m_userEndpoints[ownerAddress];
                    if (myUserEndpoint.UserEndpoint.State == LocalEndpointState.Terminating || myUserEndpoint.UserEndpoint.State == LocalEndpointState.Terminated)
                    {
                        myUserEndpoint = null; // Loose it since it is going away.
                        m_userEndpoints.Remove(ownerAddress);
                        m_userEndpointReferenceCounts.Remove(ownerAddress);
                    }
                    else
                    {
                        int count = m_userEndpointReferenceCounts[ownerAddress];
                        count++;
                        m_userEndpointReferenceCounts[ownerAddress] = count;
                    }
                }
                if (myUserEndpoint == null)
                {
                    // Create and add user endpoint into dictionary.
                    // One could use the platform discover server from uri if the topology has DNS srv records for server auto discovery. 
                    // Normally, this would point to a director. Here, we will use the proxy of the application endpoint.
                    UserEndpointSettings userEndpointSettings = new UserEndpointSettings(ownerUri, m_settings.ProxyHost, m_settings.ProxyPort);
                    UserEndpoint userEndpoint = new UserEndpoint(m_parent.Platform, userEndpointSettings);
                    myUserEndpoint = new MyUserEndpoint(userEndpoint);
                    m_userEndpoints.Add(ownerAddress, myUserEndpoint);
                    m_userEndpointReferenceCounts.Add(ownerAddress, 1);
                    myUserEndpoint.UserEndpoint.StateChanged += UserEndpointStateChanged; // Ensures that only one registration per endpoint.
                }
                UserEndpointCreationActionResult result = new UserEndpointCreationActionResult(myUserEndpoint, null);
                task.TaskResult = result; // Store it for now.
                if (myUserEndpoint.UserEndpoint.State == LocalEndpointState.Established)
                {
                    task.Complete(null, result);
                }
                else if (myUserEndpoint.UserEndpoint.State == LocalEndpointState.Establishing)
                {
                    // Wait till the endpoint establish completes.
                    lock (this.SyncRoot)
                    {
                        m_pendingUserEndpointCreationTasks.Add(task);
                    }
                }
                else if (myUserEndpoint.UserEndpoint.State == LocalEndpointState.Idle)
                {
                    AsyncTask establishTask = new AsyncTask(this.StartupUserEndpoint, myUserEndpoint.UserEndpoint);
                    establishTask.TaskCompleted +=
                        delegate(object sender, AsyncTaskCompletedEventArgs e)
                        {
                            task.TaskResult.Exception = e.ActionResult.Exception; // Transfer
                            task.Complete(e.ActionResult.Exception, task.TaskResult);
                            lock (this.SyncRoot)
                            {
                                // Complete pending tasks
                                foreach (AsyncTask pendingTask in m_pendingUserEndpointCreationTasks)
                                {
                                    pendingTask.TaskResult.Exception = e.ActionResult.Exception;
                                    pendingTask.Complete(e.ActionResult.Exception, pendingTask.TaskResult);
                                }
                                m_pendingUserEndpointCreationTasks.Clear();
                            }
                        };
                    establishTask.StartTask();
                }
            }
        }

        /// <summary>
        /// Releases the user endpoint from usage from a component.
        /// </summary>
        /// <param name="task">The task to be done.</param>
        /// <param name="state">The user endpoint to release.</param>
        /// <remarks>If ref count is reduced to 0, the userendpoint will be terminated.</remarks>
        public void RelaseUserEndpoint(AsyncTask task, object state)
        {
            MyUserEndpoint myUserEndpoint = state as MyUserEndpoint;
            RealTimeAddress ownerAddress = new RealTimeAddress(myUserEndpoint.UserEndpoint.OwnerUri);
            bool completeNeeded = true;
            lock (this.SyncRoot)
            {
                if (m_userEndpoints.ContainsKey(ownerAddress))
                {
                    MyUserEndpoint storedEndpoint = m_userEndpoints[ownerAddress];
                    if (storedEndpoint == myUserEndpoint) // What we have matches the released endpoint. Reduce ref count.
                    {
                        int count = m_userEndpointReferenceCounts[ownerAddress];
                        count--;
                        m_userEndpointReferenceCounts[ownerAddress] = count;
                        if (count == 0)
                        {
                            // Terminate the endpoint.
                            AsyncTask terminateEndpointTask = new AsyncTask(this.ShutdownUserEndpoint, myUserEndpoint.UserEndpoint);
                            terminateEndpointTask.TaskCompleted +=
                                delegate(object sender, AsyncTaskCompletedEventArgs e)
                                {
                                    task.Complete(e.ActionResult.Exception);
                                };
                            terminateEndpointTask.StartTask();
                            completeNeeded = false;
                        }
                    }
                }
            }
            if (completeNeeded)
            {
                task.Complete(null);
            }
        }

        private void UserEndpointStateChanged(object sender, LocalEndpointStateChangedEventArgs e)
        {
            UserEndpoint userEndpoint = sender as UserEndpoint;
            RealTimeAddress ownerAddress = new RealTimeAddress(userEndpoint.OwnerUri);
            if (e.State == LocalEndpointState.Terminating)
            {
                lock (this.SyncRoot)
                {
                    // Lost the cache.
                    if (m_userEndpoints.ContainsKey(ownerAddress))
                    {
                        m_userEndpoints.Remove(ownerAddress);
                        m_userEndpointReferenceCounts.Remove(ownerAddress);
                    }
                }
            }
        }
        #endregion

        #region Protected methods

        protected override void StartupCore()
        {
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.Name = "StartupAppFrontEnd";
            sequence.SuccessCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.FailureCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.AddTask(new AsyncTask(this.StartupCallbackManager));
            sequence.AddTask(new AsyncTask(this.StartupMusicProvider));
            sequence.AddTask(new AsyncTask(this.StartupEndpoint));
            sequence.Start();
        }

        protected override void ShutdownCore()
        {
            m_endpoint.UnregisterForIncomingCall<AudioVideoCall>(this.ReceiveIncomingAvCall);
            m_endpoint.UnregisterForIncomingCall<InstantMessagingCall>(this.ReceiveIncomingIMCall);
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.Name = "ShutdownAppFrontEnd";
            sequence.SuccessCompletionReportHandlerDelegate = this.CompleteShutdown;
            sequence.FailureCompletionReportHandlerDelegate = this.CompleteShutdown;
            sequence.AddTask(new AsyncTask(this.ShutdownCallbackManager));
            sequence.AddTask(new AsyncTask(this.ShutdownMusicProvider));

            // Shutdown customer sessions in parallel. 
            AsyncTaskSequenceParallel customerSessionsSequence = new AsyncTaskSequenceParallel(this);
            AsyncTask shutdownCustomerSessions = new AsyncTask(AsyncTask.SequenceStartingMethod, customerSessionsSequence);
            customerSessionsSequence.Name = "ShutdownCustomerSessions";
            customerSessionsSequence.FailureCompletionReportHandlerDelegate = shutdownCustomerSessions.Complete;
            customerSessionsSequence.SuccessCompletionReportHandlerDelegate = shutdownCustomerSessions.Complete;
            // Populate the parallel sequence with individual customer sessions to terminate.
            lock (this.SyncRoot)
            {
                foreach (CustomerSession session in m_customerSessions)
                {
                    AsyncTask task = new AsyncTask(this.ShutdownCustomerSession, session);
                    customerSessionsSequence.AddTask(task);
                }
            }
            sequence.AddTask(shutdownCustomerSessions);
       
            // Shutdown user endpoint just in case they are still there.    
            AsyncTaskSequenceParallel userEndpointsShutdownSequence = new AsyncTaskSequenceParallel(this);
            AsyncTask shutdownUserEndpoints = new AsyncTask(AsyncTask.SequenceStartingMethod, userEndpointsShutdownSequence);
            userEndpointsShutdownSequence.Name = "ShutdownUserEndpoints";
            userEndpointsShutdownSequence.FailureCompletionReportHandlerDelegate = shutdownUserEndpoints.Complete;
            userEndpointsShutdownSequence.SuccessCompletionReportHandlerDelegate = shutdownUserEndpoints.Complete;
            lock (this.SyncRoot)
            {
                foreach (MyUserEndpoint myUserEndpoint in m_userEndpoints.Values)
                {
                    AsyncTask task = new AsyncTask(this.ShutdownUserEndpoint, myUserEndpoint.UserEndpoint);
                    userEndpointsShutdownSequence.AddTask(task);
                }
            }
            sequence.AddTask(shutdownUserEndpoints);
            sequence.AddTask(new AsyncTask(this.ShutdownEndpoint));
            sequence.Start();
        }

        public override void CompleteShutdown()
        {
            base.CompleteShutdown();
        }

        public override void CompleteStartup(Exception exp)
        {
            if (exp != null)
            {
                this.Logger.Log(Logger.LogLevel.Error, "FrontEnd failed to establish : " + m_endpoint.OwnerUri, exp);
                this.BeginShutdown(ar => this.EndShutdown(ar), null);
            }
            else
            {
                this.Logger.Log(Logger.LogLevel.Info, "FrontEnd established successfully : " + m_endpoint.OwnerUri);
            }

            base.CompleteStartup(exp);
        }

        #endregion Protected methods

        #region Private methods

        private void EndpointStateChanged(object sender, LocalEndpointStateChangedEventArgs e)
        {
            if (e.State == LocalEndpointState.Terminating)
            {
                this.BeginShutdown(ar => this.EndShutdown(ar), null);
            }
        }

        private void StartupUserEndpoint(AsyncTask task, object state)
        {
            UserEndpoint userEndpoint = state as UserEndpoint;
            task.DoOneStep(
                delegate()
                {
                    if (userEndpoint == null)
                    {
                        task.Complete(new InvalidOperationException("UserEndpoint is needed to establish."));
                        return;
                    }
                    else if (userEndpoint.State == LocalEndpointState.Established)
                    {
                        task.Complete(null); // Already established.
                    }
                    else if (userEndpoint.State == LocalEndpointState.Idle)
                    {
                        Logger.Log(Logger.LogLevel.Info, "Establishing UserEndpoint." + userEndpoint.OwnerUri);
                        userEndpoint.BeginEstablish(
                            delegate(IAsyncResult ar)
                            {
                                task.DoOneStep(
                                    delegate()
                                    {
                                        userEndpoint.EndEstablish(ar);
                                        Logger.Log(Logger.LogLevel.Info, "Established UserEndpoint." + userEndpoint.OwnerUri);
                                        userEndpoint.LocalOwnerPresence.BeginSubscribe(
                                            delegate(IAsyncResult ar2)
                                            {
                                                task.DoFinalStep(
                                                    delegate()
                                                    {
                                                        userEndpoint.LocalOwnerPresence.EndSubscribe(ar2);
                                                    });
                                            },
                                            null);
                                    });
                            },
                            null);
                    }
                    else
                    {
                        task.Complete(new InvalidOperationException("UserEndpoint should be in Idle state to establish."));
                    }
                });
        }

        private void ShutdownUserEndpoint(AsyncTask task, object state)
        {
            UserEndpoint userEndpoint = state as UserEndpoint;
            if (userEndpoint == null)
            {
                task.Complete(null);
                return;
            }
            task.DoOneStep(
                delegate()
                {
                    Logger.Log(Logger.LogLevel.Info, "Terminating UserEndpoint." + userEndpoint.OwnerUri);
                    userEndpoint.BeginTerminate(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    userEndpoint.EndTerminate(ar);
                                    Logger.Log(Logger.LogLevel.Info, "Terminated UserEndpoint." + userEndpoint.OwnerUri);
                                });
                        },
                        null);
                });

        }


        private void StartupEndpoint(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    m_endpoint = new ApplicationEndpoint(m_parent.Platform, m_settings);
                    m_endpoint.RegisterForIncomingCall<AudioVideoCall>(this.ReceiveIncomingAvCall);
                    m_endpoint.RegisterForIncomingCall<InstantMessagingCall>(this.ReceiveIncomingIMCall);
                    m_endpoint.LocalOwnerPresence.SubscriberNotificationReceived += SubscriberNotificationReceived;
                    m_endpoint.StateChanged += this.EndpointStateChanged;
                    Logger.Log(Logger.LogLevel.Info, "Starting Application Endpoint.");
                    m_endpoint.BeginEstablish(
                        delegate(IAsyncResult ar)
                        {
                            task.DoOneStep(
                                delegate()
                                {
                                    m_endpoint.EndEstablish(ar);
                                    Logger.Log(Logger.LogLevel.Info, "Started Application Endpoint. Tel #:" + m_endpoint.OwnerPhoneUri??"null");
                                    m_endpoint.LocalOwnerPresence.BeginSubscribe(
                                        delegate(IAsyncResult ar2)
                                        {
                                            task.DoFinalStep(
                                                delegate()
                                                {
                                                    m_endpoint.LocalOwnerPresence.EndSubscribe(ar2);
                                                });
                                        },
                                        null);
                                });
                        }, 
                        null);
                });

        }

        /// <summary>
        /// The method that handles notification about subscribers to this application owner uri.
        /// </summary>
        /// <param name="sender">The sender of this event. LocalOwnerPresence.</param>
        /// <param name="e">The event argument.</param>
        private void SubscriberNotificationReceived(object sender, Presence.SubscriberNotificationEventArgs e)
        {
            Collection<Presence.Subscriber> subscribers = e.WatcherList;
            foreach(Presence.Subscriber subscriber in subscribers)
            {
                if (subscriber.NetworkType == SourceNetwork.SameEnterprise)
                {
                    // This subscriber has added this application as contact. If the user is from same enterprise, we can get phone numbers of this user
                    // and add them to our RNL table.
                    System.Threading.ThreadPool.QueueUserWorkItem(this.AddUserPhoneNumbersToRnl, subscriber.Id);
                }
                try
                {
                    m_endpoint.LocalOwnerPresence.BeginAcknowledgeSubscriber(subscriber.Id, ar => m_endpoint.LocalOwnerPresence.EndAcknowledgeSubscriber(ar), null);
                }
                catch (InvalidOperationException)
                {
                    // Ignore failures for acknowledgement.
                }
                catch (RealTimeException)
                {
                    // Ignore failures for acknowledgement.
                }
            }
        }

        private void AddUserPhoneNumbersToRnl(object state)
        {
            string userUri = state as string;
            if (!userUri.StartsWith("sip"))
            {
                userUri = "sip:" + userUri;
            }
            AsyncTask getUserEndpointTask = new AsyncTask(this.CreateOrGetUserEndpoint, userUri);
            getUserEndpointTask.TaskCompleted += 
                delegate(object sender, AsyncTaskCompletedEventArgs e)
                    {
                        UserEndpointCreationActionResult res = e.ActionResult as UserEndpointCreationActionResult;
                        Debug.Assert(res != null);

                        if (res.MyUserEndpoint.ContactCard != null)
                        {
                            foreach (Presence.PhoneNumber p in res.MyUserEndpoint.ContactCard.PhoneNumbers)
                            {
                                this.AppPlatform.ReverseNumberLookUp.AddEntry(p.Uri, res.MyUserEndpoint.UserEndpoint.OwnerUri);
                            }
                        }                            
                    };
            getUserEndpointTask.StartTask();
        }

        void getUserEndpointTask_TaskCompleted(object sender, AsyncTaskCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ShutdownEndpoint(AsyncTask task, object state)
        {
            if (m_endpoint == null)
            {
                task.Complete(null);
                return;
            }
            task.DoOneStep(
                delegate()
                {
                    Logger.Log(Logger.LogLevel.Info, "Terminating Application Endpoint.");
                    m_endpoint.BeginTerminate(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_endpoint.EndTerminate(ar);
                                    Logger.Log(Logger.LogLevel.Info, "Terminated Application Endpoint.");
                                });
                        },
                        null);
                });

        }

        private void StartupCallbackManager(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    m_callbackManager = new CallbackManager(this);
                    //Logger.Log(Logger.LogLevel.Info, "Starting Callback Manager.");
                    m_callbackManager.BeginStartup(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_callbackManager.EndStartup(ar);
                                    //Logger.Log(Logger.LogLevel.Info, "Started Callback Manager.");
                                });
                        }, 
                        null);
                });
        }

        private void StartupMusicProvider(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    var mohFileConfig = ApplicationConfiguration.GetMusicOnHoldConfiguration();
                    m_mohProvider = new MusicProvider(this.AppPlatform, mohFileConfig.FilePath);
                    //Logger.Log(Logger.LogLevel.Info, "Starting Music Provider.");
                    m_mohProvider.BeginStartup(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_mohProvider.EndStartup(ar);
                                    //Logger.Log(Logger.LogLevel.Info, "Started Music Provider.");
                                });
                        }, 
                        null);
                });
        }

        private void StartupCustomerSession(AsyncTask task, object state)
        {
            CustomerSession customerSession = (CustomerSession)state;
            task.DoOneStep(
                delegate()
                {
                    customerSession.BeginStartup(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    customerSession.EndStartup(ar);
                                    //this.Logger.Log(Logger.LogLevel.Info, String.Format("Customer session {0} shutdown.", customerSession.Customer.UserUri));
                                });
                        },
                        null);
                });
        }

        private void ShutdownCustomerSession(AsyncTask task, object state)
        {
            CustomerSession customerSession = (CustomerSession)state;
            task.DoOneStep(
                delegate()
                {
                    customerSession.BeginShutdown(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    customerSession.EndShutdown(ar);
                                    //this.Logger.Log(Logger.LogLevel.Info, String.Format("Customer session {0} shutdown.", customerSession.Customer.UserUri));
                                });
                        },
                        null);
                });
        }

        private void ShutdownCallbackManager(AsyncTask task, object state)
        {
            if (m_callbackManager == null)
            {
                task.Complete(null);
                return;
            }
            task.DoOneStep(
                delegate()
                {
                    CallbackManager callbackManager = m_callbackManager;
                    callbackManager.BeginShutdown(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    callbackManager.EndShutdown(ar);
                                    this.Logger.Log(Logger.LogLevel.Verbose, "Callback Manager shutdown.");
                                });
                        },
                        null);

                });
        }

        private void ShutdownMusicProvider(AsyncTask task, object state)
        {
            if (m_mohProvider == null)
            {
                task.Complete(null);
                return;
            }
            task.DoOneStep(
                delegate()
                {
                    MusicProvider musicProvider = m_mohProvider;
                    musicProvider.BeginShutdown(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                            delegate()
                            {
                                musicProvider.EndShutdown(ar);
                                this.Logger.Log(Logger.LogLevel.Verbose, "Music provider shutdown.");
                            });
                        },
                        null);

                });
        }

        private void ReceiveIncomingIMCall(object sender, CallReceivedEventArgs<InstantMessagingCall> e)
        {
            this.Logger.Log(Logger.LogLevel.Verbose,
                string.Format(
                CultureInfo.InvariantCulture,
                "IMCall {0} received.",
                Logger.Pointer(e.Call)));

            if (this.IsTerminatingTerminated || e.IsConferenceDialOut)
            {
                var declineOptions = new CallDeclineOptions();
                declineOptions.ResponseCode = ResponseCode.RequestTerminated;
                this.RejectCall(e.Call, declineOptions);
                return;
            }
            ConversationParticipant caller = e.Call.RemoteEndpoint.Participant;
            RealTimeAddress uriAddress = new RealTimeAddress(caller.Uri);
            if (!uriAddress.IsPhone && 
                 e.Call.RemoteEndpoint.Participant.SourceNetwork == SourceNetwork.SameEnterprise)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(this.HandleIMCall, e);
            }
            else
            {
                var declineOptions = new CallDeclineOptions();
                declineOptions.ResponseCode = ResponseCode.RequestTerminated;
                this.RejectCall(e.Call, declineOptions);
                return;
            }
        }

        private void HandleIMCall(object state)
        {
            CallReceivedEventArgs<InstantMessagingCall> e = state as CallReceivedEventArgs<InstantMessagingCall>;
            ConversationParticipant caller = e.Call.RemoteEndpoint.Participant;
            InstantMessagingCall imCall = e.Call;
            InstantMessagingFlow imFlow = null;
            int messageCount = 0;
            imCall.InstantMessagingFlowConfigurationRequested +=
                delegate(object sender2, InstantMessagingFlowConfigurationRequestedEventArgs e2)
                {
                    imFlow = e2.Flow;
                    imFlow.MessageReceived +=
                        delegate(object sender3, InstantMessageReceivedEventArgs e3)
                        {
                            messageCount++;
                            string message = e3.TextBody;
                            message = message.Trim();
                            if (!String.IsNullOrEmpty(message) && message.StartsWith("add", StringComparison.OrdinalIgnoreCase))
                            {
                                string[] tokens = message.Split(' ');
                                if (this.AppPlatform.ReverseNumberLookUp.AddEntry(tokens[1], caller.Uri))
                                {
                                    this.SendIMResponse(imFlow, "Added telephone number.");

                                }
                                else
                                {
                                    this.SendIMResponse(imFlow, "Telephone number exists.");
                                }
                            }
                            else if (!String.IsNullOrEmpty(message) && message.StartsWith("remove", StringComparison.OrdinalIgnoreCase))
                            {
                                string[] tokens = message.Split(' ');
                                if (this.AppPlatform.ReverseNumberLookUp.RemoveEntry(tokens[1], caller.Uri))
                                {
                                    this.SendIMResponse(imFlow, "Removed telephone number.");

                                }
                                else
                                {
                                    this.SendIMResponse(imFlow, "Telephone number does not exist or you do not own the tel #.");
                                }
                            }
                            else if (!String.IsNullOrEmpty(message) && message.Equals("list", StringComparison.OrdinalIgnoreCase))
                            {
                                Collection<string> list = this.AppPlatform.ReverseNumberLookUp.FindPhoneNumbers(caller.Uri);
                                StringBuilder response = new StringBuilder();
                                foreach (string s in list)
                                {
                                    response.Append(s); response.Append("\r\n");
                                }
                                this.SendIMResponse(imFlow, response.ToString());
                            }
                            else
                            {
                                this.SendIMHelpMessage(imFlow);
                            }
                            if (messageCount > 5) // We could also terminate based on timer.
                            {
                                this.TerminateIMCall(imCall);
                            }
                        };
                };
            try
            {
                imCall.BeginAccept(
                    delegate(IAsyncResult ar)
                    {
                        try
                        {
                            imCall.EndAccept(ar);
                            this.SendIMHelpMessage(imFlow);
                        }
                        catch (RealTimeException)
                        {
                        }
                    },
                    null);
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void TerminateIMCall(InstantMessagingCall imCall)
        {
            imCall.BeginTerminate(ar => { Logger.Log(Logger.LogLevel.Info, "IMCall terminated."); imCall.EndTerminate(ar); }, null);
        }

        private void SendIMResponse(InstantMessagingFlow imFlow, string message)
        {
            try
            {
                imFlow.BeginSendInstantMessage(
                        message,
                        delegate(IAsyncResult ar)
                        {
                            try
                            {
                                imFlow.EndSendInstantMessage(ar);
                            }
                            catch (RealTimeException)
                            {
                            }
                        },
                        null);
            }
            catch (InvalidOperationException)
            {
            }
        }

        /// <summary>
        /// Sends IM help message to caller.
        /// </summary>
        /// <param name="imFlow">The IM flow.</param>
        private void SendIMHelpMessage(InstantMessagingFlow imFlow)
        {
            string helpMessage = "The following commands are supported:\r\n1. add <tel>.\r\n2. remove <tel>.\r\n3. list.";
            this.SendIMResponse(imFlow, helpMessage);
        }

        private void ReceiveIncomingAvCall(object sender, CallReceivedEventArgs<AudioVideoCall> e)
        {
            this.Logger.Log(Logger.LogLevel.Verbose,
                string.Format(
                CultureInfo.InvariantCulture,
                "AvCall {0} received.",
                Logger.Pointer(e.Call)));

            if (this.IsTerminatingTerminated || e.IsConferenceDialOut)
            {
                var declineOptions = new CallDeclineOptions();
                declineOptions.ResponseCode = ResponseCode.RequestTerminated;
                this.RejectCall(e.Call, declineOptions);
                return;
            }

            this.StartupCustomerSession(e.Call);
        }

        private void RejectCall(Call call, CallDeclineOptions options)
        {
            this.Logger.Log(Logger.LogLevel.Info, string.Format(CultureInfo.InvariantCulture, "Rejecting the call {0}.", Logger.Pointer(call)));

            try
            {
                call.Decline(options);
            }
            catch (InvalidOperationException ioe)
            {
                this.Logger.Log(Logger.LogLevel.Info, ioe); // This can happen if the call was cancelled by now.
            }
            catch (RealTimeException rte)
            {
                this.Logger.Log(Logger.LogLevel.Error, rte);
            }
        }

        private void StartupCustomerSession(AudioVideoCall call)
        {
            CustomerSession customerSession = new CustomerSession(this, call);
            this.StartupCustomerSession(customerSession);
        }

        private void StartupCustomerSession(CustomerSession customerSession)
        {
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.AddTask(new AsyncTask(this.StartupCustomerSession, customerSession));
            // There is nothing to do when the start completes successfully for the customer session.
            sequence.FailureCompletionReportHandlerDelegate =
                delegate(Exception excception)
                {
                    this.RemoveSession(customerSession);
                };
            sequence.Start();
        }

        private void AddNewSession(CustomerSession newSession)
        {
            lock (this.SyncRoot)
            {
                m_customerSessions.AddLast(newSession);
            }
        }

        private void RemoveSession(CustomerSession session)
        {
            lock (this.SyncRoot)
            {
                m_customerSessions.Remove(session);
            }
        }

        #endregion
    }

    [Serializable]
    public class VoiceCompanionException : Exception
    {
        public VoiceCompanionException() :
            base()
        {
        }

        public VoiceCompanionException(string message) :
            base(message)
        {
        }

        public VoiceCompanionException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        protected VoiceCompanionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

}