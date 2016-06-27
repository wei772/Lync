/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using FastHelpClient.Data.Communication;
    using FastHelpClient.Data.Navigation;
    using FastHelpCore;
    using System.Globalization;

    /// <summary>
    /// Base ViewModel
    /// </summary>
    public class BaseViewModel : INavigable
    {
        /// <summary>
        /// Instance of OptionService
        /// </summary>
        private IOptionService optionService;

        /// <summary>
        /// Number of options on the Grid
        /// </summary>
        protected const int MaxOptions = 12;

        /// <summary>
        /// Options for the view
        /// </summary>
        private ObservableCollection<FastHelpMenuOption> options = new ObservableCollection<FastHelpMenuOption>();

        /// <summary>
        /// Instance of Call Handler
        /// </summary>
        private CallHandler callHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseViewModel"/> class.
        /// </summary>
        /// <param name="optService">The opt service.</param>
        public BaseViewModel(IOptionService optService)
        {
            this.callHandler = Communication.CallHandler.GetCallHandler();
            this.callHandler.IM_Received += new EventHandler<Events.CallEventArgs>(this.CallHandler_IM_Received);
            this.optionService = optService;
            this.optionService.ServiceLoadComplete += new EventHandler(OptionService_ServiceLoadComplete);
            this.optionService.OptionLoadingComplete += new EventHandler<OptionServiceLoadingEventArgs>(this.OptionService_OptionLoadingComplete);
            this.optionService.OptionLoadingError += new EventHandler<OptionServiceErrorEventArgs>(this.OptionService_ErrorLoading);
        }



        /// <summary>
        /// Occurs when [load complete].
        /// </summary>
        public event EventHandler LoadComplete;

        /// <summary>
        /// Occurs when [error loading].
        /// </summary>
        public event EventHandler ErrorLoading;

        /// <summary>
        /// Gets the options.
        /// </summary>
        public ObservableCollection<FastHelpMenuOption> Options
        {
            get
            {
                return this.options;
            }
        }

        public bool IsLoaded
        {
            get
            {
                return this.optionService.IsLoaded;
            }
        }
        /// <summary>
        /// Gets or sets the navigation service.
        /// </summary>
        /// <value>
        /// The navigation service.
        /// </value>
        public virtual INavigationService NavigationService { get; set; }

        /// <summary>
        /// Gets or sets the current navigation level.
        /// </summary>
        /// <value>
        /// The current navigation level.
        /// </value>
        public int CurrentNavigationLevel
        {
            get
            {
                return this.callHandler.CurrentMenuLevel;
            }

            set
            {
                this.callHandler.CurrentMenuLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the current menu request.
        /// </summary>
        /// <value>
        /// The current menu request.
        /// </value>
        public string CurrentMenuRequest
        {
            get
            {
                return this.callHandler.MenuRequest;
            }

            set
            {
                this.callHandler.MenuRequest = value;
            }
        }

        /// <summary>
        /// Gets or sets the call handler.
        /// </summary>
        /// <value>
        /// The call handler.
        /// </value>
        protected CallHandler CallHandler
        {
            get
            {
                return this.callHandler;
            }

            set
            {
                this.callHandler = value;
            }
        }

        /// <summary>
        /// Sends the IM.
        /// </summary>
        /// <param name="text">The text.</param>
        public virtual void SendIM(string text)
        {
            this.CallHandler.SendIM(text);
        }

        /// <summary>
        /// Gets the top level options.
        /// </summary>
        public void GetTopLevelOptions()
        {
            this.optionService.GetTopLevelOptions();
        }

        /// <summary>
        /// Gets the top level options by id.
        /// </summary>
        public FastHelpMenuOption GetTopLevelOptionById(string levelId)
        {
           return this.optionService.GetTopLevelOptionById(levelId);
        }

        /// <summary>
        /// Gets the options for level.
        /// </summary>
        /// <param name="level">The level.</param>
        public void GetOptionsForLevel(string level)
        {
            this.optionService.GetOptionsForLevel(level);
        }

        /// <summary>
        /// Raises the load complete.
        /// </summary>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void RaiseLoadComplete(EventArgs args)
        {
            if (this.LoadComplete != null)
            {
                this.LoadComplete(this, args);
            }

           this.optionService.OptionLoadingComplete -= new EventHandler<OptionServiceLoadingEventArgs>(this.OptionService_OptionLoadingComplete);
        }

        /// <summary>
        /// Raises the error loading.
        /// </summary>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void RaiseErrorLoading(EventArgs args)
        {
            if (this.ErrorLoading != null)
            {
                this.ErrorLoading(this, args);
            }

           this.optionService.OptionLoadingError -= new EventHandler<OptionServiceErrorEventArgs>(this.OptionService_ErrorLoading);
        }

        /// <summary>
        /// Handles the ErrorLoading event of the OptionService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FastHelpClient.Data.OptionServiceErrorEventArgs"/> instance containing the event data.</param>
        protected virtual void OptionService_ErrorLoading(object sender, OptionServiceErrorEventArgs e)
        {
        }

        /// <summary>
        /// Handles the OptionLoadingComplete event of the OptionService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FastHelpClient.Data.OptionServiceLoadingEventArgs"/> instance containing the event data.</param>
        protected virtual void OptionService_OptionLoadingComplete(object sender, OptionServiceLoadingEventArgs e)
        {
        }


        /// <summary>
        /// Handles the ServiceLoadComplete event of the OptionService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OptionService_ServiceLoadComplete(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Handles the Received event of the Call Handler .
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FastHelpClient.Data.Events.CallEventArgs"/> instance containing the event data.</param>
        protected virtual void CallHandler_IM_Received(object sender, Events.CallEventArgs e)
        {
        }

        
        /// <summary>
        /// Handles the Received event of the Call Handler .
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FastHelpClient.Data.Events.CallEventArgs"/> instance containing the event data.</param>
        public virtual void NavigateToMainMenu(bool sendMsg)
        {
            if (sendMsg)
            {
                SendIM("#");
            }

            NavigateToMenu("MainMenu", "Back=true");
        }

        /// <summary>
        /// Handles the Received event of the Call Handler .
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FastHelpClient.Data.Events.CallEventArgs"/> instance containing the event data.</param>
        public virtual void NavigateToSearchMenu()
        {
            SendIM("#search");
            NavigateToMenu("Search",string.Empty);
        }

        /// <summary>
        /// Handles the Received event of the Call Handler .
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FastHelpClient.Data.Events.CallEventArgs"/> instance containing the event data.</param>
        public virtual void NavigateToTicketsMenu()
        {
            SendIM("#");
            NavigateToMenu("Tickets", string.Empty);
        }

        /// <summary>
        /// Handles the Received event of the Call Handler .
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FastHelpClient.Data.Events.CallEventArgs"/> instance containing the event data.</param>
        protected virtual void NavigateToMenu(string menuName,string args)
        {
            this.NavigationService.Navigate(string.Format(CultureInfo.CurrentCulture, "/Views/{0}.xaml?{1}",menuName,args));
        } 
    }
}
