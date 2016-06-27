/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/

// .NET namespaces
using System;
using System.Threading;
using Microsoft.Win32;
// UCMA namespaces
using Microsoft.Rtc.Signaling;

// UCMA samples namespaces
using Microsoft.Rtc.Collaboration.Sample.Common;

namespace Microsoft.Rtc.Collaboration.Sample.UCMAConversationContextChannel
{
    public class UCMASampleConversationContextChannel
    {
        #region Locals
        // The application endpoint that creates a conversation context channel.
        private ApplicationEndpoint _appEndpoint;

        // A helper that takes care of establishing the endpoint.
        private UCMASampleHelper _helper;

        // The string representation Guid of the application that needs to be
        // loaded in the context channel window.
        private string _applicationGuidString;

        // The Guid representation of AppGuid
        // This can be any Guid used from the user provided config.
        // This Guid should match the contextPackage regkey created below.
        // If this is null, then no application is loaded in the extension window.
        private Guid _applicationGuid;

        // The url to be loaded in the conversation extension window.
        // If this is null, no application is loaded in the extension window.
        // Ensure that this url is listed with the trusted sites for the default
        // browser on the machine where is sample is run.
        private string _targetUrl;

        // Conversation on which context channel is created.
        private Conversation _conversation;

        // Context application Regkey- This regkey needs to be created so that
        // Lync knows that an application needs to be activated in the
        // conversation extension window.
        // The required registry key modifications are explained in detail in
        // the SetUpRegistryKeys method below.
        private const string _contextApplicationRegkey = @"Software\Microsoft\Communicator\ContextPackages\";

        // RegistryKey representation of Context application Regkey.
        private RegistryKey _applicationRegistryKey;

        // Bool that is true when the Context application Regkey is created on
        // the local machine.
        private bool _registryKeysWereCreated = false;
        #endregion

        # region Methods

        /// <summary>
        /// Instantiate and run the ConversationContextChannel quickstart.
        /// </summary>
        /// <param name="args">unused</param>
        static void Main(string[] args)
        {
            UCMASampleConversationContextChannel ucmaSampleConversationContextChannel
                = new UCMASampleConversationContextChannel();
            ucmaSampleConversationContextChannel.Run();
        }

