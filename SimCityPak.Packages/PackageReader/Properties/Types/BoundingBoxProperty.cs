using System;
using System.IO;
using Gibbed.Spore.Helpers;

namespace Gibbed.Spore.Properties
{
	[PropertyDefinitionAttribute("bbox", "bboxes", 57)]
	public class BoundingBoxProperty : ComplexProperty
	{
		public float MinX;
		public float MinY;
		public float MinZ;
		public float MaxX;
		public float MaxY;
		public float MaxZ;

        public override string DisplayValue
        {
            get { return string.Format("Bounding Box (MinX = {0}, MinY = {1}, MinZ = {2}, MaxX = {3}, MaxY = {4}, MaxZ = {5})", MinX, MinY, MinZ, MaxX, MaxY, MaxZ); }
        }

		public override void ReadProp(Stream input, bool array)
		{
            this.MinX = input.ReadF32BE();
            this.MinY = input.ReadF32BE();
			this.MinZ = input.ReadF32BE();
            this.MaxX = input.ReadF32BE();
            this.MaxY = input.ReadF32BE();
            this.MaxZ = input.ReadF32BE();
		}

		public override void WriteProp(Stream output, bool array)
		{
            output.WriteF32BE(this.MinX);
            output.WriteF32BE(this.MinY);
            output.WriteF32BE(this.MinZ);
            output.WriteF32BE(this.MaxX);
            output.WriteF32BE(this.MaxY);
            output.WriteF32BE(this.MaxZ);
        }
	}
}
