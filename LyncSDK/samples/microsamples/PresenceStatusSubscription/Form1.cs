//
// Copyright (c) Microsoft Corporation
//
// The example below illustrates how programs use Lync Client API to
// show the statuses of Lync contacts even when:
//
//  - Lync signs out and back in
//  - Lync exits and restarts
//  - Lync is terminated (or crashes) and restarts
//  - Lync loses and regains the network connection
//

using System;
using System.Windows.Forms;
using Microsoft.Lync.Model;

namespace WinFormsLyncPresenceStatus
{
    public partial class Form1 : Form
    {
        string _contactUri = "gluebot@microsoft.com";
        LyncClient _client = null;
        Contact _contact = null;

        public Form1()
        {
            InitializeComponent();
            WaitForLyncClient();
        }

        void WaitForLyncClient()
        {
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(OnTimer);
            timer.Start();
        }

        void OnTimer(object sender, EventArgs e)
        {
            if (_client == null)
            {
                try
                {
                    _client = LyncClient.GetClient();
                    _client.ClientDisconnected += OnClientDisconnected;
                    _client.StateChanged += OnClientStateChanged;

                    if (_client.State == ClientState.SignedIn)
                    {
                        SubscribeToLyncEvents();
                    }
                }
                catch (Exception)
                {
                    // we could not subscribe to the lync client and will retry later
                    ShowText("Waiting for Lync");
                }
            }
            else if (_client.State == ClientState.Invalid)
            {
                Cleanup();
                ShowText("Lync exited");
            }
        }

        void OnClientDisconnected(object sender, EventArgs e)
        {
            Cleanup();
            ShowText("Lync exited");
        }

        void Cleanup()
        {
            _contact = null;
            _client = null;
        }

        void OnClientStateChanged(object sender, ClientStateChangedEventArgs e)
        {
            try
            {
                switch (e.NewState)
                {
                    case ClientState.SignedIn:
                        SubscribeToLyncEvents();
                        break;

                    case ClientState.SignedOut:
                        _contact = null;
                        ShowText("Lync signed out");
                        break;
                }
            }
            catch (Exception)
            {
                Cleanup();
            }
        }

        void SubscribeToLyncEvents()
        {
            if (_contact == null)
            {
                _contact = _client.ContactManager.GetContactByUri(_contactUri);
                _contact.ContactInformationChanged += OnContactInformationChanged;
                ShowPresence(_contact);
            }
        }

        void OnContactInformationChanged(object sender, ContactInformationChangedEventArgs e)
        {
            try
            {
                ShowPresence(sender as Contact);
            }
            catch (Exception)
            {
                Cleanup();
            }
        }

        string GetPresenceText(Contact contact)
        {
            string name = contact.GetContactInformation(ContactInformationType.DisplayName).ToString();
            string activity = contact.GetContactInformation(ContactInformationType.Activity).ToString();
            return name + " - " + activity;
        }

        void ShowPresence(Contact contact)
        {
            ShowText(GetPresenceText(contact));
        }

        void ShowText(string text)
        {
            // update UI on the main thread
            Invoke(new Action(() => { Text = text; }));
        }
    }
}
