/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Reflection;
    /// <summary>
    ///  Parser for parsing options from xml file
    /// </summary>
    public class XmlParser
    {
        /// <summary>
        /// Xml Document
        /// </summary>
        private XDocument xdoc;

        public event EventHandler Loaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlParser"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public XmlParser(string path)
        {
            this.Path = path;
            this.xdoc = XDocument.Load(this.Path);
            if (this.Loaded != null)
            {
                this.Loaded(this, null);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlParser"/> class.
        /// </summary>
        public XmlParser()
        {

        }

        public void FetchXml()
        {
            string url = Constants.RestServiceUrl;
            GetIVRMenuOptionsFromRestService(url);
        }

        /// <summary>
        /// Gets ivr menu options from Rest Service
        /// </summary>
        /// <param name="tomorrowForecastUrl">The tomorrow forecast URL.</param>
        private void GetIVRMenuOptionsFromRestService(string ivrMenuRestUrl)
        {
            WebClient client = new WebClient();
            
            client.DownloadStringCompleted += (s, ev) =>
                    {
                        if (ev.Result != null &&
                            !string.IsNullOrEmpty(ev.Result))
                        {
                            string xml = string.Format(CultureInfo.CurrentCulture, "{0}{1}", "<?xml version=\"1.0\" encoding=\"utf-8\" ?>", ev.Result);
                            this.xdoc = XDocument.Parse(xml);
                            if (this.Loaded != null)
                            {
                                this.Loaded(this, null);
                            }
                        }
                    };

                client.DownloadStringAsync(new Uri(ivrMenuRestUrl));
        }

        /// <summary>
        /// Gets or sets the xml file path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets submenus of a given menu from the IVR XML.
        /// </summary>
        /// <param name="topLevelMenuName">Name of the top level menu.</param>
        /// <returns>List of Options </returns>
        public Collection<FastHelpMenuOption> SubOptions(string topLevelMenuName)
        {  
            List<FastHelpMenuOption> options = new List<FastHelpMenuOption>();

            var topLevelNodes = this.xdoc.Descendants("TopLevelOptions");

            var topLevelOption = (from optionXml in topLevelNodes.Elements("TopLevelOption")
                                  where
                                  optionXml.Attribute("name").Value.Equals(topLevelMenuName, StringComparison.OrdinalIgnoreCase)
                               || optionXml.Attribute("writtenText").Value.Equals(topLevelMenuName, StringComparison.OrdinalIgnoreCase)
                               || optionXml.Attribute("id").Value.Equals(topLevelMenuName, StringComparison.OrdinalIgnoreCase)
                                  select optionXml).FirstOrDefault();

            if (topLevelOption != null)
            {
                options = (from subOption in topLevelOption.Element("Options").Descendants("Option")
                           select new FastHelpMenuOption
                           {
                               Id = subOption.Attribute("id").Value,
                               Name = subOption.Attribute("name").Value,
                               TileColor = subOption.Attribute("color").Value,
                               ImageUrl = new Uri(subOption.Attribute("image").Value),
                               PhoneNo = subOption.Attribute("tel").Value,
                               WrittenText = subOption.Attribute("writtenText").Value,
                               GraphicalText = subOption.Attribute("graphicalText").Value,
                           }).ToList<FastHelpMenuOption>();
            }

            return new Collection<FastHelpMenuOption>(options);
        }

        /// <summary>
        /// Gets top level IVR menus from the XML.
        /// </summary>
        /// <returns>List of options</returns>
        public Collection<FastHelpMenuOption> TopLevelMenuOptions()
        {
            var topLevelNodes = this.xdoc.Descendants("TopLevelOptions");


            var topLevelOptions = from optionXml in topLevelNodes.Elements("TopLevelOption")
                                    select new FastHelpMenuOption
                                    {
                                        Id = optionXml.Attribute("id").Value,
                                        Name = optionXml.Attribute("name").Value,
                                        TileColor = optionXml.Attribute("color").Value,
                                        ImageUrl = new Uri(optionXml.Attribute("image").Value),
                                        PhoneNo = optionXml.Attribute("tel").Value,
                                        WrittenText = optionXml.Attribute("writtenText").Value,
                                        GraphicalText = optionXml.Attribute("graphicalText").Value,
                                    };
            return new Collection<FastHelpMenuOption>(topLevelOptions.ToList());
        }

        /// <summary>
        /// Gets submenus of a given menu from the IVR XML.
        /// </summary>
        /// <param name="topLevelMenuName">Name of the top level menu.</param>
        /// <returns>List of Options </returns>
        public FastHelpMenuOption GetTopLevelOptionById(string topLevelMenuId)
        {
            var topLevelNodes = this.xdoc.Descendants("TopLevelOptions");

            var topLevelOptions = from optionXml in topLevelNodes.Elements("TopLevelOption")
                                  select new FastHelpMenuOption
                                  {
                                      Id = optionXml.Attribute("id").Value,
                                      Name = optionXml.Attribute("name").Value,
                                      TileColor = optionXml.Attribute("color").Value,
                                      ImageUrl = new Uri(optionXml.Attribute("image").Value),
                                      PhoneNo = optionXml.Attribute("tel").Value,
                                      WrittenText = optionXml.Attribute("writtenText").Value,
                                      GraphicalText = optionXml.Attribute("graphicalText").Value,
                                  };

            var topLevelOption = topLevelOptions.Where<FastHelpMenuOption>
               ( opt => opt.Id.Equals(topLevelMenuId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            return topLevelOption;
        }


        /// <summary>
        /// Helps the desk number.
        /// </summary>
        /// <param name="menuName">Name of the menu.</param>
        /// <param name="subMenuName">Name of the sub menu.</param>
        /// <returns>Help Desk Number to call for particular Toplevel menu and Submenu combination</returns>
        public string HelpdeskNumber(string menuName, string submenuName)
        {
            Collection<FastHelpMenuOption> subOptions = this.SubOptions(menuName);
            var option = subOptions.Where<FastHelpMenuOption>
                (
                opt => opt.Name.Equals(submenuName, StringComparison.OrdinalIgnoreCase) ||
                    opt.WrittenText.Equals(submenuName, StringComparison.OrdinalIgnoreCase) ||
                    opt.Id.Equals(submenuName, StringComparison.OrdinalIgnoreCase)
                   ).FirstOrDefault();
            return option.PhoneNo;
        }

        /// <summary>
        /// Validate user input against the options availabe in the IVR XML.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        /// <param name="menuLevel">The menu level.</param>
        /// <param name="menuName">Name of the menu.</param>
        /// <returns>True if User input is within the acceptable range of options.</returns>
        public bool ValidateInput(string userInput, int menuLevel, string menuName)
        {
            Collection<FastHelpMenuOption> expectedOptions = new Collection<FastHelpMenuOption>();
            if (menuLevel == 1)
            {
                expectedOptions = this.TopLevelMenuOptions();
            }
            else
            {
                expectedOptions = this.SubOptions(menuName);
            }

            var selectedOption = expectedOptions.Where<FastHelpMenuOption>(
                opt =>
                    opt.Name.Equals(userInput, StringComparison.OrdinalIgnoreCase) ||
                    opt.WrittenText.Equals(userInput, StringComparison.OrdinalIgnoreCase) ||
                    opt.Id.Equals(userInput, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

            return selectedOption != null;
        }
    }
}
