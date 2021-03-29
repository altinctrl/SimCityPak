using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using Gibbed.Spore.Helpers;
using Microsoft.Win32;
using Gibbed.Spore.Package;
using SporeMaster.RenderWare4;
using System.Windows.Media;

namespace SimCityPak
{
    public enum RasterChannel
    {
        Preview,
        A,
        R,
        G,
        B,
        All,
        FacadeColor
    }

    public class RasterImage
    {
        public WriteableBitmap[] MipMaps { get; set; }
        public uint RasterType { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint MipMapCount { get; set; }
        public uint PixelSize { get; set; }
        public uint RasterPixelFormat { get; set; }
        public byte[] RasterData { get; set; }

        public static RasterImage CreateFromStream(Stream byteStream, RasterChannel channel)
        {
            return RasterImage.CreateFromStream(byteStream, channel, Colors.Black, Colors.Red, Color.FromRgb(0, 255, 0), Colors.Blue);
        }

        public static RasterImage CreateFromStream(Stream byteStream, RasterChannel channel, Color color1, Color color2, Color color3, Color color4)
        {
            RasterImage image = new RasterImage();
            image.RasterType = byteStream.ReadU32().Swap();
            image.Width = byteStream.ReadU32().Swap();
            image.Height = byteStream.ReadU32().Swap();
            image.MipMapCount = byteStream.ReadU32().Swap();
            image.PixelSize = byteStream.ReadU32().Swap();
            image.RasterPixelFormat = byteStream.ReadU32().Swap();

            uint originalWidth = image.Width;
            uint originalHeight = image.Height;

            int width = (int)image.Width;
            int height = (int)image.Height;

            image.MipMaps = new WriteableBitmap[image.MipMapCount];

            for (int i = 0; i < image.MipMapCount; i++)
            {
                uint blockSize = byteStream.ReadU32().Swap();
                WriteableBitmap bitmap = new WriteableBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32, BitmapPalettes.Halftone252);
                if (image.RasterPixelFormat == 21)
                {
                    for (int j = 0; j < (blockSize / 4); j++)
                    {
                        byte r = byteStream.ReadU8();
                        byte g = byteStream.ReadU8();
                        byte b = byteStream.ReadU8();
                        byte a = byteStream.ReadU8();

                        if (channel == RasterChannel.Preview)
                        {
                            //layers are in RGBA order  

                            int layer1 = a >= 128 ? a : 0;
                            int layer2 = r >= 128 ? r : 0;
                            int layer3 = g >= 128 ? g : 0;
                            int layer4 = b >= 128 ? b : 0;

                            if (layer1 > 0)
                            {
                                bitmap.SetPixel((int)(j % width), (int)(j / width), (byte)255, (byte)color1.R, (byte)color1.G, (byte)color1.B);
                            }
                            else if (layer2 > 0)
                            {
                                bitmap.SetPixel((int)(j % width), (int)(j / width), (byte)255, (byte)color2.R, (byte)color2.G, (byte)color2.B);
                            }
                            else if (layer3 > 0)
                            {
                                bitmap.SetPixel((int)(j % width), (int)(j / width), (byte)255, (byte)color3.R, (byte)color3.G, (byte)color3.B);
                            }
                            else if (layer4 > 0)
                            {
                                bitmap.SetPixel((int)(j % width), (int)(j / width), (byte)255, (byte)color4.R, (byte)color4.G, (byte)color4.B);
                            }
                            else
                            {
                                bitmap.SetPixel((int)(j % width), (int)(j / width), (byte)0, (byte)0, (byte)0, (byte)0);
                            }


                            // bitmap.SetPixel((int)(j % width), (int)(j / width), (byte)Math.Max(Math.Max(Math.Max(layer1, layer2), layer3), layer4)  , (byte)layer2, (byte)layer3, (byte)layer4);

                        }
                        else if (channel == RasterChannel.All)
                        {
                            bitmap.SetPixel((int)(j % width), (int)(j / width), a, r, g, b);
                        }
                        else if (channel == RasterChannel.A)
                        {
                            bitmap.SetPixel((int)(j % width), (int)(j / width), 255, a, a, a);
                        }
                        else if (channel == RasterChannel.R)
                        {
                            bitmap.SetPixel((int)(j % width), (int)(j / width), 255, r, 0, 0);
                        }
                        else if (channel == RasterChannel.G)
                        {
                            bitmap.SetPixel((int)(j % width), (int)(j / width), 255, 0, g, 0);
                        }
                        else if (channel == RasterChannel.B)
                        {
                            bitmap.SetPixel((int)(j % width), (int)(j / width), 255, 0, 0, b);
                        }
                        else if (channel == RasterChannel.FacadeColor)
                        {
                            if (g >= 0 && g < 64)
                                bitmap.SetPixel((int)(j % width), (int)(j / width), a, 255, 0, 0);
                            if (g >= 64 && g < 192)
                                bitmap.SetPixel((int)(j % width), (int)(j / width), a, 0, 255, 0);
                            if (g >= 192)
                                bitmap.SetPixel((int)(j % width), (int)(j / width), a, 0, 0, 255);
                            //bitmap.SetPixel((int)(j % width), (int)(j / width), 255, 0, 0, b);
                        }

                    }

                    image.MipMaps[i] = bitmap;

                    width = Math.Max(width / 2, 1);
                    height = Math.Max(height / 2, 1);

                }
            }
            return image;
        }

