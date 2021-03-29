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
    public class ModelFormatException : FormatException
    {
        public string exception_type;
        public ModelFormatException(Stream r, string description, object argument)
            : base(String.Format("{0} {2} @0x{1:x}", description, r == null ? 0 : r.Position, argument))
        {
            exception_type = description;
        }
    }

    public static class StreamHelpers
    {
        public static byte[] ReadBytes(this Stream r, int count)
        {
            var a = new byte[count];
            if (r.Read(a, 0, count) != count) throw new EndOfStreamException();
            return a;
        }
        public static void WriteU32s(this Stream w, UInt32[] values)
        {
            foreach (var v in values)
                w.WriteU32(v);
        }
        public static void ReadPadding(this Stream r, long pad)
        {
            if (pad < 0) throw new ModelFormatException(r, "Negative padding", pad);
            for (long i = 0; i < pad; i++)
                if (r.ReadByte() != 0)
                    throw new ModelFormatException(r, "Nonzero padding", null);
        }
        public static void WritePadding(this Stream w, long pad)
        {
            if (pad < 0) throw new ModelFormatException(w, "Negative padding", pad);
            for (long i = 0; i < pad; i++) w.WriteByte(0);
        }
        public static void expect(this Stream r, UInt32 value, string error)
        {
            var actual = r.ReadU32();
            if (actual != value)
                throw new ModelFormatException(r, error, actual);
        }
        public static void expect(this Stream r, UInt32[] values, string error)
        {
            foreach (var v in values)
                r.expect(v, error);
        }
    };

    public partial class RW4Model
    {
        public RW4Header Header
        { get; set; }

        public enum FileTypes { Model, Texture };

        public List<RW4Section> Sections { get { return Header.Sections; } set { Header.Sections = value; } }
        public FileTypes FileType { get { return Header.file_type; } set { Header.file_type = value; } }

        public RW4Model() { }

        public IList<RW4Object> GetObjects(UInt32 type_code)
        {
            return (from s in Header.Sections where s.type_code == type_code select s.obj).ToList();
        }

        public void AddObject(RW4Object obj, UInt32 type_code)
        {
            obj.section = InsertSection(Header.Sections.Count);
            obj.section.type_code = type_code;
            obj.section.obj = obj;
            obj.section.Alignment = 0x10;  // by default
        }

        public void RemoveSection(RW4Section section)
        {
            if (Header.Sections[(int)section.Number] != section) throw new ArgumentException("Attempt to remove invalid section.");
            //var new_sections = new RW4Section[header.Sections.Length - 1];
            /*for (int i = 0; i < section.Number; i++)
                new_sections[i] = header.Sections[i];
            for (int i = (int)section.Number; i < new_sections.Length; i++ )
            {
                new_sections[i] = header.Sections[i + 1];
                new_sections[i].Number = (uint)i;
            }*/
            // header.Sections = new_sections;
            section.Number = 0xffffffff;
        }

        public RW4Section InsertSection(int index)
        {
            /* var new_sections = new RW4Section[header.Sections.Count + 1];
             for (int i = 0; i < index; i++)
                 new_sections[i] = header.Sections[i];
             for (int i = index + 1; i < new_sections.Length; i++)
             {
                 new_sections[i] = header.Sections[i - 1];
                 new_sections[i].Number = (uint)i;
             }*/

            RW4Section newSection = new RW4Section();
            newSection.Number = (uint)index;
            Header.Sections.Add(newSection);


            return newSection;
        }

        void Pack()
        {
            //Recalculate the section sizes
            foreach (RW4Section section in Header.Sections)
            {
                var sectionSize = section.obj.ComputeSize();
                if (sectionSize != -1)
                    section.Size = (uint)sectionSize;
            }

            var sections = Header.Sections;   //(from s in header.sections orderby s.pos select s).ToArray();


            uint posNonBlob = Header.GetHeaderEnd();

            //calculate where the header is going to end.


            //calculate the new position of each section

            foreach (RW4Section section in Header.Sections.Where(s => s.TypeCode != SectionTypeCodes.Blob))
            {

                uint np = (posNonBlob + section.Alignment - 1) & ~(section.Alignment - 1);
                if (section.Pos != np)
                {
                    section.Pos = np;
                }
                posNonBlob = section.Pos + section.Size;
            }

            Header.SectionIndexBegin = posNonBlob;

            uint posBlob = Header.SectionIndexEnd;

            foreach (RW4Section section in Header.Sections.Where(s => s.TypeCode == SectionTypeCodes.Blob))
            {
                var np = (posBlob + section.Alignment - 1) & ~(section.Alignment - 1);
                if (section.Pos != np)
                {
                    section.Pos = np;
                }
                posBlob = section.Pos + section.Size;
            }
        }

        public void New()
        {
            Header = new RW4Header();
        }

        public void Read(System.IO.Stream r)
        {
            Header = new RW4Header();
            Header.Read(r);

            foreach (var section in Header.Sections)
            {
                switch (section.type_code)
                {
                     case RW4Mesh.type_code:
                          {
                              try
                              {
                                  RW4Mesh t;
                                  section.GetObject(this, r, out t);
                                  break;
                              }
                              catch
                              {
                                  break;
                              }
                          }  
                    case RW4Material.type_code:
                         {
                             RW4Material t;
                             section.GetObject(this, r, out t);
                             break;
                         }
                    /* case RW4Skeleton.type_code:
                          {
                              RW4Skeleton t;
                              section.GetObject(this, r, out t);
                              break;
                          }*/
                     case Texture.type_code:
                         {
                             Texture t;
                             section.GetObject(this, r, out t);
                             break;
                         }
                    /* case RW4HierarchyInfo.type_code:
                         {
                             RW4HierarchyInfo t;
                             section.GetObject(this, r, out t);
                             break;
                         } */
                    /* case RWMeshMaterialAssignment.type_code:
                         {
                             RWMeshMaterialAssignment t;
                             section.GetObject(this, r, out t);
                             break;
                         }*/
                  /*  case RW4BBox.type_code:
                        {
                            RW4BBox b;
                            section.GetObject(this, r, out b);
                            break;
                        }*/
                    case VertexFormat.type_code:
                        {
                            VertexFormat vf;
                            section.GetObject(this, r, out vf);
                            break;
                        } 
                    /*
           case ModelHandles.type_code:
               {
                   ModelHandles t;
                   section.GetObject(this, r, out t);
                   break;
               }*/
                    /*  case Anim.type_code:
                          {
                              Anim t;
                              section.GetObject(this, r, out t);
                              break;
                          }*/
                    /*  case Animations.type_code:
                          {
                              Animations t;
                              section.GetObject(this, r, out t);
                              break;
                          }*/

                    /*   case Matrices4x4.type_code:
                           {
                               Matrices4x4 t;
                               section.GetObject(this, r, out t);
                               break;
                           }
                       case Matrices4x3.type_code:
                           {
                               Matrices4x3 t;
                               section.GetObject(this, r, out t);
                               break;
                           }*/
                }
            }
            foreach (var section in Header.Sections)
            {
                if (section.obj == null)
                {
                    try
                    {
                        section.LoadObject(this, new UnreferencedSection(), r);
                    }
                    catch
                    {

                    }
                }
            }
        }

        public void Write(Stream w)
        {
            Pack(); 
            w.WritePadding(Sections.Max(s => s.Pos + s.Size));
            w.Seek(0, SeekOrigin.Begin);
            Header.Write(w);
           

            //section size and pos should already have been set during the "pack" phase...
            foreach (RW4Section section in Header.Sections.Where(s => s.TypeCode != SectionTypeCodes.Blob).OrderBy(s => s.Pos))
            {
                w.Seek(section.Pos, SeekOrigin.Begin);
                section.Write(w, this);
            }

            w.WritePadding(Header.SectionIndexBegin - w.Position);
            w.Seek(Header.SectionIndexBegin, SeekOrigin.Begin);
            Header.WriteIndex(w);

            foreach (RW4Section section in Header.Sections.Where(s => s.TypeCode == SectionTypeCodes.Blob).OrderBy(s => s.Pos))
            {
                w.Seek(section.Pos, SeekOrigin.Begin);
                section.Write(w, this);
            }

            //write the final file size
            w.Seek(76, SeekOrigin.Begin);
            w.WriteU32((uint)w.Length - Header.SectionIndexEnd);

        }
    }
}
