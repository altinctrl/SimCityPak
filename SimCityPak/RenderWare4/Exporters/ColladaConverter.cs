using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Media3D;
using System.Globalization;

namespace SporeMaster.RenderWare4
{
    public class ColladaConverter : IConverter
    {
        public void Export(RW4Mesh mesh, string fileName)
        {
            throw new NotImplementedException();
        }

        public RW4Mesh Import(RW4Mesh mesh, string fileName)
        {
            Collada141.COLLADA colladaFile = Collada141.COLLADA.Load(fileName);


            //Retrieve relevant information from the original RW4 Mesh
            uint verticesSectionNumber = mesh.vertices.vertices.section.Number;
            uint trianglesSectionNumber = mesh.triangles.triangles.section.Number;
            VertexFormat vertexFormatSection = (VertexFormat)mesh.model.Sections.First(s => s.TypeCode == SectionTypeCodes.VertexFormat).obj;

            //Get the visual_scene object that contains all the relevant scene information
            Collada141.library_visual_scenes scenes = colladaFile.Items.OfType<Collada141.library_visual_scenes>().First();
            Collada141.visual_scene scene = scenes.visual_scene[0];

            //Load the geometries container
            Collada141.library_geometries geometry = colladaFile.Items.OfType<Collada141.library_geometries>().First();

            //Define the lists to which the temporary items can be saved
            List<int> triangleDefinitions = new List<int>();
            List<Vertex> vertexList = new List<Vertex>();
            List<string> vertexDefinitions = new List<string>();

            int elementIndex = 0;

            //Loop through all nodes in the scene to read the information from the geometry and 
            foreach (Collada141.node node in scene.node)
            {
                //check if the node contains any geometry - lights will be ignored
                if (node.instance_geometry != null)
                {
                    Collada141.geometry geo = geometry.geometry.First(g => "#" + g.id == node.instance_geometry[0].url);

                    // Collada141.geometry geo = geometry.geometry[0];
                    Collada141.mesh geoMesh = geo.Item as Collada141.mesh;

                    ///get the array of positions for the vertices
                    Collada141.source src = geoMesh.source.First(s => "#" + s.id == geoMesh.vertices.input.First(g => g.semantic == "POSITION").source);
                    Collada141.float_array positions = src.Item as Collada141.float_array;

                    foreach (Collada141.triangles triangles in geoMesh.Items)
                    {

                        List<string> localVertexDefinitions = new List<string>();
                        //calculate how many input indices each triangle exists of
                        ulong triangleIndexSize = triangles.input.Max(t => t.offset) + 1;
                        string[] triangleIndices = triangles.p.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < triangleIndices.Length; i += (int)triangleIndexSize)
                        {
                            string readVertexDefinition = string.Empty;

                            for (int j = 0; j < (int)triangleIndexSize; j++)
                            {
                                readVertexDefinition += triangleIndices[i + j] + " ";
                            }
                            readVertexDefinition += elementIndex + " ";
                            if (!vertexDefinitions.Contains(readVertexDefinition))
                            {
                                vertexDefinitions.Add(readVertexDefinition);
                                localVertexDefinitions.Add(readVertexDefinition);
                            }
                            triangleDefinitions.Add(vertexDefinitions.IndexOf(readVertexDefinition));
                        }



                        //create a vertex for each definition
                        for (int i = 0; i < localVertexDefinitions.Count; i++)
                        {
                            Vertex v = new Vertex();
                            v.Element = elementIndex;

                            v.SetSize(vertexFormatSection.VertexSize);

                            //Create vertexcomponents in the new vertex
                            v.VertexComponents = new List<IVertexComponentValue>();
                            foreach (VertexUsage usage in vertexFormatSection.VertexElements)
                            {
                                IVertexComponentValue component = VertexComponentValueFactory.CreateComponent(usage.DeclarationType);
                                component.Usage = usage.Usage;
                                v.VertexComponents.Add(component);
                            }




                            string[] vdef = localVertexDefinitions[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            //create vertexcomponents based on 
                            int positionIndex = int.Parse(vdef[0]);
                            VertexFloat3Value positionComponent = (VertexFloat3Value)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_POSITION && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT3);
                            if (positionComponent != null)
                            {
                                positionComponent.X = (float)positions.Values[(positionIndex * 3)];
                                positionComponent.Y = (float)positions.Values[(positionIndex * 3) + 1];
                                positionComponent.Z = (float)positions.Values[(positionIndex * 3) + 2];
                            }

                            Collada141.source normalSrc = geoMesh.source.First(s => "#" + s.id == triangles.input.First(g => g.semantic == "NORMAL").source);
                            Collada141.float_array normals = normalSrc.Item as Collada141.float_array;
                            int normalIndex = int.Parse(vdef[1]);
                            VertexUByte4Value normalComponent = (VertexUByte4Value)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_NORMAL && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_UBYTE4);
                            if (normalComponent != null)
                            {
                                normalComponent.X = (byte)((float)normals.Values[normalIndex * 3] * 127.5F + 127.5F);
                                normalComponent.Y = (byte)((float)normals.Values[(normalIndex * 3) + 1] * 127.5F + 127.5F);
                                normalComponent.Z = (byte)((float)normals.Values[(normalIndex * 3) + 2] * 127.5F + 127.5F);
                                normalComponent.W = (byte)(255);
                            }

                            IEnumerable<Collada141.InputLocalOffset> tangentInputs = triangles.input.Where(g => g.semantic == "TEXTANGENT");
                            Collada141.source bottomTangentSrc = geoMesh.source.First(s => "#" + s.id == tangentInputs.ElementAt(0).source);
                            Collada141.float_array bottomTangents = bottomTangentSrc.Item as Collada141.float_array;
                            int interiorTangentIndex = int.Parse(vdef[3]);

                            IEnumerable<IVertexComponentValue> tangentComponents = v.VertexComponents.Where(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TANGENT && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_UBYTE4);
                            VertexUByte4Value tangentComponent = (VertexUByte4Value)tangentComponents.ElementAt(0);

                            tangentComponent.X = (byte)((float)bottomTangents.Values[interiorTangentIndex * 3] * 127.5F + 127.5F);
                            tangentComponent.Y = (byte)((float)bottomTangents.Values[(interiorTangentIndex * 3) + 1] * 127.5F + 127.5F);
                            tangentComponent.Z = (byte)((float)bottomTangents.Values[(interiorTangentIndex * 3) + 2] * 127.5F + 127.5F);
                            tangentComponent.W = (byte)(255);


                            IEnumerable<Collada141.InputLocalOffset> textureCoordinateInputs = triangles.input.Where(g => g.semantic == "TEXCOORD");
                            Collada141.source bottomUVsource = geoMesh.source.First(s => "#" + s.id == textureCoordinateInputs.ElementAt(0).source);
                            Collada141.source topUVsource = geoMesh.source.First(s => "#" + s.id == textureCoordinateInputs.ElementAt(1).source);

                            Collada141.float_array bottomTextureCoordinates = bottomUVsource.Item as Collada141.float_array;
                            Collada141.float_array topTextureCoordinates = topUVsource.Item as Collada141.float_array;

                            int uvIndex = int.Parse(vdef[textureCoordinateInputs.ElementAt(0).offset]);
                            int topUvIndex = int.Parse(vdef[textureCoordinateInputs.ElementAt(1).offset]);

                            //Get all the texture elements (should be 2 in the case of a building)
                            IEnumerable<IVertexComponentValue> uvMapComponents = v.VertexComponents.Where(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TEXCOORD && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT4);

                            VertexFloat4Value uvBottomComponent = (VertexFloat4Value)uvMapComponents.ElementAt(0);
                            uvBottomComponent.X = (float)bottomTextureCoordinates.Values[(uvIndex * 3)];
                            uvBottomComponent.Y = 1 - (float)bottomTextureCoordinates.Values[(uvIndex * 3) + 1];
                            uvBottomComponent.Z = (float)topTextureCoordinates.Values[(topUvIndex * 3)];
                            uvBottomComponent.W = 1 - (float)topTextureCoordinates.Values[(topUvIndex * 3) + 1];

                            if (uvMapComponents.Count() > 1)
                            {
                                VertexFloat4Value uvTopComponent = (VertexFloat4Value)uvMapComponents.ElementAt(1);
                                uvTopComponent.X = 0;
                                uvTopComponent.Y = 0;
                                uvTopComponent.Z = 1;
                                uvTopComponent.W = 1;

                            }


                            VertexD3DColorValue colorComponent = (VertexD3DColorValue)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_COLOR && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR);
                            if (colorComponent != null)
                            {
                                colorComponent.A = (byte)elementIndex;
                                colorComponent.R = (byte)elementIndex;
                                colorComponent.G = (byte)elementIndex;
                                colorComponent.B = (byte)elementIndex;
                            }

                            if (triangles.material.StartsWith("SCP"))
                            {
                                string materialString = triangles.material;
                                string[] materialValues = materialString.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);


                                VertexD3DColorValue colorComponent2 = (VertexD3DColorValue)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_COLOR && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR);
                                if (colorComponent != null)
                                {
                                    colorComponent2.A = byte.Parse(materialValues[7]);
                                    colorComponent2.R = byte.Parse(materialValues[8]);
                                    colorComponent2.G = byte.Parse(materialValues[1]);
                                    colorComponent2.B = byte.Parse(materialValues[2]);
                                }

                                VertexShort4NValue texComponent1 = (VertexShort4NValue)v.VertexComponents.FirstOrDefault(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TEXCOORD && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_SHORT4N);
                                if (texComponent1 != null)
                                {
                                    texComponent1.X = float.Parse(materialValues[3], CultureInfo.InvariantCulture.NumberFormat);
                                    texComponent1.Y = float.Parse(materialValues[4], CultureInfo.InvariantCulture.NumberFormat);
                                    texComponent1.Z = float.Parse(materialValues[5], CultureInfo.InvariantCulture.NumberFormat);
                                    texComponent1.W = float.Parse(materialValues[6], CultureInfo.InvariantCulture.NumberFormat);
                                }

                                VertexShort4NValue texComponent2 = (VertexShort4NValue)v.VertexComponents.LastOrDefault(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TEXCOORD && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_SHORT4N);
                                if (texComponent2 != null)
                                {
                                    texComponent2.X = float.Parse(materialValues[9], CultureInfo.InvariantCulture.NumberFormat);
                                    texComponent2.Y = float.Parse(materialValues[10], CultureInfo.InvariantCulture.NumberFormat);
                                    texComponent2.Z = float.Parse(materialValues[11], CultureInfo.InvariantCulture.NumberFormat);
                                    texComponent2.W = float.Parse(materialValues[12], CultureInfo.InvariantCulture.NumberFormat);
                                }

                                VertexFloat4Value uvComponent1 = (VertexFloat4Value)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TEXCOORD && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT4);
                                if (uvComponent1 != null)
                                {
                                    if (materialValues[2] == "0")
                                    {
                                        uvComponent1.X = uvComponent1.X * float.Parse(materialValues[3], CultureInfo.InvariantCulture.NumberFormat);
                                        uvComponent1.Y = uvComponent1.Y * float.Parse(materialValues[4], CultureInfo.InvariantCulture.NumberFormat);
                                    }
                                    uvComponent1.Z = uvComponent1.Z * float.Parse(materialValues[9], CultureInfo.InvariantCulture.NumberFormat);
                                    uvComponent1.W = uvComponent1.W * float.Parse(materialValues[10], CultureInfo.InvariantCulture.NumberFormat);
                                }
                                if (uvMapComponents.Count() > 1)
                                {
                                    VertexFloat4Value uvTopComponent = (VertexFloat4Value)uvMapComponents.ElementAt(1);
                                    uvTopComponent.X = float.Parse(materialValues[13], CultureInfo.InvariantCulture.NumberFormat);
                                    uvTopComponent.Y = float.Parse(materialValues[13], CultureInfo.InvariantCulture.NumberFormat);
                                    uvTopComponent.Z = 1;
                                    uvTopComponent.W = 1;

                                }
                            }

