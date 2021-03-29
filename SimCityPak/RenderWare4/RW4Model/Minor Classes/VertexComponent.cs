using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Spore.Helpers;
using System.Diagnostics;
using SimCityPak;
using System.Globalization;
using Gibbed.Spore.Helpers;

namespace SporeMaster.RenderWare4
{
    public interface IVertexComponentValue
    {
        D3DDECLUSAGE Usage { get; set; }
        D3DDECLTYPE DeclarationType { get; }
        string Value { get; }

        void Read(Stream r);
        void Write(Stream r);
    }

    public static class VertexComponentValueFactory
    {
        public static IVertexComponentValue CreateComponent(D3DDECLTYPE declarationType)
        {
            switch(declarationType)
            {
                case D3DDECLTYPE.D3DDECLTYPE_FLOAT1: return new VertexFloat1Value();
                case D3DDECLTYPE.D3DDECLTYPE_FLOAT2: return new VertexFloat2Value();
                case D3DDECLTYPE.D3DDECLTYPE_FLOAT3 : return new VertexFloat3Value(); 
                case D3DDECLTYPE.D3DDECLTYPE_FLOAT4 : return new VertexFloat4Value();
                case D3DDECLTYPE.D3DDECLTYPE_UBYTE4: return new VertexUByte4Value();
                case D3DDECLTYPE.D3DDECLTYPE_SHORT2: return new VertexShort2Value();
                case D3DDECLTYPE.D3DDECLTYPE_SHORT4N: return new VertexShort4NValue();
                case D3DDECLTYPE.D3DDECLTYPE_SHORT4: return new VertexShort4Value();
                case D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR: return new VertexD3DColorValue();
                  
                default : return null;
            }
        }
    }

    public class VertexFloat1Value : IVertexComponentValue
    {
        public float X { get; set; }

        public D3DDECLUSAGE Usage { get; set; }
        public D3DDECLTYPE DeclarationType { get { return D3DDECLTYPE.D3DDECLTYPE_FLOAT1; } }

        public string Value { get { return string.Format("X: {0}", X); } }

        public void Read(Stream r)
        {
            X = r.ReadF32();
        }

        public void Write(Stream w)
        {
            w.WriteF32(X);
        }
    }

    public class VertexFloat2Value : IVertexComponentValue
    {
        public float X { get; set; }
        public float Y { get; set; }

        public D3DDECLUSAGE Usage { get; set; }
        public D3DDECLTYPE DeclarationType { get { return D3DDECLTYPE.D3DDECLTYPE_FLOAT2; } }

        public string Value { get { return string.Format("X: {0} Y: {1}", X, Y); } }

        public void Read(Stream r)
        {
            X = r.ReadF32();
            Y = r.ReadF32();
        }

        public void Write(Stream w)
        {
            w.WriteF32(X);
            w.WriteF32(Y);
        }
    }

    public class VertexFloat3Value : IVertexComponentValue
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public D3DDECLUSAGE Usage { get; set; }
        public D3DDECLTYPE DeclarationType { get { return D3DDECLTYPE.D3DDECLTYPE_FLOAT3; } }

        public string Value { get { return string.Format("X: {0} Y: {1} Z: {2}", X, Y, Z); } }

        public void Read(Stream r)
        {
            X = r.ReadF32();
            Y = r.ReadF32();
            Z = r.ReadF32();
        }

