using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Linq;
using Gibbed.Spore.Helpers;
using SimCityPak;
using System.Diagnostics;

namespace Gibbed.Spore.Properties
{
    public static class TypeHelpers
    {
        private static PropertyDefinitionAttribute GetPropertyDefinition(this Type type)
        {
            if (type.IsSubclassOf(typeof(Property)) == false)
            {
                return null;
            }

            object[] attributes = type.GetCustomAttributes(typeof(PropertyDefinitionAttribute), false);

            if (attributes.Length == 0)
            {
                return null;
            }

            return (PropertyDefinitionAttribute)(attributes[0]);
        }

        public static string GetSingularName(this Type type)
        {
            PropertyDefinitionAttribute def = type.GetPropertyDefinition();

            if (def == null)
            {
                return null;
            }

            return def.Name;
        }

        public static string GetPluralName(this Type type)
        {
            PropertyDefinitionAttribute def = type.GetPropertyDefinition();

            if (def == null)
            {
                return null;
            }

            return def.PluralName;
        }
    }

    public class PropertyLookup
    {
        public Type Type;
        public PropertyDefinitionAttribute Definition;
    }

    public class PropertyFactory
    {
        public static PropertyFactory Singleton
        {
            get
            {
                if (instance == null) instance = new PropertyFactory();
                return instance;
            }
        }
        static PropertyFactory instance = null;

        private Dictionary<ushort, PropertyLookup> PropertyTypes;
        private PropertyFactory()
        {
            this.PropertyTypes = new Dictionary<ushort, PropertyLookup>();
            foreach (Type type in Assembly.GetAssembly(this.GetType()).GetTypes())
            {
                if (type.IsSubclassOf(typeof(Property)))
                {
                    object[] attributes = type.GetCustomAttributes(typeof(PropertyDefinitionAttribute), false);
                    if (attributes.Length > 0)
                    {
                        PropertyDefinitionAttribute propDef = (PropertyDefinitionAttribute)(attributes[0]);

                        if (this.PropertyTypes.ContainsKey(propDef.FileType) == true)
                        {
                            throw new Exception("duplicate property type id " + propDef.FileType.ToString());
                        }

                        this.PropertyTypes[propDef.FileType] = new PropertyLookup();
                        this.PropertyTypes[propDef.FileType].Type = type;
                        this.PropertyTypes[propDef.FileType].Definition = propDef;
                    }
                }
            }
        }

        public PropertyDefinitionAttribute FindPropertyType(Type type)
        {
            foreach (PropertyLookup lookup in this.PropertyTypes.Values)
            {
                if (lookup.Type == type)
                {
                    return lookup.Definition;
                }
            }

            return null;
        }

        public PropertyLookup FindPropertyType(string name)
        {
            foreach (PropertyLookup lookup in this.PropertyTypes.Values)
            {
                if (lookup.Definition.Name == name || lookup.Definition.PluralName == name)
                {
                    return lookup;
                }
            }

            return null;
        }

        public Type GetTypeFromFileType(ushort dataType)
        {
            if (this.PropertyTypes.ContainsKey(dataType))
            {
                return this.PropertyTypes[dataType].Type;
            }

            return null;
        }
    };

    public class PropertyFile
    {
        private Dictionary<uint, Property> _values = new Dictionary<uint, Property>();
        private uint _propertyCount;

        public uint PropertyCount
        {
            get { return _propertyCount; }
            set { _propertyCount = value; }
        }

        public Dictionary<uint, Property> Values
        {
            get { return _values; }
            set { _values = value; }
        }

        PropertyFactory PropTypes = PropertyFactory.Singleton;

        public PropertyFile()
        {
        }


        public static Property AddProperty(uint id, Type propertyType, bool isArray)
        {
            Property newProperty;
            if (isArray)
            {
                ArrayProperty arrProperty = new ArrayProperty();
                arrProperty.Values = new List<Property>();

                Property subProperty = Activator.CreateInstance(propertyType) as Property;

                arrProperty.Values.Add(subProperty);
                newProperty = arrProperty;
            }
            else
            {
                newProperty = Activator.CreateInstance(propertyType) as Property;
            }
            return newProperty;

        }

