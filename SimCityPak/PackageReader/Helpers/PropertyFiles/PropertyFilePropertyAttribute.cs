using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Gibbed.Spore.Properties;

namespace SimCityPak
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyFilePropertyAttribute : PropertyFilePropertyAttributeBase
    {
        public PropertyFilePropertyAttribute()
        {
            Optional = false;
        }

        public uint PropertyID { get; set; }

        public Type PropertyType { get; set; }

        public bool Optional { get; set; }


        public override List<uint> Load(PropertyFileObject file, PropertyInfo property, Dictionary<uint, Property> properties)
        {
            List<uint> definedProperties = new List<uint>();
            if (properties.ContainsKey(this.PropertyID))
            {
                property.SetValue(file, properties[this.PropertyID], null);
            }
            else if (!this.Optional)
            {
                throw new Exception(string.Format("Missing property {0:X8} in property file!", this.PropertyID));
            }
            definedProperties.Add(this.PropertyID);
            return definedProperties;
        }

        public override void Save(PropertyFileObject file, PropertyInfo property, Dictionary<uint, Property> returnValues)
        {
            if (property.GetValue(file, null) != null)
            {
                returnValues.Add(this.PropertyID, (Property)property.GetValue(file, null));
            }
            else if (!this.Optional)
            {
                throw new Exception("Missing property in property file!");
            }
        }
    }
}
