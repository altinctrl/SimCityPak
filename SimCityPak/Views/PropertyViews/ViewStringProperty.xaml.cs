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
    /// Interaction logic for ViewStringProperty.xaml
    /// </summary>
    public partial class ViewStringProperty : UserControl
    {
        public ViewStringProperty()
        {
            InitializeComponent();
        }

        private void txtString_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.DataContext is String8Property)
            {
                String8Property prop = this.DataContext as String8Property;
                prop.Value = txtString.Text;
            }
            if (this.DataContext is String16Property)
            {
                String16Property prop = this.DataContext as String16Property;
                prop.Value = txtString.Text;
            }
        }
    }
}
