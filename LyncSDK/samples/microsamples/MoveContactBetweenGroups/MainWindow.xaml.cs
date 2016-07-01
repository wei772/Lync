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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Group;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MoveContactBetweenGroups
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window , INotifyPropertyChanged
    {
        LyncClient LyncClient;
        ContactManager ContactManager;

        public ObservableCollection<GroupInfo> LeftGroupList { get; set; }
        public ObservableCollection<GroupInfo> RightGroupList { get; set; }
        public GroupInfo LeftCurrentGroup { get; set; }
        public GroupInfo RightCurrentGroup { get; set; }
        public bool IsMoveButtonEnabled { get; set; }


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LyncClient = LyncClient.GetClient();
                ContactManager = LyncClient.ContactManager;
                this.DataContext = this;

                //Load custom group combo box and associated contact list from 
                //LyncClient.ContactManager.Groups property
                UpdateGroupList();
            }
            catch (Exception) {}
        }

        /// <summary>
        /// Fills the group combo boxes and contact lists with custom groups and their
        /// associated contacts from the Lync contact list
        /// </summary>
        void UpdateGroupList()
        {
            //Get the collection of Lync custom groups and add the collection to the left list
            LeftGroupList = new ObservableCollection<GroupInfo>(GetCustomGroups().OrderBy(p=>p.Name));

            //Add the same custom group collection to the right list
            RightGroupList = LeftGroupList;

            //Set the left and right combo boxes to the first custom group in the groups collection
            LeftCurrentGroup = LeftGroupList.First();
            RightCurrentGroup = RightGroupList.First();

            //Notify UI that data bound to controls have changed.
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("LeftGroupList"));
                PropertyChanged(this, new PropertyChangedEventArgs("RightGroupList"));
                PropertyChanged(this, new PropertyChangedEventArgs("LeftCurrentGroup"));
                PropertyChanged(this, new PropertyChangedEventArgs("RightCurrentGroup"));
            }
        }

        /// <summary>
        /// Iterates on the Group collection exposede by ContactManager.Groups property
        /// and gets any custom group except "Other Contacts"
        /// </summary>
        /// <returns>List of GroupInfo. A collection of GroupInfo objects that represent the
        /// Custom groups in a user's contact list
        /// </returns>
        List<GroupInfo> GetCustomGroups()
        {
            List<GroupInfo> result = new List<GroupInfo>();
            foreach(Group group in ContactManager.Groups)
            {
                if (group.Type == GroupType.CustomGroup && group.Name != "Other Contacts")
                {
                    result.Add(new GroupInfo(group));
                    ((GroupInfo)result.Last()).PropertyChanged += new PropertyChangedEventHandler(MainWindow_PropertyChanged);
                }
            }

            return result;
        }


        void MainWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateGroupList();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void LeftContactsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateIsMoveEnabled();
        }

        private void moveToRightButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContactInfo contactInfo = (ContactInfo)contactListBox1.SelectedItem;
                RightCurrentGroup.Group.BeginAddContact(contactInfo.Contact, AddContact_Callback, contactInfo);
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot move to target group.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void AddContact_Callback(IAsyncResult ar)
        {
            ContactInfo contactInfo = (ContactInfo)ar.AsyncState;
            RightCurrentGroup.Group.EndAddContact(ar);
            this.Dispatcher.Invoke(new Action(() =>
            {
                RightCurrentGroup.Contacts.Add(contactInfo);
            }), null);

            LeftCurrentGroup.Group.BeginRemoveContact(contactInfo.Contact, RemoveContact_Callback, contactInfo);

        }

        private void RemoveContact_Callback(IAsyncResult ar)
        {
            ContactInfo contactInfo = (ContactInfo)ar.AsyncState;
            RightCurrentGroup.Group.EndRemoveContact(ar);
            this.Dispatcher.Invoke(new Action(() =>
            {
                LeftCurrentGroup.Contacts.Remove(contactInfo);
            }), null);
        }

        private void groupComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateIsMoveEnabled();
        }

        private void groupComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateIsMoveEnabled();
        }

        void UpdateIsMoveEnabled()
        {
            IsMoveButtonEnabled = false;
            if (LeftCurrentGroup == null)
            {
                LeftCurrentGroup = LeftGroupList.First();
            }
            if (RightCurrentGroup == null)
            {
                RightCurrentGroup = RightGroupList.First();
            }

            if (LeftCurrentGroup.Name != RightCurrentGroup.Name && contactListBox1.SelectedItem != null)
            {
                IsMoveButtonEnabled = true;
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("IsMoveButtonEnabled"));
            }
        }
    }

    public class GroupInfo : INotifyPropertyChanged
    {
        public Group Group { get; private set; }
        public string Name { get; set; }
        public ObservableCollection<ContactInfo> Contacts { get; set; }

        public GroupInfo(Group group)
        {
            this.Group = group;
            this.Group.NameChanged += Group_PropertyChanged;
            this.Group.ContactAdded += Group_ContactMembershipChanged;
            this.Group.ContactRemoved += Group_ContactMembershipChanged;
            this.Name = group.Name;
            List<ContactInfo> contactInfoList = new List<ContactInfo>();
            foreach (Contact contact in group)
            {
                contactInfoList.Add(new ContactInfo(contact));
            }
            contactInfoList.OrderBy(c => c.DisplayName);
            this.Contacts = new ObservableCollection<ContactInfo>(contactInfoList);
        }


        void Group_ContactMembershipChanged(object sender, GroupMemberChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("MembershipChanged"));
            }
        }


        void Group_PropertyChanged(object sender, GroupNameChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }

        //Event to be raised to UI when bound property value changed for the bound "Name" property
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ContactInfo : INotifyPropertyChanged
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

        public string SipUri {
            get; 
            set; 
        }

        public Contact Contact { get; set; }

        public ContactInfo(Contact contact)
        {
            this.Contact = contact;
            this.SipUri = contact.Uri;
            displayName = (string)this.Contact.GetContactInformation(ContactInformationType.DisplayName);
            this.Contact.ContactInformationChanged += Contact_ContactInformationChanged;
        }

        void Contact_ContactInformationChanged(object sender, ContactInformationChangedEventArgs e)
        {
            if (e.ChangedContactInformation.Contains(ContactInformationType.DisplayName))
            {
                displayName = this.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString();
                if (PropertyChanged != null)
                { 
                    PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
                }
            }
        }

        //Event to be raised to UI when bound property value changed for the bound "DisplayName" property
        public event PropertyChangedEventHandler PropertyChanged;

    }

}
