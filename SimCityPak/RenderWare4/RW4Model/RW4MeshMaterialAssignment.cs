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
    class RWMeshMaterialAssignment : RW4Object
    {
        public const int type_code = 0x2001a;
        public RW4Mesh mesh;
        public RW4TexMetadata[] mat;

        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            var sn1 = r.ReadS32();
            var count = r.ReadU32();        // always exactly 1, so I am guessing
            var sns = new Int32[count];
            for (int i = 0; i < count; i++)
                sns[i] = r.ReadS32();

            m.Sections[sn1].GetObject(m, r, out mesh);
            mat = new RW4TexMetadata[count];
            //for (int i = 0; i < count; i++)
            //    m.Sections[sns[i]].GetObject(m, r, out mat[i]);
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            w.WriteU32(mesh.section.Number);
            w.WriteU32((uint)mat.Length);
            foreach (var x in mat)
                w.WriteU32(x.section.Number);
        }
        public override int ComputeSize()
        {
            return 8 + 4 * mat.Length;
        }
    };
}
