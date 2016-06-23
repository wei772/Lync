namespace ShareResources
{
    partial class ShareResources_Form
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
            this.ClientState_Label = new System.Windows.Forms.Label();
            this.ClientStateString_Label = new System.Windows.Forms.Label();
            this.Contact_ListBox = new System.Windows.Forms.ListBox();
            this.StartSharingResource_Button = new System.Windows.Forms.Button();
            this.Accept_Button = new System.Windows.Forms.Button();
            this.Decline_Button = new System.Windows.Forms.Button();
            this.Grant_Button = new System.Windows.Forms.Button();
            this.SharingAction_GroupBox = new System.Windows.Forms.GroupBox();
            this.Revoke_Button = new System.Windows.Forms.Button();
            this.Request_Button = new System.Windows.Forms.Button();
            this.Release_Button = new System.Windows.Forms.Button();
            this.Disconnect_Button = new System.Windows.Forms.Button();
            this.SharingParticipationState_Label = new System.Windows.Forms.Label();
            this.SharingParticipationStateString_Label = new System.Windows.Forms.Label();
            this.SharedResources_ListBox = new System.Windows.Forms.ListBox();
            this.ConversationActions_Group = new System.Windows.Forms.GroupBox();
            this.EndConversation_Button = new System.Windows.Forms.Button();
            this.Start_Button = new System.Windows.Forms.Button();
            this.ResourceController_Label = new System.Windows.Forms.Label();
            this.ResourceControllerName_Label = new System.Windows.Forms.Label();
            this.ShareableResources_Label = new System.Windows.Forms.Label();
            this.ContactList_Label = new System.Windows.Forms.Label();
            this.SharedResource_Label = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RejectSharing_Button = new System.Windows.Forms.Button();
            this.AcceptSharing_Button = new System.Windows.Forms.Button();
            this.RefreshResource_Button = new System.Windows.Forms.Button();
            this.AlertLevel_Textbox = new System.Windows.Forms.TextBox();
            this.SideCheck = new System.Windows.Forms.CheckBox();
            this.StartLync_Button = new System.Windows.Forms.Button();
            this.UserName_Label = new System.Windows.Forms.Label();
            this.StopLync_Button = new System.Windows.Forms.Button();
            this.autoSizeView_Checkbox = new System.Windows.Forms.CheckBox();
            this.ClientDisplayMode_Label = new System.Windows.Forms.Label();
            this.AliasNumber_Numeric = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.ShowChrome_Checkbox = new System.Windows.Forms.CheckBox();
            this.SharingAction_GroupBox.SuspendLayout();
            this.ConversationActions_Group.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AliasNumber_Numeric)).BeginInit();
            this.SuspendLayout();
            // 
            // ClientState_Label
            // 
            this.ClientState_Label.AutoSize = true;
            this.ClientState_Label.Location = new System.Drawing.Point(11, 113);
            this.ClientState_Label.Name = "ClientState_Label";
            this.ClientState_Label.Size = new System.Drawing.Size(64, 13);
            this.ClientState_Label.TabIndex = 0;
            this.ClientState_Label.Text = "Client State:";
            // 
            // ClientStateString_Label
            // 
            this.ClientStateString_Label.AutoSize = true;
            this.ClientStateString_Label.Location = new System.Drawing.Point(110, 115);
            this.ClientStateString_Label.Name = "ClientStateString_Label";
            this.ClientStateString_Label.Size = new System.Drawing.Size(58, 13);
            this.ClientStateString_Label.TabIndex = 1;
            this.ClientStateString_Label.Text = "Signed out";
            // 
            // Contact_ListBox
            // 
            this.Contact_ListBox.FormattingEnabled = true;
            this.Contact_ListBox.Location = new System.Drawing.Point(11, 163);
            this.Contact_ListBox.Name = "Contact_ListBox";
            this.Contact_ListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.Contact_ListBox.Size = new System.Drawing.Size(180, 82);
            this.Contact_ListBox.TabIndex = 2;
            // 
            // StartSharingResource_Button
            // 
            this.StartSharingResource_Button.Location = new System.Drawing.Point(10, 497);
            this.StartSharingResource_Button.Name = "StartSharingResource_Button";
            this.StartSharingResource_Button.Size = new System.Drawing.Size(153, 23);
            this.StartSharingResource_Button.TabIndex = 3;
            this.StartSharingResource_Button.Text = "4) Share Local Resource";
            this.StartSharingResource_Button.UseVisualStyleBackColor = true;
            this.StartSharingResource_Button.Click += new System.EventHandler(this.StartSharingResource_Button_Click);
            // 
            // Accept_Button
            // 
            this.Accept_Button.Enabled = false;
            this.Accept_Button.Location = new System.Drawing.Point(11, 28);
            this.Accept_Button.Name = "Accept_Button";
            this.Accept_Button.Size = new System.Drawing.Size(130, 23);
            this.Accept_Button.TabIndex = 4;
            this.Accept_Button.Text = "Accept";
            this.Accept_Button.UseVisualStyleBackColor = true;
            this.Accept_Button.Click += new System.EventHandler(this.Accept_Button_Click);
            // 
            // Decline_Button
            // 
            this.Decline_Button.Enabled = false;
            this.Decline_Button.Location = new System.Drawing.Point(11, 57);
            this.Decline_Button.Name = "Decline_Button";
            this.Decline_Button.Size = new System.Drawing.Size(130, 23);
            this.Decline_Button.TabIndex = 5;
            this.Decline_Button.Text = "Decline";
            this.Decline_Button.UseVisualStyleBackColor = true;
            this.Decline_Button.Click += new System.EventHandler(this.Decline_Button_Click);
            // 
            // Grant_Button
            // 
            this.Grant_Button.Enabled = false;
            this.Grant_Button.Location = new System.Drawing.Point(11, 86);
            this.Grant_Button.Name = "Grant_Button";
            this.Grant_Button.Size = new System.Drawing.Size(130, 23);
            this.Grant_Button.TabIndex = 6;
            this.Grant_Button.Text = "Grant";
            this.Grant_Button.UseVisualStyleBackColor = true;
            this.Grant_Button.Click += new System.EventHandler(this.Grant_Button_Click);
            // 
            // SharingAction_GroupBox
            // 
            this.SharingAction_GroupBox.Controls.Add(this.Revoke_Button);
            this.SharingAction_GroupBox.Controls.Add(this.Request_Button);
            this.SharingAction_GroupBox.Controls.Add(this.Release_Button);
            this.SharingAction_GroupBox.Controls.Add(this.Accept_Button);
            this.SharingAction_GroupBox.Controls.Add(this.Grant_Button);
            this.SharingAction_GroupBox.Controls.Add(this.Decline_Button);
            this.SharingAction_GroupBox.Location = new System.Drawing.Point(233, 115);
            this.SharingAction_GroupBox.Name = "SharingAction_GroupBox";
            this.SharingAction_GroupBox.Size = new System.Drawing.Size(162, 216);
            this.SharingAction_GroupBox.TabIndex = 7;
            this.SharingAction_GroupBox.TabStop = false;
            this.SharingAction_GroupBox.Text = "Take a control action";
            // 
            // Revoke_Button
            // 
            this.Revoke_Button.Enabled = false;
            this.Revoke_Button.Location = new System.Drawing.Point(11, 115);
            this.Revoke_Button.Name = "Revoke_Button";
            this.Revoke_Button.Size = new System.Drawing.Size(130, 23);
            this.Revoke_Button.TabIndex = 9;
            this.Revoke_Button.Text = "Revoke";
            this.Revoke_Button.UseVisualStyleBackColor = true;
            this.Revoke_Button.Click += new System.EventHandler(this.Revoke_Button_Click);
            // 
            // Request_Button
            // 
            this.Request_Button.Enabled = false;
            this.Request_Button.Location = new System.Drawing.Point(11, 144);
            this.Request_Button.Name = "Request_Button";
            this.Request_Button.Size = new System.Drawing.Size(130, 23);
            this.Request_Button.TabIndex = 8;
            this.Request_Button.Text = "Request";
            this.Request_Button.UseVisualStyleBackColor = true;
            this.Request_Button.Click += new System.EventHandler(this.Request_Button_Click);
            // 
            // Release_Button
            // 
            this.Release_Button.Enabled = false;
            this.Release_Button.Location = new System.Drawing.Point(11, 173);
            this.Release_Button.Name = "Release_Button";
            this.Release_Button.Size = new System.Drawing.Size(130, 23);
            this.Release_Button.TabIndex = 7;
            this.Release_Button.Text = "Release";
            this.Release_Button.UseVisualStyleBackColor = true;
            this.Release_Button.Click += new System.EventHandler(this.Release_Button_Click);
            // 
            // Disconnect_Button
            // 
            this.Disconnect_Button.Enabled = false;
            this.Disconnect_Button.Location = new System.Drawing.Point(11, 525);
            this.Disconnect_Button.Name = "Disconnect_Button";
            this.Disconnect_Button.Size = new System.Drawing.Size(152, 23);
            this.Disconnect_Button.TabIndex = 10;
            this.Disconnect_Button.Text = "Stop sharing";
            this.Disconnect_Button.UseVisualStyleBackColor = true;
            this.Disconnect_Button.Click += new System.EventHandler(this.StopSharing_Button_Click);
            // 
            // SharingParticipationState_Label
            // 
            this.SharingParticipationState_Label.AutoSize = true;
            this.SharingParticipationState_Label.Location = new System.Drawing.Point(241, 334);
            this.SharingParticipationState_Label.Name = "SharingParticipationState_Label";
            this.SharingParticipationState_Label.Size = new System.Drawing.Size(27, 13);
            this.SharingParticipationState_Label.TabIndex = 8;
            this.SharingParticipationState_Label.Text = "I am";
            // 
            // SharingParticipationStateString_Label
            // 
            this.SharingParticipationStateString_Label.AutoSize = true;
            this.SharingParticipationStateString_Label.Location = new System.Drawing.Point(265, 334);
            this.SharingParticipationStateString_Label.Name = "SharingParticipationStateString_Label";
            this.SharingParticipationStateString_Label.Size = new System.Drawing.Size(62, 13);
            this.SharingParticipationStateString_Label.TabIndex = 9;
            this.SharingParticipationStateString_Label.Text = " not sharing";
            // 
            // SharedResources_ListBox
            // 
            this.SharedResources_ListBox.FormattingEnabled = true;
            this.SharedResources_ListBox.HorizontalScrollbar = true;
            this.SharedResources_ListBox.Location = new System.Drawing.Point(10, 383);
            this.SharedResources_ListBox.Name = "SharedResources_ListBox";
            this.SharedResources_ListBox.Size = new System.Drawing.Size(181, 108);
            this.SharedResources_ListBox.TabIndex = 11;
            // 
            // ConversationActions_Group
            // 
            this.ConversationActions_Group.Controls.Add(this.EndConversation_Button);
            this.ConversationActions_Group.Controls.Add(this.Start_Button);
            this.ConversationActions_Group.Location = new System.Drawing.Point(10, 258);
            this.ConversationActions_Group.Name = "ConversationActions_Group";
            this.ConversationActions_Group.Size = new System.Drawing.Size(161, 89);
            this.ConversationActions_Group.TabIndex = 13;
            this.ConversationActions_Group.TabStop = false;
            this.ConversationActions_Group.Text = "2) Start a conversation";
            // 
            // EndConversation_Button
            // 
            this.EndConversation_Button.Enabled = false;
            this.EndConversation_Button.Location = new System.Drawing.Point(16, 50);
            this.EndConversation_Button.Name = "EndConversation_Button";
            this.EndConversation_Button.Size = new System.Drawing.Size(130, 23);
            this.EndConversation_Button.TabIndex = 0;
            this.EndConversation_Button.Text = "End";
            this.EndConversation_Button.UseVisualStyleBackColor = true;
            this.EndConversation_Button.Click += new System.EventHandler(this.EndConversation_Button_Click);
            // 
            // Start_Button
            // 
            this.Start_Button.Enabled = false;
            this.Start_Button.Location = new System.Drawing.Point(16, 21);
            this.Start_Button.Name = "Start_Button";
            this.Start_Button.Size = new System.Drawing.Size(130, 23);
            this.Start_Button.TabIndex = 16;
            this.Start_Button.Text = "Start";
            this.Start_Button.UseVisualStyleBackColor = true;
            this.Start_Button.Click += new System.EventHandler(this.Start_Button_Click);
            // 
            // ResourceController_Label
            // 
            this.ResourceController_Label.AutoSize = true;
            this.ResourceController_Label.Location = new System.Drawing.Point(241, 393);
            this.ResourceController_Label.Name = "ResourceController_Label";
            this.ResourceController_Label.Size = new System.Drawing.Size(103, 13);
            this.ResourceController_Label.TabIndex = 14;
            this.ResourceController_Label.Text = "Resource Controller:";
            // 
            // ResourceControllerName_Label
            // 
            this.ResourceControllerName_Label.AutoSize = true;
            this.ResourceControllerName_Label.Location = new System.Drawing.Point(241, 415);
            this.ResourceControllerName_Label.Name = "ResourceControllerName_Label";
            this.ResourceControllerName_Label.Size = new System.Drawing.Size(0, 13);
            this.ResourceControllerName_Label.TabIndex = 15;
            // 
            // ShareableResources_Label
            // 
            this.ShareableResources_Label.AutoSize = true;
            this.ShareableResources_Label.Location = new System.Drawing.Point(11, 367);
            this.ShareableResources_Label.Name = "ShareableResources_Label";
            this.ShareableResources_Label.Size = new System.Drawing.Size(93, 13);
            this.ShareableResources_Label.TabIndex = 17;
            this.ShareableResources_Label.Text = "3) Pick a resource";
            // 
            // ContactList_Label
            // 
            this.ContactList_Label.AutoSize = true;
            this.ContactList_Label.Location = new System.Drawing.Point(10, 144);
            this.ContactList_Label.Name = "ContactList_Label";
            this.ContactList_Label.Size = new System.Drawing.Size(99, 13);
            this.ContactList_Label.TabIndex = 20;
            this.ContactList_Label.Text = "1) Choose contacts";
            // 
            // SharedResource_Label
            // 
            this.SharedResource_Label.AutoSize = true;
            this.SharedResource_Label.Location = new System.Drawing.Point(244, 431);
            this.SharedResource_Label.Name = "SharedResource_Label";
            this.SharedResource_Label.Size = new System.Drawing.Size(33, 13);
            this.SharedResource_Label.TabIndex = 21;
            this.SharedResource_Label.Text = "None";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RejectSharing_Button);
            this.groupBox1.Controls.Add(this.AcceptSharing_Button);
            this.groupBox1.Location = new System.Drawing.Point(236, 462);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(164, 96);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sharing Invitiation Action";
            // 
            // RejectSharing_Button
            // 
            this.RejectSharing_Button.Enabled = false;
            this.RejectSharing_Button.Location = new System.Drawing.Point(11, 48);
            this.RejectSharing_Button.Name = "RejectSharing_Button";
            this.RejectSharing_Button.Size = new System.Drawing.Size(130, 23);
            this.RejectSharing_Button.TabIndex = 1;
            this.RejectSharing_Button.Text = "Reject";
            this.RejectSharing_Button.UseVisualStyleBackColor = true;
            this.RejectSharing_Button.Click += new System.EventHandler(this.RejectSharing_Button_Click);
            // 
            // AcceptSharing_Button
            // 
            this.AcceptSharing_Button.Enabled = false;
            this.AcceptSharing_Button.Location = new System.Drawing.Point(11, 19);
            this.AcceptSharing_Button.Name = "AcceptSharing_Button";
            this.AcceptSharing_Button.Size = new System.Drawing.Size(130, 23);
            this.AcceptSharing_Button.TabIndex = 0;
            this.AcceptSharing_Button.Text = "Accept";
            this.AcceptSharing_Button.UseVisualStyleBackColor = true;
            this.AcceptSharing_Button.Click += new System.EventHandler(this.AcceptSharing_Button_Click);
            // 
            // RefreshResource_Button
            // 
            this.RefreshResource_Button.Location = new System.Drawing.Point(10, 588);
            this.RefreshResource_Button.Name = "RefreshResource_Button";
            this.RefreshResource_Button.Size = new System.Drawing.Size(153, 23);
            this.RefreshResource_Button.TabIndex = 23;
            this.RefreshResource_Button.Text = "Refresh resources";
            this.RefreshResource_Button.UseVisualStyleBackColor = true;
            this.RefreshResource_Button.Click += new System.EventHandler(this.RefreshResource_Button_Click);
            // 
            // AlertLevel_Textbox
            // 
            this.AlertLevel_Textbox.Location = new System.Drawing.Point(244, 351);
            this.AlertLevel_Textbox.Multiline = true;
            this.AlertLevel_Textbox.Name = "AlertLevel_Textbox";
            this.AlertLevel_Textbox.Size = new System.Drawing.Size(154, 39);
            this.AlertLevel_Textbox.TabIndex = 24;
            this.AlertLevel_Textbox.Visible = false;
            // 
            // SideCheck
            // 
            this.SideCheck.AutoSize = true;
            this.SideCheck.Location = new System.Drawing.Point(13, 13);
            this.SideCheck.Name = "SideCheck";
            this.SideCheck.Size = new System.Drawing.Size(83, 17);
            this.SideCheck.TabIndex = 26;
            this.SideCheck.Text = "Side-by-side";
            this.SideCheck.UseVisualStyleBackColor = true;
            // 
            // StartLync_Button
            // 
            this.StartLync_Button.Location = new System.Drawing.Point(218, 9);
            this.StartLync_Button.Name = "StartLync_Button";
            this.StartLync_Button.Size = new System.Drawing.Size(75, 23);
            this.StartLync_Button.TabIndex = 27;
            this.StartLync_Button.Text = "Start Lync";
            this.StartLync_Button.UseVisualStyleBackColor = true;
            this.StartLync_Button.Click += new System.EventHandler(this.StartLync_Button_Click);
            // 
            // UserName_Label
            // 
            this.UserName_Label.AutoSize = true;
            this.UserName_Label.Location = new System.Drawing.Point(8, 86);
            this.UserName_Label.Name = "UserName_Label";
            this.UserName_Label.Size = new System.Drawing.Size(16, 13);
            this.UserName_Label.TabIndex = 28;
            this.UserName_Label.Text = " --";
            // 
            // StopLync_Button
            // 
            this.StopLync_Button.Location = new System.Drawing.Point(299, 9);
            this.StopLync_Button.Name = "StopLync_Button";
            this.StopLync_Button.Size = new System.Drawing.Size(75, 23);
            this.StopLync_Button.TabIndex = 29;
            this.StopLync_Button.Text = "Stop Lync";
            this.StopLync_Button.UseVisualStyleBackColor = true;
            this.StopLync_Button.Click += new System.EventHandler(this.StopLync_Button_Click);
            // 
            // autoSizeView_Checkbox
            // 
            this.autoSizeView_Checkbox.AutoSize = true;
            this.autoSizeView_Checkbox.Checked = true;
            this.autoSizeView_Checkbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoSizeView_Checkbox.Location = new System.Drawing.Point(11, 34);
            this.autoSizeView_Checkbox.Name = "autoSizeView_Checkbox";
            this.autoSizeView_Checkbox.Size = new System.Drawing.Size(94, 17);
            this.autoSizeView_Checkbox.TabIndex = 30;
            this.autoSizeView_Checkbox.Text = "Auto-size view";
            this.autoSizeView_Checkbox.UseVisualStyleBackColor = true;
            this.autoSizeView_Checkbox.CheckedChanged += new System.EventHandler(this.autoSizeView_Checkbox_CheckedChanged);
            // 
            // ClientDisplayMode_Label
            // 
            this.ClientDisplayMode_Label.AutoSize = true;
            this.ClientDisplayMode_Label.Location = new System.Drawing.Point(26, 61);
            this.ClientDisplayMode_Label.Name = "ClientDisplayMode_Label";
            this.ClientDisplayMode_Label.Size = new System.Drawing.Size(0, 13);
            this.ClientDisplayMode_Label.TabIndex = 31;
            // 
            // AliasNumber_Numeric
            // 
            this.AliasNumber_Numeric.Location = new System.Drawing.Point(169, 9);
            this.AliasNumber_Numeric.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.AliasNumber_Numeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.AliasNumber_Numeric.Name = "AliasNumber_Numeric";
            this.AliasNumber_Numeric.Size = new System.Drawing.Size(43, 20);
            this.AliasNumber_Numeric.TabIndex = 32;
            this.AliasNumber_Numeric.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.AliasNumber_Numeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(127, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 33;
            this.label1.Text = "User";
            // 
            // ShowChrome_Checkbox
            // 
            this.ShowChrome_Checkbox.AutoSize = true;
            this.ShowChrome_Checkbox.Location = new System.Drawing.Point(11, 55);
            this.ShowChrome_Checkbox.Name = "ShowChrome_Checkbox";
            this.ShowChrome_Checkbox.Size = new System.Drawing.Size(92, 17);
            this.ShowChrome_Checkbox.TabIndex = 35;
            this.ShowChrome_Checkbox.Text = "Show Chrome";
            this.ShowChrome_Checkbox.UseVisualStyleBackColor = true;
            // 
            // ShareResources_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 638);
            this.Controls.Add(this.ShowChrome_Checkbox);
            this.Controls.Add(this.Disconnect_Button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.AliasNumber_Numeric);
            this.Controls.Add(this.ClientDisplayMode_Label);
            this.Controls.Add(this.autoSizeView_Checkbox);
            this.Controls.Add(this.StopLync_Button);
            this.Controls.Add(this.UserName_Label);
            this.Controls.Add(this.StartLync_Button);
            this.Controls.Add(this.SideCheck);
            this.Controls.Add(this.AlertLevel_Textbox);
            this.Controls.Add(this.RefreshResource_Button);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.SharedResource_Label);
            this.Controls.Add(this.ContactList_Label);
            this.Controls.Add(this.ShareableResources_Label);
            this.Controls.Add(this.ResourceControllerName_Label);
            this.Controls.Add(this.ResourceController_Label);
            this.Controls.Add(this.ConversationActions_Group);
            this.Controls.Add(this.SharedResources_ListBox);
            this.Controls.Add(this.SharingParticipationStateString_Label);
            this.Controls.Add(this.SharingParticipationState_Label);
            this.Controls.Add(this.SharingAction_GroupBox);
            this.Controls.Add(this.StartSharingResource_Button);
            this.Controls.Add(this.Contact_ListBox);
            this.Controls.Add(this.ClientStateString_Label);
            this.Controls.Add(this.ClientState_Label);
            this.Name = "ShareResources_Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Resource Sharing Console";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShareResources_Form_FormClosing);
            this.Load += new System.EventHandler(this.ShareResources_Form_Load);
            this.SharingAction_GroupBox.ResumeLayout(false);
            this.ConversationActions_Group.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.AliasNumber_Numeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ClientState_Label;
        private System.Windows.Forms.Label ClientStateString_Label;
        private System.Windows.Forms.ListBox Contact_ListBox;
        private System.Windows.Forms.Button StartSharingResource_Button;
        private System.Windows.Forms.Button Accept_Button;
        private System.Windows.Forms.Button Decline_Button;
        private System.Windows.Forms.Button Grant_Button;
        private System.Windows.Forms.GroupBox SharingAction_GroupBox;
        private System.Windows.Forms.Button Release_Button;
        private System.Windows.Forms.Button Revoke_Button;
        private System.Windows.Forms.Button Request_Button;
        private System.Windows.Forms.Label SharingParticipationState_Label;
        private System.Windows.Forms.Label SharingParticipationStateString_Label;
        private System.Windows.Forms.ListBox SharedResources_ListBox;
        private System.Windows.Forms.GroupBox ConversationActions_Group;
        private System.Windows.Forms.Button EndConversation_Button;
        private System.Windows.Forms.Button Disconnect_Button;
        private System.Windows.Forms.Label ResourceController_Label;
        private System.Windows.Forms.Label ResourceControllerName_Label;
        private System.Windows.Forms.Button Start_Button;
        private System.Windows.Forms.Label ShareableResources_Label;
        private System.Windows.Forms.Label ContactList_Label;
        private System.Windows.Forms.Label SharedResource_Label;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button RejectSharing_Button;
        private System.Windows.Forms.Button AcceptSharing_Button;
        private System.Windows.Forms.Button RefreshResource_Button;
        private System.Windows.Forms.TextBox AlertLevel_Textbox;
        private System.Windows.Forms.CheckBox SideCheck;
        private System.Windows.Forms.Button StartLync_Button;
        private System.Windows.Forms.Label UserName_Label;
        private System.Windows.Forms.Button StopLync_Button;
        private System.Windows.Forms.CheckBox autoSizeView_Checkbox;
        private System.Windows.Forms.Label ClientDisplayMode_Label;
        private System.Windows.Forms.NumericUpDown AliasNumber_Numeric;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel _ViewPanel;
        private System.Windows.Forms.CheckBox ShowChrome_Checkbox;
    }
}

