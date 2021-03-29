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
using Gibbed.Spore.Properties;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewBoolean.xaml
    /// </summary>
    public partial class ViewBooleanProperty : UserControl
    {
        public ViewBooleanProperty()
        {
            InitializeComponent();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BoolProperty)
            {
                BoolProperty prop = this.DataContext as BoolProperty;
                prop.Value = checkboxBool.IsChecked.GetValueOrDefault(false);
            }
          
        }


    }
}