        /// <summary>
        /// Retrieves the application configuration and begins running the
        /// sample.
        /// </summary>
        public void Run()
        {
            try
            {
                UCMASampleHelper.WriteLine("This sample needs that Lync Client SDK  is "
                    + "installed on the machine where the sample is being run."
                    + "Otherwise the conversation extension window will not show up. "
                    + "Please ensure that the Lync Client SDK is installed before"
                    + " running the sample.\n");

                _helper = new UCMASampleHelper();

                // Get the application endpoint from an auto provisioned platform
                // where the application id is specified by the config file or
                // the user.
                // If multiple endpoints are provisioned on the same
                // application, the first of the application endpoints that is
                // discovered is used.
                // Note that a UserEndpoint using a ServerPlatform may also be
                // used to create a context channel,
                // instead of an ApplicationEndpoint
                _appEndpoint = _helper.CreateAutoProvisionedApplicationEndpoint();

                if (_appEndpoint != null)
                {
                    if (_appEndpoint.State != LocalEndpointState.Established)
                    {
                        UCMASampleHelper.WriteLine("The Application endpoint is not currently in the"
                            + " Established state, exiting...");
                        UCMASampleHelper.FinishSample();
                        return;
                    }

                    // Create a new conversation for the application endpoint.
                    _conversation = new Conversation(_appEndpoint);

                    // If the config file does not contain the
                    // InvitationTargetURI, the user will be prompted to enter
                    // the same.
                    string prompt = "Please enter the URI of the user to send an IM to, in the User@Host format => ";

                    string invitationTargetUri = UCMASampleHelper.PromptUser(prompt, "InvitationTargetURI");
                    if (!string.IsNullOrEmpty(invitationTargetUri))
                    {
                        const string sipPrefix = "sip:";
                        if (!invitationTargetUri.Trim().StartsWith(sipPrefix,
                                                                    StringComparison.OrdinalIgnoreCase))
                        {
                            invitationTargetUri = sipPrefix + invitationTargetUri.Trim();
                        }

                        // Retrieve the application GUID - this Guid is
                        // associated with the application to be loaded
                        // in the conversation extension window in the registry.
                        // This Guid should match the parameter Guid in the
                        // ConversationContextChannelEstablishOptions.

                        // If the Lync user is signed in within the intranet
                        // then the InternalUrl as specified by the regkey is
                        // treated as the page to be loaded in the application window.

                        // If the Lync user is signed in outside the intranet,
                        // then the ExternalUrl as specified by the regkey is
                        // treated as the page to be loaded in the application window.

                        // If the regkey Path is used to indicate the location of
                        // this application on the local machine, then a link to
                        // this application is shown on the conversation window
                        // which when activated will open the application on the
                        // local machine.

                        // If all of the regkeys are created for the given
                        // contextual package then the user has an option to
                        // either click on the link as specified by the "Path"
                        // regkey or use the page activated in the extension
                        //  window as specified by the "InternalUrl" or the 
                        // "ExternalUrl" as the case may be.

                        // You can generate your own Guid .For more information
                        // to generate the GUID, please look up
                        // http://msdn.microsoft.com/en-us/library/ms241442(VS.80).aspx
                        prompt = "Please enter the GUID for the application => ";
                        _applicationGuidString = UCMASampleHelper.PromptUser(prompt, "AppGuid");

                        // Validating the _appGuid
                        try
                        {
                            _applicationGuid = new Guid(_applicationGuidString);
                        }
                        catch (FormatException formatException)
                        {
                            // FormatException will be thrown when Guid supplied
                            // is not well formatted.
                            // TODO (Left to the reader): Error handling code to
                            // either retry establishing the endpoint, log the
                            // error for debugging or gracefully exit the program.
                            UCMASampleHelper.WriteException(formatException);
                            UCMASampleHelper.WriteLine("Guid specified by user is not valid, ending Sample.\n");
                            UCMASampleHelper.FinishSample();
                        }

                        // Retrieve the uri of the site to be loaded
                        // in the conversation extension window.
                        // _targetUrl is the page that gets loaded in the
                        // extension window.
                        // _targetUrl might also be a Silverlight based application.
                        prompt = "Please enter the url of website to load (This is mandatory for the sample"
                                    +" to work ) => ";
                        _targetUrl = UCMASampleHelper.PromptUser(prompt, "TargetUrl");
                        if (!string.IsNullOrEmpty(_targetUrl))
                        {
                            UCMASampleHelper.WriteLine("The context channel will now be created....\n");
                        }
                        else
                        {
                            // If the targetUrl is not specified, extension window
                            // is not opened for the signed in Lync user, ending
                            // sample.
                            UCMASampleHelper.WriteLine("The target url is not specified,no application will"
                                                       +" be loaded in the extension window.\n");
                            UCMASampleHelper.FinishSample();
                        }

                        // Context channel can only be created with any type of
                        // call after a call is established
                        // between and local and the remote endpoints.
                        UCMASampleHelper.WriteLine("Establishing an Im Call.\n");
                        var newImCall = new InstantMessagingCall(_conversation);

                        UCMASampleHelper.WriteLine("Establishing an Im Call with " + invitationTargetUri + ".\n");
                        // Calling begin Establish to setup an imcall with Remote
                        // Participant.
                        var establishCallResult = newImCall.BeginEstablish(invitationTargetUri, 
                                                                            null,
                                                                            ImCallEstablishCompleted,
                                                                            newImCall);
                        UCMASampleHelper.WriteLine("Waiting for the ImCall to be established.\n");
                    }
                    else
                    {
                        // When the remote participant is not specified, end the
                        // sample.
                        UCMASampleHelper.WriteLine("No Remote participant uri provided, ending Sample.\n");
                        UCMASampleHelper.FinishSample();
                    }
                }
                else
                {
                    // When the application endpoint is not discovered , end the
                    // sample.
                    UCMASampleHelper.WriteLine("No application endpoint has been discovered on this "
                                                +"application, ending Sample.\n");
                    UCMASampleHelper.FinishSample();
                }
            }
            finally
            {
                // Wait for the sample to finish before shutting down the
                // platform and returning from the main thread.
                UCMASampleHelper.WaitForSampleFinish();

                // Terminate the platform which will in turn terminate any
                // endpoints.
                UCMASampleHelper.WriteLine("Shutting down the platform.\n");
                _helper.ShutdownPlatform();
                // Cleanup the regkey entries created by _contextApplicationRegkey.
                if (_registryKeysWereCreated && Registry.CurrentUser.OpenSubKey(_contextApplicationRegkey
                                                                            + _applicationGuidString) != null)
                {
                    Registry.CurrentUser.DeleteSubKeyTree(_contextApplicationRegkey + _applicationGuidString);
                }
                else
                {
                    UCMASampleHelper.WriteLine("ContextApplicationRegkey has not been created, Ending Sample.\n");
                }
                UCMASampleHelper.FinishSample();
            }
        }

