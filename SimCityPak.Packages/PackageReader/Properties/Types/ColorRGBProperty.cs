using System.IO;
using Gibbed.Spore.Helpers;
using System.Windows.Media;



namespace Gibbed.Spore.Properties
{
	[PropertyDefinition("colorRGB", "colorRGBs", 50, "viewColorProperty")]
	public class ColorRGBProperty : Property
	{
		public float R;
		public float G;
		public float B;

        public override string DisplayValue
        {
            get
            {
                return string.Format("R {0}-G {1}-B {2}", R, G, B);
            }
        }

        public Color Color
        {
            get
            {
                return Color.FromScRgb(1, R, G, B);
            }
            set
            {
                R = value.ScR;
                G = value.ScG;
                B = value.ScB;
            }

        }


		public override void ReadProp(Stream input, bool array)
		{
            this.R = input.ReadF32BE();
            this.G = input.ReadF32BE();
            this.B = input.ReadF32BE();

		}

		public override void WriteProp(Stream output, bool array)
		{
			output.WriteF32BE(this.R);
            output.WriteF32BE(this.G);
            output.WriteF32BE(this.B);
		}
	}
}
