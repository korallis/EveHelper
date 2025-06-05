using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EveHelper.App.Converters
{
    /// <summary>
    /// Converts boolean values to Visibility values (inverted)
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value (inverted)
        /// </summary>
        /// <param name="value">The boolean value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">The culture</param>
        /// <returns>Collapsed if true, Visible if false</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }

            if (value is string stringValue)
            {
                return !string.IsNullOrWhiteSpace(stringValue) ? Visibility.Collapsed : Visibility.Visible;
            }

            return value != null ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Converts a Visibility value back to a boolean value (inverted)
        /// </summary>
        /// <param name="value">The Visibility value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">The culture</param>
        /// <returns>False if Visible, true otherwise</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }

            return true;
        }
    }
} 