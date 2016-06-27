/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.Data.Navigation
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Helper Class to implement naviagtion between Pages of type INavigable
    /// </summary>
    public static class Navigator
    {
        /// <summary>
        /// Defines a Dependancy property named "Source"
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
             DependencyProperty.RegisterAttached("Source", typeof(INavigable), typeof(Navigator), new PropertyMetadata(OnSourceChanged));

        /// <summary>
        /// Gets the Source
        /// </summary>
        /// <param name="obj">Dependancy object like a Page</param>
        /// <returns> The source of the  </returns>
        public static INavigable GetSource(DependencyObject obj)
        {
            return (INavigable)obj.GetValue(SourceProperty);
        }

        /// <summary>
        /// Sets the source.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="value">The value.</param>
        public static void SetSource(DependencyObject obj, INavigable value)
        {
            obj.SetValue(SourceProperty, value);
        }

        /// <summary>
        /// Called when [source changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Page page = (Page)d;

            page.Loaded += PageLoaded;
        }

        /// <summary>
        /// Page load handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private static void PageLoaded(object sender, RoutedEventArgs e)
        {
            Page page = (Page)sender;

            INavigable navSource = GetSource(page);

            if (navSource != null)
            {
                navSource.NavigationService = new NavigationService(page.NavigationService);
            }
        }
    }
}