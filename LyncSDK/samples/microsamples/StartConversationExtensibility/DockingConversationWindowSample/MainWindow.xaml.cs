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

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Lync.Controls;
using Panel = System.Windows.Forms.Panel;
using Size = System.Drawing.Size;

namespace DockingConversationWindowSample
{
    /// <summary>
    /// This class provides the interaction logic for a WPF window into which we
    /// will dock a Lync ConversationWindow.
    /// </summary>
    public partial class MainWindow
    {

        #region Fields

        private readonly DockingConversationViewModel _dockingConversationVm;
        private Panel _conversationWindowParentPanel;
        private readonly WindowFlash _windowFlash;

        #endregion

        #region Constructor

        /// <summary>
        /// The class constructor instantiates _dockingConversationVm and subscribes to it's events.
        /// It also creates and instance of WindowFlash and subscribes to the main window Activated event.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += HandleLoaded;
            Unloaded += HandleUnloaded;
            _dockingConversationVm = new DockingConversationViewModel();
            _dockingConversationVm.ConversationAddedEvent += HandleModelConversationAddedEvent;
            _dockingConversationVm.ConversationRemoveEvent += HandleModelConversationRemoveEvent;
            _dockingConversationVm.ConversationWindowNeedsAttentionEvent += HandleModelConversationWindowNeedsAttentionEvent;
            _dockingConversationVm.ConversationWindowNeedsSizeChangedEvent += HandleModelConversationWindowNeedsSizeChangedEvent;

            _windowFlash = new WindowFlash(this, false);
            Activated += MainWindowActivated;
        }

        #endregion

        #region Window Event Handlers

        /// <summary>
        /// When the MainWindow changes in size, we must resize the _conversationWindowParentPanel,
        /// and redock the ConversationWindow to it.
        /// </summary>
        void HandleWindowSizeChanged(object sender, EventArgs eventArgs)
        {
            _conversationWindowParentPanel.Invoke((Action)ResizeConversation);
        }

        /// <summary>
        /// This method handles the Main Window Loaded Event. It gets fired when the window is loaded.
        /// </summary>
        void HandleLoaded(object sender, RoutedEventArgs e)
        {
            // Create the host for the conversation window.
            _conversationWindowParentPanel = new Panel();

            // We are using 2 layers of WindowsForms panels to achieve a scrolling effect.  The inner layer (_conversationWindowParentPanel)
            // is the direct ancestor of the conversation window, and the size of this panel determines the size of the ConversationWindow 
            // when we invoke Dock().  Therefore, we want to retain control over the sizing of this panel to insure that it can always be 
            // at least as big as the MinimumSize required by the ConversationWindow, as dictated in the NeedsSizeChange event.
            // The outer layer (_scrollViewer) is used to accommodate the _conversationWindowParentPanel when it is too large for the 
            // application. This panel is set to scroll automatically, or turn the scrollbars off when they are not needed. Because of the
            // automatic sizing and scrolling behavior, we cannot host the ConversationWindow directly in this panel, since it does not 
            // guarantee our minimum size requirements.
            _scrollViewer.Controls.Add(_conversationWindowParentPanel);

            // Get the handle of the panel where we will dock the conversation:
            IntPtr handle = _conversationWindowParentPanel.Handle;

            // Tell the StartInstantMessagingButton to dock the conversation into the panel we created above:
            _myStartIMButton.ContextualInformation = new ConversationContextualInfo{ParentWindowHandle = handle};

            // Subscribe to the MainWindow.SizeChanged event so we can redock the conversation when layout changes happen.
            SizeChanged += HandleWindowSizeChanged;
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            _dockingConversationVm.Unregister();
        }

        /// <summary>
        /// Event fired when Main Window is activated. 
        /// This method is used to stop flashing the window.
        /// </summary>
        void MainWindowActivated(object sender, EventArgs e)
        {
            _windowFlash.StopFlashing();
        }

        #endregion

        #region Model Event Handlers

        /// <summary>
        /// This method handles the ConversationWindow's NeedSizeChanged event. 
        /// </summary>
        void HandleModelConversationWindowNeedsSizeChangedEvent(object sender, EventArgs eventArgs)
        {
            // The conversation window events do not fire on the UI thread, so we use the Invoke method to handle them.
            _conversationWindowParentPanel.Invoke((Action)ResizeConversation);
        }

        /// <summary>
        /// this method handles the ConversationWindow's NeedsAttention event
        /// </summary>
        void HandleModelConversationWindowNeedsAttentionEvent(object sender, EventArgs eventArgs)
        {
            _conversationWindowParentPanel.Invoke((Action)FlashWhenInactive);
        }

        /// <summary>
        /// Handler for the Conversation Added event
        /// </summary>
        static void HandleModelConversationAddedEvent(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("HandleModelNewConversationEvent");
        }

        /// <summary>
        /// Handler for the Conversation Removed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        static void HandleModelConversationRemoveEvent(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("HandleModelConversationRemoveEvent");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Helper method to Resize the conversation window.
        /// When the MainWindow size is changed, or the ConversationWindow changes size, we must adjust our layout.
        /// This method determines whether or not scrollbars will be needed (they are added automatically by _scrollViewer panel if needed).
        /// Then, based on this information, it manually sets the size of the _conversationWindowParentPanel to a new dimension
        /// which will be at least as big as the MinimumSize required by the ConversationWindow, but bigger if more space is available.
        /// The size is then adjusted down when one scrollbar has been added to insure that the window fits perfectly in the remaining space
        /// </summary>
        private void ResizeConversation()
        {
            int minWidth = _dockingConversationVm.MinSize.Width;
            int minHeight = _dockingConversationVm.MinSize.Height;
            int availWidthWithScrollbar = _scrollViewer.Size.Width - SystemInformation.VerticalScrollBarWidth;
            int availableHeightWithScrollbar = _scrollViewer.Size.Height - SystemInformation.HorizontalScrollBarHeight;

            bool hasHorizontalScrollbars = _scrollViewer.Size.Width < minWidth;
            bool hasVerticalScrollbars = _scrollViewer.Size.Height < minHeight;

            if (hasHorizontalScrollbars ^ hasVerticalScrollbars)
            {
                // If only one scrollbar is visible, check to see whether or not the addition
                // of this scrollbar forces the other scrollbar to become visible...
                if (hasVerticalScrollbars)
                {
                    hasHorizontalScrollbars = availWidthWithScrollbar < minWidth;
                }
                else
                {
                    hasVerticalScrollbars = availableHeightWithScrollbar < minHeight;
                }
            }

            int width = hasHorizontalScrollbars
                ? minWidth : (hasVerticalScrollbars ? availWidthWithScrollbar : _scrollViewer.Size.Width);
            int height = hasVerticalScrollbars
                ? minHeight : (hasHorizontalScrollbars ? availableHeightWithScrollbar : _scrollViewer.Size.Height);

            _conversationWindowParentPanel.Size = new Size(width, height);
            _conversationWindowParentPanel.Invoke((Action<IntPtr>)_dockingConversationVm.RedockConversation, _conversationWindowParentPanel.Handle);
        }

        /// <summary>
        /// Helper method to set the window flashing
        /// </summary>
        private void FlashWhenInactive()
        {
            if (IsFocused || IsActive)
            {
                _windowFlash.StopFlashing();
            }
            else
            {
                _windowFlash.StartFlashing();
            }
        }

        #endregion
    }
}
