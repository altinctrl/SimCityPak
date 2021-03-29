using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Gibbed.Spore.Package;
using Gibbed.Spore.Properties;
using SimCityPak.PackageReader;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewScanUnusedIDs.xaml
    /// </summary>
    public partial class ViewScanUnusedIDs : Window
    {
        public MainWindow Main { get; set; }

        List<TGIRecord> NotFoundRecords;

        public ViewScanUnusedIDs(MainWindow main)
        {
            InitializeComponent();
            this.Main = main;
            InitializeBackgroundWorker();
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
        }

        void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgSearchProgress.Value = e.ProgressPercentage;
        }


        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                NotFoundRecords = (List<TGIRecord>)e.Result;
                if (chkShowFound.IsChecked == true)
                {
                    List<TGIRecord> ur = TGIRegistry.Instance.Instances.Cache.Values.ToList();
                    ur.RemoveAll(r => NotFoundRecords.Contains(r));
                    dataGrid.ItemsSource = ur;
                    lblCount.Content = ur.Count.ToString();
                }
                else
                {
                    dataGrid.ItemsSource = NotFoundRecords;
                    lblCount.Content = NotFoundRecords.Count.ToString();
                }
                prgSearchProgress.Value = 100;
            }
            else
            {
                prgSearchProgress.Value = 0;
            }
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            List<TGIRecord> foundItems = TGIRegistry.Instance.Instances.Cache.Values.ToList();
            int completed = 0;
            IEnumerable<DatabaseIndex> propitems = DatabaseManager.Instance.Where(item => item.TypeId == 0xb1b104);
            int count = propitems.Count();
            foreach (DatabaseIndex item in propitems)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                try
                {
                    byte[] data = item.GetIndexData(true);
                    using (MemoryStream byteStream = new MemoryStream(data))
                    {
                        PropertyFile file = new PropertyFile();
                        file.Read(byteStream);
                        foreach (KeyProperty prop in file.Values.Values.Where(p => p is KeyProperty))
                        {
                            //if the value is found remove it from the list.
                            foundItems.RemoveAll(r => r.Id == prop.InstanceId);
                        }
                        foreach (ArrayProperty prop in file.Values.Values.Where(p => p is ArrayProperty))
                        {
                            foreach (KeyProperty propi in prop.Values.Where(p => p is KeyProperty))
                            {
                                foundItems.RemoveAll(r => r.Id == propi.InstanceId);
                            }
                        }
                    }
                }
                catch { }
                worker.ReportProgress((int)(((float)completed++ / (float)count) * 100));
            }
            e.Result = foundItems;
        }

        private void dataGridInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem != null && Main != null)
            {
                TGIRecord record = dataGrid.SelectedItem as TGIRecord;
                SimCityPak.Views.ViewSearchProperties res = new SimCityPak.Views.ViewSearchProperties(Main, record.HexId, null, true);
                res.Show();
            }
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (NotFoundRecords != null)
            {
                List<TGIRecord> ur = TGIRegistry.Instance.Instances.Cache.Values.ToList();
                ur.RemoveAll(r => NotFoundRecords.Contains(r));
                dataGrid.ItemsSource = ur;
                lblCount.Content = ur.Count.ToString();
            }
        }

        private void chkShowFound_Unchecked(object sender, RoutedEventArgs e)
        {
            if (NotFoundRecords != null)
            {
                dataGrid.ItemsSource = NotFoundRecords;
                lblCount.Content = NotFoundRecords.Count.ToString();
            }
        }
    }
}
