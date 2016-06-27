/*===================================================================== 
  This file is part of the Microsoft Office 2013 Lync Code Samples. 

  Copyright (C) 2013 Microsoft Corporation.  All rights reserved. 

This source code is intended only as a supplement to Microsoft 
Development Tools and/or on-line documentation.  See these other 
materials for detailed information regarding Microsoft code samples. 

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
PARTICULAR PURPOSE. 
=====================================================================*/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace ShareResources
{
    public partial class Chrome : Form
    {        
        private Rectangle innerRectangle;
        private Rectangle outerRectangle;
        private int BorderWidth = 5;        
        private Color fillColor = Color.Gold;
        private Color borderColor = Color.Navy;       
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cX, int cY, uint uFlags);  

        public Chrome()
        {
            InitializeComponent();
            BackColor = fillColor;
            ForeColor = borderColor;
            base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.Manual;            
            base.Text = "";
     
        }      

        /// <summary>
        /// Sets the Z order position and location of the chrome window
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="desktop"></param>
        /// <param name="borderWidth"></param>
        public void Highlight(Rectangle rectangle, bool desktop,int borderWidth)
        {
            BorderWidth = borderWidth;
            SetLocation(rectangle, desktop);

            if (desktop)
            {
                //Set window above all non-topmost windows.
                SetWindowPos(this.Handle, new IntPtr(-1), 0, 0, 0, 0, 0x43);
            }
            else
            {
                //Place window on top of the Z order.
                SetWindowPos(this.Handle, new IntPtr(0), 0, 0, 0, 0, 0x43);
            }
            Show();
        }

        /// <summary>
        /// Sets the dimensions and visible properties of the chrome window
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="desktop"></param>
        public void SetLocation(Rectangle rectangle, bool desktop)
        {
            
            int width = BorderWidth;
            if (desktop)
            {
                this.TopMost = true;

                //Make the chrome window surface transparent

                //Define rectangle for chrome window border
                outerRectangle = new Rectangle(new Point(0, 0), rectangle.Size);
                //Define rectangle for chrome window content (everything inside of the window border)
                innerRectangle = new Rectangle(new Point(BorderWidth, BorderWidth), rectangle.Size - new Size(BorderWidth*2, BorderWidth*2));
                
                Region region = new Region(outerRectangle);
                //Exclude the contents of chrome window from region
                region.Exclude(innerRectangle);

                base.Location = rectangle.Location; 
                base.Size = outerRectangle.Size;
                base.Region = region;
            }
            else
            {
                this.TopMost = false;

                //Make the chrome window surface transparent
                //Define rectangle for chrome window border
                outerRectangle = new Rectangle(new Point(0, 0), rectangle.Size + new Size(width * 2, width * 2));
                //Define rectangle for chrome window content (everything inside of the window border)
                innerRectangle = new Rectangle(new Point(BorderWidth, BorderWidth), rectangle.Size);
                Region region = new Region(outerRectangle);
                //Exclude the contents of chrome window from region
                region.Exclude(innerRectangle);
                base.Location = rectangle.Location - new Size(width, width);
                base.Size = outerRectangle.Size;
                base.Region = region;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            
            Rectangle rect = new Rectangle(outerRectangle.Left, outerRectangle.Top, outerRectangle.Width, outerRectangle.Height);
            Rectangle rectangle2 = new Rectangle(innerRectangle.Left , innerRectangle.Top , innerRectangle.Width, innerRectangle.Height );
            e.Graphics.DrawRectangle(new Pen(ForeColor), rectangle2);
            e.Graphics.DrawRectangle(new Pen(ForeColor), rect);
        }

        public void Show()
        {
            SetWindowPos(base.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x53);
        }

    }
}
