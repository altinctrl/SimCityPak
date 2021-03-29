using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gibbed.Spore.Properties;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace SimCityPak.Views.AdvancedEditors
{
    public class UnitPathPoint : PropertyFileObject, ILotEditorItem
    {
        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.UnitPathPoint)]
        public Vector3Property Point { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.UnitPathPointTangent)]
        public Vector3Property Tangent { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.UnitPathPointIndex)]
        public Int32Property Index { get; set; }

        public LotEditorItemMarker ModelRepresentation { get; set; }

        public LotEditorItemMarker CreateGeometry()
        {
            LotEditorItemMarker item = new LotEditorItemMarker(this);

            SphereVisual3D point = new SphereVisual3D();
            point.Radius = 2;
           
            point.Material = MaterialHelper.CreateMaterial(Brushes.Black, Brushes.Aqua);

            item.ManipulatorModel = point.Model; 
            item.Transform = new TranslateTransform3D(Point.X, Point.Y, Point.Z);
            this.ModelRepresentation = item;
            return item;
        }

        public void UpdateTransform()
        {
            if (ModelRepresentation != null)
            {
                Point.X = (float)ModelRepresentation.Transform.Value.OffsetX;
                Point.Y = (float)ModelRepresentation.Transform.Value.OffsetY;
                Point.Z = (float)ModelRepresentation.Transform.Value.OffsetZ;
                
                //Transform = ModelRepresentation.Transform.Value;
            }
        }

        public void UpdateGeometry()
        {
            ModelRepresentation = CreateGeometry();
        }
    }
}
