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
using System.IO;
using Gibbed.Spore.Properties;
using SimCityPak.PackageReader;
using Gibbed.Spore.Package;
using SimCityPak.Views.AdvancedEditors.DecalDictionary;

namespace SimCityPak.Views.AdvancedEditors
{
    /// <summary>
    /// Interaction logic for ViewDecalDictionary.xaml
    /// </summary>
    public partial class ViewDecalDictionary : UserControl
    {
        public ViewDecalDictionary()
        {
            Decals = new ObservableList<DecalImageModel>();
            InitializeComponent();
        }

        public DecalImageDictionary DecalDictionaryEntry { get; set; }
        private PropertyFile propertyFile;
        public ObservableList<DecalImageModel> Decals { get; set; }
        DatabaseIndexData index;

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(DatabaseIndexData))
            {

                Decals.Clear();

                index = (DatabaseIndexData)this.DataContext;
                Dictionary<uint, Property> propertyContainer;

                try
                {
                    MemoryStream byteStream = new MemoryStream(index.Data);

                    propertyFile = new PropertyFile();
                    propertyFile.Read(byteStream);
                    propertyContainer = propertyFile.Values;

                    DecalDictionaryEntry = new DecalImageDictionary();
                    DecalDictionaryEntry.Load(propertyContainer);

                    gridDetails.DataContext = DecalDictionaryEntry;

                    DecalDictionaryEntry.DecalImages.ForEach(d => d.RefreshPreview());

                    Decals.AddRange(DecalDictionaryEntry.DecalImages);

                              var expression = txtMaterialId.GetBindingExpression(TextBox.TextProperty);
                    if (expression != null) expression.UpdateTarget();

                    expression = txtTextureSizeX.GetBindingExpression(TextBox.TextProperty);
                    if (expression != null) expression.UpdateTarget();

                    expression = txtTextureSizeY.GetBindingExpression(TextBox.TextProperty);
                    if (expression != null) expression.UpdateTarget();

                    expression = txtAtlasSizeX.GetBindingExpression(TextBox.TextProperty);
                    if (expression != null) expression.UpdateTarget();

                    expression = txtAtlasSizeY.GetBindingExpression(TextBox.TextProperty);
                    if (expression != null) expression.UpdateTarget();




                }
                catch (Exception ex)
                {


                    Exception subEx = ex.InnerException;
                    while (subEx != null)
                    {
                        //    exceptionMessage += subEx.Message + Environment.NewLine;
                        subEx = ex.InnerException;
                    }

                }

            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuEditDecal_Click(object sender, RoutedEventArgs e)
        {
            ViewEditDecalDictionaryEntry editWindow = new ViewEditDecalDictionaryEntry((sender as MenuItem).DataContext as DecalImageModel);
            editWindow.ShowDialog();
            DecalImageModel img = editWindow.DecalImage;
            img.RefreshPreview();

            Refresh();



        }

        private void Refresh()
        {
            //update properties 
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

            return DecalDictionaryEntry.Save();
        }

        private void mnuGenerateDecal_Click(object sender, RoutedEventArgs e)
        {
            ViewDecalCreator creator = new ViewDecalCreator(index.Index);
            if (creator.ShowDialog().GetValueOrDefault())
            {
                Decals.Add(creator.Decal);
                DecalDictionaryEntry.DecalImages.Add(creator.Decal);

                Decals.Changed();

                Refresh();
            }
        }

        private void mnuAddDecal_Click(object sender, RoutedEventArgs e)
        {
            DecalImageModel decal = new DecalImageModel();
            decal.IdProperty = TGIRandomGenerator.GetNext().ToHex();
            decal.AspectRatioProperty = 1;
            
            Decals.Add(decal);
            DecalDictionaryEntry.DecalImages.Add(decal);

            Decals.Changed();

            Refresh();
        }

        private void mnuDeleteDecal_Click(object sender, RoutedEventArgs e)
        {
            DecalImageModel img = (sender as MenuItem).DataContext as DecalImageModel;

            Decals.Remove(img);
            DecalDictionaryEntry.DecalImages.Remove(img);

            Decals.Changed();

            Refresh();
        }
    }