        /// <summary>
        /// This function creates a context channel with the participant endpoint
        /// and attempts to establish the context channel.
        /// </summary>
        private void CreateContextChannelToEndpoint()
        {
            UCMASampleHelper.WriteLine("Creating context channel....\n");

            // Get the participant endpoint of the remote participant of the
            // conversation with whom the context channel will be established.
            ParticipantEndpoint participantEndpointUser = _conversation.RemoteParticipants[0].GetEndpoints()[0];
            UCMASampleHelper.WriteLine("The participant endpoint uri is: " + participantEndpointUser.Uri + "\n");

            // Create a new context channel on the conversation. A context channel
            // represents a channel with which a custom application can
            // communicate with remote endpoint signed in through Lync client.
            ConversationContextChannel contextChannel = new ConversationContextChannel(_conversation,
                                                                                    participantEndpointUser);

            // Subscribe to ContextChannel StateChanged and DataReceived events.
            contextChannel.StateChanged +=
                new EventHandler<ConversationContextChannelStateChangedEventArgs>(ContextChannel_StateChanged);
            contextChannel.DataReceived +=
                new EventHandler<ConversationContextChannelDataReceivedEventArgs>(ContextChannel_DataReceived);


            // Set the ContextChannel options.
            ConversationContextChannelEstablishOptions channelEstablishOptions = new ConversationContextChannelEstablishOptions();

            // A link to where the application can be installed from
            // (used only when the app is not currently installed on
            // the machine where the Lync user is signed in).
            channelEstablishOptions.ApplicationInstallerPath = "http://Contoso.com/InstallApplication.html";

            // The name for your application. The remote endpoint uses
            // this name to display messages about your application.
            // This does not need to be the same name as the actual application;
            // this name is only for display on the conversation extension window.
            channelEstablishOptions.ApplicationName = "ContosoApplicationName";

            // Any data that the application needs for initialization.
            // ContextualData may be null.
            // For example if you want to open a page showing certain queried
            // database rows on the page you could send the query itself as
            // ContextualData.
            channelEstablishOptions.ContextualData = "ContosoContextData";

            // The link that shows in the conversation history.
            channelEstablishOptions.SimpleLink = "http://Contoso.com/AboutApplication.html";

            // ConversationContextChannel Toast Message
            channelEstablishOptions.Toast = "Context channel toast message";
            // Establish the context channel with the
            // ConversationContextChannelOptions above.
            contextChannel.BeginEstablish(  _applicationGuid,
                                            channelEstablishOptions,
                                            ConversationContextChannelEstablishComplete,
                                            contextChannel);
        }

        #endregion

        #region Callback Delegates

        private void ImCallEstablishCompleted(IAsyncResult result)
        {
            try
            {
                InstantMessagingCall call = result.AsyncState as InstantMessagingCall;
                if (call != null)
                {
                    call.EndEstablish(result);
                }
                UCMASampleHelper.WriteLine("An Im call has been established.\n");

                // Sets up the contextApplicationRegkey to associate the Guid
                // with the application to be loaded in the extension window.
                SetUpRegistryKeys();
                CreateContextChannelToEndpoint();
            }
            catch (FailureResponseException failureResEx)
            {
                // FailureResponseException will be thrown when the remote side
                // returns a failure response for the establish request operation.
                // TODO (Left to the reader): Error handling code to either
                // retry establishing the endpoint, log the error for debugging
                // or gracefully exit the program.
                UCMASampleHelper.WriteException(failureResEx);
                UCMASampleHelper.FinishSample();
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException will be thrown when any of the
                // sub operations of this operation failed due to invalid
                // object state.
                // TODO (Left to the reader): Error handling code to either
                // retry establishing the endpoint, log the error for debugging
                // or gracefully exit the program.
                UCMASampleHelper.WriteException(opFailEx);
                UCMASampleHelper.FinishSample();
            }
            catch (OperationTimeoutException opTimeoutEx)
            {
                // OperationTimeoutException will be thrown when this operation
                // timed out.
                // TODO (Left to the reader): Error handling code to either
                // retry establishing the endpoint, log the error for debugging
                // or gracefully exit the program.
                UCMASampleHelper.WriteException(opTimeoutEx);
                UCMASampleHelper.FinishSample();
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException will be thrown when any of the sub
                // operations of this operation failed due to SIP related
                // errors.
                // TODO (Left to the reader): Error handling code to either
                // retry establishing the endpoint, log the error for debugging
                // or gracefully exit the program.
                UCMASampleHelper.WriteException(realTimeEx);
                UCMASampleHelper.FinishSample();
            }
        }

