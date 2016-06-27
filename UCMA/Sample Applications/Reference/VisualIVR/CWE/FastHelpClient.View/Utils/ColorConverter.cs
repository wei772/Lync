/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.View.Utils
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// Color code converter 
    /// </summary>
    public class ColorConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value.ToString();
            if (string.IsNullOrEmpty(val)) 
            {
                return null;
            }

            val = val.Replace("#", string.Empty);
            byte a = System.Convert.ToByte("ff", 16);
            byte pos = 0;
            if (val.Length == 8)
            {
                a = System.Convert.ToByte(val.Substring(pos, 2), 16);
                pos = 2;
            }

            byte r = System.Convert.ToByte(val.Substring(pos, 2), 16);
            pos += 2;
            byte g = System.Convert.ToByte(val.Substring(pos, 2), 16);
            pos += 2;
            byte b = System.Convert.ToByte(val.Substring(pos, 2), 16);
            Color col = Color.FromArgb(a, r, g, b); 
            return new SolidColorBrush(col);
        }

        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Converted value</returns>
        public object Convert(object value)
        {
            return this.Convert(value, null, null, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush val = value as SolidColorBrush;
            return "#" + val.Color.A.ToString() + val.Color.R.ToString() + val.Color.G.ToString() + val.Color.B.ToString();
        }
        #endregion
    }
}
