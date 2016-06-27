/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

// .NET namespaces
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

// UCMA namespaces
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.ManualProvisioning
{
    public class UCMASampleManualProvisioning
    {
        #region Locals
        #region UCMA 3.0 Core Classes
        /// <summary>
        /// The settings typically used for creating a CollaborationPlatform
        /// instance to be used for manually creating ApplicationEndpoints.
        /// </summary>
        private ServerPlatformSettings _platformSettings;

        /// <summary>
        /// The highest-level API in UCMA, responsible for managing connections
        /// between servers, establishing trust with other servers and
        /// providing control over all the endpoints bound to it.
        /// </summary>
        private CollaborationPlatform _platform;

        /// <summary>
        /// The settings typically used for creating an ApplicationEndpoint.
        /// </summary>
        private ApplicationEndpointSettings _endpointSettings;

        /// <summary>
        /// The endpoint designed to be globally trusted by other server
        /// components.
        /// </summary>
        private ApplicationEndpoint _endpoint;
        #endregion

        #region Configuration Settings
        /// <summary>
        /// The part of the user agent string that identifies the application.
        /// </summary>
        private const string _applicationUserAgent = "UCMASampleApp";

        /// <summary>
        /// Fully Qualified Domain Name (FQDN) of the computer in the trusted
        /// application pool.
        /// </summary>
        private string _applicationHostFQDN;

        /// <summary>
        /// Port assigned to the trusted application.
        /// </summary>
        private int _applicationHostPort;

        /// <summary>
        /// GRUU assigned to this computer for the trusted application.
        /// </summary>
        private string _computerGRUU;

        /// <summary>
        /// Friendly name of the certificate identifying this computer.
        /// </summary>
        /// <remarks>
        /// See http://msdn.microsoft.com/en-us/library/ms788967.aspx for help
        /// viewing certificates with the MMC snap-in.
        /// </remarks>
        private string _certificateFriendlyName;

        /// <summary>
        /// Certificate identifying this computer.
        /// </summary>
        private X509Certificate2 _certificate;

        /// <summary>
        /// SIP URI assigned to the trusted application endpoint.
        /// </summary>
        private string _endpointOwnerURI;

        /// <summary>
        /// FQDN of the registrar pool to which this trusted application
        /// endpoint is assigned.
        /// </summary>
        private string _registrarFQDN;

        /// <summary>
        /// Port of the registrar pool to which this trusted application
        /// endpoint is assigned.
        /// </summary>
        private int _registrarPort;
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the ManualProvisioning quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleManualProvisioning ucmaSampleManualProvisioning
                = new UCMASampleManualProvisioning();
            ucmaSampleManualProvisioning.Run();
        }
        
        /// <summary>
        /// Retrieves the application configuration and begins starting up the
        /// platform.
        /// </summary>
        private void Run()
        {
            // Prompt for the settings necessary to initialize the
            // CollaborationPlatform and the ApplicationEndpoint if they are
            // not declared in App.config.
            // TODO (Left to the reader): Input sanitization on the
            // collected parameters.
            _applicationHostFQDN = UCMASampleHelper.PromptUser(
                "Please enter the FQDN assigned to this computer in the trusted application pool => ",
                "TrustedAppComputerFQDN");
            if (string.IsNullOrEmpty(_applicationHostFQDN))
            {
                UCMASampleHelper.WriteErrorLine(
                    "No FQDN was found in App.config or input by the user.");
            }
            string inputPort = UCMASampleHelper.PromptUser(
                "Please enter the port assigned to this trusted application => ",
                "TrustedAppPort");
            if (!int.TryParse(inputPort, out _applicationHostPort))
            {
                UCMASampleHelper.WriteErrorLine(
                    "Port could not be parsed from App.config or from input by the user.");
            }
            _computerGRUU = UCMASampleHelper.PromptUser(
                "Please enter the GRUU assigned to this computer for this trusted application => ",
                "TrustedAppComputerGRUU");
            if (string.IsNullOrEmpty(_computerGRUU))
            {
                UCMASampleHelper.WriteErrorLine("No GRUU was found in App.config or input by the user.");
            }
            _certificateFriendlyName = UCMASampleHelper.PromptUser(
                "Please enter the friendly name of the certificate identifying this computer => ",
                "CertificateFriendlyName");
            if (string.IsNullOrEmpty(_certificateFriendlyName))
            {
                UCMASampleHelper.WriteErrorLine(
                    "No certificate friendly name was found in App.config or input by the user.");
            }
            _certificate = UCMASampleHelper.GetLocalCertificate(_certificateFriendlyName);
            if (_certificate == null)
            {
                UCMASampleHelper.WriteErrorLine("Certificate with friendly name '" + _certificateFriendlyName
                    + "' could not be found in computer account Personal certificate store.");
            }
            _endpointOwnerURI = UCMASampleHelper.PromptUser(
                "Please enter the SIP URI assigned to this trusted application endpoint => ",
                "TrustedAppEpOwnerURI");
            if (string.IsNullOrEmpty(_endpointOwnerURI))
            {
                UCMASampleHelper.WriteErrorLine("No SIP URI was found in App.config or input by the user.");
            }
            _registrarFQDN = UCMASampleHelper.PromptUser(
                "Please enter the FQDN of the registrar pool  to which this endpoint is assigned => ",
                "RegistrarFQDN");
            if (string.IsNullOrEmpty(_registrarFQDN))
            {
                UCMASampleHelper.WriteErrorLine(
                    "No registrar pool FQDN was found in App.config or input by the user.");
            }
            string inputRegistrarPort = UCMASampleHelper.PromptUser(
                "Please enter the port used by the registrar pool to which this endpoint is assigned => ",
                "RegistrarPort");
            if (!int.TryParse(inputRegistrarPort, out _registrarPort))
            {
                UCMASampleHelper.WriteErrorLine(
                    "Registrar port could not be parsed from App.config or from input by the user.");
            }

            try
            {
                // Create the CollaborationPlatform using the
                // ServerPlatformSettings.
                _platformSettings = new ServerPlatformSettings(_applicationUserAgent,
                    _applicationHostFQDN,
                    _applicationHostPort,
                    _computerGRUU,
                    _certificate);
                _platform = new CollaborationPlatform(_platformSettings);

                // Initialize and startup the platform. EndPlatformStartup()
                // will be called when the platform finishes starting up.
                UCMASampleHelper.WriteLine("Starting platform...");
                _platform.BeginStartup(PlatformStartupCompleted, _platform);
            }
            catch (ArgumentNullException argumentNullException)
            {
                // ArgumentNullException will be thrown if the parameters used
                // to construct ServerPlatformSettings or CollaborationPlatform
                // are null.
                // TODO (Left to the reader): Error handling code to either
                // retry creating the platform with non-null parameters, log the
                // error for debugging or gracefully exit the program.
                UCMASampleHelper.WriteException(argumentNullException);
                UCMASampleHelper.FinishSample();
            }
            catch (ArgumentOutOfRangeException argumentOutOfRangeException)
            {
                // ArgumentOutOfRangeException will be thrown if the port
                // parameter used to construct ServerPlatformSettings is greater
                // than 65536 or less than 0.
                // TODO (Left to the reader): Error handling code to either
                // retry creating the platform with a valid port, log the error
                // for debugging or gracefully exit the program.
                UCMASampleHelper.WriteException(argumentOutOfRangeException);
                UCMASampleHelper.FinishSample();
            }
            catch (ArgumentException argumentException)
            {
                // ArgumentException will be thrown if the parameters used to
                // construct ServerPlatformSettings or CollaborationPlatform are
                // invalid.
                // TODO (Left to the reader): Error handling code to either
                // retry creating the platform with corrected parameters, log
                // the error for debugging or gracefully exit the program.
                UCMASampleHelper.WriteException(argumentException);
                UCMASampleHelper.FinishSample();
            }
            catch (TlsFailureException tlsFailureException)
            {
                // TlsFailureException will be thrown if the certificate used to
                // construct CollaborationPlatform is invalid or otherwise
                // unusable.
                // TODO (Left to the reader): Error handling code to either
                // retry creating the platform with a valid certificate, log the
                // error for debugging or gracefully exit the program.
                UCMASampleHelper.WriteException(tlsFailureException);
                UCMASampleHelper.FinishSample();
            }
            catch (CryptographicException cryptographicException)
            {
                // CryptographicException will be thrown if the certificate used
                // to construct ServerPlatformSettings is invalid.
                // TODO (Left to the reader): Error handling code to either
                // retry creating the platform with a valid certificate, log the
                // error for debugging or gracefully exit the program.
                UCMASampleHelper.WriteException(cryptographicException);
                UCMASampleHelper.FinishSample();
            }
            catch (InvalidOperationException invalidOperationException)
            {
                // InvalidOperationException will be thrown if the platform has
                // already been started or shutdown.
                // TODO (Left to the reader): Error handling code to log the
                // error for debugging.
                UCMASampleHelper.WriteException(invalidOperationException);
                UCMASampleHelper.FinishSample();
            }
            finally
            {
                // Wait for the sample to finish before shutting down the
                // platform and returning from the main thread.
                UCMASampleHelper.WaitForSampleFinish();

                // It is possible the platform was never created due to issues
                // collecting configuration parameters.
                if (_platform != null)
                {
                    // Shutdown the platform, thereby terminating any attached
                    // endpoints.
                    UCMASampleHelper.WriteLine("Shutting down the platform...");
                    _platform.BeginShutdown(PlatformShutdownCompleted, _platform);
                }
            }
        }

        /// <summary>
        /// Acquire settings necessary for sample to run.
        /// </summary>
        private void AcquireSettings()
        {
        }
        #endregion

        #region Callback Delegates
        /// <summary>
        /// Callback from <code>BeginStartup</code> method on platform.
        /// </summary>
        /// <param name="result">
        /// Status of the platform startup operation.
        /// </param>
        private void PlatformStartupCompleted(IAsyncResult result)
        {
            // Extract the platform that was passed in as the state argument to
            // the BeginStartup() method.
            CollaborationPlatform platform = result.AsyncState as CollaborationPlatform;

            if (platform == null)
            {
                UCMASampleHelper.WriteErrorLine("CollaborationPlatform not passed into BeginStartup() method.");
                UCMASampleHelper.FinishSample();
                return;
            }

            try
            {
                // Determine whether the startup operation completed
                // successfully.
                platform.EndStartup(result);

                UCMASampleHelper.WriteLine("Platform has been started.");
            }
            catch (TlsFailureException tlsFailureException)
            {
                // TlsFailureException will be thrown if the certificate used to
                // startup the platform is invalid or otherwise unusable.
                // TODO (Left to the reader): Error handling code to either
                // retry the platform startup operation with a different
                // certificate, log the error for debugging, or gracefully exit
                // the program.
                UCMASampleHelper.WriteException(tlsFailureException);
                UCMASampleHelper.FinishSample();
            }
            catch (ConnectionFailureException connectionFailureException)
            {
                // ConnectionFailureException will be thrown when the platform
                // could not listen on any of the configured IP/port
                // combinations.
                // TODO (Left to the reader): Error handling code to notify user
                // the configured IP address cannot be used or some other
                // process is already using the port, log the error for
                // debugging, or gracefully exit the program.
                UCMASampleHelper.WriteException(connectionFailureException);
                UCMASampleHelper.FinishSample();
            }
            catch (RealTimeException realTimeException)
            {
                // RealTimeException will be thrown when the platform startup
                // operation failed for some other reason.
                // TODO (Left to the reader): Error handling code to notify user
                // the configured IP address cannot be used or some other
                // process is already using the port, log the error for
                // debugging, or gracefully exit the program.
                UCMASampleHelper.WriteException(realTimeException);
                UCMASampleHelper.FinishSample();
            }
            try
            {
                // Create the ApplicationEndpointSettings.
                _endpointSettings = new ApplicationEndpointSettings(_endpointOwnerURI,
                    _registrarFQDN,
                    _registrarPort);

                // Create the ApplicationEndpoint from the
                // ApplicationEndpointSettings and bind it to the platform.
                _endpoint = new ApplicationEndpoint(platform, _endpointSettings);

                // Bind an event handler to notify when the endpoint changes
                // state.
                _endpoint.StateChanged += new EventHandler<LocalEndpointStateChangedEventArgs>(
                    _endpoint_StateChanged);

                // Establish the endpoint so that it can receive incoming calls
                // and conference invitations. EndEndpointEstablish()  will be
                // called when the endpoint finishes establishment.
                _endpoint.BeginEstablish(EndpointEstablishmentCompleted, _endpoint);

                UCMASampleHelper.WriteLine("Endpoint has been established.");
            }
            catch (ArgumentNullException argumentNullException)
            {
                // ArgumentNullException will be thrown if the parameters used to
                // construct ApplicationEndpointSettings or ApplicationEndpoint
                // are null.
                // TODO (Left to the reader): Error handling code to either
                // retry creating the endpoint with non-null parameters, log the
                // error for debugging or gracefully exit the program.
                UCMASampleHelper.WriteException(argumentNullException);
                UCMASampleHelper.FinishSample();
            }
            catch (ArgumentException argumentException)
            {
                // ArgumentException will be thrown if the parameters used to
                // construct ApplicationEndpointSettings or ApplicationEndpoint
                // are invalid.
                // TODO (Left to the reader): Error handling code to either
                // retry creating the endpoint with corrected parameters, log
                // the error for debugging or gracefully exit the program.
                UCMASampleHelper.WriteException(argumentException);
                UCMASampleHelper.FinishSample();
            }
            catch (InvalidOperationException invalidOperationException)
            {
                // InvalidOperationException will be thrown if the platform is
                // already shutdown, an endpoint with the same SIP URI already
                // exists, or the endpoint has already been established or
                // terminated.
                // TODO (Left to the reader): Error handling code to log the
                // error for debugging.
                UCMASampleHelper.WriteException(invalidOperationException);
                UCMASampleHelper.FinishSample();
            }
        }

        /// <summary>
        /// Callback from <code>BeginEstablish</code> method on endpoint.
        /// </summary>
        /// <param name="result">
        /// Status of the endpoint establishment operation.
        /// </param>
        private void EndpointEstablishmentCompleted(IAsyncResult result)
        {
            // Extract the endpoint that was passed in as the state argument to
            // the BeginEstablish() method.
            ApplicationEndpoint endpoint = result.AsyncState as ApplicationEndpoint;

            if (endpoint == null)
            {
                UCMASampleHelper.WriteErrorLine("ApplicationEndpoint not passed into BeginEstablish() method.");
                UCMASampleHelper.FinishSample();
                return;
            }

            try
            {
                // Determine whether the establish operation completed
                // successfully.
                endpoint.EndEstablish(result);

                // Endpoint state change event handler will log the successful
                // establishment of the endpoint.
                UCMASampleHelper.FinishSample();
            }
            catch (ConnectionFailureException connectionFailureException)
            {
                // ConnectionFailureException will be thrown when no connection
                // can be made to the server.
                // TODO (Left to the reader): Error handling code to either
                // retry establishing the endpoint, log the error for debugging
                // or gracefully exit the program.
                UCMASampleHelper.WriteException(connectionFailureException);
                UCMASampleHelper.FinishSample();
            }
            catch (OperationFailureException operationFailureException)
            {
                // OperationFailureException will be thrown if the retrieval of
                // in-band provisioning data fails.
                // TODO (Left to the reader): Error handling code to either
                // retry establishing the endpoint, log the error for debugging
                // or gracefully exit the program.
                UCMASampleHelper.WriteException(operationFailureException);
                UCMASampleHelper.FinishSample();
            }
            catch (RegisterException registerException)
            {
                // RegisterException will be thrown if the SIP REGISTER
                // operation fails.
                // TODO (Left to the reader): Error handling code to either
                // retry establishing the endpoint, log the error for debugging
                // or gracefully exit the program.
                UCMASampleHelper.WriteException(registerException);
                UCMASampleHelper.FinishSample();
            }
            catch (AuthenticationException authenticationException)
            {
                // AuthenticationException will be thrown if general
                // authentication-related problem occur.
                // TODO (Left to the reader): Error handling code to either
                // retry establishing the endpoint, log the error for debugging
                // or gracefully exit the program.
                UCMASampleHelper.WriteException(authenticationException);
                UCMASampleHelper.FinishSample();
            }
            catch (OperationTimeoutException operationTimeoutException)
            {
                // OperationTimeoutException will be thrown if the registrar
                // server does not respond to REGISTER request.
                // TODO (Left to the reader): Error handling code to either
                // retry establishing the endpoint, log the error for debugging
                // or gracefully exit the program.
                UCMASampleHelper.WriteException(operationTimeoutException);
                UCMASampleHelper.FinishSample();
            }
            catch (RealTimeException realTimeException)
            {
                // RealTimeException will be thrown if the endpoint establish
                // operation fails due to some other issue.
                // TODO (Left to the reader): Error handling code to either
                // retry establishing the endpoint, log the error for debugging
                // or gracefully exit the program.
                UCMASampleHelper.WriteException(realTimeException);
                UCMASampleHelper.FinishSample();
            }
        }
        
        /// <summary>
        /// Callback from <code>BeginShutdown</code> method on platform.
        /// </summary>
        /// <param name="result">
        /// Status of the platform shutdown operation.
        /// </param>
        private void PlatformShutdownCompleted(IAsyncResult result)
        {
            // Extract the platform that was passed in as the state argument to
            // the BeginShutdown() method.
            CollaborationPlatform platform = result.AsyncState as CollaborationPlatform;

            if (platform == null)
            {
                UCMASampleHelper.WriteErrorLine(
                    "CollaborationPlatform not passed into BeginShutdown() method.");
                UCMASampleHelper.FinishSample();
                return;
            }

            // Determine whether the shutdown operation completed
            // successfully.
            platform.EndShutdown(result);

            UCMASampleHelper.WriteLine("The platform is now shut down.");
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Record the endpoint state transitions to the console.
        /// </summary>
        /// <param name="sender">Endpoint that saw its state change.</param>
        /// <param name="e">Data about the endpoint state change event.</param>
        private void _endpoint_StateChanged(object sender, LocalEndpointStateChangedEventArgs e)
        {
            // Extract the endpoint that sent the state change event.
            ApplicationEndpoint endpoint = sender as ApplicationEndpoint;

            UCMASampleHelper.WriteLine("Endpoint (" + endpoint.OwnerUri
                + ") has changed state. The previous endpoint state was '"
                + e.PreviousState + "' and the current state is '" + e.State + "'.");
        }
        #endregion
    }
}
