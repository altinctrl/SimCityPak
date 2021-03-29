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
using System.Reflection;
using System.Globalization;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewAddProperty.xaml
    /// </summary>
    public partial class ViewAddProperty : Window
    {
        public uint PropertyId
        { get; set; }

        public Type PropertyType
        { get; set; }

        public bool IsArray
        { get; set; }

        public ViewAddProperty()
        {
            InitializeComponent();

            cbTypeName.ItemsSource = Assembly.GetAssembly(typeof(Property)).GetTypes().Where(t => t.IsSubclassOf(typeof(Property))).OrderBy(t => t.Name).Where(
                t => t.Name != "ArrayProperty" && t.Name != "ComplexProperty");

            cbPropertyName.ItemsSource = TGIRegistry.Instance.Properties.Cache.Values.OrderBy(p => p.DisplayName);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bool isValid = true;
            if (cbPropertyName.SelectedItem != null)
            {
                TGIRecord registry = (TGIRecord)cbPropertyName.SelectedItem;
                PropertyId = registry.Id;
            }
            else
            {
                IFormatProvider format = CultureInfo.CurrentCulture;
                uint id;
                if (cbPropertyName.Text.ToLower().StartsWith("0x"))
                {
                    isValid &= UInt32.TryParse(cbPropertyName.Text.Trim().Substring(2), NumberStyles.AllowHexSpecifier, format, out id);
                }
                else
                {
                    isValid &= UInt32.TryParse(cbPropertyName.Text.Trim(), NumberStyles.Number, format, out id);
                }
                if (isValid)
                {
                    PropertyId = id;
                }
            }

            if (isValid)
            {
                Type selectedPropertyType = (Type)cbTypeName.SelectedItem;
                PropertyType = selectedPropertyType;

                IsArray = chkIsArray.IsChecked.GetValueOrDefault(false);

                this.DialogResult = true;
                this.Close();
            }

        }
    }
}
