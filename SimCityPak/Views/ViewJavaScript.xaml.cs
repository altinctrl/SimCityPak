using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Gibbed.Spore.Package;
using SimCityPak.Views;
using SimCityPak.Views.valueConverters;
using SimCityPak.PackageReader;
using System.IO;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewJavaScript.xaml
    /// </summary>
    public partial class ViewJavaScript : UserControl
    {
        private ViewTextFind textFind;
        private bool isDirty; //To track manual edits to the text

        public ViewJavaScript()
        {
            InitializeComponent();
            this.textFind = new ViewTextFind(this.textBoxJavaScript);
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
                    StringToJS conv = new StringToJS();
                    textBoxJavaScript.Text = (string)conv.Convert(ASCIIEncoding.UTF8.GetString(index.Data), typeof(string), null, CultureInfo.CurrentCulture);
                }
                else if (this.DataContext is byte[])
                {
                    byte[] data = this.DataContext as byte[];
                    StringToJS conv = new StringToJS();
                    textBoxJavaScript.Text = (string)conv.Convert(ASCIIEncoding.UTF8.GetString(data), typeof(string), null, CultureInfo.CurrentCulture);
                }
                textBoxJavaScript.ScrollToHome();
                txtLineNumber.Text = "0";
                this.textFind.Reset(); //New content is loaded so reset search to start position
            }
        }

        private void OnMnFNVClick(object sender, RoutedEventArgs e)
        {
            ViewFNV vfnv = new ViewFNV();
            vfnv.textBox1.Text = textBoxJavaScript.SelectedText;
            vfnv.Show();
        }

        private void txtLineNumber_Changed(object sender, TextChangedEventArgs e)
        {
            if (txtLineNumber.IsFocused == false) return;
            try
            {
                int line = System.Convert.ToInt32(txtLineNumber.Text);
                textBoxJavaScript.ScrollToLine(line);
            }
            catch { }
        }

        private void textBoxJavaScript_SelectionChanged(object sender, RoutedEventArgs e)
        {
            txtLineNumber.Text = textBoxJavaScript.GetLineIndexFromCharacterIndex(textBoxJavaScript.CaretIndex).ToString();  
        }

        private void btnExportPretty_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog();
            saveDialog.ShowDialog();
            if (saveDialog.FileName != null && saveDialog.FileName != "")
            {
                File.WriteAllText(saveDialog.FileName, textBoxJavaScript.Text);
            }
        }

        private void textBoxJavaScript_TextChanged(object sender, TextChangedEventArgs e)
        {
            DatabaseIndexData index = this.DataContext as DatabaseIndexData;
            if (index != null && this.isDirty)
            {
                //if the text has been changed, create a new modifiedIndex
                Window owner = Window.GetWindow(this);
                MainWindow main = null;
                if (owner is MainWindow)
                {
                    main = owner as MainWindow;
                }
                else if (owner is ViewWindow)
                {
                    main = (owner as ViewWindow).Main;
                }
                if (main != null)
                {
                    DatabaseIndex originalIndex = DatabaseManager.Instance.Find(i => i.TypeId == index.Index.TypeId &&
                                                                 i.GroupContainer == index.Index.GroupContainer &&
                                                                 i.InstanceId == index.Index.InstanceId);
                    originalIndex.IsModified = true;
                    ModifiedTextFile textFile = new ModifiedTextFile();
                    textFile.Text = textBoxJavaScript.Text;
                    originalIndex.ModifiedData = textFile;
                }
            }
            this.isDirty = true;
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            this.textFind.Show(); //Opens find window
            this.textFind.Focus(); //And force focus in case window already exists
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.textFind.Close();
        }
    }
}
