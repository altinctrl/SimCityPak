using System.IO;
using Gibbed.Spore.Helpers;

namespace Gibbed.Spore.Properties
{
	[PropertyDefinitionAttribute("vector3", "vector3s", 49)]
	public class Vector3Property : Property
	{
		public float X;
		public float Y;
		public float Z;

        public override string DisplayValue
        {
            get { return string.Format("Vector3 (X = {0}, Y = {1}, Z = {2})", X, Y, Z); }
        }

		public override void ReadProp(Stream input, bool array)
		{
            this.X = input.ReadF32BE();
            this.Y = input.ReadF32BE();
            this.Z = input.ReadF32BE();

			if (array == false)
			{
				//input.Seek(4, SeekOrigin.Current);
			}
		}

		public override void WriteProp(Stream output, bool array)
		{
			output.WriteF32BE(this.X);
			output.WriteF32BE(this.Y);
			output.WriteF32BE(this.Z);
		}
	}
}
