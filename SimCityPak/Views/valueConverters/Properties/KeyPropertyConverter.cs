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
    [ValueConversion(typeof(string), typeof(KeyProperty))]
    public class KeyPropertyConverter : MarkupExtension,  IValueConverter
    {
        private static KeyPropertyConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // determine if we have an instance of converter
            // return converter to client
            return _converter ?? (_converter = new KeyPropertyConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {

         
            if (parameter != null && parameter.ToString().ToLower() == "uint")
            {
                return ((KeyProperty)value).InstanceId;
            }
            if (targetType == typeof(string))
            {
                return string.Format("0x{0:X8}", ((KeyProperty)value).InstanceId);
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                try
                {
                    if (((string)value).ToLower().StartsWith("0x"))
                    {
                        return new KeyProperty() { InstanceId = System.Convert.ToUInt32(((string)value).Substring(2), 16) };
                    }
                    else
                    {
                        return DependencyProperty.UnsetValue;
                    }
                }
                catch { return DependencyProperty.UnsetValue; }
            }
            else if(value is uint || value is int)
            {
                return new KeyProperty() { InstanceId = (uint)value };
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
