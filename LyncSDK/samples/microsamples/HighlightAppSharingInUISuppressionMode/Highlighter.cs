using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HighlightAppSharingInUISuppressionMode
{
    public class HighlighterProperties
    {
        public HighlighterProperties()
        {
            _borderWidth = 5;
            _borderBuffer = 1;
            _color = Color.Yellow;
        }

        public int BorderWidth
        {
            get
            {
                return this._borderWidth;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Border width must be greater than zero", "BorderWidth");
                }
                this._borderWidth = value;
            }
        }

        public int BorderBuffer
        {
            get
            {
                return this._borderBuffer;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Parameter must be greater than zero", "BorderBuffer");
                }
                this._borderBuffer = value;
            }
        }

        public Color Color
        {
            get
            {
                return this._color;
            }

            set
            {
                this._color = value;
            }
        }

        public static HighlighterProperties Default
        {
            get
            {
                return HighlighterProperties._defaultProperties;
            }
            set
            {
                if (value != null)
                {
                    HighlighterProperties._defaultProperties = value;
                }
                else
                {
                    HighlighterProperties._defaultProperties = new HighlighterProperties();
                }
            }
        }

        private int _borderWidth;

        private int _borderBuffer;

        private Color _color;

        private static HighlighterProperties _defaultProperties = new HighlighterProperties();
    }

    public enum HighLighterMode
    {
        Screen,
        Process,
        Window
    }

    public class Highlighter : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

        }

        private static class NativeMethods
        {
            public const int SWP_NOSIZE = 0x0001;
            public const int SWP_NOMOVE = 0x0002;
            public const int SWP_NOACTIVATE = 0x0010;
            public const int SWP_SHOWWINDOW = 0x0040;

            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cX, int cY, uint uFlags);
        }

        /// <summary>
        /// Initializes the Highlighter class.
        /// </summary>
        public Highlighter(HighLighterMode mode, int param)
            : this(mode, param, HighlighterProperties.Default)
        {
        }

        /// <summary>
        /// Initializes the Highlighter class, with the given properties.
        /// </summary>
        /// <param name="properties">The properties to use for highlighting.</param>
        public Highlighter(HighLighterMode mode, int param, HighlighterProperties properties)
        {
            this._mode = mode;
            this._param = param;
            this._disposed = false;
            this._properties = properties;
            this.BackColor = this._properties.Color;
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Highlighter Window";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Text = this.Name;
            this.TopMost = true;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (false == this._disposed)
            {
                base.Dispose(disposing);
                this._disposed = true;
            }
        }

        /// <summary>
        /// Specifies the location that should be highlighted.
        /// </summary>
        /// <param name="rectangle">A Rectangle defining the location to be highlighted.</param>
        protected void SetLocation(Rectangle rectangle)
        {
            int totalBorder = _properties.BorderBuffer + _properties.BorderWidth;

            this._outerRectangle = new Rectangle(
                    new System.Drawing.Point(0, 0),
                    rectangle.Size + new System.Drawing.Size(totalBorder * 2, totalBorder * 2));

            this._innerRectangle = new Rectangle(
                    new System.Drawing.Point(this._properties.BorderWidth, this._properties.BorderWidth),
                    rectangle.Size + new System.Drawing.Size(this._properties.BorderBuffer * 2, this._properties.BorderBuffer * 2));

            // Set the region of the form
            //
            Region frmRegion = new Region(_outerRectangle);
            frmRegion.Exclude(_innerRectangle);

            this.Location = rectangle.Location - new System.Drawing.Size(totalBorder, totalBorder);
            this.Size = this._outerRectangle.Size;
            this.Region = frmRegion;
        }

        /// <summary>
        /// Displays the control to the user. 
        /// </summary>
        public new void Show()
        {
            NativeMethods.SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
        }

        public void Highlight()
        {
            Highlight(_mode, _param);
        }

        protected void Highlight(HighLighterMode mode, int param)
        {
            Rectangle rectangle = new Rectangle();
            if (mode == HighLighterMode.Screen)
            {
                rectangle.X = Screen.AllScreens[param].Bounds.Left + (Properties.BorderBuffer + Properties.BorderWidth);
                rectangle.Y = Screen.AllScreens[param].Bounds.Top + (Properties.BorderBuffer + Properties.BorderWidth);
                rectangle.Height = Screen.AllScreens[param].Bounds.Height - (Properties.BorderBuffer + Properties.BorderWidth) * 2;
                rectangle.Width = Screen.AllScreens[param].Bounds.Width - (Properties.BorderBuffer + Properties.BorderWidth) * 2;
            }
            else if (mode == HighLighterMode.Process)
            {
                Process proc = Process.GetProcessById(param);
                IntPtr windowHandle = proc.MainWindowHandle;
                rectangle = GetWindowRectangle(windowHandle);
            }
            else if (mode == HighLighterMode.Window)
            {
                IntPtr windowHandle = new IntPtr(param);
                rectangle = GetWindowRectangle(windowHandle);
            }

            SetLocation(rectangle);
        }

        private Rectangle GetWindowRectangle(IntPtr wndHandle)
        {
            Rectangle rectangle = new Rectangle();
            RECT rect = new RECT();
            GetWindowRect(wndHandle, ref rect);

            rectangle.X = rect.left;
            rectangle.Y = rect.top;
            rectangle.Height = rect.bottom - rect.top;
            rectangle.Width = rect.right - rect.left;

            return rectangle;
        }

        /// <summary>
        /// Override the 'OnPaint' method to provide our imlpementation.
        /// </summary>
        /// <param name="e">The PaintEventArgs associated with this event.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // need to tweak the rectangles to paint the border correctly
            Rectangle tmpOuterRectangle = new Rectangle(this._outerRectangle.Left, this._outerRectangle.Top, this._outerRectangle.Width - 1, this._outerRectangle.Height - 1);
            Rectangle tmpInnerRectangle = new Rectangle(this._innerRectangle.Left - 1, this._innerRectangle.Top - 1, this._innerRectangle.Width + 1, this._innerRectangle.Height + 1);

            // draw the border
            e.Graphics.DrawRectangle(new Pen(this.ForeColor), tmpInnerRectangle);
            e.Graphics.DrawRectangle(new Pen(this.ForeColor), tmpOuterRectangle);
        }

        /// <summary>
        /// The HighlighterProperties associated with this Highlighter instance.
        /// </summary>
        /// <value>The HighlighterProperties associated with this Highlighter instance.</value>
        protected HighlighterProperties Properties
        {
            get { return this._properties; }
        }

        private HighLighterMode _mode;

        private int _param;

        /// <summary>
        /// The HighlighterProperties associated with this Highlighter instance.
        /// </summary>
        private HighlighterProperties _properties;

        /// <summary>
        /// true if we've been disposed, false otherwise.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The 'innerRectangle'.
        /// </summary>
        private Rectangle _innerRectangle;

        /// <summary>
        /// The 'outerRectangle'.
        /// </summary>
        private Rectangle _outerRectangle;
    }
}
