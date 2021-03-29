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
using System.Windows.Media.Media3D;
using Gibbed.Spore.Properties;

namespace SimCityPak.Views.PropertyViews.PropertyEditViews
{
    /// <summary>
    /// Interaction logic for ViewEditTransformProperty.xaml
    /// </summary>
    public partial class ViewEditTransformProperty : Window
    {
        //public Matrix3D matrix;
        //public uint Unkown;
        //public uint Flags;

        public TransformProperty EditProperty;

        public ViewEditTransformProperty(TransformProperty prop)
        {
            InitializeComponent();
            EditProperty = prop;

            txtM11.Text = EditProperty.Matrix[0].ToString();
            txtM12.Text = EditProperty.Matrix[1].ToString();
            txtM13.Text = EditProperty.Matrix[2].ToString();
            txtM21.Text = EditProperty.Matrix[3].ToString();
            txtM22.Text = EditProperty.Matrix[4].ToString();
            txtM23.Text = EditProperty.Matrix[5].ToString();
            txtM31.Text = EditProperty.Matrix[6].ToString();
            txtM32.Text = EditProperty.Matrix[7].ToString();
            txtM33.Text = EditProperty.Matrix[8].ToString();
            txtM41.Text = EditProperty.Matrix[9].ToString();
            txtM42.Text = EditProperty.Matrix[10].ToString();
            txtM43.Text = EditProperty.Matrix[11].ToString();

            txtFlags.Text = EditProperty.Flags.ToString();
            txtUnkown.Text = EditProperty.Unknown.ToString();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (EditProperty != null)
            {
                if (txtUnkown.Text.StartsWith("0x"))
                {
                    EditProperty.Unknown = Convert.ToUInt32(txtUnkown.Text.Substring(2), 16);
                }

                EditProperty.Matrix[0] = float.Parse(txtM11.Text);
                EditProperty.Matrix[1] = float.Parse(txtM12.Text);
                EditProperty.Matrix[2] = float.Parse(txtM13.Text);
                EditProperty.Matrix[3] = float.Parse(txtM21.Text);
                EditProperty.Matrix[4] = float.Parse(txtM22.Text);
                EditProperty.Matrix[5] = float.Parse(txtM23.Text);
                EditProperty.Matrix[6] = float.Parse(txtM31.Text);
                EditProperty.Matrix[7] = float.Parse(txtM32.Text);
                EditProperty.Matrix[8] = float.Parse(txtM33.Text);
                EditProperty.Matrix[9] = float.Parse(txtM41.Text);
                EditProperty.Matrix[10] = float.Parse(txtM42.Text);
                EditProperty.Matrix[11] = float.Parse(txtM43.Text);

                EditProperty.Flags = ushort.Parse(txtFlags.Text);

            }

            DialogResult = true;
            Close();
        }
    }
}
