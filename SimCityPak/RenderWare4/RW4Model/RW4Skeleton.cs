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
    class RW4Skeleton : RW4Object
    {
        public const int type_code = 0x7000c;
        public Matrices4x3 mat3;
        public Matrices4x4 mat4;
        public RW4HierarchyInfo jointInfo;
        public UInt32 unk1;
        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            if (s.type_code != type_code) throw new ModelFormatException(r, "SK000", s.type_code);
            r.expect(0x400000, "SK001");

            unk1 = r.ReadU32();
            //  r.expect(unk1=0x8d6da0, "SK002");  //< TODO: Not universal, but not sure other decoding is robust with other values
            var sn1 = r.ReadS32();
            var sn2 = r.ReadS32();
            var sn3 = r.ReadS32();

            m.Sections[sn1].GetObject(m, r, out mat3);
            m.Sections[sn2].GetObject(m, r, out jointInfo);
            m.Sections[sn3].GetObject(m, r, out mat4);
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            w.WriteU32(0x400000);
            w.WriteU32(unk1);
            w.WriteU32(mat3.section.Number);
            w.WriteU32(jointInfo.section.Number);
            w.WriteU32(mat4.section.Number);
        }
        public override int ComputeSize() { return 4 * 5; }
    };
}
