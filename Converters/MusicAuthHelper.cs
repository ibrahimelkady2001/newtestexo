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
    /// Works out whether the delete button should be shown for specific music tracks on the view
    /// </summary>
    class MusicAuthHelper : IValueConverter
    {
        /// <summary>
        /// Music with auth of 0 cannot be deleted
        /// </summary>
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int auth;
            
            if(int.TryParse(value.ToString(), out auth))
            {
                if(auth == 0)
                {
                    return false;
                }
            }
            return true;
            throw new NotImplementedException();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
