using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Spore.Helpers;
using System.Diagnostics;
using SimCityPak;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace SporeMaster.RenderWare4
{
    public struct Vertex : IRW4Struct
    {
        //minimum size = 40
        private uint size;

        public uint VertexSize
        {
            get { return size; }
            set { size = value; }
        }

        public List<IVertexComponentValue> VertexComponents { get; set; }


        public VertexFloat3Value Position
        {
            get
            {
                return (VertexFloat3Value)VertexComponents.First(v => v.Usage == D3DDECLUSAGE.D3DDECLUSAGE_POSITION && v.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT3);

            }
        }

        public int Element { get; set; }

        public VertexFloat4Value TextureCoordinates
        {
            get
            {
                return (VertexFloat4Value)VertexComponents.First(v => v.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TEXCOORD && v.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT4);
            }
        }

        public Vector3 Normal
        {
            get
            {
                Vector3 vector = new Vector3();
                IVertexComponentValue component = VertexComponents.First(v => v.Usage == D3DDECLUSAGE.D3DDECLUSAGE_NORMAL);
                if (component.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_UBYTE4)
                {
                    VertexUByte4Value byteValue = component as VertexUByte4Value;
                    vector = new Vector3(
                                         (((float)byteValue.X) - 127.5f) / 127.5f,
                                         (((float)byteValue.Y) - 127.5f) / 127.5f,
                                         (((float)byteValue.Z) - 127.5f) / 127.5f
                                       );
                }
                return vector;
            }
        }

        public uint PackedBoneIndices { get; set; }
        public uint PackedBoneWeights { get; set; }

        public byte[] additionalData;

        public int AdditionalDataSize
        {
            get
            {
                if (additionalData != null)
                {
                    return additionalData.Length;
                }
                return 0;
            }
        }

        public uint Size() { return size; }

        public void Read(Stream r)
        {
            throw new Exception("Use Read(Stream r, VertexFormat vFormat) instead for Vertices");
        }

        public void Read(Stream r, VertexFormat vFormat)
        {
            VertexComponents = new List<IVertexComponentValue>();

            size = vFormat.VertexSize;
            long pos = r.Position;

            foreach (VertexUsage usage in vFormat.VertexElements.OrderBy(vu => vu.Offset))
            {
                r.Seek(pos + usage.Offset, SeekOrigin.Begin);


                IVertexComponentValue componentValue = VertexComponentValueFactory.CreateComponent(usage.DeclarationType);
                if (componentValue != null)
                {
                    componentValue.Usage = usage.Usage;
                    componentValue.Read(r);

                    VertexComponents.Add(componentValue);
                }
            }

            r.Seek(pos + size, SeekOrigin.Begin);
        }

        public void SetSize(uint _size)
        {
            this.size = _size;
        }

        public void Write(Stream w)
        {
            foreach (IVertexComponentValue value in this.VertexComponents)
            {
                value.Write(w);
            }
        }
        public void Read4(Stream r)
        {
            //X = r.ReadF32(); Y = r.ReadF32(); Z = r.ReadF32(); r.expect(0, "4V001");
        }
        public void Write4(Stream w)
        {
            //w.WriteF32(X); w.WriteF32(Y); w.WriteF32(Z); w.WriteU32(0);
        }

        public static UInt32 PackNormal(float x, float y, float z)
        {
            float invl = 127.5F;
            var xb = (byte)(x * invl + 127.5);
            var yb = (byte)(y * invl + 127.5);
            var zb = (byte)(z * invl + 127.5);
            return ((UInt32)xb) + ((UInt32)yb << 8) + ((UInt32)zb << 16) + ((UInt32)0x01 << 24);
        }

        public static UInt32 PackNormal(float x, float y, float z, float w)
        {
            float invl = 127.5F;
            var xb = (byte)(x * invl + 127.5);
            var yb = (byte)(y * invl + 127.5);
            var zb = (byte)(z * invl + 127.5);
            var wb = (byte)(w * invl + 127.5);
            return ((UInt32)xb) + ((UInt32)yb << 8) + ((UInt32)zb << 16) + ((UInt32)0x01 << 24);
        }

        public static float UnpackNormal(UInt32 packed, int dim)
        {
            byte b = (byte)((packed >> (dim * 8)) & 0xff);
            return (((float)b) - 127.5f) / 127.5f;
        }


    };
}
