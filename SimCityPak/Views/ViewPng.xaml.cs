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
using SimCityPak.PackageReader;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewPng.xaml
    /// </summary>
    public partial class ViewPng : UserControl
    {
        public ViewPng()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;

                if (index.Index.TypeId == 0x02393756 && index.Data[0] == 0) //cursor or ico file, there is a riff file which contains a list of icons, hence the check for the first byte
                {
                    index.Data[2] = 1; //cursor files are ico files except the 3rd byte is 2, not 1. IconBitmapDecoder requires the 3rd byte to be 1.
                    MemoryStream byteStream = new MemoryStream(index.Data);
                    try
                    {

                        IconBitmapDecoder decoder = new IconBitmapDecoder(byteStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
                        imagePreview.Source = decoder.Frames[0].Thumbnail;
                        txtSize.Text = decoder.Frames.Count.ToString();
                    }
                    catch
                    {
                    }
                }
                else
                {
                    LoadImage(index.Data);
                }
            }
        }

        private void LoadImage(byte[] data)
        {
            MemoryStream byteStream = new MemoryStream(data);
            BitmapImage image = new BitmapImage();
            try
            {
                image.BeginInit();
                image.StreamSource = byteStream;
                image.EndInit();
                imagePreview.Source = image;

                imagePreview.Stretch = Stretch.Fill;

                imagePreview.Height = image.PixelHeight;
                imagePreview.Width = image.PixelWidth;

                txtHeight.Text = image.PixelHeight.ToString();
                txtWidth.Text = image.PixelWidth.ToString();
                txtSize.Text = byteStream.Length.ToString();

                txtPixelFormat.Text = image.Format.ToString();
            }
            catch
            {

            }
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                byte[] data = File.ReadAllBytes(dlg.FileName);

                if (this.DataContext.GetType() == typeof(DatabaseIndexData))
                {
                    DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                    //if the image has been changed, create a new modifiedIndex

                    Window owner = Window.GetWindow(this);
                    MainWindow main = null;
                    if (owner is MainWindow)
                    {
                        main = owner as MainWindow;
                    }
                    else if (owner is ViewWindow)
                    {
                        main = ((ViewWindow)owner).Main;
                    }
                    if (main != null)
                    {
                        DatabaseIndex originalIndex = DatabaseManager.Instance.Find(i => i.TypeId == index.Index.TypeId &&
                                                                     i.GroupContainer == index.Index.GroupContainer &&
                                                                     i.InstanceId == index.Index.InstanceId);
                        originalIndex.IsModified = true;
                        ModifiedPngFile propertyFile = new ModifiedPngFile();
                        propertyFile.ImageFileData = data;           
                        originalIndex.ModifiedData = propertyFile;

                        LoadImage(data);
                    }
                }
            }
        }

 
    }
}
