/*=====================================================================
  File:      MainPage.xaml.cs

  Summary:   Main page view for MiniProposalTracker project. 

---------------------------------------------------------------------
This file is part of the Microsoft Lync SDK Code Samples

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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;

namespace MiniProposalTracker
{
    /// <summary>
    /// Class to support Contextual Conversation between two clients.
    /// </summary>
    public partial class MainPage : UserControl
    {
        //The GUID specifies the project. For a new application you should create another GUID
        //and then modify the reg file to reflect your new GUID. See SDK documentation for details.
        private const string ApplicationGuid = "{AFCFD912-E1B7-4CB4-92EE-174D5E7A35DD}";
        private readonly Conversation _conversation;
        private readonly String _subject;
        private readonly String _appData;

        public MainPage()
        {
            InitializeComponent();

            //Get the conversation from the Lync client
            try
            {
                _conversation = (Conversation)LyncClient.GetHostingConversation();
            }
            catch (LyncClientException lyncClientException)
            {
                Debug.WriteLine(lyncClientException);
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    Console.WriteLine("Error: " + systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }

            if (_conversation != null)
            {
                //Get the subject and appdata from the conversation. In this application they represent the
                //Proposals ProjectName and Description respectively.
                try
                {
                    _subject = (string)_conversation.Properties[ConversationProperty.Subject];
                    _appData = _conversation.GetApplicationData(ApplicationGuid);
                }
                catch (LyncClientException lyncClientException)
                {
                    Debug.WriteLine(lyncClientException);
                }
                catch (SystemException systemException)
                {
                    if (IsLyncException(systemException))
                    {
                        // Log the exception thrown by the Lync Model API.
                        Console.WriteLine("Error: " + systemException);
                    }
                    else
                    {
                        // Rethrow the SystemException which did not come from the Lync Model API.
                        throw;
                    }
                }

                //Event to get fired when a context is received from other participants.
                _conversation.ContextDataReceived += ConversationContextDataReceived;
                
                //By default, we want one of the checkboxes to get checked when the context loads.
                PieChartRadioButton.IsChecked = true;

                //Bind the subject to be the title and the app data to be the description.
                MainPortletFrame.PortletTitle = _subject;
                AppDataTextBlock.Text = _appData;
            }
        }

        /// <summary>
        /// Event fired when a context data is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ConversationContextDataReceived(object sender, ContextEventArgs e)
        {
            //Find the RadioButton (using its received name string) and make it checked. 
            //The Chart images' visibility property is bound to each RadioButton's checked property. Check MainPage.xaml for details.
            RadioButton receivingRadioButton =FindName(e.ContextData) as RadioButton;
            if (receivingRadioButton != null)
            {
                receivingRadioButton.IsChecked = true;
            }
        }

        /// <summary>
        /// Method responsible for sending the name of the checked RadioButton. 
        /// The try/catch is inserted to make sure that a friendly error message is 
        /// thrown when the context is sent to a participant that has left the conversation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectChartTypeRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            RadioButton checkedRadioButton = e.OriginalSource as RadioButton;
            try
            {
                if (checkedRadioButton != null)
                {
                    try
                    {
                        //Send the RadioButton name as the contextdata.
                        _conversation.BeginSendContextData(ApplicationGuid, "text/plain", checkedRadioButton.Name, null,null);
                    }
                    catch (LyncClientException lyncClientException)
                    {
                        Debug.WriteLine(lyncClientException);
                    }
                    catch (SystemException systemException)
                    {
                        if (IsLyncException(systemException))
                        {
                            // Log the exception thrown by the Lync Model API.
                            Console.WriteLine("Error: " + systemException);
                        }
                        else
                        {
                            // Rethrow the SystemException which did not come from the Lync Model API.
                            throw;
                        }
                    }

                    //If the context data has been sent, make the error text block invisible.
                    ErrorTextBlock.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception exception)
            {
                ErrorTextBlock.Text =
                    string.Format(
                        "Your context data was not sent. Please verify that there is at least one participant connected. Exception: {0}",
                        exception.Message);
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
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
                ex is InvalidCastException;
        }

    }
}
