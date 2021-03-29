using System.IO;
using Gibbed.Spore.Helpers;
using System;

namespace Gibbed.Spore.Properties
{
    [PropertyDefinition("bool", "bools", 1, "viewBoolProperty")]
    public class BoolProperty : Property
    {
        private bool _value;

        public bool Value
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
            get
            {
                return _value.ToString();
            }
        }

        public override void ReadProp(Stream input, bool array)
        {
            this._value = input.ReadBoolean();
        }

        public override void WriteProp(Stream output, bool array)
        {
            output.WriteBoolean(this._value);
        }
    }
}
