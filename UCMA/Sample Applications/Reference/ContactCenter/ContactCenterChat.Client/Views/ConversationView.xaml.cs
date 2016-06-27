/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
********************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows.Browser;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient;
using System.Diagnostics;
using System.Windows.Data;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.Views
{
	public partial class ConversationView : Page
	{
        private bool m_needToTerminateConversation = true;

		public ConversationView()
		{
			InitializeComponent();

            this.RegisterOnBeforeUnload();

            SetBinding(DataContextDummyProperty, new Binding());
		}


        public void RegisterOnBeforeUnload()
        {
            //Register Silverlight object for availability in Javascript.
            const string ScriptableObjectName = "Bridge";
            HtmlPage.RegisterScriptableObject(ScriptableObjectName, this);
        }

        [ScriptableMember]
        public void OnBeforeUnload() 
        {
            if (m_needToTerminateConversation)
            {
                var conversationVM = ((ViewModels.ConversationViewModel)DataContext);
                if (conversationVM != null && conversationVM.CanExecuteTerminateConversationCommand(null))
                {
                    conversationVM.TerminateConversationCommand.Execute(null);
                    m_needToTerminateConversation = true;
                }
            }
        }

        void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                this.MessagesScrollViewer.ScrollToBottom();
            });
        }

        private void LayoutRoot_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void UserInputTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

        	if( e.Key == Key.Enter )
			{
                var conversationVM = ((ViewModels.ConversationViewModel)DataContext);
                if (conversationVM.SendMessageCommand.CanExecute(null))
                {
                    conversationVM.SendMessageCommand.Execute(null);
                    UserInputTextBox.Text = String.Empty;
                }
			}
        }

        public Object DataContextDummy
        {
            get { return (Object)GetValue(DataContextDummyProperty); }
            set { SetValue(DataContextDummyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataContextDummy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataContextDummyProperty =
            DependencyProperty.Register("DataContextDummy", typeof(Object), typeof(ConversationView), new PropertyMetadata(null, DataContextChanged));

        private static void DataContextChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
                ((ViewModels.ConversationViewModel)e.OldValue).Messages.CollectionChanged -= ((ConversationView)target).Messages_CollectionChanged;
            if (e.NewValue != null)
                ((ViewModels.ConversationViewModel)e.NewValue).Messages.CollectionChanged += ((ConversationView)target).Messages_CollectionChanged;
        }

	}
}