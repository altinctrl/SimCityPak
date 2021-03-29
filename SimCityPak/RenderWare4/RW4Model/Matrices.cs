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
    class Matrices<T> : RW4Object where T : IRW4Struct, new()
    {
        static int Tsize = (int)(new T()).Size();
        public T[] items;
        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            var p1 = r.ReadU32();
            var count = r.ReadU32();
            r.expect(0, "MS001");      /// These bytes are padding to 16 bytes, but this structure is itself always aligned
            r.expect(0, "MS002");
            if (p1 != r.Position)
                throw new ModelFormatException(r, "MS001", p1);
            items = new T[count];
            for (int i = 0; i < count; i++)
                items[i].Read(r);
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            var p1 = (uint)w.Position + 16;
            w.WriteU32(p1);
            w.WriteU32((uint)items.Length);
            w.WriteU32(0);
            w.WriteU32(0);
            foreach (var i in items)
                i.Write(w);
        }
        public override int ComputeSize()
        {
            return 16 + Tsize * items.Length;
        }
    };

    class Matrices4x4 : Matrices<Mat4x4>
    {
        public const int type_code = 0x70003;
    };

    class Matrices4x3 : Matrices<Mat4x3>
    {
        public const int type_code = 0x7000f;
    };

}
