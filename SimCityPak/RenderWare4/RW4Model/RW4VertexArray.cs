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
    public class RW4VertexArray : RW4Object
    {
        public const uint type_code = 0x20005;
        public VertexFormat format;  // Vertex format?
        public UInt32 unk2;             // Dangling pointer from data structure??
        public VertexBuffer vertices;
        public int vertexFormatSection;
        public uint vertexSize;

        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            if (s.type_code != type_code) throw new ModelFormatException(r, "VA000 Bad type code", s.type_code);
            var section_1 = r.ReadS32();
            vertexFormatSection = section_1;
            this.unk2 = r.ReadU32();
            r.expect(0, "VA001");
            var vcount = r.ReadU32();
            r.expect(8, "VA002");

            vertexSize = r.ReadU32();

            var section_number = r.ReadS32();

            var section = m.Sections[section_number];
            section.LoadObject(m, vertices = new VertexBuffer((int)vcount), r);

            section = m.Sections[section_1];
            if (section.type_code != VertexFormat.type_code)
                throw new ModelFormatException(r, "VA101 Bad section type code", section.type_code);
            section.GetObject(m, r, out format); //get the stuff
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            w.WriteU32((uint)this.vertexFormatSection);
            w.WriteU32(this.unk2);
            w.WriteU32(0);
            w.WriteU32((uint)vertices.Length);
            w.WriteU32(8);
            w.WriteU32(vertexSize);
            w.WriteU32(vertices.section.Number);
        }
        public override int ComputeSize() { return 4 * 7; }
    };
}
