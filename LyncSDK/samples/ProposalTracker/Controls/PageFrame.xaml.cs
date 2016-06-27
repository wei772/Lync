/*=====================================================================
  File:      PageFrame.xaml.cs

  Summary:   Backend class for PageFrame.xaml.

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
    [ContentProperty("PageCustomContent")]
    public partial class PageFrame : UserControl
    {
        public PageFrame()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Page Title Property
        /// </summary>
        public string PageTitle
        {
            get
            {
                return (string)GetValue(PageTitleProperty);
            }
            set
            {
                SetValue(PageTitleProperty, value);
            }
        }

        /// <summary>
        /// Page Icon Property
        /// </summary>
        public object PageIcon
        {
            get
            {
                return (FrameworkElement)GetValue(PageIconProperty);
            }
            set
            {
                SetValue(PageIconProperty, value);
            }
        }

        /// <summary>
        /// PageStatusArea Property (for MyStatus Area)
        /// </summary>
        public object PageStatusArea
        {
            get
            {
                return (FrameworkElement)GetValue(PageStatusAreaProperty);
            }
            set
            {
                SetValue(PageStatusAreaProperty, value);
            }

        }

        /// <summary>
        /// PageCustomContent Property
        /// </summary>
        public object PageCustomContent
        {
            get
            {
                return (FrameworkElement)GetValue(PageCustomContentProperty);
            }
            set
            {
                SetValue(PageCustomContentProperty, value);
            }
        }

        /// <summary>
        /// Title Dependency Property.
        /// </summary>
        public static readonly DependencyProperty PageTitleProperty =
            DependencyProperty.Register("Title",
                                        typeof(string),
                                        typeof(PageFrame),
                                        new PropertyMetadata("Title"));

        /// <summary>
        /// PageIcon Dependency Property.
        /// </summary>
        public static readonly DependencyProperty PageIconProperty =
            DependencyProperty.Register("PageIcon",
                                        typeof(object),
                                        typeof(PageFrame),
                                        new PropertyMetadata(null));

        /// <summary>
        /// PageIcon Dependency Property.
        /// </summary>
        public static readonly DependencyProperty PageStatusAreaProperty =
            DependencyProperty.Register("PageStatusArea",
                                        typeof(object),
                                        typeof(PageFrame),
                                        new PropertyMetadata(null));


        /// <summary>
        /// PageCustomContent Dependency Property.
        /// </summary>
        public static readonly DependencyProperty PageCustomContentProperty =
            DependencyProperty.Register("PageCustomContent",
                                        typeof(object),
                                        typeof(PageFrame),
                                        new PropertyMetadata(null));



        
    }
}
