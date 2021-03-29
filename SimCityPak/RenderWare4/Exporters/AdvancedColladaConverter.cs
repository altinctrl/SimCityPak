using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Media3D;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Gibbed.Spore.Package;
using SimCityPak.PackageReader;
using SimCityPak.Views.AdvancedEditors;
using SimCityPak;

namespace SporeMaster.RenderWare4
{
    public class AdvancedColladaConverter : IConverter
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

            //create the materials - but only if the material IDs start with "SCP-"
            List<MaterialDefinition> materialDatas = new List<MaterialDefinition>();

            Collada141.library_materials materialLibrary = colladaFile.Items.OfType<Collada141.library_materials>().First();
            if (((Collada141.material)materialLibrary.material[0]).id.StartsWith("SCP-"))
            {
                int materialIndex = 0;
                foreach (Collada141.material mat in materialLibrary.material)
                {
                    string materialString = mat.id;
                    string[] materialValues = materialString.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

                    MaterialDefinition mater = new MaterialDefinition()
                    {
                        Index = int.Parse(materialValues[15], CultureInfo.InvariantCulture.NumberFormat),
                        Id = mat.name,

                        ColorTop = byte.Parse(materialValues[8], CultureInfo.InvariantCulture.NumberFormat),
                        ColorBottom = byte.Parse(materialValues[1], CultureInfo.InvariantCulture.NumberFormat),
                        InteriorData1 = byte.Parse(materialValues[7], CultureInfo.InvariantCulture.NumberFormat),
                        InteriorData2 = byte.Parse(materialValues[2], CultureInfo.InvariantCulture.NumberFormat),

                        TopX = float.Parse(materialValues[3], CultureInfo.InvariantCulture.NumberFormat),
                        TopY = float.Parse(materialValues[4], CultureInfo.InvariantCulture.NumberFormat),
                        TopHeight = float.Parse(materialValues[5], CultureInfo.InvariantCulture.NumberFormat),
                        TopWidth = float.Parse(materialValues[6], CultureInfo.InvariantCulture.NumberFormat),

                        BottomX = float.Parse(materialValues[9], CultureInfo.InvariantCulture.NumberFormat),
                        BottomY = float.Parse(materialValues[10], CultureInfo.InvariantCulture.NumberFormat),
                        BottomHeight = float.Parse(materialValues[11], CultureInfo.InvariantCulture.NumberFormat),
                        BottomWidth = float.Parse(materialValues[12], CultureInfo.InvariantCulture.NumberFormat),

                        TilingTopX = float.Parse(materialValues[13], CultureInfo.InvariantCulture.NumberFormat),
                        TilingTopY = float.Parse(materialValues[14], CultureInfo.InvariantCulture.NumberFormat),
                        InteriorSizeX = 0, //1 / float.Parse(materialValues[9], CultureInfo.InvariantCulture.NumberFormat),
                        InteriorSizeY = 0 //1 / float.Parse(materialValues[10], CultureInfo.InvariantCulture.NumberFormat)
                    };

                    if (mater.InteriorData1 > 0) 
                    {
                        mater.InteriorSizeX = 1;
                        mater.InteriorSizeY = 1;
                    }

                    materialDatas.Add(mater);

                    materialIndex++;
                }

                float[, ,] materialColors = new float[materialDatas.Max(m => m.Index + 1), 4, 4];

                foreach (MaterialDefinition def in materialDatas)
                {

                    materialColors[def.Index, 0, 0] = (float)def.ColorBottom / 256;
                    materialColors[def.Index, 0, 1] = (float)def.ColorTop / 256;
                    materialColors[def.Index, 0, 2] = (float)def.InteriorData1 / 256;
                    materialColors[def.Index, 0, 3] = (float)def.InteriorData2 / 256;

                    materialColors[def.Index, 1, 0] = def.TopX;
                    materialColors[def.Index, 1, 1] = def.TopY;
                    materialColors[def.Index, 1, 2] = def.TopHeight;
                    materialColors[def.Index, 1, 3] = def.TopWidth;

                    materialColors[def.Index, 2, 0] = def.BottomX;
                    materialColors[def.Index, 2, 1] = def.BottomY;
                    materialColors[def.Index, 2, 2] = def.BottomHeight;
                    materialColors[def.Index, 2, 3] = def.BottomWidth;

                    materialColors[def.Index, 3, 0] = def.TilingTopX;
                    materialColors[def.Index, 3, 1] = def.TilingTopY;
                    materialColors[def.Index, 3, 2] = def.InteriorSizeX;
                    materialColors[def.Index, 3, 3] = def.InteriorSizeY;
                }

                //save the materialsbitmap 
                MaterialTextureReference texRef = mesh.model.Materials.Where(m => m.Unknown4 == 0).First();


                DatabaseIndex imageIndex = DatabaseManager.Instance.Indices.Find(idx => idx.InstanceId == texRef.TextureInstanceId && idx.TypeId == PropertyConstants.RW4ImageType);
                if (imageIndex != null)
                {
                    using (MemoryStream imageByteStream = new MemoryStream(imageIndex.GetIndexData(true)))
                    {
                        RW4Model model = new RW4Model();
                        model.Read(imageByteStream);

                        RW4Section textureSection = model.Sections.First(s => s.TypeCode == SectionTypeCodes.Texture);
                        Texture oldSection = textureSection.obj as Texture;
                        uint texDataSection = oldSection.texData.section.Number;

                        Texture newTexture = MaterialTextureConverter.SetTexture(materialColors);

                        newTexture.texData.section = new RW4Section() { Number = texDataSection };
                        newTexture.texData.section.obj = new TextureBlob() { blob = newTexture.texData.blob };

                        textureSection.obj = newTexture;

                        SaveRW4Model(imageIndex, model);
                    }
                }
            }

