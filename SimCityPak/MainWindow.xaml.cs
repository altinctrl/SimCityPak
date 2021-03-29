using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using Gibbed.Spore.Package;
using SimCityPak.PackageReader;
using SimCityPak.Views;
using Clipboard = System.Windows.Clipboard;
using MenuItem = System.Windows.Controls.MenuItem;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System.Text.RegularExpressions;
using SimCityPak.Views.AdvancedEditors.DecalDictionary;
using Gibbed.Spore.Properties;
using System.Globalization;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ListCollectionView view;
        private ListCollectionView packageView;
        private List<string> _currentPackageFiles;

        public MainWindow()
        {
            InitializeComponent();
            view = new ListCollectionView(DatabaseManager.Instance.Indices);
            view.Filter = TextFilter;
            dataGridInstances.ItemsSource = view;
            packageView = new ListCollectionView(DatabaseManager.Instance.PackageFiles);
            packageDataGrid.ItemsSource = packageView;
            _currentPackageFiles = new List<string>();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Package Files|*.package";
            if (fileDialog.ShowDialog().GetValueOrDefault(false))
            {
                //  packageFile = new FileInfo(fileDialog.FileName);
                loadPackagesFromPath(fileDialog.FileNames);
            }
        }

        private void loadPackagesFromPath(string[] paths)
        {
            DatabaseManager.Instance.LoadPackages(paths);
            this.Title = "SimCityPak";
            mnuSearchPROP.IsEnabled = true;
        }

        private void dataGridInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                if (e.AddedItems[0].GetType() == typeof(DatabaseIndex))
                {
                    DatabaseIndex index = (DatabaseIndex)e.AddedItems[0];
                    byte[] data;
                    if (index.IsModified)
                    {
                        data = index.ModifiedData.GetData();
                    }
                    else
                    {
                        data = index.GetIndexData(true);
                    }
                    viewContainer.Content = new DatabaseIndexData(index, data);
                    viewHexContainer.DataContext = new DatabaseIndexData(index, data);
                }
            }
        }

        private void filterTextBoxes_TextChanged(object sender, TextChangedEventArgs e)
        {
            view.Refresh();
        }

        private void filterCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (view != null)
            {
                view.Refresh();
            }
            /* no longer required?
            if (sender == comboboxTypes)
            {
                comboboxInstances.ItemsSource = TGIRegistry.Instance.Instances.Cache.Values.Where(it => !it.DisplayName.Equals(it.HexId) && (comboboxTypes.SelectedItem == null || it.TypeId == ((TypeRegistryRecord)comboboxTypes.SelectedItem).Id*)).OrderBy<TGIRecord, String>(x => x.DisplayName);
            }
            */
        }

        bool IsMatchRegex(string str, string pat, char singleWildcard = '?', char multipleWildcard = '*')
        {
            pat = Regex.Escape(pat);
            pat = pat.Replace(Regex.Escape("?"), ".");
            pat = "^.*" + pat.Replace(Regex.Escape("*"), ".*") + ".*$";

            Regex reg = new Regex(pat);

            return reg.IsMatch(str);
        }

        public bool TextFilter(object o)
        {
            DatabaseIndex p = (o as DatabaseIndex);
            if (comboboxInstances.SelectedItem == null)
            {
                if (!string.IsNullOrEmpty(comboboxInstances.Text))
                {
                    if (!IsMatchRegex(p.InstanceId.ToHex(), comboboxInstances.Text))
                        return false;
                }
            }
            else
            {
                TGIRecord entry = (TGIRecord)comboboxInstances.SelectedItem;
                if (p.InstanceId != entry.Id)
                    return false;
            }
            if (comboboxGroups.SelectedItem == null)
            {
                if (!string.IsNullOrEmpty(comboboxGroups.Text))
                {
                    if (!IsMatchRegex(p.GroupContainer.ToHex(), comboboxGroups.Text))
                        return false;
                }
            }
            else
            {
                TGIRecord entry = (TGIRecord)comboboxGroups.SelectedItem;
                if (p.GroupId != entry.Id)
                    return false;
            }
            if (comboboxTypes.SelectedItem != null)
            {
                TGIRecord entry = (TGIRecord)comboboxTypes.SelectedItem;

                if (p.TypeId != entry.Id)
                    return false;
            }
            return true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.DefaultFile))
            {
                loadPackagesFromPath(new string[] { Properties.Settings.Default.DefaultFile });
            }
            //load the types into the combo box
            FillFilter();
        }

        private void FillFilter()
        {
            if (DatabaseManager.Instance.Indices != null)
            {
                comboboxTypes.Items.Clear();
                foreach (TGIRecord regRecord in TGIRegistry.Instance.FileTypes.CacheSortedList)
                {
                    if (DatabaseManager.Instance.LoadedFileTypeIds.Contains(regRecord.Id))
                    {
                        comboboxTypes.Items.Add(regRecord);
                    }
                }
                comboboxTypes.UpdateLayout();

                comboboxGroups.Items.Clear();
                foreach (TGIRecord regRecord in TGIRegistry.Instance.Groups.CacheSortedList)
                {
                    if (DatabaseManager.Instance.LoadedGroupIds.Contains(regRecord.Id))
                    {
                        comboboxGroups.Items.Add(regRecord);
                    }
                }
                comboboxGroups.UpdateLayout();

                comboboxInstances.Items.Clear();
                foreach (TGIRecord regRecord in TGIRegistry.Instance.Instances.CacheSortedList)
                {
                    if (DatabaseManager.Instance.LoadedInstanceIds.Contains(regRecord.Id))
                    {
                        comboboxInstances.Items.Add(regRecord);
                    }
                }
                comboboxInstances.UpdateLayout();
            }
        }

        private void mnuInstanceName_Click(object sender, RoutedEventArgs e)
        {
            DatabaseIndex index = (DatabaseIndex)((MenuItem)sender).DataContext;
            ViewInstanceRegistryRecord window = new ViewInstanceRegistryRecord(index);
            window.ShowDialog();
        }

        private void mnuExportInstance_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridInstances.SelectedItems.Count > 1)
            {
                using (FolderBrowserDialog browserDialog = new FolderBrowserDialog())
                {
                    browserDialog.Description = "Choose Export Folder";
                    browserDialog.ShowNewFolderButton = true;
                    DialogResult result = browserDialog.ShowDialog();
                    string path = browserDialog.SelectedPath;
                    if (!Directory.Exists(path))
                    {
                        return;
                    }
                    foreach (var selectedItem in dataGridInstances.SelectedItems)
                    {
                        DatabaseIndex element = selectedItem as DatabaseIndex;
                        if (element == null)
                            continue;

                        try
                        {
                            DatabasePackedFile.WriteToPath(element, Path.Combine(path, element.GetExportFileName()));
                        }
                        catch
                        {
                            System.Windows.Forms.MessageBox.Show("File is readonly", "Error");
                        }
                    }
                }
            }
            else
            {
                SaveFileDialog fileDialog = new SaveFileDialog();
                DatabaseIndex index = (DatabaseIndex)((MenuItem)sender).DataContext;
                fileDialog.FileName = index.GetExportFileName();
                if (fileDialog.ShowDialog().GetValueOrDefault(false))
                {
                    try
                    {
                        DatabasePackedFile.WriteToPath(index, fileDialog.FileName);
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("File is readonly", "Error");
                    }
                }
            }
        }

        private void mnuExportPackage_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Package Files|*.package";
            dlg.DefaultExt = "package";
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                List<DatabaseIndex> indices = new List<DatabaseIndex>();
                foreach (var selectedItem in dataGridInstances.SelectedItems)
                {
                    DatabaseIndex element = selectedItem as DatabaseIndex;
                    indices.Add(element);
                }

                try
                {
                    DatabasePackedFile.SaveAs(dlg.FileName, indices);
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("File is readonly", "Error");
                }
            }
        }

        private void mnuSettings_Click(object sender, RoutedEventArgs e)
        {
            Views.ViewSettings settings = new ViewSettings();
            settings.ShowDialog();
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            ViewAbout about = new ViewAbout();
            about.ShowDialog();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            comboboxTypes.SelectedItem = null;
            comboboxGroups.Text = null;
            comboboxInstances.Text = null;
            view.Refresh();
        }

        private void mnuCopyType_Click(object sender, RoutedEventArgs e)
        {
            DatabaseIndex index = (DatabaseIndex)((MenuItem)sender).DataContext;
            Clipboard.SetText(index.TypeId.ToHex());
        }

        private void mnuCopyGroup_Click(object sender, RoutedEventArgs e)
        {
            DatabaseIndex index = (DatabaseIndex)((MenuItem)sender).DataContext;
            Clipboard.SetText(index.GroupContainer.ToHex());
        }

        private void mnuCopyInstance_Click(object sender, RoutedEventArgs e)
        {
            DatabaseIndex index = (DatabaseIndex)((MenuItem)sender).DataContext;
            Clipboard.SetText(index.InstanceId.ToHex());
        }

        private void mnuFilterType_Click(object sender, RoutedEventArgs e)
        {
            DatabaseIndex index = (DatabaseIndex)((MenuItem)sender).DataContext;
            foreach (TGIRecord item in comboboxTypes.Items)
            {
                if (item.Id == index.TypeId)
                {
                    comboboxTypes.SelectedItem = item;
                    break;
                }
            }
        }

        private void mnuFilterGroup_Click(object sender, RoutedEventArgs e)
        {
            DatabaseIndex index = (DatabaseIndex)((MenuItem)sender).DataContext;
            comboboxGroups.Text = index.GroupContainer.ToHex();
        }

        private void mnuFilterInstance_Click(object sender, RoutedEventArgs e)
        {
            DatabaseIndex index = (DatabaseIndex)((MenuItem)sender).DataContext;
            comboboxInstances.Text = index.InstanceId.ToHex();
        }

        private void mnuSearchPROP_Click(object sender, RoutedEventArgs e)
        {
            ViewSearchProperties res = new ViewSearchProperties(this);
            res.Show();
        }

        private void mnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            DatabasePackedFile PackageFile = (DatabasePackedFile)packageDataGrid.SelectedItem;
            if (PackageFile != null)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Package Files|*.package";
                dlg.DefaultExt = "package";
                if (dlg.ShowDialog().GetValueOrDefault(false))
                {
                    PackageFile.SaveAs(dlg.FileName);
                }
            }
        }

        private void mnuFNV_Click(object sender, RoutedEventArgs e)
        {
            new ViewFNV().Show();
        }

        private void mnuSearchPROPIID_Click(object sender, RoutedEventArgs e)
        {
            DatabaseIndex index = (DatabaseIndex)((MenuItem)sender).DataContext;
            ViewSearchProperties res = new ViewSearchProperties(this, index.InstanceId.ToHex(), null, true);
            res.Show();
        }

        private void mnuEditTGI_Click(object sender, RoutedEventArgs e)
        {
            DatabaseIndex index = (DatabaseIndex)((MenuItem)sender).DataContext;

            ViewEditKeyProperty viewEditKey = new ViewEditKeyProperty(index.TypeId, index.GroupContainer, index.InstanceId);
            if (viewEditKey.ShowDialog().GetValueOrDefault(false))
            {
                index.ModifiedTypeId = viewEditKey.TypeId;
                index.ModifiedGroupId = viewEditKey.GroupContainer;
                index.ModifiedInstanceId = viewEditKey.InstanceId;
            }
        }

        private void OpenPackages_click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = "Package Files|*.package";

            if (fileDialog.ShowDialog().GetValueOrDefault(false))
            {
                _currentPackageFiles.Clear();
                _currentPackageFiles = fileDialog.FileNames.ToList();

                DatabaseManager.Instance.CloseAll();
                viewContainer.Content = null;
                viewHexContainer.DataContext = null;

                loadPackagesFromPath(fileDialog.FileNames);
            }

            FillFilter();
        }

        private void AddPackages_click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = "Package Files|*.package";

            try
            {
                if (fileDialog.ShowDialog().GetValueOrDefault(false))
                {
                    _currentPackageFiles.AddRange(fileDialog.FileNames.ToList());

                    loadPackagesFromPath(fileDialog.FileNames);
                }

                FillFilter();
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Invalid package file", "Error");
            }
        }

        public void ReloadPackages()
        {
            try
            {
                if (_currentPackageFiles.Count > 0)
                {
                    DatabaseManager.Instance.CloseAll();
                    viewContainer.Content = null;
                    viewHexContainer.DataContext = null;

                    loadPackagesFromPath(_currentPackageFiles.ToArray());
                }

                FillFilter();
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Invalid package file", "Error");
            }
        }

        private void ReloadPackages_click(object sender, RoutedEventArgs e)
        {
            TGIRegistry.Instance.SqlCacheReloadAll();
            ReloadPackages();
        }

        private void dataGridInstances_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            DataGridColumn column = e.Column;
            ListSortDirection direction = (column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
            column.SortDirection = direction;
            //ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(view);
            DataBaseIndexComparer mySort = new DataBaseIndexComparer(direction, column);
            view.CustomSort = mySort;  // provide our own sort
        }

        private void mnuClose_Click(object sender, RoutedEventArgs e)
        {
            IList<object> packages = (IList<object>)packageDataGrid.SelectedItems;

            //apparently this is how you clone a list...
            //cloning the list is needed because closing a package causes SelectedItems to change
            List<DatabasePackedFile> tmp = packages.Select<object, DatabasePackedFile>(i => (DatabasePackedFile)i).ToList();

            foreach (DatabasePackedFile p in tmp)
            {
                DatabaseManager.Instance.ClosePackage(p);
            }
        }

        private void CloseAllPackages_click(object sender, RoutedEventArgs e)
        {
            DatabaseManager.Instance.CloseAll();

            _currentPackageFiles.Clear();
            ReloadPackages(); // reloads empty list of packages, setting null to content is not getting anywhere..

            mnuSearchPROP.IsEnabled = false;
            //mnuSearchInvalidFnvNames.IsEnabled = false;
        }

        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            //Save As to a temp file, delete the old one and move the new one
            DatabasePackedFile PackageFile = (DatabasePackedFile)packageDataGrid.SelectedItem;
            if (PackageFile != null)
            {
                string tempFileName = string.Format("{0}\\{1}.temp", PackageFile.packageFileInfo.Directory.FullName, Guid.NewGuid());
                string originalFileName = PackageFile.packageFileInfo.FullName;
                PackageFile.SaveAs(tempFileName);
                File.Delete(originalFileName);
                File.Move(tempFileName, originalFileName);
            }
        }

        private void mnuProperty_Click(object sender, RoutedEventArgs e)
        {
            TGIRecord record = new TGIRecord();
            ViewPropertyRegistryRecord window = new ViewPropertyRegistryRecord(record);
            window.ShowDialog();
        }

        private void mnuScan_Click(object sender, RoutedEventArgs e)
        {
            ViewScanUnusedIDs window = new ViewScanUnusedIDs(this);
            window.Show();
        }

        private void mnuImportInstance_Click(object sender, RoutedEventArgs e)
        {
            DatabasePackedFile packageFile = (DatabasePackedFile)packageDataGrid.SelectedItem;
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Exported Files (SCP_*)|SCP_*|All Files (*.*)|*.*";
            if (fileDialog.ShowDialog().GetValueOrDefault(false))
            {
                FileInfo fi = new FileInfo(fileDialog.FileName);
                uint typeId = 0; uint GroupContainer = 0; uint instanceId = 0;
                try
                {
                    string[] fileNameParts = Path.GetFileNameWithoutExtension(fileDialog.FileName).Split('_');
                    string[] tgi = fileNameParts[fileNameParts.Length - 1].Split('-');
                    typeId = Convert.ToUInt32(tgi[0].Substring(2), 16);
                    GroupContainer = Convert.ToUInt32(tgi[1].Substring(2), 16);
                    instanceId = Convert.ToUInt32(tgi[2].Substring(2), 16);
                }
                catch { }
                ViewEditKeyProperty viewEditKey = new ViewEditKeyProperty(typeId, GroupContainer, instanceId);
                if (viewEditKey.ShowDialog().GetValueOrDefault(false))
                {
                    typeId = viewEditKey.TypeId;
                    GroupContainer = viewEditKey.GroupContainer;
                    instanceId = viewEditKey.InstanceId;
                }
                DatabaseIndex createdIndex = null;
                using (FileStream stream = fi.OpenRead())
                {
                    byte[] data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    createdIndex = packageFile.InsertSubFile(data, typeId, GroupContainer, instanceId);
                }
                if (createdIndex != null)
                {
                    tabControl1.SelectedIndex = 0;
                    dataGridInstances.SelectedItem = createdIndex;
                    dataGridInstances.ScrollIntoView(createdIndex);
                }
            }
        }

        private void mnuCopyKey_Click(object sender, RoutedEventArgs e)
        {
            DatabaseIndex index = (DatabaseIndex)((MenuItem)sender).DataContext;
            Clipboard.SetText(index.TypeId.ToHex() + "-" + index.GroupContainer.ToHex() + "-" + index.InstanceId.ToHex());
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try //Send Close to all open child windows (PROP search, Text search etc)
            {
                for (int intCounter = App.Current.Windows.Count - 1; intCounter > 0; intCounter--)
                    App.Current.Windows[intCounter].Close();
            }
            catch { }

            App.Current.Shutdown();
        }

        private void mnuOpenInNewWindow_Click(object sender, RoutedEventArgs e)
        {
            ViewWindow window = new ViewWindow(this, (DatabaseIndex)dataGridInstances.SelectedItem);
            window.Show();
        }

        private void dataGridInstances_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dataGridInstances.SelectedItem != null)
            {
                ViewWindow window = new ViewWindow(this, (DatabaseIndex)dataGridInstances.SelectedItem);
                window.Show();
            }
        }

        private void mnuFNVNameHashScanner_Click(object sender, RoutedEventArgs e)
        {
            ViewFNVNameHashScan nameHashScan = new ViewFNVNameHashScan();
            nameHashScan.Show();
        }

        private void mnuFNVNameHashImport_Click(object sender, RoutedEventArgs e)
        {
            ViewFNVNameHashImport nameHashImport = new ViewFNVNameHashImport();
            nameHashImport.Show();
        }

        private void Exit_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void mnuSQLExportImport_Click(object sender, RoutedEventArgs e)
        {
            ViewSQLExportImport sqlwindow = new ViewSQLExportImport();
            sqlwindow.ShowDialog();
            ReloadPackages();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
           // ViewDecalCreator decalCreator = new ViewDecalCreator();
            //decalCreator.ShowDialog();
        }

        private void mnuInstanceIds_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.LocaleFile != string.Empty)
            {
                try
                {
                    Dictionary<uint, string> itemNames = new Dictionary<uint, string>();
                    foreach (DatabaseIndex index in view)
                    {
                        //for all property files, see if the name property exists
                        if (index.TypeId == 0x00b1b104)
                        {
                            using (MemoryStream byteStream = new MemoryStream(index.GetIndexData(true)))
                            {
                                PropertyFile propertyFile = new PropertyFile();
                                propertyFile.Read(byteStream);

                                if (!itemNames.ContainsKey(index.InstanceId))
                                {

                                    if (propertyFile.Values.ContainsKey(0x09FB78CB))
                                    {
                                        ArrayProperty arrprop = propertyFile.Values[0x09FB78CB] as ArrayProperty;
                                        TextProperty prop = arrprop.Values[0] as TextProperty;
                                        string name = LocaleRegistry.Instance.GetLocalizedString(prop.TableId, prop.InstanceId);

                                        itemNames.Add(index.InstanceId, name.Replace("'", "''"));
                                    }
                                    else if (propertyFile.Values.ContainsKey(0x0A09F5FA))
                                    {
                                        ArrayProperty arrprop = propertyFile.Values[0x0A09F5FA] as ArrayProperty;
                                        TextProperty prop = arrprop.Values[0] as TextProperty;
                                        string name = LocaleRegistry.Instance.GetLocalizedString(prop.TableId, prop.InstanceId);

                                        itemNames.Add(index.InstanceId, name.Replace("'", "''"));
                                    }
                                    else if (propertyFile.Values.ContainsKey(0x09B711C3))
                                    {
                                        ArrayProperty arrprop = propertyFile.Values[0x09B711C3] as ArrayProperty;
                                        TextProperty prop = arrprop.Values[0] as TextProperty;
                                        string name = LocaleRegistry.Instance.GetLocalizedString(prop.TableId, prop.InstanceId);

                                        itemNames.Add(index.InstanceId, name.Replace("'", "''"));
                                    }
                                    else if (propertyFile.Values.ContainsKey(0x0E28B5BC))
                                    {
                                        ArrayProperty arrprop = propertyFile.Values[0x0E28B5BC] as ArrayProperty;
                                        TextProperty prop = arrprop.Values[0] as TextProperty;
                                        string name = LocaleRegistry.Instance.GetLocalizedString(prop.TableId, prop.InstanceId);

                                        itemNames.Add(index.InstanceId, name.Replace("'", "''"));
                                    }
                                    else if (propertyFile.Values.ContainsKey(0x0E28B5D5))
                                    {
                                        ArrayProperty arrprop = propertyFile.Values[0x0E28B5D5] as ArrayProperty;
                                        TextProperty prop = arrprop.Values[0] as TextProperty;
                                        string name = LocaleRegistry.Instance.GetLocalizedString(prop.TableId, prop.InstanceId);

                                        itemNames.Add(index.InstanceId, name.Replace("'", "''"));
                                    }
                                }
                            }
                        }
                    }

                    foreach (KeyValuePair<uint, string> name in itemNames)
                    {
                        TGIRegistry.Instance.Instances.InsertRecord(new TGIRecord() { Id = name.Key, Name = name.Value, Comments = name.Value });
                    }

                    ReloadPackages();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(string.Format("An error occurred while attempting to load instance names from your locale file: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("The Locale File is not specified in the application settings!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void mnuDeployScripts_Click(object sender, RoutedEventArgs e)
        {
            /*string ecoGamePath = @"E:\Games\Origin\SimCityDevTest\SimCityUserData\EcoGame\";
            DirectoryInfo folderEcoGame = new DirectoryInfo(ecoGamePath);
            FileInfo[] scriptsFiles = folderEcoGame.GetFiles("SimCity-Scripts_*.package", SearchOption.TopDirectoryOnly);
            FileInfo scriptsFile = scriptsFiles.First(sf => sf.LastWriteTime == scriptsFiles.Max(sf2 => sf2.LastWriteTime));
            DatabasePackedFile ScriptsPackage = DatabasePackedFile.LoadFromFile(scriptsFile.FullName);

            //get all scripts mods
            DirectoryInfo folderMods = new DirectoryInfo(@"E:\Games\Origin\SimCityDevTest\Mods\");
            foreach (FileInfo modPackageFile in folderMods.GetFiles("*.package"))
            {
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
            }

            string tempFileName = string.Format("{0}\\{1}.temp", ScriptsPackage.packageFileInfo.Directory.FullName, Guid.NewGuid());
            string originalFileName = ScriptsPackage.packageFileInfo.FullName;
            ScriptsPackage.SaveAs(tempFileName);
            File.Delete(originalFileName);
            File.Move(tempFileName, originalFileName);*/
        }

        private void mnuExportCopy_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Package Files|*.package";
            dlg.DefaultExt = "package";
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                List<DatabaseIndex> indices = new List<DatabaseIndex>();
                foreach (var selectedItem in dataGridInstances.SelectedItems)
                {
                    DatabaseIndex element = selectedItem as DatabaseIndex;
                    indices.Add(element);
                }

                try
                {
                    DatabasePackedFile.SaveAs(dlg.FileName, indices);
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("File is readonly", "Error");
                }
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {

        }
    }

    public class DataBaseIndexComparer : IComparer
    {
        public DataBaseIndexComparer(ListSortDirection direction, DataGridColumn column)
        {
            Direction = direction;
            Column = column;
        }

        public ListSortDirection Direction { get; private set; }

        public DataGridColumn Column { get; private set; }

        int IComparer.Compare(object a, object b)
        {
            DatabaseIndex x = (DatabaseIndex)a, y = (DatabaseIndex)b;
            if (Direction != ListSortDirection.Ascending)
            {
                DatabaseIndex t = x;
                x = y;
                y = t;
            }

            switch ((string)Column.Header)
            {
                case "":
                    {
                        int result = x.TypeName.CompareTo(y.TypeName);
                        if (result != 0)
                            return result;
                        else
                            return x.InstanceType.CompareTo(y.InstanceType);
                    }

                case "GroupContainer":
                    return x.GroupName.CompareTo(y.GroupName);

                case "Instance":
                    //return x.InstanceId.CompareTo(y.InstanceId);
                    if (x.InstanceName != null)
                        return x.InstanceName.CompareTo(y.InstanceName);
                    return 0;

                case "Compressed":
                    return x.Compressed.CompareTo(y.Compressed);

                case "Size":
                    return x.CompressedSize.CompareTo(y.CompressedSize);

                case "Modified":
                    return x.IsModified.CompareTo(y.IsModified);
            }
            return 0;
        }
    }
}
