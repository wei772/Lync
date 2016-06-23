/*===================================================================== 
  This file is part of the Microsoft Office 2013 Lync Code Samples. 

  Copyright (C) 2013 Microsoft Corporation.  All rights reserved. 

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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShareResources
{
    public partial class SignInCreds : Form
    {
        public string UserName
        {
            get
            {
                return UserName_textBox.Text;
            }
        }
        public string Password
        {
            get
            {
                return Password_textBox.Text;
            }
        }


        public SignInCreds()
        {
            InitializeComponent();
        }
        public SignInCreds(string credentialType)
        {
            InitializeComponent();
            this.Text = credentialType + " credentials needed";
        }


        private void Ok_Button_Click(object sender, EventArgs e)
        {
           // this.Close();
        }
    }
}
