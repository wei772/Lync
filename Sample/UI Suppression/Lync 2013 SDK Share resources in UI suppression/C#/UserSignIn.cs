/*===================================================================== 
  This file is part of the Microsoft Office 2013 Lync Code Samples. 

  Copyright (C) 2013 Microsoft Corporation.  All rights reserved. 

This source code is intended only as a supplement to Microsoft 
Development Tools and/or on-line documentation.  See these other 
materials for detailed information regarding Microsoft code samples. 

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
PARTICULAR PURPOSE. 
=====================================================================*/

using System;
using Microsoft.Lync.Model;
using System.Windows.Forms;

namespace ShareResources
{
    /// <summary>
    /// This class handles signing the user into Lync and handling or bubbling LyncClient platform events to calling code.
    /// </summary>
    class UserSignIn
    {
        public delegate void SetWindowCursorDelegate(Cursor newCursor);
        //Client state requires a change to the window cursor. 
        public event SetWindowCursorDelegate SetWindowCursor;

        public delegate void CloseAppConditionDelegate();
        //An error condition or client shut down requires parent window to close.
        public event CloseAppConditionDelegate CloseAppConditionHit;

        public delegate void UserIsSignedInDelegate(LyncClient lyncClient);
        //User has signed in to Lync
        public event UserIsSignedInDelegate UserIsSignedIn;

        public delegate void ClientStateChangedDelegate(string newState);
        //The state of the Lync client has changed.
        public event ClientStateChangedDelegate ClientStateChanged;

        /// <summary>
        /// Flag that indicates that this instance of the ShareResources
        /// process initialized Lync. Other instances of ShareResources must not
        /// attempt to shut down Lync
        /// </summary>
        private Boolean _thisProcessInitializedLync = false;

        /// <summary>
        /// Indicates the user is starting a Side-by-side instance of Lync
        /// </summary>
        private Boolean _inSideBySideMode = false;

        /// <summary>
        /// Lync client platform. The entry point to the API
        /// </summary>
        Microsoft.Lync.Model.LyncClient _LyncClient;

        string _UserUri;

        public Microsoft.Lync.Model.LyncClient Client
        {
            get
            {
                return _LyncClient;
            }
        }

        public Boolean ThisProcessInitializedLync
        {
            get
            {
                return _thisProcessInitializedLync;
            }
        }

