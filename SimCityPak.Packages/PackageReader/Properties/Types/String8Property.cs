using System.IO;
using System.Text;
using Gibbed.Spore.Helpers;
using System;

namespace Gibbed.Spore.Properties
{
    [PropertyDefinition("string8", "string8s", 18, "viewStringProperty")]
    public class String8Property : Property
    {
        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(new EventArgs());
            }
        }

        public override string DisplayValue
        {
            get { return _value; }
        }

        public override void ReadProp(Stream input, ushort flags, bool array)
        {
            uint length = input.ReadU32BE();
            //if the length is 0, and a flag is set, read another byte for the length...?
            if (length == 0 && (flags & 0x100) != 0)
            {
                length = input.ReadU8();
            }
            byte[] data = new byte[length];
            input.Read(data, 0, data.Length);
            this.Value = Encoding.UTF8.GetString(data);
        }

        public override void ReadProp(Stream input, bool array)
        {
            //just call the proper one with no flags
            ReadProp(input, 0, array);
        }

        public override void WriteProp(Stream output, bool array)
        {
            byte[] data = Encoding.UTF8.GetBytes(this.Value);
            output.WriteS32BE(data.Length);
            output.Write(data, 0, data.Length);
        }
    }
}
