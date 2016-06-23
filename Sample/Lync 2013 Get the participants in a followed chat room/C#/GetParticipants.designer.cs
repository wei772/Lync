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
namespace GetParticipants
{
    partial class GetParticipants
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
            this.label13 = new System.Windows.Forms.Label();
            this.Participants_ListBox = new System.Windows.Forms.ListBox();
            this.Participants_Label = new System.Windows.Forms.Label();
            this.FollowedRoom_Numeric = new System.Windows.Forms.NumericUpDown();
            this.FollowedRoomTitle_Label = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FollowedRoom_Numeric)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.LyncClientState_Label);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(9, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(233, 23);
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
            this.label1.Location = new System.Drawing.Point(18, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Lync Client State";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 60);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(80, 13);
            this.label13.TabIndex = 39;
            this.label13.Text = "Followed Room";
            // 
            // Participants_ListBox
            // 
            this.Participants_ListBox.FormattingEnabled = true;
            this.Participants_ListBox.HorizontalScrollbar = true;
            this.Participants_ListBox.Location = new System.Drawing.Point(9, 153);
            this.Participants_ListBox.Name = "Participants_ListBox";
            this.Participants_ListBox.Size = new System.Drawing.Size(233, 160);
            this.Participants_ListBox.TabIndex = 40;
            // 
            // Participants_Label
            // 
            this.Participants_Label.AutoSize = true;
            this.Participants_Label.Location = new System.Drawing.Point(9, 128);
            this.Participants_Label.Name = "Participants_Label";
            this.Participants_Label.Size = new System.Drawing.Size(62, 13);
            this.Participants_Label.TabIndex = 41;
            this.Participants_Label.Text = "Participants";
            // 
            // FollowedRoom_Numeric
            // 
            this.FollowedRoom_Numeric.Location = new System.Drawing.Point(93, 60);
            this.FollowedRoom_Numeric.Name = "FollowedRoom_Numeric";
            this.FollowedRoom_Numeric.Size = new System.Drawing.Size(62, 20);
            this.FollowedRoom_Numeric.TabIndex = 42;
            this.FollowedRoom_Numeric.ValueChanged += new System.EventHandler(this.FollowedRoom_Numeric_ValueChanged);
            // 
            // FollowedRoomTitle_Label
            // 
            this.FollowedRoomTitle_Label.AutoSize = true;
            this.FollowedRoomTitle_Label.Location = new System.Drawing.Point(9, 104);
            this.FollowedRoomTitle_Label.Name = "FollowedRoomTitle_Label";
            this.FollowedRoomTitle_Label.Size = new System.Drawing.Size(80, 13);
            this.FollowedRoomTitle_Label.TabIndex = 43;
            this.FollowedRoomTitle_Label.Text = "Followed Room";
            // 
            // GetParticipants
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(253, 326);
            this.Controls.Add(this.FollowedRoomTitle_Label);
            this.Controls.Add(this.FollowedRoom_Numeric);
            this.Controls.Add(this.Participants_Label);
            this.Controls.Add(this.Participants_ListBox);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.panel1);
            this.Name = "GetParticipants";
            this.Text = "Get Room Participants";
            this.Load += new System.EventHandler(this.GetParticipants_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FollowedRoom_Numeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label LyncClientState_Label;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ListBox Participants_ListBox;
        private System.Windows.Forms.Label Participants_Label;
        private System.Windows.Forms.NumericUpDown FollowedRoom_Numeric;
        private System.Windows.Forms.Label FollowedRoomTitle_Label;
    }
}

