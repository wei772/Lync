/*=====================================================================
  File:      AppPlatform.cs

  Summary:   Implements the application platform.

***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/



using System;
using System.Collections.Generic;
using Microsoft.Rtc.Signaling;
//using System.Workflow.Runtime;
//using Microsoft.Rtc.Workflow.Activities;
using System.Diagnostics;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class AppPlatform : ComponentBase
    {
        #region Private fields
        private Logger m_logger = new Logger();
        //private WorkflowRuntime m_workflowRuntime;
        private readonly string m_userAgent;
        private readonly string m_applicationId;
        private CollaborationPlatform m_platform;
        private ReverseNumberLookup m_reverseNumberLookup;
        private List<AppFrontEnd> m_frontEnds = new List<AppFrontEnd>();
        private TimerWheel m_timerWheel = new TimerWheel();
        #endregion

        #region Public constructor and methods

        public AppPlatform(string userAgent, string applicationId):base(null)
        {
            m_userAgent = userAgent;
            m_applicationId = applicationId;
        }

        public TimerWheel TimerWheel
        {
            get { return m_timerWheel; }
        }

        public CollaborationPlatform Platform
        {
            get
            {
                return m_platform;
            }
        }

        public ReverseNumberLookup ReverseNumberLookUp
        {
            get 
            { 
                return m_reverseNumberLookup; 
            }
        }

        //public WorkflowRuntime WorkflowRuntime
        //{
        //    get
        //    {
        //        return m_workflowRuntime;
        //    }
        //}

        public override void CompleteStartup(Exception e)
        {
            try
            {
                if (e != null)
                {
                    this.BeginShutdown(
                        ar => this.EndShutdown(ar), null);
                }
            }
            finally
            {
                base.CompleteStartup(e);
            }
        }

        /// <summary>
        /// Gets the logger component for this platform.
        /// </summary>
        public override Logger Logger
        {
            get
            {
                return m_logger;
            }
        }
        #endregion

        #region Protected methods

        protected override void StartupCore()
        {
            if (!ApplicationConfiguration.LoadConfiguration())
            {
                this.CompleteStartup(new InvalidOperationException("Cannot load configuration"));
            }

            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.Name = "StartupAppPlatform";
            sequence.AddTask(new AsyncTask(this.StartupPlatform));
            //sequence.AddTask(new AsyncTask(this.StartWorkflowRuntime));
            sequence.AddTask(new AsyncTask(this.StartupReverseLookup));
            sequence.SuccessCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.FailureCompletionReportHandlerDelegate = this.CompleteStartup;
            sequence.Start();
        }

        protected override void ShutdownCore()
        {
            AsyncTaskSequenceSerial sequence = new AsyncTaskSequenceSerial(this);
            sequence.Name = "ShutdownAppPlatform";
            sequence.AddTask(new AsyncTask(this.ShutdownReverseLookup, null));
            //sequence.AddTask(new AsyncTask(this.ShutdownWorkflowRuntime, null));
            // Shutdown all front ends in parallel. 
            AsyncTaskSequenceParallel shutdownAppFrontEndsSequence = new AsyncTaskSequenceParallel(this);
            AsyncTask shutdownAppFrontEndsAction = new AsyncTask(AsyncTask.SequenceStartingMethod, shutdownAppFrontEndsSequence);
            sequence.AddTask(shutdownAppFrontEndsAction);
            shutdownAppFrontEndsSequence.Name = "ShutdownAppFrontEnds";
            shutdownAppFrontEndsSequence.FailureCompletionReportHandlerDelegate = shutdownAppFrontEndsAction.Complete;
            shutdownAppFrontEndsSequence.SuccessCompletionReportHandlerDelegate = shutdownAppFrontEndsAction.Complete;
            // Populate the parallel sequence with individual customer sessions to terminate.
            lock (this.SyncRoot)
            {
                foreach (AppFrontEnd fe in m_frontEnds)
                {
                    AsyncTask task = new AsyncTask(this.ShutdownFrontEnd, fe);
                    shutdownAppFrontEndsSequence.AddTask(task);
                }
            }

            sequence.AddTask(new AsyncTask(this.ShutdownCollaborationPlatform));
            sequence.SuccessCompletionReportHandlerDelegate = this.CompleteShutdown;
            sequence.FailureCompletionReportHandlerDelegate = this.CompleteShutdown;
            sequence.Start();
        }
        #endregion

        #region Private methods
        private void ApplicationEndpointOwnerDiscovered(object sender, ApplicationEndpointSettingsDiscoveredEventArgs e)
        {
            AppFrontEnd frontEnd = null;

            if (this.IsTerminatingTerminated)
            {
                return;
            }
            lock (this.SyncRoot)
            {
                this.Logger.Log(Logger.LogLevel.Info, "A new endpoint was detected : " + e.ApplicationEndpointSettings.OwnerUri);
                frontEnd = new AppFrontEnd(this, e.ApplicationEndpointSettings);
                this.AddFrontEnd(frontEnd);
            }

            Debug.Assert(frontEnd != null, "frontEnd should not be null");

            try
            {
                frontEnd.BeginStartup(
                    ar =>
                    {
                        try
                        {
                            frontEnd.EndStartup(ar);
                        }
                        catch (RealTimeException ex)
                        {
                            
                            this.Logger.Log(Logger.LogLevel.Error,"Failed to start front end : ", ex);                                                                
                            this.RemoveFrontEnd(frontEnd);
                        }
                    }, null);
            }
            catch (InvalidOperationException ex)
            {
                this.Logger.Log(Logger.LogLevel.Error,"Failed to start front end : ", ex);
                this.RemoveFrontEnd(frontEnd);
            }
        }
        //Commented the functions used to start and stop workflow runtime service as workflows are not to be used
        /*
        private void StartWorkflowRuntime(AsyncTask task, object state)
        {
            m_workflowRuntime = new WorkflowRuntime();
            m_workflowRuntime.AddService(new CommunicationsWorkflowRuntimeService());
            m_workflowRuntime.AddService(new TrackingDataWorkflowRuntimeService());

            task.DoFinalStep(
                delegate()
                {
                    m_workflowRuntime.StartRuntime();
                    this.Logger.Log(Logger.LogLevel.Info, "Workflow started.");
                });
        }
       
        private void ShutdownWorkflowRuntime(AsyncTask task, object state)
        {
            if (m_workflowRuntime == null)
            {
                task.Complete(null);
                return;
            }
            task.DoFinalStep(
                delegate()
                {
                    m_workflowRuntime.StopRuntime();
                    this.Logger.Log(Logger.LogLevel.Info, "Workflow shutdown.");
                });
        }
 */
        private void StartupPlatform(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    var settings = new ProvisionedApplicationPlatformSettings(m_userAgent, m_applicationId);
                    m_platform = new CollaborationPlatform(settings);
                    m_platform.RegisterForApplicationEndpointSettings(this.ApplicationEndpointOwnerDiscovered);

                    //this.Logger.Log(Logger.LogLevel.Info, "Starting the Collaboration Platform.");
                    m_platform.BeginStartup(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_platform.EndStartup(ar);
                                    //this.Logger.Log(Logger.LogLevel.Info, "Collaboration Platform started.");
                                });
                        }, 
                        null);
                });
        }

        private void StartupReverseLookup(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    m_reverseNumberLookup = ReverseNumberLookup.GetLookupInstance(this);
                    //this.Logger.Log(Logger.LogLevel.Info, "Starting Reverse Number Lookup");
                    m_reverseNumberLookup.BeginStartup(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_reverseNumberLookup.EndStartup(ar);
                                    //this.Logger.Log(Logger.LogLevel.Info, "Reverse Number Lookup started");
                                });
                        }, 
                        null);
                });
        }

        private void ShutdownReverseLookup(AsyncTask task, object state)
        {
            ReverseNumberLookup rnl = m_reverseNumberLookup;
            if (rnl == null)
            {
                task.Complete(null);
                return;
            }
            task.DoOneStep(
                delegate()
                {
                    this.Logger.Log(Logger.LogLevel.Info, "Shuttong down Reverse Number Lookup.");
                    rnl.BeginShutdown(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    rnl.EndShutdown(ar);
                                    this.Logger.Log(Logger.LogLevel.Info, "Reverse Number Lookup shutdown.");
                                });
                        }, 
                        null);
                });
        }

        private void ShutdownFrontEnd(AsyncTask task, object state)
        {
            task.DoOneStep(
                delegate()
                {
                    AppFrontEnd frontEnd = (AppFrontEnd)state;
                    this.Logger.Log(Logger.LogLevel.Info, String.Format("Sutting down FrontEnd {0}.", frontEnd.Endpoint.OwnerUri));
                    frontEnd.BeginShutdown(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    frontEnd.EndShutdown(ar);
                                    this.Logger.Log(Logger.LogLevel.Info, String.Format("FrontEnd {0} shutdown.", frontEnd.Endpoint.OwnerUri));
                                });
                        },
                        null);
                });
        }

        private void ShutdownCollaborationPlatform(AsyncTask task, object state)
        {
            CollaborationPlatform platform = m_platform;
            if (platform == null)
            {
                task.Complete(null);
                return;
            }
            task.DoOneStep(
                delegate()
                {
                    m_platform.BeginShutdown(
                        delegate(IAsyncResult ar)
                        {
                            task.DoFinalStep(
                                delegate()
                                {
                                    m_platform.EndShutdown(ar);
                                    this.Logger.Log(Logger.LogLevel.Info, "Platform shutdown completed.");
                                });

                        }, 
                        null);
                });
        }

        private void AddFrontEnd(AppFrontEnd frontEnd)
        {
            Debug.Assert(frontEnd != null, "front end should not be null");

            lock (this.SyncRoot)
            {
                m_frontEnds.Add(frontEnd);
            }
        }

        private void RemoveFrontEnd(AppFrontEnd frontEnd)
        {
            Debug.Assert(frontEnd != null, "front end should not be null");

            lock (this.SyncRoot)
            {
                m_frontEnds.Remove(frontEnd);
            }
        }
        #endregion
    }
}