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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;

// Added to use Lync related APIs.
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Device;


namespace Devices_Scenarios
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region private fields

        private LyncClient lyncClient;          // LyncClient is the main class for accessing Lync functionality.
        private DeviceManager deviceManager;    // DeviceManager class is the main class for accessing device related functions.
        private Dispatcher dispatcher;          // Used to manage the app functionality and not specific to Lync SDK.

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // get Lync client
            try
            {
                lyncClient = LyncClient.GetClient();
            }
            catch (ClientNotFoundException ex)
            {
                // Lync is not started.
                Log(ex.ToString());
                return;
            }
            catch (NotSignedInException ex)
            {
                // Lync app is running but no signed in.
                Log(ex.ToString());
                return;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                return;
            }

            // get the device manager class. 
            deviceManager = lyncClient.DeviceManager;
            RefreshUI();
        }

        #region Update UI

        /// <summary>
        /// Get the active audio device and displays it name.
        /// </summary>
        private void GetActiveAudioDevice()
        {
            try
            {
                Device activeAudioDevice = deviceManager.ActiveAudioDevice;
                textBlockActiveAudioDevice.Text = activeAudioDevice.Name;
            }
            catch (Exception ex)
            {
                textBlockActiveAudioDevice.Text = "ERROR";
                Log(ex.ToString());
            }
        }

        /// <summary>
        /// Get the active video device and dislays its name.
        /// </summary>
        private void GetActiveVideoDevice()
        {
            try
            {
                Device activeVideoDevice = deviceManager.ActiveVideoDevice;
                textBlockActiveVideoDevice.Text = activeVideoDevice.Name;
            }
            catch (Exception ex)
            {
                textBlockActiveVideoDevice.Text = "ERROR";
                Log(ex.ToString());
            }
        }

        /// <summary>
        /// Gets all the audio devices plugged into the PC
        /// </summary>
        private void GetAllAudioDevices()
        {
            // List box will show output of Device.Name method instead of Device.ToString()
            listBoxDevices.DisplayMemberPath = "Name";
            // Clear previous items from the ListBox.
            listBoxDevices.Items.Clear();

            IList<Device> audioDevices = deviceManager.AudioDevices;
            foreach (Device d in audioDevices)
            {
                listBoxDevices.Items.Add(d);
            }
        }

        /// <summary>
        /// Gets all the video devices plugged into the PC
        /// </summary>
        private void GetAllVideoDevices()
        {
            // List box will show output of Device.Name method instead of Device.ToString()
            listBoxDevices.DisplayMemberPath = "Name";
            // Clear previous items from the ListBox.
            listBoxDevices.Items.Clear();

            IList<Device> videoDevices = deviceManager.VideoDevices;
            foreach (Device d in videoDevices)
            {
                listBoxDevices.Items.Add(d);
            }
        }

        /// <summary>
        /// Call all methods to get all devices and active devices.
        /// </summary>
        private void RefreshUI()
        {
            if (this.deviceManager != null)
            {
                if ((bool)radioButtonAudio.IsChecked)
                {
                    GetAllAudioDevices();
                }

                if ((bool)radioButtonVideo.IsChecked)
                {
                    GetAllVideoDevices();
                }

                GetActiveAudioDevice();
                GetActiveVideoDevice();
            }
        }

        #endregion

        #region Respond to actions on the UI.

        /// <summary>
        /// This method will set the currently selected Audio or video device as the Active Device.
        /// </summary>
        private void buttonSetActiveDevice_Click(object sender, RoutedEventArgs e)
        {
            Device toSet = (Device)listBoxDevices.SelectedItem;

            if ((bool)radioButtonAudio.IsChecked)
            {
                // Setting Audio device
                deviceManager.ActiveAudioDevice = (AudioDevice)toSet;
            }

            if ((bool)radioButtonVideo.IsChecked)
            {
                // Setting Video device
                deviceManager.ActiveVideoDevice = (VideoDevice)toSet;
            }

            RefreshUI();
        }

        /// <summary>
        /// Plays the file on the Communication device.
        /// </summary>
        private void buttonPlayAudioFile_Click(object sender, RoutedEventArgs e)
        {
            this.deviceManager.BeginPlayAudioFile(textBoxAudioFilePath.Text, AudioPlayBackModes.Communication, true, null, null);
        }

        /// <summary>
        /// Related to sample and not specific to Lync SDK
        /// Used to ensure latest information is visible in the app.
        /// </summary>
        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshUI();
        }

        #endregion

        #region helper methods

        private void Log(string msg)
        {
            textBlockLog.Text = msg + "\n" + textBlockLog.Text;
        }

        #endregion

    }
}
