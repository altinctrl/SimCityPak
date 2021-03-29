using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Gibbed.Spore.Properties;
using System.Globalization;
using System.Windows.Markup;

namespace SimCityPak
{
    [ValueConversion(typeof(string), typeof(String8Property))]
    public class StringPropertyConverter : MarkupExtension, IValueConverter
    {
        private static StringPropertyConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // determine if we have an instance of converter
            // return converter to client
            return _converter ?? (_converter = new StringPropertyConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((String8Property)value).Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new String8Property() { Value = (string)value };
        }
    }
}
