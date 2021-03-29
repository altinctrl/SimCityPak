using System.IO;
using Gibbed.Spore.Helpers;

namespace Gibbed.Spore.Properties
{
	[PropertyDefinition("vector2", "vector2s", 48)]
	public class Vector2Property : Property
	{
		public float X;
		public float Y;

        public override string DisplayValue
        {
            get { return string.Format("Vector2 (X = {0}, Y = {1})", X, Y); }
        }

		public override void ReadProp(Stream input, bool array)
		{
            this.X = input.ReadF32BE();
            this.Y = input.ReadF32BE();

			//if (array == false)
			//{
			//	input.Seek(8, SeekOrigin.Current);
			//}
		}

		public override void WriteProp(Stream output, bool array)
		{
			output.WriteF32BE(this.X);
			output.WriteF32BE(this.Y);
		}
	}
}
