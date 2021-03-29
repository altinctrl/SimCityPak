using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework.Graphics;
using SporeMaster.RenderWare4;
using System.Windows.Interop;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Microsoft.Win32;
using System.IO;
using Microsoft.Xna.Framework;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewMesh.xaml
    /// </summary>
    public partial class ViewMesh : UserControl
    {
        public ViewMesh()
        {
            InitializeComponent();




        }

        private BoundingBoxVisual3D _boundingBox;

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(RW4ModelSectionView))
            {
                matDiffuseMain.Brush = new SolidColorBrush(Colors.LightGray);
                RW4ModelSectionView sectionView = this.DataContext as RW4ModelSectionView;

               // listBoxTextures.ItemsSource = sectionView.Model.Sections.Where<RW4Section>(s => s.TypeCode == SectionTypeCodes.Texture);

                RW4Section section = sectionView.Section;
                SporeMaster.RenderWare4.RW4Mesh mesh = section.obj as SporeMaster.RenderWare4.RW4Mesh;

                meshMain.TriangleIndices.Clear();
                meshMain.Positions.Clear();
                meshMain.Normals.Clear();
                meshMain.TextureCoordinates.Clear();


                if (_boundingBox != null)
                {
                    viewPort.Children.Remove(_boundingBox);
                    _boundingBox = null;
                }


                RW4Section bboxSection = sectionView.Model.Sections.FirstOrDefault<RW4Section>(s => s.TypeCode == SectionTypeCodes.BBox);

                try
                {
                    if (mesh != null)
                    {
                        VertexD3DColorValue lastColorValue = null;

                        //separate the mesh into 'chunks' that can be given different textures if necessary
                        foreach (var v in mesh.vertices.vertices)
                        {
                            if (v.VertexComponents.Exists(vc => vc.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR))
                            {
                                VertexD3DColorValue colorVal = v.VertexComponents.First(vc => vc.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR) as VertexD3DColorValue;
                                if (colorVal != lastColorValue)
                                {
                                    //create a new 'chunk'.
                                    ModelVisual3D newChunckContainer = new ModelVisual3D();
                                    viewPort.Children.Add(newChunckContainer);
                                    GeometryModel3D newChunk = new GeometryModel3D();

                                    //  GeometryContainer.Content.add



                                }
                            }


                            VertexFloat3Value position = v.VertexComponents.First(vc => vc.Usage == D3DDECLUSAGE.D3DDECLUSAGE_POSITION) as VertexFloat3Value;

                            meshMain.Positions.Add(new Point3D(position.X, position.Y, position.Z));

                            IVertexComponentValue normal = v.VertexComponents.First(vc => vc.Usage == D3DDECLUSAGE.D3DDECLUSAGE_NORMAL);
                            if (normal != null)
                            {
                                if (normal is VertexUByte4Value)
                                {
                                    VertexUByte4Value normalValue = normal as VertexUByte4Value;
                                    meshMain.Normals.Add(new Vector3D(
                                         (((float)normalValue.X) - 127.5f) / 127.5f,
                                      (((float)normalValue.Y) - 127.5f) / 127.5f,
                                       (((float)normalValue.Z) - 127.5f) / 127.5f
                                       ));
                                }
                                else if (normal is VertexFloat3Value)
                                {
                                    VertexFloat3Value normalValue = normal as VertexFloat3Value;
                                    meshMain.Normals.Add(new Vector3D(normalValue.X, normalValue.Y, normalValue.Z));
                                }
                            }

                            IVertexComponentValue textureCoordinates = v.VertexComponents.First(vc => vc.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TEXCOORD);
                            if (textureCoordinates != null)
                            {
                                if (textureCoordinates is VertexFloat2Value)
                                {
                                    VertexFloat2Value textureCoordinatesValue = textureCoordinates as VertexFloat2Value;
                                    meshMain.TextureCoordinates.Add(new System.Windows.Point(textureCoordinatesValue.X, textureCoordinatesValue.Y));
                                }
                                if (textureCoordinates is VertexFloat4Value)
                                {
                                    VertexFloat4Value textureCoordinatesValue = textureCoordinates as VertexFloat4Value;
                                    meshMain.TextureCoordinates.Add(new System.Windows.Point(textureCoordinatesValue.X, textureCoordinatesValue.Y));
                                }


                            }
                        }
                        foreach (var t in mesh.triangles.triangles)
                        {
                            meshMain.TriangleIndices.Add((int)t.i);
                            meshMain.TriangleIndices.Add((int)t.j);
                            meshMain.TriangleIndices.Add((int)t.k);
                        }


                    }
                }
                catch
                {

                }
                viewPort.ZoomExtents();
            }
        }

        private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void OnViewportMouseMove(object sender, MouseEventArgs e)
        {

        }

        private void Zoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //double s = -Math.Pow(2.0, -Zoom.Value);
            // camMain.Position = new Point3D(camMain.LookDirection.X * s, camMain.LookDirection.Y * s, camMain.LookDirection.Z * s);
        }

        private void Zoom_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // RotateTransform3D tranf = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), Rotate.Value));
            // Model.Transform = tranf;
        }

        private void listBoxTextures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //get the texture
           /* if (listBoxTextures.SelectedItem != null)
            {
                RW4Section textureSection = (RW4Section)listBoxTextures.SelectedItem;
                try
                {
                    SporeMaster.RenderWare4.Texture tex = textureSection.obj as SporeMaster.RenderWare4.Texture;
                    if (tex != null)
                    {
                        ImageBrush brush = new ImageBrush(tex.ToImage());
                        matDiffuseMain.Brush = brush;
                    }
                }
                catch
                {

                }
            }*/
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            
        }


        private void TangentSolver(RW4Mesh mesh)
        {
            int vertexCount = mesh.vertices.vertices.Length;
            SporeMaster.RenderWare4.VertexBuffer vertices = mesh.vertices.vertices;
            Buffer<Triangle> triangles = mesh.triangles.triangles;

            Vector3[] tan1 = new Vector3[vertices.Length];
            Vector3[] tan2 = new Vector3[vertices.Length];

            int triangleCount = mesh.triangles.triangles.Length;
            for (int i = 0; i < triangleCount; i += 3)
            {
                Triangle i1 = triangles[i];

                Vertex v1 = vertices[(int)i1.i];
                Vertex v2 = vertices[(int)i1.j];
                Vertex v3 = vertices[(int)i1.k];

                var x1 = v2.Position.X - v1.Position.X;
                var x2 = v3.Position.X - v1.Position.X;
                var y1 = v2.Position.Y - v1.Position.Y;
                var y2 = v3.Position.Y - v1.Position.Y;
                var z1 = v2.Position.Z - v1.Position.Z;
                var z2 = v3.Position.Z - v1.Position.Z;

                var s1 = v2.TextureCoordinates.X - v1.TextureCoordinates.X;
                var s2 = v3.TextureCoordinates.X - v1.TextureCoordinates.X;
                var t1 = v2.TextureCoordinates.Y - v1.TextureCoordinates.Y;
                var t2 = v3.TextureCoordinates.Y - v1.TextureCoordinates.Y;

                float r = 1.0f / (s1 * t2 - s2 * t1);

                var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1.i] += sdir;
                tan1[i1.j] += sdir;
                tan1[i1.k] += sdir;

                tan2[i1.i] += tdir;
                tan2[i1.j] += tdir;
                tan2[i1.k] += tdir;
            }


            for (int i = 0; i < mesh.vertices.vertices.Length; i++)
            {

                Vector3 normal = new Vector3(mesh.vertices.vertices[i].Normal.X,
                    mesh.vertices.vertices[i].Normal.Y,
                    mesh.vertices.vertices[i].Normal.Z);

                // Gram-Schmidt orthogonalize  
                Vector3 tangent = tan1[i] - normal * Vector3.Dot(normal, tan1[i]);
                tangent.Normalize();

                // Calculate handedness (here maybe you need to switch >= with <= depend on the geometry winding order)  
                float tangentdir = (Vector3.Dot(Vector3.Cross(normal, tan1[i]), tan2[i]) >= 0.0f) ? 1.0f : -1.0f;
                Vector3 binormal = Vector3.Cross(normal, tangent) * tangentdir;

                Vertex v = mesh.vertices.vertices[i];

                VertexUByte4Value tangentValue = (VertexUByte4Value)v.VertexComponents.First(c => c.Usage == D3DDECLUSAGE.D3DDECLUSAGE_TANGENT && c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_UBYTE4);
                tangentValue.X = (byte)(tangent.X * 127.5F + 127.5F);
                tangentValue.Y = (byte)(tangent.Y * 127.5F + 127.5F);
                tangentValue.Z = (byte)(tangent.Z * 127.5F + 127.5F);
                tangentValue.W = (byte)255;

                mesh.vertices.vertices[i] = v;
            }

        }
    
       private void mnuExportOBJ_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".obj";
            dlg.Filter = "WaveFront .OBJ|*.obj";
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                if (this.DataContext != null && this.DataContext.GetType() == typeof(RW4ModelSectionView))
                {
                    matDiffuseMain.Brush = new SolidColorBrush(Colors.LightGray);
                    RW4ModelSectionView sectionView = this.DataContext as RW4ModelSectionView;

                   // listBoxTextures.ItemsSource = sectionView.Model.Sections.Where<RW4Section>(s => s.TypeCode == SectionTypeCodes.Texture);

                    RW4Section section = sectionView.Section;
                    RW4Mesh mesh = section.obj as RW4Mesh;
                    mesh.Export(dlg.FileName);
                }
            }
        }

        private void mnuImportCollada_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".dae";
            dlg.Filter = "COLLADA|*.dae";
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                RW4ModelSectionView sectionView = this.DataContext as RW4ModelSectionView;
                VertexFormat vertexFormatSection = (VertexFormat)sectionView.Model.Sections.First(s => s.TypeCode == SectionTypeCodes.VertexFormat).obj;

                RW4Section section = sectionView.Section;
                SporeMaster.RenderWare4.RW4Mesh meshSection = section.obj as SporeMaster.RenderWare4.RW4Mesh;

                IConverter converter = new AdvancedColladaConverter();

                RW4Mesh newMesh = converter.Import(meshSection, dlg.FileName);

                meshSection.vertices.vertices.section.obj = newMesh.vertices.vertices;
                meshSection.triangles.triangles.section.obj = newMesh.triangles.triangles;

                this.DataContext = sectionView;

                section.Changed();
            }
        }

        private void mnuImportOBJ_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".obj";
            dlg.Filter = "Wavefront .OBJ|*.obj";
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                RW4ModelSectionView sectionView = this.DataContext as RW4ModelSectionView;
                VertexFormat vertexFormatSection = (VertexFormat)sectionView.Model.Sections.First(s => s.TypeCode == SectionTypeCodes.VertexFormat).obj;

                RW4Section section = sectionView.Section;
                SporeMaster.RenderWare4.RW4Mesh meshSection = section.obj as SporeMaster.RenderWare4.RW4Mesh;

                IConverter converter = new WaveFrontOBJConverter();

                RW4Mesh newMesh = converter.Import(meshSection, dlg.FileName);

                meshSection.vertices.vertices.section.obj = newMesh.vertices.vertices;
                meshSection.triangles.triangles.section.obj = newMesh.triangles.triangles;

                this.DataContext = sectionView;

                section.Changed();
            }
        }

        private void mnuRecalculateTangent_Click(object sender, RoutedEventArgs e)
        {
            RW4ModelSectionView sectionView = this.DataContext as RW4ModelSectionView;
            RW4Section section = sectionView.Section;

            SporeMaster.RenderWare4.RW4Mesh meshSection = section.obj as SporeMaster.RenderWare4.RW4Mesh;

            TangentSolver(meshSection);

            section.Changed();
        }
    }
}
