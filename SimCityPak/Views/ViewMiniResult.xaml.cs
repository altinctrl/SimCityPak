using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Gibbed.Spore.Package;
using Gibbed.Spore.Properties;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewResult.xaml
    /// </summary>
    public partial class ViewMiniResult : Window
    {
        public MainWindow Main { get; set; }
        public ViewMiniResult(MainWindow main)
        {
            InitializeComponent();
            this.Main = main;
            this.Topmost = true;
        }
        public ViewMiniResult(MainWindow main, List<DatabaseIndex> indexes) : this(main)
        {
            dataGrid.ItemsSource = indexes;
        }

        private void dataGridInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                if (Main != null)
                {
                    ViewWindow window = new ViewWindow(Main, (DatabaseIndex)dataGrid.SelectedItem);
                    window.Show();
                    this.Close();
                }
            }
        }
    }
}
