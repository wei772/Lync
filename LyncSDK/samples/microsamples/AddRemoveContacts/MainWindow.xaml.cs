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

namespace AddRemoveContacts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        LyncClient LyncClient;
        ContactManager ContactManager;
        Group OtherContactsGroup;

        public ObservableCollection<ContactInfo> OtherContacts { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                OtherContacts = new ObservableCollection<ContactInfo>();
                LyncClient = LyncClient.GetClient();
                ContactManager = LyncClient.ContactManager;
                OtherContactsGroup = GetOtherContactsGroup();
                OtherContactsGroup.ContactAdded += new EventHandler<GroupMemberChangedEventArgs>(OtherContactsGroup_ContactAdded);
                OtherContactsGroup.ContactRemoved += new EventHandler<GroupMemberChangedEventArgs>(OtherContactsGroup_ContactRemoved);
                this.DataContext = this;
                UpdateOtherContactsGroupContactInfo();
            }
            catch (Exception ex)
            {
            }
        }

        void OtherContactsGroup_ContactRemoved(object sender, GroupMemberChangedEventArgs e)
        {
            UpdateOtherContactsGroupContactInfo();
        }

        void OtherContactsGroup_ContactAdded(object sender, GroupMemberChangedEventArgs e)
        {
            UpdateOtherContactsGroupContactInfo();
        }

        private void UpdateOtherContactsGroupContactInfo()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                OtherContacts.Clear();
                foreach (Contact contact in OtherContactsGroup)
                {
                    OtherContacts.Add(new ContactInfo(contact));
                }
                OtherContacts = new ObservableCollection<ContactInfo>( OtherContacts.OrderBy(p => p.DisplayName));
            }), null);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("OtherContacts"));
            }
        }

        private Group GetOtherContactsGroup()
        {
            foreach (Group group in ContactManager.Groups)
            {
                if (group.Type == GroupType.CustomGroup && group.Name == "Other Contacts")
                {
                    return group;
                }
            }
            return null;            
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void AddContact_Callback(IAsyncResult ar)
        {
            OtherContactsGroup.EndAddContact(ar);
            this.Dispatcher.Invoke(new Action(() =>
            {
                UpdateOtherContactsGroupContactInfo();
            }), null);

        }

        private void RemoveContact_Callback(IAsyncResult ar)
        {
            OtherContactsGroup.EndRemoveContact(ar);
            this.Dispatcher.Invoke(new Action(() =>
            {
                UpdateOtherContactsGroupContactInfo();
            }), null);
        }

        private void AddContactButton_Click(object sender, RoutedEventArgs e)
        {
            string contactUri = ContactSipUriTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(contactUri))
            {
                Contact contact = ContactManager.GetContactByUri(contactUri);
                if (contact != null)
                {
                    try
                    {
                        OtherContactsGroup.BeginAddContact(contact, AddContact_Callback, null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Contact already exists",
                            "Add contact",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
        }

        private void RemoveContactButton_Click(object sender, RoutedEventArgs e)
        {
            if (otherContactsListBox.SelectedItem != null)
            {
                ContactInfo contactInfo = (ContactInfo)otherContactsListBox.SelectedItem;
                MessageBoxResult mbr = MessageBox.Show(
                    string.Format("Do you want to delete contact \"{0}\"?", contactInfo.DisplayName),
                    "Delete contact",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (mbr == MessageBoxResult.Yes)
                {
                    OtherContactsGroup.BeginRemoveContact(contactInfo.Contact, RemoveContact_Callback, null);
                }
            }
        }

        private void otherContactsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class ContactInfo
    {
        private string displayName = null;
        public string DisplayName 
        {
            get
            {
                if (string.IsNullOrEmpty(displayName))
                {
                    return SipUri;
                }
                else
                {
                    return displayName + " (" + SipUri + ")";
                }
            }
        }

        public string SipUri { get; set; }

        public Contact Contact { get; set; }

        public ContactInfo(Contact contact)
        {
            this.Contact = contact;
            this.SipUri = contact.Uri;
            displayName = (string)this.Contact.GetContactInformation(ContactInformationType.DisplayName);
        }
    }

}
