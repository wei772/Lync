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
namespace RoomQuery
{
    partial class RoomQuery
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
            this.QueriedRooms_ListBox = new System.Windows.Forms.ListBox();
            this.RoomQueryString_TextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.StartQuery_Button = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.LyncClientState_Label);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(21, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(276, 23);
            this.panel1.TabIndex = 1;
            // 
            // lblLyncClient
            // 
            this.LyncClientState_Label.AutoSize = true;
            this.LyncClientState_Label.Location = new System.Drawing.Point(111, 4);
            this.LyncClientState_Label.Name = "lblLyncClient";
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
            // QueriedRooms_ListBox
            // 
            this.QueriedRooms_ListBox.FormattingEnabled = true;
            this.QueriedRooms_ListBox.Location = new System.Drawing.Point(21, 80);
            this.QueriedRooms_ListBox.Name = "QueriedRooms_ListBox";
            this.QueriedRooms_ListBox.Size = new System.Drawing.Size(276, 186);
            this.QueriedRooms_ListBox.TabIndex = 53;
            // 
            // RoomQueryString_TextBox
            // 
            this.RoomQueryString_TextBox.Location = new System.Drawing.Point(103, 48);
            this.RoomQueryString_TextBox.Name = "RoomQueryString_TextBox";
            this.RoomQueryString_TextBox.Size = new System.Drawing.Size(100, 20);
            this.RoomQueryString_TextBox.TabIndex = 54;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 55;
            this.label2.Text = "Room to query";
            // 
            // StartQuery_Button
            // 
            this.StartQuery_Button.Location = new System.Drawing.Point(222, 48);
            this.StartQuery_Button.Name = "StartQuery_Button";
            this.StartQuery_Button.Size = new System.Drawing.Size(75, 23);
            this.StartQuery_Button.TabIndex = 56;
            this.StartQuery_Button.Text = "Go";
            this.StartQuery_Button.UseVisualStyleBackColor = true;
            this.StartQuery_Button.Click += new System.EventHandler(this.StartQuery_Button_Click);
            // 
            // RoomQuery
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 292);
            this.Controls.Add(this.StartQuery_Button);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.RoomQueryString_TextBox);
            this.Controls.Add(this.QueriedRooms_ListBox);
            this.Controls.Add(this.panel1);
            this.Name = "RoomQuery";
            this.Text = "Simple Group Chat";
            this.Load += new System.EventHandler(this.RoomQuery_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label LyncClientState_Label;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox QueriedRooms_ListBox;
        private System.Windows.Forms.TextBox RoomQueryString_TextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button StartQuery_Button;
    }
}