    public class DecalImageDictionary : PropertyFileObject
    {
        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.DecalDictionaryMaterial, PropertyType = typeof(KeyProperty))]
        public KeyProperty MaterialId { get; set; }

        public string MaterialIdProperty
        {
            get { return string.Format("0x{0:X8}", MaterialId.InstanceId); }
            set
            {
                try
                {
                    if (value.StartsWith("0x"))
                    {
                        MaterialId.InstanceId = Convert.ToUInt32(value.Substring(2), 16);
                    }
                }
                catch { }
            }
        }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.DecalDictionaryTextureSize, PropertyType = typeof(Vector2Property))]
        public Vector2Property TextureSize { get; set; }

        public float TextureSizeX { get { return TextureSize.X; } set { TextureSize.X = value; } }
        public float TextureSizeY { get { return TextureSize.Y; } set { TextureSize.Y = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.DecalDictionaryAtlasSize, PropertyType = typeof(Vector2Property))]
        public Vector2Property AtlasSize { get; set; }

        public float AtlasSizeX { get { return AtlasSize.X; } set { AtlasSize.X = value; } }
        public float AtlasSizeY { get { return AtlasSize.Y; } set { AtlasSize.Y = value; } }

        [PropertyFileObjectCollectionAttribute()]
        public ObservableList<DecalImageModel> DecalImages { get; set; }
    }

    public class DecalImageModel : PropertyFileObject
    {
        public DecalImageModel()
        {
            ID = new KeyProperty();
            DecalID = new KeyProperty();
            AspectRatio = new FloatProperty();
            Color1Property = new Vector4Property();
            Color2Property = new Vector4Property();
            Color3Property = new Vector4Property();
            Color4Property = new Vector4Property();


        }

        public WriteableBitmap ImageSource { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.DecalDictionaryDecalID)]
        public KeyProperty ID { get; set; }
        public string IdProperty
        {
            get { return string.Format("0x{0:X8}", ID.InstanceId); }
            set
            {
                try
                {
                    if (value.StartsWith("0x"))
                    {
                        ID.InstanceId = Convert.ToUInt32(value.Substring(2), 16);
                    }
                }
                catch { }
            }
        }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.DecalDictionaryDecalImage)]
        public KeyProperty DecalID { get; set; }
        public string DecalIdProperty
        {
            get { return string.Format("0x{0:X8}", DecalID.InstanceId); }
            set
            {
                try
                {
                    if (value.StartsWith("0x"))
                    {
                        DecalID.InstanceId = Convert.ToUInt32(value.Substring(2), 16);
                        RefreshPreview();
                    }
                }
                catch { }
            }
        }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.DecalDictionaryDecalRatio)]
        public FloatProperty AspectRatio { get; set; }
        public float AspectRatioProperty { get { return AspectRatio.Value; } set { AspectRatio.Value = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.DecalDictionaryDecalColor1)]
        public Vector4Property Color1Property { get; set; }
        public Color Color1
        {
            get { return Color.FromScRgb(1, Color1Property.X * 2, Color1Property.Y * 2, Color1Property.Z * 2); }
            set
            {
                Color1Property.X = value.ScR / 2;
                Color1Property.Y = value.ScG / 2;
                Color1Property.Z = value.ScB / 2;
            }
        }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.DecalDictionaryDecalColor2)]
        public Vector4Property Color2Property { get; set; }
        public Color Color2
        {
            get { return Color.FromScRgb(1, Color2Property.X * 2, Color2Property.Y * 2, Color2Property.Z * 2); }
            set
            {
                Color2Property.X = value.ScR / 2;
                Color2Property.Y = value.ScG / 2;
                Color2Property.Z = value.ScB / 2;
            }
        }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.DecalDictionaryDecalColor3)]
        public Vector4Property Color3Property { get; set; }
        public Color Color3
        {
            get { return Color.FromScRgb(1, Color3Property.X * 2, Color3Property.Y * 2, Color3Property.Z * 2); }
            set
            {
                Color3Property.X = value.ScR / 2;
                Color3Property.Y = value.ScG / 2;
                Color3Property.Z = value.ScB / 2;
            }
        }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.DecalDictionaryDecalColor4)]
        public Vector4Property Color4Property { get; set; }
        public Color Color4
        {
            get { return Color.FromScRgb(1, Color4Property.X * 2, Color4Property.Y * 2, Color4Property.Z * 2); }
            set
            {
                Color4Property.X = value.ScR / 2;
                Color4Property.Y = value.ScG / 2;
                Color4Property.Z = value.ScB / 2;
            }
        }

        public float Glow1 { get; set; }
        public float Glow2 { get; set; }
        public float Glow3 { get; set; }
        public float Glow4 { get; set; }

        public void RefreshPreview()
        {
            try
            {
                DatabaseIndex imageIndex = DatabaseManager.Instance.Indices.Find(idx => idx.InstanceId == DecalID.InstanceId && idx.TypeId == PropertyConstants.RasterImageType);
                if (imageIndex != null)
                {
                    using (MemoryStream imageByteStream = new MemoryStream(imageIndex.GetIndexData(true)))
                    {
                        RasterImage img = RasterImage.CreateFromStream(imageByteStream, RasterChannel.Preview, Color4, Color3, Color2, Color1);
                        ImageSource = img.MipMaps[0];
                        OnPropertyChanged("ImageSource");
                    }
                }
            }
            catch
            {

            }
        }

    }
}
