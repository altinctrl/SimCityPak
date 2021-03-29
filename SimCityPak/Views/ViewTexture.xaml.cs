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
using Microsoft.Xna.Framework.Graphics;
using Gibbed.Spore.Helpers;
using System.Windows.Interop;
using SporeMaster.RenderWare4;
using System.IO;
using Microsoft.Win32;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewTexture.xaml
    /// </summary>
    public partial class ViewTexture : UserControl
    {
        public ViewTexture()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(RW4Section))
            {
                RW4Section section = this.DataContext as RW4Section;
                SporeMaster.RenderWare4.Texture textureSection = section.obj as SporeMaster.RenderWare4.Texture;

                if (textureSection.textureType != 21)
                {
                    if (textureSection != null)
                    {
                        using (var stream = new MemoryStream())
                        {
                            DXT5Image img = new DXT5Image();
                            img.Height = textureSection.height;
                            img.Width = textureSection.width;
                            img.TextureType = textureSection.textureType;
                            img.MipMaps = (int)textureSection.mipmapInfo;

                            img.SetData(textureSection.texData.blob);

                            img.Write(stream);

                            if (textureSection.textureType == 0x21)
                            {
                                mnuExportBMP.IsEnabled = true;
                                mnuExportDDS.IsEnabled = false;
                                mnuImportBMP.IsEnabled = true;
                                mnuImportDDS.IsEnabled = false;
                            }
                            else
                            {
                                mnuExportBMP.IsEnabled = false;
                                mnuExportDDS.IsEnabled = true;
                                mnuImportBMP.IsEnabled = false;
                                mnuImportDDS.IsEnabled = true;
                            }


                            GraphicsDeviceService.AddRef(new WindowInteropHelper(Application.Current.MainWindow).Handle);
                            Texture2D texture;

                            try
                            {
                                DDSLib.DDSFromStream(stream, GraphicsDeviceService.Instance.GraphicsDevice, 0, true, out texture);
                                texturePreview.Source = DDSLib.Texture2Image(texture);

                                texturePreview.Visibility = System.Windows.Visibility.Visible;
                                textBlockError.Visibility = System.Windows.Visibility.Hidden;
                            }
                            catch (Exception ex)
                            {
                                texturePreview.Visibility = System.Windows.Visibility.Hidden;
                                textBlockError.Visibility = System.Windows.Visibility.Visible;
                                textBlockError.Text = ex.Message;
                            }
                        }
                    }
                }
                else
                {
                    using (MemoryStream byteStream = new MemoryStream(textureSection.texData.blob, 0, textureSection.texData.blob.Length))
                    {
                        for (int i = 0; i < textureSection.mipmapInfo; i++)
                        {
                            //  uint blockSize = byteStream.ReadU32().Swap();
                            WriteableBitmap bitmap = new WriteableBitmap((int)textureSection.width, (int)textureSection.height, 300, 300, PixelFormats.Pbgra32, BitmapPalettes.Halftone64);
                            if (textureSection.textureType == 21)
                            {
                                for (int j = 0; j < (byteStream.Length / 4); j++)
                                {
                                    byte r = byteStream.ReadU8();
                                    byte g = byteStream.ReadU8();
                                    byte b = byteStream.ReadU8();
                                    byte a = byteStream.ReadU8();


                                    try
                                    {
                                        if ((j / textureSection.width) < textureSection.height)
                                        {
                                            bitmap.SetPixel((int)(j % textureSection.width), (int)(j / textureSection.width), a, r, g, b);
                                        }
                                    }
                                    catch
                                    {

                                    }
                                }

                                texturePreview.Source = bitmap;
                                break;
                            }

                        }
                    }

                }

                if (textureSection != null)
                {
                    txtHeight.Text = textureSection.height.ToString();
                    txtWidth.Text = textureSection.width.ToString();

                    txtTextureType.Text = textureSection.textureType.ToHex();
                    txtMipMapInfo.Text = textureSection.mipmapInfo.ToHex();

                    txtSection.Text = textureSection.texData.section.Number.ToString();
                    txtUnknown.Text = textureSection.unk1.ToHex();
                }

                //  .AddObject(texture.texData, TextureBlob.type_code);

            }
        }

        private void mnuImportDDS_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.DataContext != null && this.DataContext.GetType() == typeof(RW4Section))
                {
                    //get the section
                    RW4Section section = this.DataContext as RW4Section;
                    SporeMaster.RenderWare4.Texture textureSection = section.obj as SporeMaster.RenderWare4.Texture;

                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.Filter = "DirectDraw Surface .DDS|*.dds";
                    if (dlg.ShowDialog().GetValueOrDefault(false))
                    {
                        uint textureBlobNumber = textureSection.texData.section.Number;

                        //Import as DXT5
                        if (textureSection.textureType != 0x15)
                        {
                            DXT5Image img = new DXT5Image();
                            using (Stream strm = File.OpenRead(dlg.FileName))
                            {
                                img.Read(strm);

                                var texture = new SporeMaster.RenderWare4.Texture()
                                {
                                    width = (ushort)img.Width,
                                    height = (ushort)img.Height,
                                    mipmapInfo = (uint)(0x100 * img.MipMaps + 0x08),
                                    textureType = img.TextureType,
                                    texData = new TextureBlob() { blob = img.GetAsByteArray() },
                                    unk1 = 0
                                };

                                texture.texData.section = new RW4Section() { Number = textureBlobNumber };
                                texture.texData.section.obj = new TextureBlob() { blob = texture.texData.blob };

                                section.obj = texture;
                            }
                        }
                    }
                    section.Changed();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An exception occurred: {0}", ex.Message));
            }

        }

        private void mnuImportBMP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.DataContext != null && this.DataContext.GetType() == typeof(RW4Section))
                {
                    //get the section
                    RW4Section section = this.DataContext as RW4Section;
                    SporeMaster.RenderWare4.Texture textureSection = section.obj as SporeMaster.RenderWare4.Texture;

                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.Filter = "BMP|*.bmp";
                    if (dlg.ShowDialog().GetValueOrDefault(false))
                    {
                        uint textureBlobNumber = textureSection.texData.section.Number;
                        {
                            BitmapSource source = new BitmapImage(new Uri(dlg.FileName));
                            SimCityPak.ExtensionMethods.PixelColor[,] colors = source.GetPixels();

                            byte[] colorsData = new byte[((int)source.PixelHeight * (int)source.PixelWidth) * 4];

                            using (MemoryStream writer = new MemoryStream(colorsData, 0, colorsData.Length, true))
                            {
                                foreach (SimCityPak.ExtensionMethods.PixelColor color in colors)
                                {
                                    writer.WriteU8(color.Blue);
                                    writer.WriteU8(color.Green);
                                    writer.WriteU8(color.Red);
                                    writer.WriteU8(color.Alpha);
                                }
                            }

                            SporeMaster.RenderWare4.Texture texture = new SporeMaster.RenderWare4.Texture()
                            {
                                width = (ushort)source.PixelWidth,
                                height = (ushort)source.PixelHeight,
                                mipmapInfo = 0x120,
                                textureType = 0x15,
                                texData = new TextureBlob() { blob = colorsData },
                                unk1 = 0
                            };

                            texture.texData.section = new RW4Section() { Number = textureBlobNumber };
                            texture.texData.section.obj = new TextureBlob() { blob = colorsData };

                            section.obj = texture;
                        }
                    }
                    section.Changed();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An exception occurred: {0}", ex.Message));
            }
        }

        private void mnuExportDDS_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.DataContext != null && this.DataContext.GetType() == typeof(RW4Section))
                {
                    RW4Section section = this.DataContext as RW4Section;
                    SporeMaster.RenderWare4.Texture textureSection = section.obj as SporeMaster.RenderWare4.Texture;

                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "DirectDraw Surface .DDS|*.dds";
                    if (dlg.ShowDialog().GetValueOrDefault(false))
                    {
                        using (Stream stream = File.Create(dlg.FileName))
                        {
                            DXT5Image img = new DXT5Image();
                            img.Height = textureSection.height;
                            img.Width = textureSection.width;
                            img.TextureType = textureSection.textureType;
                            img.MipMaps = (int)textureSection.mipmapInfo;

                            img.SetData(textureSection.texData.blob);

                            img.Write(stream);

                            GraphicsDeviceService.AddRef(new WindowInteropHelper(Application.Current.MainWindow).Handle);
                            Texture2D texture;

                            try
                            {
                                DDSLib.DDSFromStream(stream, GraphicsDeviceService.Instance.GraphicsDevice, 0, true, out texture);
                                texturePreview.Source = DDSLib.Texture2Image(texture);

                                texturePreview.Visibility = System.Windows.Visibility.Visible;
                                textBlockError.Visibility = System.Windows.Visibility.Hidden;
                            }
                            catch (Exception ex)
                            {
                                texturePreview.Visibility = System.Windows.Visibility.Hidden;
                                textBlockError.Visibility = System.Windows.Visibility.Visible;
                                textBlockError.Text = ex.Message;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An exception occurred: {0}", ex.Message));
            }
        }

        private void mnuExportBMP_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (this.DataContext != null && this.DataContext.GetType() == typeof(RW4Section))
                {
                    RW4Section section = this.DataContext as RW4Section;
                    SporeMaster.RenderWare4.Texture textureSection = section.obj as SporeMaster.RenderWare4.Texture;

                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "BMP|*.bmp";
                    if (dlg.ShowDialog().GetValueOrDefault(false))
                    {
                        {
                            WriteableBitmap newBitmap = new WriteableBitmap(textureSection.width, textureSection.height, 96, 96, PixelFormats.Pbgra32, null);

                            int i = 0;
                            int j = 0;
                            using (MemoryStream reader = new MemoryStream(textureSection.texData.blob, 0, textureSection.texData.blob.Length, true))
                            {
                                while (j < textureSection.height)
                                {
                                    byte B = reader.ReadU8();
                                    byte G = reader.ReadU8();
                                    byte R = reader.ReadU8();
                                    byte A = reader.ReadU8();

                                    newBitmap.SetPixel(i, j, Color.FromArgb(A, R, G, B));

                                    i++;
                                    if (i >= textureSection.width)
                                    {
                                        i = 0;
                                        j++;
                                    }
                                }
                            }

                            using (FileStream stream5 = new FileStream(dlg.FileName, FileMode.Create))
                            {
                                BmpBitmapEncoder encoder5 = new BmpBitmapEncoder();
                                encoder5.Frames.Add(BitmapFrame.Create(newBitmap));
                                encoder5.Save(stream5);
                                stream5.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An exception occurred: {0}", ex.Message));
            }
        }

        private void mnuImportDXT8_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = "dds";
            dlg.Filter = "DDS|*.dds";

            //get the section
            RW4Section section = this.DataContext as RW4Section;
            SporeMaster.RenderWare4.Texture textureSection = section.obj as SporeMaster.RenderWare4.Texture;

            //needs to be an 8.8.8.8 ARGB texture
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                uint textureBlobNumber = textureSection.texData.section.Number;

                DXT8Image img = new DXT8Image();
                using (Stream strm = File.OpenRead(dlg.FileName))
                {
                    img.Read(strm);

                    var texture = new SporeMaster.RenderWare4.Texture()
                    {
                        width = (ushort)img.Width,
                        height = (ushort)img.Height,
                        mipmapInfo = (uint)(0x120),
                        textureType = img.TextureType,
                        texData = new TextureBlob() { blob = img.GetAsByteArray() },
                        unk1 = 0x0121afec
                    };

                    texture.texData.section = new RW4Section() { Number = textureBlobNumber };
                    texture.texData.section.obj = new TextureBlob() { blob = texture.texData.blob };

                    section.obj = texture;

                    section.Changed();
                }
            }
        }

        private void mnuExportDDS8_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.DataContext != null && this.DataContext.GetType() == typeof(RW4Section))
                {
                    RW4Section section = this.DataContext as RW4Section;
                    SporeMaster.RenderWare4.Texture textureSection = section.obj as SporeMaster.RenderWare4.Texture;

                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "DirectDraw Surface .DDS|*.dds";
                    if (dlg.ShowDialog().GetValueOrDefault(false))
                    {
                        using (Stream stream = File.Create(dlg.FileName))
                        {
                            DXT8Image img = new DXT8Image();
                            img.Height = textureSection.height;
                            img.Width = textureSection.width;
                            img.TextureType = textureSection.textureType;
                            img.MipMaps = (int)textureSection.mipmapInfo;

                            img.SetData(textureSection.texData.blob);

                            img.Write(stream);
                        }
                    }
                }
            }
            catch
            {

            }
        }

    }

}

