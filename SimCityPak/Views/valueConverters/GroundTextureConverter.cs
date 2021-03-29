using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using Gibbed.Spore.Package;
using System.Windows.Media.Imaging;

namespace SimCityPak
{
    [ValueConversion(typeof(int), typeof(ImageSource))]
    public class GroundTextureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                BitmapImage image = new BitmapImage(new Uri(string.Format(@"/SimCityPak;component/Images/LotEditor/GroundTextures/{0}.png", (int)value), UriKind.Relative));
                return image;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

}