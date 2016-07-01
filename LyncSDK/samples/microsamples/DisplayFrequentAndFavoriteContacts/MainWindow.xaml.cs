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
using Microsoft.Lync.Model.Group;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DisplayFrequentAndFavoriteContacts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        LyncClient client;
        public ObservableCollection<ContactInfo> FrequentContacts {get; private set;}
        public ObservableCollection<ContactInfo> FavoriteContacts {get; private set;}

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                FrequentContacts = new ObservableCollection<ContactInfo>();
                FavoriteContacts = new ObservableCollection<ContactInfo>();
                client = LyncClient.GetClient();
                client.StateChanged += new EventHandler<ClientStateChangedEventArgs>(client_StateChanged);
                statusTextBlock.Text = client.State.ToString();
                GetFrequentContacts();
                GetFavoriteContacts();
                this.DataContext = this;
            }
            catch (Exception ex)
            {
                statusTextBlock.Text = ex.Message;
            }

        }

        void client_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                statusTextBlock.Text = e.NewState.ToString();
            }), null);
        }

        private void GetFrequentContacts()
        {
            FrequentContacts.Clear();
            Group frequentContactsGroup = GetSpecialGroup(GroupType.FrequentContacts);
            if (frequentContactsGroup != null)
            {
                foreach (Contact contact in frequentContactsGroup)
                {
                    FrequentContacts.Add(GetContactInfo(contact));
                }
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("FrequentContacts"));
            }
        }

        private void GetFavoriteContacts()
        {
            FavoriteContacts.Clear();
            Group favoriteContactsGroup = GetSpecialGroup(GroupType.FavoriteContacts);
            if (favoriteContactsGroup != null)
            {
                foreach (Contact contact in favoriteContactsGroup)
                {
                    FavoriteContacts.Add(GetContactInfo(contact));
                }
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("FavoriteContacts"));
            }
        }

        private Group GetSpecialGroup(GroupType groupType)
        {
            foreach (Group group in client.ContactManager.Groups)
            {
                if (group.Type == groupType)
                {
                    return group;
                }
            }

            return null;
        }

        private ContactInfo GetContactInfo(Contact contact)
        {
            string displayName = (string)contact.GetContactInformation(ContactInformationType.DisplayName);
            Stream mStream = null;
            BitmapImage photoImage = null;
            try
            {
                mStream = (Stream)contact.GetContactInformation(ContactInformationType.Photo);
                if (mStream != null)
                {
                    photoImage = new BitmapImage();
                    photoImage.StreamSource = mStream;
                }
            }
            catch (Exception ex)
            {

            }
            return new ContactInfo(displayName, contact.Uri, photoImage);
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            GetFrequentContacts();
            GetFavoriteContacts();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ContactInfo
    {
        public string DisplayName { get; private set; }
        public string SipUri { get; private set; }
        public BitmapImage PhotoImage { get; private set; }

        public ContactInfo(string displayName, string sipUri, BitmapImage photoImage)
        {
            this.DisplayName = displayName;
            this.SipUri = sipUri;
            this.PhotoImage = photoImage;
        }
    }
}