        /// <summary>
        /// Gets the Lync client, initializes if in UI suppression, and 
        /// starts the user sign in process. This method can raise exceptions
        /// which are thrown if the calling form has not registered a callback for
        /// exception specific events that are declared in this class.
        /// </summary>
        /// <param name="sideBySide">boolean. Specifies endpoint mode</param> 
        internal void StartUpLync(Boolean sideBySide)
        {
            //Calling GetClient a second time in a running process will
            //return the previously cached client. For example, calling GetClient(boolean sideBySideFlag)
            // the first time in a process returns a new endpoint.  Calling the method a second
            //time returns the original endpoint. If you call GetClient(false) to get a client 
            //endpoint and then GetClient(true), the original client enpoint is returned even though
            // a true value argument is passed with the second call.

            try
            {
                if (_LyncClient == null)
                {
                    //If sideBySide == false, a standard endpoint is created
                    //Otherwise, a side-by-side endpoint is created
                    _LyncClient = LyncClient.GetClient(sideBySide);
                }
                _inSideBySideMode = sideBySide;

                //Display the current state of the Lync client.
                if (ClientStateChanged != null)
                {
                    ClientStateChanged(_LyncClient.State.ToString());
                }

                //Register for the three Lync client events needed so that application is notified when:
                // * Lync client signs in or out
                _LyncClient.StateChanged += _LyncClient_StateChanged;
                _LyncClient.SignInDelayed += _LyncClient_SignInDelayed;
                _LyncClient.CredentialRequested += _LyncClient_CredentialRequested;



                //Client state of uninitialized means that Lync is configured for UI suppression mode and
                //must be initialized before a user can sign in to Lync
                if (_LyncClient.State == ClientState.Uninitialized)
                {
                    _LyncClient.BeginInitialize(
                        (ar) =>
                        {
                            _LyncClient.EndInitialize(ar);
                            _thisProcessInitializedLync = true;
                        },
                        null);
                }

                else if (_LyncClient.State == ClientState.SignedIn)
                {
                    if (UserIsSignedIn != null)
                    {
                        UserIsSignedIn(_LyncClient);
                    }

                }
                //If the Lync client is signed out, sign into the Lync client
                else if (_LyncClient.State == ClientState.SignedOut)
                {
                    SignUserIn();
                }
                else if (_LyncClient.State == ClientState.SigningIn)
                {
                    if (MessageBox.Show(
                        "Lync is signing in. Do you want to continue waiting?",
                        "Sign in delay",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
                    {
                        if (CloseAppConditionHit != null)
                        {
                            CloseAppConditionHit();
                        }

                    }
                }
            }
            catch (NotInitializedException ni)
            {
                MessageBox.Show(
                    "Client is not initialized.  Closing form", 
                    "Lync Client Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Hand);

                //If calling form has registered a handler for this delegate,
                //call the delegate
                if (CloseAppConditionHit != null)
                {
                    CloseAppConditionHit();
                }
                //otherwise, throw the exception.
                else
                {
                    throw ni;
                }

            }
            catch (ClientNotFoundException cnf)
            {
                MessageBox.Show(
                    "Client is not running.  Closing form", 
                    "Lync Client Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Hand);
                //If calling form has registered a handler for this delegate,
                //call the delegate
                if (CloseAppConditionHit != null)
                {
                    CloseAppConditionHit();
                }
                //otherwise, throw the exception.
                else
                {
                    throw cnf;
                }

            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    "General exception: " +
                    exc.Message, "Lync Client Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Hand);
                //If calling form has registered a handler for this delegate,
                //call the delegate
                if (CloseAppConditionHit != null)
                {
                    CloseAppConditionHit();
                }
                //otherwise, throw the exception.
                else
                {
                    throw exc;
                }

            }
        }

        /// <summary>
        /// Signs a user in to Lync as one of two possible users. User that is
        /// signed in depends on whether side-by-side client is chosen.
        /// </summary>
        internal void SignUserIn()
        {
            //Set the display cursor to indicate that user must wait for
            //sign in to complete
            if (SetWindowCursor != null)
            {
                SetWindowCursor(Cursors.WaitCursor);
            }

            //Set the sign in credentials of the user to the
            //appropriate credentials for the endpoint mode
            string userUri = string.Empty;
            string userPassword = string.Empty;


            SignInCreds getCreds;
            getCreds = new SignInCreds("Sign in");
            if (getCreds.ShowDialog() == DialogResult.OK)
            {
                userUri = getCreds.UserName;
                userPassword = getCreds.Password;
                getCreds.Close();
            }

            _UserUri = userUri;
            _LyncClient.BeginSignIn(
                userUri,
                userUri,
                userPassword,
                (ar) =>
                {
                    try
                    {
                        _LyncClient.EndSignIn(ar);
                    }
                    catch (Exception exc)
                    {
                        throw exc;
                    }
                },
                null);
        }


        /// <summary>
        /// Raised when user's credentials are rejected by Lync or a service that
        /// Lync depends on requests credentials
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _LyncClient_CredentialRequested(object sender, CredentialRequestedEventArgs e)
        {
            //If the request for credentials comes from Lync server then sign out, get new creentials
            //and sign in.
            if (e.Type == CredentialRequestedType.LyncAutodiscover)
            {
                try
                {
                    _LyncClient.BeginSignOut((ar) =>
                    {
                        _LyncClient.EndSignOut(ar);
                        //Ask user for credentials and attempt to sign in again
                        SignUserIn();
                    }, null);
                }
                catch (Exception ex)
                {
                    if (SetWindowCursor != null)
                    {
                        SetWindowCursor(Cursors.Arrow);
                    }
                    MessageBox.Show(
                        "Exception on attempt to sign in, abandoning sign in: " +
                        ex.Message,
                        "Lync Client sign in delay",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                SignInCreds getCreds;
                getCreds = new SignInCreds(e.Type.ToString());
                if (getCreds.ShowDialog() == DialogResult.OK)
                {
                    string userUri = getCreds.UserName;
                    string userPassword = getCreds.Password;
                    getCreds.Close();
                    e.Submit(userUri, userPassword, false);
                }

            }
        }

        void _LyncClient_SignInDelayed(object sender, SignInDelayedEventArgs e)
        {
            if (MessageBox.Show(
                "Delay started at " + 
                e.EstimatedStartDelay.ToString() + 
                " Status code:" + 
                e.StatusCode.ToString(), 
                "Lync Client sign in delay", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.No)
            {
                if (CloseAppConditionHit != null)
                {
                    CloseAppConditionHit();
                }
            }
            else
            {
                try
                {
                    _LyncClient.BeginSignOut((ar) => { _LyncClient.EndSignOut(ar); }, null);
                }
                catch (LyncClientException lce)
                {
                    MessageBox.Show("Exception on sign out in SignInDelayed event: " + lce.Message);
                }
            }

        }
        /// <summary>
        /// Handles the event raised when a user signs in to or out of the Lync client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _LyncClient_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ClientState.SignedOut:
                    if (e.OldState == ClientState.Initializing)
                    {
                        SignUserIn();
                    }
                    if (e.OldState == ClientState.SigningOut)
                    {
                        _LyncClient.BeginShutdown((ar) =>
                        {
                            _LyncClient.EndShutdown(ar);
                        }, null);
                    }
                    break;
                case ClientState.Uninitialized:
                    if (e.OldState == ClientState.ShuttingDown)
                    {
                        _LyncClient.StateChanged -= _LyncClient_StateChanged;
                        try
                        {
                            if (CloseAppConditionHit != null)
                            {
                                CloseAppConditionHit();
                            }
                        }
                        catch (InvalidOperationException oe)
                        {
                            System.Diagnostics.Debug.WriteLine("Invalid operation exception on close: " + oe.Message);
                        }
                    }
                    break;
                case ClientState.SignedIn:
                    if (UserIsSignedIn != null)
                    {
                        UserIsSignedIn(_LyncClient);
                    }
                    break;
            }
            if (ClientStateChanged != null)
            {
                ClientStateChanged(e.NewState.ToString());
            }


        }


    }
}
