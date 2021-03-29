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
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using Gibbed.Spore.Package;

namespace SimCityPak.Views.AdvancedEditors.DecalDictionary
{
    /// <summary>
    /// Interaction logic for ViewDecalCreator.xaml
    /// </summary>
    public partial class ViewDecalCreator : Window
    {
        string testFileName;
        string originalFile;

        public RasterImage RasterResult;
        public uint InstanceId;
        private DatabaseIndex _index;
        public DecalImageModel Decal { get; set; }

        public ViewDecalCreator(DatabaseIndex index)
        {
            _index = index;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
 
        }

        private WriteableBitmap layer1Bitmap;

        private void btnBrowseLayer1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "Png|*.png";

            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                originalFile = dlg.FileName;
                layer1Bitmap = new WriteableBitmap(new BitmapImage(new Uri(dlg.FileName)));

                imgLayer1.Source = layer1Bitmap;

                GenerateRasterImage();
            }
        }

        public void GenerateRasterImage()
        {
            if (!string.IsNullOrEmpty(originalFile))
            {
                try
                {
                    nQuant.WuQuantizer quantizer = new nQuant.WuQuantizer();
                    System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(originalFile);

                    quantizer.QuantizeImage(bitmap);
                    rectangle1.Background = new SolidColorBrush(Color.FromRgb(quantizer.Palette.Colors[0].R, quantizer.Palette.Colors[0].G, quantizer.Palette.Colors[0].B ));
                    rectangle2.Background = new SolidColorBrush(Color.FromRgb(quantizer.Palette.Colors[1].R, quantizer.Palette.Colors[1].G, quantizer.Palette.Colors[1].B));
                    rectangle3.Background = new SolidColorBrush(Color.FromRgb(quantizer.Palette.Colors[2].R, quantizer.Palette.Colors[2].G, quantizer.Palette.Colors[2].B));
                    rectangle4.Background = new SolidColorBrush(Color.FromRgb(quantizer.Palette.Colors[3].R, quantizer.Palette.Colors[3].G, quantizer.Palette.Colors[3].B));

                    QuantizedSignedDistanceFieldGenerator generator = new QuantizedSignedDistanceFieldGenerator(quantizer.Palette, bitmap.Height, bitmap.Width );
                    generator.Generate(0);
                    generator.Generate(1);
                    generator.Generate(2);
                    generator.Generate(3);

                    quantizedBitmap = generator.GetBitmap();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error during generation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        WriteableBitmap quantizedBitmap;

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RasterResult = RasterImage.CreateFromBitmap(quantizedBitmap);

                InstanceId = TGIRandomGenerator.GetNext();

                DatabaseIndex newIndex = new DatabaseIndex(_index.Owner);
                newIndex.TypeId = (uint)TypeIds.RasterFile;
                newIndex.InstanceId = InstanceId;
                newIndex.Flags = 1;
                newIndex.IsModified = true;
                newIndex.Compressed = false;
                newIndex.ModifiedData = new ModifiedRasterFile() { ImageFileData = RasterResult.ToIndexData() };
                _index.Owner.Indices.Add(newIndex);
                SimCityPak.PackageReader.DatabaseManager.Instance.Indices.Add(newIndex);

                Decal = new DecalImageModel();
                Decal.IdProperty = TGIRandomGenerator.GetNext().ToHex();
                Decal.DecalIdProperty = InstanceId.ToHex();
                Decal.Color4 = ((SolidColorBrush)rectangle1.Background).Color;
                Decal.Color1 = ((SolidColorBrush)rectangle2.Background).Color;
                Decal.Color2 = ((SolidColorBrush)rectangle3.Background).Color;
                Decal.Color3 = ((SolidColorBrush)rectangle4.Background).Color;
                Decal.AspectRatioProperty = 1;

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error during generation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
