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
using Microsoft.Win32;
using System.IO;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewSettings.xaml
    /// </summary>
    public partial class ViewSettings : Window
    {
        public ViewSettings()
        {
            InitializeComponent();
            txtLocale.Text = Properties.Settings.Default.LocaleFile;
            txtDefaultFile.Text = Properties.Settings.Default.DefaultFile;
            txtRegistryFiles.Text = Properties.Settings.Default.RegistryFolder;
            chkPROPImages.IsChecked = Properties.Settings.Default.PropImagesExpanded;
            chkPropertyIds.IsChecked = Properties.Settings.Default.ShowPropertyIds;
            chkPropertyInternalNames.IsChecked = Properties.Settings.Default.ShowPropertyInternalNames;
            txtSimCityFolder.Text = Properties.Settings.Default.SimCityFolder;
        }

        private void btnLocaleBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Package Files|*.package";
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                txtLocale.Text = dlg.FileName;
            }
        }

        private void btnDefaultFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Package Files|*.package";
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                txtDefaultFile.Text = dlg.FileName;
            }
        }
        private void btnRegistryFilesBrowse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowDialog();
            txtRegistryFiles.Text = dlg.SelectedPath;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.LocaleFile = txtLocale.Text;
            Properties.Settings.Default.DefaultFile = txtDefaultFile.Text;
            Properties.Settings.Default.RegistryFolder = txtRegistryFiles.Text;
            Properties.Settings.Default.SimCityFolder = txtSimCityFolder.Text;
            Properties.Settings.Default.PropImagesExpanded = (bool)chkPROPImages.IsChecked;
            Properties.Settings.Default.ShowPropertyIds = (bool)chkPropertyIds.IsChecked;
            Properties.Settings.Default.ShowPropertyInternalNames = (bool)chkPropertyInternalNames.IsChecked;
            Properties.Settings.Default.Save();

            //Load all the locale files into memory
            if (!string.IsNullOrEmpty(Properties.Settings.Default.LocaleFile))
            {
                if (File.Exists(Properties.Settings.Default.LocaleFile))
                {
                    LocaleRegistry.Instance = LocaleRegistry.Create();
                }
            }

            this.Close();
        }

        private void btnSimCityFolderBrowse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = txtSimCityFolder.Text;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(dlg.SelectedPath))
                {
                    txtSimCityFolder.Text = dlg.SelectedPath;
                }
            }
        }
    }
}
