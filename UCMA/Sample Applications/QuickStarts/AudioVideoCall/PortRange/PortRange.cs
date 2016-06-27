/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   	*
*                                                       *
********************************************************/

using System;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

namespace Microsoft.Rtc.Collaboration.Sample.PortRange
{
    // This demo establishes a basic audio video connection between the user provided below, and the target user, intended to demonstrate the usage of port range settings for AudioVideoFlow
    // First, this application initializes a platform with a port range set for the audio video traffic, then conducts an Audio Video call normally.
    // Note: This sample only represents an outbound call. it is necessary that there be a recieving client logged in as the far end participant (callee).
    // After an AudioVideo Flow is established, the application tears down the platform, and ends..
    // (We suggest you use Microsoft Lync as the target of this application.)

    // This application requires the credentials of Microsoft Lync Server users, enabled for voice, and that UCMA be present on this machine.
    // Warning: Though the code below makes use of UserEndpoint/user credentials, this is a simplification for ease of use of the sample. For all trusted operations, use ApplicationEndpoint.
    public class UCMABasicAVCall
    {
        private AudioVideoCall _audioVideoCall;
        private AudioVideoFlow _audioVideoFlow;
        private CollaborationPlatform _collabPlatform;
        private UserEndpoint _userEndpoint;

        //Construct the network credential that the UserEndpoint will use to authenticate to the Microsoft Lync Server server.
        private static String _userName = "( User Name )";
        private static String _userPassword = "( Password )";
        private static String _userDomain = "( Domain )";
        private static System.Net.NetworkCredential _credential = new System.Net.NetworkCredential(_userName, _userPassword, _userDomain);

        //The URI and connection server of the user used.
        private static String _userURI = "sip:( User Name )@( SIP Suffix )";
        private static String _userServer = "( Server FQDN )";

        // Transport type used to communicate with your Microsoft Lync Server instance.
        private static Microsoft.Rtc.Signaling.SipTransportType _transportType = Microsoft.Rtc.Signaling.SipTransportType.Tls;

        //The information for the conversation and the far end participant.
        private static String _calledParty = "sip:( User Name )@( SIP Suffix )";
        private static String _conversationSubject = "The Microsoft Lync Server!";
        private static String _conversationPriority = ConversationPriority.Urgent;
        
        //The name of this application, to be used as the outgoing user agent string.
        //The user agent string is put in outgoing message headers to indicate the Application used.
        private static String _applicationName = "UCMASampleCode";

        //Wait handles are only present to keep things synchronous and easy to read.
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private AutoResetEvent _autoResetShutdownEvent = new AutoResetEvent(false);
        
        static void Main(string[] args)
        {
            UCMABasicAVCall BasicAVCall= new UCMABasicAVCall();
            BasicAVCall.Run();
        }

        public void Run()
        {
 
            //Initalize and startup the platform.
            ClientPlatformSettings clientPlatformSettings = new ClientPlatformSettings(_applicationName, _transportType);
            _collabPlatform = new CollaborationPlatform(clientPlatformSettings);
            _collabPlatform.BeginStartup(EndPlatformStartup, _collabPlatform);

            // Get port range
            NetworkPortRange portRange = CollaborationPlatform.AudioVideoSettings.GetPortRange();
            Console.WriteLine("Port range is from " + portRange.LocalNetworkPortMin + " to " + portRange.LocalNetworkPortMax);

            // Modifying port range
            portRange.SetRange(1500, 2000);
            CollaborationPlatform.AudioVideoSettings.SetPortRange(portRange);
            Console.WriteLine("Port range now is from " + portRange.LocalNetworkPortMin + " to " + portRange.LocalNetworkPortMax);

            //Sync; wait for the startup to complete.
            _autoResetEvent.WaitOne();

            
            //Initalize and register the endpoint, using the credentials of the user the application will be acting as.
            UserEndpointSettings userEndpointSettings = new UserEndpointSettings(_userURI, _userServer);
            userEndpointSettings.Credential = _credential;
            _userEndpoint = new UserEndpoint(_collabPlatform, userEndpointSettings);
            _userEndpoint.BeginEstablish(EndEndpointEstablish, _userEndpoint);

            //Sync; wait for the registration to complete.
            _autoResetEvent.WaitOne();
       

            //Setup the conversation and place the call.
            ConversationSettings convSettings = new ConversationSettings();
            convSettings.Priority = _conversationPriority;
            convSettings.Subject = _conversationSubject;
            //Conversation represents a collection of modalities in the context of a dialog with one or multiple callees.
            Conversation conversation = new Conversation(_userEndpoint, convSettings);

            _audioVideoCall = new AudioVideoCall(conversation);

            //Call: StateChanged: Only hooked up for logging.
            _audioVideoCall.StateChanged += new EventHandler<CallStateChangedEventArgs>(audioVideoCall_StateChanged);

            //Subscribe for the flow configuration requested event; the flow will be used to send the media.
            //Ultimately, as a part of the callback, the media will be sent/recieved.
            _audioVideoCall.AudioVideoFlowConfigurationRequested += this.audioVideoCall_FlowConfigurationRequested;


            //Place the call to the remote party;
            _audioVideoCall.BeginEstablish(_calledParty, null, EndCallEstablish, _audioVideoCall);

            //Sync; wait for the call to complete.
            _autoResetEvent.WaitOne();

            // Shutdown the platform
            _collabPlatform.BeginShutdown(EndPlatformShutdown, _collabPlatform);

            //Wait for shutdown to occur.
            _autoResetShutdownEvent.WaitOne();          
        }

