namespace BuildABot.Samples.UC
{
    partial class FrmMain
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
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.btnStartUCHost = new System.Windows.Forms.Button();
            this.txtSipUri = new System.Windows.Forms.TextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.txtReply = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnSendMessage
            // 
            this.btnSendMessage.Location = new System.Drawing.Point(204, 111);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(100, 23);
            this.btnSendMessage.TabIndex = 0;
            this.btnSendMessage.Text = "Send Message";
            this.btnSendMessage.UseVisualStyleBackColor = true;
            this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);
            // 
            // btnStartUCHost
            // 
            this.btnStartUCHost.Location = new System.Drawing.Point(98, 111);
            this.btnStartUCHost.Name = "btnStartUCHost";
            this.btnStartUCHost.Size = new System.Drawing.Size(100, 23);
            this.btnStartUCHost.TabIndex = 1;
            this.btnStartUCHost.Text = "Start UC Host";
            this.btnStartUCHost.UseVisualStyleBackColor = true;
            this.btnStartUCHost.Click += new System.EventHandler(this.btnStartUCHost_Click);
            // 
            // txtSipUri
            // 
            this.txtSipUri.Location = new System.Drawing.Point(108, 18);
            this.txtSipUri.Name = "txtSipUri";
            this.txtSipUri.Size = new System.Drawing.Size(155, 20);
            this.txtSipUri.TabIndex = 2;
            this.txtSipUri.Text = "sip:joe@contoso.com";
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(108, 44);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(155, 20);
            this.txtMessage.TabIndex = 3;
            this.txtMessage.Text = "abnormal temperature";
            // 
            // txtReply
            // 
            this.txtReply.Location = new System.Drawing.Point(108, 70);
            this.txtReply.Name = "txtReply";
            this.txtReply.Size = new System.Drawing.Size(405, 20);
            this.txtReply.TabIndex = 4;
            this.txtReply.Text = "It seems your house is getting too warm (80F). Do you want me to adjust the heate" +
    "r?";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(78, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(24, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "SIP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "System message";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "User message";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 149);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtReply);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.txtSipUri);
            this.Controls.Add(this.btnStartUCHost);
            this.Controls.Add(this.btnSendMessage);
            this.Name = "FrmMain";
            this.Text = "Home automation bot sample";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSendMessage;
        private System.Windows.Forms.Button btnStartUCHost;
        private System.Windows.Forms.TextBox txtSipUri;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.TextBox txtReply;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}