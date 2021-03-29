using System.IO;
using System.Text;
using Gibbed.Spore.Helpers;
using System;

namespace Gibbed.Spore.Properties
{
    [PropertyDefinition("string16", "string16s", 19, "viewStringProperty")]
    public class String16Property : Property
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

        public override void ReadProp(Stream input, bool array)
        {
            int length = input.ReadS32BE();
            byte[] data = new byte[length * 2];
            input.Read(data, 0, data.Length);
            this.Value = Encoding.BigEndianUnicode.GetString(data);
        }

        public override void WriteProp(Stream output, bool array)
        {
            byte[] data = Encoding.BigEndianUnicode.GetBytes(this.Value);
            output.WriteS32BE(data.Length / 2);
            output.Write(data, 0, data.Length);
        }
    }
}
