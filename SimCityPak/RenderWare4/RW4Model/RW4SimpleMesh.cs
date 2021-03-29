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
    class RW4SimpleMesh : RW4Object
    {
        public const int type_code = 0x80003;
        RW4BBox bbox = new RW4BBox();
        uint unk1;
        public Vertex[] vertices;
        public Triangle[] triangles;
        uint[] unknown_data_2;

        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            var p0 = r.Position;
            bbox.Read(m, null, r);
            r.expect(0xd59208, "SM001");      // most but not all creature models
            this.unk1 = r.ReadU32();
            var tri_count = r.ReadU32();
            r.expect(0, "SM002");
            var vertexcount = r.ReadU32();

            var p2 = r.ReadU32();
            var p1 = r.ReadU32();
            var p4 = r.ReadU32();
            var p3 = r.ReadU32();

            if (p1 != (uint)((r.Position + 15) & ~15)) throw new ModelFormatException(r, "SM010", null);

            r.ReadPadding(p1 - r.Position);

            if (p2 != p1 + vertexcount * 16) throw new ModelFormatException(r, "SM101", null);
            if (p3 != p2 + tri_count * 16) throw new ModelFormatException(r, "SM102", null);
            if (p4 != p3 + (((tri_count / 2) + 15) & ~15)) throw new ModelFormatException(r, "SM103", null);

            vertices = new Vertex[vertexcount];
            for (int i = 0; i < vertexcount; i++)
                vertices[i].Read4(r);

            triangles = new Triangle[tri_count];
            for (int t = 0; t < tri_count; t++)
            {
                var index = r.ReadU32(); if (index >= vertexcount) throw new ModelFormatException(r, "SM200", t);
                triangles[t].i = (UInt16)index;
                index = r.ReadU32(); if (index >= vertexcount) throw new ModelFormatException(r, "SM200", t);
                triangles[t].j = (UInt16)index;
                index = r.ReadU32(); if (index >= vertexcount) throw new ModelFormatException(r, "SM200", t);
                triangles[t].k = (UInt16)index;
                r.expect(0, "SM201");
            }

            UInt32 x = 0;
            for (int t = 0; t < tri_count; t++)
            {
                if ((t & 7) == 0)
                    x = r.ReadU32();
                triangles[t].unk1 = (byte)((x >> ((t & 7) * 4)) & 0xf);
            }
            for (int t = (int)tri_count; t < ((tri_count + 7) & ~7); t++)
                if ((byte)((x >> ((t & 7) * 4)) & 0xf) != 0xf)
                    throw new ModelFormatException(r, "SM210", t);
            r.ReadPadding(p4 - r.Position);
            //if (r.Position != p4) throw new ModelFormatException(r, "SM299", r.Position - p4);

            r.expect(p1 - 8 * 4, "SM301");
            var u2_count = r.ReadU32();
            r.expect(tri_count, "SM302");
            r.expect(0, "SM303");

            var bbox2 = new RW4BBox();
            bbox2.Read(m, null, r);
            if (!bbox.IsIdentical(bbox2)) throw new ModelFormatException(r, "SM310", bbox2);

            unknown_data_2 = new uint[u2_count * 8];  // Actually this is int*6 + float*2
            for (int i = 0; i < unknown_data_2.Length; i++)
                unknown_data_2[i] = r.ReadU32();

        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            var tri_count = triangles.Length;

            bbox.Write(m, null, w);
            w.WriteU32(0xd59208);
            w.WriteU32(this.unk1);
            w.WriteS32(tri_count);
            w.WriteU32(0);
            w.WriteS32(vertices.Length);

            long p1 = (w.Position + 4 * 4 + 15) & ~15;
            var p4 = p1 + vertices.Length * 16 + tri_count * 16 + (((tri_count / 2) + 15) & ~15);
            w.WriteU32((uint)(p1 + vertices.Length * 16));
            w.WriteU32((uint)p1);
            w.WriteU32((uint)p4);
            w.WriteU32((uint)(p1 + vertices.Length * 16 + tri_count * 16));

            w.WritePadding(p1 - w.Position);

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Write4(w);
            for (int t = 0; t < tri_count; t++)
            {
                w.WriteU32(triangles[t].i);
                w.WriteU32(triangles[t].j);
                w.WriteU32(triangles[t].k);
                w.WriteU32(0);
            }
            UInt32 pack = 0;
            for (int t = 0; t < tri_count; t++)
            {
                pack |= ((UInt32)triangles[t].unk1) << ((t & 7) * 4);
                if ((t & 7) == 7)
                {
                    w.WriteU32(pack);
                    pack = 0;
                }
            }
            for (int t = (int)tri_count; t < ((tri_count + 7) & ~7); t++)
            {
                pack |= 0xfU << ((t & 7) * 4);
                if ((t & 7) == 7)
                    w.WriteU32(pack);
            }

            w.WritePadding(p4 - w.Position);

            w.WriteU32((uint)(p1 - 8 * 4));
            w.WriteU32((uint)(unknown_data_2.Length / 8));
            w.WriteU32((uint)tri_count);
            w.WriteU32(0);
            bbox.Write(m, null, w);
            w.WriteU32s(unknown_data_2);
        }
    };
}
