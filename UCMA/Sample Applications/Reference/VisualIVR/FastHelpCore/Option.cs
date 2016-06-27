/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpCore
{
    using System.ComponentModel;
    using System;

    /// <summary>
    /// Represents an Option in the Help Desk Menu
    /// </summary>
    public class FastHelpMenuOption : INotifyPropertyChanged
    {
        /// <summary>
        ///  Id of the option
        /// </summary>
        private string id;

        /// <summary>
        ///  Name of the option
        /// </summary>
        private string name;

        /// <summary>
        ///  Tile color of the option
        /// </summary>
        private string tileColor;

        /// <summary>
        ///  Image path of the option
        /// </summary>
        private Uri imageUrl;

        /// <summary>
        /// Help Desk  Phone number of the option 
        /// </summary>
        private string phoneNo;
       
        /// <summary>
        /// Written  text attribute for BOT to use
        /// </summary>
        private string writtenTxt;

        /// <summary>
        /// Graphical text attribute for CWE to use
        /// </summary>
        private string grahicalTxt;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        /// <value>
        /// The image URL.
        /// </value>
        public Uri ImageUrl
        {
            get
            {
                return this.imageUrl;
            }

            set
            {
                this.imageUrl = value;
                this.OnPropertyChanged(this, "ImageUrl");
            }
        }

        /// <summary>
        /// Gets or sets the color of the tile.
        /// </summary>
        /// <value>
        /// The color of the tile.
        /// </value>
        public string TileColor
        {
            get
            {
                return this.tileColor;
            }

            set
            {
                this.tileColor = value;
                this.OnPropertyChanged(this, "TileColor");
            }
        }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        public string Id
        {
            get
            {
                return this.id;
            }

            set
            {
                this.id = value;
                this.OnPropertyChanged(this, "Id");
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
                this.OnPropertyChanged(this, "Name");
            }
        }

        /// <summary>
        /// Gets or sets the phone no.
        /// </summary>
        /// <value>
        /// The phone no.
        /// </value>
        public string PhoneNo
        {
            get
            {
                return this.phoneNo;
            }

            set
            {
                this.phoneNo = value;
                this.OnPropertyChanged(this, "PhoneNo");
            }
        }

        /// <summary>
        /// Gets or sets the written text.
        /// </summary>
        /// <value>
        /// The written text.
        /// </value>
        public string WrittenText
        {
            get
            {
                return this.writtenTxt;
            }

            set
            {
                this.writtenTxt = value;
                this.OnPropertyChanged(this, "WrittenText");
            }
        }

        /// <summary>
        /// Gets or sets the graphical text.
        /// </summary>
        /// <value>
        /// The graphical text.
        /// </value>
        public string GraphicalText
        {
            get
            {
                return this.grahicalTxt;
            }

            set
            {
                this.grahicalTxt = value;
                this.OnPropertyChanged(this, "GraphicalText");
            }
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
