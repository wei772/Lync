/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpServer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Net.Mime;
    using System.Text;
    using System.Threading;
    using FastHelpCore;
    using Microsoft.Rtc.Collaboration;
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using Microsoft.Rtc.Signaling;
    using FastHelp.Logging;
    using System.Net;

    /// <summary>
    /// UCMA BOT for Call and IM handling
    /// </summary>
    public sealed class FastHelpServerApp
    {

        #region private properties

        /// <summary>
        /// Application Id of the BOT
        /// </summary>
        private string applicationId;

        /// <summary>
        /// Application name of the BOT
        /// </summary>
        private string applicationName;

        /// <summary>
        /// trustedContactUri of the BOT
        /// </summary>
        private string trustedContactUri;

        /// <summary>
        /// The Lync Server that the user listed will log in to
        /// </summary>
        private string lyncServer = "lync-se.fabrikam.com";

        /// <summary>
        /// The Lync Server that the user listed will log in to
        /// </summary>
        private string cweGuid;

        /// <summary>
        /// Collaboration Platform
        /// </summary>
        private CollaborationPlatform collabPlatform;

        /// <summary>
        /// Application endpoint
        /// </summary>
        // private ApplicationEndpoint applicationEndpoint;
        private LocalEndpoint applicationEndpoint;

        /// <summary>
        /// Service Gruu
        /// </summary>
        private string gruu;

        /// <summary>
        /// Application port
        /// </summary>
        private int port;

        /// <summary>
        /// Certificate Name used to authenticating application endpoint
        /// </summary>
        private string certificateName;

        /// <summary>
        /// Type of presence
        /// </summary>
        private const string ApplicationPresentityType = "automaton";

        /// <summary>
        /// Description for presenece
        /// </summary>
        private const string ApplicationPresentityTypeDescription = "AlwaysOnlineBot";

        /// <summary>
        /// XmlParser object for parsing IVR menu xml
        /// </summary>
        private XmlParser xmlParser;

        /// <summary>
        /// Specifies to register bot as user endpoint
        /// </summary>
        private bool useUserEndPoint;

        /// <summary>
        /// Config change: Specifies whether to use Auto Provisioning for Applcation end point.
        /// </summary>
        private bool useApplicationAutoProvisioning;

        /// <summary>
        /// Web address which has more information on the bot
        /// </summary>
        private string moreInformationLink;

        /// <summary>
        /// Registry file for CWE
        /// </summary>
        private string registryFilePath;

        /// <summary>
        /// Message for more information
        /// </summary>
        private string moreInformationMessage;

        /// <summary>
        ///  Message for CWE
        /// </summary>
        private string cweMessage;

        /// <summary>
        /// Lock object.
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// For Logging  
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Dictionary to save user state
        /// </summary>
        private readonly Dictionary<string, CustomerSession> activeCustomerSessions = new Dictionary<string, CustomerSession>(300, StringComparer.OrdinalIgnoreCase);

        // Transport type used to communicate with your Microsoft Lync Server instance.
        private Microsoft.Rtc.Signaling.SipTransportType transportType = Microsoft.Rtc.Signaling.SipTransportType.Tls;


        #endregion

        #region public properties

        /// <summary>
        /// Gets the name of this application.
        /// </summary>
        public string Name
        {
            get { return this.applicationName; }
        }

        /// <summary>
        /// Gets the converastion window extension guid to use.
        /// </summary>
        public string CweGuid
        {
            get { return this.cweGuid; }
        }

        /// <summary>
        /// Gets the xml parser.
        /// </summary>
        public XmlParser XmlParser
        {
            get { return this.xmlParser; }
        }
        #endregion


        #region public methods.
        /// <summary>
        /// Setups the UCMA platform.
        /// </summary>
        public void Start()
        {

            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ApplicationId"]) ||
               string.IsNullOrEmpty(ConfigurationManager.AppSettings["ApplicationName"]) ||
               string.IsNullOrEmpty(ConfigurationManager.AppSettings["TrustedContactURI"]) ||
               string.IsNullOrEmpty(ConfigurationManager.AppSettings["LyncServerName"]) ||
               string.IsNullOrEmpty(ConfigurationManager.AppSettings["ApplicationPort"]) ||
               string.IsNullOrEmpty(ConfigurationManager.AppSettings["Gruu"])||
               string.IsNullOrEmpty(ConfigurationManager.AppSettings["CertificateName"]))

            {
                throw new ArgumentNullException("applicationId", "Please provide values for ApplicationId,ApplicationName, TrustedContactURI in app.config.");
            }

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(currentDomain_UnhandledException);



            this.applicationId = ConfigurationManager.AppSettings["ApplicationId"];
            this.applicationName = ConfigurationManager.AppSettings["ApplicationName"];
            this.trustedContactUri = ConfigurationManager.AppSettings["TrustedContactURI"];
            this.lyncServer = ConfigurationManager.AppSettings["LyncServerName"];
            this.gruu = ConfigurationManager.AppSettings["Gruu"];
            this.port = int.Parse(ConfigurationManager.AppSettings["ApplicationPort"]);
            this.certificateName = ConfigurationManager.AppSettings["CertificateName"];

            this.cweGuid = ConfigurationManager.AppSettings["CWEGuid"];

            this.useUserEndPoint = Boolean.Parse(ConfigurationManager.AppSettings["UseUserEndPoint"]);

            // Config change: Flag for conditional Auto Provisioning.
            this.useApplicationAutoProvisioning = Boolean.Parse(ConfigurationManager.AppSettings["useApplicationAutoProvisioning"]);

            this.moreInformationLink = ConfigurationManager.AppSettings["MoreInformationLink"];

            this.registryFilePath = ConfigurationManager.AppSettings["CWERegistryFilePath"];

            this.moreInformationMessage = ConfigurationManager.AppSettings["MoreInformationMessage"];

            this.cweMessage = ConfigurationManager.AppSettings["CWEMessage"];

            this.logger = new EventLogger(this.applicationName);
            
            this.StartPlatform();

            this.xmlParser = new XmlParser();
            this.xmlParser.FetchXml();


            this.logger.Log("The application endpoint is now established and registered.Waiting for audio call.");
            Console.WriteLine("The application endpoint is now established and registered.");
            Console.WriteLine("Waiting for audio call.");

        }

        /// <summary>
        /// Terminate endpoints when BOT closes
        /// </summary>
        public void Stop()
        {
            var platform = this.collabPlatform;
            if (platform != null)
            {
                this.logger.Log("Shutown started");
                Console.WriteLine("Shutdown started");

                var endpoint = this.applicationEndpoint;
                if (endpoint != null)
                {
                    endpoint.UnregisterForIncomingCall<AudioVideoCall>(this.AV_Received);
                    endpoint.UnregisterForIncomingCall<InstantMessagingCall>(this.IM_Received);
                }

                platform.EndShutdown(platform.BeginShutdown(null, null));
                this.logger.Log("Shutown completed");
                Console.WriteLine("Shutdown completed");
            }
            else
            {
                this.logger.Log("Shutdown completed");
                Console.WriteLine("Shutdown completed");
            }
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Unbinds customer session.
        /// </summary>
        internal void UnbindCustomerSesssion(CustomerSession customerSession)
        {
            lock (activeCustomerSessions)
            {
                activeCustomerSessions.Remove(customerSession.CustomerUri);
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Setup Collaboration Platform
        /// </summary>
        private void StartPlatform()
        {
            try
            {
                if (!useUserEndPoint)
                {
                    string localhost = Dns.GetHostEntry("localhost").HostName;
                    
                    // Config change: for conditional Auto Provisioning.
                    if (this.useApplicationAutoProvisioning)
                    {
                        ProvisionedApplicationPlatformSettings appSettings = new ProvisionedApplicationPlatformSettings("fasthelp", this.applicationId);
                        this.collabPlatform = new CollaborationPlatform(appSettings);
                        this.collabPlatform.RegisterForApplicationEndpointSettings(
                                this.Platform_ApplicationEndpointOwnerDiscovered);
                    }
                    else
                    {
                        ServerPlatformSettings platform = new ServerPlatformSettings(this.applicationName, localhost, this.port, this.gruu, CertificateHelper.GetLocalCertificate(this.certificateName));
                        platform.TrustedDomains.Add(new TrustedDomain(".fabrikam.com"));
                        this.collabPlatform = new CollaborationPlatform(platform);
                    }
                    Console.WriteLine("Publishing Presence");
                }
                else
                {
                    // Initalize and startup the platform.
                    ClientPlatformSettings clientPlatformSettings =
                        new ClientPlatformSettings(this.applicationName, this.transportType);

                    this.collabPlatform = new CollaborationPlatform(clientPlatformSettings);
                }

                this.logger.Log("Starting the platform.");
                Console.WriteLine("Starting the platform.");
                this.collabPlatform.EndStartup(this.collabPlatform.BeginStartup(null, null));
                Console.WriteLine("Starting the platform complete.");

                // Config change: for conditional Auto Provisioning.
                if (!this.useApplicationAutoProvisioning)
                    this.StartEndpoint();
            }
            catch (InvalidOperationException ioe)
            {
                Console.WriteLine(ioe.ToString());
            }
            catch (ConnectionFailureException connFailEx)
            {
                // ConnectionFailureException will be thrown when the platform cannot connect
                Console.WriteLine(connFailEx.ToString());
                this.logger.Log("Connection Failure Exception {0}", connFailEx);
            }
            catch (ProvisioningFailureException provFailEx)
            {
                // ProvisioningFailureException will be thrown when the platform cannot find the trusted application
                //  entry per the application ID passed in ProvisionedApplicationPlatformSettings.
                Console.WriteLine(provFailEx.ToString());
                this.logger.Log("Provisioning Failure Exception {0}", provFailEx);
            }
            catch (RealTimeException rte)
            {
                Console.WriteLine(rte);
                this.logger.Log("Platform failed to start {0}", rte);
            }
        }


        //#region Presence
        //// Registered event handler for the ApplicationEndpointOwnerDiscovered
        //// event on the CollaborationPlatform for the provisioned application.
        private void Platform_ApplicationEndpointOwnerDiscovered(object sender,
            ApplicationEndpointSettingsDiscoveredEventArgs e)
        {
            collabPlatform.UnregisterForApplicationEndpointSettings(Platform_ApplicationEndpointOwnerDiscovered);
            ApplicationEndpointSettings settings = e.ApplicationEndpointSettings;          

            this.logger.Log("Publishing Presence");
            //Set the endpoint presence to appear as always online
            InitializePublishAlwaysOnlineSettings(settings);
            this.applicationEndpoint = new ApplicationEndpoint(this.collabPlatform, settings);

            this.applicationEndpoint.InnerEndpoint.AddFeatureParameter("isAcd");

            this.applicationEndpoint.RegisterForIncomingCall<AudioVideoCall>(this.AV_Received);
            this.applicationEndpoint.RegisterForIncomingCall<InstantMessagingCall>(this.IM_Received);

            this.logger.Log("Establishing the endpoint.");
            this.applicationEndpoint.EndEstablish(this.applicationEndpoint.BeginEstablish(null, null));
        }

        //// Configures ApplicationEndpoint created from these settings to publish
        //// its presence state as always online.
        private void InitializePublishAlwaysOnlineSettings(ApplicationEndpointSettings settings)
        {
            settings.AutomaticPresencePublicationEnabled = true;
            settings.Presence.PresentityType = ApplicationPresentityType;
            settings.Presence.Description = ApplicationPresentityTypeDescription;
        }
        //#endregion


        /// <summary>
        /// Initialize application endpoint
        /// </summary>
        private void StartEndpoint()
        {
            try
            {
                if (!useUserEndPoint)
                {
                    this.logger.Log("Setting up application end point");

                   
                    var settings = new ApplicationEndpointSettings(
                        ownerUri: this.trustedContactUri,
                        proxyHost: this.lyncServer,
                        proxyPort: 0);

                    settings.IsDefaultRoutingEndpoint = true;
                    settings.AutomaticPresencePublicationEnabled = true;
                    settings.Presence.PresentityType = FastHelpServerApp.ApplicationPresentityType;
                    settings.Presence.Description = FastHelpServerApp.ApplicationPresentityTypeDescription;

                    this.applicationEndpoint = new ApplicationEndpoint(this.collabPlatform, settings);
                }
                else
                {
                    this.logger.Log("Setting up user end point");

                    var userURI = ConfigurationManager.AppSettings["UserURI"];
                    var userName = ConfigurationManager.AppSettings["UserName"];
                    var userPassword = ConfigurationManager.AppSettings["Password"];
                    var userDomain = ConfigurationManager.AppSettings["UserDomain"];
                    var poolFQDN = ConfigurationManager.AppSettings["PoolFQDN"];

                    var userEndpointSettings = new UserEndpointSettings(userURI, poolFQDN);

                    userEndpointSettings.AutomaticPresencePublicationEnabled = true;

                    if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userPassword))
                    {
                        userEndpointSettings.Credential = System.Net.CredentialCache.DefaultNetworkCredentials;
                    }
                    else
                    {
                        var credential = new System.Net.NetworkCredential(userName, userPassword, userDomain);
                        userEndpointSettings.Credential = credential;
                    }

                    this.applicationEndpoint = new UserEndpoint(this.collabPlatform, userEndpointSettings);
                }

                this.applicationEndpoint.InnerEndpoint.AddFeatureParameter("isAcd");

                this.applicationEndpoint.RegisterForIncomingCall<AudioVideoCall>(this.AV_Received);
                this.applicationEndpoint.RegisterForIncomingCall<InstantMessagingCall>(this.IM_Received);

                this.logger.Log("Establishing the endpoint.");
                this.applicationEndpoint.EndEstablish(this.applicationEndpoint.BeginEstablish(null, null));

            }
            catch (InvalidOperationException ioe)
            {
                // InvalidOperationException will be thrown when the platform isn't started or the endpoint isn't established
                this.logger.Log("Invalid Operation Exception {0}", ioe);
                Console.WriteLine(ioe);
            }
            catch (ConnectionFailureException connFailEx)
            {
                // ConnectionFailureException will be thrown when the endpoint cannot connect to the server, 
                //or the credentials are invalid.
                this.logger.Log("Connection Failure Exception {0}", connFailEx);
                Console.WriteLine(connFailEx);
            }
            catch (RegisterException regEx)
            {
                // RegisterException will be thrown when the endpoint cannot be registered (usually due to bad credentials).
                this.logger.Log("Register Exception  {0}", regEx);
                Console.WriteLine(regEx);
            }
            catch (AuthenticationException ae)
            {
                // AuthenticationException will be thrown when a general authentication-related problem occurred.
                this.logger.Log("Authentication Exception  {0}", ae);
                Console.WriteLine(ae);
            }
            catch (OperationTimeoutException ate)
            {
                // OperationTimeoutException will be thrown when server did not respond for Register request.
                this.logger.Log("Operation Timeout Exception {0}", ate);
                Console.WriteLine(ate);
            }
            catch (RealTimeException rte)
            {
                this.logger.Log("Operation Timeout Exception {0}", rte);
                Console.WriteLine(rte);
            }
        }

        /// <summary>
        /// Accept AV.
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">AudioVideoCall arguments</param>
        private void AV_Received(object sender, CallReceivedEventArgs<AudioVideoCall> e)
        {
            bool handled = false;
            if (e.IsConferenceDialOut)
            {
                if (e.Call.Conversation.Id != null)
                {
                    string customerUri = e.Call.Conversation.Id.Trim('>', '<');
                    lock (this.activeCustomerSessions)
                    {
                        if (activeCustomerSessions.ContainsKey(customerUri))
                        {
                            activeCustomerSessions[customerUri].HandleIncomingDialOutCall(e.Call);
                            handled = true;
                        }
                    }
                }
            }
            else if (e.IsNewConversation)
            {
                if (e.Call.Conversation.Id != null)
                {
                    string customerUri = e.Call.Conversation.Id.Trim('>', '<');
                    lock (this.activeCustomerSessions)
                    {
                        if (activeCustomerSessions.ContainsKey(customerUri))
                        {
                            activeCustomerSessions[customerUri].HandleIncomingAudioCall(e.Call);
                            handled = true;
                        }
                        else
                        {
                            string uri = e.Call.RemoteEndpoint.Uri;
                            Customer customer = new Customer(uri);
                            Menu menu = new Menu(this.moreInformationMessage, this.moreInformationLink, this.cweMessage, this.registryFilePath, this.XmlParser);
                            CustomerSession customerSession = new CustomerSession(customer, menu, this, this.logger);
                            this.activeCustomerSessions.Add(e.Call.RemoteEndpoint.Uri, customerSession);
                            customerSession.HandleIncomingAudioCall(e.Call);
                            handled = true;
                        }
                    }
                }
            }


            if (!handled)
            {
                try
                {
                    this.logger.Log("Declining AV call since this is a modality escalating call.");
                    Console.WriteLine("Declining AV call since this is a modality escalating call.");
                    e.Call.Decline();
                }
                catch (InvalidOperationException ioe)
                {
                    this.logger.Log("Exception while declining call {0}", ioe);
                    Console.WriteLine("Exception while declining call {0}", ioe);
                }
                catch (RealTimeException rte)
                {
                    this.logger.Log("Exception while declining call {0}", rte);
                    Console.WriteLine("Exception while declining call {0}", rte);
                }
            }

        }

        /// <summary>
        /// Accept IM from Lync client
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">InstantMessagingCall arguments</param>
        private void IM_Received(object sender, CallReceivedEventArgs<InstantMessagingCall> e)
        {
            bool handled = false;
            if (e.IsNewConversation)
            {
                lock (this.activeCustomerSessions)
                {
                    if(!this.activeCustomerSessions.ContainsKey(e.Call.RemoteEndpoint.Uri))
                    {
                        Customer customer = new Customer(e.Call.RemoteEndpoint.Uri);
                        Menu menu = new Menu(this.moreInformationMessage, this.moreInformationLink, this.cweMessage, this.registryFilePath, this.XmlParser);
                        CustomerSession customerSession = new CustomerSession(customer, menu, this, this.logger);
                        this.activeCustomerSessions.Add(e.Call.RemoteEndpoint.Uri, customerSession);
                        customerSession.HandleIncomingInstantMessagingCall(e.Call);
                        handled = true;
                    }
                }
            }


            if(!handled)
            {
                try
                {
                    this.logger.Log("Declining IM call since this is a modality escalating call.");
                    Console.WriteLine("Declining IM call since this is a modality escalating call.");
                    e.Call.Decline();
                }
                catch (InvalidOperationException ioe)
                {
                    this.logger.Log("Exception while declining call {0}", ioe);
                    Console.WriteLine("Exception while declining call {0}", ioe);
                }
                catch (RealTimeException rte)
                {
                    this.logger.Log("Exception while declining call {0}", rte);
                    Console.WriteLine("Exception while declining call {0}", rte);
                }
            }
        }

        /// <summary>
        /// Platforms the shutdown completed.
        /// </summary>
        /// <param name="result">The result.</param>
        private void PlatformShutdownCompleted(IAsyncResult result)
        {
            // Shutdown actions will not throw.
            this.collabPlatform.EndShutdown(result);
            this.logger.Log("The platform has now shutdown.");
        }

        private void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                this.logger.Log("An unhandled exception has been caught : {0}", ex);
            }
        }

        #endregion
    }
}
