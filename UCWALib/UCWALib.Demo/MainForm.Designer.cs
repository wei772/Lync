namespace UCWALib.Demo
{
    partial class MainForm
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
            this.txtToken = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSubject = new System.Windows.Forms.TextBox();
            this.txtMeetingURI = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.linkMeeting = new System.Windows.Forms.LinkLabel();
            this.btnStartMeeting = new System.Windows.Forms.Button();
            this.linkToken = new System.Windows.Forms.LinkLabel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.chkAdmin = new System.Windows.Forms.CheckBox();
            this.linkToken2 = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // txtToken
            // 
            this.txtToken.Location = new System.Drawing.Point(12, 35);
            this.txtToken.Multiline = true;
            this.txtToken.Name = "txtToken";
            this.txtToken.Size = new System.Drawing.Size(640, 192);
            this.txtToken.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Token:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(380, 230);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Subject:";
            // 
            // txtSubject
            // 
            this.txtSubject.Location = new System.Drawing.Point(383, 249);
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.Size = new System.Drawing.Size(269, 20);
            this.txtSubject.TabIndex = 3;
            // 
            // txtMeetingURI
            // 
            this.txtMeetingURI.Location = new System.Drawing.Point(15, 249);
            this.txtMeetingURI.Name = "txtMeetingURI";
            this.txtMeetingURI.ReadOnly = true;
            this.txtMeetingURI.Size = new System.Drawing.Size(362, 20);
            this.txtMeetingURI.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 230);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Meeting URI:";
            // 
            // linkMeeting
            // 
            this.linkMeeting.AutoSize = true;
            this.linkMeeting.Location = new System.Drawing.Point(18, 291);
            this.linkMeeting.Name = "linkMeeting";
            this.linkMeeting.Size = new System.Drawing.Size(0, 13);
            this.linkMeeting.TabIndex = 6;
            this.linkMeeting.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkMeeting_LinkClicked);
            // 
            // btnStartMeeting
            // 
            this.btnStartMeeting.Enabled = false;
            this.btnStartMeeting.Location = new System.Drawing.Point(383, 290);
            this.btnStartMeeting.Name = "btnStartMeeting";
            this.btnStartMeeting.Size = new System.Drawing.Size(269, 23);
            this.btnStartMeeting.TabIndex = 7;
            this.btnStartMeeting.Text = "Start Meeting";
            this.btnStartMeeting.UseVisualStyleBackColor = true;
            this.btnStartMeeting.Click += new System.EventHandler(this.btnStartMeeting_Click);
            // 
            // linkToken
            // 
            this.linkToken.AutoSize = true;
            this.linkToken.Location = new System.Drawing.Point(54, 16);
            this.linkToken.Name = "linkToken";
            this.linkToken.Size = new System.Drawing.Size(58, 13);
            this.linkToken.TabIndex = 8;
            this.linkToken.TabStop = true;
            this.linkToken.Text = "Get Token";
            this.linkToken.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblStatus.Location = new System.Drawing.Point(567, 16);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 9;
            // 
            // chkAdmin
            // 
            this.chkAdmin.AutoSize = true;
            this.chkAdmin.Location = new System.Drawing.Point(115, 16);
            this.chkAdmin.Name = "chkAdmin";
            this.chkAdmin.Size = new System.Drawing.Size(435, 17);
            this.chkAdmin.TabIndex = 11;
            this.chkAdmin.Text = "Admin Consent(Need Consent by Admin for a new AAD before anyone can Get Token)";
            this.chkAdmin.UseVisualStyleBackColor = true;
            // 
            // linkToken2
            // 
            this.linkToken2.AutoSize = true;
            this.linkToken2.Location = new System.Drawing.Point(54, 3);
            this.linkToken2.Name = "linkToken2";
            this.linkToken2.Size = new System.Drawing.Size(154, 13);
            this.linkToken2.TabIndex = 12;
            this.linkToken2.TabStop = true;
            this.linkToken2.Text = "Get Token(without Login page)";
            this.linkToken2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkToken2_LinkClicked);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 325);
            this.Controls.Add(this.linkToken2);
            this.Controls.Add(this.chkAdmin);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.linkToken);
            this.Controls.Add(this.btnStartMeeting);
            this.Controls.Add(this.linkMeeting);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtMeetingURI);
            this.Controls.Add(this.txtSubject);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtToken);
            this.Name = "MainForm";
            this.Text = "Get Skype Meeting Url Demo";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtToken;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSubject;
        private System.Windows.Forms.TextBox txtMeetingURI;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkMeeting;
        private System.Windows.Forms.Button btnStartMeeting;
        private System.Windows.Forms.LinkLabel linkToken;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.CheckBox chkAdmin;
        private System.Windows.Forms.LinkLabel linkToken2;
    }
}

