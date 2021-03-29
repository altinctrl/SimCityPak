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
    public enum TypeIds
    {
        PropertyFile = 0x00b1b104,
        RasterFile = 0x2f4e681c,
        Unknown
    }

    public enum PropertyFileTypeIds
    {
        Agent = 0xC600,
        Path = 0x8B7E,
        Unit = 0xC000,
        Network = 0xC400,
        Menu = 0xC900,
        Menu2 = 0x8A01,
        Map = 0xE000,
        Descriptor = 0x2043,
        DecalAtlas = 0xb185,
        DecalAtlas2 = 0x1651,
        DecalAtlas3 = 0x1652,
        Unknown
    }


    [ValueConversion(typeof(DatabaseIndex), typeof(ImageSource))]
    public class InstanceTypeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is DatabaseIndex || value is uint)
            {
                uint? typeId = null;
                uint? instanceType = null;
                if (value is DatabaseIndex)
                {
                    DatabaseIndex index = value as DatabaseIndex;
                    //get the icon based on the abbreviation
                    typeId = index.TypeId;
                    instanceType = index.InstanceType;
                }
                else if(value is uint)
                {
                    typeId = value as uint?;
                }

                if (typeId != null)
                {
                    if (typeId != (uint)TypeIds.PropertyFile)
                    {
                        BitmapImage image = new BitmapImage(new Uri(string.Format(@"/SimCityPak;component/Images/InstanceTypeIcons/16x16_{0}.png", TGIRegistry.Instance.FileTypes.GetAbbreviation(typeId.Value).Trim()), UriKind.Relative));
                        return image;
                    }
                    else if (instanceType != null)
                    {
                        switch ((PropertyFileTypeIds)instanceType)
                        {
                            case PropertyFileTypeIds.Agent: return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/PROP/16x16_AGENT.png", UriKind.Relative)); //AGENT
                            case PropertyFileTypeIds.Path: return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/PROP/16x16_PATH.png", UriKind.Relative)); //PATH
                            case PropertyFileTypeIds.Unit: return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/PROP/16x16_UNIT.png", UriKind.Relative)); //UNIT
                            case PropertyFileTypeIds.Network: return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/PROP/16x16_NETWORK.png", UriKind.Relative)); //MAP
                            case PropertyFileTypeIds.Menu: return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/PROP/16x16_MENU.png", UriKind.Relative)); //MENU
                            case PropertyFileTypeIds.Menu2: return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/PROP/16x16_MENU.png", UriKind.Relative)); //MENU
                            case PropertyFileTypeIds.Map: return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/PROP/16x16_MAP.png", UriKind.Relative)); //MAP
                            case PropertyFileTypeIds.Descriptor: return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/PROP/16x16_DESC.png", UriKind.Relative)); //MAP
                            case PropertyFileTypeIds.DecalAtlas: return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/PROP/16x16_DECALATLAS.png", UriKind.Relative));
                            case PropertyFileTypeIds.DecalAtlas2: return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/PROP/16x16_DECALATLAS.png", UriKind.Relative));
                            case PropertyFileTypeIds.DecalAtlas3: return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/PROP/16x16_DECALATLAS.png", UriKind.Relative));
                        }

                        return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/16x16_PROP.png", UriKind.Relative));
                    }
                    else
                    {
                        return new BitmapImage(new Uri(@"/SimCityPak;component/Images/InstanceTypeIcons/16x16_PROP.png", UriKind.Relative));
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

}