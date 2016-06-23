namespace AudioVideoConversation
{
    partial class ConversationWindow
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelConvesation = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelModality = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelAudioChannel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelVideoChannel = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBoxRoster = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.listBoxRosterContacts = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAddRosterContact = new System.Windows.Forms.Button();
            this.buttonRemoveRosterContact = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonConnectAudio = new System.Windows.Forms.Button();
            this.buttonDisconnectAudio = new System.Windows.Forms.Button();
            this.buttonHold = new System.Windows.Forms.Button();
            this.buttonRetrieve = new System.Windows.Forms.Button();
            this.buttonTransfer = new System.Windows.Forms.Button();
            this.buttonConsultTransfer = new System.Windows.Forms.Button();
            this.buttonForward = new System.Windows.Forms.Button();
            this.buttonSendDTMF = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.buttonReject = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panelOutgoingVideo = new System.Windows.Forms.Panel();
            this.panelIncomingVideo = new System.Windows.Forms.Panel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonStartVideo = new System.Windows.Forms.Button();
            this.buttonStopVideo = new System.Windows.Forms.Button();
            this.checkBoxAutoTerminateOnIdle = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.groupBoxRoster.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.statusStrip, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxRoster, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(599, 508);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabelConvesation,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabelModality,
            this.toolStripStatusLabel4,
            this.toolStripStatusLabelAudioChannel,
            this.toolStripStatusLabel5,
            this.toolStripStatusLabelVideoChannel});
            this.statusStrip.Location = new System.Drawing.Point(0, 488);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(599, 20);
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(80, 15);
            this.toolStripStatusLabel1.Text = "Conversation:";
            // 
            // toolStripStatusLabelConvesation
            // 
            this.toolStripStatusLabelConvesation.Name = "toolStripStatusLabelConvesation";
            this.toolStripStatusLabelConvesation.Size = new System.Drawing.Size(48, 15);
            this.toolStripStatusLabelConvesation.Text = "Inactive";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(66, 15);
            this.toolStripStatusLabel2.Text = "   Modality:";
            // 
            // toolStripStatusLabelModality
            // 
            this.toolStripStatusLabelModality.Name = "toolStripStatusLabelModality";
            this.toolStripStatusLabelModality.Size = new System.Drawing.Size(79, 15);
            this.toolStripStatusLabelModality.Text = "Disconnected";
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(95, 15);
            this.toolStripStatusLabel4.Text = "   AudioChannel:";
            // 
            // toolStripStatusLabelAudioChannel
            // 
            this.toolStripStatusLabelAudioChannel.Name = "toolStripStatusLabelAudioChannel";
            this.toolStripStatusLabelAudioChannel.Size = new System.Drawing.Size(36, 15);
            this.toolStripStatusLabelAudioChannel.Text = "None";
            // 
            // toolStripStatusLabel5
            // 
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(93, 15);
            this.toolStripStatusLabel5.Text = "   VideoChannel:";
            // 
            // toolStripStatusLabelVideoChannel
            // 
            this.toolStripStatusLabelVideoChannel.Name = "toolStripStatusLabelVideoChannel";
            this.toolStripStatusLabelVideoChannel.Size = new System.Drawing.Size(36, 15);
            this.toolStripStatusLabelVideoChannel.Text = "None";
            // 
            // groupBoxRoster
            // 
            this.groupBoxRoster.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxRoster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxRoster.Location = new System.Drawing.Point(3, 3);
            this.groupBoxRoster.Name = "groupBoxRoster";
            this.groupBoxRoster.Size = new System.Drawing.Size(593, 94);
            this.groupBoxRoster.TabIndex = 1;
            this.groupBoxRoster.TabStop = false;
            this.groupBoxRoster.Text = "Roster";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.Controls.Add(this.listBoxRosterContacts, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(587, 75);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // listBoxRosterContacts
            // 
            this.listBoxRosterContacts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxRosterContacts.FormattingEnabled = true;
            this.listBoxRosterContacts.Location = new System.Drawing.Point(3, 3);
            this.listBoxRosterContacts.Name = "listBoxRosterContacts";
            this.listBoxRosterContacts.Size = new System.Drawing.Size(501, 69);
            this.listBoxRosterContacts.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.buttonAddRosterContact, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.buttonRemoveRosterContact, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(510, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(74, 69);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // buttonAddRosterContact
            // 
            this.buttonAddRosterContact.Location = new System.Drawing.Point(3, 3);
            this.buttonAddRosterContact.Name = "buttonAddRosterContact";
            this.buttonAddRosterContact.Size = new System.Drawing.Size(68, 23);
            this.buttonAddRosterContact.TabIndex = 0;
            this.buttonAddRosterContact.Text = "Add...";
            this.buttonAddRosterContact.UseVisualStyleBackColor = true;
            this.buttonAddRosterContact.Click += new System.EventHandler(this.buttonAddRosterContact_Click);
            // 
            // buttonRemoveRosterContact
            // 
            this.buttonRemoveRosterContact.Enabled = false;
            this.buttonRemoveRosterContact.Location = new System.Drawing.Point(3, 37);
            this.buttonRemoveRosterContact.Name = "buttonRemoveRosterContact";
            this.buttonRemoveRosterContact.Size = new System.Drawing.Size(68, 23);
            this.buttonRemoveRosterContact.TabIndex = 1;
            this.buttonRemoveRosterContact.Text = "Remove";
            this.buttonRemoveRosterContact.UseVisualStyleBackColor = true;
            this.buttonRemoveRosterContact.Click += new System.EventHandler(this.buttonRemoveRosterContact_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel4);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 103);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(593, 94);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Audio";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 5;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel4.Controls.Add(this.buttonConnectAudio, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.buttonDisconnectAudio, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.buttonHold, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.buttonRetrieve, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.buttonTransfer, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.buttonConsultTransfer, 2, 1);
            this.tableLayoutPanel4.Controls.Add(this.buttonForward, 3, 0);
            this.tableLayoutPanel4.Controls.Add(this.buttonSendDTMF, 3, 1);
            this.tableLayoutPanel4.Controls.Add(this.buttonAccept, 4, 0);
            this.tableLayoutPanel4.Controls.Add(this.buttonReject, 4, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(587, 75);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // buttonConnectAudio
            // 
            this.buttonConnectAudio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonConnectAudio.Enabled = false;
            this.buttonConnectAudio.Location = new System.Drawing.Point(3, 3);
            this.buttonConnectAudio.Name = "buttonConnectAudio";
            this.buttonConnectAudio.Size = new System.Drawing.Size(111, 31);
            this.buttonConnectAudio.TabIndex = 0;
            this.buttonConnectAudio.Text = "Connect";
            this.buttonConnectAudio.UseVisualStyleBackColor = true;
            this.buttonConnectAudio.Click += new System.EventHandler(this.buttonConnectAudio_Click);
            // 
            // buttonDisconnectAudio
            // 
            this.buttonDisconnectAudio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonDisconnectAudio.Enabled = false;
            this.buttonDisconnectAudio.Location = new System.Drawing.Point(3, 40);
            this.buttonDisconnectAudio.Name = "buttonDisconnectAudio";
            this.buttonDisconnectAudio.Size = new System.Drawing.Size(111, 32);
            this.buttonDisconnectAudio.TabIndex = 1;
            this.buttonDisconnectAudio.Text = "Disconnect";
            this.buttonDisconnectAudio.UseVisualStyleBackColor = true;
            this.buttonDisconnectAudio.Click += new System.EventHandler(this.buttonDisconnectAudio_Click);
            // 
            // buttonHold
            // 
            this.buttonHold.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonHold.Enabled = false;
            this.buttonHold.Location = new System.Drawing.Point(120, 3);
            this.buttonHold.Name = "buttonHold";
            this.buttonHold.Size = new System.Drawing.Size(111, 31);
            this.buttonHold.TabIndex = 2;
            this.buttonHold.Text = "Hold";
            this.buttonHold.UseVisualStyleBackColor = true;
            this.buttonHold.Click += new System.EventHandler(this.buttonHold_Click);
            // 
            // buttonRetrieve
            // 
            this.buttonRetrieve.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonRetrieve.Enabled = false;
            this.buttonRetrieve.Location = new System.Drawing.Point(120, 40);
            this.buttonRetrieve.Name = "buttonRetrieve";
            this.buttonRetrieve.Size = new System.Drawing.Size(111, 32);
            this.buttonRetrieve.TabIndex = 3;
            this.buttonRetrieve.Text = "Retrieve";
            this.buttonRetrieve.UseVisualStyleBackColor = true;
            this.buttonRetrieve.Click += new System.EventHandler(this.buttonRetrieve_Click);
            // 
            // buttonTransfer
            // 
            this.buttonTransfer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonTransfer.Enabled = false;
            this.buttonTransfer.Location = new System.Drawing.Point(237, 3);
            this.buttonTransfer.Name = "buttonTransfer";
            this.buttonTransfer.Size = new System.Drawing.Size(111, 31);
            this.buttonTransfer.TabIndex = 4;
            this.buttonTransfer.Text = "Transfer...";
            this.buttonTransfer.UseVisualStyleBackColor = true;
            this.buttonTransfer.Click += new System.EventHandler(this.buttonTransfer_Click);
            // 
            // buttonConsultTransfer
            // 
            this.buttonConsultTransfer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonConsultTransfer.Enabled = false;
            this.buttonConsultTransfer.Location = new System.Drawing.Point(237, 40);
            this.buttonConsultTransfer.Name = "buttonConsultTransfer";
            this.buttonConsultTransfer.Size = new System.Drawing.Size(111, 32);
            this.buttonConsultTransfer.TabIndex = 5;
            this.buttonConsultTransfer.Text = "Consult Transfer...";
            this.buttonConsultTransfer.UseVisualStyleBackColor = true;
            this.buttonConsultTransfer.Click += new System.EventHandler(this.buttonConsultTransfer_Click);
            // 
            // buttonForward
            // 
            this.buttonForward.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonForward.Enabled = false;
            this.buttonForward.Location = new System.Drawing.Point(354, 3);
            this.buttonForward.Name = "buttonForward";
            this.buttonForward.Size = new System.Drawing.Size(111, 31);
            this.buttonForward.TabIndex = 6;
            this.buttonForward.Text = "Forward...";
            this.buttonForward.UseVisualStyleBackColor = true;
            this.buttonForward.Click += new System.EventHandler(this.buttonForward_Click);
            // 
            // buttonSendDTMF
            // 
            this.buttonSendDTMF.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonSendDTMF.Enabled = false;
            this.buttonSendDTMF.Location = new System.Drawing.Point(354, 40);
            this.buttonSendDTMF.Name = "buttonSendDTMF";
            this.buttonSendDTMF.Size = new System.Drawing.Size(111, 32);
            this.buttonSendDTMF.TabIndex = 7;
            this.buttonSendDTMF.Text = "Send DTMF...";
            this.buttonSendDTMF.UseVisualStyleBackColor = true;
            this.buttonSendDTMF.Click += new System.EventHandler(this.buttonSendDTMF_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonAccept.Enabled = false;
            this.buttonAccept.Location = new System.Drawing.Point(471, 3);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(113, 31);
            this.buttonAccept.TabIndex = 8;
            this.buttonAccept.Text = "Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // buttonReject
            // 
            this.buttonReject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonReject.Enabled = false;
            this.buttonReject.Location = new System.Drawing.Point(471, 40);
            this.buttonReject.Name = "buttonReject";
            this.buttonReject.Size = new System.Drawing.Size(113, 32);
            this.buttonReject.TabIndex = 9;
            this.buttonReject.Text = "Reject";
            this.buttonReject.UseVisualStyleBackColor = true;
            this.buttonReject.Click += new System.EventHandler(this.buttonReject_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel5);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 203);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(593, 282);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Video";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 3;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel5.Controls.Add(this.panelOutgoingVideo, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.panelIncomingVideo, 2, 1);
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel6, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.checkBoxAutoTerminateOnIdle, 2, 2);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 3;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(587, 263);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(277, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = "Outgoing video";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(306, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(278, 30);
            this.label2.TabIndex = 1;
            this.label2.Text = "Incoming video";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelOutgoingVideo
            // 
            this.panelOutgoingVideo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelOutgoingVideo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOutgoingVideo.Location = new System.Drawing.Point(3, 33);
            this.panelOutgoingVideo.Name = "panelOutgoingVideo";
            this.panelOutgoingVideo.Size = new System.Drawing.Size(277, 187);
            this.panelOutgoingVideo.TabIndex = 2;
            // 
            // panelIncomingVideo
            // 
            this.panelIncomingVideo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelIncomingVideo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelIncomingVideo.Location = new System.Drawing.Point(306, 33);
            this.panelIncomingVideo.Name = "panelIncomingVideo";
            this.panelIncomingVideo.Size = new System.Drawing.Size(278, 187);
            this.panelIncomingVideo.TabIndex = 3;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Controls.Add(this.buttonStartVideo, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.buttonStopVideo, 1, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 226);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(277, 34);
            this.tableLayoutPanel6.TabIndex = 4;
            // 
            // buttonStartVideo
            // 
            this.buttonStartVideo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonStartVideo.Enabled = false;
            this.buttonStartVideo.Location = new System.Drawing.Point(3, 3);
            this.buttonStartVideo.Name = "buttonStartVideo";
            this.buttonStartVideo.Size = new System.Drawing.Size(132, 28);
            this.buttonStartVideo.TabIndex = 0;
            this.buttonStartVideo.Text = "Start";
            this.buttonStartVideo.UseVisualStyleBackColor = true;
            this.buttonStartVideo.Click += new System.EventHandler(this.buttonStartVideo_Click);
            // 
            // buttonStopVideo
            // 
            this.buttonStopVideo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonStopVideo.Enabled = false;
            this.buttonStopVideo.Location = new System.Drawing.Point(141, 3);
            this.buttonStopVideo.Name = "buttonStopVideo";
            this.buttonStopVideo.Size = new System.Drawing.Size(133, 28);
            this.buttonStopVideo.TabIndex = 1;
            this.buttonStopVideo.Text = "Stop";
            this.buttonStopVideo.UseVisualStyleBackColor = true;
            this.buttonStopVideo.Click += new System.EventHandler(this.buttonStopVideo_Click);
            // 
            // checkBoxAutoTerminateOnIdle
            // 
            this.checkBoxAutoTerminateOnIdle.AutoSize = true;
            this.checkBoxAutoTerminateOnIdle.Checked = true;
            this.checkBoxAutoTerminateOnIdle.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoTerminateOnIdle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxAutoTerminateOnIdle.Location = new System.Drawing.Point(306, 226);
            this.checkBoxAutoTerminateOnIdle.Name = "checkBoxAutoTerminateOnIdle";
            this.checkBoxAutoTerminateOnIdle.Size = new System.Drawing.Size(278, 34);
            this.checkBoxAutoTerminateOnIdle.TabIndex = 5;
            this.checkBoxAutoTerminateOnIdle.Text = "Close this window when the call disconnects";
            this.checkBoxAutoTerminateOnIdle.UseVisualStyleBackColor = true;
            this.checkBoxAutoTerminateOnIdle.CheckStateChanged += new System.EventHandler(this.checkBoxAutoTerminateOnIdle_CheckStateChanged);
            // 
            // ConversationWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(599, 508);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ConversationWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Conversation window";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConversationWindow_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.groupBoxRoster.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBoxRoster;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ListBox listBoxRosterContacts;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button buttonAddRosterContact;
        private System.Windows.Forms.Button buttonRemoveRosterContact;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button buttonConnectAudio;
        private System.Windows.Forms.Button buttonDisconnectAudio;
        private System.Windows.Forms.Button buttonHold;
        private System.Windows.Forms.Button buttonRetrieve;
        private System.Windows.Forms.Button buttonTransfer;
        private System.Windows.Forms.Button buttonConsultTransfer;
        private System.Windows.Forms.Button buttonForward;
        private System.Windows.Forms.Button buttonSendDTMF;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Button buttonReject;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelOutgoingVideo;
        private System.Windows.Forms.Panel panelIncomingVideo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Button buttonStartVideo;
        private System.Windows.Forms.Button buttonStopVideo;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelConvesation;
        private System.Windows.Forms.CheckBox checkBoxAutoTerminateOnIdle;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelModality;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelAudioChannel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel5;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelVideoChannel;
    }
}