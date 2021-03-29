using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gibbed.Spore.Package;
using Gibbed.Spore.Helpers;
using System.IO;
using Microsoft.Win32;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewRaw.xaml
    /// </summary>
    public partial class ViewRawImage : UserControl
    {
        public ViewRawImage()
        {
            InitializeComponent();
        }

        public uint Unknown1 { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public uint DataType { get; set; }
        public uint ImageSize { get; set; }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                MemoryStream byteStream = new MemoryStream(index.Data);

                Unknown1 = byteStream.ReadU32BE();
                Width = byteStream.ReadS32BE();
                Height = byteStream.ReadS32BE();
                DataType = byteStream.ReadU32BE();
                ImageSize = byteStream.ReadU32BE();

                BitmapSource bs = null;
                float[] data = new float[Width * Height];
                float min = float.MaxValue, max = 0f;
                int bits = 0;
                if (DataType == 7)
                {
                    bits = 16;
                    for (int i = 0; i < Width * Height; i++)
                    {
                        data[i] = ((float)byteStream.ReadU16()) / 65536f;
                        min = Math.Min(data[i], min);
                        max = Math.Max(data[i], max);
                    }
                }
                else if (DataType == 2)
                {
                    bits = 32;
                    for (int i = 0; i < Width * Height; i++)
                    {
                        data[i] = (float)(((double)byteStream.ReadU32()) / (double)UInt32.MaxValue);
                        min = Math.Min(data[i], min);
                        max = Math.Max(data[i], max);
                    }
                }
                else if (DataType == 1)
                {
                    bits = 8;
                    for (int i = 0; i < Width * Height; i++)
                    {
                        data[i] = ((float)byteStream.ReadU8()) / 255f;
                        min = Math.Min(data[i], min);
                        max = Math.Max(data[i], max);
                    }
                }

                if (checkBox1.IsChecked == true)
                {
                    //scale the values for better contrast
                    float mult = 1f / (max - min);
                    for (int i = 0; i < Width * Height; i++)
                    {
                        data[i] = (data[i] - min) * mult;
                    }
                }

                bs = BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Gray32Float, null, data, Width * 4);

                try
                {
                    imagePreview.Source = bs;

                    txtHeight.Text = Height.ToString();
                    txtWidth.Text = Width.ToString();
                    txtSize.Text = ImageSize.ToString();

                    txtPixelFormat.Text = bits + "-bit greyscale";
                }
                catch
                {

                }

            }
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            this.UserControl_DataContextChanged(sender, new DependencyPropertyChangedEventArgs());
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            this.UserControl_DataContextChanged(sender, new DependencyPropertyChangedEventArgs());
        }

        private void buttonExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = "bmp";
            dlg.Filter = "BMP|*.bmp";
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                using (FileStream filestream = new FileStream(dlg.FileName, FileMode.Create))
                {
                    BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(imagePreview.Source as BitmapSource));
                    encoder.Save(filestream);
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = "bmp";
            dlg.Filter = "BMP|*.bmp";
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;

                BitmapImage bitmap = new BitmapImage(new Uri(dlg.FileName));

                SimCityPak.ExtensionMethods.PixelColor[,] pixels = bitmap.GetPixels();
                int pixelSize = 1;
                if (DataType == 7)
                {
                    pixelSize = 2;
                }
                if (DataType == 2)
                {
                    pixelSize = 4;
                }


                int imageByteSize = (5 * 4) + (bitmap.PixelHeight * bitmap.PixelWidth * pixelSize);

                byte[] rasterData = new byte[imageByteSize];


                using (MemoryStream byteStream = new MemoryStream(rasterData))
                {
                    byteStream.WriteU32BE(Unknown1);
                    byteStream.WriteS32BE(bitmap.PixelWidth);
                    byteStream.WriteS32BE(bitmap.PixelHeight);
                    byteStream.WriteU32BE(DataType);
                    byteStream.WriteU32BE((uint)(bitmap.PixelHeight * bitmap.PixelWidth * pixelSize));

                    for (int i = 0; i < bitmap.PixelWidth; i++)
                    {
                        for (int j = 0; j < bitmap.PixelHeight; j++)
                        {

                            if (DataType == 7)
                            {
                                byteStream.WriteU16((ushort)(pixels[j, i].Red * 8));
                            }
                            if (DataType == 2)
                            {
                                byteStream.WriteU32((uint)(pixels[j, i].Red * 16));
                            }
                            if (DataType == 1)
                            {
                                byteStream.WriteU8(pixels[j, i].Red);
                            }
                        }
                    }
                }

                index.Index.ModifiedData = new ModifiedRawImage() { ImageFileData = rasterData };
                index.Index.IsModified = true;
            }
        }
    }
}