            int elementIndex = 0;

            //Loop through all nodes in the scene to read the information from the geometry and 
            foreach (Collada141.node node in scene.node)
            {
                Collada141.instance_geometry geometryInstance = null;
                if (node.instance_geometry != null)
                {
                    geometryInstance = node.instance_geometry[0];
                }
                else if (node.node1 != null)
                {
                    if (node.node1[0].instance_geometry != null)
                    {
                        geometryInstance = node.node1[0].instance_geometry[0];
                    }
                }

                //check if the node contains any geometry - lights will be ignored 
                if (geometryInstance != null)
                {
                    Collada141.geometry geo = geometry.geometry.First(g => "#" + g.id == geometryInstance.url);
                    //Collada141.geometry geo = geometry.geometry.First(g => "#" + g.id == node.instance_geometry[0].url);

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
                            VertexUByte4Value normalComponent = (VertexUByte4Value)v.VertexComponents.FirstOrDefault(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_NORMAL && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_UBYTE4);
                            if (normalComponent != null)
                            {
                                normalComponent.X = (byte)((float)normals.Values[normalIndex * 3] * 127.5F + 127.5F);
                                normalComponent.Y = (byte)((float)normals.Values[(normalIndex * 3) + 1] * 127.5F + 127.5F);
                                normalComponent.Z = (byte)((float)normals.Values[(normalIndex * 3) + 2] * 127.5F + 127.5F);
                                normalComponent.W = (byte)(255);
                            }
                            else
                            {
                                //check if there is a different normal component
                                VertexFloat3Value normalComponent2 = (VertexFloat3Value)v.VertexComponents.FirstOrDefault(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_NORMAL && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT3);
                                if (normalComponent2 != null)
                                {
                                    normalComponent2.X = (float)normals.Values[normalIndex * 3];
                                    normalComponent2.Y = (float)normals.Values[(normalIndex * 3) + 1];
                                    normalComponent2.Z = (float)normals.Values[(normalIndex * 3) + 2];
                                }
                            }

                            //Get the external tangents
                            IEnumerable<Collada141.InputLocalOffset> tangentInputs = triangles.input.Where(g => g.semantic == "TEXTANGENT");
                            Collada141.source bottomTangentSrc = geoMesh.source.First(s => "#" + s.id == tangentInputs.ElementAt(0).source);
                            Collada141.float_array bottomTangents = bottomTangentSrc.Item as Collada141.float_array;
                            int interiorTangentIndex = int.Parse(vdef[3]);

                            IEnumerable<IVertexComponentValue> tangentComponents = v.VertexComponents.Where(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TANGENT && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_UBYTE4);
                            if (tangentComponents.Count() != 0)
                            {

                                VertexUByte4Value tangentComponent = (VertexUByte4Value)tangentComponents.ElementAt(0);

                                tangentComponent.X = (byte)((float)bottomTangents.Values[interiorTangentIndex * 3] * 127.5F + 127.5F);
                                tangentComponent.Y = (byte)((float)bottomTangents.Values[(interiorTangentIndex * 3) + 1] * 127.5F + 127.5F);
                                tangentComponent.Z = (byte)((float)bottomTangents.Values[(interiorTangentIndex * 3) + 2] * 127.5F + 127.5F);
                                tangentComponent.W = (byte)(255);

                                //Get the internal tangents
                                if (tangentComponents.Count() > 1)
                                {
                                    Collada141.source internalTangentSrc = geoMesh.source.First(s => "#" + s.id == tangentInputs.ElementAt(1).source);
                                    Collada141.float_array internalTangents = internalTangentSrc.Item as Collada141.float_array;
                                    interiorTangentIndex = int.Parse(vdef[5]);


                                    VertexUByte4Value internalTangentComponent = (VertexUByte4Value)tangentComponents.ElementAt(1);

                                    internalTangentComponent.X = (byte)((float)internalTangents.Values[interiorTangentIndex * 3] * 127.5F + 127.5F);
                                    internalTangentComponent.Y = (byte)((float)internalTangents.Values[(interiorTangentIndex * 3) + 1] * 127.5F + 127.5F);
                                    internalTangentComponent.Z = (byte)((float)internalTangents.Values[(interiorTangentIndex * 3) + 2] * 127.5F + 127.5F);
                                    internalTangentComponent.W = (byte)(255);
                                }
                            }
                            else
                            {
                                //check for other tangents 
                                VertexFloat3Value tangentComponent2 = (VertexFloat3Value)v.VertexComponents.FirstOrDefault(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TANGENT && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT3);
                                if (tangentComponent2 != null)
                                {
                                    tangentComponent2.X = (float)bottomTangents.Values[interiorTangentIndex * 3];
                                    tangentComponent2.Y = (float)bottomTangents.Values[(interiorTangentIndex * 3) + 1];
                                    tangentComponent2.Z = (float)bottomTangents.Values[(interiorTangentIndex * 3) + 2];
                                }
                            }

                            IEnumerable<Collada141.InputLocalOffset> textureCoordinateInputs = triangles.input.Where(g => g.semantic == "TEXCOORD");
                            Collada141.source bottomUVsource = geoMesh.source.First(s => "#" + s.id == textureCoordinateInputs.ElementAt(0).source);
                            Collada141.source topUVsource = null;
                            if (textureCoordinateInputs.Count() > 1)
                            {
                                topUVsource = geoMesh.source.First(s => "#" + s.id == textureCoordinateInputs.ElementAt(1).source);
                            }
                            else
                            {
                                topUVsource = geoMesh.source.First(s => "#" + s.id == textureCoordinateInputs.ElementAt(0).source);
                            }

                            Collada141.float_array bottomTextureCoordinates = bottomUVsource.Item as Collada141.float_array;
                            Collada141.float_array topTextureCoordinates = topUVsource.Item as Collada141.float_array;

                            int uvIndex = int.Parse(vdef[textureCoordinateInputs.ElementAt(0).offset]);
                            int topUvIndex = 0;
                            if (textureCoordinateInputs.Count() > 1)
                            {
                                topUvIndex = int.Parse(vdef[textureCoordinateInputs.ElementAt(1).offset]);
                            }
                            else
                            {
                                topUvIndex = int.Parse(vdef[textureCoordinateInputs.ElementAt(0).offset]);
                            }

                            //Get all the texture elements (should be 2 in the case of a building)
                            IEnumerable<IVertexComponentValue> uvMapComponents = v.VertexComponents.Where(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TEXCOORD && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT4);

                            if (uvMapComponents.Count() != 0)
                            {
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
                            }
                            else
                            {
                                //check for other UVs 
                                VertexFloat2Value uvComponent2 = (VertexFloat2Value)v.VertexComponents.FirstOrDefault(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TEXCOORD && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT2);
                                if (uvComponent2 != null)
                                {
                                    uvComponent2.X = (float)bottomTextureCoordinates.Values[uvIndex * 3];
                                    uvComponent2.Y = (float)bottomTextureCoordinates.Values[(uvIndex * 3) + 1];
                                }

                            }


                            VertexD3DColorValue colorComponent = (VertexD3DColorValue)v.VertexComponents.FirstOrDefault(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_COLOR && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR);
                            if (triangles.material.StartsWith("SCP"))
                            {
                                //get the index of the material
                                MaterialDefinition def = materialDatas.First(m => m.Id == triangles.material);
                                VertexD3DColorValue colorComponent2 = (VertexD3DColorValue)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_COLOR && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR);
                                if (colorComponent != null)
                                {
                                    //Unused
                                    colorComponent2.A = 0;
                                    //Unused
                                    colorComponent2.R = 0;
                                    //Material index
                                    colorComponent2.G = (byte)def.Index;
                                    //RNG for windows
                                    colorComponent2.B = 84;
                                }

                                //if the item has a defined interior, recalculate the interior UV's 
                                if (def.InteriorData1 > 0)
                                {
                                   // IEnumerable<IVertexComponentValue> uvMapComponentInterior = v.VertexComponents.Where(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TEXCOORD && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_FLOAT4);
                                  //  VertexFloat4Value uvInterior = (VertexFloat4Value)uvMapComponents.ElementAt(0);
                                   // uvInterior.X = (uvInterior.X / def.BottomX);
                                   // uvInterior.Y = (uvInterior.Y / def.TopX);
                                }
                            }

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

        private void SaveRW4Model(DatabaseIndex index, RW4Model model)
        {
            //Save the new thing to a stream!

            ModifiedRW4File modifiedData = new ModifiedRW4File();

            foreach (RW4Section section in model.Sections)
            {
                if (section.TypeCode == SectionTypeCodes.Texture)
                {
                    SporeMaster.RenderWare4.Texture tex = section.obj as SporeMaster.RenderWare4.Texture;
                    model.Sections[(int)tex.texData.section.Number].obj = tex.texData;
                }
            }

            RW4Section meshSection = model.Sections.Find(s => s.TypeCode == SectionTypeCodes.Mesh);
            if (meshSection != null)
            {
                SporeMaster.RenderWare4.RW4Mesh mesh = meshSection.obj as SporeMaster.RenderWare4.RW4Mesh;

                model.Sections[(int)mesh.vertices.section.Number].obj = mesh.vertices;
                model.Sections[(int)mesh.triangles.section.Number].obj = mesh.triangles;

                model.Sections[(int)mesh.vertices.vertices.section.Number].obj = mesh.vertices.vertices.section.obj;
                model.Sections[(int)mesh.triangles.triangles.section.Number].obj = mesh.triangles.triangles.section.obj;
            }

            //save back the model
            using (MemoryStream writer = new MemoryStream())
            {
                model.Write(writer);

                modifiedData.RW4FileData = writer.ToArray();
                index.ModifiedData = modifiedData;
                index.IsModified = true;
                index.Compressed = false;
            }
        }

        public class MaterialDefinition
        {
            public byte ColorTop { get; set; }
            public byte ColorBottom { get; set; }
            public byte InteriorData1 { get; set; }
            public byte InteriorData2 { get; set; }

            public float TopX { get; set; }
            public float TopY { get; set; }
            public float TopHeight { get; set; }
            public float TopWidth { get; set; }

            public float BottomX { get; set; }
            public float BottomY { get; set; }
            public float BottomHeight { get; set; }
            public float BottomWidth { get; set; }

            public float TilingTopX { get; set; }
            public float TilingTopY { get; set; }
            public float InteriorSizeX { get; set; }
            public float InteriorSizeY { get; set; }

            public int Index { get; set; }
            public string Id { get; set; }

        }

    }




}