        private void SetUpRegistryKeys()
        {
            // Create a Regkey in the specified path below
            // for each application that needs to be activated in
            // the conversation extension window.
            // Each application is associated with its own Guid (regkey name)
            // and some settings like Name, InternalUrl, ExtensibilityWindowSize
            // which are specified in the regkey subtree.
            // The ContextPackages registry folder must contain a subfolder for
            // each application, with the same name as the application’s GUID.
            // These subfolders will contain information
            // which Lync will use when loading the application.

            // Create folder named Guid which specifies the details of this
            // extension window app.
             _applicationRegistryKey = Registry.CurrentUser.CreateSubKey(_contextApplicationRegkey 
                                                                            + _applicationGuidString);

             _registryKeysWereCreated = true;
            // Create key DefaultContextPackage reg_DWORD - Specifies whether
            // this package needs to be activated in the extension window by
            // default or needs to be activated only on receiving/sending
            // an invite.
            // If set to 0, the application is loaded in the conversation 
            // extension window on receiving/sending invite only.
            // If set to 1, the application is loaded in the conversation
            // extension window when Lync is signed into.
            _applicationRegistryKey.SetValue("DefaultContextPackage", 0, RegistryValueKind.DWord);

            // Create key ExtensibilityWindowSize reg_DWORD - Sets the minimum
            // size of the extensibility window.
            // 0 = small (300 x 200 pixels), 1 = medium (400 x 600 pixels),
            // 2 = large (800 x 600 pixels).
            _applicationRegistryKey.SetValue("ExtensibilityWindowSize", 1, RegistryValueKind.DWord);
            // Create registry key InternalUrl Reg_SZ so that
            // the user selected url can be loaded in the context
            // channel window.
            // Specifies a context application URL in the
            // Microsoft Lync Server 2010 domain.
            // The application automatically detects which URL 
            // to use, InternalURL or ExternalURL, based on the
            // client location. This entry also accepts the 
            // three optional parameters AppData, AppId, AppName
            // which are the parameters passed in the
            // ContextChannelEstablishOptions.
            // If the regkey "ExternalUrl"/"InternalUrl" is
            // specified as "%AppData%" when
            // ConvCntxtChannelOptions.ContextualData = "http://Contoso.com/FinanceApp"
            // and when the conversation extension window is
            // opened in the internet, ExternalUrl
            // will point to http://Contoso.com/FinanceApp
            // else if the conversation extension window
            // is opened in the intranet, InternalUrl will 
            // point to http://Contoso.com/FinanceApp.
            _applicationRegistryKey.SetValue("InternalUrl",
                                            _targetUrl,
                                            RegistryValueKind.String);

            // Create registry key Name Reg_SZ - Set the name to
            // be displayed at the lower left corner of
            // the extension window.
            _applicationRegistryKey.SetValue("Name",
                                            "Contoso Application",
                                            RegistryValueKind.String);
        }

