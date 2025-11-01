using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EXOApp.Converters
{
    public class MusicNameFormatter : IValueConverter
    {
        /// <summary>
        /// Converts music tracks to a readable name
        /// </summary>
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return convertMusicName(value.ToString());
            throw new NotImplementedException();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes specific parts of the music sql entry to be more readable
        /// </summary>
        public static string convertMusicName(string input)
        {
            if (input.StartsWith("df_"))
            {
                return input.Replace("df_", "").Replace("_", " ").Replace(".mp3", "");
            }
            else
            {
                return input.Replace(".mp3", "");
            }
        }
    }
}
