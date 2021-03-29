using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Threading;
using Gibbed.Spore.Package;
using Gibbed.Spore.Properties;
using SimCityPak.PackageReader;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewFNVNameHashImport.xaml
    /// </summary>
    public partial class ViewFNVNameHashImport : Window
    {
        private DispatcherTimer _statusUpdateTimer; // timer for gui updates
        private List<BackgroundWorker> _workers; // all worker threads
        private List<uint> _cacheProperties; // local cash of all Property keyId's, speeds up things nicely..!
        private List<uint> _cacheInstances; // local cash of all Property keyId's, speeds up things nicely..!
        private List<uint> _cacheGroups; // local cash of all Property keyId's, speeds up things nicely..!
        private bool _cacheReady;
        private Dictionary<uint, string> _keywordHashes; // all loaded keywords and their hashes KEY, HASH

        private string _statusText; // status text for gui
        private string _inFile; // input CSV file
        private int _countTotalKeywords; // number of initially loaded keywords
        private int _countAdded; // number of added entries
        private int _countDupe; // number of dupes
        private int _countNameHashesDone; // number of hashes already scanned
        private bool _interrupted; // to track user interrupts
        private int _currentKeywordHash;

        public ViewFNVNameHashImport()
        {
            InitializeComponent();

            // for gui label updates
            _statusUpdateTimer = new DispatcherTimer();
            _statusUpdateTimer.Tick += new EventHandler(statusUpdateTimer_Tick);
            _statusUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            _statusUpdateTimer.Start();

            _keywordHashes = new Dictionary<uint, string>();
            _cacheProperties = new List<uint>();
            _cacheInstances = new List<uint>();
            _cacheGroups = new List<uint>();
            _cacheReady = false;

            _statusText = "Idle...";
            _inFile = "FNV_keywords_file.csv";

            txtInFile.Text = _inFile;
        }

        private void StopWorkers()
        {
            _interrupted = true;
            for (int i = 0; i < _workers.Count; i++)
            {
                if (_workers[i] != null && _workers[i].IsBusy)
                    _workers[i].CancelAsync();
            }
        }

        private bool IsReady()
        {
            if (_workers == null || _workers.Count == 0)
                return true;

            for (int i = 0; i < _workers.Count; i++)
            {
                if (_workers[i].IsBusy)
                    return false;
            }

            return true;
        }

        public bool IsDirty()
        {
            if (_countAdded > 0)
                return true;

            return false;
        }

        void statusUpdateTimer_Tick(object sender, EventArgs e)
        {
            lblStatus.Content = _statusText;
        }

        void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgSearchProgress.Value = e.ProgressPercentage;
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_interrupted)
            {
                prgSearchProgress.Value = 0;
                txtInFile.IsEnabled = true;
                btnSelectInFile.IsEnabled = true;
                btnImport.Content = "Import";
                txtDescription.Text = "User abort";
                cmbNumberOfThreads.IsEnabled = true;
            }
            else if (IsReady())
            {
                prgSearchProgress.Value = 100;

                _statusText = "Searching done (" + _countTotalKeywords.ToString() + " hashes), added: " + _countAdded + " dupes: " + _countDupe;

                prgSearchProgress.Value = 0;
                txtInFile.IsEnabled = true;
                btnSelectInFile.IsEnabled = true;
                btnImport.Content = "Import";
                cmbNumberOfThreads.IsEnabled = true;
            }
        }

        void CachePropKeys()
        {
            _cacheProperties.Clear();
            _cacheInstances.Clear();
            _cacheGroups.Clear();

            // loads up local cache. double cache is required because it will add values from props later on
            _cacheInstances = DatabaseManager.Instance.LoadedInstanceIds;

            // get instance GroupId's from the GroupContainer
            foreach (uint groupContainer in DatabaseManager.Instance.LoadedGroupIds)
            {
                if (!_cacheGroups.Contains(groupContainer))
                {
                    _cacheGroups.Add(groupContainer);
                }
            }

            // build up cache for prop files (is a bit more complex due to the value arrays)
            IEnumerable<DatabaseIndex> propitems = DatabaseManager.Instance.Where(item => item.TypeId == 0xb1b104);
            foreach (DatabaseIndex item in propitems)
            {
                PropertyFile file = new PropertyFile();
                byte[] data = item.GetIndexData(true);
                using (MemoryStream byteStream = new MemoryStream(data))
                {
                    file.Read(byteStream);
                }

                Dictionary<uint, Property> propertyCollection;
                propertyCollection = file.Values;

                foreach (KeyValuePair<uint, Property> prop in propertyCollection)
                {
                    if (_cacheProperties.Contains(prop.Key) == false) // add Prop key Name
                    {
                        _cacheProperties.Add(prop.Key);
                    }

                    if (prop.Value is ArrayProperty) // add Values from prop array's, these are all instance id's!
                    {
                        ArrayProperty arr = prop.Value as ArrayProperty;
                        try
                        {
                            foreach (KeyProperty subProp in arr.Values)
                            {
                                if (_cacheInstances.Contains(subProp.InstanceId) == false && subProp.InstanceId != 0)
                                {
                                    _cacheInstances.Add(subProp.InstanceId);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            _cacheReady = true; // set ready flag for threads to continue...
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (_cacheReady == false) // wait for cache to be ready
            {
                Thread.Sleep(100);
            }

            while (_keywordHashes.Count > _currentKeywordHash && !_interrupted)
            {
                KeyValuePair<uint, string> keywordHash;
                lock (_keywordHashes) { keywordHash = _keywordHashes.ElementAt(_currentKeywordHash++); }

                if (_keywordHashes != null && _cacheInstances.Contains(keywordHash.Key)) // search currently loaded Instance ID's for possible matches against our hash database
                {
                    TGIRecord newRecord = new TGIRecord();
                    newRecord.DisplayName = keywordHash.Value;
                    newRecord.Id = keywordHash.Key;

                    if (newRecord.Id != 0)
                    {
                        if (TGIRegistry.Instance.Instances.Cache.ContainsKey(newRecord.Id))
                        {
                            TGIRecord currentRecord = new TGIRecord();

                            if (TGIRegistry.Instance.Instances.Cache.TryGetValue(newRecord.Id, out currentRecord) && // if entry exists
                                currentRecord.Name.ToLowerInvariant() == newRecord.Name.ToLowerInvariant()) // and if it's a dupe
                            {
                                _countDupe++;
                            }
                            else // not a dupe, but still a new value. Going to overwrite it!
                            {
                                newRecord.Comments = currentRecord.Name; // adds old value to the comments
                                TGIRegistry.Instance.Instances.InsertRecord(newRecord);
                                _countAdded++;
                            }
                        }
                        else
                        {
                            TGIRegistry.Instance.Instances.InsertRecord(newRecord);
                            _countAdded++;
                        }
                    }
                }
                else if (_cacheGroups.Contains(keywordHash.Key)) // search currently loaded Group Container ID's for possible matches against our hash database
                {
                    TGIRecord newRecord = new TGIRecord();
                    newRecord.DisplayName = keywordHash.Value;
                    newRecord.Id = keywordHash.Key;

                    if (newRecord.Id != 0)
                    {
                        if (TGIRegistry.Instance.Groups.Cache.ContainsKey(newRecord.Id))
                        {
                            TGIRecord currentRecord = new TGIRecord();
                            TGIRegistry.Instance.Groups.Cache.TryGetValue(newRecord.Id, out currentRecord);

                            if (currentRecord.Name.ToLowerInvariant() == newRecord.Name.ToLowerInvariant()) // is it a valid one?
                            {
                                _countDupe++;
                            }
                            else // always add when importing group names (no way to validate them)
                            {
                                newRecord.Comments = currentRecord.Name;
                                TGIRegistry.Instance.Groups.InsertRecord(newRecord);
                                _countAdded++;
                            }
                        }
                        else
                        {
                            TGIRegistry.Instance.Groups.InsertRecord(newRecord);
                            _countAdded++;
                        }
                    }
                }
                else if (_cacheProperties.Contains(keywordHash.Key)) // search currelty loaded Property Key ID's for possible matches against our hash database
                {
                    TGIRecord newRecord = new TGIRecord();
                    newRecord.DisplayName = keywordHash.Value;
                    newRecord.Id = keywordHash.Key;

                    if (newRecord.Id != 0)
                    {
                        if (TGIRegistry.Instance.Properties.Cache.ContainsKey(newRecord.Id))
                        {
                            TGIRecord currentRecord = new TGIRecord();
                            TGIRegistry.Instance.Properties.Cache.TryGetValue(newRecord.Id, out currentRecord);

                            if (currentRecord.Name.ToLowerInvariant() == newRecord.Name.ToLowerInvariant()) // is it a valid one?
                            {
                                _countDupe++;
                            }
                            else // names do not match, is the old one even a FNV valid one?
                            {
                                newRecord.Comments = currentRecord.DisplayName;
                                TGIRegistry.Instance.Properties.InsertRecord(newRecord);
                                _countAdded++;
                            }
                        }
                        else
                        {
                            TGIRegistry.Instance.Properties.InsertRecord(newRecord);
                            _countAdded++;
                        }
                    }
                }

                // report back our progress..
                worker.ReportProgress((int)(((float)_countNameHashesDone++ / (float)_countTotalKeywords) * 100));
                _statusText = "Searching hashes (" + _countNameHashesDone.ToString() + "/" + _countTotalKeywords + "), added: " + _countAdded + " dupes: " + _countDupe;
            }
        }

        private void btnSelectInFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "CSV File|*.csv";

            if (fileDialog.ShowDialog().GetValueOrDefault(false))
            {
                txtInFile.Text = fileDialog.FileName;
            }
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            if (btnImport.Content.ToString() == "Import")
            {
                txtInFile.IsEnabled = false;
                btnSelectInFile.IsEnabled = false;
                btnImport.Content = "Cancel";
                prgSearchProgress.Value = 0;
                cmbNumberOfThreads.IsEnabled = false;

                // read raw CSV file from disk
                _statusText = "Reading CSV dictionary file...";
                try
                {
                    using (TextReader reader = File.OpenText(_inFile))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] values = line.Split(new Char[] { ',' });

                            if (values.Length < 2) // invalid CSV entry
                                continue;

                            try
                            {
                                uint valueKey = Convert.ToUInt32(values[0].Substring(2), 16);
                                if (!_keywordHashes.ContainsKey(valueKey))
                                    _keywordHashes.Add(valueKey, values[1]); // hash, keyword
                            }
                            catch { }
                        }
                    }
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("Unable to read CSV dictionary file", "Error");
                    return;
                }

                // start up worker to scan all InstanceId's for matches with our imported CSV
                _countTotalKeywords = _keywordHashes.Count;
                _interrupted = false;
                _countAdded = 0;
                _countDupe = 0;
                _countNameHashesDone = 0;
                _statusText = "Starting threads...";
                _workers = new List<BackgroundWorker>();
                _currentKeywordHash = 0;

                _statusText = "Running...";
                for (int i = 0; i < cmbNumberOfThreads.SelectedIndex + 1; i++)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.WorkerReportsProgress = true;
                    backgroundWorker.WorkerSupportsCancellation = true;
                    backgroundWorker.DoWork += backgroundWorker_DoWork;
                    backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
                    backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;

                    backgroundWorker.RunWorkerAsync();
                    _workers.Add(backgroundWorker);
                }

                // and build up cache
                if (_cacheReady == false)
                {
                    new Thread(() =>
                    {
                        _statusText = "Building cache...";
                        CachePropKeys();
                        _statusText = "Running...";
                    }).Start();
                }
            }
            else
            {
                StopWorkers();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!IsReady())
                StopWorkers();
        }

        private void txtInFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            _inFile = txtInFile.Text;
        }
    }
}
