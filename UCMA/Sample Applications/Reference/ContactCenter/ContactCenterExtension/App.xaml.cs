/*=====================================================================
  File:      App.xaml.cs

  Summary:   Class for declaring and handling shared resources.

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
using System.Windows;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model;

namespace Microsoft.Lync.Samples.ContactCenterExtension
{
    public partial class App : Application
    {
        public App()
        {
            Startup += ApplicationStartup;

            Exit += Application_Exit;
            UnhandledException += Application_UnhandledException;

            InitializeComponent();
        }

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            Conversation conversation = LyncClient.GetHostingConversation() as Conversation;

            if (conversation != null)
            {
                bool agent = false;
                try
                {
                    conversation.GetApplicationData("{63D37F02-47B3-4B9E-AA8E-FEF3665298DC}");
                    RootVisual = new Views.AgentDashboardView(new ViewModels.AgentDashboard());
                    agent = true;
                }
                catch (Exception)
                {
                }

                if (!agent)
                {
                    RootVisual = new Views.SupervisorDashboardView(new ViewModels.SupervisorDashboard());
                }
            }
            //RootVisual = new Views.SupervisorDashboardView(new ViewModels.SupervisorDashboard());
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!System.Diagnostics.Debugger.IsAttached)
            {

                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }
        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
