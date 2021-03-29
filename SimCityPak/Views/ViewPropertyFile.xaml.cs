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
using Gibbed.Spore.Properties;
using System.IO;
using System.Xml;
using SimCityPak.Views;
using Gibbed.Spore.Package;
using System.Collections;
using SimCityPak.PackageReader;
using System.Diagnostics;
using Microsoft.Win32;
using SimCityPak.Views.AdvancedEditors;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewPropertyFile.xaml
    /// </summary>
    public partial class ViewPropertyFile : UserControl
    {
        public ViewPropertyFile()
        {
            InitializeComponent();
        }

        private List<PropertyModel> displayProperties = new List<PropertyModel>();
        private PropertyFile propertyFile;

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                Dictionary<uint, Property> propertyContainer;

                try
                {
                    using (MemoryStream byteStream = new MemoryStream(index.Data))
                    {

                        propertyFile = new PropertyFile();
                        propertyFile.Read(byteStream);
                        propertyContainer = propertyFile.Values;

                        pnlImages.Children.RemoveRange(0, pnlImages.Children.Count);
                        displayProperties = new List<PropertyModel>();

                        ReloadDisplayProperties(propertyContainer);
                    }
                }
                catch (Exception ex)
                {

                    tbxError.Visibility = System.Windows.Visibility.Visible;
                    dataGrid1.Visibility = System.Windows.Visibility.Collapsed;
                    string exceptionMessage = ex.Message + Environment.NewLine;
                    Exception subEx = ex.InnerException;
                    while (subEx != null)
                    {
                        exceptionMessage += subEx.Message + Environment.NewLine;
                        subEx = ex.InnerException;
                    }

                    tbxError.Text = exceptionMessage + Environment.NewLine + ex.StackTrace;
                }

            }
        }

        private void AddProperty(uint id, Property prop)
        {
            if (prop is ArrayProperty)
            {
                int arrIndex = 0;
                ArrayProperty arr = prop as ArrayProperty;
                foreach (Property subProp in arr.Values)
                {
                    subProp.PropertyChanged += new EventHandler(Value_PropertyChanged);
                    displayProperties.Add(new PropertyModel(id, subProp, true, arrIndex));
                    arrIndex++;
                }
            }
            else
            {
                displayProperties.Add(new PropertyModel(id, prop, false, 0));
            }


        }

        private void ReloadDisplayProperties(Dictionary<uint, Property> propertyCollection)
        {
            try
            {
                displayProperties = new List<PropertyModel>();
                foreach (KeyValuePair<uint, Property> prop in propertyCollection)
                {

                    if (prop.Value is ArrayProperty)
                    {
                        int arrIndex = 0;
                        ArrayProperty arr = prop.Value as ArrayProperty;
                        foreach (Property subProp in arr.Values)
                        {
                            subProp.PropertyChanged += new EventHandler(Value_PropertyChanged);
                            displayProperties.Add(new PropertyModel(prop.Key, subProp, true, arrIndex));
                            arrIndex++;
                        }
                    }
                    else
                    {
                        displayProperties.Add(new PropertyModel(prop.Key, prop.Value, false, 0));
                    }
                    if (prop.Value is KeyProperty)
                    {
                        KeyProperty kp = prop.Value as KeyProperty;
                        if (kp.TypeId == 0x2f7d0004 || kp.TypeId == 0x3f8662ea)//PNG
                        {
                            DatabaseIndex imageIndex = DatabaseManager.Instance.Indices.Find(idx => idx.InstanceId == kp.InstanceId && idx.GroupContainer == kp.GroupContainer && idx.TypeId == kp.TypeId);
                            if (imageIndex != null)
                            {
                                Image imageControl = new Image();
                                byte[] imgdata = imageIndex.GetIndexData(true);
                                using (MemoryStream imageByteStream = new MemoryStream(imgdata))
                                {
                                    try
                                    {
                                        BitmapImage image = new BitmapImage();
                                        image.BeginInit();
                                        image.StreamSource = imageByteStream;
                                        image.CacheOption = BitmapCacheOption.OnLoad;
                                        image.EndInit();
                                        imageControl.Source = image;
                                        imageControl.Width = 80;
                                        imageControl.Height = 80;
                                        imageControl.Stretch = Stretch.Fill;
                                        pnlImages.Children.Add(imageControl);
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                    }
                    prop.Value.PropertyChanged += new EventHandler(Value_PropertyChanged);

                }
                ((Expander)pnlImages.Parent).Visibility = (pnlImages.Children.Count == 0) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

                dataGrid1.ItemsSource = displayProperties;

                txtStatus.Text = string.Format("Loaded {0} of {1} properties", propertyFile.Values.Count, propertyFile.PropertyCount);
            }
            catch
            {

            }
        }

        void Value_PropertyChanged(object sender, EventArgs e)
        {

            if (this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                //if the text has been changed, create a new modifiedIndex
                DatabaseIndex originalIndex = DatabaseManager.Instance.Find(i => i.TypeId == index.Index.TypeId &&
                                                             i.GroupContainer == index.Index.GroupContainer &&
                                                             i.InstanceId == index.Index.InstanceId);
                originalIndex.IsModified = true;
                ModifiedPropertyFile propertyFile = new ModifiedPropertyFile();
                propertyFile.PropertyFile = new PropertyFile();
                propertyFile.PropertyFile.Values = FlattenToPropertiesDictionary();
                propertyFile.PropertyFile.PropertyCount = (uint)propertyFile.PropertyFile.Values.Count;
                originalIndex.ModifiedData = propertyFile;
            }
        }

        private Dictionary<uint, Property> FlattenToPropertiesDictionary()
        {
            Dictionary<uint, Property> returnDictionary = new Dictionary<uint, Property>();
            foreach (PropertyModel prop in displayProperties.OrderBy(p => p.Id).ThenBy(p => p.ArrayIndex))
            {
                if (prop.IsArray)
                {
                    if (prop.ArrayIndex == 0)
                    {
                        ArrayProperty arrayProp = new ArrayProperty();
                        arrayProp.PropertyType = prop.Value.GetType();
                        arrayProp.Values.Add(prop.Value);

                        returnDictionary.Add(prop.Id, arrayProp);
                    }
                    else
                    {
                        ArrayProperty arrayProp = (ArrayProperty)returnDictionary[prop.Id];
                        arrayProp.Values.Add(prop.Value);
                    }
                }
                else
                {
                    returnDictionary.Add(prop.Id, prop.Value);
                }

            }
            return returnDictionary;
        }

        private MainWindow getMainWindow()
        {
            Window owner = Window.GetWindow(this);
            MainWindow main = null;
            if (owner is MainWindow)
            {
                main = owner as MainWindow;
            }
            else if (owner is ViewWindow)
            {
                main = ((ViewWindow)owner).Main;
            }
            return main;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            TGIRecord record = (TGIRecord)((MenuItem)sender).DataContext;
            ViewPropertyRegistryRecord window = new ViewPropertyRegistryRecord(record);
            window.ShowDialog();
            dataGrid1.Items.Refresh();
        }

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // PropertyModel model = dataGrid1.SelectedItem as PropertyModel;
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            dataGrid1.Items.Refresh();
        }

        private void btnViewChildren_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = getMainWindow();
            if (main != null)
            {
                ViewSearchProperties res = new ViewSearchProperties(main);
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                res.txtSearchValue.Text = index.Index.InstanceId.ToHex();
                res.txtPropertyID.Text = "0x00b2cccb";
                res.startSearch();
                res.Show();
            }
        }


        private void btnDebug_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                //if the text has been changed, create a new modifiedIndex

                DatabaseIndex originalIndex = DatabaseManager.Instance.Find(i => i.TypeId == index.Index.TypeId &&
                                                             i.GroupContainer == index.Index.GroupContainer &&
                                                             i.InstanceId == index.Index.InstanceId);
                //  originalIndex.IsModified = true;
                ModifiedPropertyFile propertyFile = new ModifiedPropertyFile();
                propertyFile.PropertyFile = new PropertyFile();
                propertyFile.PropertyFile.Values = new Dictionary<uint, Property>();
                foreach (PropertyModel prop in displayProperties)
                {
                    if (prop.IsArray)
                    {
                        if (prop.ArrayIndex == 0)
                        {
                            ArrayProperty arrayProp = new ArrayProperty();
                            arrayProp.PropertyType = prop.Value.GetType();
                            arrayProp.Values.Add(prop.Value);

                            propertyFile.PropertyFile.Values.Add(prop.Id, arrayProp);
                        }
                        else
                        {
                            ArrayProperty arrayProp = (ArrayProperty)propertyFile.PropertyFile.Values[prop.Id];
                            arrayProp.Values.Add(prop.Value);
                        }
                    }
                    else
                    {
                        propertyFile.PropertyFile.Values.Add(prop.Id, prop.Value);
                    }

                }
                propertyFile.PropertyFile.PropertyCount = (uint)propertyFile.PropertyFile.Values.Count;
                //
                // originalIndex.ModifiedData = propertyFile;

                ViewHexDiff diff = new ViewHexDiff(index.Data, propertyFile.GetData());
                diff.ShowDialog();

            }

        }

        private void mnuAddArrayItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                PropertyModel m = (PropertyModel)((MenuItem)sender).DataContext;
                int arrayIndex = displayProperties.IndexOf(m);

                int lastIndex = displayProperties.Where(p => p.Id == m.Id).Max(p => p.ArrayIndex);
                int maxIndex = displayProperties.IndexOf(displayProperties.Last(p => p.Id == m.Id && p.ArrayIndex == lastIndex));

                PropertyModel newModel = new PropertyModel(m.Id, m.Value, true, lastIndex + 1);

                int indexOfProperty = displayProperties.IndexOf(m);
                displayProperties.Insert(maxIndex + 1, newModel);

                Value_PropertyChanged(this, new EventArgs());

                dataGrid1.Items.Refresh();
            }
        }

        private void mnuRemoveArrayItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                PropertyModel m = (PropertyModel)((MenuItem)sender).DataContext;

                int arrayItemCount = displayProperties.Where(p => p.Id == m.Id).Count();

                if (arrayItemCount > 1)
                {
                    displayProperties.Remove(m);

                    //recalculate array indices

                    List<PropertyModel> arrayProperties = displayProperties.Where(p => p.Id == m.Id).ToList();
                    for (int i = 0; i < (arrayItemCount - 1); i++)
                    {
                        arrayProperties[i].ArrayIndex = i;
                    }
                }

                Value_PropertyChanged(this, new EventArgs());

                dataGrid1.Items.Refresh();
            }
        }

        private void btnAddProperty_Click(object sender, RoutedEventArgs e)
        {
            ViewAddProperty dlg = new ViewAddProperty();
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                AddProperty(dlg.PropertyId, PropertyFile.AddProperty(dlg.PropertyId, dlg.PropertyType, dlg.IsArray));
                Value_PropertyChanged(this, new EventArgs());
                dataGrid1.Items.Refresh();
            }
        }

        private void mnuDeleteProperty_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this property?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                //delete all the seleted properties
                foreach (PropertyModel model in dataGrid1.SelectedItems)
                {
                    displayProperties.RemoveAll(p => p.Id == model.Id);

                }
                //PropertyModel m = (PropertyModel)((MenuItem)sender).DataContext;

                //displayProperties.RemoveAll(p => p.Id == m.Id);
                Value_PropertyChanged(this, new EventArgs());
                dataGrid1.Items.Refresh();
            }
        }

        private void btnLightsEditor_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;

                ViewLotEditor editor = new ViewLotEditor(index.Index, FlattenToPropertiesDictionary());
                if (editor.ShowDialog().GetValueOrDefault(false))
                {
                    //     displayProperties = editor.Properties;
                    //Value_PropertyChanged(this, new EventArgs());

                    //if the text has been changed, create a new modifiedIndex
                    DatabaseIndex originalIndex = DatabaseManager.Instance.Find(i => i.TypeId == index.Index.TypeId &&
                                                                 i.GroupContainer == index.Index.GroupContainer &&
                                                                 i.InstanceId == index.Index.InstanceId);
                    originalIndex.IsModified = true;
                    ModifiedPropertyFile newpropertyFile = new ModifiedPropertyFile();
                    newpropertyFile.PropertyFile = new PropertyFile();
                    newpropertyFile.PropertyFile.Values = editor.UnitFileEntry.Save();
                    newpropertyFile.PropertyFile.PropertyCount = (uint)newpropertyFile.PropertyFile.Values.Count;
                    originalIndex.ModifiedData = newpropertyFile;

                    ReloadDisplayProperties(newpropertyFile.PropertyFile.Values);

                }
            }


        }

        private void mnuCopyProperties_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid1.SelectedItems != null)
            {
                List<PropertyModel> selectedProperties = new List<PropertyModel>();
                foreach (PropertyModel model in dataGrid1.SelectedItems)
                {
                    selectedProperties.Add(model);
                }

                AppClipboard.Instance.Properties = selectedProperties;
            }
        }

        private void mnuPasteProperties_Click(object sender, RoutedEventArgs e)
        {
            object data = AppClipboard.Instance.Properties;
            if (data.GetType() == typeof(List<PropertyModel>))
            {
                List<PropertyModel> selectedProperties = data as List<PropertyModel>;

                foreach (PropertyModel model in selectedProperties)
                {
                    displayProperties.Add(model);
                }

                Value_PropertyChanged(this, new EventArgs());
                dataGrid1.Items.Refresh();
            }
        }

        private void mnuInsertArrayItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                PropertyModel m = (PropertyModel)((MenuItem)sender).DataContext;
                int arrayIndex = displayProperties.IndexOf(m);

                int lastIndex = displayProperties.Where(p => p.Id == m.Id).Max(p => p.ArrayIndex);
                int maxIndex = displayProperties.IndexOf(displayProperties.Last(p => p.Id == m.Id && p.ArrayIndex == lastIndex));

                PropertyModel newModel = new PropertyModel(m.Id, m.Value, true, lastIndex + 1);

                int indexOfProperty = displayProperties.IndexOf(m);
                displayProperties.Insert(arrayIndex, newModel);

                List<PropertyModel> arrayProperties = displayProperties.Where(p => p.Id == m.Id).ToList();
                for (int i = 0; i < (arrayProperties.Count - 1); i++)
                {
                    arrayProperties[i].ArrayIndex = i;
                }

                Value_PropertyChanged(this, new EventArgs());

                dataGrid1.Items.Refresh();
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //Flatten the inheritance of this property file
            Dictionary<uint, Property> flattenedProperties = new Dictionary<uint, Property>();
            flattenedProperties = GetParentProperties(flattenedProperties, FlattenToPropertiesDictionary());
            if (flattenedProperties.ContainsKey(0x0A0209B2))
            {
                ArrayProperty excludedProperties = flattenedProperties[0x0A0209B2] as ArrayProperty;
                if (flattenedProperties.ContainsKey(0x0A0209A2))
                {
                    ArrayProperty prop = flattenedProperties[0x0A0209A2] as ArrayProperty;
                    foreach (KeyProperty keyProp in excludedProperties.Values)
                    {
                        prop.Values.RemoveAll(kp => (kp as KeyProperty).InstanceId == keyProp.InstanceId);
                    }
                }
                if (flattenedProperties.ContainsKey(0x0A0209A3))
                {
                    ArrayProperty prop = flattenedProperties[0x0A0209A3] as ArrayProperty;
                    foreach (KeyProperty keyProp in excludedProperties.Values)
                    {
                        prop.Values.RemoveAll(kp => (kp as KeyProperty).InstanceId == keyProp.InstanceId);
                    }
                }
                if (flattenedProperties.ContainsKey(0x0A0209A4))
                {
                    ArrayProperty prop = flattenedProperties[0x0A0209A4] as ArrayProperty;
                    foreach (KeyProperty keyProp in excludedProperties.Values)
                    {
                        prop.Values.RemoveAll(kp => (kp as KeyProperty).InstanceId == keyProp.InstanceId);
                    }
                }
                if (flattenedProperties.ContainsKey(0x0A0209A5))
                {
                    ArrayProperty prop = flattenedProperties[0x0A0209A5] as ArrayProperty;
                    foreach (KeyProperty keyProp in excludedProperties.Values)
                    {
                        prop.Values.RemoveAll(kp => (kp as KeyProperty).InstanceId == keyProp.InstanceId);
                    }
                }
                flattenedProperties.Remove(0x0A0209B2);
            }

            ReloadDisplayProperties(flattenedProperties);

            Value_PropertyChanged(this, new EventArgs());

            dataGrid1.Items.Refresh();
        }

        public Dictionary<uint, Property> GetParentProperties(Dictionary<uint, Property> returnProperties, Dictionary<uint, Property> parentProperties)
        {
            if (returnProperties == null)
            {
                returnProperties = new Dictionary<uint, Property>();
            }

            //add keys that didn't exist yet
            foreach (uint index in parentProperties.Keys)
            {
                if (!returnProperties.ContainsKey(index) && index != 0x00B2CCCB)
                {
                    //This property specifies behavior bundles to exclude - instead of adding this property, remove all the bundles from the appropriate arrays

                    returnProperties.Add(index, parentProperties[index]);

                }


            }

            if (parentProperties.ContainsKey(0x00B2CCCB))
            {
                Property parentProperty = parentProperties[0x00B2CCCB];
                KeyProperty parentPropertyValue = parentProperty as KeyProperty;

                //search the currently opened package for this type/instance group id. if it doesn't exist, find it in the default simcity packages

                DatabaseIndex parentIndex = null;
                if (DatabaseManager.Instance.Indices.Exists(i => i.InstanceId == parentPropertyValue.InstanceId && i.TypeId == parentPropertyValue.TypeId && i.GroupContainer == parentPropertyValue.GroupContainer))
                {
                    parentIndex = DatabaseManager.Instance.Indices.First(i => i.InstanceId == parentPropertyValue.InstanceId && i.TypeId == parentPropertyValue.TypeId && i.GroupContainer == parentPropertyValue.GroupContainer);
                }
                else
                {
                    string folderEcoGamePath = Properties.Settings.Default.SimCityFolder + @"\SimCityUserData\EcoGame\";
                    DirectoryInfo folderEcoGame = new DirectoryInfo(folderEcoGamePath);

                    //search the simcity data packages for this type/instance group id. if it doesn't exist, find it in the default simcity packages
                    FileInfo[] scriptsFiles = folderEcoGame.GetFiles("SimCity-Scripts_*.package", SearchOption.TopDirectoryOnly);
                    FileInfo scriptsFile = scriptsFiles.First(sf => sf.LastWriteTime == scriptsFiles.Max(sf2 => sf2.LastWriteTime));
                    DatabasePackedFile ScriptsPackage = DatabasePackedFile.LoadFromFile(scriptsFile.FullName);

                    parentIndex = ScriptsPackage.Indices.FirstOrDefault(i => i.InstanceId == parentPropertyValue.InstanceId && i.TypeId == parentPropertyValue.TypeId && i.GroupContainer == parentPropertyValue.GroupContainer);
                }

                // if(parentIndex != null)
                // {
                byte[] data = parentIndex.GetIndexData(true);

                using (Stream s = new MemoryStream(data, 0, data.Length))
                {
                    PropertyFile propertyFile = new PropertyFile();
                    propertyFile.Read(s);

                    return GetParentProperties(returnProperties, propertyFile.Values);
                }



                throw new Exception("Inheritance not found!");
                return returnProperties;
            }
            else
            {
                return returnProperties;
            }
        }

        private void mnuCopy_Click(object sender, RoutedEventArgs e)
        {
            //check if current index is a valid menu item
            if (this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                DatabaseIndexData menuItemIndex = (DatabaseIndexData)this.DataContext;
                menuItemIndex.Index.IsModified = true;
                if (menuItemIndex.Index.InstanceType == (uint)PropertyFileTypeIds.Menu2)
                {
                    SaveFileDialog dialog = new SaveFileDialog();
                    if (dialog.ShowDialog().GetValueOrDefault(false))
                    {
                        List<DatabaseIndex> indices = new List<DatabaseIndex>();

                        menuItemIndex.Index.ModifiedInstanceId = TGIRandomGenerator.GetNext();
                        indices.Add(menuItemIndex.Index);

                        PropertyFile menuFile = new PropertyFile(menuItemIndex.Index);

                        #region Extract Icon

                        if (menuFile.Values.ContainsKey(PropertyConstants.menuIcon))
                        {
                            KeyProperty iconProperty = (KeyProperty)menuFile.Values[PropertyConstants.menuIcon];
                            DatabaseIndex iconIndex = DatabaseIndex.FindIndex(iconProperty.TypeId, null, null, iconProperty.InstanceId, true, false);

                            iconIndex.ModifiedInstanceId = TGIRandomGenerator.GetNext();
                            indices.Add(iconIndex);

                            iconProperty.InstanceId = iconIndex.ModifiedInstanceId.Value;
                            menuItemIndex.Index.ModifiedData = new ModifiedPropertyFile(){ PropertyFile = menuFile };
                            
                            menuFile.Values[PropertyConstants.menuIcon] = iconProperty;
                           
                        }

                        #endregion

                        #region Extract Marquee Image

                        if (menuFile.Values.ContainsKey(PropertyConstants.menuMarqueeImage))
                        {
                            KeyProperty marqueeProperty = (KeyProperty)menuFile.Values[PropertyConstants.menuMarqueeImage];
                            DatabaseIndex marqueeIndex = DatabaseIndex.FindIndex(marqueeProperty.TypeId, null, null, marqueeProperty.InstanceId, true, false);

                            marqueeIndex.ModifiedInstanceId = TGIRandomGenerator.GetNext();
                            indices.Add(marqueeIndex);

                            marqueeProperty.InstanceId = marqueeIndex.ModifiedInstanceId.Value;
                            menuItemIndex.Index.ModifiedData = new ModifiedPropertyFile() { PropertyFile = menuFile };

                            menuFile.Values[PropertyConstants.menuMarqueeImage] = marqueeProperty;

                        }

                        #endregion

                        #region Extract Ecogame Unit

                        if (menuFile.Values.ContainsKey(PropertyConstants.menuUnit))
                        {
                            KeyProperty unitProperty = (KeyProperty)menuFile.Values[PropertyConstants.menuUnit];
                            DatabaseIndex unitIndex = DatabaseIndex.FindIndex((uint)TypeIds.PropertyFile, null, 0xe0, unitProperty.InstanceId, false, true);

                            PropertyFile unitFile = new PropertyFile(unitIndex);
                            unitFile.FlattenParentInheritance();

                            #region Extract Unit Display

                            if (unitFile.Values.ContainsKey(PropertyConstants.ecoGameUnitDisplayModel))
                            {
                                KeyProperty unitDisplayProperty = (KeyProperty)unitFile.Values[PropertyConstants.ecoGameUnitDisplayModel];
                                DatabaseIndex unitDisplayIndex = DatabaseIndex.FindIndex((uint)TypeIds.PropertyFile, null, 0xe1, unitDisplayProperty.InstanceId, false, true);

                                unitDisplayIndex.ModifiedInstanceId = TGIRandomGenerator.GetNext();
                                unitDisplayProperty.InstanceId = unitDisplayIndex.ModifiedInstanceId.Value;

                                unitFile.Values[PropertyConstants.ecoGameUnitDisplayModel] = unitDisplayProperty;

                                PropertyFile unitDisplayFile = new PropertyFile(unitDisplayIndex);

                                #region Extract LODs

                                if (unitDisplayFile.Values.ContainsKey(PropertyConstants.UnitLOD1))
                                {
                                    KeyProperty LOD1Property = (KeyProperty)unitDisplayFile.Values[PropertyConstants.UnitLOD1];
                                    DatabaseIndex LOD1Index = DatabaseIndex.FindIndex(LOD1Property.TypeId, null, null, LOD1Property.InstanceId, true, false);
                                    LOD1Index.ModifiedInstanceId = TGIRandomGenerator.GetNext();
                                    indices.Add(LOD1Index);
                                    LOD1Property.InstanceId = LOD1Index.ModifiedInstanceId.Value;
                                    unitDisplayFile.Values[PropertyConstants.UnitLOD1] = LOD1Property;
                                }

                                if (unitDisplayFile.Values.ContainsKey(PropertyConstants.UnitLOD2))
                                {
                                    KeyProperty LOD2Property = (KeyProperty)unitDisplayFile.Values[PropertyConstants.UnitLOD2];
                                    DatabaseIndex LOD2Index = DatabaseIndex.FindIndex(LOD2Property.TypeId, null, null, LOD2Property.InstanceId, true, false);
                                    LOD2Index.ModifiedInstanceId = TGIRandomGenerator.GetNext();
                                    indices.Add(LOD2Index);
                                    LOD2Property.InstanceId = LOD2Index.ModifiedInstanceId.Value;
                                    unitDisplayFile.Values[PropertyConstants.UnitLOD2] = LOD2Property;
                                }

                                if (unitDisplayFile.Values.ContainsKey(PropertyConstants.UnitLOD3))
                                {
                                    KeyProperty LOD3Property = (KeyProperty)unitDisplayFile.Values[PropertyConstants.UnitLOD3];
                                    DatabaseIndex LOD3Index = DatabaseIndex.FindIndex(LOD3Property.TypeId, null, null, LOD3Property.InstanceId, true, false);
                                    LOD3Index.ModifiedInstanceId = TGIRandomGenerator.GetNext();
                                    indices.Add(LOD3Index);
                                    LOD3Property.InstanceId = LOD3Index.ModifiedInstanceId.Value;
                                    unitDisplayFile.Values[PropertyConstants.UnitLOD3] = LOD3Property;
                                }

                                if (unitDisplayFile.Values.ContainsKey(PropertyConstants.UnitLOD4))
                                {
                                    KeyProperty LOD4Property = (KeyProperty)unitDisplayFile.Values[PropertyConstants.UnitLOD4];
                                    DatabaseIndex LOD4Index = DatabaseIndex.FindIndex(LOD4Property.TypeId, null, null, LOD4Property.InstanceId, true, false);
                                    LOD4Index.ModifiedInstanceId = TGIRandomGenerator.GetNext();
                                    indices.Add(LOD4Index);
                                    LOD4Property.InstanceId = LOD4Index.ModifiedInstanceId.Value;
                                    unitDisplayFile.Values[PropertyConstants.UnitLOD4] = LOD4Property;
                                }


                                #endregion

                                unitDisplayIndex.ModifiedData = new ModifiedPropertyFile() { PropertyFile = unitDisplayFile };
                                unitDisplayIndex.IsModified = true;

                                indices.Add(unitDisplayIndex);
                            }

                            #endregion

                            unitIndex.ModifiedInstanceId = TGIRandomGenerator.GetNext();
                            unitIndex.ModifiedData = new ModifiedPropertyFile() { PropertyFile = unitFile };
                            unitIndex.IsModified = true;

                            indices.Add(unitIndex);

                            unitProperty.InstanceId = unitIndex.ModifiedInstanceId.Value;
                            menuItemIndex.Index.ModifiedData = new ModifiedPropertyFile() { PropertyFile = menuFile };

                            menuFile.Values[PropertyConstants.menuUnit] = unitProperty;
                        }
                     
                        #endregion
                        DatabasePackedFile.SaveAs(dialog.FileName, indices);
                    }
                }
                else
                {
                    MessageBox.Show("This only works for menu items...");
                }
            }
        }
    }
}