        public void Write(Stream w)
        {
            w.WriteF32(X);
            w.WriteF32(Y);
            w.WriteF32(Z);
        }
    }

    public class VertexFloat4Value : IVertexComponentValue
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public D3DDECLUSAGE Usage { get; set; }
        public D3DDECLTYPE DeclarationType { get { return D3DDECLTYPE.D3DDECLTYPE_FLOAT4; } }

        public string Value { get { return string.Format("X: {0} Y: {1} Z: {2} W: {3}", X, Y, Z, W); } }

        public void Read(Stream r)
        {
            X = r.ReadF32();
            Y = r.ReadF32();
            Z = r.ReadF32();
            W = r.ReadF32();
        }

        public void Write(Stream w)
        {
            w.WriteF32(X);
            w.WriteF32(Y);
            w.WriteF32(Z);
            w.WriteF32(W);
        }
    }

    public class VertexUByte4Value : IVertexComponentValue
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Z { get; set; }
        public byte W { get; set; }

        public D3DDECLUSAGE Usage { get; set; }
        public D3DDECLTYPE DeclarationType { get { return D3DDECLTYPE.D3DDECLTYPE_UBYTE4; } }

        public string Value { get { return string.Format("X: {0} Y: {1} Z: {2} W: {3}", X, Y, Z, W); } }

        public void Read(Stream r)
        {
            X = r.ReadU8();
            Y = r.ReadU8();
            Z = r.ReadU8();
            W = r.ReadU8();
        }

        public void Write(Stream w)
        {
            w.WriteU8(X);
            w.WriteU8(Y);
            w.WriteU8(Z);
            w.WriteU8(W);
        }
    }

    public class VertexD3DColorValue : IVertexComponentValue
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public D3DDECLUSAGE Usage { get; set; }
        public D3DDECLTYPE DeclarationType { get { return D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR; } }

        public string Value { get { return string.Format("A: {0} R: {1} G: {2} B: {3}", A, R, G, B); } }

        public void Read(Stream r)
        {
            A = r.ReadU8();
            R = r.ReadU8();
            G = r.ReadU8();
            B = r.ReadU8();
        }

        public void Write(Stream w)
        {
            w.WriteU8(A);
            w.WriteU8(R);
            w.WriteU8(G);
            w.WriteU8(B);
        }
    }

    public class VertexShort4NValue : IVertexComponentValue
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double W { get; set; }

        public D3DDECLUSAGE Usage { get; set; }
        public D3DDECLTYPE DeclarationType { get { return D3DDECLTYPE.D3DDECLTYPE_SHORT4N; } }

        public string Value { get { return string.Format("X: {0} Y: {1} Z: {2} W: {3}", X, Y, Z, W); } }

        public void Read(Stream r)
        {
            X = r.ReadS16() / 32767.0;
            Y = r.ReadS16() / 32767.0;
            Z = r.ReadS16() / 32767.0;
            W = r.ReadS16() /32767.0;
        }

        public void Write(Stream w)
        {
            w.WriteS16((short)(X * 32767.0));
            w.WriteS16((short)(Y * 32767.0));
            w.WriteS16((short)(Z * 32767.0));
            w.WriteS16((short)(W * 32767.0));
        }
    }

    public class VertexShort4Value : IVertexComponentValue
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public short W { get; set; }

        public D3DDECLUSAGE Usage { get; set; }
        public D3DDECLTYPE DeclarationType { get { return D3DDECLTYPE.D3DDECLTYPE_SHORT4; } }

        public string Value { get { return string.Format("X: {0} Y: {1} Z: {2} W: {3}", X, Y, Z, W); } }

        public void Read(Stream r)
        {
            X = r.ReadS16();
            Y = r.ReadS16();
            Z = r.ReadS16();
            W = r.ReadS16();
        }

        public void Write(Stream w)
        {
            w.WriteS16(X);
            w.WriteS16(Y);
            w.WriteS16(Z);
            w.WriteS16(W);
        }
    }

    public class VertexShort2Value : IVertexComponentValue
    {
        public short X { get; set; }
        public short Y { get; set; }

        public D3DDECLUSAGE Usage { get; set; }
        public D3DDECLTYPE DeclarationType { get { return D3DDECLTYPE.D3DDECLTYPE_SHORT2; } }

        public string Value { get { return string.Format("X: {0} Y: {1}", X, Y); } }

        public void Read(Stream r)
        {
            X = r.ReadS16();
            Y = r.ReadS16();
        }

        public void Write(Stream w)
        {
            w.WriteS16(X);
            w.WriteS16(Y);
        }
    }
}
