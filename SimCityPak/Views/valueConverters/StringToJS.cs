using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

using JSBeautifyLib;

namespace SimCityPak.Views.valueConverters
{
    /// <summary>
    /// Beautifies a string with JavaScript code.
    /// </summary>
    public class StringToJS : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var js = new JSBeautify(value as String,
                new JSBeautifyOptions
                {
                    indent_char = ' ',
                    indent_level = 0,
                    indent_size = 4,
                    preserve_newlines = true
                });
            return js.GetResult();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("TODO: Implement JavaScript Minimizer.");
        }
    }
}
