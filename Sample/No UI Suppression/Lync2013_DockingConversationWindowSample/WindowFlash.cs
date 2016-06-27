/*=====================================================================
  File:      WindowFlash.cs

  Summary:   This class flashes the specified window when attention is
             required.

---------------------------------------------------------------------
This file is part of the Microsoft Lync SDK Code Samples

  Copyright (C) 2010 Microsoft Corporation.  All rights reserved.

This source code is intended only as a supplement to Microsoft
Development Tools and/or on-line documentation.  See these other
materials for detailed information regarding Microsoft code samples.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
=====================================================================*/

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace DockingConversationWindowSample
{
    /// <summary>
    /// This class helps in flashing a window to inform the user that the window 
    /// requires attention when it doesn't have the focus or is not active.
    /// </summary>
    class WindowFlash
    {
        #region Fields

        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hwnd, bool invert);

        private readonly Timer _clock;
        private readonly Boolean _flashing;
        private readonly Window _target;
        private const int Interval = 500;

        #endregion

        #region Constructor
        /// <summary>
        /// The constructor instantiates the window that requires flashing, the flashing, and the clock.
        /// </summary>
        public WindowFlash(Window sender, bool flashing)
        {
            _target = sender;
            _clock = new Timer();
            _flashing = flashing;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method starts the clock. The value of Inverval can be altered to change the speed of flashing.
        /// (default 500 mseconds)
        /// </summary>
        public void StartFlashing()
        {
            _clock.Interval = Interval;
            _clock.Start();
            _clock.Tick += ClockTick;
        }

        /// <summary>
        /// This method stops the clock
        /// </summary>
        public void StopFlashing()
        {
            _clock.Stop();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method gets fired when the clock ticks
        /// </summary>
        void ClockTick(object sender, EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(_target).Handle;
            FlashWindow(hwnd, !_flashing);
        }

        #endregion
    }
}
