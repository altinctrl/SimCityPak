using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Gibbed.Spore.Properties;

namespace SimCityPak
{
    public abstract class PropertyFilePropertyAttributeBase : Attribute
    {
        public abstract void Save(PropertyFileObject file, PropertyInfo property, Dictionary<uint, Property> returnValues);
        public abstract List<uint> Load(PropertyFileObject file, PropertyInfo property, Dictionary<uint, Property> properties);
    }
}
