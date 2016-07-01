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

namespace SearchContacts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        LyncClient LyncClient;
        ContactManager ContactManager;
        DateTime SearchStartTime;
        DateTime SearchEndTime;

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
                SearchStatisticsTextBox.Text = "Lync client State: " + LyncClient.State.ToString();
            }
            catch (Exception ex)
            {
                SearchStatisticsTextBox.Text = "Exception: " + ex.Message;
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchStartTime = DateTime.Now;
                SearchResultListBox.Items.Clear();
                SearchResultListBox.Items.Add("Search ...");
                if (PeopleSearchRadioButton.IsChecked == true)
                {
                    ContactManager.BeginSearch(SearchKeywordTextBox.Text, SearchProviders.Default, SearchFields.AllFields, SearchOptions.Default, 50, SearchCallback, new object[] { SearchKeywordTextBox.Text, SearchProviders.Default });
                }
                else if (SkillSearchRadioButton.IsChecked == true)
                {
                    ContactManager.BeginSearch(SearchKeywordTextBox.Text, SearchProviders.Expert, SearchFields.AllFields, SearchOptions.Default, 50, SearchCallback, new object[] { SearchKeywordTextBox.Text, SearchProviders.Expert });
                }
            }
            catch (Exception ex)
            {
                SearchStatisticsTextBox.Text = "Exception: " + ex.Message;
            }
        }

        private void SearchCallback(IAsyncResult ar)
        {
            try
            {
                SearchEndTime = DateTime.Now;
                SearchResults SearchResults = ContactManager.EndSearch(ar);
                object[] state = (object[])ar.AsyncState;

                string searchKeyword = (string)state[0];
                SearchProviders searchProvider = (SearchProviders)state[1];

                Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    () =>
                    {
                        SearchResultListBox.Items.Clear();
                        foreach (Contact contact in SearchResults.Contacts)
                        {
                            SearchResultListBox.Items.Add("Contact: " + contact.Uri);
                        }
                        foreach (Group group in SearchResults.Groups)
                        {
                            SearchResultListBox.Items.Add("Distribution Group: " + group.Name);
                        }
                        SearchStatisticsTextBox.Text +=
                            "\n\nSearch Results:" + SearchEndTime +
                            "\nSearch keyword => " + SearchKeywordTextBox.Text +
                            "\nSearch provider => " + searchProvider.ToString() +
                            "\nNumber of Contacts => " + SearchResults.Contacts.Count +
                            "\nNumber of Groups => " + SearchResults.Groups.Count +
                            "\nSearch duration => " + (SearchEndTime - SearchStartTime).ToString();
                    }));
            }
            catch (Exception ex)
            {
                SearchStatisticsTextBox.Text = "Exception: " + ex.Message;
            }
        }
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
