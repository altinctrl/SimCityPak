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
using System.IO;
using Gibbed.Spore.Helpers;
using Microsoft.Win32;
using Gibbed.Spore.Package;
using SporeMaster.RenderWare4;
using SimCityPak.Views.AdvancedEditors.DecalDictionary;



namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewRaster.xaml
    /// </summary>
    public partial class ViewRaster : UserControl
    {
        public ViewRaster()
        {
            InitializeComponent();
        }

        public RasterImage RasterImage { get; set; }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                LoadImage();
            }
        }

        private void LoadImage()
        {
            try
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                if (index != null)
                {
                    using (MemoryStream byteStream = new MemoryStream(index.Data))
                    {
                        RasterChannel channel = RasterChannel.Preview;
                        if (rAll.IsChecked.GetValueOrDefault(false)) channel = RasterChannel.All;
                        if (rCombined.IsChecked.GetValueOrDefault(false)) channel = RasterChannel.Preview;
                        if (rAlpha.IsChecked.GetValueOrDefault(false)) channel = RasterChannel.A;
                        if (rRed.IsChecked.GetValueOrDefault(false)) channel = RasterChannel.R;
                        if (rGreen.IsChecked.GetValueOrDefault(false)) channel = RasterChannel.G;
                        if (rBlue.IsChecked.GetValueOrDefault(false)) channel = RasterChannel.B;
                        if (rFacadeColor.IsChecked.GetValueOrDefault(false)) channel = RasterChannel.FacadeColor;

                        RasterImage = RasterImage.CreateFromStream(byteStream, channel);

                            imagePreview.Source = RasterImage.MipMaps[0];
                            imagePreview.Width = RasterImage.MipMaps[0].PixelWidth;
                            imagePreview.Height = RasterImage.MipMaps[0].PixelHeight;
                            imagePreview.Stretch = Stretch.None;                       
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void buttonExport_Click(object sender, RoutedEventArgs e)
        {
          
        }

        private void buttonImport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rAlpha_Checked(object sender, RoutedEventArgs e)
        {
            LoadImage();
        }

        private void mnuImportDDS_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = "dds";
            dlg.Filter = "DDS|*.dds";
            //needs to be an 8.8.8.8 ARGB texture
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;

                using (Stream strm = File.OpenRead(dlg.FileName))
                {
                    strm.expect(0x20534444, "DDS000");  // 'DDS '
                    int headerSize = strm.ReadS32();
                    if (headerSize < 0x7C) throw new ModelFormatException(strm, "DDS001", headerSize);
                    var flags = strm.ReadU32();
                    var height = strm.ReadS32();
                    var width = strm.ReadS32();
                    var pitchOrLinearSize = strm.ReadU32();
                    strm.expect(0, "DDS002");  // < depth
                    var mipmaps = strm.ReadS32();

                    strm.Seek(strm.Position + 11 * 4, SeekOrigin.Begin);
                    var pfsize = strm.ReadS32();
                    if (pfsize < 32) throw new ModelFormatException(strm, "DDS011", pfsize);
                    var pf_flags = strm.ReadU32();
                    var fourcc = strm.ReadU32();

                    strm.Seek(headerSize + 4, SeekOrigin.Begin);
                    var sizes = (from i in Enumerable.Range(0, mipmaps)
                                 select (Math.Max(width >> i, 1) * Math.Max(height >> i, 1) * 4)  // DXT5: 16 bytes per 4x4=16 pixels
                                    ).ToArray();
                    var all_mipmaps = new byte[sizes.Sum()];
                    for (int offset = 0, i = 0; i < mipmaps; i++)
                    {
                        if (strm.Read(all_mipmaps, offset, sizes[i]) != sizes[i])
                            throw new ModelFormatException(strm, "Unexpected EOF reading .DDS file", null);
                        offset += sizes[i];
                    }

                    byte[] rasterData = new byte[all_mipmaps.Length + (4 * 6) + (sizes.Length * 4)];


                    using (MemoryStream byteStream = new MemoryStream(rasterData))
                    {

                        byteStream.WriteU32BE(RasterImage.RasterType);
                        byteStream.WriteU32BE((uint)width);
                        byteStream.WriteU32BE((uint)height);
                        byteStream.WriteU32BE((uint)sizes.Length);
                        byteStream.WriteU32BE((uint)8);
                        byteStream.WriteU32BE((uint)RasterImage.RasterPixelFormat);


                        int position = 0;
                        foreach (int mipMapSize in sizes.OrderByDescending(m => m))
                        {
                            //int blockSize = height * width * 4;
                            byteStream.WriteU32BE((uint)mipMapSize);



                            for (int j = 0; j < mipMapSize; j++)
                            {
                                byteStream.WriteU8(all_mipmaps[position]);
                                position++;
                            }

                        }
                    }

                    index.Index.ModifiedData = new ModifiedRasterFile() { ImageFileData = rasterData };
                    index.Index.IsModified = true;


                    LoadImage();

                }
            }
        }

        private void mnuExportPNG_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = "png";
            dlg.Filter = "PNG|*.png";
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                if(RasterImage != null)
                {
                    using (FileStream filestream = new FileStream(dlg.FileName, FileMode.Create))
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(RasterImage.MipMaps[0]));
                        encoder.Save(filestream);
                    }
                }
            }
        }

        private void mnuGenerateSDF_Click(object sender, RoutedEventArgs e)
        {
           // ViewDecalCreator decalCreator = new ViewDecalCreator();
          //  decalCreator.ShowDialog();
        }

    }
}
