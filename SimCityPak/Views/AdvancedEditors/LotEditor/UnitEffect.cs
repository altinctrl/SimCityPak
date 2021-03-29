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
    public class UnitEffect : PropertyFileObject, ILotEditorItem
    {
        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.effectID)]
        public KeyProperty ID
        { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.effectTransform)]
        public TransformProperty Transform
        { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.effectAlwaysZero)]
        public KeyProperty Zero
        { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.effectEffectID)]
        public KeyProperty EffectID
        { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.effectEnabled)]
        public BoolProperty Enabled
        { get; set; }

        public LotEditorItemMarker ModelRepresentation { get; set; }

        public LotEditorItemMarker CreateGeometry()
        {
           LotEditorItemMarker marker = new LotEditorItemMarker(this);
           TruncatedConeVisual3D cone = new TruncatedConeVisual3D();
            cone.Height = 2.5;
            cone.BaseRadius = 0;
            cone.TopRadius = 1;
            cone.Material = MaterialHelper.CreateMaterial(Brushes.Gold, new SolidColorBrush(Color.FromScRgb(1f, 0.2f, 0.2f, 0f)));
            marker.ManipulatorModel = cone.Model;
            marker.Transform = new MatrixTransform3D(this.Transform.GetAsMatrix3D());
            ModelRepresentation = marker;
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
}
