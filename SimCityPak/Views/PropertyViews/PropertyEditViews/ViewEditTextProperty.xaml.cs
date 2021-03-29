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
    public partial class ViewEditTextProperty : Window
    {
        public TextProperty EditProperty { get; set; }

        public ViewEditTextProperty(TextProperty prop)
        {
            InitializeComponent();
            EditProperty = prop;

            txtTableId.Text = EditProperty.TableId.ToHex();
            txtInstanceId.Text = EditProperty.InstanceId.ToHex();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (EditProperty != null)
            {
                if (txtTableId.Text.StartsWith("0x"))
                {
                    EditProperty.TableId = Convert.ToUInt32(txtTableId.Text.Substring(2), 16);
                }
                if (txtInstanceId.Text.StartsWith("0x"))
                {
                    EditProperty.InstanceId = Convert.ToUInt32(txtInstanceId.Text.Substring(2), 16);
                }
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
