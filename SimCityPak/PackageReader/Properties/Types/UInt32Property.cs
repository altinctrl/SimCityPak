using Gibbed.Spore.Helpers;
using SimCityPak;
using System;

namespace Gibbed.Spore.Properties
{
    [PropertyDefinition("uint32", "uint32s", 10, "viewFloatProperty")]
    public class UInt32Property : Property
    {
        private uint _value;
        public uint Value
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
            get { return _value.ToHex(); }
        }

        public override void ReadProp(System.IO.Stream input, bool array)
        {
            this.Value = input.ReadU32BE();
        }

        public override void WriteProp(System.IO.Stream output, bool array)
        {
            output.WriteU32BE(this.Value);
        }
    }
}
