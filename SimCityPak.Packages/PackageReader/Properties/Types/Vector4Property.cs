using System.IO;
using Gibbed.Spore.Helpers;

namespace Gibbed.Spore.Properties
{
	[PropertyDefinitionAttribute("vector4", "vector4s", 51)]
	public class Vector4Property : Property
	{
		public float X;
		public float Y;
		public float Z;
		public float W;

        public override string DisplayValue
        {
            get { return string.Format("Vector4 (X = {0}, Y = {1}, Z = {2}, W = {3})", X, Y, Z, W); }
        }

		public override void ReadProp(Stream input, bool array)
		{
			this.X = input.ReadF32BE();
            this.Y = input.ReadF32BE();
            this.Z = input.ReadF32BE();
            this.W = input.ReadF32BE();
		}

		public override void WriteProp(Stream output, bool array)
		{
			output.WriteF32BE(this.X);
			output.WriteF32BE(this.Y);
			output.WriteF32BE(this.Z);
			output.WriteF32BE(this.W);
		}
	}
}
