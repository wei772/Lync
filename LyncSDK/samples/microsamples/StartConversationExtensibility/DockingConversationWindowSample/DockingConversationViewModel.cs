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
using System.Drawing;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Extensibility;

namespace DockingConversationWindowSample
{
    /// <summary>
    /// This class encapsulates the behavior of Lync, and exposes data and
    /// events about a conversation to the View.
    /// </summary>
    class DockingConversationViewModel
    {
        #region Events

        public event EventHandler ConversationWindowNeedsAttentionEvent;
        public event EventHandler ConversationWindowNeedsSizeChangedEvent;
        public event EventHandler ConversationRemoveEvent;
        public event EventHandler ConversationAddedEvent;

        #endregion

        #region Fields

        private ConversationWindow _conversationWindow;
        private Conversation _conversation;
        private LyncClient _lync;

        #endregion

        #region Properties

        public Size MinSize
        {
            get;
            private set;
        }

        public Size MaxSize
        {
            get;
            private set;
        }

        public Size RecommendedSize
        {
            get;
            private set;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// The class constructor creates an instance of LyncClient and subscribes to the 
        /// ConversationManager's ConversationAdded and ConversationRemoved events.
        /// </summary>
        /// <exception cref="Exception">Lync is not signed in</exception>
        public DockingConversationViewModel()
        {
            try
            {
                _lync = LyncClient.GetClient();
            }
            catch (ClientNotFoundException clientNotFoundException)
            {
                Console.WriteLine(clientNotFoundException);
                return;
            }
            catch (NotStartedByUserException notStartedByUserException)
            {
                Console.Out.WriteLine(notStartedByUserException);
                return;
            }
            catch (LyncClientException lyncClientException)
            {
                Console.Out.WriteLine(lyncClientException);
                return;
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    Console.WriteLine("Error: " + systemException);
                    return;
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }

            if (_lync.State != ClientState.SignedIn)
            {
                Console.WriteLine("Not signed in");
            }

            //Subscribe to the Lync ConversationManager's ConversationAdded and ConversationRemoved events
            _lync.ConversationManager.ConversationAdded += HandleConversationAdded;
            _lync.ConversationManager.ConversationRemoved += HandleConversationRemoved;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method redocks the conversation window. When the docked conversation window adds a new visual
        /// element(such as video, a participant list, desktop sharing e.t.c) it's size changes. To accomodate
        /// this new element the conversation window will increase/decrease it's size accordingly. During this
        /// process we will have to redock the newly changed window into the parent window. panelHandle is the
        /// Handle property of the parent window where docking will occur.
        /// </summary>
        public void RedockConversation(IntPtr panelHandle)
        {
            if (_conversationWindow != null && _conversationWindow.IsDocked)
            {
                
                try
                {
                    _conversationWindow.Dock(panelHandle);
                }
                catch (LyncClientException lyncClientException)
                {
                    Console.WriteLine(lyncClientException);
                }
                catch (SystemException systemException)
                {
                    if (IsLyncException(systemException))
                    {
                        // Log the exception thrown by the Lync Model API.
                        Console.WriteLine("Error: " + systemException);
                    }
                    else
                    {
                        // Rethrow the SystemException which did not come from the Lync Model API.
                        throw;
                    }
                }

            }
        }

        public void Unregister()
        {
            if (_lync != null)
            {
                _lync.ConversationManager.ConversationAdded += HandleConversationAdded;
                _lync.ConversationManager.ConversationRemoved += HandleConversationRemoved;
            }
            if (_conversationWindow != null)
            {
                _conversationWindow.NeedsAttention += HandleNeedsAttention;
                _conversationWindow.NeedsSizeChange += HandleNeedsSizeChange;
            }
        }

        #endregion

        #region ConversationWindow events

        /// <summary>
        /// The ConversationWindow.NeedsSizeChanged event fires when the docked conversation window has added a new
        /// visual element (such as video, a participant list, desktop sharing, etc), and if it were not docked, the
        /// window would have automatically grown in size to accommodate this new element. We must respond to this 
        /// event within 5 seconds by increasing the size of the conversation windows first direct ancestor (the 
        /// ParentWindow handle to which it is docked). Max, min, and recommended size information for the 
        /// ConversationWindow is made available in this event, and cannot be  be accessed outside the event, so we 
        /// capture it here before propagating the event to our MainWindow where the containing panel can be adjusted.
        /// </summary>
        void HandleNeedsSizeChange(object sender, ConversationWindowNeedsSizeChangeEventArgs e)
        {
            MaxSize = new Size(e.MaximumWindowWidth, e.MaximumWindowHeight);
            MinSize = new Size(e.MinimumWindowWidth, e.MinimumWindowHeight);
            RecommendedSize = new Size(e.RecommendedWindowWidth, e.RecommendedWindowHeight);

            var conversationWindow = sender as ConversationWindow;
            if (ConversationWindowNeedsSizeChangedEvent != null && conversationWindow != null)
            {
                ConversationWindowNeedsSizeChangedEvent(this, null);
            }
        }

        /// <summary>
        /// The ConversationWindow.NeedsAttention event fires when a new message or other conversation
        /// element has been delivered (such as an incoming IM) but the window does not have focus.
        /// Lync handles this situation by causing the title bar of the window to flash until the user clicks on the
        /// window to acknowledge the new information.  We will propagate this event to the MainWindow
        /// where a similar behavior can be simulated.
        /// </summary>
        void HandleNeedsAttention(object sender, ConversationWindowNeedsAttentionEventArgs e)
        {
            if (ConversationWindowNeedsAttentionEvent != null)
            {
                ConversationWindowNeedsAttentionEvent(this, null);
            }
        }

        #endregion

        #region ConversationManager events

        /// <summary>
        /// This event is fired when the ConversationWindow is closing.  This sample application is handling
        /// the event for illustration purposes only.
        /// </summary>
        void HandleConversationRemoved(object sender, ConversationManagerEventArgs e)
        {
            if (ConversationRemoveEvent != null)
            {
                ConversationRemoveEvent(this, null);
            }
        }

        /// <summary>
        /// This event is fired when the Conversation is created.  We use the automation
        /// API to get the ConversationWindow for the Conversation, and subscribe to important window events.”
        /// </summary>
        void HandleConversationAdded(object sender, ConversationManagerEventArgs e)
        {
            if (ConversationAddedEvent != null)
            {
                _conversation = e.Conversation;
                try
                {
                    Automation automation = LyncClient.GetAutomation();
                    _conversationWindow = automation.GetConversationWindow(_conversation);
                }
                catch (LyncClientException lyncClientException)
                {
                    Console.WriteLine(lyncClientException);
                }
                catch (SystemException systemException)
                {
                    if (IsLyncException(systemException))
                    {
                        // Log the exception thrown by the Lync Model API.
                        Console.WriteLine("Error: " + systemException);
                    }
                    else
                    {
                        // Rethrow the SystemException which did not come from the Lync Model API.
                        throw;
                    }
                }

                if (_conversationWindow != null)
                {
                    //Subscribe to ConversationWindows's NeedsAttention and NeedsSizeChanged events
                    _conversationWindow.NeedsAttention += HandleNeedsAttention;
                    _conversationWindow.NeedsSizeChange += HandleNeedsSizeChange;
                    ConversationAddedEvent(this, null);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Identify if a particular SystemException is one of the exceptions which may be thrown
        /// by the Lync Model API.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private bool IsLyncException(SystemException ex)
        {
            return
                ex is NotImplementedException ||
                ex is ArgumentException ||
                ex is NullReferenceException ||
                ex is NotSupportedException ||
                ex is ArgumentOutOfRangeException ||
                ex is IndexOutOfRangeException ||
                ex is InvalidOperationException ||
                ex is TypeLoadException ||
                ex is TypeInitializationException ||
                ex is InvalidCastException;
        }

        #endregion
    }
}


