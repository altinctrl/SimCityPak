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
using Gibbed.Spore.Package;
using Gibbed.Spore.Helpers;
using System.IO;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewWindow.xaml
    /// </summary>
    public partial class ViewWindow : Window
    {
        private MainWindow main;
        public MainWindow Main
        {
            get { return main; }
            set { main = value; }
        }
        public ViewWindow(MainWindow main, DatabaseIndex index)
        {
            InitializeComponent();
            this.Main = main;
            byte[] data = index.GetIndexData(true);
            contentHolder.Content = new DatabaseIndexData(index, data);
            this.Title = string.Format("T:{0} G:{1} I:{2} ({3})", index.TypeName, index.GroupName, index.InstanceName, index.InstanceId.ToHex());
            this.WindowState = System.Windows.WindowState.Normal;

            // scale window to a more decent size
            switch (index.TypeName.Substring(0, index.TypeName.IndexOf(' ')))
            {
                case "PNG": this.SizeToContent = System.Windows.SizeToContent.WidthAndHeight; break;
                case "TGA": this.SizeToContent = System.Windows.SizeToContent.WidthAndHeight; break;
                case "RASTER": this.SizeToContent = System.Windows.SizeToContent.WidthAndHeight; break;
                case "RW4": this.SizeToContent = System.Windows.SizeToContent.WidthAndHeight; break;// System.Windows.SizeToContent.Manual; this.Width = 700; this.Height = 500; break;
                case "JS": this.SizeToContent = System.Windows.SizeToContent.Manual; this.Width = 600; this.Height = 500; break;
                default: this.SizeToContent = System.Windows.SizeToContent.Manual; this.Width = 600; this.Height = 350; break;
            }
        }
    }
}
