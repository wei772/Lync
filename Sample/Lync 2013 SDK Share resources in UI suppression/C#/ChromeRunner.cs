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
using Microsoft.Lync.Model.Conversation.Sharing;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace ShareResources
{
    partial class ChromeRunner
    {

        #region chrome form state
        public int borderWith = 5;
        public Chrome currentChrome;
        
        
        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, uint Msg);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        private const UInt32 SW_MAXIMIZE = 3;
        private const UInt32 SW_MINIMIZE = 2;
        private const UInt32 SW_RESTORE = 0x09;

        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int X;
            public int Y;
            public int Height;
            public int Width;

        }

        #endregion


        /// <summary>
        /// shows the shared resource "chrome" around the desktop
        /// </summary>
        /// <param name="screen"></param>
        internal void ShowDesktopChrome(Screen screen)
        {
            int x = screen.Bounds.X;
            int y = screen.Bounds.Y;
            Size size = screen.Bounds.Size;
            Rectangle desktop = new Rectangle(new Point(x, y), size);
            currentChrome = new Chrome();
            currentChrome.Highlight(desktop, true, borderWith);

        }
        /// <summary>
        /// Shows the shared resouce "chrome" around the shared process
        /// </summary>
        /// <param name="selectedResourceObject"></param>
        internal void ShowProcessChrome(SharingResource selectedResourceObject)
        {
            Process[] processes = Process.GetProcesses();
            Process currentProcess = null;
            foreach (Process process in processes)
            {
                //In the case of Outlook, the main outlook window MainWindowHandle is the Id of the process.
                if (selectedResourceObject.Id == (Int32)process.MainWindowHandle 
                    || selectedResourceObject.Id == process.Id)
                {
                    currentProcess = process;
                    break;

                }
            }
            if (currentProcess == null)
            {
                return;
            }

            //activate the selected application and bring to foreground
            IntPtr currentProcessHandle = currentProcess.MainWindowHandle;
            SetForegroundWindow(currentProcessHandle);

            //Get the visual state of the shared application
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(currentProcessHandle, out placement);

            if (placement.showCmd == SW_MINIMIZE)
            {
                ShowWindow(currentProcessHandle, SW_RESTORE);
            }


            //Get the location and rectangle of the selected application
            RECT processWindowSize = new RECT();
            GetWindowRect(currentProcessHandle, ref processWindowSize);
            Size size = new Size();
            size = new Size(processWindowSize.Height - processWindowSize.X, processWindowSize.Width - processWindowSize.Y);
            Rectangle processWindow = new Rectangle(new Point(processWindowSize.X, processWindowSize.Y), size);
            Rectangle currentProcessWindow = processWindow;


            //set the size of the Chrome window based on the visual state of the
            //shared application
            currentChrome = new Chrome();
            if (placement.showCmd == SW_MAXIMIZE)
            {
                int taskBarSize = Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height;
                processWindow.X = 0;
                processWindow.Y = 0;
                processWindow.Height = (processWindowSize.Width - borderWith * 2) + 2;
                processWindow.Width = processWindowSize.Height + processWindowSize.X;
                currentChrome.Highlight(processWindow, true, borderWith);
            }
            else
            {
                currentChrome.Highlight(processWindow, false, borderWith);
            }
        }

        /// <summary>
        /// Closes the "chrome" winform that borders a shared resource
        /// </summary>
        internal void CloseTheChrome()
        {
            if (currentChrome != null)
            {
                currentChrome.Close();
                currentChrome.Dispose();
            }
        }
    }
}