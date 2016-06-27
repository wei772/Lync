/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data
{
    using System;
    using System.Collections.Generic;
    using FastHelpCore;
    using System.Threading;

    /// <summary>
    /// Service defined to load options from xml file
    /// </summary>
    public class OptionService : IOptionService
    {
        /// <summary>
        /// Singleton OptionService Object
        /// </summary>
        private static OptionService optionService = null;

        /// <summary>
        ///  Instance of xml parser
        /// </summary>
        private XmlParser xmlparser;

        private bool isLoaded;

        /// <summary>
        /// Prevents a default instance of the <see cref="OptionService"/> class from being created.
        /// </summary>
        private OptionService()
        {
            isLoaded = false;
            LoadXml();
        }

        /// <summary>
        /// Occurs when options are loaded
        /// </summary>
        public event EventHandler<OptionServiceLoadingEventArgs> OptionLoadingComplete;

        /// <summary>
        /// Occurs when an error occurs while loading the options
        /// </summary>
        #pragma warning disable 0067
        public event EventHandler<OptionServiceErrorEventArgs> OptionLoadingError;
        #pragma warning restore 0067
        /// <summary>
        /// Occurs when options are loaded
        /// </summary>
        public event EventHandler ServiceLoadComplete;

        /// <summary>
        /// Gets or sets the XML path for xml parser.
        /// </summary>
        /// <value>
        /// The XML path.
        /// </value>
        public string XmlPath { get; set; }

        public bool IsLoaded
        {
            get
            {
                return this.isLoaded;
            }
            set
            {
                this.isLoaded = value;
            }
        }
        /// <summary>
        /// Options the service instance.
        /// </summary>
        /// <returns>OptionService Object</returns>
        public static OptionService Instance
        {
            get
            {
                if (optionService == null)
                {
                    optionService = new OptionService();
                }

                return optionService;
            }
        }

        public void LoadXml()
        {
            this.xmlparser = new XmlParser();
            this.xmlparser.FetchXml();
            this.xmlparser.Loaded += (sender, evt) =>
             {
                 RaiseServiceLoadComplete(null);
             };
        }

        /// <summary>
        /// Gets the top level options.
        /// </summary>
        public void GetTopLevelOptions()
        {
            var results = this.xmlparser.TopLevelMenuOptions();
            if (this.OptionLoadingComplete != null)
            {               
                this.OptionLoadingComplete(this, new OptionServiceLoadingEventArgs(results));
            }
        }

        /// <summary>
        /// Gets the top level options.
        /// </summary>
        public FastHelpMenuOption GetTopLevelOptionById(string levelId)
        {
            if (!string.IsNullOrEmpty(levelId))
            {
                var result = this.xmlparser.GetTopLevelOptionById(levelId);
                return result;
            }

            return null;
        }


        /// <summary>
        /// Gets the options for level.
        /// </summary>
        /// <param name="levelname">The levelname.</param>
        public void GetOptionsForLevel(string levelname)
        {
            var results = this.xmlparser.SubOptions(levelname);
            if (this.OptionLoadingComplete != null)
            {                
                this.OptionLoadingComplete(this, new OptionServiceLoadingEventArgs(results));
            }
        }

        /// <summary>
        /// Gets the top level options with levels.
        /// </summary>
        public void GetTopLevelOptionsWithLevels()
        {
            throw new NotImplementedException();
        }

        protected virtual void RaiseServiceLoadComplete(EventArgs evt)
        { 
            isLoaded = true;
            if (this.ServiceLoadComplete != null)
            {               
                this.ServiceLoadComplete(this, evt);
            }
        }        
    }
}
