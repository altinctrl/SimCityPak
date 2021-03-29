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
    public partial class RW4Mesh : RW4Object
    {
        public const int type_code = 0x20009;
        public RW4VertexArray vertices = null;
        public RW4TriangleArray triangles = null;
        uint vert_count, tri_count;

        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            // TODO: some files have multiple meshes referencing the same vertex and/or triangle buffers
            r.expect(40, "ME001");
            r.expect(4, "ME002");
            var tri_section = r.ReadS32();
            this.tri_count = r.ReadU32();
            r.expect(1, "ME003");
            r.expect(0, "ME004");
            r.expect(tri_count * 3, "ME005");
            r.expect(0, "ME006");
            this.vert_count = r.ReadU32();
            var vert_section = r.ReadS32();

            m.Sections[tri_section].GetObject(m, r, out triangles);
            if (triangles.triangles.Length != tri_count) throw new ModelFormatException(r, "ME100 Triangle count mismatch", new KeyValuePair<uint, int>(tri_count, triangles.triangles.Length));

            if (vert_section != 0x400000)
            {
                m.Sections[vert_section].GetObject(m, r, out vertices);
                if (vertices.vertices.Length != vert_count) throw new ModelFormatException(r, "ME200 Vertex count mismatch", new KeyValuePair<uint, int>(vert_count, vertices.vertices.Length));
                //seperate the vertices 
                Vertex previousVertex = new Vertex();
                int element = 0;
                for(int j = 0; j < vertices.vertices.Length; j++) 
                {
                    
                    if (previousVertex.VertexComponents != null)
                    {   
                        bool isMatch = true;
                        for (int i = 0; i < vertices.vertices[j].VertexComponents.Count; i++)
                        {
                            if (vertices.vertices[j].VertexComponents[i].DeclarationType == D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR)
                            {
                                if (vertices.vertices[j].VertexComponents[i].Value != previousVertex.VertexComponents[i].Value)
                                    isMatch = false;
                            }
                        }
                        if (!isMatch)
                        {
                            element++;
                        }
                    }
                    Vertex newVertex = vertices.vertices[j];
                    newVertex.Element = element;
                    vertices.vertices[j] = newVertex;
                    previousVertex = vertices.vertices[j];
                }

                vertices.section.obj = vertices;
            }
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            if (vertices != null) vert_count = (uint)vertices.vertices.Length;
            if (triangles != null) tri_count = (uint)triangles.triangles.Length;
            w.WriteU32(40);
            w.WriteU32(4);
            w.WriteU32(triangles.section.Number);
            w.WriteU32(tri_count);
            w.WriteU32(1);
            w.WriteU32(0);
            w.WriteU32(tri_count * 3);
            w.WriteU32(0);
            w.WriteU32(vert_count);
            w.WriteU32(vertices.section.Number);
        }
        public override int ComputeSize() { return 4 * 10; }
    };
}
