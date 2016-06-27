/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
********************************************************/

using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Browser;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.Views;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.ViewModels;
using System.Globalization;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient
{
    public partial class App : Application
    {
        private const string UserNameQueryString = "un";

        private const string UserPhoneNumberQueryString = "up";

        private const string QueueNameString = "queueName";

        private const string ProductIdString = "id";

        private string _appName = String.Empty;

        /// <summary>
        /// Service name.
        /// </summary>
        private const string ServiceName = "ContactCenterWcfService.svc";

        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
			UIElement root = null;
			Dictionary<string, string> queryStringContext = new Dictionary<string, string>(HtmlPage.Document.QueryString);

			ParticipantConfirmationViewModel participantConfirmationViewModel = this.GetParticipantConfirmationViewModel();
            
            // Allow application to load different Views based on app queryStringContext
            //  If none is specified, default ContactCenter Views are loaded.
            _appName = queryStringContext.ContainsKey("app") ? queryStringContext["app"] : String.Empty;
            
			var participantConfirmationView = new ParticipantConfirmationView();
            root = participantConfirmationView;
			participantConfirmationViewModel.Dispatcher = participantConfirmationView.Dispatcher;
			participantConfirmationView.DataContext = participantConfirmationViewModel;

            Grid rootGrid = new Grid();
            this.RootVisual = rootGrid;
			rootGrid.Children.Add(root);
        }

        private ParticipantConfirmationViewModel GetParticipantConfirmationViewModel()
        {
            string userName = null;
            string phoneNumber = null;
            string queueName = null;
            string productId = null;
            IDictionary<string, string> queryString = HtmlPage.Document.QueryString;
            if(queryString != null) 
            {
                Dictionary<string, string> queryStringContext = new Dictionary<string, string>(HtmlPage.Document.QueryString);
                userName = App.GetUserName(queryStringContext);
                phoneNumber = App.GetUserPhoneNumber(queryStringContext);
                queueName = App.GetQueueName(queryStringContext);
                productId = App.GetProductId(queryStringContext);
            }
            ParticipantConfirmationViewModel participantConfirmationViewModel = new ParticipantConfirmationViewModel(userName, phoneNumber, productId, queueName);
            participantConfirmationViewModel.ConversationCreationRequested += this.ConversationCreationRequested;
            return participantConfirmationViewModel;
        }

        /// <summary>
        /// Conversation creation requested event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConversationCreationRequested(object sender, ConversationCreationRequestedEventArgs e)
        {
            //Switch to conversation view.
            string htmlDocUri = HtmlPage.Document.DocumentUri.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);
            string endpointAddress = String.Format(CultureInfo.CurrentCulture, "http://{0}/{1}", htmlDocUri, App.ServiceName);

            FrameworkElement conversationView;
            
            conversationView = new ConversationView();
            ConversationViewModel conversationViewModel = new ConversationViewModel(conversationView.Dispatcher, endpointAddress, e.UserName, e.PhoneNumber, e.QueueName, e.ProductId);
            conversationView.DataContext = conversationViewModel;

            Grid rootGrid = Application.Current.RootVisual as Grid;
            rootGrid.Children.Clear();
            rootGrid.Children.Add(conversationView);

        }


        /// <summary>
        /// Gets the user name from the query string context.
        /// </summary>
        /// <param name="query">Query string context.</param>
        /// <returns>User name.</returns>
        private static string GetUserName(Dictionary<string, string> queryStringContext)
        {
            string userName = null;
            if (queryStringContext.ContainsKey(App.UserNameQueryString))
            {
                userName = queryStringContext[App.UserNameQueryString];
            }
            return userName;
        }

        /// <summary>
        /// Gets the queue name from the query string context.
        /// </summary>
        /// <param name="query">Query string context.</param>
        /// <returns>queue name.</returns>
        private static string GetQueueName(Dictionary<string, string> queryStringContext)
        {
            string queueName = null;
            if (queryStringContext.ContainsKey(App.QueueNameString))
            {
                queueName = queryStringContext[App.QueueNameString];
            }
            return queueName;
        }

        /// <summary>
        /// Gets the product id from the query string context.
        /// </summary>
        /// <param name="query">Query string context.</param>
        /// <returns>product id.</returns>
        private static string GetProductId(Dictionary<string, string> queryStringContext)
        {
            string productId = null;
            if (queryStringContext.ContainsKey(App.ProductIdString))
            {
                productId = queryStringContext[App.ProductIdString];
            }
            return productId;
        }


        /// <summary>
        /// Gets the user phone number from the query string context.
        /// </summary>
        /// <param name="query">Query string context.</param>
        /// <returns>Phone number.</returns>
        private static string GetUserPhoneNumber(Dictionary<string, string> queryStringContext)
        {
            string userPhoneNumber = null;
            if (queryStringContext.ContainsKey(App.UserPhoneNumberQueryString))
            {
                userPhoneNumber = queryStringContext[App.UserPhoneNumberQueryString];
            }
            return userPhoneNumber;
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
                //Deployment.Current.Dispatcher.BeginInvoke(delegate { this.ReportErrorToDOM(e); });
            }
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight 2 Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