                            //Apply transformation
                            
                            //node.ItemsElementName.Where(g => g == Collada141.ItemsChoiceType2.rotate);


                            if (node.Items != null)
                            {
                                for (int transIndex = node.Items.Length - 1; transIndex >= 0; transIndex--)
                                {

                                    v = ApplyTransformation(v, node.Items[transIndex], node.ItemsElementName[transIndex]);
                                }
                            }

                            vertexList.Add(v);
                        }
                        elementIndex++;
                    }
                }
            }

            mesh.vertices.vertices = new SporeMaster.RenderWare4.VertexBuffer(vertexList.Count);
            mesh.vertices.vertices.section = new RW4Section() { Number = verticesSectionNumber };
            mesh.vertices.vertexSize = vertexFormatSection.VertexSize;

            for (int i = 0; i < vertexList.Count; i++)
            {
                mesh.vertices.vertices[i] = vertexList[i];
            }

            //mesh.triangles = new RW4TriangleArray();
            mesh.triangles.triangles = new Buffer<Triangle>(triangleDefinitions.Count / 3);
            mesh.triangles.triangles.section = new RW4Section() { Number = trianglesSectionNumber };


            for (int i = 0; i < triangleDefinitions.Count / 3; i++)
            {
                mesh.triangles.triangles[i] = new Triangle()
                {
                    i = (uint)triangleDefinitions[(i * 3)],
                    j = (uint)triangleDefinitions[(i * 3) + 1],
                    k = (uint)triangleDefinitions[(i * 3) + 2],
                };
            }

          
            return mesh;
        }

        public Vertex ApplyTransformation(Vertex v, object transformation, Collada141.ItemsChoiceType2 elementName)
        {
            if (transformation.GetType() == typeof(Collada141.matrix))
            {
                Collada141.matrix transformationTranslate = transformation as Collada141.matrix;
                Matrix3D empty = new Matrix3D(transformationTranslate.Values[0],
                    transformationTranslate.Values[1],
                    transformationTranslate.Values[2],
                    transformationTranslate.Values[3],
                    transformationTranslate.Values[4],
                    transformationTranslate.Values[5],
                    transformationTranslate.Values[6],
                    transformationTranslate.Values[7],
                    transformationTranslate.Values[8],
                    transformationTranslate.Values[9],
                    transformationTranslate.Values[10],
                    transformationTranslate.Values[11],
                    transformationTranslate.Values[12],
                    transformationTranslate.Values[13],
                    transformationTranslate.Values[14],
                    transformationTranslate.Values[15]);

                VertexFloat3Value positionComponent = (VertexFloat3Value)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_POSITION && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT3);
                if (positionComponent != null)
                {
                    Point3D point = new Point3D(positionComponent.X, positionComponent.Y, positionComponent.Z);
                    Point3D p2 = empty.Transform(point);
                    positionComponent.X = (float)p2.X;
                    positionComponent.Y = (float)p2.Y;
                    positionComponent.Z = (float)p2.Z;
                }
            }

            if (transformation.GetType() == typeof(Collada141.TargetableFloat3))
            {
                if (elementName == Collada141.ItemsChoiceType2.translate)
                {
                    Collada141.TargetableFloat3 transformationTranslate = transformation as Collada141.TargetableFloat3;
                    Matrix3D empty = new Matrix3D();
                    empty.Translate(new Vector3D(transformationTranslate.Values[0], transformationTranslate.Values[1], transformationTranslate.Values[2]));

                    VertexFloat3Value positionComponent = (VertexFloat3Value)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_POSITION && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT3);
                    if (positionComponent != null)
                    {
                        Point3D point = new Point3D(positionComponent.X, positionComponent.Y, positionComponent.Z);
                        Point3D p2 = empty.Transform(point);
                        positionComponent.X = (float)p2.X;
                        positionComponent.Y = (float)p2.Y;
                        positionComponent.Z = (float)p2.Z;
                    }
                }
                else if (elementName == Collada141.ItemsChoiceType2.scale)
                {
                    Collada141.TargetableFloat3 transformationScale = transformation as Collada141.TargetableFloat3;
                    Matrix3D empty = new Matrix3D();
                    empty.Scale(new Vector3D(transformationScale.Values[0], transformationScale.Values[1], transformationScale.Values[2]));

                    VertexFloat3Value positionComponent = (VertexFloat3Value)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_POSITION && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT3);
                    if (positionComponent != null)
                    {
                        Point3D point = new Point3D(positionComponent.X, positionComponent.Y, positionComponent.Z);
                        Point3D p2 = empty.Transform(point);
                        positionComponent.X = (float)p2.X;
                        positionComponent.Y = (float)p2.Y;
                        positionComponent.Z = (float)p2.Z;
                    }
                }
            }

          

            if (transformation.GetType() == typeof(Collada141.rotate))
            {
                Collada141.rotate transformationTranslate = transformation as Collada141.rotate;

                RotateTransform3D empty = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(transformationTranslate.Values[0], transformationTranslate.Values[1], transformationTranslate.Values[2]), transformationTranslate.Values[3]));

                VertexFloat3Value positionComponent = (VertexFloat3Value)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_POSITION && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT3);
                if (positionComponent != null)
                {
                    Point3D point = new Point3D(positionComponent.X, positionComponent.Y, positionComponent.Z);
                    Point3D p2 = empty.Transform(point);
                    positionComponent.X = (float)p2.X;
                    positionComponent.Y = (float)p2.Y;
                    positionComponent.Z = (float)p2.Z;
                }

                //VertexFloat3Value positionComponent = (VertexFloat3Value)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_POSITION && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT3);
                foreach (VertexUByte4Value byteValue in v.VertexComponents.Where(c => (c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_NORMAL) && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_UBYTE4))
                {
                    Vector3D vector = new Vector3D(
                                         (((float)byteValue.X) - 127.5f) / 127.5f,
                                         (((float)byteValue.Y) - 127.5f) / 127.5f,
                                         (((float)byteValue.Z) - 127.5f) / 127.5f
                                       );
                    Vector3D p2 = empty.Transform(vector);
                    byteValue.X = (byte)((float)p2.X * 127.5F + 127.5F);
                    byteValue.Y = (byte)((float)p2.Y * 127.5F + 127.5F);
                    byteValue.Z = (byte)((float)p2.Z * 127.5F + 127.5F);
                }

                //VertexFloat3Value positionComponent = (VertexFloat3Value)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_POSITION && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT3);
                foreach (VertexUByte4Value byteValue in v.VertexComponents.Where(c => (c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TANGENT) && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_UBYTE4))
                {
                    Vector3D vector = new Vector3D(
                                         (((float)byteValue.X) - 127.5f) / 127.5f,
                                         (((float)byteValue.Y) - 127.5f) / 127.5f,
                                         (((float)byteValue.Z) - 127.5f) / 127.5f
                                       );
                    Vector3D p2 = empty.Transform(vector);
                    byteValue.X = (byte)((float)p2.X * 127.5F + 127.5F);
                    byteValue.Y = (byte)((float)p2.Y * 127.5F + 127.5F);
                    byteValue.Z = (byte)((float)p2.Z * 127.5F + 127.5F);
                }



            }
            return v;
        }
    }


}
