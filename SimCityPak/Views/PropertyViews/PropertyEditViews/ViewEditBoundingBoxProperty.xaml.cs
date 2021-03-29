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
    /// Interaction logic for ViewEditVector3Property.xaml
    /// </summary>
    public partial class ViewEditBoundingBoxProperty : Window
    {
        public BoundingBoxProperty EditProperty { get; set; }

        public ViewEditBoundingBoxProperty(BoundingBoxProperty prop)
        {
            InitializeComponent();
            EditProperty = prop;

            txtMinX.Text = EditProperty.MinX.ToString();
            txtMinY.Text = EditProperty.MinY.ToString();
            txtMinZ.Text = EditProperty.MinZ.ToString();
            txtMaxX.Text = EditProperty.MaxX.ToString();
            txtMaxY.Text = EditProperty.MaxY.ToString();
            txtMaxZ.Text = EditProperty.MaxZ.ToString();

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (EditProperty != null)
            {
                EditProperty.MinX = float.Parse(txtMinX.Text);
                EditProperty.MinY = float.Parse(txtMinY.Text);
                EditProperty.MinZ = float.Parse(txtMinZ.Text);
                EditProperty.MaxX = float.Parse(txtMaxX.Text);
                EditProperty.MaxY = float.Parse(txtMaxY.Text);
                EditProperty.MaxZ = float.Parse(txtMaxZ.Text);
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
