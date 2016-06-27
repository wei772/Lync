namespace Microsoft.Rtc.Collaboration.Sample.VoiceXml
{
    partial class VoiceXmlSample
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
            this.startPageLabel = new System.Windows.Forms.Label();
            this.hangupCallButton = new System.Windows.Forms.Button();
            this.resultsTextBox = new System.Windows.Forms.TextBox();
            this.stopBrowserButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.resultsLabel = new System.Windows.Forms.Label();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabExecution = new System.Windows.Forms.TabPage();
            this.applyServerSettingsButton = new System.Windows.Forms.Button();
            this.serverPortTB = new System.Windows.Forms.TextBox();
            this.serverPortLabel = new System.Windows.Forms.Label();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.domainUserLabel = new System.Windows.Forms.Label();
            this.passwordTB = new System.Windows.Forms.TextBox();
            this.userUriTB = new System.Windows.Forms.TextBox();
            this.mcsUriLabel = new System.Windows.Forms.Label();
            this.domainUserTB = new System.Windows.Forms.TextBox();
            this.useCurrentUserCB = new System.Windows.Forms.CheckBox();
            this.credentialsLabel = new System.Windows.Forms.Label();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.callEventsCB = new System.Windows.Forms.CheckBox();
            this.clearAllButton = new System.Windows.Forms.Button();
            this.checkAllButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.syncRB = new System.Windows.Forms.RadioButton();
            this.asyncRB = new System.Windows.Forms.RadioButton();
            this.disconnectedCB = new System.Windows.Forms.CheckBox();
            this.disconnectingCB = new System.Windows.Forms.CheckBox();
            this.exitStartedCB = new System.Windows.Forms.CheckBox();
            this.cancelStartedCB = new System.Windows.Forms.CheckBox();
            this.startPageTextBox = new System.Windows.Forms.TextBox();
            this.callTransferCompleteCB = new System.Windows.Forms.CheckBox();
            this.eventsLabel = new System.Windows.Forms.Label();
            this.callTransferStartCB = new System.Windows.Forms.CheckBox();
            this.runCompletedCB = new System.Windows.Forms.CheckBox();
            this.pageLoadedCB = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.tabMain.SuspendLayout();
            this.tabExecution.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // startPageLabel
            // 
            this.startPageLabel.AutoSize = true;
            this.startPageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.startPageLabel.Location = new System.Drawing.Point(235, 23);
            this.startPageLabel.Name = "startPageLabel";
            this.startPageLabel.Size = new System.Drawing.Size(101, 17);
            this.startPageLabel.TabIndex = 10;
            this.startPageLabel.Text = "Start page URI";
            // 
            // hangupCallButton
            // 
            this.hangupCallButton.BackColor = System.Drawing.SystemColors.Control;
            this.hangupCallButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.hangupCallButton.Location = new System.Drawing.Point(23, 429);
            this.hangupCallButton.Name = "hangupCallButton";
            this.hangupCallButton.Size = new System.Drawing.Size(127, 33);
            this.hangupCallButton.TabIndex = 7;
            this.hangupCallButton.Text = "Hang up call";
            this.hangupCallButton.UseVisualStyleBackColor = false;
            this.hangupCallButton.Click += new System.EventHandler(this.hangupButton_Click);
            // 
            // resultsTextBox
            // 
            this.resultsTextBox.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.resultsTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.resultsTextBox.Location = new System.Drawing.Point(23, 193);
            this.resultsTextBox.Multiline = true;
            this.resultsTextBox.Name = "resultsTextBox";
            this.resultsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.resultsTextBox.Size = new System.Drawing.Size(607, 217);
            this.resultsTextBox.TabIndex = 6;
            this.resultsTextBox.TabStop = false;
            // 
            // stopBrowserButton
            // 
            this.stopBrowserButton.Location = new System.Drawing.Point(278, 430);
            this.stopBrowserButton.Name = "stopBrowserButton";
            this.stopBrowserButton.Size = new System.Drawing.Size(112, 32);
            this.stopBrowserButton.TabIndex = 10;
            this.stopBrowserButton.Text = "Stop Browser";
            this.stopBrowserButton.UseVisualStyleBackColor = true;
            this.stopBrowserButton.Click += new System.EventHandler(this.stopBrowserButton_Click);
            // 
            // exitButton
            // 
            this.exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.exitButton.Location = new System.Drawing.Point(553, 430);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(77, 32);
            this.exitButton.TabIndex = 11;
            this.exitButton.Text = "E&xit";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // resultsLabel
            // 
            this.resultsLabel.AutoSize = true;
            this.resultsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resultsLabel.Location = new System.Drawing.Point(20, 173);
            this.resultsLabel.Name = "resultsLabel";
            this.resultsLabel.Size = new System.Drawing.Size(118, 17);
            this.resultsLabel.TabIndex = 7;
            this.resultsLabel.Text = "Event Notification";
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabExecution);
            this.tabMain.Controls.Add(this.tabSettings);
            this.tabMain.Location = new System.Drawing.Point(12, 15);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(668, 515);
            this.tabMain.TabIndex = 10;
            // 
            // tabExecution
            // 
            this.tabExecution.Controls.Add(this.applyServerSettingsButton);
            this.tabExecution.Controls.Add(this.serverPortTB);
            this.tabExecution.Controls.Add(this.serverPortLabel);
            this.tabExecution.Controls.Add(this.passwordLabel);
            this.tabExecution.Controls.Add(this.domainUserLabel);
            this.tabExecution.Controls.Add(this.passwordTB);
            this.tabExecution.Controls.Add(this.userUriTB);
            this.tabExecution.Controls.Add(this.mcsUriLabel);
            this.tabExecution.Controls.Add(this.domainUserTB);
            this.tabExecution.Controls.Add(this.useCurrentUserCB);
            this.tabExecution.Controls.Add(this.credentialsLabel);
            this.tabExecution.Controls.Add(this.hangupCallButton);
            this.tabExecution.Controls.Add(this.stopBrowserButton);
            this.tabExecution.Controls.Add(this.resultsLabel);
            this.tabExecution.Controls.Add(this.exitButton);
            this.tabExecution.Controls.Add(this.resultsTextBox);
            this.tabExecution.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabExecution.Location = new System.Drawing.Point(4, 22);
            this.tabExecution.Name = "tabExecution";
            this.tabExecution.Padding = new System.Windows.Forms.Padding(3);
            this.tabExecution.Size = new System.Drawing.Size(660, 489);
            this.tabExecution.TabIndex = 0;
            this.tabExecution.Text = "Browser Execution";
            this.tabExecution.UseVisualStyleBackColor = true;
            // 
            // applyServerSettingsButton
            // 
            this.applyServerSettingsButton.BackColor = System.Drawing.SystemColors.Control;
            this.applyServerSettingsButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.applyServerSettingsButton.Location = new System.Drawing.Point(467, 122);
            this.applyServerSettingsButton.Name = "applyServerSettingsButton";
            this.applyServerSettingsButton.Size = new System.Drawing.Size(127, 46);
            this.applyServerSettingsButton.TabIndex = 6;
            this.applyServerSettingsButton.Text = "Apply Server Settings";
            this.applyServerSettingsButton.UseVisualStyleBackColor = false;
            this.applyServerSettingsButton.Click += new System.EventHandler(this.applyServerSettingsButton_Click);
            // 
            // serverPortTB
            // 
            this.serverPortTB.Location = new System.Drawing.Point(23, 24);
            this.serverPortTB.Name = "serverPortTB";
            this.serverPortTB.Size = new System.Drawing.Size(607, 23);
            this.serverPortTB.TabIndex = 1;
            this.serverPortTB.Text = "sip.contoso.com:443";
            // 
            // serverPortLabel
            // 
            this.serverPortLabel.AutoSize = true;
            this.serverPortLabel.Location = new System.Drawing.Point(20, 4);
            this.serverPortLabel.Name = "serverPortLabel";
            this.serverPortLabel.Size = new System.Drawing.Size(326, 17);
            this.serverPortLabel.TabIndex = 45;
            this.serverPortLabel.Text = "MCS Server FQDN and Port:  sip.contoso.com:443";
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.passwordLabel.Location = new System.Drawing.Point(20, 151);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(73, 17);
            this.passwordLabel.TabIndex = 44;
            this.passwordLabel.Text = "Password:";
            // 
            // domainUserLabel
            // 
            this.domainUserLabel.AutoSize = true;
            this.domainUserLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.domainUserLabel.Location = new System.Drawing.Point(20, 122);
            this.domainUserLabel.Name = "domainUserLabel";
            this.domainUserLabel.Size = new System.Drawing.Size(129, 17);
            this.domainUserLabel.TabIndex = 43;
            this.domainUserLabel.Text = "Domain\\Username:";
            // 
            // passwordTB
            // 
            this.passwordTB.Enabled = false;
            this.passwordTB.Location = new System.Drawing.Point(99, 145);
            this.passwordTB.Name = "passwordTB";
            this.passwordTB.PasswordChar = '*';
            this.passwordTB.Size = new System.Drawing.Size(311, 23);
            this.passwordTB.TabIndex = 5;
            // 
            // userUriTB
            // 
            this.userUriTB.Location = new System.Drawing.Point(23, 70);
            this.userUriTB.Name = "userUriTB";
            this.userUriTB.Size = new System.Drawing.Size(607, 23);
            this.userUriTB.TabIndex = 2;
            this.userUriTB.Text = "user@contoso.com";
            // 
            // mcsUriLabel
            // 
            this.mcsUriLabel.AutoSize = true;
            this.mcsUriLabel.Location = new System.Drawing.Point(20, 50);
            this.mcsUriLabel.Name = "mcsUriLabel";
            this.mcsUriLabel.Size = new System.Drawing.Size(265, 17);
            this.mcsUriLabel.TabIndex = 40;
            this.mcsUriLabel.Text = "URI to use with MCS: user@contoso.com";
            // 
            // domainUserTB
            // 
            this.domainUserTB.Enabled = false;
            this.domainUserTB.Location = new System.Drawing.Point(155, 116);
            this.domainUserTB.Name = "domainUserTB";
            this.domainUserTB.Size = new System.Drawing.Size(255, 23);
            this.domainUserTB.TabIndex = 4;
            // 
            // useCurrentUserCB
            // 
            this.useCurrentUserCB.AutoSize = true;
            this.useCurrentUserCB.Checked = true;
            this.useCurrentUserCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useCurrentUserCB.Location = new System.Drawing.Point(213, 95);
            this.useCurrentUserCB.Name = "useCurrentUserCB";
            this.useCurrentUserCB.Size = new System.Drawing.Size(197, 21);
            this.useCurrentUserCB.TabIndex = 3;
            this.useCurrentUserCB.Text = "Use Current Windows User";
            this.useCurrentUserCB.UseVisualStyleBackColor = true;
            this.useCurrentUserCB.CheckedChanged += new System.EventHandler(this.useCurrentUserCB_CheckedChanged);
            // 
            // credentialsLabel
            // 
            this.credentialsLabel.AutoSize = true;
            this.credentialsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.credentialsLabel.Location = new System.Drawing.Point(20, 96);
            this.credentialsLabel.Name = "credentialsLabel";
            this.credentialsLabel.Size = new System.Drawing.Size(187, 17);
            this.credentialsLabel.TabIndex = 37;
            this.credentialsLabel.Text = "Credentials to use with MCS:";
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.callEventsCB);
            this.tabSettings.Controls.Add(this.clearAllButton);
            this.tabSettings.Controls.Add(this.checkAllButton);
            this.tabSettings.Controls.Add(this.groupBox1);
            this.tabSettings.Controls.Add(this.disconnectedCB);
            this.tabSettings.Controls.Add(this.disconnectingCB);
            this.tabSettings.Controls.Add(this.exitStartedCB);
            this.tabSettings.Controls.Add(this.startPageLabel);
            this.tabSettings.Controls.Add(this.cancelStartedCB);
            this.tabSettings.Controls.Add(this.startPageTextBox);
            this.tabSettings.Controls.Add(this.callTransferCompleteCB);
            this.tabSettings.Controls.Add(this.eventsLabel);
            this.tabSettings.Controls.Add(this.callTransferStartCB);
            this.tabSettings.Controls.Add(this.runCompletedCB);
            this.tabSettings.Controls.Add(this.pageLoadedCB);
            this.tabSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabSettings.Location = new System.Drawing.Point(4, 22);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabSettings.Size = new System.Drawing.Size(660, 489);
            this.tabSettings.TabIndex = 1;
            this.tabSettings.Text = "Browser Settings";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // callEventsCB
            // 
            this.callEventsCB.AutoSize = true;
            this.callEventsCB.Checked = true;
            this.callEventsCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.callEventsCB.Location = new System.Drawing.Point(219, 276);
            this.callEventsCB.Name = "callEventsCB";
            this.callEventsCB.Size = new System.Drawing.Size(147, 21);
            this.callEventsCB.TabIndex = 29;
            this.callEventsCB.Text = "Call State Changes";
            this.callEventsCB.UseVisualStyleBackColor = true;
            // 
            // clearAllButton
            // 
            this.clearAllButton.Location = new System.Drawing.Point(380, 418);
            this.clearAllButton.Name = "clearAllButton";
            this.clearAllButton.Size = new System.Drawing.Size(135, 35);
            this.clearAllButton.TabIndex = 28;
            this.clearAllButton.Text = "Clear All";
            this.clearAllButton.UseVisualStyleBackColor = true;
            this.clearAllButton.Click += new System.EventHandler(this.clearAllButton_Click);
            // 
            // checkAllButton
            // 
            this.checkAllButton.Location = new System.Drawing.Point(196, 417);
            this.checkAllButton.Name = "checkAllButton";
            this.checkAllButton.Size = new System.Drawing.Size(140, 37);
            this.checkAllButton.TabIndex = 27;
            this.checkAllButton.Text = "Check All";
            this.checkAllButton.UseVisualStyleBackColor = true;
            this.checkAllButton.Click += new System.EventHandler(this.checkAllButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.syncRB);
            this.groupBox1.Controls.Add(this.asyncRB);
            this.groupBox1.Location = new System.Drawing.Point(26, 23);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(154, 139);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Processing Type";
            // 
            // syncRB
            // 
            this.syncRB.AutoSize = true;
            this.syncRB.Location = new System.Drawing.Point(23, 86);
            this.syncRB.Name = "syncRB";
            this.syncRB.Size = new System.Drawing.Size(109, 21);
            this.syncRB.TabIndex = 21;
            this.syncRB.Text = "Synchronous";
            this.syncRB.UseVisualStyleBackColor = true;
            // 
            // asyncRB
            // 
            this.asyncRB.AutoSize = true;
            this.asyncRB.Checked = true;
            this.asyncRB.Location = new System.Drawing.Point(23, 34);
            this.asyncRB.Name = "asyncRB";
            this.asyncRB.Size = new System.Drawing.Size(116, 21);
            this.asyncRB.TabIndex = 20;
            this.asyncRB.TabStop = true;
            this.asyncRB.Text = "Asynchronous";
            this.asyncRB.UseVisualStyleBackColor = true;
            // 
            // disconnectedCB
            // 
            this.disconnectedCB.AutoSize = true;
            this.disconnectedCB.Checked = true;
            this.disconnectedCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.disconnectedCB.Location = new System.Drawing.Point(460, 240);
            this.disconnectedCB.Name = "disconnectedCB";
            this.disconnectedCB.Size = new System.Drawing.Size(113, 21);
            this.disconnectedCB.TabIndex = 19;
            this.disconnectedCB.Text = "Disconnected";
            this.disconnectedCB.UseVisualStyleBackColor = true;
            // 
            // disconnectingCB
            // 
            this.disconnectingCB.AutoSize = true;
            this.disconnectingCB.Checked = true;
            this.disconnectingCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.disconnectingCB.Location = new System.Drawing.Point(460, 206);
            this.disconnectingCB.Name = "disconnectingCB";
            this.disconnectingCB.Size = new System.Drawing.Size(116, 21);
            this.disconnectingCB.TabIndex = 18;
            this.disconnectingCB.Text = "Disconnecting";
            this.disconnectingCB.UseVisualStyleBackColor = true;
            // 
            // exitStartedCB
            // 
            this.exitStartedCB.AutoSize = true;
            this.exitStartedCB.Checked = true;
            this.exitStartedCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.exitStartedCB.Location = new System.Drawing.Point(460, 276);
            this.exitStartedCB.Name = "exitStartedCB";
            this.exitStartedCB.Size = new System.Drawing.Size(99, 21);
            this.exitStartedCB.TabIndex = 17;
            this.exitStartedCB.Text = "Exit Started";
            this.exitStartedCB.UseVisualStyleBackColor = true;
            // 
            // cancelStartedCB
            // 
            this.cancelStartedCB.AutoSize = true;
            this.cancelStartedCB.Checked = true;
            this.cancelStartedCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cancelStartedCB.Location = new System.Drawing.Point(38, 276);
            this.cancelStartedCB.Name = "cancelStartedCB";
            this.cancelStartedCB.Size = new System.Drawing.Size(89, 21);
            this.cancelStartedCB.TabIndex = 16;
            this.cancelStartedCB.Text = "Canceling";
            this.cancelStartedCB.UseVisualStyleBackColor = true;
            // 
            // startPageTextBox
            // 
            this.startPageTextBox.Location = new System.Drawing.Point(238, 43);
            this.startPageTextBox.Name = "startPageTextBox";
            this.startPageTextBox.Size = new System.Drawing.Size(376, 23);
            this.startPageTextBox.TabIndex = 9;
            this.startPageTextBox.Text = "VoiceXMLSampleData\\Color.vxml";
            // 
            // callTransferCompleteCB
            // 
            this.callTransferCompleteCB.AutoSize = true;
            this.callTransferCompleteCB.Checked = true;
            this.callTransferCompleteCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.callTransferCompleteCB.Location = new System.Drawing.Point(218, 240);
            this.callTransferCompleteCB.Name = "callTransferCompleteCB";
            this.callTransferCompleteCB.Size = new System.Drawing.Size(152, 21);
            this.callTransferCompleteCB.TabIndex = 15;
            this.callTransferCompleteCB.Text = "Transfer Completed";
            this.callTransferCompleteCB.UseVisualStyleBackColor = true;
            // 
            // eventsLabel
            // 
            this.eventsLabel.AutoSize = true;
            this.eventsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.eventsLabel.Location = new System.Drawing.Point(23, 186);
            this.eventsLabel.Name = "eventsLabel";
            this.eventsLabel.Size = new System.Drawing.Size(98, 17);
            this.eventsLabel.TabIndex = 11;
            this.eventsLabel.Text = "Events to see:";
            // 
            // callTransferStartCB
            // 
            this.callTransferStartCB.AutoSize = true;
            this.callTransferStartCB.Checked = true;
            this.callTransferStartCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.callTransferStartCB.Location = new System.Drawing.Point(218, 206);
            this.callTransferStartCB.Name = "callTransferStartCB";
            this.callTransferStartCB.Size = new System.Drawing.Size(131, 21);
            this.callTransferStartCB.TabIndex = 14;
            this.callTransferStartCB.Text = "Transfer Started";
            this.callTransferStartCB.UseVisualStyleBackColor = true;
            // 
            // runCompletedCB
            // 
            this.runCompletedCB.AutoSize = true;
            this.runCompletedCB.Checked = true;
            this.runCompletedCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.runCompletedCB.Location = new System.Drawing.Point(38, 206);
            this.runCompletedCB.Name = "runCompletedCB";
            this.runCompletedCB.Size = new System.Drawing.Size(148, 21);
            this.runCompletedCB.TabIndex = 12;
            this.runCompletedCB.Text = "Session Completed";
            this.runCompletedCB.UseVisualStyleBackColor = true;
            // 
            // pageLoadedCB
            // 
            this.pageLoadedCB.AutoSize = true;
            this.pageLoadedCB.Checked = true;
            this.pageLoadedCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.pageLoadedCB.Location = new System.Drawing.Point(38, 240);
            this.pageLoadedCB.Name = "pageLoadedCB";
            this.pageLoadedCB.Size = new System.Drawing.Size(112, 21);
            this.pageLoadedCB.TabIndex = 13;
            this.pageLoadedCB.Text = "Page Loaded";
            this.pageLoadedCB.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(20, 144);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 17);
            this.label1.TabIndex = 44;
            this.label1.Text = "Password:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(20, 115);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 17);
            this.label2.TabIndex = 43;
            this.label2.Text = "Domain\\Username:";
            // 
            // textBox1
            // 
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(99, 138);
            this.textBox1.Name = "textBox1";
            this.textBox1.PasswordChar = '*';
            this.textBox1.Size = new System.Drawing.Size(311, 20);
            this.textBox1.TabIndex = 42;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(23, 63);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(387, 20);
            this.textBox2.TabIndex = 41;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(265, 17);
            this.label3.TabIndex = 40;
            this.label3.Text = "URI to use with MCS: user@contoso.com";
            // 
            // textBox3
            // 
            this.textBox3.Enabled = false;
            this.textBox3.Location = new System.Drawing.Point(155, 109);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(255, 20);
            this.textBox3.TabIndex = 39;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(213, 88);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(197, 21);
            this.checkBox1.TabIndex = 38;
            this.checkBox1.Text = "Use Current Windows User";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label4.Location = new System.Drawing.Point(20, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(187, 17);
            this.label4.TabIndex = 37;
            this.label4.Text = "Credentials to use with MCS:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(303, 430);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(126, 32);
            this.button1.TabIndex = 10;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.SystemColors.Control;
            this.button2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button2.Location = new System.Drawing.Point(37, 429);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(127, 33);
            this.button2.TabIndex = 2;
            this.button2.Text = "Hang up call";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // button3
            // 
            this.button3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button3.Location = new System.Drawing.Point(170, 430);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(127, 33);
            this.button3.TabIndex = 3;
            this.button3.Text = "Transfer Call";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(435, 430);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(112, 32);
            this.button4.TabIndex = 4;
            this.button4.Text = "Cancel Async";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(20, 173);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(118, 17);
            this.label5.TabIndex = 7;
            this.label5.Text = "Event Notification";
            // 
            // button5
            // 
            this.button5.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button5.Location = new System.Drawing.Point(553, 430);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(77, 32);
            this.button5.TabIndex = 5;
            this.button5.Text = "E&xit";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // textBox4
            // 
            this.textBox4.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.textBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.textBox4.Location = new System.Drawing.Point(23, 193);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox4.Size = new System.Drawing.Size(607, 217);
            this.textBox4.TabIndex = 6;
            // 
            // VoiceXmlSample
            // 
            this.AcceptButton = this.hangupCallButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.exitButton;
            this.ClientSize = new System.Drawing.Size(692, 555);
            this.Controls.Add(this.tabMain);
            this.Name = "VoiceXmlSample";
            this.Text = "VoiceXML Browser sample program";
            this.tabMain.ResumeLayout(false);
            this.tabExecution.ResumeLayout(false);
            this.tabExecution.PerformLayout();
            this.tabSettings.ResumeLayout(false);
            this.tabSettings.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button hangupCallButton;
        private System.Windows.Forms.Button stopBrowserButton;
        private System.Windows.Forms.Button exitButton;
        public System.Windows.Forms.TextBox resultsTextBox;
        private System.Windows.Forms.Label resultsLabel;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabExecution;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.CheckBox exitStartedCB;
        private System.Windows.Forms.CheckBox cancelStartedCB;
        private System.Windows.Forms.TextBox startPageTextBox;
        private System.Windows.Forms.CheckBox callTransferCompleteCB;
        private System.Windows.Forms.Label eventsLabel;
        private System.Windows.Forms.CheckBox callTransferStartCB;
        private System.Windows.Forms.CheckBox runCompletedCB;
        private System.Windows.Forms.CheckBox pageLoadedCB;
        private System.Windows.Forms.CheckBox disconnectedCB;
        private System.Windows.Forms.CheckBox disconnectingCB;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton syncRB;
        private System.Windows.Forms.RadioButton asyncRB;
        private System.Windows.Forms.Button clearAllButton;
        private System.Windows.Forms.Button checkAllButton;
        private System.Windows.Forms.Label startPageLabel;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.Label domainUserLabel;
        private System.Windows.Forms.TextBox passwordTB;
        private System.Windows.Forms.TextBox userUriTB;
        private System.Windows.Forms.Label mcsUriLabel;
        private System.Windows.Forms.TextBox domainUserTB;
        private System.Windows.Forms.CheckBox useCurrentUserCB;
        private System.Windows.Forms.Label credentialsLabel;
        private System.Windows.Forms.TextBox serverPortTB;
        private System.Windows.Forms.Label serverPortLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button5;
        public System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Button applyServerSettingsButton;
        private System.Windows.Forms.CheckBox callEventsCB;
    }

}

