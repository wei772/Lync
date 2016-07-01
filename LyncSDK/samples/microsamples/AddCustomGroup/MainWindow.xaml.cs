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

namespace AddCustomGroup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        LyncClient LyncClient;
        ContactManager ContactManager;

        public ObservableCollection<GroupInfo> Groups { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Groups = new ObservableCollection<GroupInfo>();
                LyncClient = LyncClient.GetClient();
                ContactManager = LyncClient.ContactManager;
                ContactManager.GroupAdded += new EventHandler<GroupCollectionChangedEventArgs>(ContactManager_GroupAdded);
                ContactManager.GroupRemoved += new EventHandler<GroupCollectionChangedEventArgs>(ContactManager_GroupRemoved);
                this.DataContext = this;
                PopulateGroupInfo();
            }
            catch (Exception ex)
            {
            }
        }

        void ContactManager_GroupRemoved(object sender, GroupCollectionChangedEventArgs e)
        {
            UpdateGroupList();
        }

        void ContactManager_GroupAdded(object sender, GroupCollectionChangedEventArgs e)
        {
            UpdateGroupList();
        }

        private void UpdateGroupList()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                PopulateGroupInfo();
                if (groupListBox.Items.Count > 0)
                {
                    groupListBox.SelectedIndex = 0;
                }
            }), null);
        }

        private void PopulateGroupInfo()
        {
            Groups.Clear();

            Group favoriteGroup = GetSpecialGroup(GroupType.FavoriteContacts);
            Groups.Add(GetGroupInfo(favoriteGroup));
            Group frequentGroup = GetSpecialGroup(GroupType.FrequentContacts);
            Groups.Add(GetGroupInfo(frequentGroup));
            Group otherContactsGroup = GetOtherContactsGroup();
            Groups.Add(GetGroupInfo(otherContactsGroup));

            foreach (Group group in ContactManager.Groups)
            {
                if (group.Type == GroupType.CustomGroup && group.Name != "Other Contacts")
                {
                    Groups.Add(GetGroupInfo(group));
                }
            }

            foreach (Group group in ContactManager.Groups)
            {
                if (group.Type == GroupType.DistributionGroup)
                {
                    Groups.Add(GetGroupInfo(group));
                }
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Groups"));
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

        private Group GetSpecialGroup(GroupType groupType)
        {
            foreach (Group group in ContactManager.Groups)
            {
                if (group.Type == groupType)
                {
                    return group;
                }
            }
            return null;
        }

        private GroupInfo GetGroupInfo(Group group)
        {
            int ContactCounts = 0;

            GroupType type = group.Type;

            if (type != GroupType.DistributionGroup)
            {
                ContactCounts = group.Count;
            }

            return new GroupInfo(group);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void AddCustomGroupButton_Click(object sender, RoutedEventArgs e)
        {
            string groupName = CustomGroupNameTextBox.Text.Trim();
            if (!String.IsNullOrWhiteSpace(groupName))
            {
                foreach (GroupInfo gi in Groups)
                {
                    if (gi.GroupName.Equals(groupName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        MessageBox.Show(
                            string.Format("Group \"{0}\" is already in the contact list. You cannot add it now.", groupName),
                            "Failed adding custom group", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Exclamation);
                        return;
                    }
                }
                ContactManager.BeginAddGroup(CustomGroupNameTextBox.Text.Trim(), AddGroup_Callback, null);
            }
        }

        private void RemoveCustomGroupButton_Click(object sender, RoutedEventArgs e)
        {
            GroupInfo groupInfo = (GroupInfo)groupListBox.SelectedItem;
            MessageBoxResult mbr = MessageBox.Show(
                string.Format("Do you want to delete group \"{0}\"?", groupInfo.GroupName),
                "Remove Group", 
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (mbr == MessageBoxResult.Yes)
            {
                Group group = groupInfo.Group;
                ContactManager.BeginRemoveGroup(group, RemoveGroup_Callback, null);
            }
        }

        private void groupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (groupListBox.SelectedItem != null)
            {
                bool canRemove = ((GroupInfo)groupListBox.SelectedItem).CanRemove;
                RemoveCustomGroupButton.IsEnabled = canRemove;

                CustomGroupNameTextBox.Text = ((GroupInfo)groupListBox.SelectedItem).GroupName;
            }
        }

        private void AddGroup_Callback(IAsyncResult ar)
        {
            ContactManager.EndAddGroup(ar);
            //this.Dispatcher.Invoke(new Action(() =>
            //{
            //    PopulateGroupInfo();
            //    if (groupListBox.Items.Count > 0)
            //    {
            //        groupListBox.SelectedIndex = 0;
            //    }
            //}), null);
        }

        private void RemoveGroup_Callback(IAsyncResult ar)
        {
            ContactManager.EndRemoveGroup(ar);
            this.Dispatcher.Invoke(new Action(() =>
            {
                PopulateGroupInfo();
                if (groupListBox.Items.Count > 0)
                {
                    groupListBox.SelectedIndex = 0;
                }
            }), null);
        }
    }

    public class GroupInfo
    {
        public string GroupName { get; set; }
        public int ContactNumber { get; set; }
        public GroupType Type { get; set; }
        public bool CanRemove { get; set; }

        public Group Group { get; set; }

        public GroupInfo(Group group)
        {
            this.Group = group;
            this.GroupName = group.Name;
            this.ContactNumber = group.Count;
            this.Type = group.Type;
            if (group.Type == GroupType.DistributionGroup ||
                (group.Type == GroupType.CustomGroup && group.Name != "Other Contacts"))
            {
                this.CanRemove = true;
            }
            else
            {
                this.CanRemove = false;
            }
        }
    }

}
