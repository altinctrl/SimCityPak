using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Windows.Media;
using Gibbed.Spore.Properties;

namespace SimCityPak.Views.AdvancedEditors
{
    public class UnitTransform : ILotEditorItem
    {
        public uint ID
        { get; set; }

        public int ArrayIndex
        { get; set; }

        public Matrix3D Transform
        { get; set; }

        public LotEditorItemMarker ModelRepresentation
        { get; set; }

        public LotEditorItemMarker CreateGeometry()
        {
            LotEditorItemMarker item = new LotEditorItemMarker(this);
            
            BoxVisual3D box = new BoxVisual3D();
            box.Height = 2;
            box.Width = 2;
            box.Length = 2;
            box.Transform = new MatrixTransform3D(this.Transform);
            box.Material = MaterialHelper.CreateMaterial(Brushes.Black, Brushes.Blue);
          //  this.ModelRepresentation = box;
            return item;
        }

        public void UpdateTransform()
        {
            if (ModelRepresentation != null)
            {
                Transform = ModelRepresentation.Transform.Value;
            }
        }

        public void UpdateGeometry()
        {
            ModelRepresentation = CreateGeometry();
        }
    }
}
