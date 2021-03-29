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
using Gibbed.Spore.Package;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewInstanceRegistryRecord.xaml
    /// </summary>
    public partial class ViewInstanceRegistryRecord : Window
    {
        public ViewInstanceRegistryRecord()
        {
            InitializeComponent();
        }

        DatabaseIndex _index;

        public ViewInstanceRegistryRecord(DatabaseIndex index)
        {
            InitializeComponent();

            _index = index;
            TGIRecord record;
            TGIRegistry.Instance.Instances.Cache.TryGetValue(index.InstanceId, out record);

            if (record != null)
            {
                txtId.Text = record.HexId;
                txtDisplayName.Text = record.DisplayName;
                txtComment.Text = record.Comments;
                /*
                chkHidden.IsChecked = record.Hidden;
                if (record.Tags != null)
                {
                    txtTags.Text = String.Join(" ", record.Tags);
                }*/
            }
            else
            {
                txtId.Text = index.InstanceName;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            TGIRecord newRecord = new TGIRecord();
            newRecord.Id = _index.InstanceId;
            newRecord.Name = txtDisplayName.Text;
            newRecord.Comments = txtComment.Text;

            TGIRegistry.Instance.Instances.InsertRecord(newRecord);

            //FIXME Hidden should be added..?
            this.Close();
        }
    }
}
