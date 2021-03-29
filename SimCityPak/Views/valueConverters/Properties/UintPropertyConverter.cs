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
    [ValueConversion(typeof(uint), typeof(UInt32Property))]
    public class UintPropertyConverter : MarkupExtension, IValueConverter
    {
        private static UintPropertyConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // determine if we have an instance of converter
            // return converter to client
            return _converter ?? (_converter = new UintPropertyConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((UInt32Property)value).Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new UInt32Property() { Value = (uint)value };
        }
    }
}
