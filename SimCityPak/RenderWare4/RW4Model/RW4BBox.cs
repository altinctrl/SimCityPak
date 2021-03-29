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
    class RW4BBox : RW4Object
    {
        public const int type_code = 0x80005;  //< When found alone.  Also used in RW4SimpleMesh
        public float minx, miny, minz;
        public float maxx, maxy, maxz;
        public uint unk1, unk2;

        public bool IsIdentical(RW4BBox b)
        {
            return minx == b.minx && miny == b.miny && minz == b.minz && maxx == b.maxx && maxy == b.maxy && maxz == b.maxz &&
                unk1 == b.unk1 && unk2 == b.unk2;
        }

        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            minx = r.ReadF32(); miny = r.ReadF32(); minz = r.ReadF32(); unk1 = r.ReadU32();
            maxx = r.ReadF32(); maxy = r.ReadF32(); maxz = r.ReadF32(); unk2 = r.ReadU32();
            if (minx > maxx || miny > maxy || minz > maxz) throw new ModelFormatException(r, "BBOX011", null);
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            w.WriteF32(minx); w.WriteF32(miny); w.WriteF32(minz); w.WriteU32(unk1);
            w.WriteF32(maxx); w.WriteF32(maxy); w.WriteF32(maxz); w.WriteU32(unk2);
        }
        public override int ComputeSize() { return 4 * 8; }
    };
}
