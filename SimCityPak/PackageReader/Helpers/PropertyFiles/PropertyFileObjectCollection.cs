using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Gibbed.Spore.Properties;

namespace SimCityPak
{
    public class PropertyFileObjectCollectionAttribute : PropertyFilePropertyAttributeBase
    {

        public override void Save(PropertyFileObject file, PropertyInfo property, Dictionary<uint, Property> returnValues)
        {
            IEnumerable<object> list = (IEnumerable<object>)property.GetValue(file, null);
            //must be based on List<PropertyFileObject>
            Type listItemType = property.PropertyType.GetGenericArguments()[0];
            List<PropertyInfo> listItemProperties = listItemType.GetProperties().Where(p => p.GetCustomAttributes(typeof(PropertyFilePropertyAttribute), false).Count() == 1).ToList<PropertyInfo>();

            foreach (PropertyFileObject obj in list)
            {
                foreach (PropertyInfo listItemProperty in listItemProperties)
                {
                    PropertyFilePropertyAttribute listItemAttr = (PropertyFilePropertyAttribute)listItemProperty.GetCustomAttributes(typeof(PropertyFilePropertyAttribute), false)[0];
                    if (!returnValues.ContainsKey(listItemAttr.PropertyID))
                    {
                        returnValues.Add(listItemAttr.PropertyID, new ArrayProperty() { Values = new List<Property>(), PropertyType = listItemProperty.PropertyType });
                    }
                    ((ArrayProperty)returnValues[listItemAttr.PropertyID]).Values.Add((Property)listItemProperty.GetValue(obj, null));
                }

            }
        }

        public override List<uint> Load(PropertyFileObject file, PropertyInfo property, Dictionary<uint, Property> properties)
        {
            List<uint> definedProperties = new List<uint>();
            object list = property.PropertyType.GetConstructor(new Type[] { }).Invoke(new object[] { });
            property.SetValue(file, list, null);
            //must be based on List<PropertyFileObject>
            Type listItemType = property.PropertyType.GetGenericArguments()[0];
            List<PropertyInfo> listItemProperties = listItemType.GetProperties().Where(p => p.GetCustomAttributes(typeof(PropertyFilePropertyAttribute), false).Count() == 1).ToList<PropertyInfo>();

            int index = 0;
            int maxIndex = int.MaxValue;

            while (index < maxIndex)
            {
                PropertyFileObject listInstance = (PropertyFileObject)listItemType.GetConstructor(new Type[] { }).Invoke(new object[] { });
                bool containsProperty = false;
                foreach (PropertyInfo listItemProperty in listItemProperties)
                {
                    PropertyFilePropertyAttribute listItemAttr = (PropertyFilePropertyAttribute)listItemProperty.GetCustomAttributes(typeof(PropertyFilePropertyAttribute), false)[0];
                    if (properties.ContainsKey(listItemAttr.PropertyID))
                    {
                        maxIndex = ((ArrayProperty)properties[listItemAttr.PropertyID]).Values.Count;
                        listItemProperty.SetValue(listInstance, ((ArrayProperty)properties[listItemAttr.PropertyID]).Values[index], null);
                        containsProperty = true;
                    }
                    else if (!listItemAttr.Optional)
                    {
                        if (index == 0) //the array may be empty, which could still be valid
                        {

                        }
                        else
                        {
                            throw new Exception(string.Format("Missing property {0:X8} in property file!", listItemAttr.PropertyID));
                        }
                    }
                    definedProperties.Add(listItemAttr.PropertyID);
                }
                if (containsProperty)
                {
                    list.GetType().GetMethod("Add").Invoke(list, new object[] { listInstance });
                }
                else
                {
                    break;
                }

                index++;
            }
            return definedProperties;
        }
    }
}
