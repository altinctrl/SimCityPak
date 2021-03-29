using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimCityPak
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyFilePropertyArrayAttribute : PropertyFilePropertyAttributeBase
    {
        public PropertyFilePropertyArrayAttribute(uint[] propertyIDs)
        {
            PropertyIDs = propertyIDs;
            Optional = false;
        }

        public uint[] PropertyIDs { get; set; }

        public Type PropertyType { get; set; }

        public bool Optional { get; set; }

        public override void Save(PropertyFileObject file, System.Reflection.PropertyInfo property, Dictionary<uint, Gibbed.Spore.Properties.Property> returnValues)
        {
            throw new NotImplementedException();
        }

        public override List<uint> Load(PropertyFileObject file, System.Reflection.PropertyInfo property, Dictionary<uint, Gibbed.Spore.Properties.Property> properties)
        {
            throw new NotImplementedException();
        }
    }

}
