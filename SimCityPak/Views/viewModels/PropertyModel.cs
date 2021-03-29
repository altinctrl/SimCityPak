using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gibbed.Spore.Properties;

namespace SimCityPak
{
    [Serializable]
    public class PropertyModel
    {

        public PropertyModel(uint id, Property property, bool isArray, int arrayIndex)
        {
            Id = id;
            Value = property;
            _isArray = isArray;
            _arrayIndex = arrayIndex;

            //check if a record exists for this property. If not, create it.
            if (TGIRegistry.Instance.Properties.Cache.TryGetValue(Id, out _propertyDetails) == false)
            {
                _propertyDetails = new TGIRecord() { Id = id };
            }

            object[] attributes = property.GetType().GetCustomAttributes(typeof(PropertyDefinitionAttribute), false);

            if (attributes.Length > 0)
            {
                PropertyDefinitionAttribute attribute = (PropertyDefinitionAttribute)attributes[0];
                _propertyType = attribute.Name;
            }

        }

        private TGIRecord _propertyDetails;
        public TGIRecord PropertyDetails
        {
            get { return _propertyDetails; }
            set { _propertyDetails = value; }
        }

        public uint Id { get; set; }

        private bool _isArray;
        public bool IsArray
        {
            get { return _isArray; }
            set { _isArray = value; }
        }

        private int _arrayIndex;
        public int ArrayIndex
        {
            get { return _arrayIndex; }
            set { _arrayIndex = value; }
        }

        private string _propertyType;
        public string PropertyType
        {
            get { return _propertyType; }
            set { _propertyType = value; }
        }


        public string DisplayName
        {
            get
            {
                string displayName = string.Empty;
                if (Properties.Settings.Default.ShowPropertyInternalNames)
                {
                    displayName += _propertyDetails.DisplayName;
                }
                else
                {
                    if (_propertyDetails.Comments == string.Empty)
                    {
                        displayName += _propertyDetails.DisplayName;
                    }
                    else
                    {
                        displayName += _propertyDetails.Comments;
                    }
                }
                if (Properties.Settings.Default.ShowPropertyIds)
                {
                    displayName += string.Format("({0})", Id.ToHex());
                }
                if (_isArray)
                {
                    displayName += string.Format(" [{0}]", _arrayIndex);
                }
                return displayName;
            }
        }

        public string ToolTip
        {
            get
            {
                if (_arrayIndex == 0)
                {
                    return string.Format("{0} ({1}) : {2}", Id.ToHex(), _propertyType, _propertyDetails.Comments);
                }
                return null;
            }
        }

        public Property Value { get; set; }
    }
}
