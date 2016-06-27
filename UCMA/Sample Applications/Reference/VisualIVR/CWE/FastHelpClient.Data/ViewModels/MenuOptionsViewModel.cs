/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data
{
    using System;
    using System.Windows;
    using FastHelpClient.Data.ViewModels;
    using FastHelpCore;
    using System.Globalization;
    using FastHelpClient.Data.Events;

    /// <summary>
    /// Viewmodel for Submenu page
    /// </summary>
    public class MenuOptionsViewModel : BaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuOptionsViewModel"/> class.
        /// </summary>
        public MenuOptionsViewModel() :
            base(OptionService.Instance)
        {
        }

        /// <summary>
        /// Sends the IM.
        /// </summary>
        /// <param name="text">The text.</param>
        public override void SendIM(string text)
        {
            if (text.Equals("#") || this.CallHandler.CheckCallStatus())
            {
                this.CallHandler.SendIM(text);
            }
        }

        /// <summary>
        /// Handles the Received event of the CallHandler_IM control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FastHelpClient.Data.Events.CallEventArgs"/> instance containing the event data.</param>
        protected override void CallHandler_IM_Received(object sender, CallEventArgs e)
        {
            if (e.MenuName == "#")
            {
                this.NavigateToMainMenu(false);
            }
        }

        public override void NavigateToMainMenu(bool sendMsg)
        {
            this.CallHandler.IM_Received -= new EventHandler<CallEventArgs>(this.CallHandler_IM_Received);
            base.NavigateToMainMenu(sendMsg);
        }

        /// <summary>
        /// Handles the OptionLoadingComplete event of the OptionService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FastHelpClient.Data.OptionServiceLoadingEventArgs"/> instance containing the event data.</param>
        protected override void OptionService_OptionLoadingComplete(object sender, OptionServiceLoadingEventArgs e)
        {
            // Fire Event on UI Thread
            Application.Current.RootVisual.Dispatcher.BeginInvoke(() =>
            {
                this.Options.Clear();

                if (e.Results != null)
                {
                    foreach (FastHelpMenuOption opt in e.Results)
                    {
                        this.Options.Add(opt);
                    }

                    int count = this.Options.Count;
                    if (count < MaxOptions)
                    {

                        for (; count < MaxOptions - 3; count++)
                        {
                            this.Options.Add(new FastHelpMenuOption { Name = string.Empty, TileColor = "#7F7F7F", Id = (count + 1).ToString() });
                        }


                        string[] lastRow = { "*", "0", "#" };

                        for (int rowCount = 0; rowCount < 3; rowCount++)
                        {
                            this.Options.Add(new FastHelpMenuOption
                            {
                                Name = string.Empty,
                                TileColor = "#7F7F7F",
                                Id = lastRow[rowCount]
                            });
                        }
                    }
                }

                base.RaiseLoadComplete(null);
            });
        }

        private string GetImagePath()
        {
            string hostName = Application.Current.Host.Source.AbsoluteUri;
            int index = hostName.IndexOf("Clientbin", StringComparison.OrdinalIgnoreCase);

            if (index < 0)
            {
                index = hostName.IndexOf("bin/Debug", StringComparison.OrdinalIgnoreCase);
            }

            hostName = hostName.Substring(0, index);
            return string.Format(CultureInfo.CurrentCulture, "{0}{1}", hostName, "Images");
        }
    }
}
