/*===================================================================== 
  This file is part of the Microsoft Unified Communications Code Samples. 

  Copyright (C) 2010 Microsoft Corporation.  All rights reserved. 

This source code is intended only as a supplement to Microsoft 
Development Tools and/or on-line documentation.  See these other 
materials for detailed information regarding Microsoft code samples. 

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
PARTICULAR PURPOSE. 
=====================================================================*/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;

namespace AudioVideoConversation
{
    /// <summary>
    /// Implements a simple Main Window of a Lync-like application.
    /// </summary>
    public partial class MainWindow : Form
    {
        //holds the Lync client instance
        private LyncClient client;

        //Controls created in this sample
        private SimpleContactFinderControl contactFinderControl;
        private SignInControl signInControl;
        
        // Saves a list of the conversation windows, so they can be closed
        // when a conversation is removed.        
        private Dictionary<Conversation, ConversationWindow> conversationWindows = new Dictionary<Conversation, ConversationWindow>();

        //used to prevent Lync events from firing before the sample conversation window is loaded
        private AutoResetEvent conversationWindowLock = new AutoResetEvent(false);

        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                //obtains the lync client instance
                client = LyncClient.GetClient();

                //creates the contact list user control
                contactFinderControl = new SimpleContactFinderControl(client.ContactManager, "Create a conversation");
                //register for the control ContactsSelected event
                contactFinderControl.ContactSelected += new ContactSelected(contactFinderControl_ContactSelected);

