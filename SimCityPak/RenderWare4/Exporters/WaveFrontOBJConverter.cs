using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace SporeMaster.RenderWare4
{
    public class WaveFrontOBJConverter : IConverter
    {
        public void Export(RW4Mesh mesh, string fileName)
        {
            using (TextWriter writer = File.CreateText(fileName))
            {
                StringBuilder sb = new StringBuilder();
                IFormatProvider fp = new System.Globalization.CultureInfo("en-US");

                sb.AppendLine("# SimCityPak Wavefront OBJ Exporter");
                sb.AppendFormat(fp, "# File Created: {0} \r\n", DateTime.Now.ToString());
                sb.AppendLine();

                //add vertex coordinates
                foreach (Vertex v in mesh.vertices.vertices)
                {
                    sb.AppendFormat(fp, "v  {0:F12} {1:F12} {2:F12}\r\n", v.Position.X, v.Position.Y, v.Position.Z);
                }
                sb.AppendFormat(fp, "# {0} vertices\r\n", mesh.vertices.vertices.Length);
                sb.AppendLine();

                //add normals
                foreach (Vertex v in mesh.vertices.vertices)
                {
                    sb.AppendFormat(fp, "vn  {0:F12} {1:F12} {2:F12}\r\n", v.Normal.X, v.Normal.Y, v.Normal.Z);
                }
                sb.AppendFormat(fp, "# {0} vertex normals\r\n", mesh.vertices.vertices.Length);
                sb.AppendLine();

                //add texture coordinates
                foreach (Vertex v in mesh.vertices.vertices)
                {
                    sb.AppendFormat(fp, "vt  {0:F12} {1:F12}\r\n", v.TextureCoordinates.X, v.TextureCoordinates.Y);
                }
                sb.AppendFormat(fp, "# {0} texture coordinates\r\n", mesh.vertices.vertices.Length);
                sb.AppendLine();

                //add faces
                foreach (Triangle t in mesh.triangles.triangles)
                {
                    if (t.i != t.j)
                    {
                        sb.AppendFormat(fp, "f  {0} {1} {2}\r\n", t.i + 1, t.j + 1, t.k + 1);
                    }
                }
                sb.AppendFormat(fp, "# {0} faces\r\n", mesh.triangles.triangles.Length);
                sb.AppendLine();

                writer.WriteLine(sb.ToString());
            }
        }

        public RW4Mesh Import(RW4Mesh mesh, string fileName)
        {
            //Retrieve relevant information from the original RW4 Mesh
            uint verticesSectionNumber = mesh.vertices.vertices.section.Number;
            uint trianglesSectionNumber = mesh.triangles.triangles.section.Number;
            VertexFormat vertexFormatSection = (VertexFormat)mesh.model.Sections.First(s => s.TypeCode == SectionTypeCodes.VertexFormat).obj;

            using (TextReader reader = File.OpenText(fileName))
            {
                List<Vector3> vertices = new List<Vector3>();
                List<int[]> triangles = new List<int[]>();

                List<Vector3> vertexNormals = new List<Vector3>();
                List<Vector3> vertexUVs = new List<Vector3>();
                List<KeyValuePair<string, Vertex>> uniqueVertices = new List<KeyValuePair<string, Vertex>>();

                while (reader.Peek() != -1)
                {
                    string lastLine = reader.ReadLine();
                    if (lastLine.StartsWith("v "))
                    {
                        //It's a vertex
                        string[] values = lastLine.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                        Vector3 vertex = new Vector3(
                            float.Parse(values[1].Trim(), CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(values[2].Trim(), CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(values[3].Trim(), CultureInfo.InvariantCulture.NumberFormat)
                            );
                        vertices.Add(vertex);
                    }
                    if (lastLine.StartsWith("vn"))
                    {
                        //It's a vertex normal
                        string[] values = lastLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Vector3 vertexNormal = new Vector3(
                            float.Parse(values[1].Trim(), CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(values[2].Trim(), CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(values[3].Trim(), CultureInfo.InvariantCulture.NumberFormat)
                            );
                        vertexNormals.Add(vertexNormal);
                    }
                    if (lastLine.StartsWith("vt"))
                    {
                        //It's a vertex UV (texture coordinate)
                        string[] values = lastLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Vector3 vertexUV = new Vector3(
                            float.Parse(values[1].Trim(), CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(values[2].Trim(), CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(values[3].Trim(), CultureInfo.InvariantCulture.NumberFormat)
                            );
                        vertexUVs.Add(vertexUV);
                    }
                    if (lastLine.StartsWith("f "))
                    {
                        //It's a face, get all the vertices it consists of
                        string[] values = lastLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int[] triangleValues = new int[values.Length - 1];

                        for (int i = 1; i < values.Length; i++)
                        {
                            if (!uniqueVertices.Exists(uvv => uvv.Key == values[i].Trim()))
                            {
                                string[] vertexInfo = values[i].Split('/');
                                int coord = int.Parse(vertexInfo[0]);
                                int uv = int.Parse(vertexInfo[1]);
                                int normal = int.Parse(vertexInfo[2]);

                                Vertex vert = new Vertex();
                                vert.SetSize(vertexFormatSection.VertexSize);
                                vert.VertexComponents = new List<IVertexComponentValue>();
                                foreach (VertexUsage usage in vertexFormatSection.VertexElements)
                                {
                                    IVertexComponentValue component = VertexComponentValueFactory.CreateComponent(usage.DeclarationType);
                                    component.Usage = usage.Usage;
                                    vert.VertexComponents.Add(component);
                                }

                                VertexFloat3Value positionComponent = (VertexFloat3Value)vert.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_POSITION && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT3);
                                if (positionComponent != null)
                                {
                                    positionComponent.X = vertices[coord - 1].X;
                                    positionComponent.Y = vertices[coord - 1].Y;
                                    positionComponent.Z = vertices[coord - 1].Z;
                                }
                                VertexUByte4Value normalComponent = (VertexUByte4Value)vert.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_NORMAL && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_UBYTE4);
                                if (normalComponent != null)
                                {
                                    normalComponent.X = (byte)(vertexNormals[normal - 1].X  * 127.5F + 127.5F);
                                    normalComponent.Y = (byte)(vertexNormals[normal - 1].Y * 127.5F + 127.5F);
                                    normalComponent.Z = (byte)(vertexNormals[normal - 1].Z * 127.5F + 127.5F);
                                    normalComponent.W = (byte)(255);
                                }
                                VertexFloat4Value uvComponent = (VertexFloat4Value)vert.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TEXCOORD && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT4);
                                uvComponent.X = vertexUVs[uv - 1].X;
                                uvComponent.Y = vertexUVs[uv - 1].Y;
                                uvComponent.Z = 0;
                                uvComponent.W = 0;

                                uniqueVertices.Add(new KeyValuePair<string, Vertex>(values[i], vert));
                            }
                            triangleValues[i - 1] = uniqueVertices.FindIndex(uvv => uvv.Key == values[i]);
                        }

                        if (triangleValues.Length == 4)
                        {
                            int[] triangleDef1 = new int[3];
                            triangleDef1[0] = triangleValues[0];
                            triangleDef1[1] = triangleValues[1];
                            triangleDef1[2] = triangleValues[3];

                            int[] triangleDef2 = new int[3];
                            triangleDef2[0] = triangleValues[1];
                            triangleDef2[1] = triangleValues[2];
                            triangleDef2[2] = triangleValues[3];

                            triangles.Add(triangleDef1);
                            triangles.Add(triangleDef2);
                        }
                        else if (triangleValues.Length == 3)
                        {
                            int[] triangleDef = new int[3];
                            triangleDef[0] = triangleValues[0];
                            triangleDef[1] = triangleValues[1];
                            triangleDef[2] = triangleValues[2];

                            triangles.Add(triangleDef);
                        }
                       
                    }
                }

                //when read fully, set everything

                mesh.vertices.vertices = new SporeMaster.RenderWare4.VertexBuffer(uniqueVertices.Count);
                mesh.vertices.vertices.section = new RW4Section() { Number = verticesSectionNumber };
                mesh.vertices.vertexSize = vertexFormatSection.VertexSize;

                for (int i = 0; i < uniqueVertices.Count; i++)
                {
                    mesh.vertices.vertices[i] = uniqueVertices.ElementAt(i).Value;
                }

                            //mesh.triangles = new RW4TriangleArray();
            mesh.triangles.triangles = new Buffer<Triangle>(triangles.Count);
            mesh.triangles.triangles.section = new RW4Section() { Number = trianglesSectionNumber };


            for (int i = 0; i < triangles.Count; i++)
            {
                mesh.triangles.triangles[i] = new Triangle()
                {
                    i = (uint)triangles[i][0],
                    j = (uint)triangles[i][1],
                    k = (uint)triangles[i][2],
                };
            }

            }
            return mesh;
        }
    }
}
