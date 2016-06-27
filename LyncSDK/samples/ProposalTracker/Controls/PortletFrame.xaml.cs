/*=====================================================================
  File:      PortletFrame.xaml.cs

  Summary:   Backend class for PortletFrame.xaml.

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ProposalTracker.Controls
{
    [ContentProperty("CustomContent")]
    public partial class PortletFrame : UserControl
    {
        public PortletFrame()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Title Property
        /// </summary>
        public string PortletTitle
        {
            get
            {
                return (string)GetValue(PortletTitleProperty);
            }
            set
            {
                SetValue(PortletTitleProperty, value);
            }
        }

        /// <summary>
        /// PortletIcon Property
        /// </summary>
        public object PortletIcon
        {
            get
            {
                return (FrameworkElement)GetValue(PortletIconProperty);
            }
            set
            {
                SetValue(PortletIconProperty, value);
            }
        }

        /// <summary>
        /// CustomContent Property
        /// </summary>
        public object CustomContent
        {
            get
            {
                return (FrameworkElement)GetValue(CustomContentProperty);
            }
            set
            {
                SetValue(CustomContentProperty, value);
            }
        }

        /// <summary>
        /// Title Dependency Property.
        /// </summary>
        public static readonly DependencyProperty PortletTitleProperty =
            DependencyProperty.Register("Title",
                                        typeof(string),
                                        typeof(PortletFrame),
                                        new PropertyMetadata("Title"));

        /// <summary>
        /// PortletIcon Dependency Property.
        /// </summary>
        public static readonly DependencyProperty PortletIconProperty =
            DependencyProperty.Register("PortletIcon",
                                        typeof(object),
                                        typeof(PortletFrame),
                                        new PropertyMetadata(null));


        /// <summary>
        /// CustomContent Dependency Property.
        /// </summary>
        public static readonly DependencyProperty CustomContentProperty =
            DependencyProperty.Register("CustomContent",
                                        typeof(object),
                                        typeof(PortletFrame),
                                        new PropertyMetadata(null));


    }
}