        public static RasterImage CreateFromBitmap(WriteableBitmap bitmap)
        {
            RasterImage image = new RasterImage();
            image.RasterType = 2;
            image.Width = (uint)bitmap.PixelWidth;
            image.Height = (uint)bitmap.PixelHeight;
            image.MipMapCount = 1;
           // image.MipMapCount = (uint)Math.Ceiling(Math.Log((Math.Max(bitmap.PixelWidth, bitmap.PixelHeight)), 2)) + 1;
            image.PixelSize = 8;
            image.RasterPixelFormat = 21;

            uint originalWidth = image.Width;
            uint originalHeight = image.Height;

            int width = (int)image.Width;
            int height = (int)image.Height;

            image.MipMaps = new WriteableBitmap[image.MipMapCount];

            SimCityPak.ExtensionMethods.PixelColor[,] pixels = bitmap.GetPixels();

            using (MemoryStream byteStream = new MemoryStream())
            {
                for (int i = 0; i < image.MipMapCount; i++)
                {
                    int blockSize = bitmap.PixelHeight * bitmap.PixelWidth * 4;
                    byteStream.WriteU32BE((uint)blockSize);

                    for (int j = 0; j < (blockSize / 4); j++)
                    {
                        byteStream.WriteU8(pixels[(int)(j % bitmap.PixelWidth), (int)(j / bitmap.PixelWidth)].Alpha);
                        byteStream.WriteU8(pixels[(int)(j % bitmap.PixelWidth), (int)(j / bitmap.PixelWidth)].Red);
                        byteStream.WriteU8(pixels[(int)(j % bitmap.PixelWidth), (int)(j / bitmap.PixelWidth)].Green);
                        byteStream.WriteU8(pixels[(int)(j % bitmap.PixelWidth), (int)(j / bitmap.PixelWidth)].Blue);
                    }

                    WriteableBitmap resized = resize_image(bitmap, 0.5);

                    pixels = resized.GetPixels();

                    bitmap = resized;
                }
                image.RasterData = byteStream.ToArray();
            }

            

            return image;
        }

        public byte[] ToIndexData()
        {
                int imageByteSize = (6 * 4);
                imageByteSize += RasterData.Length;
             
                byte[] rasterData = new byte[imageByteSize];
              using (MemoryStream byteStream = new MemoryStream(rasterData))
                {
                    byteStream.WriteU32BE(RasterType);
                    byteStream.WriteU32BE(Width);
                    byteStream.WriteU32BE(Height);
                    byteStream.WriteU32BE(MipMapCount);
                    byteStream.WriteU32BE(PixelSize);
                    byteStream.WriteU32BE(RasterPixelFormat);

                    byteStream.Write(RasterData, 0, RasterData.Length);
                }

              return rasterData;
        }





        static WriteableBitmap resize_image(WriteableBitmap img, double scale)
        {
            var height = img.PixelHeight;
            var width = img.PixelWidth;
            var pixelBytes = new byte[height * width * 4];
            img.CopyPixels(pixelBytes, img.PixelWidth * 4, 0);

            for (int i = 0; i < pixelBytes.Length; i++)
            {
                if (pixelBytes[i] < 126)
                {
                   // if (pixelBytes[i] > 5)
                   // {
                        pixelBytes[i] = (byte)(128 - (128 - pixelBytes[i]) / 2);
                   // }
                }
                else
                {
                    pixelBytes[i] = (byte)(130 - (128 - pixelBytes[i]) / 2);
                }
            }

            img.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), pixelBytes, width * 4, 0);

            BitmapSource source = img;
  
            var s = new ScaleTransform(scale, scale);

            var res = new TransformedBitmap(img, s);

            WriteableBitmap bitmap = convert_BitmapSource_to_WriteableBitmap(res);

          
            

            
            return bitmap;
        }

        static WriteableBitmap convert_BitmapSource_to_WriteableBitmap(BitmapSource source)
        {
            // Calculate stride of source
            int stride = source.PixelWidth * (source.Format.BitsPerPixel / 8);

            // Create data array to hold source pixel data
            byte[] data = new byte[stride * source.PixelHeight];

            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);

            // Create WriteableBitmap to copy the pixel data to.      
            WriteableBitmap target = new WriteableBitmap(source.PixelWidth
                , source.PixelHeight, source.DpiX, source.DpiY
                , source.Format, null);

            // Write the pixel data to the WriteableBitmap.
            target.WritePixels(new System.Windows.Int32Rect(0, 0
                , source.PixelWidth, source.PixelHeight)
                , data, stride, 0);

            return target;
        }
    }

}