        //Just to record the state transitions in the console.
        void audioVideoCall_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Console.WriteLine("Call has changed state. The previous call state was: " + e.PreviousState + " and the current state is: " + e.State);
        }

        //Flow configuration requested indicates that there is a flow present to begin media operations with that it is no longer null, and is ready to be configured.
        public void audioVideoCall_FlowConfigurationRequested(object sender, AudioVideoFlowConfigurationRequestedEventArgs e)
        {
            Console.WriteLine("Flow Configuration Requested.");
            _audioVideoFlow = e.Flow;
            
            //Now that the flow is non-null, bind the event handler for State Changed.
            // When the flow goes active, (as indicated by the state changed event) the program will perform media related actions..
            _audioVideoFlow.StateChanged += new EventHandler<MediaFlowStateChangedEventArgs>(audioVideoFlow_StateChanged);

        }

        private void audioVideoFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            Console.WriteLine("Flow state changed from " + e.PreviousState + " to " + e.State);

            //When flow is active, media operations can begin
            if (e.State == MediaFlowState.Active)
            {
                // Flow-related media operations normally begin here.
         

            }
        }

        private void EndPlatformStartup(IAsyncResult ar)
        {
            CollaborationPlatform collabPlatform = ar.AsyncState as CollaborationPlatform;
            try
            {
                collabPlatform.EndStartup(ar);
                Console.WriteLine("The platform is now started.");
            }
            catch (ConnectionFailureException connFailEx)
            {
                //ConnectionFailureException will be thrown when the platform cannot connect.
                Console.WriteLine(connFailEx.ToString());
            }

            //Again, just for sync. reasons.
            _autoResetEvent.Set();

        }

        private void EndPlatformShutdown(IAsyncResult ar)
        {
            CollaborationPlatform collabPlatform = ar.AsyncState as CollaborationPlatform;

            //Shutdown actions will not throw.
            collabPlatform.EndShutdown(ar);
            Console.WriteLine("The platform is now shutdown.");

            //Again, just to sync the completion of the code and the platform teardown.
            _autoResetShutdownEvent.Set();

        }

        private void EndEndpointEstablish(IAsyncResult ar)
        {
            UserEndpoint userEndpoint = ar.AsyncState as UserEndpoint;
            try
            {
                userEndpoint.EndEstablish(ar);
                Console.WriteLine("The User Endpoint owned by URI: ");
                Console.WriteLine(userEndpoint.OwnerUri);
                Console.WriteLine(" is now established and registered.");
            }
            catch (ConnectionFailureException connFailEx)
            {
                // ConnectionFailureException will be thrown when the endpoint cannot connect to the server, or the credentials are invalid.
                // It is left to the developer to write real error handling code.
                Console.WriteLine(connFailEx.ToString());
            }
            catch (InvalidOperationException iOpEx)
            {
                // InvalidOperationException will be thrown when the endpoint is not in a valid state to connect. To connect, the platform must be started and the Endpoint Idle.
                // It is left to the developer to write real error handling code.
                Console.WriteLine(iOpEx.ToString());
            }
            catch (RegisterException regEx)
            {
                // RegisterException will be thrown when the endpoint cannot be registered (usually due to bad credentials).
                // It is left to the developer to write real error handling code (here, the appropriate action is likely reprompting for user/password/domain strings).
                Console.WriteLine(regEx.ToString());
            }

            //Again, just for sync. reasons.
            _autoResetEvent.Set();
        }

        private void EndCallEstablish(IAsyncResult ar)
        {
            Call call = ar.AsyncState as Call;
            try
            {
                call.EndEstablish(ar);
                Console.WriteLine("The call with Local Participant: " + call.Conversation.LocalParticipant + " and Remote Participant: " + call.RemoteEndpoint.Participant + " is now in the established state.");
            }
            catch (OperationFailureException opFailEx)
            {
                // OperationFailureException: Indicates failure to connect the call to the remote party.
                // It is left to the application to perform real error handling here.
                Console.WriteLine(opFailEx.ToString());
            }
            catch (RealTimeException exception)
            {
                // RealTimeException may be thrown on media or link-layer failures.
                // It is left to the application to perform real error handling here.
                Console.WriteLine(exception.ToString());
            }
            finally
            {
                //Again, just for sync. reasons.
                _autoResetEvent.Set();
            }

        }

    }
}
