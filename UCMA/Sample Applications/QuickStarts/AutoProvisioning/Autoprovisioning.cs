/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/


// .NET namespaces
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Threading;

// UCMA namespaces
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;


namespace Microsoft.Rtc.Collaboration.Sample.AutoProvisioning
{
    public class UCMASampleAutoProvisioning
    {
        #region Locals
        private CollaborationPlatform _collabPlatform;
        private ApplicationEndpoint _appEndpoint;
        private String _appID;
        #endregion

        #region Methods
        /// <summary>
        /// Instantiate and run the AutoProvisioning quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            UCMASampleAutoProvisioning ucmaSampleAutoProvisioning
                = new UCMASampleAutoProvisioning();
            ucmaSampleAutoProvisioning.Run();
        }
        
        private void Run()
        {
            _appID = null;
            try
            {
                // Attempt to retrieve the application ID of the provisioned 
                // application from the config file.
                _appID = System.Configuration.ConfigurationManager.AppSettings["ApplicationID"];
                if (string.IsNullOrEmpty(_appID))
                {
                    // The application ID wasn't retrieved from the config file
                    // so prompt for the application ID for the application that
                    // has been provisioned.
                    string prompt = "Please enter the unique ID of the application that is provisioned in "
                        + "the topology => ";
                    _appID = UCMASampleHelper.PromptUser(prompt, null);
                }
                if (!string.IsNullOrEmpty(_appID))
                {
                    Console.WriteLine("Creating CollaborationPlatform for the provisioned application with "
                        + "ID \'{0}\' using ProvisionedApplicationPlatformSettings.", _appID);
                    ProvisionedApplicationPlatformSettings settings
                        = new ProvisionedApplicationPlatformSettings("UCMASampleApp", _appID);
                    _collabPlatform = new CollaborationPlatform(settings);

                    // Wire up a handler for the 
                    // ApplicationEndpointOwnerDiscovered event.
                    _collabPlatform.RegisterForApplicationEndpointSettings(
                        this.Platform_ApplicationEndpointOwnerDiscovered);
                    // Initalize and startup the platform.
                    _collabPlatform.BeginStartup(EndPlatformStartup, _collabPlatform);
                }
                else
                {
                    Console.WriteLine("No application ID was specified by the user. Unable to demonstrate "
                        + "AutoProvisioning feature for this reason.");
                }
                Console.WriteLine("Please hit any key to end the sample.");
                Console.ReadKey();
            }
            catch (InvalidOperationException iOpEx)
            {
                // Invalid Operation Exception may be thrown if the data provided
                // to the BeginXXX methods was invalid/malformed.
                // TODO (Left to the reader): Error handling code.
                Console.WriteLine("Invalid Operation Exception: " + iOpEx.ToString());
            }
            finally
            {
                // Terminate the platform which would in turn terminate any endpoints.
                Console.WriteLine("Shutting down the platform.");
                ShutdownPlatform();
            }
        }

        // Registered event handler for the ApplicationEndpointOwnerDiscovered event on the
        // CollaborationPlatform for the provisioned application.
        void Platform_ApplicationEndpointOwnerDiscovered(object sender,
            ApplicationEndpointSettingsDiscoveredEventArgs e)
        {
            Console.WriteLine("ApplicationEndpointOwnerDiscovered event was raised during startup of the "
                + "CollaborationPlatform.");
            Console.WriteLine("The ApplicationEndpointOwnerConfiguration that corresponds to the provisioned "
                + "application with ID \'{0}\' are: ", _appID);  
            Console.WriteLine("Owner display name is: "
                + e.ApplicationEndpointSettings.OwnerDisplayName);
            Console.WriteLine("Owner URI is: " + e.ApplicationEndpointSettings.OwnerUri);
            Console.WriteLine("Now retrieving the ApplicationEndpointSettings from the "
                + "ApplicationEndpointSettingsDiscoveredEventArgs.");        
            ApplicationEndpointSettings settings = e.ApplicationEndpointSettings;
            settings.SupportedMimePartContentTypes = new ContentType[] { new ContentType("text/plain") };

            Console.WriteLine("Initializing the ApplicationEndpoint that corresponds to the provisioned "
                + "application with ID \'{0}\'.", _appID);
                    
            // Initalize the endpoint using the settings retrieved above.
            _appEndpoint = new ApplicationEndpoint(_collabPlatform, settings);
            // Wire up the StateChanged event.
            _appEndpoint.StateChanged += this.Endpoint_StateChanged;
            // Wire up the ApplicationEndpointOwnerPropertiesChanged event.
            _appEndpoint.OwnerPropertiesChanged += this.Endpoint_ApplicationEndpointOwnerPropertiesChanged;

            try
            {
                // Establish the endpoint.
                _appEndpoint.BeginEstablish(EndEndpointEstablish, _appEndpoint);
            }
            catch (InvalidOperationException iOpEx)
            {
                // Invalid Operation Exception may be thrown if the data provided
                // to the BeginXXX methods was invalid/malformed.
                // TODO (Left to the reader): Error handling code.
                Console.WriteLine("Invalid Operation Exception: " + iOpEx.ToString());
            }
        }

