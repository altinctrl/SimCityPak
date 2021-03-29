using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Gibbed.Spore.Properties;

namespace SimCityPak
{
    public class PropertyFileObjectCollectionArrayAttribute : PropertyFilePropertyAttributeBase
    {
        public PropertyFileObjectCollectionArrayAttribute(int count)
        {
            Count = count;
        }

        public int Count
        { get; set; }

        public override void Save(PropertyFileObject file, System.Reflection.PropertyInfo property, Dictionary<uint, Gibbed.Spore.Properties.Property> returnValues)
        {
            Array array = (Array)property.GetValue(file, null);
            Type arrayItemType = array.GetType().GetElementType();

            for (int i = 0; i < this.Count; i++)
            {
                IEnumerable<object> list = (IEnumerable<object>)array.GetValue(i);
                //must be based on List<PropertyFileObject>
                Type listItemType = property.PropertyType.GetGenericArguments()[0];
                List<PropertyInfo> listItemProperties = listItemType.GetProperties().Where(p => p.GetCustomAttributes(typeof(PropertyFilePropertyArrayAttribute), false).Count() == 1).ToList<PropertyInfo>();
                int index = 0;

                foreach (PropertyFileObject obj in list)
                {
                    foreach (PropertyInfo listItemProperty in listItemProperties)
                    {
                        PropertyFilePropertyArrayAttribute listItemAttr = (PropertyFilePropertyArrayAttribute)listItemProperty.GetCustomAttributes(typeof(PropertyFilePropertyArrayAttribute), false)[0];
                        if (!returnValues.ContainsKey(listItemAttr.PropertyIDs[i]))
                        {
                            returnValues.Add(listItemAttr.PropertyIDs[i], new ArrayProperty() { Values = new List<Property>(), PropertyType = listItemProperty.PropertyType });
                        }
                        ((ArrayProperty)returnValues[listItemAttr.PropertyIDs[i]]).Values.Add((Property)listItemProperty.GetValue(obj, null));
                    }

                    index++;
                }
            }
        }



        public override List<uint> Load(PropertyFileObject file, PropertyInfo property, Dictionary<uint, Property> properties)
        {
            //get the properties that exist in arrays with multiple levels
            List<uint> definedProperties = new List<uint>();

            //make a nested list of lists
            object list = property.PropertyType.GetConstructor(new Type[] { typeof(Int32) }).Invoke(new object[] { this.Count }); //this creates the containing array[] of lists
            property.SetValue(file, list, null);

            PropertyFileObjectCollectionArrayAttribute arrayPropAttribute = (PropertyFileObjectCollectionArrayAttribute)property.GetCustomAttributes(typeof(PropertyFileObjectCollectionArrayAttribute), false)[0];
            Array listArray = list as Array;

            for (int i = 0; i < arrayPropAttribute.Count; i++)
            {
                //make a list of lists - 
                Type innerListType = listArray.GetType().GetElementType();
                //make an inner list instance
                object innerList = innerListType.GetConstructor(new Type[] { }).Invoke(new object[] { }); //this creates the containing array[] of lists
                listArray.SetValue(innerList, i);

                //must be based on List<PropertyFileObject>
                Type listItemType = innerListType.GetGenericArguments()[0];
                List<PropertyInfo> listItemProperties = listItemType.GetProperties().Where(p => p.GetCustomAttributes(typeof(PropertyFilePropertyArrayAttribute), false).Count() == 1).ToList<PropertyInfo>();

                int index = 0;
                int maxIndex = int.MaxValue;

                while (index < maxIndex)
                {
                    PropertyFileObject listInstance = (PropertyFileObject)listItemType.GetConstructor(new Type[] { }).Invoke(new object[] { });
                    bool containsProperty = false;
                    foreach (PropertyInfo listItemProperty in listItemProperties)
                    {
                        PropertyFilePropertyArrayAttribute listItemAttr = (PropertyFilePropertyArrayAttribute)listItemProperty.GetCustomAttributes(typeof(PropertyFilePropertyArrayAttribute), false)[0];
                        if (properties.ContainsKey(listItemAttr.PropertyIDs[i]))
                        {
                            maxIndex = ((ArrayProperty)properties[listItemAttr.PropertyIDs[i]]).Values.Count;
                            listItemProperty.SetValue(listInstance, ((ArrayProperty)properties[listItemAttr.PropertyIDs[i]]).Values[index], null);
                            containsProperty = true;
                        }
                        else if (!listItemAttr.Optional)
                        {
                            if (index == 0) //the array may be empty, which could still be valid
                            {

                            }
                            else
                            {
                                throw new Exception(string.Format("Missing property {0:X8} in property file!", listItemAttr.PropertyIDs[i]));
                            }
                        }
                        definedProperties.Add(listItemAttr.PropertyIDs[i]);
                    }
                    if (containsProperty)
                    {
                        innerList.GetType().GetMethod("Add").Invoke(innerList, new object[] { listInstance });
                    }
                    else
                    {
                        break;
                    }

                    index++;
                }
            }
            return definedProperties;
        }
    }
}
