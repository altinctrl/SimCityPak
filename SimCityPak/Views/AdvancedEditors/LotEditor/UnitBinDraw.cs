using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Gibbed.Spore.Properties;
using HelixToolkit.Wpf;
using System.Windows.Media;

namespace SimCityPak.Views.AdvancedEditors
{

    public class UnitBinDrawSlot : PropertyFileObject, ILotEditorItem
    {
       // [PropertyFilePropertyArrayAttribute(new uint[] { 0x0c12ef20, 0x0c12ef21, 0x0c12ef22, 0x0c12ef23, 0x0c12ef24, 0x0c12ef25, 0x0c12ef26, 0x0c12ef27, 0x0c12ef28, 0x0c12ef29, 0x0c12ef2a, 0x0c12ef2b, 0x0c12ef2c, 0x0c12ef2d, 0x0c12ef2e })]
       // public KeyProperty ID
       // { get; set; }

        public int Index
        { get; set; }

        [PropertyFilePropertyArrayAttribute(new uint[] { 0x0c12ef30, 0x0c12ef31, 0x0c12ef32, 0x0c12ef33, 0x0c12ef34, 0x0c12ef35, 0x0c12ef36, 0x0c12ef37, 0x0c12ef38, 0x0c12ef39, 0x0c12ef3a, 0x0c12ef3b, 0x0c12ef3c, 0x0c12ef3d, 0x0c12ef3e })]
        public TransformProperty Transform
        { get; set; }

        public float TransformUnknown
        { get; set; }

        public ushort TransFormFlag
        { get; set; }

        [PropertyFilePropertyArrayAttribute(new uint[] { 0x0c12ef40, 0x0c12ef41, 0x0c12ef42, 0x0c12ef43, 0x0c12ef44, 0x0c12ef45, 0x0c12ef46, 0x0c12ef47, 0x0c12ef48, 0x0c12ef49, 0x0c12ef4a, 0x0c12ef4b, 0x0c12ef4c, 0x0c12ef4d, 0x0c12ef4e })]
        public Int32Property Slot
        { get; set; }

        public LotEditorItemMarker ModelRepresentation
        { get; set; }
        
       // [PropertyFilePropertyArrayAttribute(new uint[] { 0x0c12ef50, 0x0c12ef51, 0x0c12ef52, 0x0c12ef53, 0x0c12ef54, 0x0c12ef55, 0x0c12ef56, 0x0c12ef57, 0x0c12ef58, 0x0c12ef59, 0x0c12ef5a, 0x0c12ef5b, 0x0c12ef5c, 0x0c12ef5d, 0x0c12ef5e }, Optional=true)]
        public BoolProperty RandomizeSlot
        { get; set; }

      //  [PropertyFilePropertyArrayAttribute(new uint[] { 0x0c12ef60, 0x0c12ef61, 0x0c12ef62, 0x0c12ef63, 0x0c12ef64, 0x0c12ef65, 0x0c12ef66, 0x0c12ef67, 0x0c12ef68, 0x0c12ef69, 0x0c12ef6a, 0x0c12ef6b, 0x0c12ef6c, 0x0c12ef6d, 0x0c12ef6e }, Optional = true)]
        public BoolProperty PercentFill
        { get; set; }

        public LotEditorItemMarker CreateGeometry()
        {
            LotEditorItemMarker marker = new LotEditorItemMarker(this);
            Model3DCollection coll = new Model3DCollection();
            TruncatedConeVisual3D cone = new TruncatedConeVisual3D();
            cone.Height = 2.5;
            cone.BaseRadius = 0;
            cone.TopRadius = 1;
            cone.Material = MaterialHelper.CreateMaterial(Brushes.Red, new SolidColorBrush(Color.FromScRgb(1f, 0.2f, 0.0f, 0f)));
            marker.ManipulatorModel = cone.Model;
            marker.Transform = new MatrixTransform3D(this.Transform.GetAsMatrix3D());
            ModelRepresentation = marker;

            BillboardTextVisual3D billboard = new BillboardTextVisual3D();
            billboard.Text = Index.ToString();
            billboard.Background = Brushes.White;
            billboard.Transform = new TranslateTransform3D(0,0,5);
            
            marker.Children.Add(billboard);
            
            
            return marker;
        }

        public void UpdateTransform()
        {
            if (ModelRepresentation != null)
            {
                Transform.SetMatrix(ModelRepresentation.Transform.Value);
            }
        }

        public void UpdateGeometry()
        {
            ModelRepresentation = CreateGeometry();
        }
    }

    public class UnitBinDraw : PropertyFileObject
    {
        public int GroupIndex
        { get; set; }

        public List<uint> IDs
        { get; set; }

        public List<UnitBinDrawSlot> Slots
        { get; set; }
    }
}
