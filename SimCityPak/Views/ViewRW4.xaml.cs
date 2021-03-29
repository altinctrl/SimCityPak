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
using System.IO;
using Gibbed.Spore.Helpers;
using SporeMaster.RenderWare4;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Interop;
using Gibbed.Spore.Package;
using Microsoft.Win32;
using SimCityPak.Views;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewRW4.xaml
    /// </summary>
    public partial class ViewRW4 : UserControl
    {
        public ViewRW4()
        {
            InitializeComponent();
        }

        private RW4Model _rw4model;
        public RW4Model Rw4model
        {
            get { return _rw4model; }
            set { _rw4model = value; }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null)
            {
                try
                {
                    DatabaseIndexData index = (DatabaseIndexData)this.DataContext;

                    _rw4model = new RW4Model();
                    using (Stream stream = new MemoryStream(index.Data))
                    {
                        _rw4model.Read(stream);


                    }
                    //textBlockRW4Type.Text = model.FileType.ToString();

                    // gridHeaderDetails.DataContext = _rw4model.Header;
                    txtHeaderSectionBegin.Text = _rw4model.Header.SectionIndexBegin.ToString();
                    txtHeaderPadder.Text = _rw4model.Header.SectionIndexPadding.ToString();
                    txtHeaderEnd.Text = _rw4model.Header.SectionIndexEnd.ToString();
                    txtHeaderEndPos.Text = _rw4model.Header.HeaderEnd.ToString();

                    dataGrid1.ItemsSource = _rw4model.Sections;

                    _rw4model.Sections.ForEach(t => t.SectionChanged += new EventHandler(section_SectionChanged));

                    RW4Section sec = _rw4model.Sections.FirstOrDefault(s => s.TypeCode == SectionTypeCodes.Mesh);
                    if (sec != null)
                    {
                        dataGrid1.SelectedItem = sec;
                    }
                    else
                    {
                        RW4Section sec2 = _rw4model.Sections.FirstOrDefault(s => s.TypeCode == SectionTypeCodes.Texture);
                        dataGrid1.SelectedItem = sec2;
                    }
                }
                catch
                {

                }

            }


        }

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSection((RW4Section)dataGrid1.SelectedItem);
        }

        private void LoadSection(RW4Section section)
        {
            if (section != null)
            {
                if (section.GetType() == typeof(RW4Section))
                {
                    try
                    {
                        viewContainer.Content = new RW4ModelSectionView() { Section = section, Model = _rw4model };
                    }
                    catch { }
                    try
                    {
                        DatabaseIndexData index = (DatabaseIndexData)this.DataContext;

                        byte[] buffer = new byte[section.Size];
                        using (Stream stream = new MemoryStream(index.Data))
                        {
                            stream.Seek((int)section.Pos, SeekOrigin.Begin);
                            stream.Read(buffer, 0, (int)section.Size);
                        }
                        viewHex.DataContext = buffer;
                    }
                    catch
                    {
                    }
                }
            }
        }

        void section_SectionChanged(object sender, EventArgs e)
        {
            if (sender is RW4Section)
            {
                SaveRW4Model();
                //Reload the current section, for it has changed

                //  dataGrid1.SelectedItem = sender as RW4Section;

                LoadSection(sender as RW4Section);
            }
        }

        private void buttonExport_Click(object sender, RoutedEventArgs e)
        {

            SaveRW4Model();


        }

        private void SaveRW4Model()
        {
            //Save the new thing to a stream!

            if (this.DataContext != null)
            {
                //  try
                //  {

                ModifiedRW4File modifiedData = new ModifiedRW4File();
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;

                using (Stream stream = new MemoryStream(index.Data))
                {
                    //first read the current RW4model

                    List<RW4Section> sections = dataGrid1.ItemsSource as List<RW4Section>;
                    _rw4model.Sections = sections;

                    foreach (RW4Section section in _rw4model.Sections)
                    {
                        if (section.TypeCode == SectionTypeCodes.Texture)
                        {
                            SporeMaster.RenderWare4.Texture tex = section.obj as SporeMaster.RenderWare4.Texture;
                            _rw4model.Sections[(int)tex.texData.section.Number].obj = tex.texData;
                        }


                    }

                    RW4Section meshSection = _rw4model.Sections.Find(s => s.TypeCode == SectionTypeCodes.Mesh);
                    if (meshSection != null)
                    {
                        SporeMaster.RenderWare4.RW4Mesh mesh = meshSection.obj as SporeMaster.RenderWare4.RW4Mesh;

                        //update the bounding box
                        /*RW4Section bboxSection = _rw4model.Sections.Find(s => s.TypeCode == SectionTypeCodes.BBox);
                        if (bboxSection != null)
                        {
                            RW4BBox boundingBox = bboxSection.obj as RW4BBox;
                            if (meshSection != null)
                            {
                                boundingBox.minx = mesh.vertices.vertices.Min(v => v.X);
                                boundingBox.miny = mesh.vertices.vertices.Min(v => v.Y);
                                boundingBox.minz = mesh.vertices.vertices.Min(v => v.Z);

                                boundingBox.maxx = mesh.vertices.vertices.Max(v => v.X);
                                boundingBox.maxy = mesh.vertices.vertices.Max(v => v.Y);
                                boundingBox.maxz = mesh.vertices.vertices.Max(v => v.Z);

                                bboxSection.obj = boundingBox;
                            }
                        }*/

                        _rw4model.Sections[(int)mesh.vertices.section.Number].obj = mesh.vertices;
                        _rw4model.Sections[(int)mesh.triangles.section.Number].obj = mesh.triangles;

                        _rw4model.Sections[(int)mesh.vertices.vertices.section.Number].obj = mesh.vertices.vertices.section.obj;
                        _rw4model.Sections[(int)mesh.triangles.triangles.section.Number].obj = mesh.triangles.triangles.section.obj;
                    }






                    //save back the model

                    using (MemoryStream writer = new MemoryStream())
                    {
                        _rw4model.Write(writer);

                        modifiedData.RW4FileData = writer.ToArray();
                        index.Index.ModifiedData = modifiedData;
                        index.Index.IsModified = true;
                        index.Index.Compressed = false;
                    }
                }

                //ViewHexDiff hex = new ViewHexDiff(index.Data, modifiedData.RW4FileData);
                //hex.ShowDialog();
            }
        }

        private void viewHex_DataChangedHandler(object sender, EventArgs e)
        {
            if (dataGrid1.SelectedItem != null)
            {
                if (dataGrid1.SelectedItem.GetType() == typeof(RW4Section))
                {

                    RW4Section section = (RW4Section)dataGrid1.SelectedItem;

                    DatabaseIndexData index = (DatabaseIndexData)this.DataContext;

                    byte[] buffer = viewHex.DataContext as byte[];
                    //   using (Stream stream = new MemoryStream(index.Data))
                    //  {
                    //      stream.Seek((int)section.Pos, SeekOrigin.Begin);
                    //      stream.Write(buffer, 0, (int)section.Size);
                    //  }
                    (section.obj as RW4Blob).blob = buffer;

                    SaveRW4Model();
                }
            }
        }
    }

    public class RW4ModelSectionView
    {
        private RW4Model _model;
        public RW4Model Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private RW4Section _section;
        public RW4Section Section
        {
            get { return _section; }
            set { _section = value; }
        }
    }
}
