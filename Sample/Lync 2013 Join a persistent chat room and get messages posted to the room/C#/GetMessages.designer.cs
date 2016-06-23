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
namespace GetMessages
{
    partial class GetMessages
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.LyncClientState_Label = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUnreadMessageCountChanged = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.RoomDescription_label = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnRetrieveAdditionalMessages = new System.Windows.Forms.Button();
            this.listBoxMessages = new System.Windows.Forms.ListBox();
            this.txtLastMessageID = new System.Windows.Forms.TextBox();
            this.NumberOfMessagesToGet_Text = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.FollowedRooms_listbox = new System.Windows.Forms.ListBox();
            this.label13 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.LyncClientState_Label);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(97, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 23);
            this.panel1.TabIndex = 1;
            // 
            // LyncClientState_Label
            // 
            this.LyncClientState_Label.AutoSize = true;
            this.LyncClientState_Label.Location = new System.Drawing.Point(111, 4);
            this.LyncClientState_Label.Name = "LyncClientState_Label";
            this.LyncClientState_Label.Size = new System.Drawing.Size(25, 13);
            this.LyncClientState_Label.TabIndex = 1;
            this.LyncClientState_Label.Text = "Null";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Lync Client State";
            // 
            // txtUnreadMessageCountChanged
            // 
            this.txtUnreadMessageCountChanged.AutoSize = true;
            this.txtUnreadMessageCountChanged.Location = new System.Drawing.Point(305, 86);
            this.txtUnreadMessageCountChanged.Name = "txtUnreadMessageCountChanged";
            this.txtUnreadMessageCountChanged.Size = new System.Drawing.Size(24, 13);
            this.txtUnreadMessageCountChanged.TabIndex = 8;
            this.txtUnreadMessageCountChanged.Text = "test";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(180, 86);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(119, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Unread Message Count";
            // 
            // RoomDescription_label
            // 
            this.RoomDescription_label.AutoSize = true;
            this.RoomDescription_label.Location = new System.Drawing.Point(305, 63);
            this.RoomDescription_label.Name = "RoomDescription_label";
            this.RoomDescription_label.Size = new System.Drawing.Size(84, 13);
            this.RoomDescription_label.TabIndex = 6;
            this.RoomDescription_label.Text = "room description";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(180, 63);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Description";
            // 
            // btnRetrieveAdditionalMessages
            // 
            this.btnRetrieveAdditionalMessages.Location = new System.Drawing.Point(545, 209);
            this.btnRetrieveAdditionalMessages.Name = "btnRetrieveAdditionalMessages";
            this.btnRetrieveAdditionalMessages.Size = new System.Drawing.Size(75, 49);
            this.btnRetrieveAdditionalMessages.TabIndex = 7;
            this.btnRetrieveAdditionalMessages.Text = "Retrieve Additional Messages";
            this.btnRetrieveAdditionalMessages.UseVisualStyleBackColor = true;
            this.btnRetrieveAdditionalMessages.Click += new System.EventHandler(this.btnRetrieveAdditionalMessages_Click);
            // 
            // listBoxMessages
            // 
            this.listBoxMessages.FormattingEnabled = true;
            this.listBoxMessages.HorizontalScrollbar = true;
            this.listBoxMessages.Location = new System.Drawing.Point(9, 123);
            this.listBoxMessages.Name = "listBoxMessages";
            this.listBoxMessages.Size = new System.Drawing.Size(517, 407);
            this.listBoxMessages.TabIndex = 8;
            // 
            // txtLastMessageID
            // 
            this.txtLastMessageID.Location = new System.Drawing.Point(545, 142);
            this.txtLastMessageID.Name = "txtLastMessageID";
            this.txtLastMessageID.Size = new System.Drawing.Size(100, 20);
            this.txtLastMessageID.TabIndex = 10;
            // 
            // txtNumMessages
            // 
            this.NumberOfMessagesToGet_Text.Location = new System.Drawing.Point(545, 183);
            this.NumberOfMessagesToGet_Text.Name = "txtNumMessages";
            this.NumberOfMessagesToGet_Text.Size = new System.Drawing.Size(100, 20);
            this.NumberOfMessagesToGet_Text.TabIndex = 11;
            this.NumberOfMessagesToGet_Text.Text = "27";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(545, 126);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(87, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Last Message ID";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(545, 167);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(107, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Number of Messages";
            // 
            // FollowedRooms_listbox
            // 
            this.FollowedRooms_listbox.FormattingEnabled = true;
            this.FollowedRooms_listbox.Location = new System.Drawing.Point(9, 48);
            this.FollowedRooms_listbox.Name = "FollowedRooms_listbox";
            this.FollowedRooms_listbox.Size = new System.Drawing.Size(165, 69);
            this.FollowedRooms_listbox.TabIndex = 38;
            this.FollowedRooms_listbox.SelectedValueChanged += new System.EventHandler(this.FollowedRoomsList_SelectedValueChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 31);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(85, 13);
            this.label13.TabIndex = 39;
            this.label13.Text = "Followed Rooms";
            // 
            // GetMessages
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(656, 551);
            this.Controls.Add(this.RoomDescription_label);
            this.Controls.Add(this.txtUnreadMessageCountChanged);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.FollowedRooms_listbox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.NumberOfMessagesToGet_Text);
            this.Controls.Add(this.txtLastMessageID);
            this.Controls.Add(this.listBoxMessages);
            this.Controls.Add(this.btnRetrieveAdditionalMessages);
            this.Controls.Add(this.panel1);
            this.Name = "GetMessages";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GetMessages";
            this.Load += new System.EventHandler(this.GetMessages_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label LyncClientState_Label;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label RoomDescription_label;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnRetrieveAdditionalMessages;
        private System.Windows.Forms.ListBox listBoxMessages;
        private System.Windows.Forms.TextBox txtLastMessageID;
        private System.Windows.Forms.TextBox NumberOfMessagesToGet_Text;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label txtUnreadMessageCountChanged;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ListBox FollowedRooms_listbox;
        private System.Windows.Forms.Label label13;
    }
}