        // Record the endpoint state transitions to the console.
        void Endpoint_StateChanged(object endpoint, LocalEndpointStateChangedEventArgs e)
        {
            // When the endpoint is terminated because of a contact being deleted,
            // the application receives Terminating and Terminated state changes.
            Console.WriteLine("Endpoint has changed state. The previous endpoint state was: "
                + e.PreviousState + " and the current state is: " + e.State);
        }

        // Record the ApplicationEndpoint's owner changes to the console.
        void Endpoint_ApplicationEndpointOwnerPropertiesChanged(object endpoint, 
            ApplicationEndpointOwnerPropertiesChangedEventArgs e)
        {
            // When provisioning data for the endpoint changes, the 
            // ProvisioningDataChanged event is raised.
            Console.WriteLine("ApplicationEndpoint's owner properties have changed");
            Console.WriteLine("The set of changed properties for the provisioned application with ID \'{0}\' "
                + "are: ", _appID);
            foreach (var property in e.ChangedPropertyNames)
            {
                Console.WriteLine(property);
            }
        }

        // Callback for CollaborationPlatform's BeginStartup().
        void EndPlatformStartup(IAsyncResult ar)
        {
            CollaborationPlatform collabPlatform = ar.AsyncState as CollaborationPlatform;
            try
            {
                // The CollaborationPlatform should now be started.
                collabPlatform.EndStartup(ar);
                Console.WriteLine("Collaboration platform associated with the provisioned application with "
                    + "ID {0} has been started", _appID);
            }
            catch (ProvisioningFailureException pfEx)
            {
                // ProvisioningFailureException may be thrown during EndStartup()
                // for many reasons such as inaccessible configuration data, 
                // unable to find a matching application, and invalid 
                // configuration data. The FailureReason property exposes these
                // issues via a value of the ProvisioningFailureReason enumerated
                // type, which can have values of ConfigurationDataInaccessible, 
                // ApplicationNotFound and InvalidConfiguration.
                // TODO (Left to the reader): Error handling code.
                Console.WriteLine("ProvisioningFailure Exception: " + pfEx.ToString());
                Console.WriteLine("The FailureReason for the ProvisioningFailure Exception: "
                    + pfEx.FailureReason.ToString());
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException will be thrown when the platform 
                // cannot establish, usually due to invalid data.
                // TODO (Left to the reader): Error handling code.
                Console.WriteLine("OperationFailure Exception: " + opFailEx.ToString());
            }
            catch (ConnectionFailureException connFailEx)
            {
                // ConnectionFailureException will be thrown when the platform
                // cannot connect.
                // TODO (Left to the reader): Error handling code.
                Console.WriteLine("ConnectionFailure Exception: " + connFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException may be thrown as a result of any
                // UCMA operation.
                // TODO (Left to the reader): Error handling code.
                Console.WriteLine("RealTimeException : " + realTimeEx.ToString());
            }
        }

        // Callback for ApplicationEndpoint's BeginEstablish().
        void EndEndpointEstablish(IAsyncResult ar)
        {
            LocalEndpoint currentEndpoint = ar.AsyncState as LocalEndpoint;
            try
            {
                currentEndpoint.EndEstablish(ar);
            }
            catch (ConnectionFailureException connFailEx)
            {
                // ConnectionFailureException will be thrown when the endpoint
                // cannot connect to the server.
                // TODO (Left to the reader): Error handling code.
                Console.WriteLine("ConnectionFailure Exception: " + connFailEx.ToString());
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException may be thrown as a result of any 
                // UCMA operation.
                // TODO (Left to the reader): Error handling code.
                Console.WriteLine("RealTimeException : " + realTimeEx.ToString());
            }
        }

        // Method to shutdown the CollaborationPlatform.
        void ShutdownPlatform()
        {
            _collabPlatform.BeginShutdown(EndPlatformShutdown, _collabPlatform);
        }

        // Callback for CollaborationPlatform's BeginShutdown().
        void EndPlatformShutdown(IAsyncResult ar)
        {
            CollaborationPlatform collabPlatform = ar.AsyncState as CollaborationPlatform;
            try
            {
                collabPlatform.EndShutdown(ar);
                Console.WriteLine("The platform is now shut down.");
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException may be thrown as a result of any 
                // UCMA operation.
                // TODO (Left to the reader): Error handling code.
                Console.WriteLine("RealTimeException : " + realTimeEx.ToString());
            }
        }
        #endregion
    }
}
