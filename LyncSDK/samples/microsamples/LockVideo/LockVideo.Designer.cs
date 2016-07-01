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
namespace LockVideo
{
    partial class LockVideo
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
            this.buttonGetConversations = new System.Windows.Forms.Button();
            this.listBoxConversations = new System.Windows.Forms.ListBox();
            this.buttonGetParticipants = new System.Windows.Forms.Button();
            this.listBoxParticipants = new System.Windows.Forms.ListBox();
            this.buttonGetParticipant = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.labelParticipantName = new System.Windows.Forms.Label();
            this.buttonLockVideo = new System.Windows.Forms.Button();
            this.buttonUnlockVideo = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.labelLockedState = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonGetConversations
            // 
            this.buttonGetConversations.Location = new System.Drawing.Point(13, 13);
            this.buttonGetConversations.Name = "buttonGetConversations";
            this.buttonGetConversations.Size = new System.Drawing.Size(105, 23);
            this.buttonGetConversations.TabIndex = 0;
            this.buttonGetConversations.Text = "Get Conversations";
            this.buttonGetConversations.UseVisualStyleBackColor = true;
            this.buttonGetConversations.Click += new System.EventHandler(this.buttonGetConversations_Click);
            // 
            // listBoxConversations
            // 
            this.listBoxConversations.FormattingEnabled = true;
            this.listBoxConversations.Location = new System.Drawing.Point(141, 13);
            this.listBoxConversations.Name = "listBoxConversations";
            this.listBoxConversations.Size = new System.Drawing.Size(120, 30);
            this.listBoxConversations.TabIndex = 1;
            // 
            // buttonGetParticipants
            // 
            this.buttonGetParticipants.Location = new System.Drawing.Point(13, 55);
            this.buttonGetParticipants.Name = "buttonGetParticipants";
            this.buttonGetParticipants.Size = new System.Drawing.Size(105, 23);
            this.buttonGetParticipants.TabIndex = 2;
            this.buttonGetParticipants.Text = "Get Participants";
            this.buttonGetParticipants.UseVisualStyleBackColor = true;
            this.buttonGetParticipants.Click += new System.EventHandler(this.buttonGetParticipants_Click);
            // 
            // listBoxParticipants
            // 
            this.listBoxParticipants.FormattingEnabled = true;
            this.listBoxParticipants.Location = new System.Drawing.Point(141, 55);
            this.listBoxParticipants.Name = "listBoxParticipants";
            this.listBoxParticipants.Size = new System.Drawing.Size(120, 56);
            this.listBoxParticipants.TabIndex = 3;
            // 
            // buttonGetParticipant
            // 
            this.buttonGetParticipant.Location = new System.Drawing.Point(13, 85);
            this.buttonGetParticipant.Name = "buttonGetParticipant";
            this.buttonGetParticipant.Size = new System.Drawing.Size(105, 23);
            this.buttonGetParticipant.TabIndex = 4;
            this.buttonGetParticipant.Text = "Get Participant";
            this.buttonGetParticipant.UseVisualStyleBackColor = true;
            this.buttonGetParticipant.Click += new System.EventHandler(this.buttonGetParticipant_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 127);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Participant Name";
            // 
            // labelParticipantName
            // 
            this.labelParticipantName.AutoSize = true;
            this.labelParticipantName.Location = new System.Drawing.Point(138, 127);
            this.labelParticipantName.Name = "labelParticipantName";
            this.labelParticipantName.Size = new System.Drawing.Size(35, 13);
            this.labelParticipantName.TabIndex = 6;
            this.labelParticipantName.Text = "Name";
            // 
            // buttonLockVideo
            // 
            this.buttonLockVideo.Location = new System.Drawing.Point(13, 160);
            this.buttonLockVideo.Name = "buttonLockVideo";
            this.buttonLockVideo.Size = new System.Drawing.Size(105, 23);
            this.buttonLockVideo.TabIndex = 7;
            this.buttonLockVideo.Text = "LockVideo";
            this.buttonLockVideo.UseVisualStyleBackColor = true;
            this.buttonLockVideo.Click += new System.EventHandler(this.buttonLockVideo_Click);
            // 
            // buttonUnlockVideo
            // 
            this.buttonUnlockVideo.Location = new System.Drawing.Point(13, 190);
            this.buttonUnlockVideo.Name = "buttonUnlockVideo";
            this.buttonUnlockVideo.Size = new System.Drawing.Size(105, 23);
            this.buttonUnlockVideo.TabIndex = 8;
            this.buttonUnlockVideo.Text = "Unlock Video";
            this.buttonUnlockVideo.UseVisualStyleBackColor = true;
            this.buttonUnlockVideo.Click += new System.EventHandler(this.buttonUnLockVideo_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 220);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Locked State";
            // 
            // labelLockedState
            // 
            this.labelLockedState.AutoSize = true;
            this.labelLockedState.Location = new System.Drawing.Point(138, 220);
            this.labelLockedState.Name = "labelLockedState";
            this.labelLockedState.Size = new System.Drawing.Size(32, 13);
            this.labelLockedState.TabIndex = 10;
            this.labelLockedState.Text = "State";
            // 
            // LockVideo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.labelLockedState);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonUnlockVideo);
            this.Controls.Add(this.buttonLockVideo);
            this.Controls.Add(this.labelParticipantName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonGetParticipant);
            this.Controls.Add(this.listBoxParticipants);
            this.Controls.Add(this.buttonGetParticipants);
            this.Controls.Add(this.listBoxConversations);
            this.Controls.Add(this.buttonGetConversations);
            this.Name = "LockVideo";
            this.Text = "Lock Video";
            this.Load += new System.EventHandler(this.LockVideo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonGetConversations;
        private System.Windows.Forms.ListBox listBoxConversations;
        private System.Windows.Forms.Button buttonGetParticipants;
        private System.Windows.Forms.ListBox listBoxParticipants;
        private System.Windows.Forms.Button buttonGetParticipant;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelParticipantName;
        private System.Windows.Forms.Button buttonLockVideo;
        private System.Windows.Forms.Button buttonUnlockVideo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelLockedState;
    }
}

