using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Spore.Helpers;
using System.Diagnostics;
using SimCityPak;
using System.Globalization;

namespace SporeMaster.RenderWare4
{
    public interface IRW4Struct
    {
        void Read(Stream r);
        void Write(Stream w);
        uint Size();
    };




    public struct Mat4x4 : IRW4Struct
    {
        public float[] m;
        public uint Size() { return 16 * 4; }
        public void Read(Stream s)
        {
            m = new float[16];
            for (int i = 0; i < m.Length; i++) m[i] = s.ReadF32();
        }
        public void Write(Stream s)
        {
            foreach (var f in m) s.WriteF32(f);
        }
    };

    public struct Mat4x3 : IRW4Struct
    {
        public float[] m;
        public uint Size() { return 12 * 4; }
        public void Read(Stream s)
        {
            m = new float[12];
            for (int i = 0; i < m.Length; i++) m[i] = s.ReadF32();
        }
        public void Write(Stream s)
        {
            foreach (var f in m) s.WriteF32(f);
        }
    };
}
