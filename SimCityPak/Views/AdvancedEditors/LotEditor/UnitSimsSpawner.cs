using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gibbed.Spore.Properties;
using HelixToolkit.Wpf;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SimCityPak.Views.AdvancedEditors
{
    public class UnitSimsSpawner : PropertyFileObject, ILotEditorItem
    {
        [PropertyFilePropertyAttribute(PropertyID = 0x0E1BAC61)]
        public KeyProperty ID
        { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = 0x0E1BAC62)]
        public TransformProperty Transform
        { get; set; }



        public LotEditorItemMarker ModelRepresentation { get; set; }

        public LotEditorItemMarker CreateGeometry()
        {
            LotEditorItemMarker marker = new LotEditorItemMarker(this);
            TruncatedConeVisual3D cone = new TruncatedConeVisual3D();
            cone.Height = 2.5;
            cone.BaseRadius = 0;
            cone.TopRadius = 1;
            cone.Material = MaterialHelper.CreateMaterial(Brushes.Blue, new SolidColorBrush(Color.FromScRgb(1f, 0.0f, 0.0f, 0.2f)));
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
