using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Gibbed.Spore.Package;
using Gibbed.Spore.Properties;
using SimCityPak.PackageReader;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewResult.xaml
    /// </summary>
    public partial class ViewSearchProperties : Window
    {
        private MainWindow main;
        public MainWindow Main
        {
            get { return main; }
            set { main = value; }
        }
        ICollectionView view;
        public ViewSearchProperties(MainWindow main)
        {
            InitializeComponent();
            this.Main = main;
            InitializeBackgroundWorker();
        }
        public ViewSearchProperties(MainWindow main, String searchTerm, String searchProperty, bool startImmediately)
            : this(main)
        {
            txtSearchValue.Text = searchTerm;
            txtPropertyID.Text = searchProperty;
            if (startImmediately)
            {
                startSearch();
            }
        }
        BackgroundWorker backgroundWorker;
        private void InitializeBackgroundWorker()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
        }

        void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgSearchProgress.Value = e.ProgressPercentage;
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                view = CollectionViewSource.GetDefaultView(e.Result);
                dataGridInstances.ItemsSource = view;
                prgSearchProgress.Value = 100;
            }
            else
            {
                prgSearchProgress.Value = 0;
            }
            btnCancel.IsEnabled = false;
            btnSearch.IsEnabled = true;
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            System.Collections.Concurrent.ConcurrentBag<DatabaseIndex> foundItems = new System.Collections.Concurrent.ConcurrentBag<DatabaseIndex>();
            //List<DatabaseIndex> foundItems = new List<DatabaseIndex>();
            int completed = 0;
            IEnumerable<DatabaseIndex> propitems = DatabaseManager.Instance.Where(item => item.TypeId == 11645188);
            int count = propitems.Count();
            Object[] args = e.Argument as Object[];
            String searchText = args[0] as String;
            String propertyIDText = args[1] as String;
            uint propertyID = 0;
            if (!String.IsNullOrEmpty(propertyIDText)) 
            {
                if (propertyIDText.StartsWith("0x")) propertyIDText = propertyIDText.Remove(0,2);
                propertyID = UInt32.Parse(propertyIDText, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            if (searchText == null) {
                searchText = "";
            }
            int propType = (int)args[2];
            //foreach (DatabaseIndex item in propitems)
            Func<KeyValuePair<uint, Property>, bool> searchExpression = null;
            if (searchText == "")
            {
                searchExpression = pair => pair.Key == propertyID;
            }
            else
            {
                switch (propType)
                {
                    case 0://key
                        searchExpression = pair => (propertyID == 0 || pair.Key == propertyID) &&
                                    ((pair.Value is KeyProperty && ((KeyProperty)pair.Value).InstanceId.ToHex().Contains(searchText)) ||
                                    (pair.Value is ArrayProperty && ((ArrayProperty)pair.Value).Values.Count(val => val is KeyProperty && ((KeyProperty)val).InstanceId.ToHex().Contains(searchText)) != 0));
                        break;
                    case 1://number
                        searchExpression = pair => (propertyID == 0 || pair.Key == propertyID) &&
                                    ((pair.Value is Int32Property && ((Int32Property)pair.Value).Value.ToString() == searchText) ||
                                    (pair.Value is UInt32Property && ((UInt32Property)pair.Value).Value.ToString() == searchText) ||
                                    (pair.Value is FloatProperty && ((FloatProperty)pair.Value).Value.ToString() == searchText) ||
                                    (pair.Value is ArrayProperty && ((ArrayProperty)pair.Value).Values.Count(val =>
                                        (val is Int32Property && ((Int32Property)val).Value.ToString() == searchText) ||
                                        (val is UInt32Property && ((UInt32Property)val).Value.ToString() == searchText) ||
                                        (val is FloatProperty && ((FloatProperty)val).Value.ToString() == searchText)) != 0));
                        break;
                    case 2://bool
                        searchExpression = pair => (propertyID == 0 || pair.Key == propertyID) && (pair.Value is BoolProperty && ((BoolProperty)pair.Value).Value.ToString().Equals(searchText, StringComparison.CurrentCultureIgnoreCase));
                        break;
                }
            }
            Parallel.ForEach(propitems, (item, loopstate) =>
            {

                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    loopstate.Stop();
                    //return;
                }
                byte[] data = item.GetIndexData(true);
                using (MemoryStream byteStream = new MemoryStream(data))
                {
                    PropertyFile file = new PropertyFile();
                    file.Read(byteStream);
                    int pcount = 0;
                    pcount = file.Values.Count(searchExpression); ;
                    if (pcount > 0)
                        foundItems.Add(item);
                }
                worker.ReportProgress((int)(((float)completed++ / (float)count) * 100));
            });
            e.Result = foundItems;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) 
        {
            startSearch();
        }
        public void startSearch()
        {
            txtSearchValue.Text = txtSearchValue.Text.ToLower();
            dataGridInstances.SelectedItem = null;
            dataGridInstances.ItemsSource = null;
            prgSearchProgress.Value = 0;
            backgroundWorker.RunWorkerAsync(new object[]{txtSearchValue.Text, txtPropertyID.Text, cmbPropertyType.SelectedIndex});
            btnSearch.IsEnabled = false;
            btnCancel.IsEnabled = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (backgroundWorker.IsBusy)
            {
                backgroundWorker.CancelAsync();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            backgroundWorker.CancelAsync();
        }

        private void dataGridInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DatabaseIndex item = dataGridInstances.SelectedItem as DatabaseIndex;
            if (item == null)
            {
                return;
            }
            if (chkViewInNewWindow.IsChecked == true)
            {
                ViewWindow window = new ViewWindow(this.Main, item);
                window.Show();
            }
            else
            {
                Main.dataGridInstances.SelectedItem = item;
                Main.dataGridInstances.ScrollIntoView(item);
            }
        }


    }
}
