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
using System.Windows.Interop;
using Gibbed.Spore.Package;
using Gibbed.Spore.Helpers;
using System.IO;
using Microsoft.Win32;
using SimCityPak.PackageReader;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewTga.xaml
    /// </summary>
    public partial class ViewTga : UserControl
    {
        public ViewTga()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                LoadImage(index.Data);
            }
        }

        private void LoadImage(byte[] data)
        {
            try
            {
                TargaImage image = new TargaImage(data);
                imagePreview.Source = Imaging.CreateBitmapSourceFromHBitmap(image.Image.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                txtHeight.Text = image.Image.Height.ToString();
                txtWidth.Text = image.Image.Width.ToString();
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
                    //if the text has been changed, create a new modifiedIndex

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
                        ModifiedTgaFile propertyFile = new ModifiedTgaFile();
                        propertyFile.ImageFileData = data;                    
                        originalIndex.ModifiedData = propertyFile;

                        LoadImage(data);
                    }
                }
            }
        }

 
    }
}
