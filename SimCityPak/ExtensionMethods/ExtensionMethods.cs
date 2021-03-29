using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace SimCityPak
{
    public static class ExtensionMethods
    {
        public static string ToHex(this uint value)
        {
            return string.Format("0x{0:x8}", value); 
        }

        public static uint ToUint(this float value)
        {
            return Convert.ToUInt32(value);
        }

        public static IEnumerable<string> SplitIntoChunks(this string text, int chunkSize)
        {
            int offset = 0;
            while (offset < text.Length)
            {
                int size = Math.Min(chunkSize, text.Length - offset);
                yield return text.Substring(offset, size);
                offset += size;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PixelColor
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;
        }

        public static void CopyPixelsOverride(this BitmapSource source, PixelColor[,] pixels, int stride, int offset)
        {
            var height = source.PixelHeight;
            var width = source.PixelWidth;
            var pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, stride, 0);
            int y0 = offset / width;
            int x0 = offset - width * y0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixels[x + x0, y + y0] = new PixelColor
                    {
                        Blue = pixelBytes[(y * width + x) * 4 + 0],
                        Green = pixelBytes[(y * width + x) * 4 + 1],
                        Red = pixelBytes[(y * width + x) * 4 + 2],
                        Alpha = pixelBytes[(y * width + x) * 4 + 3],
                    };
        }

        public static PixelColor[,] GetPixels(this BitmapSource source)
        {
           // if (source.Format != PixelFormats.Pbgra32)
           //     source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[width, height];

            source.CopyPixelsOverride(result, width * 4, 0);
            return result;
        }


        public static DependencyObject GetParent(this DependencyObject source, Type type)
        {
            if (source == null)
            {
                return null;
            }
            if (source.GetType() == type)
            {
                return source;
            }
            else
            {
                return VisualTreeHelper.GetParent(source).GetParent(type);
            }
        }
    }
}