        public void Read(Stream input)
        {
            _propertyCount = input.ReadU32BE();
            uint lasthash = 0;

            //  Debug.WriteLine(string.Format("Opening property file with {0} properties", _propertyCount));

            //added check for eof as one PROPs file has less properties than it claims it does.
            for (uint i = 0; i < _propertyCount && input.Position < input.Length; i++)
            {
                long pos = input.Position;
                uint hash = input.ReadU32BE();
                ushort fileType = input.ReadU16BE();
                ushort flags = input.ReadU16BE();

                //Debug.WriteLine(string.Format("Found property with at pos {3} hash {0} and type {1} and flags {2}", hash.ToHex(), fileType.ToString("X"), flags.ToString("X"), input.Position));

                // if (i > 0 && hash < lasthash)
                //      throw new Exception("property file has keys out of order: " + lasthash.ToString("X8") + ", " + hash.ToString("X8"));
                lasthash = hash;

                if (this.Values.ContainsKey(hash))
                {
                    throw new Exception("property file already has " + hash.ToString("X8") + " defined");
                }

                Property property = null;
                Type type = PropTypes.GetTypeFromFileType(fileType);

                if (type == null)
                {
                    throw new Exception(string.Format("type {0} is Unknown2", fileType.ToString("X")));
                    // Debug.WriteLine(string.Format("Hash {0} with type {1} is not a valid type...apparently", hash.ToHex(), fileType.ToString("X")));
                }
                else
                {
                    // Debug.WriteLine(string.Format("Found property {0} with type {1}", hash.ToHex(), type.ToString()));
                    if ((flags & 0x30) == 0) // is not variant?
                    {
                        property = Activator.CreateInstance(type) as Property;
                        //added flags to read prop for string8 property
                        property.ReadProp(input, flags, false);

                        this.Values[hash] = property;
                    }
                    // Variant
                    else if ((flags & 0x40) == 0) // is not empty?
                    {
                        ArrayProperty array = new ArrayProperty();
                        array.PropertyType = type;

                        int arrayCount = input.ReadS32BE();
                        int arrayItemSize = input.ReadS32BE();

                        //var test = input.ReadArrayCount();

                        //Debug.WriteLine("Found an array property with {0} items", arrayItemSize);
                        //var test = input.ReadS16();
                        //arrayItemSize = input.ReadS8();

                        for (uint j = 0; j < arrayCount; j++)
                        {
                            Property subproperty = Activator.CreateInstance(type) as Property;

                            /* if (subproperty is ComplexProperty)
                             {
                                 if (subproperty is TextProperty)
                                 {
                                     arrayItemSize = 8;
                                 }
                                 else if (subproperty is BoundingBoxProperty)
                                 {
                                     arrayItemSize = 48;
                                 }
                                 else if (subproperty is TransformProperty)
                                 {
                                     arrayItemSize = 8;

                                 }



                                 MemoryStream memory = new MemoryStream();
                                 byte[] data = new byte[arrayItemSize];
                                 input.Read(data, 0, arrayItemSize);
                                 memory.Write(data, 0, arrayItemSize);
                                 memory.Seek(0, SeekOrigin.Begin);

                                 subproperty.ReadProp(memory, true);

                             }
                             else
                             {*/
                            subproperty.ReadProp(input, true);
                            //}

                            array.Values.Add(subproperty);
                        }

                        property = array;

                        this.Values[hash] = property;
                    }
                    else // is not empty?
                    {
                        // Debug.WriteLine(string.Format("Hash {0} with type {1} does not have a valid flag thingy...apparently", hash.ToHex(), fileType.ToString("X")));
                    }
                }
            }
            if (input.Position != input.Length)
            {
                //throw new Exception("Incomplete!");
            }
        }

        public void Write(Stream output)
        {
            output.WriteS32BE(this.Values.Count);

            // Important!  Rick's code didn't do this, which meant packed property files could randomly break
            // (order of keys from Dictionary is undefined).  In practice it seems to have worked when input XML
            // is in order, but people didn't know that...
            //var keys = from k in this.Values.Keys orderby k select k;
            var keys = this.Values.Keys;

            foreach (uint hash in keys)
            {
                Property property = this.Values[hash];

                output.WriteU32BE(hash);

                if (property is ArrayProperty)
                {
                    ArrayProperty array = (ArrayProperty)property;
                    ushort fileType = PropTypes.FindPropertyType(array.PropertyType).FileType;
                    output.WriteU16BE(fileType);
                    if ((array.PropertyType.BaseType == typeof(ComplexProperty)) && (array.Values.Count == 1))
                    {
                        if (array.PropertyType == typeof(TransformProperty))
                        {
                            output.WriteU16BE(0x809C);
                        }
                        else
                        {
                            output.WriteU16BE(0x801C);
                        }
                    }
                    else
                    {
                        output.WriteU16BE(0x809C);
                    }
                }
                /* else if (property is ComplexProperty)
                 {
                     output.WriteU16BE(PropTypes.FindPropertyType(this.Values[hash].GetType()).FileType);
                     output.WriteU16BE(0x801C);
                 }*/
                else
                {
                    output.WriteU16BE(PropTypes.FindPropertyType(this.Values[hash].GetType()).FileType);
                    output.WriteU16BE(0x8000);
                }

                if (property is ArrayProperty)
                {
                    ArrayProperty array = (ArrayProperty)property;

                    MemoryStream[] memories = new MemoryStream[array.Values.Count];

                    int index = 0;
                    uint maxSize = 0;
                    foreach (Property subproperty in array.Values)
                    {
                        memories[index] = new MemoryStream();
                        subproperty.WriteProp(memories[index], true);

                        if (memories[index].Length > maxSize)
                        {
                            maxSize = (uint)memories[index].Length;
                        }

                        index++;
                    }

                    output.WriteU32BE((uint)memories.Length);
                    if (maxSize == 8)
                    {
                        output.WriteU32BE(maxSize | 0x0200);
                    }
                    else
                    {
                        if (maxSize == 50)
                        {
                            //added for one specific case... don't know if there's anything else to this
                            output.WriteU32BE(56);
                        }
                        else if (maxSize == 54)
                        {
                                                        //added for one specific case... don't know if there's anything else to this
                            output.WriteU32BE(56);
                        }
                        else
                        {
                            output.WriteU32BE(maxSize);
                        }
                    }


                    for (int i = 0; i < memories.Length; i++)
                    {
                        byte[] data = new byte[array.Values[i] is ComplexProperty ? maxSize : memories[i].Length];
                        memories[i].Seek(0, SeekOrigin.Begin);
                        memories[i].Read(data, 0, (int)(memories[i].Length));
                        output.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    MemoryStream memory = new MemoryStream();
                    property.WriteProp(memory, false);

                    memory.Seek(0, SeekOrigin.Begin);

                    if (property is ComplexProperty)
                    {
                        output.WriteU32BE(1);
                        output.WriteU32BE((uint)memory.Length | 0x0200);
                    }

                    byte[] data = new byte[memory.Length];
                    memory.Read(data, 0, (int)memory.Length);
                    output.Write(data, 0, data.Length);
                }
            }
        }
    }
}
