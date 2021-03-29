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
    public class UnitPath : PropertyFileObject
    {
        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scUnitPathEntries)]
        public KeyProperty Id { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scUnitPathScope)]
        public KeyProperty Scope { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scUnitPathRoadConnect)]
        public KeyProperty RoadConnect { get; set; }
    }
}
