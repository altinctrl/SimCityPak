using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gibbed.Spore.Package;
using SimCityPak.Views;
using SimCityPak.PackageReader;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewText.xaml
    /// </summary>
    public partial class ViewText : UserControl
    {
        private ViewTextFind textFind;
        private bool isDirty;

        public ViewText()
        {
            InitializeComponent();
            this.textFind = new ViewTextFind(this.textBoxRawData);
            this.isDirty = false;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null)
            {
                this.isDirty = false;
                if (this.DataContext is DatabaseIndexData)
                {
                    DatabaseIndexData index = this.DataContext as DatabaseIndexData;
                    textBoxRawData.Text = ASCIIEncoding.UTF8.GetString(index.Data);
                }
                else if (this.DataContext is byte[])
                {
                    byte[] data = this.DataContext as byte[];
                    textBoxRawData.Text = ASCIIEncoding.UTF8.GetString(data);
                }
                textBoxRawData.ScrollToHome();
                txtLineNumber.Text = "0";
                this.textFind.Reset(); //New content is loaded so reset search to start position
            }
        }

        private void textBoxRawData_SelectionChanged(object sender, RoutedEventArgs e)
        {
            txtLineNumber.Text = textBoxRawData.GetLineIndexFromCharacterIndex(textBoxRawData.CaretIndex).ToString();
        }

        private void txtLineNumber_Changed(object sender, TextChangedEventArgs e)
        {
            if (txtLineNumber.IsFocused == false) return;
            try
            {
                int line = System.Convert.ToInt32(txtLineNumber.Text);
                textBoxRawData.ScrollToLine(line);
            }
            catch { }
        }

        private void OnMnFNVClick(object sender, RoutedEventArgs e)
        {
            ViewFNV vfnv = new ViewFNV();
            vfnv.textBox1.Text = textBoxRawData.SelectedText;
            vfnv.Show();
        }

        private void textBoxRawData_TextChanged(object sender, TextChangedEventArgs e)
        {
            DatabaseIndexData index = this.DataContext as DatabaseIndexData;
            if (index != null && this.isDirty)
            {
                //if the text has been changed, create a new modifiedIndex
                DatabaseIndex originalIndex = DatabaseManager.Instance.Find(i => i.TypeId == index.Index.TypeId &&
                                                             i.GroupContainer == index.Index.GroupContainer &&
                                                             i.InstanceId == index.Index.InstanceId);
                originalIndex.IsModified = true;
                ModifiedTextFile textFile = new ModifiedTextFile();
                textFile.Text = textBoxRawData.Text;
                originalIndex.ModifiedData = textFile;
            }
            this.isDirty = true;
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            this.textFind.Show(); //Opens find window
            this.textFind.Focus(); //And force focus in case window already exists
        }
    }
}