                //creates the Sign-In user control
                signInControl = new SignInControl(client);
            }
            //if the Lync process is not running and UISuppressionMode=false,
            //this exception will be thrown
            catch (ClientNotFoundException)
            {
                //explain to the user what happened
                MessageBox.Show("Microsoft Lync does not appear to be running. Please start Lync.");

                //exit (in a fully implemented application, a retry here would be recommended)
                Application.Exit();
            }
            catch (NotStartedByUserException)
            {
                //explain to the user what happened
                MessageBox.Show("Microsoft Lync does not appear to be running. Please start Lync.");

                //exit (in a fully implemented application, a retry here would be recommended)
                Application.Exit();
            }
        }

        /// <summary>
        /// Updates the initial UI state and registers for client events.
        /// </summary>
        private void MainWindow_Load(object sender, EventArgs e)
        {
            //shows the current client state
            toolStripStatusLabel.Text = client.State.ToString();

            // register for state updates. Whenever the client change its state, this
            // event will be fired. For example, during Sign-In it will likely move from:
            // SignedOut -> SigningIn -> SignedIn
            client.StateChanged += client_StateChanged;

            // register for the client exiting event
            // when/if the Lync process exits, it will fire this event
            client.ClientDisconnected += client_ClientDisconnected;

            //***********************************************************************************
            // This application works with UISuppressionMode = true or false
            //
            // UISuppressionMode hides the Lync user interface.
            //
            // Registry key for enabling UISuppressionMode:
            //
            // 32bit OS:
            // [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Office\15.0\Lync]
            // "UISuppressionMode"=dword:00000001
            //
            // 64bit OS:
            // [HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Communicator]
            // "UISuppressionMode"=dword:00000001
            //
            // When running with UISuppressionMode = 1 and this application is the only one
            // using the client, it's necessary to Initialize the client. The following check
            // verifies if the client has already been initialized. If it hasn't, the code will
            // call BeginInitialize() proving a callback method, on which this application's
            // main UI will be presented (either Sign-In or contact input, if already signed in).
            //***********************************************************************************

            //if this client is in UISuppressionMode...
            if (client.InSuppressedMode && client.State == ClientState.Uninitialized)
            {
                //...need to initialize it
                try
                {
                    client.BeginInitialize(this.ClientInitialized, null);
                }
                catch (LyncClientException lyncClientException)
                {
                    Console.WriteLine(lyncClientException);
                }
                catch (SystemException systemException)
                {
                    if (LyncModelExceptionHelper.IsLyncException(systemException))
                    {
                        // Log the exception thrown by the Lync Model API.
                        Console.WriteLine("Error: " + systemException);
                    }
                    else
                    {
                        // Rethrow the SystemException which did not come from the Lync Model API.
                        throw;
                    }
                }
            }
            else //not in UI Suppression, so the client was already initialized
            {
                //registers for conversation related events
                //these events will occur when new conversations are created (incoming/outgoing) and removed
                client.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;
                client.ConversationManager.ConversationRemoved += ConversationManager_ConversationRemoved;

                //show sign-in or contact selection
                ShowMainContent();
            }
        }

        /// <summary>
        /// Unregisters from client related events.
        /// </summary>
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.StateChanged -= client_StateChanged;
            client.ClientDisconnected -= client_ClientDisconnected;
            client.ConversationManager.ConversationAdded -= ConversationManager_ConversationAdded;
            client.ConversationManager.ConversationRemoved -= ConversationManager_ConversationRemoved;
        }

        /// <summary>
        /// Called when the client in done initializing.
        /// </summary>
        /// <param name="result"></param>
        private void ClientInitialized(IAsyncResult result)
        {
            //registers for conversation related events
            //these events will occur when new conversations are created (incoming/outgoing) and removed
            client.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;
            client.ConversationManager.ConversationRemoved += ConversationManager_ConversationRemoved;
        }

        /// <summary>
        /// After the client is initialized, show either sign-in or contact selection controls.
        /// </summary>
        private void ShowMainContent()
        {
            //depending on the client state, show sign in panel or the contact selection
            if (client.State == ClientState.SignedIn)
            {
                //no sign-in necessary, show the contact list selection
                ShowContactSelectionControl();
            }
            else if (client.State == ClientState.SignedOut)
            {
                //sign-in is needed, so show the sign-in control
                ShowSignInControl();
            }
            else
            {
                // The client here could also be in the SigningIn/SigningOut states
                // The application should take the appropriate action
                // SigningIn: the application could wait for the SignedIn state or call lyncClient.BeginSignOut()
                // SigningOut: the application should wait for the SignedOut state

                //Shows a pending sign-in/out message.
                ShowPendingSignInOut();

                //the client_StateChanged() bellow will take care of showing the proper
                //controls when the state changes to SignedIn or SignedOut
            }
        }

        /// <summary>
        /// Called when the state of the Lync client changes.
        /// Will show the correct controls on the main window based on the client state.
        /// </summary>
        void client_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {
                //shows the current client state
                toolStripStatusLabel.Text = e.NewState.ToString();

                //shows the main content based on the client state
                ShowMainContent();
            }));
        }

        /// <summary>
        /// Called when Lync is exiting.
        /// </summary>
        void client_ClientDisconnected(object sender, EventArgs e)
        {
            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {
                Console.Out.WriteLine("Lync has disconnected");
                Application.Exit();
            }));
        }

        /// <summary>
        /// Shows the contact item control.
        /// </summary>
        private void ShowContactSelectionControl()
        {
            //removes the sign in control / pending sign-in/out message
            tableLayoutPanel.Controls.Remove(signInControl);
            tableLayoutPanel.Controls.Remove(labelPendingSignInOut);
            
            //adds the contact selection control to the second row of the panel
            tableLayoutPanel.Controls.Add(contactFinderControl, 0, 1);

            //show the self contact name
            string name = null;
            try
            {
                name = client.Self.Contact.GetContactInformation(ContactInformationType.DisplayName) as string;
            }
            catch (LyncClientException e)
            {
                Console.WriteLine(e);
            }
            catch (SystemException systemException)
            {
                if (LyncModelExceptionHelper.IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    Console.WriteLine("Error: " + systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }

            // value can be null if presence document item is not available.
            labelUserName.Text = name ?? "<Unknown>";
        }

        /// <summary>
        /// Shows the Sign-In control.
        /// </summary>
        private void ShowSignInControl()
        {
            //removes the contact selection control / pending sign-in/out message
            tableLayoutPanel.Controls.Remove(contactFinderControl);
            tableLayoutPanel.Controls.Remove(labelPendingSignInOut);

            //adds the sign-in control to the second row of the panel
            tableLayoutPanel.Controls.Add(signInControl, 0, 1);
        }

        /// <summary>
        /// Shows a pending sign-in/out message.
        /// </summary>
        private void ShowPendingSignInOut()
        {
            //removes the contact selection or sign-in control
            tableLayoutPanel.Controls.Remove(signInControl);
            tableLayoutPanel.Controls.Remove(contactFinderControl);

            //adds the pending sign-in/out message to the second row of the panel
            tableLayoutPanel.Controls.Add(labelPendingSignInOut, 0, 1);
        }

        /// <summary>
        /// Called when a set of contacts are selected from the contact list.
        /// </summary>
        void contactFinderControl_ContactSelected(object source, Contact contact)
        {
            //creates a new conversation
            Conversation conversation = null;
            try
            {
                conversation = client.ConversationManager.AddConversation();
            }
            catch (LyncClientException e)
            {
                Console.WriteLine(e);
            }
            catch (SystemException systemException)
            {
                if (LyncModelExceptionHelper.IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    Console.WriteLine("Error: " + systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }

            //Adds a participant to the conversation
            //the window created for this conversation will handle the ParticipantAdded events
            if (contact != null && conversation != null)
            {
                try
                {
                    conversation.AddParticipant(contact);
                }
                catch (LyncClientException lyncClientException)
                {
                    Console.WriteLine(lyncClientException);
                }
                catch (SystemException systemException)
                {
                    if (LyncModelExceptionHelper.IsLyncException(systemException))
                    {
                        // Log the exception thrown by the Lync Model API.
                        Console.WriteLine("Error: " + systemException);
                    }
                    else
                    {
                        // Rethrow the SystemException which did not come from the Lync Model API.
                        throw;
                    }
                }
            }
        }

        #region ConversationManager event handling

        //*****************************************************************************************
        //                              ConversationManager Event Handling
        // 
        // ConversationAdded occurs when:
        // 1) A new conversation was created by this application
        // 2) A new conversation was created by another third party application or Lync itself
        // 2) An invite was received at this endpoint (InstantMessaging / AudioVideo)
        //
        // ConversationRemoved occurs when:
        // 1) A conversation is terminated
        //
        //*****************************************************************************************

        /// <summary>
        /// Called when a new conversation is added (incoming or outgoing).
        /// 
        /// Will create a window for this new conversation and show it.
        /// </summary>
        void ConversationManager_ConversationAdded(object sender, ConversationManagerEventArgs e)
        {

            //*****************************************************************************************
            //                              Registering for events
            //
            // It is very important that registering for an object's events happens within the handler
            // of that object's added event. In another words, the application should register for the
            // conversation events within the ConversationAdded event handler.
            //
            // This is required to avoid timing issues which would cause the application to miss events.
            // While this handler method is executing, the Lync client is unable to process events for  
            // this application (synce its thread is running this method), so no events will be lost.
            //
            // By registering for events here, we guarantee that all conversation related events will be 
            // caught the first time they occur.
            //
            // We want to show the availability of the buttons in the conversation window based
            // on the ActionAvailability events. The solution below uses a lock to allow the window
            // to load while holding the event queue. This prevents events from being raised even 
            // before the user interface controls get a change to load.
            //
            //*****************************************************************************************

            //creates a new window (which will register for Conversation and child object events)
            ConversationWindow window = new ConversationWindow(e.Conversation, client);

            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {
                //adds the window to the dictionary
                conversationWindows.Add(e.Conversation, window);

                //registers for window load events so that the lock of the Lync thread can be released
                window.Load += window_Load;

                //shows the new window
                window.Show(this);

            }));

            //waits until the window is loaded to release the SDK thread
            conversationWindowLock.WaitOne();
        }

        /// <summary>
        /// Releases the lock from the Lync thread when the conversation window is done loading.
        /// </summary>
        void window_Load(object sender, EventArgs e)
        {
            //releases the lock used to hold the Lync SDK thread
            conversationWindowLock.Set();
        }

        /// <summary>
        /// Called when a conversation is removed.
        /// 
        /// Will dispose the window associated with the removed conversation.
        /// </summary>
        void ConversationManager_ConversationRemoved(object sender, ConversationManagerEventArgs e)
        {
            //posts the execution into the UI thread
            this.BeginInvoke(new MethodInvoker(delegate()
            {
                //checks if a conversation window was created, and dispose it
                if (conversationWindows.ContainsKey(e.Conversation))
                {
                    //gets the existing conversation window
                    ConversationWindow window = conversationWindows[e.Conversation];

                    //remove the conversation from the dictionary
                    conversationWindows.Remove(e.Conversation);

                    //closes and disposes
                    window.Close();
                    window.Dispose();
                }

            }));
        }



        #endregion
    }
}
