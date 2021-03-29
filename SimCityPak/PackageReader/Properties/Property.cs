using System;
using System.IO;
using System.Xml;

namespace Gibbed.Spore.Properties
{
    [Serializable]
    public abstract class Property
    {
        public virtual void ReadProp(Stream input, ushort flags, bool array)
        {
            ReadProp(input, array);
        }

        public event EventHandler PropertyChanged;

        public abstract void ReadProp(Stream input, bool array);
        public abstract void WriteProp(Stream input, bool array);

        public abstract string DisplayValue { get; }

        public virtual void OnPropertyChanged(EventArgs e)
        {
            EventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    public class PropertyDefinitionAttribute : Attribute
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _viewName;
        public string ViewName
        {
            get { return _viewName; }
            set { _viewName = value; }
        }

        private string _editViewName;
        public string EditViewName
        {
            get { return _editViewName; }
            set { _editViewName = value; }
        }



        public string PluralName;
        public ushort FileType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Singular name of the property</param>
        /// <param name="pluralName">Plural name of the property</param>
        /// <param name="fileType">The id of the property as represented in a property file</param>
        public PropertyDefinitionAttribute(string name, string pluralName, ushort fileType, string viewName, string editViewName)
        {
            if (name == pluralName)
            {
                throw new Exception("singular name cannot be the same as the plural name");
            }

            this.Name = name;
            this.PluralName = pluralName;
            this.FileType = fileType;
            this.ViewName = viewName;
            this.EditViewName = editViewName;
        }

        public PropertyDefinitionAttribute(string name, string pluralName, ushort fileType, string viewName)
            : this(name, pluralName, fileType, viewName, "viewBasicProperty")
        {
            
        }

        public PropertyDefinitionAttribute(string name, string pluralName, ushort fileType)
            : this(name, pluralName, fileType, "viewBasicProperty", "viewBasicProperty")
        {

        }
    }
}
