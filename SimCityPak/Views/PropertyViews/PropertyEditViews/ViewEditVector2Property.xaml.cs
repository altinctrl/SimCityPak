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
using Gibbed.Spore.Properties;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewEditVector2Property.xaml
    /// </summary>
    public partial class ViewEditVector2Property : Window
    {
        public Vector2Property EditProperty { get; set; }

        public float X { get; set; }
        public float Y { get; set; }

        public ViewEditVector2Property(Vector2Property prop)
        {
            InitializeComponent();
            EditProperty = prop;

            txtX.Text = EditProperty.X.ToString();
            txtY.Text = EditProperty.Y.ToString();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (EditProperty != null)
            {
                EditProperty.X = float.Parse(txtX.Text);
                EditProperty.Y = float.Parse(txtY.Text);
            }

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
