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
    public struct Triangle : IRW4Struct
    {
        public UInt32 i,j,k;
        public byte unk1;           //< 4 bits found in "SimpleMesh", in a parallel section
        public void Read(Stream r)
        {
            i = r.ReadU16(); j = r.ReadU16(); k = r.ReadU16();
        }
     
        public void Write(Stream w){
            w.WriteU16((ushort)i); w.WriteU16((ushort)j); w.WriteU16((ushort)k);
        }
        public uint Size() { return 6; }
    }
}
