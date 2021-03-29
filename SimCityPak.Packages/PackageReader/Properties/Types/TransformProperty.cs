using System;
using System.IO;
using Gibbed.Spore.Helpers;
using System.Diagnostics;
using SimCityPak;
using System.Windows.Media.Media3D;

namespace Gibbed.Spore.Properties
{
    [PropertyDefinitionAttribute("transform", "transforms", 56)]
    public class TransformProperty : ComplexProperty
    {
        public ushort Flags;  //< looks like a bitfield, e.g. 0x6000300
        public float[] Matrix;

        int count = 12;

       // uint firstflags = 0;

        public float Unknown
        { get; set; }

        public Matrix3D GetAsMatrix3D()
        {
            Matrix3D matrix = new Matrix3D(
                Matrix[0],
                Matrix[1],
                Matrix[2],
                0,
                Matrix[3],
                Matrix[4],
                Matrix[5],
                0,
                Matrix[6],
                Matrix[7],
                Matrix[8],
                0,
                Matrix[9],
                Matrix[10],
                Matrix[11],
                1);
       
            return matrix;
        }



        public TransformProperty()
        {
            Unknown = 0;
        }

        public TransformProperty(Matrix3D matrix)
        {
            Matrix = new float[12];
            
                Matrix[0] = (float)matrix.M11;
                Matrix[1] = (float)matrix.M12;
                Matrix[2] = (float)matrix.M13;
                Matrix[3] = (float)matrix.M21;
                Matrix[4] = (float)matrix.M22;
                Matrix[5] = (float)matrix.M23;
                Matrix[6]= (float)matrix.M31;
                Matrix[7]= (float)matrix.M32;
                Matrix[8]= (float)matrix.M33;
                Matrix[9]= (float)matrix.OffsetX;
                Matrix[10]= (float)matrix.OffsetY;
                Matrix[11]= (float)matrix.OffsetZ;

                Flags = 14;
               // firstflags = 14;

                Unknown = 0;
        }

        public void SetMatrix(Matrix3D matrix)
        {
            Matrix = new float[12];

            Matrix[0] = (float)matrix.M11;
            Matrix[1] = (float)matrix.M12;
            Matrix[2] = (float)matrix.M13;
            Matrix[3] = (float)matrix.M21;
            Matrix[4] = (float)matrix.M22;
            Matrix[5] = (float)matrix.M23;
            Matrix[6] = (float)matrix.M31;
            Matrix[7] = (float)matrix.M32;
            Matrix[8] = (float)matrix.M33;
            Matrix[9] = (float)matrix.OffsetX;
            Matrix[10] = (float)matrix.OffsetY;
            Matrix[11] = (float)matrix.OffsetZ;

            //Flags = 14;
            // firstflags = 14;

            //Unknown = 0;
        }


        public override void ReadProp(Stream input, bool array)
        {

            //this.Flags = input.ReadU32BE();
            this.Flags = input.ReadU16BE();
            //Debug.WriteLine(string.Format("Reading Transform with flags {0}",Flags.ToHex() ));

            if (this.Flags == 0x0c)
            {
                //elfarto, I assume this is some sort of diagonal of a matrix
                count = 3;
            }
            else if (this.Flags == 0x0d)
            {
                //elfarto, this is probably another diagonal
                count = 4;
            }


           
            if (this.Flags == 15)
            {
                Unknown = input.ReadF32BE();
            }

            this.Matrix = new float[count];

            for (int i = 0; i < count; i++)
            {
                this.Matrix[i] = input.ReadF32BE();
            }

           
        }

        public override string DisplayValue
        {
            get { return string.Format("Transform (Count = {0}, {1}, {2}, {3})", Matrix.Length, String.Join(",", Matrix), Flags, Unknown.ToString()); }
        }

        public override void WriteProp(Stream output, bool array)
        {
            output.WriteU16BE(this.Flags);

            int count = 12;

            if (this.Flags == 0x0c)
            {
                //elfarto, I assume this is some sort of diagonal of a matrix
                count = 3;
            }
            else if (this.Flags == 0x0d)
            {
                //elfarto, this is probably another diagonal
                count = 4;
            }

            if (this.Flags == 15)
            {
                output.WriteF32BE(Unknown);
            }

            for (int i = 0; i < count; i++)
            {
                output.WriteF32BE(this.Matrix[i]);
            }


        }
    }
}
