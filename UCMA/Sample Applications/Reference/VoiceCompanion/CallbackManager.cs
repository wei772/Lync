/*=====================================================================
  File:      CallbackManager.cs

  Summary:   Manages and initiates callbacks to customers.

***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Timers;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Signaling;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    #region CallbackRequest

    /// <summary>
    /// Represents one instance of a callback that is setup. Stores information relevant for the callback.
    /// </summary>
    public class CallbackRequest
    {
        private Customer m_customer;
        private string m_targetUri;
        private string m_targetDisplayName;
        private DateTime m_creationTime;
        private MyUserEndpoint m_customerEndpoint;
        private RemotePresenceView m_presenceView;
        private Exception m_exception;

        public CallbackRequest(Customer customer, string targetUri, string targetDisplayName)
        {
            m_customer = customer;
            m_targetUri = targetUri;
            m_targetDisplayName = targetDisplayName;
            m_creationTime = DateTime.Now;
        }

        public Customer Customer
        {
            get
            {
                return m_customer;
            }
        }

        public string TargetUri
        {
            get
            {
                return m_targetUri;
            }
        }

        public string TargetDispalyName
        {
            get
            {
                return m_targetDisplayName;
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return m_creationTime;
            }
        }

        public MyUserEndpoint MyUserEndpoint
        {
            get
            {
                return m_customerEndpoint;
            }
            set
            {
                m_customerEndpoint = value;
            }
        }

        public UserEndpoint CutomerEndpoint
        {
            get
            {
                return m_customerEndpoint.UserEndpoint;
            }
        }

        public RemotePresenceView PresenceView
        {
            get
            {
                return m_presenceView;
            }
            set
            {
                m_presenceView = value;
            }
        }

        public Exception Exception
        {
            get
            {
                return m_exception;
            }
            set
            {
                m_exception = value;
            }
        }

    }

    #endregion

    #region CallbackManager

    public class CallbackManager : ComponentBase
    {
        #region Private fields
        /// <summary>
        /// Stores all callbacks.
        /// </summary>
        private readonly LinkedList<CallbackRequest> m_pendingRequests;
        private readonly Timer m_cleanupTimer;
        private readonly AppFrontEnd m_appFrontEnd;

        /// <summary>
        /// This represents how often the timer will clean up its items.
        /// </summary>
        private static readonly TimeSpan m_cleanupIntervalSpan = TimeSpan.FromMinutes(30);
        /// <summary>
        /// This represents how long a request can stay.
        /// </summary>
        private readonly TimeSpan m_maxLifeSpanOfRequest = TimeSpan.FromHours(2);
        #endregion

        #region Constructor

        public CallbackManager(AppFrontEnd appFrontEnd)
            : base(appFrontEnd.AppPlatform)
        {
            m_appFrontEnd = appFrontEnd;
            m_cleanupTimer = new Timer(m_cleanupIntervalSpan.TotalMilliseconds);
            m_cleanupTimer.AutoReset = true;
            m_cleanupTimer.Elapsed += this.CleanupElapsed;
            m_pendingRequests = new LinkedList<CallbackRequest>();
        }

        #endregion

        public AppFrontEnd FrontEnd
        {
            get
            {
                return m_appFrontEnd;
            }
        }

        public void AddCallback(
            Customer requestingCustomer,
            string callbackTargetUri,
            string callbackTargetDisplayName)
        {            
            var request = new CallbackRequest(
                requestingCustomer,
                callbackTargetUri,
                callbackTargetDisplayName);

            System.Threading.ManualResetEvent callbackSetupWaitHandle = new System.Threading.ManualResetEvent(false);
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.Name = "AddCallback";
            sequence.SuccessCompletionReportHandlerDelegate =
                delegate(Exception exception) { callbackSetupWaitHandle.Set(); };
            sequence.FailureCompletionReportHandlerDelegate =
                delegate(Exception exception) { request.Exception = exception; callbackSetupWaitHandle.Set();};
            lock (this.SyncRoot)
            {
                if (this.IsTerminatingTerminated)
                {
                    throw new InvalidOperationException("AddCallback called when CallbackManager is terminating/terminated.");
                }
                this.AddRequest(request);
                AsyncTask startEndpointTask = new AsyncTask(this.StartupUserEndpoint, request);
                sequence.AddTask(startEndpointTask);
                AsyncTask startPresenceView = new AsyncTask(this.StartupPresenceView, request);
                sequence.AddTask(startPresenceView);
            }
            sequence.Start();
            callbackSetupWaitHandle.WaitOne();
            if (request.Exception != null)
            {
                this.RemoveRequest(request);
                throw request.Exception;
            }
        }

        #region Internal and Private methods

        private void StartupPresenceView(AsyncTask task, object state)
        {
            task.DoFinalStep(
                delegate()
                {
                    CallbackRequest callbackRequest = (CallbackRequest)state;
                    var viewSettings = new RemotePresenceViewSettings();
                    viewSettings.SubscriptionMode = RemotePresenceViewSubscriptionMode.Persistent;
                    var presenceView = new RemotePresenceView(callbackRequest.CutomerEndpoint, viewSettings);
                    presenceView.ApplicationContext = callbackRequest;

                    var target = new RemotePresentitySubscriptionTarget(callbackRequest.TargetUri);

                    presenceView.PresenceNotificationReceived += this.PresenceView_NotificationReceived;
                    presenceView.SubscriptionStateChanged += this.PresenceView_SubscriptionStateChanged;

                    callbackRequest.PresenceView = presenceView;

                    presenceView.StartSubscribingToPresentities(
                        new RemotePresentitySubscriptionTarget[] { target });
                    
                });
        }

        private void ShutdownPresenceView(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    CallbackRequest callbackRequest = (CallbackRequest)state;
                    var presenceView = callbackRequest.PresenceView;
                    presenceView.PresenceNotificationReceived -= this.PresenceView_NotificationReceived;
                    presenceView.SubscriptionStateChanged -= this.PresenceView_SubscriptionStateChanged;

                    presenceView.BeginTerminate(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                    delegate()
                                    {
                                        presenceView.EndTerminate(ar);
                                    });
                        },
                        null);
                });
        }


        private void StartupUserEndpoint(AsyncTask task, object state)
        {
            CallbackRequest callbackRequest = (CallbackRequest)state;
            AsyncTask proxyTask = new AsyncTask(m_appFrontEnd.CreateOrGetUserEndpoint, callbackRequest.Customer.UserUri);
            proxyTask.TaskCompleted +=
                delegate(object sender, AsyncTaskCompletedEventArgs e)
                {
                    UserEndpointCreationActionResult result = proxyTask.TaskResult as UserEndpointCreationActionResult;
                    if (result != null)
                    {
                        callbackRequest.MyUserEndpoint = result.MyUserEndpoint;
                    }
                    task.Complete(e.ActionResult.Exception);
                };
            proxyTask.StartTask();
        }

        private void ShutdownUserEndpoint(AsyncTask task, object state)
        {
            CallbackRequest callbackRequest = (CallbackRequest)state;
            AsyncTask proxyTask = new AsyncTask(m_appFrontEnd.RelaseUserEndpoint, callbackRequest.MyUserEndpoint);
            proxyTask.TaskCompleted +=
                delegate(object sender, AsyncTaskCompletedEventArgs e)
                {
                    task.Complete(e.ActionResult.Exception);
                };
            proxyTask.StartTask();
        }

        private void PresenceView_NotificationReceived(object sender, RemotePresentitiesNotificationEventArgs e)
        {
            var presenceView = (RemotePresenceView)sender;
            var request = (CallbackRequest)presenceView.ApplicationContext;
            foreach (var notification in e.Notifications)
            {
                Debug.Assert(notification.PresentityUri.Equals(request.TargetUri, StringComparison.OrdinalIgnoreCase));

                if (notification.AggregatedPresenceState != null)
                {
                    PresenceState state = notification.AggregatedPresenceState;
                    var availability = state.Availability;
                    if (availability == PresenceAvailability.Online ||
                        availability == PresenceAvailability.IdleOnline)
                    {
                        string logmessage = string.Format(CultureInfo.InvariantCulture, "{0} became online", request.TargetUri);
                        this.Logger.Log(Logger.LogLevel.Verbose, logmessage);

                        this.InitiateCallback(request);
                    }

                    break;
                }
            }
        }

        private void InitiateCallback(CallbackRequest request)
        {
            lock (this.SyncRoot)
            {
                if (this.IsTerminatingTerminated)
                {
                    return;
                }                
                this.RemoveRequest(request);
            }

            //Exit the lock to avoid deadlocks.
            m_appFrontEnd.InitiateCallback(request.Customer, request.TargetUri, request.TargetDispalyName);
        }

        private void AddRequest(CallbackRequest request)
        {
            lock (this.SyncRoot)
            {
                m_pendingRequests.AddLast(request);
            }
        }

        private void RemoveRequest(CallbackRequest request)
        {
            lock (this.SyncRoot)
            {
                m_pendingRequests.Remove(request);
            }
            // We need to clean up the request. Since this is simple task, we can do it without sequence.
            AsyncTask cleanupAction = new AsyncTask(this.CleanupRequest, request);
            cleanupAction.StartTask();
        }

        private void PresenceView_SubscriptionStateChanged(object sender, RemoteSubscriptionStateChangedEventArgs e)
        {
            var presenceView = (RemotePresenceView)sender;
            var request = (CallbackRequest)presenceView.ApplicationContext;

            foreach (var pair in e.SubscriptionStateChanges)
            {             
                if (pair.Value.State == RemotePresentitySubscriptionState.Terminating)
                {
                    this.RemoveRequest(request);
                    break; // We have only one target in the view. So, we should see this only once for the target.
                }
            }
        }

        private void CleanupElapsed(object sender, ElapsedEventArgs e)
        {
            if (this.IsTerminatingTerminated)
            {
                return;
            }
            AsyncTaskSequenceParallel requestsToCleanup = new AsyncTaskSequenceParallel(this);
            requestsToCleanup.Name = "CleanupStaleRequests";
            lock (this.SyncRoot)
            {
                var cutoff = DateTime.Now.Subtract(m_maxLifeSpanOfRequest);

                while(m_pendingRequests.First != null && m_pendingRequests.First.Value.CreationTime < cutoff)
                {
                    CallbackRequest request = m_pendingRequests.First.Value;
                    m_pendingRequests.RemoveFirst();
                    AsyncTask cleanupAction = new AsyncTask(this.CleanupRequest, request);
                    cleanupAction.IsOptional = true;
                    requestsToCleanup.AddTask(cleanupAction);
                }
            }
            requestsToCleanup.Start();
        }

        /// <summary>
        /// Clean up a request given as argument in the task.
        /// </summary>
        /// <param name="task">The task for this operation.</param>
        private void CleanupRequest(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    CallbackRequest callbackRequest = (CallbackRequest)state;
                    // We need to terminate view and then endppoint. Endpoint termiantion alone is enough. We will do both here for sample usage.
                    AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
                    sequence.Name = "CleanupCallbackRequest";
                    sequence.FailureCompletionReportHandlerDelegate = delegate(Exception exception) { task.Complete(null); };
                    sequence.SuccessCompletionReportHandlerDelegate = delegate(Exception exception) { task.Complete(exception); };
                    AsyncTask viewShutdownAction = new AsyncTask(this.ShutdownPresenceView, callbackRequest);
                    sequence.AddTask(viewShutdownAction);
                    AsyncTask endpointShutdownAction = new AsyncTask(this.ShutdownUserEndpoint, callbackRequest);
                    sequence.AddTask(endpointShutdownAction);
                    sequence.Start();
                });
        }
        
        /// <summary>
        /// Starts this component. There is nothing to do for CallbackManager.
        /// </summary>
        protected override void StartupCore()
        {
            m_cleanupTimer.Start();
            this.CompleteStartup(null);
        }

        /// <summary>
        /// Shuts down the callback manager. Clean up all pending requests.
        /// </summary>
        protected override void ShutdownCore()
        {
            AsyncTaskSequenceParallel requestsToCleanup = new AsyncTaskSequenceParallel(this);
            requestsToCleanup.Name = "ShutdownCallbackManager";
            requestsToCleanup.SuccessCompletionReportHandlerDelegate = this.CompleteShutdown;
            requestsToCleanup.FailureCompletionReportHandlerDelegate = this.CompleteShutdown;
            m_cleanupTimer.Stop();
            List<CallbackRequest> requestsSnapshot = null;
            lock (this.SyncRoot)
            {
                requestsSnapshot = m_pendingRequests.ToList();
                m_pendingRequests.Clear();
            }

            foreach (CallbackRequest request in requestsSnapshot)
            {
                AsyncTask cleanupAction = new AsyncTask(this.CleanupRequest, request);
                cleanupAction.IsOptional = true; // We don't want one clean up to abort others.
                requestsToCleanup.AddTask(cleanupAction);
            }
            requestsToCleanup.Start();
        }
        #endregion
    }
    #endregion
}
