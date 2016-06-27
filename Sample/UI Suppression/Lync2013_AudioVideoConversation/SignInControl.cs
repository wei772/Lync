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
using System.Windows.Forms;

using Microsoft.Lync.Model;

namespace AudioVideoConversation
{

    /// <summary>
    /// Implements a simple Sign-In UI.
    /// </summary>
    public partial class SignInControl : UserControl
    {
        //holds the Lync client instance
        private LyncClient client;

        public SignInControl(LyncClient client)
        {         
            InitializeComponent();

            //saves the lync client instance
            this.client = client;
        }

        /// <summary>
        /// Signs-In on Lync.
        /// </summary>
        private void buttonSignIn_Click(object sender, EventArgs e)
        {
            try
            {
                client.BeginSignIn(textBoxSignInAddress.Text, textBoxUser.Text, textBoxPassword.Text, HandleEndSignIn, null);
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
            }
            catch (SystemException systemException)
            {
                if (LyncModelExceptionHelper.IsLyncException(systemException))
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
        }

        private void HandleEndSignIn(IAsyncResult ar)
        {
            try
            {
                client.EndSignIn(ar);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
            }
        }
    }
}
