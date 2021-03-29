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
using System.Collections.ObjectModel;
using SimCityPak.PackageReader;
using Gibbed.Spore.Package;

namespace SimCityPak.Views.AdvancedEditors
{

    /// <summary>
    /// Interaction logic for ViewPath.xaml
    /// </summary>
    public partial class ViewPath : UserControl
    {
     

        private PropertyFile propertyFile;
        private PathFile PathFileEntry { get; set; }

        public ViewPath()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                Dictionary<uint, Property> propertyContainer;

                try
                {
                    MemoryStream byteStream = new MemoryStream(index.Data);

                    propertyFile = new PropertyFile();
                    propertyFile.Read(byteStream);
                    propertyContainer = propertyFile.Values;
                    
                    PathFileEntry = new PathFile();
                    PathFileEntry.Load(propertyFile.Values);

                    ContainerGrid.DataContext = PathFileEntry;
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

        private void ViewPathEntry_OnDelete(object sender, RoutedEventArgs e)
        {
            ViewPathEntry pathEntry = e.OriginalSource as ViewPathEntry;
            PathElement element = pathEntry.DataContext as PathElement;
            PathFileEntry.PathElements.Remove(element);

            UpdateFile();
        }

        private void UpdateFile()
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
                propertyFile.PropertyFile.Values = PathFileEntry.Save();
                propertyFile.PropertyFile.PropertyCount = (uint)propertyFile.PropertyFile.Values.Count;
                originalIndex.ModifiedData = propertyFile; 
                originalIndex.Compressed = false;
            }
        }

        private void ViewPathEntry_OnCopy(object sender, RoutedEventArgs e)
        {
            ViewPathEntry pathEntry = e.OriginalSource as ViewPathEntry;
            PathElement element = pathEntry.DataContext as PathElement;

            PathElement newElement = new PathElement();
            newElement.ModelKeyProperty = element.ModelKeyProperty;
            newElement.SweepOffsetProperty = element.SweepOffsetProperty;
            newElement.SweepIntervalProperty = element.SweepIntervalProperty;
            newElement.SweepLengthProperty = element.SweepLengthProperty;
            newElement.SweepStepIntervalProperty = element.SweepStepIntervalProperty;

            newElement.MeshOffsetProperty = element.MeshOffsetProperty;

            newElement.DistortProperty = element.DistortProperty;
            newElement.RepeatUVProperty = element.RepeatUVProperty;

            newElement.ComponentTag1Property = element.ComponentTag1Property;
            newElement.ComponentTag2Property = element.ComponentTag2Property;
            newElement.ComponentTag3Property = element.ComponentTag3Property;

            newElement.RoundSweepIntervalProperty = element.RoundSweepIntervalProperty;

            newElement.SweepEndOffsetProperty = element.SweepEndOffsetProperty;

            newElement.MeshScaleProperty = element.MeshScaleProperty;

            newElement.MeshRotationProperty = element.MeshRotationProperty;

            newElement.ComponentIdProperty = element.ComponentIdProperty;
            PathFileEntry.PathElements.Add(newElement);

            UpdateFile();
        }

