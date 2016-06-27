/*====================================================
Copyright (c) Microsoft Corporation. All rights reserved.
 
This source code is intended only for use with Microsoft
Development Tools and/or on-line documentation.  See these other
materials for detailed information regarding Microsoft code samples.
 
THIS CODE AND INFORMATION ARE PROVIDED "AS IS."  YOU BEAR THE RISK OF USING IT.  
MICROSOFT GIVES NO EXPRESS WARRANTIES, GUARANTIES OR CONDITIONS.  
TO THE EXTENT PERMITTED UNDER YOUR LOCAL LAWS, MICROSOFT EXCLUDES 
THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NON-INFRINGEMENT.
=====================================================*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration.AudioVideo.VoiceXml;
using Microsoft.Rtc.Signaling;
using Microsoft.Speech;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.VoiceXml.Common;

//
//
// This sample shows how to run the MCS VoiceXML Browser object against a VoiceXML page and an OCS AudioVideo call 
// The major steps are as follows
// Set up the MCS objects (collaboration platform and endpoints) 
// Instantiate a VoiceXml.Browser object and do the following:
// 1. Subscribe to Browser events
// 2. set the event handler for when a call is received
//
// Inside the call received event handler, 
// 1. subscribe to the StateChanged events on the call object and then 
// 2. start a session to run the call against the VoiceXML page by calling Browser.RunAsync(callObject).
//
// As events are fired by the Browser object they are recorded in the Event Notification text box.
// 

namespace Microsoft.Rtc.Collaboration.Sample.VoiceXml
{

    public partial class VoiceXmlSample : Form
    {

        #region VoiceXML and MCS members
        private Browser voiceXmlBrowser;
        private Uri startPage; //A global keeper for the URI to the VXML start page.

        //Global keepers to the UCMA objects used.
        private CollaborationPlatform collaborationPlatform;
        private UserEndpoint endpoint;
        private AudioVideoCall currentCall;

        //Internal keepers for the sample itself.
        private ReaderWriterLock shutdownLock = new ReaderWriterLock();
        private object callCleanupLock = new object();

        #endregion

        #region Messages
        //Error and logging strings for the UI text display area.
        private string noUriError = "No URI specified.\nPlease enter a valid start-page URI and try again.";
        private string errorInUri = "Invalid Uri.";
        private string enterValidUri = "{0}\nPlease enter a valid URI for the start Page.";
        private string noActiveCall = "No active call.";
        private string callErrorCaption = "Call Handling Error.";
        #endregion

        /// <summary>
        /// Constructor called bin Program.cs
        /// </summary>
        public VoiceXmlSample()
        {
            //Perform general startup; the remainder requires user input.
            InitializeComponent();

            InitializeFormControls();
        }

        /// <summary>
        /// Initializes the Browser object and sets event handlers
        /// </summary>
        private void InitializeVoiceXmlBrowser()
        {
            // dispose any existing Browser reference before we instantiate a new one
            if (voiceXmlBrowser == null)
            {
                //Create the browser object, and bind all associated event handlers. 
                voiceXmlBrowser = new Browser();

                //These events are analogues to the similarly named call states.
                voiceXmlBrowser.Transferring += new EventHandler<TransferringEventArgs>(HandleTransferring);
                voiceXmlBrowser.Transferred += new EventHandler<TransferredEventArgs>(HandleTransferred);

                voiceXmlBrowser.Disconnecting
                    += new EventHandler<DisconnectingEventArgs>(HandleDisconnecting);
                voiceXmlBrowser.Disconnected
                    += new EventHandler<DisconnectedEventArgs>(HandleDisconnected);

                voiceXmlBrowser.SessionCompleted
                    += new EventHandler<SessionCompletedEventArgs>(HandleSessionCompleted);
            }
        }

        #region MCS setup and call event handler.

        /// <summary>
        /// Sets up the OCS objects using the credentials provided
        /// </summary>
        /// <remarks>
        /// In this sample this is invoked when the user presses the "Apply Server Settings" button
        /// </remarks>
        /// <param name="applicationUri">SIP Uri of the OCS account</param>
        /// <param name="serverName">SIP server URI</param>
        /// <param name="serverPort">Port (usually 443)</param>
        /// <param name="credential">Credentials for establishing the user endpoint</param>
        private void InitializeMCSConnection(string applicationUri, string serverName, int serverPort, NetworkCredential credential)
        {
            //Initialize new client platform.
            //Note that this sample only supports TLS, and additionally that it uses ClientPlatform rather than ServerPlatform.
            //In a production application, ServerPlatform should be used; ClientPlatform is preferred here to demonstrate the platform without
            //requiring application provisioning. This is out of the scope of this sample, however.
            ClientPlatformSettings clientPlatformSettings;
            clientPlatformSettings = new ClientPlatformSettings("VoiceXMLTestApp", SipTransportType.Tls);

            collaborationPlatform = new CollaborationPlatform(clientPlatformSettings);
            collaborationPlatform.EndStartup(collaborationPlatform.BeginStartup(null, null));

            //Create a new UserEndpoint based on the information provided through the UI.
            //As with Server/Client platform above, in production, this should be replaced with provisioned ApplicationEndpoint in most circumstances.
            UserEndpointSettings endpointSettings;

            endpointSettings = new UserEndpointSettings(applicationUri, serverName, serverPort);

            endpoint = new UserEndpoint(collaborationPlatform, endpointSettings);

            endpoint.Credential = credential;

            //Bind the event handler to handle incoming calls. Use of the strongly-typed AVCall dictates that calls not matching this type will not be raised.
            endpoint.RegisterForIncomingCall<AudioVideoCall>(AudioVideoCallReceived);                     
            
            endpoint.EndEstablish(endpoint.BeginEstablish(null, endpoint));
        }

        /// <summary>
        /// EventHandler raised when an incoming call arrives to the endpoint, above.
        /// </summary>
        /// <param name="sender">Object that sent the event</param>
        /// <param name="e">CallReceivedEventArgs object</param>
        private void AudioVideoCallReceived(object sender, CallReceivedEventArgs<AudioVideoCall> e)
        {
            //Assign the current call to the global keeper.
            currentCall = e.Call;

            currentCall.AudioVideoFlowConfigurationRequested += new EventHandler<AudioVideoFlowConfigurationRequestedEventArgs>(Call_AudioVideoFlowConfigurationRequested);

            //Bind handlers to the current call's state changed event to drive UI.
            //State change is used to inform the application of the current state of the call.
            //In particular, this should be used by the application to determine what operations are valid in a given state.
            currentCall.StateChanged
                += new EventHandler<CallStateChangedEventArgs>(HandleCallStateChanged);

            // accept the incoming call and waits for State and configuration requests.
            currentCall.EndAccept(currentCall.BeginAccept(null, null));

        }

        /// <summary>
        /// Handle the AudioVideoFlowConfigurationRequested event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">AudioVideoFlowConfigurationRequestedEventArgs</param>
        private void Call_AudioVideoFlowConfigurationRequested(object sender, AudioVideoFlowConfigurationRequestedEventArgs e)
        {
            currentCall.Flow.StateChanged += new EventHandler<MediaFlowStateChangedEventArgs>(Flow_StateChanged);
        }

        /// <summary>
        /// Handle the AudioVideoFlow state change event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">MediaFlowStateChangedEventArgs</param>
        private void Flow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            if (e.State == MediaFlowState.Active)
            {
                RunBrowser();
            }
        }

        /// <summary>
        /// Run the browser
        /// </summary>
        private void RunBrowser()
        {
            InitializeVoiceXmlBrowser();

            //If the start page has not been assigned, fail with appropriate message box.
            //More robust error checking is left as an exercise to the user.
            SetStartPage();

            if (startPage == null)
            {
                MessageBox.Show(noUriError, errorInUri, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (asyncRB.Checked)
            {
                //Associate the avCall with the VXML browser object. 
                voiceXmlBrowser.SetAudioVideoCall(currentCall);

                //Calling RunAsync on the browser object will cause the VXML browser page to execute on the associated AVCall.
                //Note, the Async model used here is not the Begin/End type prevalent elsewhere in UCMA. 
                voiceXmlBrowser.RunAsync(startPage, null);
            }
            else
            {   //This is the synchronous case
                //Disable the buttons until the page has finished running.
                EnableButtons(false);

                //Associate the avCall with the VXML browser object. 
                voiceXmlBrowser.SetAudioVideoCall(currentCall);

                // Start running the page against the current call synchronously
                VoiceXmlResult vr = voiceXmlBrowser.Run(startPage, null);

                //After run completes, re-enable the buttons.
                EnableButtons(true);
            }
        }

        /// <summary>
        /// Clean-up the call's EventHandlers
        /// </summary>
        private void CleanupCall()
        {
            lock (callCleanupLock)
            {
                if (currentCall != null)
                {
                    if (currentCall.Flow != null)
                    {
                        currentCall.Flow.StateChanged -= new EventHandler<MediaFlowStateChangedEventArgs>(Flow_StateChanged);
                    }

                    currentCall.StateChanged -= new EventHandler<CallStateChangedEventArgs>(HandleCallStateChanged);
                    currentCall.AudioVideoFlowConfigurationRequested -= new EventHandler<AudioVideoFlowConfigurationRequestedEventArgs>(Call_AudioVideoFlowConfigurationRequested);
                    if (currentCall.State == CallState.Established)
                    {
                        currentCall.EndTerminate(currentCall.BeginTerminate(null, null));
                    }
                    currentCall = null;
                }
            }
        }

        /// <summary>
        /// Cleanup the browser's EventHandlers
        /// </summary>
        private void CleanupBrowser()
        {
            if (voiceXmlBrowser != null)
            {
                // Normally, you'll want to unregister your Transferring, Transferred, Disconnecting, Disconnected, and SessionCompleted
                // EventHandlers for a browser that you no longer need. You should do this after you're sure that there are no active 
                // sessions for the browser. 
                // 
                // In this implementation, this method will only be called when we're exiting the application, so we'll short-circuit that 
                // step and unregister our handlers immediately (even though the session might not be completed).
                voiceXmlBrowser.Transferring
                    -= new EventHandler<TransferringEventArgs>(HandleTransferring);
                voiceXmlBrowser.Transferred
                    -= new EventHandler<TransferredEventArgs>(HandleTransferred);
                voiceXmlBrowser.Disconnecting
                    -= new EventHandler<DisconnectingEventArgs>(HandleDisconnecting);
                voiceXmlBrowser.Disconnected
                    -= new EventHandler<DisconnectedEventArgs>(HandleDisconnected);
                voiceXmlBrowser.SessionCompleted
                    -= new EventHandler<SessionCompletedEventArgs>(HandleSessionCompleted);
                voiceXmlBrowser = null;
            }
        }

        /// <summary>
        /// Tear down the endpoint gracefully
        /// </summary>
        private void CleanupMCS()
        {
            shutdownLock.AcquireWriterLock(Timeout.Infinite);
            shutdownLock.ReleaseWriterLock();

            // Clean-up the browser
            this.CleanupBrowser();
            // Clean-up the call
            this.CleanupCall();

            if (endpoint != null)
            {
                endpoint.EndTerminate(endpoint.BeginTerminate(null, null));
            }
        }

        #endregion

        #region Browser event handlers.

        //Basic handling only.
        //In production, this is a useful place to write application logs, as appropriate.
        private void HandleSessionCompleted(object sender, SessionCompletedEventArgs e)
        {
            hangupCallButton.BackColor = buttonColor;

            if (!runCompletedCB.Checked) return;

            if (e.Error != null)
            {
                AddResultStringToOutput(String.Format("Error on return: {0}", e.Error.Message));
            }
            else if (e.Cancelled)
            {
                AddResultStringToOutput("Run was cancelled by application. Call Status: "
                    + voiceXmlBrowser.State);
            }
            else
            {
                AddResultStringToOutput("Run completed. Reason: " + e.Result.Reason);
                AddResultStringToOutput("Namelist variables from VoiceXml page processing:\n");
                if (e.Result != null && e.Result.Namelist != null)
                {
                    foreach (string key in e.Result.Namelist.Keys)
                    {
                        AddResultStringToOutput(" " + key + " = " + e.Result.Namelist[key] + "\n");
                    }
                }
            }

            //It is good practice to check the call state after the session terminicates and take appropriate action if
            //the call is still active. Options include:
            //1. Hangup (see below)
            //2. Play a goodbye audio file on the call and then hangup
            //3. Transfer, for example to customer service
            //
            //Current implementation calls helper method, which will terminate active calls and cleanup EventHandlers.
            this.CleanupCall();
        }

        private void HandleCallStateChanged(object sender, CallStateChangedEventArgs e)
        {
            if ((e.State == CallState.Established) || (e.State == CallState.Establishing))
            {
                SetButtonEnabledState(hangupCallButton, true);
            }
            else
            {
                SetButtonEnabledState(hangupCallButton, false);
            }
            if (!callEventsCB.Checked) return;

            AddResultStringToOutput("\n Underlying Call state changed. From: " + e.PreviousState.ToString() 
                + " To: " + e.State.ToString()+ ".");
            AddResultStringToOutput("\n Reason for state change: " + e.TransitionReason.ToString());
        }

        private void HandleDisconnecting(object sender, DisconnectingEventArgs e)
        {
            if (!disconnectingCB.Checked) return;

            AddResultStringToOutput("\nDisconnect Started. Browser state: " + e.SessionState + ".\n");
        }

        private void HandleDisconnected(object sender, DisconnectedEventArgs e)
        {
            if (!disconnectedCB.Checked) return;

            AddResultStringToOutput("\nDisconnect Started. Reason for disconnect: " + e.State + ".\n");

        }

        private void HandleTransferring(object sender, TransferringEventArgs e)
        {
            if (!callTransferStartCB.Checked) return;

            AddResultStringToOutput("\nTransfer started to " + e.TargetSip + ".\n");
        }

        private void HandleTransferred(object sender, TransferredEventArgs e)
        {
            if (!callTransferCompleteCB.Checked) return;

            AddResultStringToOutput("\nTransfer completed to " + e.TargetSip + ".\n");
        }

        #endregion

        #region Windows Form Code
        /// <summary>
        /// The rest of the code is infrastructure of the VoiceXmlSample form, and is not directly related
        /// to VoiceXML functionality
        /// </summary>

        //This initializes form elements such as delegates and control colors
        private void InitializeFormControls()
        {
            SetResultsDelegate = new SetResultsText(SetResultsTextProc);
            SetButtonStateDelegate = new SetButtonState(SetButtonStateProc);

            // This enables changing the color of all buttons in once place
            buttonColor = Color.Silver;

            hangupCallButton.BackColor = buttonColor;
            stopBrowserButton.BackColor = buttonColor;
            exitButton.BackColor = buttonColor;
            hangupCallButton.Enabled = false;
            stopBrowserButton.Enabled = false;

            SetStartPage();
        }

        #region VoiceXmlSample Control event handlers

        private void stopBrowserButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(voiceXmlBrowser != null, "voiceXmlBrowser!=null");
            voiceXmlBrowser.StopAsync();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            if (voiceXmlBrowser != null && voiceXmlBrowser.State == SessionState.Active)
            {
                voiceXmlBrowser.StopAsync();
            }

            CleanupMCS();

            Application.Exit();
        }

        public void EnableButtons(bool enabled) // To disable buttons during RunSync
        {
            SetButtonEnabledState(hangupCallButton, enabled);
            SetButtonEnabledState(stopBrowserButton, enabled);
            SetButtonEnabledState(exitButton, enabled);
        }
                
        private void SetStartPage()
        {
            startPage = null;
            if (!string.IsNullOrEmpty(startPageTextBox.Text))
            {   //In case the URI text box is pre-loaded with a URI
                if (Uri.IsWellFormedUriString(startPageTextBox.Text, UriKind.Absolute))
                {
                    startPage = new Uri(startPageTextBox.Text, UriKind.Absolute);
                }
                else
                {
                    try
                    {
                        startPage = new Uri(new Uri(System.Environment.CurrentDirectory + "\\"), startPageTextBox.Text);
                    }
                    catch
                    {
                        MessageBox.Show(
                            String.Format(enterValidUri, errorInUri, MessageBoxButtons.OK, MessageBoxIcon.Exclamation));
                        startPage = null;
                    }
                }
            }
        }

        /// <summary>
        /// This gives an example of how to terminate a call, as a production application may need to do if the call is
        /// still live after the VoiceXML session is completed.
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void hangupButton_Click(object sender, EventArgs e)
        {
            if (currentCall == null || currentCall.State != CallState.Established)
            {
                MessageBox.Show(noActiveCall, callErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                // terminate the call and cleanup
                // note: the VoiceXML browser will properly detect a terminated call and stop the session
                this.CleanupCall();
            }
        }

        //Helper function to check or uncheck all checkboxes in a collection of controls
        private void SetCheckboxState(System.Windows.Forms.Control.ControlCollection controls, bool checkedState)
        {
            foreach (Control c in controls)
            {
                if (c is CheckBox)
                {
                    CheckBox cb = (CheckBox)c;
                    cb.Checked = checkedState;
                }

                if (c.Controls.Count > 0)
                {
                    SetCheckboxState(c.Controls, checkedState);
                }
            }
        }

        private void checkAllButton_Click(object sender, EventArgs e)
        {
            SetCheckboxState((ControlCollection)this.Controls, true);
        }

        private void clearAllButton_Click(object sender, EventArgs e)
        {
            SetCheckboxState((ControlCollection)this.Controls, false);
        }

        #endregion

        #region Form control members
        private Color buttonColor;
        #endregion

        #region Writing text and setting state of controls

        // For setting text box text using Invoke.
        delegate void SetResultsText(string resultsText);
        private SetResultsText SetResultsDelegate;

        // For setting text box text directly.
        private void SetResultsTextProc(string newText)
        {
            this.resultsTextBox.AppendText(newText);
        }

        // This method writes text to textbox for either case, sync or async.
        private void AddResultStringToOutput(string resultsString)
        {
            if (resultsTextBox.InvokeRequired)
            {
                resultsTextBox.Invoke(SetResultsDelegate, new object[] { resultsString + "\n" });
            }
            else
            {
                SetResultsTextProc(resultsString + "\n");
            }
        }

        //Some generic Button/UI helpers.
        delegate void SetButtonState(Button button, bool enabled);

        private SetButtonState SetButtonStateDelegate;

        private void SetButtonStateProc(Button button, bool enabled)
        {
            button.Enabled = enabled;
        }

        private void SetButtonEnabledState(Button button, bool enabled)
        {
            if (button.InvokeRequired)
            {
                button.Invoke(SetButtonStateDelegate, new object[] { button, enabled });
            }
            else
            {
                button.Enabled = enabled;
            }
        }

        //This code simply enables/disables the user and password fields when the application is 
        // told to use current or entered credentials.
        private void useCurrentUserCB_CheckedChanged(object sender, EventArgs e)
        {
            if (useCurrentUserCB.Checked)
            {
                domainUserTB.Enabled = false;
                passwordTB.Enabled = false;
            }
            else
            {
                domainUserTB.Enabled = true;
                passwordTB.Enabled = true;
            }
        }

        //Causes the sample to adopt the current settings for MCS connection.
        //Validation of the input provided is left as an exercise to the reader, and omitted here.
        private void applyServerSettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                string[] userUriTBElements = serverPortTB.Text.Split(':'); 
                                
                int port;
                Int32.TryParse(userUriTBElements[1], out port);

                if (useCurrentUserCB.Checked)
                {
                    InitializeMCSConnection("sip:" + userUriTB.Text, userUriTBElements[0], port, CredentialCache.DefaultNetworkCredentials);
                }
                else
                {
                    string[] domainUserTBElements = domainUserTB.Text.Split('\\');
                    InitializeMCSConnection("sip:" + userUriTB.Text, userUriTBElements[0], port,
                        new NetworkCredential(domainUserTBElements[1], passwordTB.Text, domainUserTBElements[0]));
                }
            }
            catch (Exception ex)
            {
                throw ex; //TODO: (Left to the application) proper string validation, parsing and exception handling. 
            }

        }

        #endregion

        #endregion

    }
}
