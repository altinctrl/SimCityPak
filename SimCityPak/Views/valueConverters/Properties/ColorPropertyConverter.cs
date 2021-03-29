using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Gibbed.Spore.Properties;
using System.Globalization;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Media;

namespace SimCityPak
{
    [ValueConversion(typeof(object), typeof(ColorRGBProperty))]
    public class ColorPropertyConverter : MarkupExtension,  IValueConverter
    {
        private static ColorPropertyConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // determine if we have an instance of converter
            // return converter to client
            return _converter ?? (_converter = new ColorPropertyConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {  
            if (value is ColorRGBAProperty)
            {
                ColorRGBAProperty color = value as ColorRGBAProperty;
                return new SolidColorBrush(Color.FromScRgb(color.A, color.R, color.G, color.B));
            }
            if (value is ColorRGBProperty)
            {
                ColorRGBProperty color = value as ColorRGBProperty;
                return new SolidColorBrush(Color.FromScRgb(1, color.R, color.G, color.B));
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
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
            else if(value is uint)
            {
                return new KeyProperty() { InstanceId = (uint)value };
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
