
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Security.Cryptography.X509Certificates;


using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities
{
    /// <summary>
    /// Represents a class to encapsulate ucma platform and endpoint initialization and termination.
    /// </summary>
    internal class UcmaHelper
    {
        #region private variables
        /// <summary>
        /// Collaboration platform instance
        /// </summary>
        private CollaborationPlatform m_collabPlatform;

        /// <summary>
        /// Application endpoint instance
        /// </summary>
        private ApplicationEndpoint m_applicationEndpoint;

        /// <summary>
        /// Used for dispose implementation
        /// </summary>
        private bool m_disposed;

        /// <summary>
        /// Helper wait handle to exit the application when all operations are done or when an error occurs.
        /// </summary>
        private ManualResetEvent m_allDone = new ManualResetEvent(false/*initialState*/);

        /// <summary>
        /// Maximum timeout is 40 seconds.
        /// </summary>
        private const int MaxTimeoutInMillis = 40000;

        private static UcmaHelper StaticInstance;

        private bool m_cleanupNeeded = true;

        private object m_syncRoot = new object();

        private TimerWheel m_timerWheel = new TimerWheel();

        #endregion

        #region string constants
        /// <summary>
        /// Local host constant
        /// </summary>
        private const String ApplicationName = "ContactCenterWcfService";

        /// <summary>
        /// Local host constant
        /// </summary>
        private const String LocalHost = "localhost";
        #endregion

        #region static methods

        /// <summary>
        /// Gets static instance.
        /// </summary>
        /// <returns>static instance.</returns>
        public static UcmaHelper GetInstance()
        {
            lock (typeof(UcmaHelper))
            {
                if (StaticInstance == null)
                {
                    StaticInstance = new UcmaHelper();
                }
                return StaticInstance;
            }
        }
        #endregion

        #region constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private UcmaHelper()
        {
        }

        #endregion


        #region public properties

        /// <summary>
        /// Gets the application endpoint.
        /// </summary>
        public ApplicationEndpoint ApplicationEndpoint
        {
            get
            {
                return m_applicationEndpoint;
            }
        }

        /// <summary>
        /// Gets the timer wheel.
        /// </summary>
        public TimerWheel TimerWheel 
        {
            get 
            {
                return m_timerWheel;
            }
        }
        #endregion

        #region protected methods

        /// <summary>
        /// Dispose ucma helper.
        /// </summary>
        /// <param name="disposing">True => clean up managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    ManualResetEvent waitHandle = m_allDone;
                    m_allDone = null;
                    if (waitHandle != null)
                    {
                        waitHandle.Close();
                    }
                    TimerWheel timerWheel = m_timerWheel;
                    if(timerWheel != null) 
                    {
                        timerWheel.Dispose();
                    }
                    lock (typeof(UcmaHelper))
                    {
                        StaticInstance = null;
                    }
                }
                m_cleanupNeeded = false;
                m_disposed = true;
            }
        }

        #endregion

        #region public methods


        /// <summary>
        /// Gets the contact center trusted gruu.
        /// </summary>
        /// <returns>Can return null.</returns>
        public string GetContactCenterTrustedGruu() 
        {
            string contactCenterGruu = null;
            if (!String.IsNullOrEmpty(Configuration.ContactCenterApplicationId))
            {
                TopologyConfiguration topologyConfig = m_collabPlatform.TopologyConfiguration;
                ApplicationTopologyData wcfApplicationTopologyData = m_collabPlatform.ApplicationTopologyData;
                if (topologyConfig != null && wcfApplicationTopologyData != null)
                {
                    Collection<ApplicationTopologyData> applicationTopologyDataCollection = topologyConfig.GetApplicationTopologyData(Configuration.ContactCenterApplicationId);
                    if (applicationTopologyDataCollection.Count == 0)
                    {
                        Helper.Logger.Error("Unable to retrieve application topology data for the contact center application. Please make sure contact center application is configured properly.");
                    }
                    else
                    {
                        //Match the contact center application in the same site.
                        foreach (ApplicationTopologyData appData in applicationTopologyDataCollection)
                        {
                            if (appData.SiteId.Equals(wcfApplicationTopologyData.SiteId))
                            {
                                contactCenterGruu = appData.PoolGruu;
                                Helper.Logger.Info("Successfully retrieved contact center address.");
                                break;
                            }
                        }

                    }
                }
                else
                {
                    Helper.Logger.Error("Unable to retrieve topology configuration from the platform. Please make sure Microsoft Lync Server data replication is complete.");
                }
            }
            else
            {
                Helper.Logger.Error("Configuration does not contain a valid contact center application id. Please configure the contact center application id in web.config file.");
            }

            return contactCenterGruu;
        }


        /// <summary>
        /// Method to shutdown and clean up collaboration platform.
        /// </summary>
        public void Stop()
        {
            lock (m_syncRoot)
            {
                if (m_cleanupNeeded)
                {
                    m_cleanupNeeded = false;
                    Helper.Logger.Info("Stopping ucma platform");
                    var allDone = m_allDone;
                    if (allDone != null)
                    {
                        allDone.Reset();
                    }
                    this.Cleanup();

                    //Wait for all operations to complete.
                    if (allDone != null)
                    {
                        bool succeeded = m_allDone.WaitOne(UcmaHelper.MaxTimeoutInMillis, false/*exitContext*/);
                        if (!succeeded)
                        {
                            Helper.Logger.Info("Shutdown did not complete in expected time.");
                        }
                        allDone.Close();
                    }

                    TimerWheel timerWheel = m_timerWheel;
                    if (timerWheel != null)
                    {
                        timerWheel.Dispose();
                    }
                    lock (typeof(UcmaHelper))
                    {
                        StaticInstance = null;
                    }
                }
            }
        }

        /// <summary>
        /// Method to start platform and endpoint initialization.
        /// </summary>
        public void Start()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException("Please restart");
            }

            string applicationId = Configuration.ApplicationId;

            if (!String.IsNullOrEmpty(applicationId))
            {
                ProvisionedApplicationPlatformSettings settings
                        = new ProvisionedApplicationPlatformSettings(UcmaHelper.ApplicationName, applicationId);

                //Create the platform.
                m_collabPlatform = new CollaborationPlatform(settings);


                //Populate im flow template.
                InstantMessagingFlowTemplate imFlowTemplate = new InstantMessagingFlowTemplate();
                imFlowTemplate.ComposingTimeoutValue = 10; //seconds
                imFlowTemplate.SupportedFormats = InstantMessagingFormat.PlainText | InstantMessagingFormat.HtmlText;
                imFlowTemplate.MessageConsumptionMode = InstantMessageConsumptionMode.ProxiedToRemoteEntity;
                imFlowTemplate.ToastFormatSupport = CapabilitySupport.UnSupported;
                m_collabPlatform.InstantMessagingSettings = imFlowTemplate;

                // Wire up a handler for the ApplicationEndpointOwnerDiscovered event.
                m_collabPlatform.RegisterForApplicationEndpointSettings(this.Platform_ApplicationEndpointOwnerDiscovered);

                bool needCleanup = true;
                try
                {
                    m_collabPlatform.BeginStartup(this.PlatformStartupCompleted, m_collabPlatform);
                    needCleanup = false;
                }
                catch (InvalidOperationException ioe)
                {
                    Helper.Logger.Error("Exception caught during startup {0}", EventLogger.ToString(ioe));
                }
                finally
                {
                    if (needCleanup)
                    {
                        this.Cleanup();
                    }
                }
                //Wait for all operations to complete.
                var allDone = m_allDone;
                if (allDone != null)
                {
                    bool succeeded = m_allDone.WaitOne(UcmaHelper.MaxTimeoutInMillis, false/*exitContext*/);
                    if (!succeeded)
                    {
                        Helper.Logger.Error("Initialization did not complete in expected time.");
                    }
                }
            }
            else
            {
                Helper.Logger.Error("Invalid application id. Please provide a valid value for application id in the web.config file.");
            }
        }
        #endregion

        #region private methods


        /// <summary>
        /// Helper method to clean up every thing and exit.
        /// </summary>
        private void Cleanup()
        {
            CollaborationPlatform platform = m_collabPlatform;
            bool allDone = true;
            try
            {

                if (platform != null)
                {
                    platform.BeginShutdown(this.PlatformShutdownCompleted, platform);
                    allDone = false;
                }
            }
            finally
            {
                var allDoneEventHandle = m_allDone;
                if (allDone && allDoneEventHandle != null)
                {
                    allDoneEventHandle.Set();
                }
            }
        }


        // Registered event handler for the ApplicationEndpointOwnerDiscovered event on the
        // CollaborationPlatform for the provisioned application.
        private void Platform_ApplicationEndpointOwnerDiscovered(object sender,
            ApplicationEndpointSettingsDiscoveredEventArgs e)
        {
            Helper.Logger.Info("ApplicationEndpointOwnerDiscovered event was raised during startup of the "
                + "CollaborationPlatform.");

            ApplicationEndpointSettings settings = e.ApplicationEndpointSettings;

            settings.UseRegistration = false;

            m_applicationEndpoint = new ApplicationEndpoint(m_collabPlatform, settings);

            bool epBeginEstablishFailed = true;
            try
            {
                this.RegisterEndpointEventHandlers(m_applicationEndpoint);
                m_applicationEndpoint.BeginEstablish(this.EndpointEstablishCompleted, m_applicationEndpoint);
                epBeginEstablishFailed = false;
            }
            catch (InvalidOperationException ioe)
            {
                Helper.Logger.Error("Endpoint establishment operation threw an exception {0}", EventLogger.ToString(ioe));
            }
            finally
            {
                if (epBeginEstablishFailed)
                {
                    this.Cleanup();
                }
            }
        }


        /// <summary>
        /// Callback method for platform startup. This method will initiate endpoint establishment.
        /// </summary>
        /// <param name="result">Async result</param>
        private void PlatformStartupCompleted(IAsyncResult result)
        {
            CollaborationPlatform collabPlatform = result.AsyncState as CollaborationPlatform;
            bool needCleanup = true;
            try
            {
                collabPlatform.EndStartup(result);
                Helper.Logger.Info("The platform is now started.");
                needCleanup = false;
            }
            catch (RealTimeException rte)
            {
                Helper.Logger.Error("Platform End startup threw an exception {0}", EventLogger.ToString(rte));
            }
            finally
            {
                if (needCleanup)
                {
                    this.Cleanup();
                }
            }
        }

        /// <summary>
        /// Callback method for platform shutdown method
        /// </summary>
        /// <param name="result">Async result</param>
        private void PlatformShutdownCompleted(IAsyncResult result)
        {
            CollaborationPlatform collabPlatform = result.AsyncState as CollaborationPlatform;

            try
            {
                //Shutdown actions will not throw.
                collabPlatform.EndShutdown(result);
            }
            finally
            {
                Helper.Logger.Info("The platform is now shutdown.");
                var allDone = m_allDone;
                if (allDone != null)
                {
                    allDone.Set();
                }
            }
        }

        /// <summary>
        /// Register endpoint event handlers.
        /// </summary>
        /// <param name="localEndpoint">Local endpoint.</param>
        private void RegisterEndpointEventHandlers(LocalEndpoint localEndpoint)
        {
            Debug.Assert(localEndpoint != null, "Local endpoint is null");
            localEndpoint.StateChanged += this.Endpoint_StateChanged;
        }

        /// <summary>
        /// Unregister endpoint event handlers.
        /// </summary>
        /// <param name="localEndpoint">Local endpoint.</param>
        private void UnregisterEndpointEventHandlers(LocalEndpoint localEndpoint)
        {
            Debug.Assert(localEndpoint != null, "Local endpoint is null");
            localEndpoint.StateChanged -= this.Endpoint_StateChanged;
        }


        /// <summary>
        /// Endpoint state changed event handler
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void Endpoint_StateChanged(object sender, LocalEndpointStateChangedEventArgs e)
        {
            LocalEndpoint endpoint = sender as LocalEndpoint;
            Helper.Logger.Info("Endpoint {0} state changed from {1} to {2}. Reason = {3}.", endpoint.OwnerUri, e.PreviousState, e.State, e.Reason);

            if (e.State == LocalEndpointState.Terminating)
            {
                //Unregister event handlers.
                this.UnregisterEndpointEventHandlers(endpoint);
            }
        }

        /// <summary>
        /// Callback method for endpoint establishment completion operation.
        /// </summary>
        /// <param name="result">Async result</param>
        private void EndpointEstablishCompleted(IAsyncResult result)
        {
            ApplicationEndpoint appEndpoint = result.AsyncState as ApplicationEndpoint;
            bool needCleanup = true;
            try
            {
                appEndpoint.EndEstablish(result);

                Helper.Logger.Info("Application Endpoint successfully established");
                needCleanup = false;
            }
            catch (InvalidOperationException ioe)
            {
                Helper.Logger.Error("Endpoint Endpoint operation threw an exception {0}", EventLogger.ToString(ioe));
            }
            catch (RealTimeException rte)
            {
                Helper.Logger.Error("Endpoint Endpoint operation threw an exception {0}", EventLogger.ToString(rte));
            }
            finally
            {
                if (needCleanup)
                {
                    this.Cleanup();
                }
                else
                {
                    var allDone = m_allDone;
                    if (allDone != null)
                    {
                        allDone.Set();
                    }
                }
            }
        }
        #endregion

    }
}
