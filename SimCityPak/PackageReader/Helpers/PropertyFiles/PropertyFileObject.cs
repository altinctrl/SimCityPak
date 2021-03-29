using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Gibbed.Spore.Properties;
using System.ComponentModel;
using System.Collections;

namespace SimCityPak
{
    public class PropertyFileObject : INotifyPropertyChanged, ICloneable
    {
        public Dictionary<uint, Property> UndefinedProperties;

        public virtual void Load(Dictionary<uint, Property> properties)
        {
            try
            {
                //Generate lists for adding undefined properties
                List<uint> definedProperties = new List<uint>();
                UndefinedProperties = new Dictionary<uint, Property>();

                List<PropertyInfo> classProperties = this.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(PropertyFilePropertyAttributeBase), false).Count() == 1).ToList<PropertyInfo>();
                classProperties.ForEach(p => definedProperties.AddRange(((PropertyFilePropertyAttributeBase)p.GetCustomAttributes(typeof(PropertyFilePropertyAttributeBase), false)[0]).Load(this, p, properties)));
                //load all the properties not defined by the file object
                foreach (KeyValuePair<uint, Property> prop in properties.Where(kp => !definedProperties.Contains(kp.Key)))
                {
                    UndefinedProperties.Add(prop.Key, prop.Value);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public virtual Dictionary<uint, Property> Save()
        {
            Dictionary<uint, Property> returnValues = new Dictionary<uint, Property>();

            List<PropertyInfo> properties = this.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(PropertyFilePropertyAttributeBase), false).Count() == 1).ToList<PropertyInfo>();
            properties.ForEach(p => ((PropertyFilePropertyAttributeBase)p.GetCustomAttributes(typeof(PropertyFilePropertyAttributeBase), false)[0]).Save(this, p, returnValues));

            //write back undefined properties
            foreach (KeyValuePair<uint, Property> prop in UndefinedProperties)
            {
                returnValues.Add(prop.Key, prop.Value);
            }

            return returnValues;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
