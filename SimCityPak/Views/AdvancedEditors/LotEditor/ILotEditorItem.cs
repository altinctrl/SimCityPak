using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace SimCityPak.Views.AdvancedEditors
{
    public interface ILotEditorItem
    {
        LotEditorItemMarker ModelRepresentation
        { get; set; }

        LotEditorItemMarker CreateGeometry();

        void UpdateTransform();

        void UpdateGeometry();
    }
}
