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
    /// Interaction logic for ViewEditKeyProperty.xaml
    /// </summary>
    public partial class ViewEditKeyProperty : Window
    {
        public KeyProperty EditProperty { get; set; }

        public uint TypeId { get; set; }
        public uint GroupContainer { get; set; }
        public uint InstanceId { get; set; }

        public ViewEditKeyProperty(KeyProperty prop)
        {
            InitializeComponent();

            EditProperty = prop;

            txtTypeId.Text = EditProperty.TypeId.ToHex();
            txtGroupContainer.Text = EditProperty.GroupContainer.ToHex();
            txtInstanceId.Text = EditProperty.InstanceId.ToHex();
        }

        public ViewEditKeyProperty(uint typeid, uint GroupContainer, uint instanceid)
        {
            InitializeComponent();

            txtTypeId.Text = typeid.ToHex();
            txtGroupContainer.Text = GroupContainer.ToHex();
            txtInstanceId.Text = instanceid.ToHex();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (EditProperty != null)
            {
                if (txtTypeId.Text.StartsWith("0x"))
                {
                    EditProperty.TypeId = Convert.ToUInt32(txtTypeId.Text.Substring(2), 16);
                }
                if (txtGroupContainer.Text.StartsWith("0x"))
                {
                    EditProperty.GroupContainer = Convert.ToUInt32(txtGroupContainer.Text.Substring(2), 16);
                }
                if (txtInstanceId.Text.StartsWith("0x"))
                {
                    EditProperty.InstanceId = Convert.ToUInt32(txtInstanceId.Text.Substring(2), 16);
                }
            }
            else
            {
                if (txtTypeId.Text.StartsWith("0x"))
                {
                    TypeId = Convert.ToUInt32(txtTypeId.Text.Substring(2), 16);
                }
                if (txtGroupContainer.Text.StartsWith("0x"))
                {
                    GroupContainer = Convert.ToUInt32(txtGroupContainer.Text.Substring(2), 16);
                }
                if (txtInstanceId.Text.StartsWith("0x"))
                {
                    InstanceId = Convert.ToUInt32(txtInstanceId.Text.Substring(2), 16);
                }
            }
            this.DialogResult = true;
            this.Close();
        }

        private void btnPasteKey_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                String key = Clipboard.GetText();
                string[] ids = key.Split('-');
                if (ids.Length == 3)
                {
                    txtTypeId.Text = ids[0];
                    txtGroupContainer.Text = ids[1];
                    txtInstanceId.Text = ids[2];
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            txtInstanceId.Text = TGIRandomGenerator.GetNext().ToHex();
        }
    }
}
