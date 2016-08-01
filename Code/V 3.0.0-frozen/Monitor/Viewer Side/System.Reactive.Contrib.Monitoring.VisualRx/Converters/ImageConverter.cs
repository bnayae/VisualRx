#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// convert marble diagram name to Image
    /// </summary>
    public class ImageCoverter : IValueConverter
    {
        #region Convert

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = (string)value;
            ImageSource img = null;
            foreach (var plugin in MarbleController.DiagramImageMappersPlugins)
            {
                try
                {
                    img = plugin.Convert(name);
                    if (img != null)
                        break;
                }

                #region Exception Handling

                catch (Exception ex)
                {
                    EventLog.WriteEntry("System.Reactive.Contrib.Monitoring.UI", ex.Message, EventLogEntryType.Error);
                }

                #endregion Exception Handling
            }
            if (img == null)
            {
                img = new BitmapImage(new Uri(@"pack://application:,,,/Images/Tmp.png"));
            }
            return img;
        }

        #endregion Convert

        #region ConvertBack

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(
         object value, Type targetType,
         object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }

        #endregion ConvertBack
    }
}