        /// <summary>
        /// Called when BeginEstablish on ConversationContextChannel is completed.
        /// </summary>
        protected void ConversationContextChannelEstablishComplete(IAsyncResult asyncResult)
        {
            UCMASampleHelper.WriteLine("Entered ConversationContextChannelEstablishComplete, calling EndEstablish.\n");
            ConversationContextChannel channel = asyncResult.AsyncState as ConversationContextChannel;
            try
            {
                if (channel != null)
                {
                    channel.EndEstablish(asyncResult);
                }
                // Send Data on the context channel if the above succeeds
                // and the context channel is established.
                System.Text.Encoding enc = System.Text.Encoding.UTF8;

                // Sample data for the newly-opened extension window to use.
                byte[] Contentbody = enc.GetBytes("Hello, this is test page");

                // Once BeginSendData completes the application is launched in
                // the conversation extension window. On the Lync side, if your
                // application is Silverlight based, it can access the
                // hosting Conversation object by calling this method in Lync ClientSDk -
                // Conversation conv = CommunicatorClient.GetHostingConversation()
                // Then they can call Conversation.GetApplicationData to access
                // the data received from the remote user.

                channel.BeginSendData(new System.Net.Mime.ContentType("application/ms-session-invite+xml"),
                                        Contentbody,
                                        ConversationContextChannelSendComplete,
                                        channel);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                // InvalidOperationException will be thrown when this method
                // is invoked multiple times using the same asyncResult.This
                // is also thrown if the ConversationContextChannel is not in
                // the Established state when BeginSendData is called.
                // TODO (Left to the reader): Error handling code to log the
                // error for debugging.
                UCMASampleHelper.WriteException(invalidOperationException);
                UCMASampleHelper.FinishSample();
            }
             catch (ArgumentNullException argumentNullException)
            {
                // ArgumentNullException will be thrown if the result
                // is null.
                // TODO (Left to the reader): Error handling code to either
                // retry creating the endpoint with non-null parameters, log the
                // error for debugging or gracefully exit the program.
                UCMASampleHelper.WriteException(argumentNullException);
                UCMASampleHelper.FinishSample();
            }
            catch (ArgumentException argumentException)
            {
                // ArgumentException will be thrown if the results parameter
                // is invalid.
                // TODO (Left to the reader): Error handling code to either
                // retry creating the endpoint with corrected parameters, log
                // the error for debugging or gracefully exit the program.
                UCMASampleHelper.WriteException(argumentException);
                UCMASampleHelper.FinishSample();
            }
            catch (FailureResponseException failureResponseException)
            {
                // FailureResponseException will be thrown if the remote side
                // returns a failure response for the establish request operation
                // TODO (Left to the reader): Error handling code to either
                // retry creating the endpoint with corrected parameters, log
                // the error for debugging or gracefully exit the program.
                UCMASampleHelper.WriteException(failureResponseException);
                UCMASampleHelper.FinishSample();
            }
            catch (OperationFailureException operationFailureException)
            {
                // OperationFailureException will be thrown if any of
                // the sub operations of this operation failed due to invalid
                // object state.
                // TODO (Left to the reader): Error handling code to either
                // retry creating the endpoint with corrected parameters, log
                // the error for debugging or gracefully exit the program.
                UCMASampleHelper.WriteException(operationFailureException);
                UCMASampleHelper.FinishSample();
            }
            catch (OperationTimeoutException operationTimeoutException)
            {
                // OperationTimeoutException will be thrown if this operation
                // timed out.
                // TODO (Left to the reader): Error handling code to either
                // retry creating the endpoint with corrected parameters, log
                // the error for debugging or gracefully exit the program.
                UCMASampleHelper.WriteException(operationTimeoutException);
                UCMASampleHelper.FinishSample();
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException will be thrown when when any of the sub
                // operations of this operation failed due to SIP related errors
                // like connection failure, Authentication failure.
                // TODO (Left to the reader): Error handling code to either
                // retry establishing the endpoint, log the error for debugging
                // or gracefully exit the program.
                UCMASampleHelper.WriteException(realTimeEx);
                UCMASampleHelper.FinishSample();
            }
        }

        /// <summary>
        /// Called when BeginSendData on ConversationContextChannel is Complete.
        /// </summary>
        protected void ConversationContextChannelSendComplete(IAsyncResult asyncResult)
        {
            UCMASampleHelper.WriteLine("Entered ConversationContextChannelSendComplete.\n");
            ConversationContextChannel channel = asyncResult.AsyncState as ConversationContextChannel;
            try
            {
                channel.EndSendData(asyncResult);
            }
            catch (RealTimeException realTimeEx)
            {
                // RealTimeException will be thrown when when any of the sub
                // operations of this operation failed due to SIP related errors
                // like connection failure, Authentication failure.
                // TODO (Left to the reader): Error handling code to either
                // retry establishing the endpoint, log the error for debugging
                // or gracefully exit the program.
                UCMASampleHelper.WriteException(realTimeEx);
                UCMASampleHelper.FinishSample();
            }
            finally
            {
                UCMASampleHelper.FinishSample();
            }
        }

         #endregion

        #region Event Handlers

        /// <summary>
        /// ContextChannel State changed Event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextChannel_StateChanged(object sender, ConversationContextChannelStateChangedEventArgs e)
        {
            UCMASampleHelper.WriteLine("ConversationContextChannel State changed from " + e.PreviousState + " to "
                                        + e.State + " with transitionReason: " + e.TransitionReason + ".\n");
        }

        /// <summary>
        /// ContextChannel Data Received  Event.
        /// This event is raised when the remote endpoint sends data on the
        /// contextual channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextChannel_DataReceived(object sender, ConversationContextChannelDataReceivedEventArgs e)
        {
            UCMASampleHelper.WriteLine("ConversationContextChannel received data with the following "
                                        +"ContentDescription: " + e.ContentDescription);
        }

        #endregion
    }

}