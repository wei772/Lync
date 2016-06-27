/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data
{
    using System;
    using System.Linq;
    using System.Windows;
    using FastHelpClient.Data.ViewModels;
    using FastHelpCore;
    using System.Globalization;

    /// <summary>
    /// ViewModel associated with Manin menu view
    /// </summary>
    public class MainMenuViewModel : BaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainMenuViewModel"/> class.
        /// </summary>
        public MainMenuViewModel()
            : base(OptionService.Instance)
        {
        }

        /// <summary>
        /// Sends the IM.
        /// </summary>
        /// <param name="text">The text.</param>
        public override void SendIM(string text)
        {
            this.CallHandler.SendIM(text);
        }

        /// <summary>
        /// Handles the OptionLoadingComplete event of the optionService control.
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

                base.RaiseLoadComplete(null);
            });
        }

        /// <summary>
        /// Handles the Received event of the Call Handler .
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FastHelpClient.Data.Events.CallEventArgs"/> instance containing the event data.</param>
        protected override void CallHandler_IM_Received(object sender, Events.CallEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.MenuName) && !e.MenuName.Equals("#") && !e.MenuName.Equals("#search"))
            {
                if (this.CurrentNavigationLevel == 1)
                {
                    this.CurrentNavigationLevel = 2;
                    this.CallHandler.IM_Received -=
                        new EventHandler<Events.CallEventArgs>(this.CallHandler_IM_Received);

                    var option = this.Options.Where<FastHelpMenuOption>(
                        opt => opt.Name.Equals(e.MenuName, StringComparison.OrdinalIgnoreCase) ||
                            opt.Id.Equals(e.MenuName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    if (option != null)
                    {
                        var args = string.Format(CultureInfo.CurrentCulture,
                            "Id={0}&Option={1}&Color={2}", option.Id, option.Name,
                            option.TileColor.Replace("#", string.Empty));

                        NavigateToMenu("MenuOptions", args);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ServiceLoadComplete event of the OptionService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OptionService_ServiceLoadComplete(object sender, EventArgs e)
        {
            this.CurrentNavigationLevel = 1;
            this.GetTopLevelOptions();
        }
    }
}
