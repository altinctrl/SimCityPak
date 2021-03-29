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
    [ValueConversion(typeof(string), typeof(Vector3Property))]
    public class Vector3PropertyConverter : MarkupExtension, IValueConverter
    {
        private static Vector3PropertyConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // determine if we have an instance of converter
            // return converter to client
            return _converter ?? (_converter = new Vector3PropertyConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            return ((FloatProperty)value).Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new FloatProperty() { Value = (float)value };
        }
    }
}
