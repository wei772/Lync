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
namespace PinVideo
{
    partial class PinVideo
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
            this.buttonPinVideo = new System.Windows.Forms.Button();
            this.buttonUnpinVideo = new System.Windows.Forms.Button();
            this.labelPinnedState = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonGetParticipant = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.labelParticipantName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonGetConversations
            // 
            this.buttonGetConversations.Location = new System.Drawing.Point(13, 13);
            this.buttonGetConversations.Name = "buttonGetConversations";
            this.buttonGetConversations.Size = new System.Drawing.Size(106, 23);
            this.buttonGetConversations.TabIndex = 1;
            this.buttonGetConversations.Text = "Get Conversations";
            this.buttonGetConversations.UseVisualStyleBackColor = true;
            this.buttonGetConversations.Click += new System.EventHandler(this.buttonGetConversations_Click);
            // 
            // listBoxConversations
            // 
            this.listBoxConversations.FormattingEnabled = true;
            this.listBoxConversations.Location = new System.Drawing.Point(137, 13);
            this.listBoxConversations.Name = "listBoxConversations";
            this.listBoxConversations.Size = new System.Drawing.Size(120, 30);
            this.listBoxConversations.TabIndex = 2;
            // 
            // buttonGetParticipants
            // 
            this.buttonGetParticipants.Location = new System.Drawing.Point(13, 52);
            this.buttonGetParticipants.Name = "buttonGetParticipants";
            this.buttonGetParticipants.Size = new System.Drawing.Size(106, 23);
            this.buttonGetParticipants.TabIndex = 3;
            this.buttonGetParticipants.Text = "Get Participants";
            this.buttonGetParticipants.UseVisualStyleBackColor = true;
            this.buttonGetParticipants.Click += new System.EventHandler(this.buttonGetParticipants_Click);
            // 
            // listBoxParticipants
            // 
            this.listBoxParticipants.FormattingEnabled = true;
            this.listBoxParticipants.Location = new System.Drawing.Point(137, 52);
            this.listBoxParticipants.Name = "listBoxParticipants";
            this.listBoxParticipants.Size = new System.Drawing.Size(120, 56);
            this.listBoxParticipants.TabIndex = 4;
            // 
            // buttonPinVideo
            // 
            this.buttonPinVideo.Location = new System.Drawing.Point(13, 146);
            this.buttonPinVideo.Name = "buttonPinVideo";
            this.buttonPinVideo.Size = new System.Drawing.Size(106, 23);
            this.buttonPinVideo.TabIndex = 5;
            this.buttonPinVideo.Text = "Pin Video";
            this.buttonPinVideo.UseVisualStyleBackColor = true;
            this.buttonPinVideo.Click += new System.EventHandler(this.buttonPinVideo_Click);
            // 
            // buttonUnpinVideo
            // 
            this.buttonUnpinVideo.Location = new System.Drawing.Point(13, 176);
            this.buttonUnpinVideo.Name = "buttonUnpinVideo";
            this.buttonUnpinVideo.Size = new System.Drawing.Size(106, 23);
            this.buttonUnpinVideo.TabIndex = 6;
            this.buttonUnpinVideo.Text = "Unpin Video";
            this.buttonUnpinVideo.UseVisualStyleBackColor = true;
            this.buttonUnpinVideo.Click += new System.EventHandler(this.buttonUnpinVideo_Click);
            // 
            // labelPinnedState
            // 
            this.labelPinnedState.AutoSize = true;
            this.labelPinnedState.Location = new System.Drawing.Point(134, 217);
            this.labelPinnedState.Name = "labelPinnedState";
            this.labelPinnedState.Size = new System.Drawing.Size(32, 13);
            this.labelPinnedState.TabIndex = 7;
            this.labelPinnedState.Text = "State";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 217);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Pinned State";
            // 
            // buttonGetParticipant
            // 
            this.buttonGetParticipant.Location = new System.Drawing.Point(13, 82);
            this.buttonGetParticipant.Name = "buttonGetParticipant";
            this.buttonGetParticipant.Size = new System.Drawing.Size(106, 23);
            this.buttonGetParticipant.TabIndex = 9;
            this.buttonGetParticipant.Text = "Get Participant";
            this.buttonGetParticipant.UseVisualStyleBackColor = true;
            this.buttonGetParticipant.Click += new System.EventHandler(this.buttonGetParticipant_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 119);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Participant";
            // 
            // labelParticipantName
            // 
            this.labelParticipantName.AutoSize = true;
            this.labelParticipantName.Location = new System.Drawing.Point(134, 119);
            this.labelParticipantName.Name = "labelParticipantName";
            this.labelParticipantName.Size = new System.Drawing.Size(35, 13);
            this.labelParticipantName.TabIndex = 11;
            this.labelParticipantName.Text = "Name";
            // 
            // PinVideo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 242);
            this.Controls.Add(this.labelParticipantName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonGetParticipant);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelPinnedState);
            this.Controls.Add(this.buttonUnpinVideo);
            this.Controls.Add(this.buttonPinVideo);
            this.Controls.Add(this.listBoxParticipants);
            this.Controls.Add(this.buttonGetParticipants);
            this.Controls.Add(this.listBoxConversations);
            this.Controls.Add(this.buttonGetConversations);
            this.Name = "PinVideo";
            this.Text = "PinVideo";
            this.Load += new System.EventHandler(this.PinVideo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonGetConversations;
        private System.Windows.Forms.ListBox listBoxConversations;
        private System.Windows.Forms.Button buttonGetParticipants;
        private System.Windows.Forms.ListBox listBoxParticipants;
        private System.Windows.Forms.Button buttonPinVideo;
        private System.Windows.Forms.Button buttonUnpinVideo;
        private System.Windows.Forms.Label labelPinnedState;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonGetParticipant;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelParticipantName;
    }
}

