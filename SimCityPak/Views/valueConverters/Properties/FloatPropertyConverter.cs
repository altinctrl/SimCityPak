using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Gibbed.Spore.Properties;
using System.Globalization;
using System.Windows.Markup;
using System.Windows;

namespace SimCityPak
{
    [ValueConversion(typeof(float), typeof(FloatProperty))]
    public class FloatPropertyConverter : MarkupExtension, IValueConverter
    {
        private static FloatPropertyConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // determine if we have an instance of converter
            // return converter to client
            return _converter ?? (_converter = new FloatPropertyConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return ((FloatProperty)value).Value;
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return new FloatProperty()
                {
                    Value = float.Parse((string)value)
                };
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}
