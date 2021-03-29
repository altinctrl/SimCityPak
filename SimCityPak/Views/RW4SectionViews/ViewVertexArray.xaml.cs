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
using SporeMaster.RenderWare4;
using Microsoft.Xna.Framework;
using SimCityPak.Views.RW4SectionViews.VertexValueViews;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewVertexArray.xaml
    /// </summary>
    public partial class ViewVertexArray : UserControl
    {
        public ViewVertexArray()
        {
            InitializeComponent();
        }

        public byte[] Bytes = { 1, 2 };

        public WriteableBitmap ColorBitmap { get; set; }

        RW4Section section = null;

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(RW4ModelSectionView))
            {
                if (dataGridVertices != null)
                {
                    dataGridVertices.Columns.Clear();
                    RW4ModelSectionView sectionView = this.DataContext as RW4ModelSectionView;
                    section = sectionView.Section;
                    //section = this.DataContext as RW4Section;
                    SporeMaster.RenderWare4.RW4VertexArray vertexArraySection = section.obj as SporeMaster.RenderWare4.RW4VertexArray;

                    if (vertexArraySection.vertices.Length > 0)
                    {
                        Vertex firstVertex = vertexArraySection.vertices[0];

                        int i = 0;
                        foreach (IVertexComponentValue value in firstVertex.VertexComponents)
                        {
                            DataGridTextColumn textColumn = new DataGridTextColumn();
                            textColumn.Header = value.Usage.ToString();
                            textColumn.Binding = new Binding(string.Format("VertexComponents[{0}].Value", i));
                            dataGridVertices.Columns.Add(textColumn);
                            i++;
                        }

                        DataGridTextColumn textColumnElement = new DataGridTextColumn();
                        textColumnElement.Header = "Element";
                        textColumnElement.Binding = new Binding(string.Format("Element", i));
                        dataGridVertices.Columns.Add(textColumnElement);

                        //load materials
                        List<MaterialTextureReference> materials = sectionView.Model.Materials;
                        dataGridVertices.ItemsSource = vertexArraySection.vertices.ToList<Vertex>();

                        VertexShort4NValue oldTexVal = new VertexShort4NValue();

                       // WriteableBitmap colorMap = materials[4].GetBitmap();

                        List<ModelElement> Elements = new List<ModelElement>();
                        try
                        {
                            IEnumerable<int> elements = vertexArraySection.vertices.Select(s => s.Element).Distinct();
                            foreach (int el in elements)
                            {
                                Vertex elementVertex = vertexArraySection.vertices.First(v => v.Element == el);
                                IEnumerable<IVertexComponentValue> textureSizeElements = elementVertex.VertexComponents.Where(c => c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_SHORT4N);
                                Elements.Add(new ModelElement()
                                {
                                    ElementId = el,
                                    MaterialEntry = materials,
                                    ColorComponent = elementVertex.VertexComponents.First(c => c.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR) as VertexD3DColorValue,
                                    TextureCoordinatesBaseLayer = textureSizeElements.ElementAt(0) as VertexShort4NValue,
                                    TextureCoordinatesTopLayer = textureSizeElements.ElementAt(1)  as VertexShort4NValue
                                });       
                            }
                            dataGridElements.ItemsSource = Elements;
                            this.ColorBitmap = materials[4].GetBitmap();
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //Get the clicked MenuItem
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            Vertex clickedVertex = (Vertex)((System.Windows.Controls.DataGridCell)(contextMenu.PlacementTarget)).DataContext;
            int columnIndex = ((System.Windows.Controls.DataGridCell)(contextMenu.PlacementTarget)).Column.DisplayIndex;

            if (clickedVertex.VertexComponents[columnIndex].DeclarationType == D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR)
            {
                VertexD3DColorValue colorValue = (VertexD3DColorValue)clickedVertex.VertexComponents[columnIndex];

                ViewD3DColor dialog = new ViewD3DColor(colorValue);
                if (dialog.ShowDialog() == true)
                {
                    SporeMaster.RenderWare4.RW4VertexArray vertexArraySection = section.obj as SporeMaster.RenderWare4.RW4VertexArray;
                    foreach (Vertex vertex in vertexArraySection.vertices)
                    {

                        foreach (IVertexComponentValue val in vertex.VertexComponents.Where(v => v.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR))
                        {
                            VertexD3DColorValue val2 = val as VertexD3DColorValue;
                            if (val2.A == colorValue.A &&
                                val2.R == colorValue.R &&
                                val2.G == colorValue.G &&
                                val2.B == colorValue.B)
                            {
                                val2.A = dialog.A;
                                val2.R = dialog.R;
                                val2.G = dialog.G;
                                val2.B = dialog.B;
                            }
                        }
                    }
                    if (section != null)
                    {
                        section.Changed();
                    }
                }
            }
            if (clickedVertex.VertexComponents[columnIndex].DeclarationType == D3DDECLTYPE.D3DDECLTYPE_UBYTE4)
            {
                VertexUByte4Value colorValue = (VertexUByte4Value)clickedVertex.VertexComponents[columnIndex];

                ViewUByte4 dialog = new ViewUByte4(colorValue);
                if (dialog.ShowDialog() == true)
                {
                    SporeMaster.RenderWare4.RW4VertexArray vertexArraySection = section.obj as SporeMaster.RenderWare4.RW4VertexArray;
                    foreach (Vertex vertex in vertexArraySection.vertices)
                    {

                        foreach (IVertexComponentValue val in vertex.VertexComponents.Where(v => v.DeclarationType == D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR))
                        {
                            VertexUByte4Value val2 = val as VertexUByte4Value;
                            if (val2.X == colorValue.X &&
                                val2.Y == colorValue.Y &&
                                val2.Z == colorValue.Z &&
                                val2.W == colorValue.W)
                            {
                                val2.X = dialog.X;
                                val2.Y = dialog.Y;
                                val2.Z = dialog.Z;
                                val2.W = dialog.W;
                            }
                        }
                    }
                    if (section != null)
                    {
                        section.Changed();
                    }
                }
            }
            if (clickedVertex.VertexComponents[columnIndex].DeclarationType == D3DDECLTYPE.D3DDECLTYPE_SHORT4N)
            {
                VertexShort4NValue colorValue = (VertexShort4NValue)clickedVertex.VertexComponents[columnIndex];

                ViewShort4N dialog = new ViewShort4N(colorValue);
                if (dialog.ShowDialog() == true)
                {
                    SporeMaster.RenderWare4.RW4VertexArray vertexArraySection = section.obj as SporeMaster.RenderWare4.RW4VertexArray;
                    foreach (Vertex vertex in vertexArraySection.vertices)
                    {


                        VertexShort4NValue val2 = (VertexShort4NValue)vertex.VertexComponents[columnIndex];
                        if (val2.X == colorValue.X &&
                            val2.Y == colorValue.Y &&
                            val2.Z == colorValue.Z &&
                            val2.W == colorValue.W)
                        {
                            val2.X = dialog.X;
                            val2.Y = dialog.Y;
                            val2.Z = dialog.Z;
                            val2.W = dialog.W;
                        }

                    }
                    if (section != null)
                    {
                        section.Changed();
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            List<ModelElement> elements = (List<ModelElement>)dataGridElements.ItemsSource;
            SporeMaster.RenderWare4.RW4VertexArray vertexArraySection = section.obj as SporeMaster.RenderWare4.RW4VertexArray;
            foreach (ModelElement el in elements)
            {
                foreach (Vertex vertex in vertexArraySection.vertices.Where(v => v.Element == el.ElementId))
                {
                    vertex.VertexComponents[3] = el.ColorComponent;
                    vertex.VertexComponents[5] = el.TextureCoordinatesBaseLayer;
                    vertex.VertexComponents[6] = el.TextureCoordinatesTopLayer;
                }
            }
            if (section != null)
            {
                section.Changed();
            }
        }

    }

    /// <summary>
    /// Has set components because this will only ever be used for buildings
    /// Buildings have the following unique setup per element:
    /// D3DColor
    /// Texture coordinates base layer
    /// Texture coordinates top layer
    /// </summary>
    public class ModelElement
    {

        public byte[] Bytes = { 1, 2 };
        public int ElementId { get; set; }

        public List<MaterialTextureReference> MaterialEntry { get; set; }

        public VertexD3DColorValue ColorComponent { get; set; }
        public VertexShort4NValue TextureCoordinatesBaseLayer { get; set; }
        public VertexShort4NValue TextureCoordinatesTopLayer { get; set; }

        public byte Color1
        {
            get
            {
                return ColorComponent.A;
            }
            set
            {
                ColorComponent.A = value;
            }
        }
        public byte Color2
        {
            get
            {
                return ColorComponent.R;
            }
            set
            {
                ColorComponent.R = value;
            }
        }
        public byte Color3
        {
            get
            {
                return ColorComponent.G;
            }
            set
            {
                ColorComponent.G = value;
            }
        }
        public byte Color4
        {
            get
            {
                return ColorComponent.B;
            }
            set
            {
                ColorComponent.B = value;
            }
        }

        public WriteableBitmap BaseLayerBitmap
        {
            get
            {
                return MaterialEntry[1].GetBitmap((float)TextureCoordinatesBaseLayer.Z,
                    (float)TextureCoordinatesBaseLayer.W,
                    (float)TextureCoordinatesBaseLayer.X,
                    (float)TextureCoordinatesBaseLayer.Y);
            }
        }

        public WriteableBitmap TopLayerBitmap
        {
            get
            {
                return MaterialEntry[1].GetBitmap((float)TextureCoordinatesTopLayer.Z,
                    (float)TextureCoordinatesTopLayer.W,
                    (float)TextureCoordinatesTopLayer.X,
                    (float)TextureCoordinatesTopLayer.Y);
            }
        }
    }
}
