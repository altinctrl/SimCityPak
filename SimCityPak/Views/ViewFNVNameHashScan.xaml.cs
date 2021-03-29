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
using Gibbed.Spore.Package;
using SimCityPak.PackageReader;
using System.Text.RegularExpressions;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewFNVNameHashScan.xaml
    /// </summary>
    public partial class ViewFNVNameHashScan : Window
    {
        private DispatcherTimer _statusUpdateTimer;
        private BackgroundWorker _backgroundWorker;
        private List<string> _keywords;
        private List<string> _filesContent;

        private string _statusText;
        private string _outFile;
        private int _wordCount;
        private int _completed;
        private bool _interrupted;

        public ViewFNVNameHashScan()
        {
            InitializeComponent();
            _statusUpdateTimer = new DispatcherTimer();
            _statusUpdateTimer.Tick += new EventHandler(statusUpdateTimer_Tick);
            _statusUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            _statusUpdateTimer.Start();

            _outFile = "FNV_keywords_file.csv";
            _keywords = new List<string>();
            _filesContent = new List<string>();
            _statusText = "Idle...";

            txtOutFile.Text = _outFile;
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
            if (!_interrupted)
            {
                prgSearchProgress.Value = 100;
                //System.Windows.Forms.MessageBox.Show(_filesContent.Count.ToString() + " files analyzed, found " + _wordCount + " unique keywords", "Done!");
                _statusText = _filesContent.Count.ToString() + " files analyzed, found " + _wordCount + " unique keywords";
                prgSearchProgress.Value = 0;
            }
            else
            {
                prgSearchProgress.Value = 0;
            }

            txtOutFile.IsEnabled = true;
            btnSelectOutFile.IsEnabled = true;
            btnScanFiles.IsEnabled = true;
            btnScanPackages.IsEnabled = true;
            btnScanPackages.Content = "Scan Open Packages";
            btnScanFiles.Content = "Scan External Files";
        }

        void StartWorker()
        {
            _statusText = "Starting threads...";

            _interrupted = false;
            _keywords.Clear();
            _completed = 0;

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += backgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            _backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;

            _backgroundWorker.RunWorkerAsync();
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string word = "";

            foreach (string fileContent in _filesContent)
            {
                for (int i = 0; i < fileContent.Length; i++)
                {
                    if (!Regex.IsMatch(fileContent[i].ToString(), @"^[a-zA-Z0-9_\-]+$")) // looks like the end of a word
                    {
                        double n = 0;

                        if (word.Length > 1 && // ignore single chars
                            word.Length < 100 && // this cannot ever be an identifier
                            word.IndexOf("0x") != 0 && // and nor do we want any of the hex values
                            !double.TryParse(word, out n)) // and ignore numbers
                        {
                            _keywords.Add(word);
                        }
                        word = "";
                    }
                    else word += (char)fileContent[i];

                    _statusText = "Searching for keywords (" + _keywords.Count + " found)";

                    // in case the user cancelled the process...
                    if (_backgroundWorker.CancellationPending)
                    {
                        _interrupted = true;
                        _statusText = "User cancelled...";

                        return;
                    }
                }

                // remove dupes
                _statusText = "Removing dupes";
                _keywords = new List<string>(_keywords.Distinct().ToArray());
                _wordCount = _keywords.Count;

                // report back our progress..
                worker.ReportProgress((int)(((float)_completed++ / (float)_filesContent.Count) * 100));
            }

            List<string> result = new List<string>();
            _completed = 0; // re-use for FNV calc state
            StreamWriter file;

            try
            {
                file = new StreamWriter(_outFile, File.Exists(_outFile)); // append if file exists
            }
            catch
            {
                _statusText = "Failed to open output file!";
                return;
            }

            // calculates all FNV hashes
            foreach (string item in _keywords)
            {
                // incase a cancallation was sent somewhere along the line...
                if (_backgroundWorker.CancellationPending)
                {
                    _interrupted = true;
                    _statusText = "User cancelled...";

                    file.Close();
                    return;
                }

                // writes directly to file
                file.WriteLine(Gibbed.Spore.Helpers.StringHelpers.FNV(item).ToHex() + "," + item);

                worker.ReportProgress((int)(((float)_completed++ / (float)_keywords.Count) * 100));
                _statusText = "Calculating FNV hashes (" + _completed + "/" + _keywords.Count.ToString() + ")";
            }

            file.Close();
            _statusText = "Idle...";
        }

        private void btnSelectOutFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "CSV File|*.csv";
            fileDialog.OverwritePrompt = false; // appends if file exists

            if (fileDialog.ShowDialog().GetValueOrDefault(false))
            {
                txtOutFile.Text = fileDialog.FileName;
            }
        }

        private void btnScanFiles_Click(object sender, RoutedEventArgs e)
        {
            _filesContent.Clear();

            if (btnScanFiles.Content.ToString() == "Scan External Files")
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Multiselect = true;
                fileDialog.Filter = "Any File|*.*";

                try
                {
                    if (fileDialog.ShowDialog().GetValueOrDefault(false))
                    {
                        foreach (string fileName in fileDialog.FileNames)
                        {
                            if (File.Exists(fileName))
                                _filesContent.Add(File.ReadAllText(fileName));
                        }

                        txtOutFile.IsEnabled = false;
                        btnSelectOutFile.IsEnabled = false;
                        btnScanPackages.IsEnabled = false;
                        btnScanFiles.Content = "Cancel";

                        StartWorker();
                    }
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("Unable to read file", "Error");
                }
            }
            else
            {
                _interrupted = true;
                if (_backgroundWorker.IsBusy)
                    _backgroundWorker.CancelAsync();
            }
        }

        private void btnScanPackages_Click(object sender, RoutedEventArgs e)
        {
            if (btnScanPackages.Content.ToString() == "Scan Open Packages")
            {
                txtOutFile.IsEnabled = false;
                btnSelectOutFile.IsEnabled = false;
                btnScanFiles.IsEnabled = false;
                btnScanPackages.Content = "Cancel";

                // only JavaScript, JSON and ER2 files are included
                IEnumerable<DatabaseIndex> packageFiles;
                packageFiles = DatabaseManager.Instance.Where(item => item.TypeId == 0x0a98eaf0 || item.TypeId == 0x67771f5c || item.TypeId == 0x08068aec);

                _filesContent.Clear();
                foreach (DatabaseIndex index in packageFiles)
                {
                    if (index == null)
                        continue;

                    byte[] content = index.GetIndexData(true);
                    _filesContent.Add(Encoding.UTF8.GetString(content, 0, content.Length));
                }

                StartWorker();
            }
            else
            {
                _interrupted = true;
                _backgroundWorker.CancelAsync();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
                _backgroundWorker.CancelAsync();            
        }

        private void txtOutFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            _outFile = txtOutFile.Text;
        }
    }
}