        private void txtMaterialId_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFile();
        }
    }


    public class PathFile : PropertyFileObject
    {
        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshMaterialId)]
        public KeyProperty MaterialIdProperty { get; set; }
        public string MaterialId
        {
            get
            {
                return string.Format("0x{0:x8}", MaterialIdProperty.InstanceId);
            }
            set
            {
                try
                {
                    if (value.StartsWith("0x"))
                    {
                        MaterialIdProperty.InstanceId = Convert.ToUInt32(value.Substring(2), 16);
                        OnPropertyChanged("MaterialId");
                        OnPropertyChanged("MaterialIdProperty");
                    }
                }
                catch
                {

                }
            }
        }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.Parent, Optional=true)]
        public KeyProperty ParentProperty { get; set; }
        public string Parent
        {
            get
            {
                return ParentProperty != null ? string.Format("0x{0:x8}", ParentProperty.InstanceId) : null;
            }
            set
            {
                try
                {
                    if (value.StartsWith("0x"))
                    {
                        ParentProperty.InstanceId = Convert.ToUInt32(value.Substring(2), 16);
                        OnPropertyChanged("Parent");
                        OnPropertyChanged("ParentProperty");
                    }
                }
                catch
                {

                }
            }
        }

        [PropertyFileObjectCollection()]
        public ObservableCollection<PathElement> PathElements { get; set; }
    }

    public class PathElement : PropertyFileObject
    {
        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshModelKeys)]
        public KeyProperty ModelKeyProperty { get; set; }
        public string ModelKey
        {
            get
            {
                return string.Format("0x{0:x8}", ModelKeyProperty.InstanceId);
            }
            set
            {
                try
                {
                    if (value.StartsWith("0x"))
                    {
                        ModelKeyProperty.InstanceId = Convert.ToUInt32(value.Substring(2), 16);
                    }
                }
                catch
                {

                }
            }
        }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshComponentIds)]
        public KeyProperty ComponentIdProperty { get; set; }
        public string ComponentId
        {
            get
            {
                return string.Format("0x{0:x8}", ComponentIdProperty.InstanceId);
            }
            set
            {
                try
                {
                    if (value.StartsWith("0x"))
                    {
                        ComponentIdProperty.InstanceId = Convert.ToUInt32(value.Substring(2), 16);
                    }
                }
                catch
                {

                }
            }
        }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshSweepOffsets)]
        public FloatProperty SweepOffsetProperty { get; set; }
        public float SweepOffset
        { get { return SweepOffsetProperty.Value; } set { SweepOffsetProperty.Value = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshSweepIntervals)]
        public FloatProperty SweepIntervalProperty { get; set; }
        public float SweepInterval
        { get { return SweepIntervalProperty.Value; } set { SweepIntervalProperty.Value = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshSweepLengths)]
        public FloatProperty SweepLengthProperty { get; set; }
        public float SweepLength
        { get { return SweepLengthProperty.Value; } set { SweepLengthProperty.Value = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshSweepStepIntervals)]
        public FloatProperty SweepStepIntervalProperty { get; set; }
        public float SweepStepInterval
        { get { return SweepStepIntervalProperty.Value; } set { SweepStepIntervalProperty.Value = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshOffsets)]
        public Vector3Property MeshOffsetProperty { get; set; }
        public float MeshOffsetX { get { return MeshOffsetProperty.X; } set { MeshOffsetProperty.X = value; } }
        public float MeshOffsetY { get { return MeshOffsetProperty.Y; } set { MeshOffsetProperty.Y = value; } }
        public float MeshOffsetZ { get { return MeshOffsetProperty.Z; } set { MeshOffsetProperty.Z = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshDistorts)]
        public BoolProperty DistortProperty { get; set; }
        public bool Distort { get { return DistortProperty.Value; } set { DistortProperty.Value = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshRepeatUVs)]
        public BoolProperty RepeatUVProperty { get; set; }
        public bool RepeatUV { get { return RepeatUVProperty.Value; } set { RepeatUVProperty.Value = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshComponentTags1)]
        public KeyProperty ComponentTag1Property { get; set; }
        public string ComponentTag1 
        {
            get
            {
                return string.Format("0x{0:X8}", ComponentTag1Property.InstanceId);
            }
            set
            {
                try
                {
                    if (value.StartsWith("0x"))
                    {
                        ComponentTag1Property.InstanceId = Convert.ToUInt32(value.Substring(2), 16);
                    }
                }
                catch
                {

                }
            }
        }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshComponentTags2)]
        public KeyProperty ComponentTag2Property { get; set; }
        public string ComponentTag2
        {
            get
            {
                return string.Format("0x{0:X8}", ComponentTag2Property.InstanceId);
            }
            set
            {
                try
                {
                    if (value.StartsWith("0x"))
                    {
                        ComponentTag2Property.InstanceId = Convert.ToUInt32(value.Substring(2), 16);
                    }
                }
                catch
                {

                }
            }
        }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshComponentTags3)]
        public KeyProperty ComponentTag3Property { get; set; }
        public string ComponentTag3
        {
            get
            {
                return string.Format("0x{0:X8}", ComponentTag3Property.InstanceId);
            }
            set
            {
                try
                {
                    if (value.StartsWith("0x"))
                    {
                        ComponentTag3Property.InstanceId = Convert.ToUInt32(value.Substring(2), 16);
                    }
                }
                catch
                {

                }
            }
        }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshRoundSweepIntervals, Optional = true)]
        public BoolProperty RoundSweepIntervalProperty { get; set; }
        public bool? RoundSweepInterval { get { return RoundSweepIntervalProperty != null ? RoundSweepIntervalProperty.Value : (bool?)null; } set { RoundSweepIntervalProperty.Value = value.Value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshSweepEndOffsets)]
        public FloatProperty SweepEndOffsetProperty { get; set; }
        public float SweepEndOffset { get { return SweepEndOffsetProperty.Value; } set { SweepEndOffsetProperty.Value = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshScales)]
        public Vector3Property MeshScaleProperty { get; set; }
        public float MeshScaleX { get { return MeshScaleProperty.X; } set { MeshScaleProperty.X = value; } }
        public float MeshScaleY { get { return MeshScaleProperty.Y; } set { MeshScaleProperty.Y = value; } }
        public float MeshScaleZ { get { return MeshScaleProperty.Z; } set { MeshScaleProperty.Z = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.proceduralMeshRotations)]
        public Vector3Property MeshRotationProperty { get; set; }
        public float MeshRotationX { get { return MeshRotationProperty.X; } set { MeshRotationProperty.X = value; } }
        public float MeshRotationY { get { return MeshRotationProperty.Y; } set { MeshRotationProperty.Y = value; } }
        public float MeshRotationZ { get { return MeshRotationProperty.Z; } set { MeshRotationProperty.Z = value; } }
    }
}
