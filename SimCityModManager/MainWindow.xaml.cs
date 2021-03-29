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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gibbed.Spore.Package;
using System.IO;
using System.ComponentModel;

namespace SimCityModManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            txtGameFolder.Text = Properties.Settings.Default.GameFolder;

            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnInstall.IsEnabled = true;
            progressBarInstalling.Value = 100;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarInstalling.Value = e.ProgressPercentage;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string gameFolder = e.Argument.ToString();

            string ecoGamePath = gameFolder + @"\SimCityUserData\EcoGame\";
            DirectoryInfo folderEcoGame = new DirectoryInfo(ecoGamePath);
            FileInfo[] scriptsFiles = folderEcoGame.GetFiles("SimCity-Scripts_*.package", SearchOption.TopDirectoryOnly);
            FileInfo scriptsFile = scriptsFiles.First(sf => sf.LastWriteTime == scriptsFiles.Max(sf2 => sf2.LastWriteTime));
            DatabasePackedFile ScriptsPackage = DatabasePackedFile.LoadFromFile(scriptsFile.FullName);

            //get all scripts mods
            
            DirectoryInfo folderMods = new DirectoryInfo(gameFolder + @"\Mods\");
            FileInfo[] packageFiles = folderMods.GetFiles("*.package");
            int i = 0;
            foreach (FileInfo modPackageFile in packageFiles)
            {
                worker.ReportProgress((i * 100) / packageFiles.Length);

                DatabasePackedFile modPackage = DatabasePackedFile.LoadFromFile(modPackageFile.FullName);
                foreach (DatabaseIndex index in modPackage.Indices)
                {
                    DatabaseIndex existingIndex = ScriptsPackage.Indices.FirstOrDefault(d => d.TypeId == index.TypeId && d.GroupId == index.GroupId && d.InstanceId == index.InstanceId);
                    if (existingIndex != null)
                    {
                        existingIndex.IsModified = true;
                        ModifiedGenericFile data = new ModifiedGenericFile();
                        data.FileData = index.GetIndexData(true);
                        existingIndex.ModifiedData = data;
                        existingIndex.Compressed = false;
                    }
                    else
                    {
                        ScriptsPackage.Indices.Add(index);
                    }
                }
                i++;
            }

            string tempFileName = string.Format("{0}\\{1}.temp", ScriptsPackage.packageFileInfo.Directory.FullName, Guid.NewGuid());
            string originalFileName = ScriptsPackage.packageFileInfo.FullName;
            ScriptsPackage.SaveAs(tempFileName);
            File.Delete(originalFileName);
            File.Move(tempFileName, originalFileName);



        }

        BackgroundWorker worker = new BackgroundWorker();

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            btnInstall.IsEnabled = false;

            worker.RunWorkerAsync(txtGameFolder.Text);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.GameFolder = txtGameFolder.Text;
            Properties.Settings.Default.Save();
        }
    }
}
