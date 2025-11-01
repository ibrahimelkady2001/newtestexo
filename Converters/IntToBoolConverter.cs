using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EXOApp.Converters
{
    /// <summary>
    /// Converts Ints to Bool for the View
    /// </summary>
    public class IntToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts an int to a bool
        /// </summary>
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (float.Parse(value.ToString()) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
            throw new NotImplementedException();
        }
        /// <summary>
        /// Converts a bool to an int
        /// </summary>
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return 1;
            }
            else
            {
                return 0;
            }
            throw new NotImplementedException();
        }
    }
}
