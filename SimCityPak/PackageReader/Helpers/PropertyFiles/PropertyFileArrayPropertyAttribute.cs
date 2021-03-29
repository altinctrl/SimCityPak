using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Gibbed.Spore.Properties;

namespace SimCityPak
{
    /// <summary>
    /// Attribute for use for properties that consist of a single array of values
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyFileArrayPropertyAttribute : PropertyFilePropertyAttribute
    {
        public PropertyFileArrayPropertyAttribute()
        {
            Optional = false;
        }

        public override List<uint> Load(PropertyFileObject file, PropertyInfo property, Dictionary<uint, Gibbed.Spore.Properties.Property> properties)
        {
            List<uint> definedProperties = new List<uint>();
            object list = property.PropertyType.GetConstructor(new Type[] { }).Invoke(new object[] { });
            property.SetValue(file, list, null);

            Type listItemType = property.PropertyType.GetGenericArguments()[0];

            Property listInstance = (Property)listItemType.GetConstructor(new Type[] { }).Invoke(new object[] { });

            if (properties.ContainsKey(this.PropertyID))
            {
                int count = ((ArrayProperty)properties[this.PropertyID]).Values.Count;
                for (int i = 0; i < count; i++)
                {
                    list.GetType().GetMethod("Add").Invoke(list, new object[] { ((ArrayProperty)properties[this.PropertyID]).Values[i] });
                }
            }
            else if (!this.Optional)
            {
                throw new Exception(string.Format("Missing property {0:X8} in property file!", this.PropertyID));
            }
            definedProperties.Add(this.PropertyID);

            return definedProperties;
        }

        public override void Save(PropertyFileObject file, PropertyInfo property, Dictionary<uint, Gibbed.Spore.Properties.Property> returnValues)
        {
            IEnumerable<object> list = (IEnumerable<object>)property.GetValue(file, null);
            Type listItemType = property.PropertyType.GetGenericArguments()[0];

            foreach (Property obj in list)
            {
                if (!returnValues.ContainsKey(this.PropertyID))
                {
                    returnValues.Add(this.PropertyID, new ArrayProperty() { Values = new List<Property>(), PropertyType = listItemType });
                }
                ((ArrayProperty)returnValues[this.PropertyID]).Values.Add(obj);
            }


        }

    }
}
