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
    public class RW4TriangleArray : RW4Object
    {
        public const int type_code = 0x20007;
        public Buffer<Triangle> triangles;
        public uint unk1;

        public override int ComputeSize() { return 4 * 7; }
        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            if (s.type_code != type_code) throw new ModelFormatException(r, "TA000 Bad type code", s.type_code);
            unk1 = r.ReadU32();
            r.expect(0, "TA001");
            var ind_count = r.ReadU32();
            r.expect(8, "TA002");
            r.expect(101, "TA003");
            r.expect(4, "TA004");
            var section_number = r.ReadS32();
            if (ind_count % 3 != 0) throw new ModelFormatException(r, "TA010", ind_count);
            var tri_count = ind_count / 3;

            var section = m.Sections[section_number];
            section.LoadObject(m, triangles = new Buffer<Triangle>((int)tri_count), r);
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            w.WriteU32(unk1);
            w.WriteU32(0);
            w.WriteU32((uint)triangles.Length * 3);
            w.WriteU32(8);
            w.WriteU32(101);
            w.WriteU32(4);
            w.WriteU32(triangles.section.Number);
        }
    }
}
