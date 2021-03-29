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
    [ValueConversion(typeof(bool), typeof(BoolProperty))]
    public class BoolPropertyConverter : MarkupExtension, IValueConverter
    {
        private static BoolPropertyConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // determine if we have an instance of converter
            // return converter to client
            return _converter ?? (_converter = new BoolPropertyConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return ((BoolProperty)value).Value;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BoolProperty() { Value = (bool)value };
        }
    }
}
