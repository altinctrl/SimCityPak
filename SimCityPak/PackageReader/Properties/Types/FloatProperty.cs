using System.IO;
using Gibbed.Spore.Helpers;
using System;

namespace Gibbed.Spore.Properties
{
    [PropertyDefinition("float", "floats", 13, "viewFloatProperty")]
    public class FloatProperty : Property
    {
        private float _value;
        public float Value
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
            this._value = input.ReadF32BE();
        }

        public override void WriteProp(Stream output, bool array)
        {
            output.WriteF32BE(this._value);
        }
    }
}
