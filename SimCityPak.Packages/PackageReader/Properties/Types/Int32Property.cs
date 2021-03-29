using System;
using System.IO;
using Gibbed.Spore.Helpers;

namespace Gibbed.Spore.Properties
{
    [PropertyDefinition("int32", "int32s", 9, "viewFloatProperty")]
    public class Int32Property : Property
    {
        private Int32 _value;
        public Int32 Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(new EventArgs());
            }
        }

        public override void OnPropertyChanged(EventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        public override string DisplayValue
        {
            get { return _value.ToString(); }
        }

        public override void ReadProp(Stream input, bool array)
        {
            this._value = input.ReadS32BE();
        }

        public override void WriteProp(Stream output, bool array)
        {
            output.WriteS32BE(this._value);
        }
    }
}
