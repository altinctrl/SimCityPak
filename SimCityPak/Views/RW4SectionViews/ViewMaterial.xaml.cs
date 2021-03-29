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
using SporeMaster.RenderWare4;
using Gibbed.Spore.Package;
using SimCityPak.PackageReader;
using SimCityPak.Views.AdvancedEditors;
using System.IO;
using Microsoft.Win32;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewMaterial.xaml
    /// </summary>
    public partial class ViewMaterial : UserControl
    {
        public ViewMaterial()
        {
            InitializeComponent();
        }

        RW4Section section = null;

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(RW4Section))
            {

                section = this.DataContext as RW4Section;
                RW4Material vertexArraySection = section.obj as RW4Material;
                if (vertexArraySection != null)
                {
                    dataGridVertices.ItemsSource = vertexArraySection.Materials;

                    foreach (MaterialTextureReference mat in vertexArraySection.Materials)
                    {
                        DatabaseIndex imageIndex = DatabaseManager.Instance.Indices.Find(idx => idx.InstanceId == mat.TextureInstanceId && idx.TypeId == PropertyConstants.RasterImageType);
                        if (imageIndex != null)
                        {
                            using (MemoryStream imageByteStream = new MemoryStream(imageIndex.GetIndexData(true)))
                            {

                                RasterImage img = RasterImage.CreateFromStream(imageByteStream, RasterChannel.All);
                                panelTextures.Children.Add(new Image() { Source = img.MipMaps[0], Stretch = Stretch.None });

                            }
                        }


                    }
                }

            }
        }

        private void dataGridVertices_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //if (section != null)
            //{
            //    section.Changed();
            //}
        }

        private void btnExport3dsMax_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(RW4Section))
            {
                section = this.DataContext as RW4Section;
                RW4Material vertexArraySection = section.obj as RW4Material;

                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                dlg.Description = "Select where to extract the files to..."; 
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedPath = dlg.SelectedPath;
                    foreach (MaterialTextureReference mat in vertexArraySection.Materials)
                    {
                        DatabaseIndex imageIndex = DatabaseManager.Instance.Indices.Find(idx => idx.InstanceId == mat.TextureInstanceId && idx.TypeId == PropertyConstants.RasterImageType);
                        if (imageIndex != null)
                        {
                            using (MemoryStream imageByteStream = new MemoryStream(imageIndex.GetIndexData(true)))
                            {
                                using (FileStream strm = File.Create(selectedPath + "\\" + imageIndex.InstanceId.ToHex() + ".png"))
                                {
                                    RasterImage img;
                                    if (mat.Unknown1 == 1)
                                    {
                                        img = RasterImage.CreateFromStream(imageByteStream, RasterChannel.FacadeColor);
                                    }
                                    else
                                    {
                                        img = RasterImage.CreateFromStream(imageByteStream, RasterChannel.All);
                                    }
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    encoder.Frames.Add(BitmapFrame.Create(img.MipMaps[0] as BitmapSource));
                                    encoder.Save(strm);
                                }
                            }
                        }

                        imageIndex = DatabaseManager.Instance.Indices.Find(idx => idx.InstanceId == mat.TextureInstanceId && idx.TypeId == PropertyConstants.RW4ImageType);
                        if (imageIndex != null)
                        {
                            using (MemoryStream imageByteStream = new MemoryStream(imageIndex.GetIndexData(true)))
                            {

                                RW4Model model = new RW4Model();
                                model.Read(imageByteStream);
                                Texture text = model.Textures[0];
                                WriteableBitmap bitmap = text.ToImage(true);
                                using (FileStream strm = File.Create(selectedPath + "\\" + imageIndex.InstanceId.ToHex() + ".png"))
                                {

                                    //panelTextures.Children.Add(new Image() { Source = img.MipMaps[0], Stretch = Stretch.None });
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    encoder.Frames.Add(BitmapFrame.Create(bitmap as BitmapSource));
                                    encoder.Save(strm);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (section != null)
            {
                section.Changed();
            }
        }
    }
}
