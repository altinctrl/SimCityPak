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
    public class UnitDecal : PropertyFileObject, ILotEditorItem
    {
        public int Category
        { get; set; }

        [PropertyFilePropertyArrayAttribute(new uint[] { 0x0d109050, 0x0d109051, 0x0d109052 })]
        public KeyProperty ID
        { get; set; }

        [PropertyFilePropertyArrayAttribute(new uint[] { 0x0d109060, 0x0d109061, 0x0d109062 })]
        public TransformProperty Transform
        { get; set; }


        public ushort TransFormFlags
        { get; set; }

        public float Scale
        { 
            get { return Transform.Unknown; }
            set { Transform.Unknown = value; }
        }

        [PropertyFilePropertyArrayAttribute(new uint[] { 0x0d109070, 0x0d109071, 0x0d109072 })]
        public FloatProperty Depth
        { get; set; }

        [PropertyFilePropertyArrayAttribute(new uint[] { 0x0d109080, 0x0d109081, 0x0d109082 })]
        public Vector3Property MaterialData
        { get; set; }

      //   [PropertyFilePropertyArrayAttribute(new uint[] { 0x0d109090, 0x0d109091, 0x0d109092 }, Optional=true)]
      //  public KeyProperty RenderGroup
      //  { get; set; }

      //   [PropertyFilePropertyArrayAttribute(new uint[] { 0x0d1090a0, 0x0d1090a1, 0x0d1090a2 }, Optional = true)]
      //   public Int32Property DecalMachineSpec
      //  { get; set; }

        public LotEditorItemMarker CreateGeometry()
        {
            LotEditorItemMarker marker = new LotEditorItemMarker(this);
            RectangleVisual3D rectangle = new RectangleVisual3D();
            rectangle.Length = 2 * Scale;
            rectangle.Width = 2 * Scale;
            rectangle.Material = new DiffuseMaterial(Brushes.Transparent);
            rectangle.BackMaterial = MaterialHelper.CreateMaterial(Brushes.Green, new SolidColorBrush(Color.FromScRgb(1f, 0.2f, 0.2f, 0f)));
            marker.ManipulatorModel = rectangle.Model;
            marker.Transform = new MatrixTransform3D(this.Transform.GetAsMatrix3D());
            ModelRepresentation = marker;
            return marker;
        }

        public LotEditorItemMarker ModelRepresentation
        { get; set; }

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

    public class UnitDecalData
    {
        public uint ID
        { get; set; }

        public uint Transform
        { get; set; }

        public uint Depth
        { get; set; }

        public uint MaterialData
        { get; set; }

        public uint RenderGroup
        { get; set; }

        public uint MachineSpec
        { get; set; }

        public static List<UnitDecalData> _decalData;
        public static List<UnitDecalData> DecalData
        {
            get
            {
                if (_decalData == null)
                {
                    _decalData = new List<UnitDecalData>();
                    _decalData.Add(new UnitDecalData() { ID = 0x0d109050, Transform = 0x0d109060, Depth = 0x0d109070, MaterialData = 0x0d109080, RenderGroup = 0x0d109090, MachineSpec = 0x0d1090a0 });
                    _decalData.Add(new UnitDecalData() { ID = 0x0d109051, Transform = 0x0d109061, Depth = 0x0d109071, MaterialData = 0x0d109081, RenderGroup = 0x0d109091, MachineSpec = 0x0d1090a1 });
                    _decalData.Add(new UnitDecalData() { ID = 0x0d109052, Transform = 0x0d109062, Depth = 0x0d109072, MaterialData = 0x0d109082, RenderGroup = 0x0d109092, MachineSpec = 0x0d1090a2 });
                }
                return _decalData;
            }

        }
    }
}
