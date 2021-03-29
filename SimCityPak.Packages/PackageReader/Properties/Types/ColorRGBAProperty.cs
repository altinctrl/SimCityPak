using System.IO;
using Gibbed.Spore.Helpers;

namespace Gibbed.Spore.Properties
{
	[PropertyDefinition("colorRGBA", "colorRGBAs", 52, "viewColorProperty")]
	public class ColorRGBAProperty : Property
	{
		public float R;
		public float G;
		public float B;
		public float A;

        public override string DisplayValue
        {
            get
            {
                return string.Format("R {0}-G {1}-B {2}-A {3}", R, G, B, A);
            }
        }

		public override void ReadProp(Stream input, bool array)
		{
            this.R = input.ReadF32BE();
            this.G = input.ReadF32BE();
            this.B = input.ReadF32BE();
            this.A = input.ReadF32BE();
		}

		public override void WriteProp(Stream output, bool array)
		{
            output.WriteF32BE(this.R);
            output.WriteF32BE(this.G);
            output.WriteF32BE(this.B);
            output.WriteF32BE(this.A);
		}
	}
}
