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

namespace AudioVideoConversation
{
    /// <summary>
    /// Called when a button in the dial pad dialog is pressed.
    /// </summary>
    public delegate void DialPadPressed(string tone);

    /// <summary>
    /// Implements a simple dialog with the buttons of a dial pad.
    /// </summary>
    public partial class DialPadDialog : Form
    {
        /// <summary>
        /// Occurs when a button in the dial pad dialog is pressed.
        /// </summary>
        public event DialPadPressed DialPadPressed;

        public DialPadDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Reads the text of a button and notifies that it was pressed with the proper DTMF text.
        /// </summary>
        private void button_Click(object sender, EventArgs e)
        {
            //reads the text of the button
            Button button = sender as Button;
            if (button != null)
            {
                string tone = button.Text;

                //updates the label with the new tone being sent
                labelSentDTMFs.Text += tone;

                //notify listener that a DTMF tone is to be sent
                DialPadPressed onDialPadPressed = DialPadPressed;
                if (onDialPadPressed != null)
                {
                    onDialPadPressed(tone);
                }
            }
        }
    }
}
