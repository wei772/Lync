/*=====================================================================
  This file is part of the Microsoft Unified Communications Code Samples.

  Copyright (C) 2012 Microsoft Corporation.  All rights reserved.

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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Lync.Model;

namespace SignInOut
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LyncClient lyncClient;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                lyncClient = LyncClient.GetClient();
                Log("got client");
                UpdateUI();
                lyncClient.StateChanged += new EventHandler<ClientStateChangedEventArgs>(lyncClient_StateChanged);
            }
            catch (Exception ex)
            {
                Log("Error in getting Lync client object: " + ex.Message);
            }
        }

        void lyncClient_SignInDelayed(object sender, SignInDelayedEventArgs e)
        {
            MessageBox.Show("e.EstimatedStartDelay = " + e.EstimatedStartDelay.ToString());
        }

        void lyncClient_CredentialRequested(object sender, CredentialRequestedEventArgs e)
        {
            Log("Credential Requested event is raised, calling Submit()");
            e.Submit(@"consoto\user1", "wewa1", e.IsPasswordSaved);
            Log("Submitted credential to complete sign in");
        }

        void lyncClient_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            UpdateUI();
            Log(string.Format("Lync client state changed: {0} -> {1}", e.OldState.ToString(), e.NewState.ToString()));
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(new Action(delegate()
                {
                    logTextBox.AppendText(message + "\n");
                }), null);
        }

        private void UpdateUI()
        {
            Dispatcher.Invoke(new Action(delegate()
            {
                isInUISuppressionModeTextBlock.Text = lyncClient.InSuppressedMode.ToString();
                isLyncSignedInTextBlock.Text = lyncClient.State.ToString();
            }), null);
        }

        private void signOutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log("Calling BeginSignOut()");
                lyncClient.BeginSignOut(signOutCallback, null);
            }
            catch (Exception ex)
            {
                Log("Signing out error: " + ex.Message);
            }
        }

        private void signOutCallback(IAsyncResult ar)
        {
            Log("Sign Out Callback, calling EndSignOut()");
            lyncClient.EndSignOut(ar);
        }

        private void signInButton_Click(object sender, RoutedEventArgs e)
        {
            string userUri = signInAddressTextBox.Text;
            string domainAndUsername = usernameTextBox.Text;
            string password = passwordBox.Password;

            if (lyncClient.State == ClientState.SignedOut)
            {
                Log("Calling BeginSignIn()");
                lyncClient.BeginSignIn(userUri, domainAndUsername, password, SignInCallback, null);
            }

        }

        private void InitializeCallback(IAsyncResult ar)
        {
            Log("Initialize Callback, calling EndInitialize()");
            lyncClient.EndInitialize(ar);
        }

        private void SignInCallback(IAsyncResult ar)
        {
            Log("Sign In Callback, calling EndSignIn()");
            lyncClient.EndSignIn(ar);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // We need to shutdown the client before the application exits.
            // Here, we assume that the Lync is in the SignedOut state.
        }

        private void ShutdownCallback(IAsyncResult ar)
        {
            Log("Shutdown Callback, calling EndShutdown()");
            lyncClient.EndShutdown(ar);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            lyncClient.CredentialRequested += new EventHandler<CredentialRequestedEventArgs>(lyncClient_CredentialRequested);
            lyncClient.SignInDelayed += new EventHandler<SignInDelayedEventArgs>(lyncClient_SignInDelayed);
            Log("Calling BeginInitialize()");
            lyncClient.BeginInitialize(InitializeCallback, null);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
                Log("BeginShutdown client");
                lyncClient.BeginShutdown(ShutdownCallback, null);
        }
    }
}
