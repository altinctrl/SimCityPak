using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gibbed.Spore.Properties;
using SimCityPak.Views.AdvancedEditors;
using SimCityPak.PackageReader;

namespace SimCityPak
{
    public class UnitFile : PropertyFileObject
    {
        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.Parent, Optional = true)]
        public KeyProperty Parent { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.UnitLOD1, Optional = true)]
        public KeyProperty LOD1 { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.UnitLOD2, Optional = true)]
        public KeyProperty LOD2 { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.UnitLOD3, Optional = true)]
        public KeyProperty LOD3 { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.UnitLOD4, Optional = true)]
        public KeyProperty LOD4 { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.ModelSize, Optional = true)]
        public FloatProperty ModelSize { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.UnitLotOverlayBoxSize, Optional = true)]
        public Vector2Property LotOverlayBoxSize { get; set; }

        public float LotSizeX { get { return LotOverlayBoxSize != null ? LotOverlayBoxSize.X : 0; } set { LotOverlayBoxSize.X = value; } }
        public float LotSizeY { get { return LotOverlayBoxSize != null ? LotOverlayBoxSize.Y : 0; } set { LotOverlayBoxSize.Y = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.LotUnitOffset, Optional = true)]
        public Vector2Property LotOverlayBoxOffset { get; set; }

        [PropertyFileArrayPropertyAttribute(PropertyID = PropertyConstants.LotPlacementTransform, Optional = true)]
        public List<TransformProperty> LotPlacementTransform { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.LotMask, Optional = true)]
        public KeyProperty LotMask { get; set; }       

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.LotColor1, Optional = true)]
        public ColorRGBAProperty LotColor1 { get; set; }
        public int LotColor1Texture { get { return LotColor1 != null ? (int)LotColor1.A : 0; } set { LotColor1.A = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.LotColor2, Optional = true)]
        public ColorRGBAProperty LotColor2 { get; set; }
        public int LotColor2Texture { get { return LotColor2 != null ? (int)LotColor2.A : 0; } set { LotColor2.A = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.LotColor3, Optional = true)]
        public ColorRGBAProperty LotColor3 { get; set; }
        public int LotColor3Texture { get { return LotColor3 != null ? (int)LotColor3.A : 0; } set { LotColor3.A = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.LotColor4, Optional = true)]
        public ColorRGBAProperty LotColor4 { get; set; }
        public int LotColor4Texture { get { return LotColor4 != null ? (int)LotColor4.A : 0; } set { LotColor4.A = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.LotBorderColor1, Optional = true)]
        public ColorRGBAProperty LotBorderColor1 { get; set; }
        public int LotBorderColor1Texture { get { return LotBorderColor1!= null ? (int)LotBorderColor1.A : 0; } set { LotBorderColor1.A = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.LotBorderColor2, Optional = true)]
        public ColorRGBAProperty LotBorderColor2 { get; set; }
        public int LotBorderColor2Texture { get { return LotBorderColor2!= null ? (int)LotBorderColor2.A : 0; } set { LotBorderColor2.A = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.LotBorderColor3, Optional = true)]
        public ColorRGBAProperty LotBorderColor3 { get; set; }
        public int LotBorderColor3Texture { get { return LotBorderColor3!= null ? (int)LotBorderColor3.A : 0; } set { LotBorderColor3.A = value; } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.LotBorderColor4, Optional = true)]
        public ColorRGBAProperty LotBorderColor4 { get; set; }
        public int LotBorderColor4Texture { get { return LotBorderColor4!= null ? (int)LotBorderColor4.A : 0; } set { LotBorderColor4.A = value; } }

        [PropertyFileObjectCollection()]
        public ObservableList<UnitLight> UnitLights { get; set; }

        [PropertyFileObjectCollection()]
        public ObservableList<UnitEffect> UnitEffects { get; set; }

        [PropertyFileObjectCollection()]
        public ObservableList<UnitSimsSpawner> UnitSimsSpawners { get; set; }

        [PropertyFileObjectCollection()]
        public ObservableList<UnitPathPoint> UnitPathPoints { get; set; }

         [PropertyFileArrayProperty(PropertyID = PropertyConstants.scUnitPaths, Optional=true)]
        public ObservableList<Int32Property> UnitPath { get; set; }

        [PropertyFileObjectCollection()]
        public ObservableList<UnitPath> UnitPaths { get; set; }

        [PropertyFileObjectCollectionArray(14)]
        public ObservableList<UnitBinDrawSlot>[] UnitBinDrawSlots { get; set; }
        public ObservableList<UnitBinDrawSlot> UnitBinDrawSlotList
        {
            get
            {
                ObservableList<UnitBinDrawSlot> list = new ObservableList<UnitBinDrawSlot>();
                list.AddRange(UnitBinDrawSlots.SelectMany(s => s));
                return list;
            }
        }

        [PropertyFileObjectCollectionArray(3)]
        public ObservableList<UnitDecal>[] UnitBinDecals { get; set; }



        public override void Load(Dictionary<uint, Property> properties)
        {
            base.Load(properties);
            UnitBinDrawSlots.ToList().ForEach(b => b.ForEach(bd => bd.Index = UnitBinDrawSlots.ToList().IndexOf(b)));
        }

        public override Dictionary<uint, Property> Save()
        {
            UnitBinDrawSlots.ToList().ForEach(b => b.ForEach(bd => bd.Slot.Value = b.IndexOf(bd)));
            return base.Save();
        }

    }
}
