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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Extensibility;
using MessageBox = System.Windows.MessageBox;

namespace Automation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Microsoft.Lync.Model.Extensibility.Automation automation;


        public MainWindow()
        {
            InitializeComponent();
            try
            {
                //Start the conversation
                automation = LyncClient.GetAutomation();
            }
            catch (LyncClientException lyncClientException)
            {
                MessageBox.Show("Failed to connect to Lync.");
                Console.Out.WriteLine(lyncClientException);
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    MessageBox.Show("Failed to connect to Lync.");
                    Console.WriteLine("Error: " + systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }
        }

        #region Handlers for user interface controls events
        /// <summary>
        /// Handler for the Loaded event of the Window.
        /// Used to initialize the values shown in the UI (e.g. number of monitors and running processes)
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Get the current number of monitors
            for (int i = 0; i < SystemInformation.MonitorCount; i++)
            {
                monitorNumberComboBox.Items.Add(i);
            }

            //Get the current running processes with window handles
            foreach (Process process in Process.GetProcesses())
            {
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    processComboBox.Items.Add(process);
                }
                processComboBox.DisplayMemberPath = "ProcessName";
            }
        }

        /// <summary>
        /// Handler for the Add Participant button. Used to add a new participant to the participant list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddParticipantButton_Click(object sender, RoutedEventArgs e)
        {
            participantsListBox.Items.Add(participantTextBox.Text);
        }

        /// <summary>
        /// Handler of the Browse button click event. Used to show the Open File Dialog for selecting the file that will be transfered.
        /// </summary>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();
            if (ofDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePathTextBox.Text = ofDialog.FileName;
            }
        }

        /// <summary>
        /// Handler of the Call button click event. We use this to set up and start the new conversation
        /// </summary>
        private void CallButton_Click(object sender, RoutedEventArgs e)
        {
            //Get the conversation modalities and settings
            AutomationModalities conversationModes = 0;
            Dictionary<AutomationModalitySettings, object> conversationSettings =
                new Dictionary<AutomationModalitySettings, object>();

            //Instant Message modality
            if (instantMessageCheckBox.IsChecked.Value)
            {
                conversationModes |= AutomationModalities.InstantMessage;
                if (!String.IsNullOrEmpty(firstMessageTextBox.Text))
                {
                    conversationSettings.Add(AutomationModalitySettings.SendFirstInstantMessageImmediately, true);
                    conversationSettings.Add(AutomationModalitySettings.FirstInstantMessage, firstMessageTextBox.Text);
                }
            }

            //Audio modality
            if (audioCheckBox.IsChecked.Value)
            {
                conversationModes |= AutomationModalities.Audio;
            }

            //Video modality
            if (videoCheckBox.IsChecked.Value)
            {
                conversationModes |= AutomationModalities.Video;
            }

            //Application Sharing modality
            if (applicationSharingCheckBox.IsChecked.Value)
            {
                conversationModes |= AutomationModalities.ApplicationSharing;
                TextBlock resourceTextBlock = (TextBlock)resourceTypeComboBox.SelectedItem;
                AutomationModalitySettings resourceSetting =
                    (AutomationModalitySettings)Enum.Parse(typeof(AutomationModalitySettings),
                    "Shared" + resourceTextBlock.Text);
                
                object resourceValue = null;
                switch (resourceSetting)
                {
                    case AutomationModalitySettings.SharedDesktop:
                        break;
                    case AutomationModalitySettings.SharedMonitor:
                        if (monitorNumberComboBox.SelectedItem == null)
                        {
                            MessageBox.Show("Please select a monitor for sharing.");
                            return;
                        }
                        resourceValue = (int)monitorNumberComboBox.SelectedItem;
                        break;
                    case AutomationModalitySettings.SharedProcess:
                        Process selectedItem = processComboBox.SelectedItem as Process;
                        if (selectedItem == null)
                        {
                            MessageBox.Show("Please select a process for sharing.");
                            return;
                        }
                        resourceValue = selectedItem.Id;
                        break;
                }
                conversationSettings.Add(resourceSetting, resourceValue);
            }

            //File Sharing modality
            if (fileTransferCheckBox.IsChecked.Value)
            {
                if (!String.IsNullOrEmpty(filePathTextBox.Text))
                {
                    conversationModes |= AutomationModalities.FileTransfer;
                    conversationSettings.Add(AutomationModalitySettings.FilePathToTransfer, filePathTextBox.Text);
                }
                else
                {
                    MessageBox.Show("Please select a file to transfer");
                    return;
                }
            }

            //Get the participants
            List<string> participants = new List<string>(participantsListBox.Items.Count);
            foreach (string participant in participantsListBox.Items)
            {
                participants.Add(participant);
            }

            if (conversationModes == 0)
            {
                MessageBox.Show("Please select a conversation mode.");
                return;
            }

            if (participants.Count == 0)
            {
                MessageBox.Show("Please add a participant.");
                return;
            }

            if (automation != null)
            {
                try
                {
                    automation.BeginStartConversation(conversationModes, participants, conversationSettings,
                                                      StartConversationCallback, null);
                }
                catch (LyncClientException lyncClientException)
                {
                    MessageBox.Show("Call failed.");
                    Console.WriteLine(lyncClientException);
                }
                catch (SystemException systemException)
                {
                    if (IsLyncException(systemException))
                    {
                        // Log the exception thrown by the Lync Model API.
                        MessageBox.Show("Call failed.");
                        Console.WriteLine("Error: " + systemException);
                    }
                    else
                    {
                        // Rethrow the SystemException which did not come from the Lync Model API.
                        throw;
                    }
                }
            }
            else
            {
                MessageBox.Show("Lync was not initialized property.  Please restart this application.");
            }
        }

        /// <summary>
        /// Handler for the Instant Message CheckBox click event.
        /// </summary>
        private void InstantMessageCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //Update the state of Instant Message related elements in the user interface
            firstMessageTextBox.IsEnabled = instantMessageCheckBox.IsChecked.Value;

            //Update the Call Button enabled state
            SetCallButtonState();
        }

        /// <summary>
        /// Handler for the Audio CheckBox click event.
        /// </summary>
        private void AudioCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //Update the Call Button enabled state
            SetCallButtonState();
        }

        /// <summary>
        /// Handler for the Video CheckBox click event.
        /// </summary>
        private void VideoCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //Update the Call Button enabled state
            SetCallButtonState();
        }

        /// <summary>
        /// Handler for the Application Sharing CheckBox click event.
        /// </summary>
        private void ApplicationSharingCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //Update the state of Application Sharing related elements in the user interface
            resourceTypeComboBox.IsEnabled = applicationSharingCheckBox.IsChecked.Value;
            monitorNumberComboBox.IsEnabled = applicationSharingCheckBox.IsChecked.Value;
            processComboBox.IsEnabled = applicationSharingCheckBox.IsChecked.Value;

            //Update the Call Button enabled state
            SetCallButtonState();
        }

        /// <summary>
        /// Handler for the File Transfer CheckBox click event.
        /// </summary>
        private void FileTransferCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //Update the state of File Transfer related elements in the user interface
            filePathTextBox.IsEnabled = fileTransferCheckBox.IsChecked.Value;
            browseButton.IsEnabled = fileTransferCheckBox.IsChecked.Value;

            //Update the Call Button enabled state
            SetCallButtonState();
        }
        #endregion

        /// <summary>
        /// Callback invoked when Automation.BeginStartConversation is completed
        /// </summary>
        /// <param name="result">The status of the asynchronous operation</param>
        private void StartConversationCallback(IAsyncResult result)
        {
            try
            {
                automation.EndStartConversation(result);
            }
            catch (LyncClientException lyncClientException)
            {
                MessageBox.Show("Call failed.");
                Console.WriteLine(lyncClientException);
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    MessageBox.Show("Call failed.");
                    Console.WriteLine("Error: " + systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }
        }

        /// <summary>
        /// Sets the state of the Call Button (enabled or disabled) depending on the conversation modalities CheckBoxes checked status.
        /// </summary>
        private void SetCallButtonState()
        {
            //Enable making a call if any of the conversation modalities is selected.
            callButton.IsEnabled = 
                instantMessageCheckBox.IsChecked.Value || 
                audioCheckBox.IsChecked.Value ||
                videoCheckBox.IsChecked.Value || 
                applicationSharingCheckBox.IsChecked.Value ||
                fileTransferCheckBox.IsChecked.Value;
        }

        /// <summary>
        /// Identify if a particular SystemException is one of the exceptions which may be thrown
        /// by the Lync Model API.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private bool IsLyncException(SystemException ex)
        {
            return
                ex is NotImplementedException ||
                ex is ArgumentException ||
                ex is NullReferenceException ||
                ex is NotSupportedException ||
                ex is ArgumentOutOfRangeException ||
                ex is IndexOutOfRangeException ||
                ex is InvalidOperationException ||
                ex is TypeLoadException ||
                ex is TypeInitializationException ||
                ex is InvalidComObjectException ||
                ex is InvalidCastException;
        }
    }
}
