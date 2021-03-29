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
using SimCityPak.Views.AdvancedEditors;
using Gibbed.Spore.Properties;
using System.Windows.Media.Media3D;
using SimCityPak.PackageReader;
using Gibbed.Spore.Package;
using SporeMaster.RenderWare4;
using System.IO;
using HelixToolkit;
using HelixToolkit.Wpf;
using Microsoft.Win32;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewLightsEditor.xaml
    /// </summary>
    public partial class ViewLotEditor : Window
    {
        private CombinedManipulator manipulator = new CombinedManipulator();
        private List<PropertyModel> _properties;
        public List<PropertyModel> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        public DatabaseIndex OriginalIndex { get; set; }
        public UnitFile UnitFileEntry { get; set; }

        public static Dictionary<uint, string> LightTypes = new Dictionary<uint, string>()
                    {
                        { 0x75d4c8cd, "Point" },
                        { 0x2f0ff9fd, "Spot" },
                        { 0x0820abaf, "Line" }
                    };

        public static Dictionary<uint, string> LightCullDistances = new Dictionary<uint, string>()
                    {
                        { 0x4394551b, "Near" },
                        { 0x467e1ea9, "Mid" },
                        { 0x468f679c, "Far" },
                        { 0x3e7e124d, "Max" }
                    };

        public ViewLotEditor(DatabaseIndex index, Dictionary<uint, Property> properties)
        {
            UnitFileEntry = new UnitFile();
            UnitFileEntry.Load(properties);

            InitializeComponent();

            OriginalIndex = index;

            ContainerGrid.DataContext = UnitFileEntry;

            LoadUnitModel();
            viewPort.Children.Add(manipulator);

            viewPort.CameraMode = CameraMode.Inspect;
            viewPort.ZoomExtentsWhenLoaded = true;


        }

        void newLight_LightTypeChanged(object sender, EventArgs e)
        {
            UnitLight light = sender as UnitLight;
            light.UpdateTransform();
            viewPort.Children.Remove(light.ModelRepresentation);
            light.ModelRepresentation = light.CreateGeometry();
            viewPort.Children.Add(light.ModelRepresentation);

            lbLights.SelectedItem = light;
        }

        private void LoadUnitModel()
        {
            CreateLotModel();
            CreateUnitModel();

            UnitFileEntry.UnitEffects.ForEach(e => viewPort.Children.Add(e.CreateGeometry()));
            UnitFileEntry.UnitLights.ForEach(l => viewPort.Children.Add(l.CreateGeometry()));
            UnitFileEntry.UnitPathPoints.ForEach(l => viewPort.Children.Add(l.CreateGeometry()));
            UnitFileEntry.UnitSimsSpawners.ForEach(l => viewPort.Children.Add(l.CreateGeometry()));
            UnitFileEntry.UnitBinDrawSlots.ToList().ForEach(l => l.ForEach(b => viewPort.Children.Add(b.CreateGeometry())));
            UnitFileEntry.UnitBinDecals.ToList().ForEach(l => l.ForEach(b => viewPort.Children.Add(b.CreateGeometry())));
            //create the paths
            int i = 0;
            if (UnitFileEntry.UnitPaths.Count > 0)
            {
                foreach (UnitPath path in UnitFileEntry.UnitPaths)
                {
                    int startIndex = UnitFileEntry.UnitPath[i * 2].Value;
                    int endIndex = UnitFileEntry.UnitPath[(i * 2) + 1].Value;

                    TubeVisual3D visual = new TubeVisual3D();
                    visual.Material = MaterialHelper.CreateMaterial(Colors.Aqua);

                    foreach (UnitPathPoint point in UnitFileEntry.UnitPathPoints.Where(p => p.Index.Value <= endIndex && p.Index.Value >= startIndex))
                    {
                        visual.Path.Add(new Point3D(point.Point.X, point.Point.Y, point.Point.Z));
                    }

                    viewPort.Children.Add(visual);

                    i++;
                }
            }
        }

        private void CreateUnitModel()
        {
            if (UnitFileEntry.LOD1 != null)
            {
                //get the 3d model to show in this scene
                KeyProperty lod1 = UnitFileEntry.LOD1;
                DatabaseIndex index = DatabaseManager.Instance.Indices.Find(k => k.InstanceId == lod1.InstanceId && k.GroupContainer == lod1.GroupContainer && k.TypeId == lod1.TypeId);

                if (index != null)
                {
                    RW4Model _rw4model = new RW4Model();
                    using (Stream stream = new MemoryStream(index.GetIndexData(true)))
                    {
                        _rw4model.Read(stream);
                    }

                    RW4Section section = _rw4model.Sections.First(s => s.TypeCode == SectionTypeCodes.Mesh);
                    if (section != null)
                    {
                        SporeMaster.RenderWare4.RW4Mesh mesh = section.obj as SporeMaster.RenderWare4.RW4Mesh;

                        meshMain.TriangleIndices.Clear();
                        meshMain.Positions.Clear();
                        meshMain.Normals.Clear();
                        meshMain.TextureCoordinates.Clear();

                        try
                        {
                            foreach (var v in mesh.vertices.vertices)
                            {
                                meshMain.Positions.Add(new Point3D(v.Position.X, v.Position.Y, v.Position.Z));
                            }
                            foreach (var t in mesh.triangles.triangles)
                            {
                                meshMain.TriangleIndices.Add((int)t.i);
                                meshMain.TriangleIndices.Add((int)t.j);
                                meshMain.TriangleIndices.Add((int)t.k);
                            }
                        }
                        catch { }
                    }

                    if (UnitFileEntry.ModelSize != null)
                    {
                        ScaleTransform3D scaleTransform = new ScaleTransform3D(UnitFileEntry.ModelSize.Value, UnitFileEntry.ModelSize.Value, UnitFileEntry.ModelSize.Value);
                        modelMain.Transform = scaleTransform;

                    }


                }
            }
        }
        private void CreateLotModel()
        {
            //Create the plane representing the lot (texture & size)
            if (UnitFileEntry.LotOverlayBoxSize != null)
            {
                Vector2Property lotSize = UnitFileEntry.LotOverlayBoxSize;

                RectangleVisual3D groundPlane = new RectangleVisual3D();

                groundPlane.Width = (lotSize.X);
                groundPlane.Length = (lotSize.Y);
                groundPlane.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), -90));
                if (UnitFileEntry.LotOverlayBoxOffset != null)
                {
                    //    groundPlane.Transform = Transform3DHelper.CombineTransform(groundPlane.Transform, new TranslateTransform3D(-1 * UnitFileEntry.LotOverlayBoxOffset.X, -1 * UnitFileEntry.LotOverlayBoxOffset.Y, 0));
                }
                if (UnitFileEntry.LotPlacementTransform != null && UnitFileEntry.LotPlacementTransform.Count > 0)
                {
                    groundPlane.Transform = Transform3DHelper.CombineTransform(groundPlane.Transform, (Transform3D)new MatrixTransform3D(UnitFileEntry.LotPlacementTransform[0].GetAsMatrix3D()).Inverse);
                }

                if (UnitFileEntry.LotMask != null)
                {
                    DatabaseIndex index = DatabaseManager.Instance.Indices.Find(k => k.InstanceId == UnitFileEntry.LotMask.InstanceId && k.TypeId == 0x2f4e681c);
                    if (index != null)
                    {
                        Color color1 = Colors.Black;
                        Color color2 = Colors.Red;
                        Color color3 = Colors.Green;
                        Color color4 = Colors.Blue;

                        if (UnitFileEntry.LotColor1 != null)
                        {
                            color1 = Color.FromRgb((byte)(255 * UnitFileEntry.LotColor1.R), (byte)(255 * UnitFileEntry.LotColor1.G), (byte)(255 * UnitFileEntry.LotColor1.B));
                        }
                        if (UnitFileEntry.LotColor2 != null)
                        {
                            color2 = Color.FromRgb((byte)(255 * UnitFileEntry.LotColor2.R), (byte)(255 * UnitFileEntry.LotColor2.G), (byte)(255 * UnitFileEntry.LotColor2.B));
                        }
                        if (UnitFileEntry.LotColor3 != null)
                        {
                            color3 = Color.FromRgb((byte)(255 * UnitFileEntry.LotColor3.R), (byte)(255 * UnitFileEntry.LotColor3.G), (byte)(255 * UnitFileEntry.LotColor3.B));
                        }
                        if (UnitFileEntry.LotColor4 != null)
                        {
                            color4 = Color.FromRgb((byte)(255 * UnitFileEntry.LotColor4.R), (byte)(255 * UnitFileEntry.LotColor4.G), (byte)(255 * UnitFileEntry.LotColor4.B));
                        }

                        using (MemoryStream byteStream = new MemoryStream(index.GetIndexData(true)))
                        {
                            RasterImage rasterImage = RasterImage.CreateFromStream(byteStream, RasterChannel.Preview, color4, color3, color2, color1);
                            groundPlane.Material = new DiffuseMaterial(new ImageBrush(rasterImage.MipMaps[0]));
                        }
                    }

                    groundPlane.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Transparent));
                }

                viewPort.Children.Add(groundPlane);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            UnitFileEntry.UnitLights.ForEach(l => l.UpdateTransform());
            UnitFileEntry.UnitEffects.ForEach(eff => eff.UpdateTransform());
            UnitFileEntry.UnitPathPoints.ForEach(eff => eff.UpdateTransform());
            UnitFileEntry.UnitBinDrawSlots.ToList().ForEach(l => l.ForEach(b => b.UpdateTransform()));
            UnitFileEntry.UnitBinDecals.ToList().ForEach(l => l.ForEach(b => b.UpdateTransform()));

            UnitFileEntry.UnitSimsSpawners.ForEach(eff => eff.UpdateTransform());

            this.DialogResult = true;
            this.Close();
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerControls.Dialogs.ColorPickerFullDialog dlg = new ColorPickerControls.Dialogs.ColorPickerFullDialog();
            UnitLight light = ((UnitLight)lbLights.SelectedItem);
            dlg.InitialColor = ((UnitLight)lbLights.SelectedItem).LightColor.Color;
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                light.LightColor.Color = dlg.SelectedColor;
            }
        }

        private void lbLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbLights.SelectedItem is UnitLight)
            {
                UnitLight light = (UnitLight)lbLights.SelectedItem;
                if (light.ModelRepresentation != null)
                {
                    BindingOperations.SetBinding(manipulator, CombinedManipulator.TargetTransformProperty, new Binding("Transform") { Source = light.ModelRepresentation });
                    ResizeManipulator(light.ModelRepresentation);
                }
                contentHolder.Content = light;
            }
        }

        private void ResizeManipulator(LotEditorItemMarker model)
        {
            if (model != null)
            {
                foreach (Visual3D visual in manipulator.Children)
                {
                    double maxSize = Math.Min(20, Math.Max(Math.Max(model.ManipulatorModel.Bounds.SizeZ, model.ManipulatorModel.Bounds.SizeX), model.ManipulatorModel.Bounds.SizeY));
                    if (visual is TranslateManipulator)
                    {
                        (visual as TranslateManipulator).Length = maxSize * 2;
                        (visual as TranslateManipulator).Diameter = maxSize * 0.1;
                    }

                    if (visual is RotateManipulator)
                    {

                        (visual as RotateManipulator).Diameter = maxSize * 1.8;
                        (visual as RotateManipulator).InnerDiameter = maxSize * 1.6;

                    }
                }
            }
        }

        private void cbLightType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            UnitLight light = ((FrameworkElement)((sender as ComboBox).Parent)).DataContext as UnitLight;
            if (light != null)
            {

                light.UpdateTransform();

                if (light.ModelRepresentation != null)
                {
                    viewPort.Children.Remove(light.ModelRepresentation);
                }
                light.CreateGeometry();
                viewPort.Children.Add(light.ModelRepresentation);
                // manipulator.Bind(light.ModelRepresentation);
            }
        }

        private void btnAddLight_Click(object sender, RoutedEventArgs e)
        {
            UnitLight light = new UnitLight();
            light.LightDebugName = new String8Property() { Value = "New" };
            light.LightColor = new ColorRGBProperty() { Color = Colors.White };
            light.LightType = new KeyProperty() { InstanceId = LightTypes.First(l => l.Value == "Point").Key };
            light.LightCullDistance = new KeyProperty() { InstanceId = LightCullDistances.First(l => l.Value == "Max").Key };

            light.LightTransform = new TransformProperty();
            light.LightTransform.SetMatrix(new TranslateTransform3D(0, 0, 10).Value);
            //light.LightTransform.SetMatrix(new TranslateTransform3D(0, 0, meshMain.Bounds.SizeZ + 10).Value);

            UnitFileEntry.UnitLights.Add(light);

            light.CreateGeometry();

            viewPort.Children.Add(light.ModelRepresentation);
            UnitFileEntry.UnitLights.Changed();
        }

        private void dgProps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProps.SelectedItem is UnitBinDrawSlot)
            {
                UnitBinDrawSlot prop = (UnitBinDrawSlot)dgProps.SelectedItem;
                if (prop.ModelRepresentation != null)
                {
                    BindingOperations.SetBinding(manipulator, CombinedManipulator.TargetTransformProperty, new Binding("Transform") { Source = prop.ModelRepresentation });
                    ResizeManipulator(prop.ModelRepresentation);
                }
                contentHolder.Content = prop;
            }
        }

        private void viewPort_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Point position = e.GetPosition(viewPort);
                Visual3D visual = viewPort.FindNearestVisual(position);

                if (visual is LotEditorItemManipulator)
                {
                    LotEditorItemManipulator manipulator = visual as LotEditorItemManipulator;
                    bool found = false;
                    //if the current visual is a modelrepresentation for an object in any of the lists, select it

                    if (!found)
                    {
                        if (manipulator.Parent.Parent is UnitLight)
                        {
                            lbLights.SelectedItem = manipulator.Parent.Parent;
                            tabLights.IsSelected = true;
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        if (manipulator.Parent.Parent is UnitBinDrawSlot)
                        {
                            dgProps.SelectedItem = manipulator.Parent.Parent;
                            tabProps.IsSelected = true;
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        if (manipulator.Parent.Parent is UnitDecal)
                        {
                            dgDecals.SelectedItem = manipulator.Parent.Parent;
                            tabDecals.IsSelected = true;
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        if (manipulator.Parent.Parent is UnitEffect)
                        {
                            dgEffects.SelectedItem = manipulator.Parent.Parent;
                            tabEffects.IsSelected = true;
                            found = true;
                        }
                    }
                }
            }

        }

        private void dgDecals_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgDecals.SelectedItem is UnitDecal)
            {
                UnitDecal binDecal = (UnitDecal)dgDecals.SelectedItem;
                /*if (binDecal.ModelRepresentation is ModelVisual3D)
                {
                    manipulator.Bind(binDecal.ModelRepresentation);
                    contentHolder.Content = binDecal;
                }*/
            }
        }

        private void dgGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgGroups.SelectedItem is LightGroup)
            {
                LightGroup group = (LightGroup)dgGroups.SelectedItem;
                contentHolder.Content = group;

            }
        }

        private void dgGroups_GotFocus(object sender, RoutedEventArgs e)
        {
            if (dgGroups.SelectedItem is LightGroup)
            {
                LightGroup group = (LightGroup)dgGroups.SelectedItem;
                contentHolder.Content = group;

            }
        }

        private void btnClone_Click(object sender, RoutedEventArgs e)
        {
            if (dgProps.SelectedItem is UnitBinDrawSlot)
            {
                UnitBinDrawSlot selProp = (UnitBinDrawSlot)dgProps.SelectedItem;
                selProp.UpdateTransform();

                UnitBinDrawSlot prop = new UnitBinDrawSlot();

                prop.Transform = selProp.Transform.Clone();
                prop.Index = selProp.Index;
                prop.Slot = selProp.Slot;

                prop.CreateGeometry();

                UnitFileEntry.UnitBinDrawSlots[selProp.Index].Add(prop);
                UnitFileEntry.UnitBinDrawSlots[selProp.Index].Changed();

                prop.ModelRepresentation.Parent = prop;

                if (checkBoxProps.IsChecked.GetValueOrDefault(false))
                {
                    viewPort.Children.Add(prop.ModelRepresentation);
                    
                }

                UnitFileEntry.UnitBinDrawSlotList.Add(prop);
                UnitFileEntry.UnitBinDrawSlotList.Changed();
                UnitFileEntry.UnitBinDrawSlots[selProp.Index].Changed();
            }
        }

        private void btnCloneLight_Click(object sender, RoutedEventArgs e)
        {
            if (lbLights.SelectedItem is UnitLight)
            {
                UnitLight selLight = (UnitLight)lbLights.SelectedItem;
                selLight.UpdateTransform();

                UnitLight light = new UnitLight();
                light.LightDebugName = new String8Property() { Value = "New" };
                light.LightColor = selLight.LightColor;
                light.LightType = selLight.LightType;
                light.LightCullDistance = selLight.LightCullDistance;
                light.LightId = selLight.LightId;

                light.LightInnerRadius = selLight.LightInnerRadius;
                light.LightOuterRadius = selLight.LightOuterRadius;
                light.LightSpecLevels = selLight.LightSpecLevels;
                light.LightLength = selLight.LightLength;
                light.IsVolumetric = selLight.IsVolumetric;
                light.LightDiffuseLevel = selLight.LightDiffuseLevel;
                light.LightVolStrength = selLight.LightVolStrength;
                light.LightFalloffStart = selLight.LightFalloffStart;

                light.LightTransform = new TransformProperty();
                light.LightTransform.SetMatrix(selLight.LightTransform.GetAsMatrix3D());
                light.LightTransform.Unknown = selLight.LightTransform.Unknown;
                light.LightTransform.Flags = selLight.LightTransform.Flags;

                light.CreateGeometry();

                UnitFileEntry.UnitLights.Add(light);
                UnitFileEntry.UnitLights.Changed();

                if (chkLights.IsChecked.GetValueOrDefault(false))
                {
                    viewPort.Children.Add(light.ModelRepresentation);
                }
            }
        }

        private void btnCloneDecal_Click(object sender, RoutedEventArgs e)
        {
            if (dgDecals.SelectedItem is UnitDecal)
            {
                /*UnitDecal selDecal = (UnitDecal)dgDecals.SelectedItem;
                UnitDecal decal = new UnitDecal();
                decal.ID = selDecal.ID;
                decal.TransFormFlags = selDecal.TransFormFlags;
                decal.Scale = selDecal.Scale;
                decal.RenderGroup = selDecal.RenderGroup;
                decal.Depth = selDecal.Depth;
                decal.DecalMachineSpec = selDecal.DecalMachineSpec;
                decal.CustomData3 = selDecal.CustomData3;
                decal.CustomData2 = selDecal.CustomData2;
                decal.CustomData1 = selDecal.CustomData1;
                decal.Category = selDecal.Category;


                decal.Transform = Matrix3D.Multiply(new Matrix3D(), selDecal.Transform);

                _binDecals.Add(decal);

                decal.CreateGeometry();

                viewPort.Children.Add(decal.ModelRepresentation);

                CollectionViewSource.GetDefaultView(_binDecals).Refresh();*/
            }
        }

        private void dgEffects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgEffects.SelectedItem is UnitEffect)
            {
                UnitEffect effect = (UnitEffect)dgEffects.SelectedItem;
                if (effect.ModelRepresentation is ModelVisual3D)
                {
                    // manipulator.Bind(effect.ModelRepresentation);
                    contentHolder.Content = effect;
                }
            }
        }

        private void btnCloneEffect_Click(object sender, RoutedEventArgs e)
        {
            /* if (dgEffects.SelectedItem is UnitEffect)
             {
                 UnitEffect selEffect = (UnitEffect)dgEffects.SelectedItem;
                 selEffect.UpdateTransform();

                 UnitEffect effect = new UnitEffect();
                 effect.ID = selEffect.ID;
                 effect.EffectID = selEffect.EffectID;
                 effect.Enabled = selEffect.Enabled;
                 effect.Transform = selEffect.Transform;
                 effect.Zero = selEffect.Zero;


                 effect.Transform = Matrix3D.Multiply(new Matrix3D(), selEffect.Transform);

                 _effects.Add(effect);

                 effect.CreateGeometry();

                 viewPort.Children.Add(effect.ModelRepresentation);

                 CollectionViewSource.GetDefaultView(_effects).Refresh();
             }*/
        }


        public bool DisplayModel { get; set; }

        public bool DisplayLights { get; set; }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (UnitFileEntry != null)
            {
                CheckBox cb = sender as CheckBox;
                DisplayLights = cb.IsChecked.GetValueOrDefault(false);
                if (!DisplayLights)
                {
                    foreach (UnitLight light in UnitFileEntry.UnitLights)
                    {
                        viewPort.Children.Remove(light.ModelRepresentation);
                    }
                }
                else
                {
                    foreach (UnitLight light in UnitFileEntry.UnitLights)
                    {
                        viewPort.Children.Add(light.ModelRepresentation);
                    }
                }
            }
        }

        public bool DisplayEffects { get; set; }
        private void CheckBoxEffects_Checked(object sender, RoutedEventArgs e)
        {
            if (UnitFileEntry != null)
            {
                CheckBox cb = sender as CheckBox;
                DisplayEffects = cb.IsChecked.GetValueOrDefault(false);
                if (!DisplayEffects)
                {
                    UnitFileEntry.UnitEffects.ForEach(ef => viewPort.Children.Remove(ef.ModelRepresentation));
                }
                else
                {
                    UnitFileEntry.UnitEffects.ForEach(ef => viewPort.Children.Add(ef.ModelRepresentation));
                }
            }
        }

        public bool DisplayDecals { get; set; }
        private void CheckBoxDecals_Checked(object sender, RoutedEventArgs e)
        {
            if (UnitFileEntry != null)
            {
                CheckBox cb = sender as CheckBox;
                DisplayDecals = cb.IsChecked.GetValueOrDefault(false);
                if (!DisplayDecals)
                {
                    UnitFileEntry.UnitBinDecals.ToList().ForEach(l => l.ForEach(b => viewPort.Children.Remove(b.ModelRepresentation)));
                }
                else
                {
                    UnitFileEntry.UnitBinDecals.ToList().ForEach(l => l.ForEach(b => viewPort.Children.Add(b.ModelRepresentation)));

                }
            }
        }

        public bool DisplayProps { get; set; }
        private void CheckBoxProps_Checked(object sender, RoutedEventArgs e)
        {
            if (UnitFileEntry != null)
            {
                CheckBox cb = sender as CheckBox;
                DisplayProps = cb.IsChecked.GetValueOrDefault(false);
                if (!DisplayProps)
                {
                    UnitFileEntry.UnitBinDrawSlots.ToList().ForEach(l => l.ForEach(b => viewPort.Children.Remove(b.ModelRepresentation)));
                }
                else
                {
                    UnitFileEntry.UnitBinDrawSlots.ToList().ForEach(l => l.ForEach(b => viewPort.Children.Add(b.ModelRepresentation)));

                }
            }
        }

        private void viewPort_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                //Delete the selected item from the list (if any)
                if (tabProps.IsSelected)
                {
                    UnitBinDrawSlot selectedItem = dgProps.SelectedItem as UnitBinDrawSlot;
                    viewPort.Children.Remove(selectedItem.ModelRepresentation);
                    foreach(ObservableList<UnitBinDrawSlot> slotList in UnitFileEntry.UnitBinDrawSlots)
                    {
                        slotList.Remove(selectedItem);
                    }
                }
                if (tabEffects.IsSelected)
                {
                    UnitEffect selectedItem = dgEffects.SelectedItem as UnitEffect;
                    viewPort.Children.Remove(selectedItem.ModelRepresentation);
                    UnitFileEntry.UnitEffects.Remove(selectedItem);
                }
                if (tabLights.IsSelected)
                {
                    UnitLight selectedItem = lbLights.SelectedItem as UnitLight;
                    viewPort.Children.Remove(selectedItem.ModelRepresentation);
                    UnitFileEntry.UnitLights.Remove(selectedItem);
                }
                if (tabDecals.IsSelected)
                {
                    UnitDecal selectedItem = dgDecals.SelectedItem as UnitDecal;
                    viewPort.Children.Remove(selectedItem.ModelRepresentation);
                    foreach (ObservableList<UnitDecal> slotList in UnitFileEntry.UnitBinDecals)
                    {
                        slotList.Remove(selectedItem);
                    }
                }
            }

        }

        // public bool DisplayTransforms { get; set; }
        private void CheckBoxTransforms_Click(object sender, RoutedEventArgs e)
        {
            /* if (_transForms != null)
             {
                 CheckBox cb = sender as CheckBox;
                 DisplayTransforms = cb.IsChecked.GetValueOrDefault(false);
                 if (!DisplayTransforms)
                 {
                     foreach (UnitTransform transform in _transForms)
                     {
                         viewPort.Children.Remove(transform.ModelRepresentation);
                     }
                 }
                 else
                 {
                     foreach (UnitTransform transform in _transForms)
                     {
                         viewPort.Children.Add(transform.ModelRepresentation);
                     }
                 }
             }*/
        }

        public bool DisplaySpawners { get; set; }
        private void chkSpawners_Click(object sender, RoutedEventArgs e)
        {
            if (UnitFileEntry != null)
            {
                CheckBox cb = sender as CheckBox;
                DisplaySpawners = cb.IsChecked.GetValueOrDefault(false);
                if (!DisplaySpawners)
                {
                    UnitFileEntry.UnitSimsSpawners.ForEach(ef => viewPort.Children.Remove(ef.ModelRepresentation));
                }
                else
                {
                    UnitFileEntry.UnitSimsSpawners.ForEach(ef => viewPort.Children.Add(ef.ModelRepresentation));
                }
            }
        }

        #region Ground Decal Generator

        private void btnImportLotTexture_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "Png|*.png";

            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                string originalFile = dlg.FileName;
                GenerateRasterImage(originalFile);
            }
        }

        public void GenerateRasterImage(string originalFile)
        {
            if (!string.IsNullOrEmpty(originalFile))
            {
                try
                {
                    nQuant.WuQuantizer quantizer = new nQuant.WuQuantizer();
                    System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(originalFile);

                    quantizer.QuantizeImage(bitmap);

                    QuantizedSignedDistanceFieldGenerator generator = new QuantizedSignedDistanceFieldGenerator(quantizer.Palette, bitmap.Height, bitmap.Width);
                    generator.Generate(0);
                    generator.Generate(1);
                    generator.Generate(2);
                    generator.Generate(3);

                    RasterImage RasterResult = RasterImage.CreateFromBitmap(generator.GetBitmap());

                    uint InstanceId = TGIRandomGenerator.GetNext();

                    DatabaseIndex newIndex = new DatabaseIndex(OriginalIndex.Owner);
                    newIndex.TypeId = (uint)TypeIds.RasterFile;
                    newIndex.InstanceId = InstanceId;
                    newIndex.Flags = 1;
                    newIndex.IsModified = true;
                    newIndex.Compressed = false;
                    newIndex.ModifiedData = new ModifiedRasterFile() { ImageFileData = RasterResult.ToIndexData() };
                    OriginalIndex.Owner.Indices.Add(newIndex);
                    SimCityPak.PackageReader.DatabaseManager.Instance.Indices.Add(newIndex);

                    UnitFileEntry.LotMask.InstanceId = InstanceId;
                    //quantizer.Palette.Colors[0].Scr
                    Color col = Color.FromArgb(quantizer.Palette.Colors[0].A, quantizer.Palette.Colors[0].R, quantizer.Palette.Colors[0].G, quantizer.Palette.Colors[0].B);
                    UnitFileEntry.LotColor4.R = col.ScR;
                    UnitFileEntry.LotColor4.G = col.ScG;
                    UnitFileEntry.LotColor4.B = col.ScB;

                    col = Color.FromArgb(quantizer.Palette.Colors[1].A, quantizer.Palette.Colors[1].R, quantizer.Palette.Colors[1].G, quantizer.Palette.Colors[1].B);
                    UnitFileEntry.LotColor1.R = col.ScR;
                    UnitFileEntry.LotColor1.G = col.ScG;
                    UnitFileEntry.LotColor1.B = col.ScB;

                    col = Color.FromArgb(quantizer.Palette.Colors[2].A, quantizer.Palette.Colors[2].R, quantizer.Palette.Colors[2].G, quantizer.Palette.Colors[2].B);
                    UnitFileEntry.LotColor2.R = col.ScR;
                    UnitFileEntry.LotColor2.G = col.ScG;
                    UnitFileEntry.LotColor2.B = col.ScB;

                    col = Color.FromArgb(quantizer.Palette.Colors[3].A, quantizer.Palette.Colors[3].R, quantizer.Palette.Colors[3].G, quantizer.Palette.Colors[3].B);
                    UnitFileEntry.LotColor3.R = col.ScR;
                    UnitFileEntry.LotColor3.G = col.ScG;
                    UnitFileEntry.LotColor3.B = col.ScB;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error during generation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        private void chkModel_Click(object sender, RoutedEventArgs e)
        {
            if (UnitFileEntry != null)
            {
                CheckBox cb = sender as CheckBox;
                DisplayModel = cb.IsChecked.GetValueOrDefault(false);
                if (!DisplaySpawners)
                {

                    viewPort.Children.Remove(VisualModelMain);
                }
                else
                {
                    viewPort.Children.Add(VisualModelMain);
                }
            }
        }





    }


}
