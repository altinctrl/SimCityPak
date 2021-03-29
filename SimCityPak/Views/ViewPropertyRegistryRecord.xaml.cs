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
using System.Globalization;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewPropertyRegistryRecord.xaml
    /// </summary>
    public partial class ViewPropertyRegistryRecord : Window
    {
        public ViewPropertyRegistryRecord()
        {
            InitializeComponent();
        }

        TGIRecord _record;

        public ViewPropertyRegistryRecord(TGIRecord record)
        {
            InitializeComponent();

            _record = record;

            txtId.Text = _record.HexId;
            txtDisplayName.Text = _record.DisplayName;
            txtComment.Text = _record.Comments;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            IFormatProvider format =  CultureInfo.CurrentCulture;

            uint id;
            bool isValid = true;
            if (txtId.Text.ToLower().StartsWith("0x"))
            {
                isValid &= UInt32.TryParse(txtId.Text.Trim().Substring(2), NumberStyles.AllowHexSpecifier, format, out id);
            }
            else
            {
                isValid &= UInt32.TryParse(txtId.Text.Trim(), NumberStyles.Number, format, out id);
            }

            //deals with the mass import - not needed to be used, usually hidden, but don't delete
            if (!string.IsNullOrEmpty(txtImport.Text))
            {
                foreach (string line in txtImport.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] items = line.Split(new string[] { "  " }, StringSplitOptions.RemoveEmptyEntries);
                    string propertyName = items[0].Trim().Substring("property ".Length);
                    string keyId = items[1].Trim().Substring(0, 10);
                    string description = string.Empty;
                    if (items.Length > 2)
                    {
                        description = items[2].Trim();
                    }

                    TGIRecord record = new TGIRecord();
                    record.Id = UInt32.Parse(keyId.Trim().Substring(2), NumberStyles.AllowHexSpecifier, format);
                    record.DisplayName = propertyName.Trim();
                    record.Comments = description;

                    TGIRegistry.Instance.Instances.InsertRecord(record);
                }
            }            

            if (isValid)
            {
                _record.Id = id;
                _record.DisplayName = txtDisplayName.Text.TrimEnd();
                _record.Comments = txtComment.Text.TrimEnd();

                //if (!TGIRegistry.Instance.Instances.Cache.ContainsKey(id))
                //{
                    TGIRegistry.Instance.Properties.InsertRecord(_record);
                //}
             

                this.Close();
            }
        }
    }
}
