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
    public class RW4SectionChangedEventArgs : EventArgs
    {
        public RW4SectionChangedEventArgs(RW4Section section)
        {
            _section = section;
        }

        private RW4Section _section;
        public RW4Section Section
        {
            get { return _section; }
            set { _section = value; }
        }
    }

    public class RW4Section
    {
        private UInt32 _number;
        public UInt32 Number
        {
            get { return _number; }
            set { _number = value; }
        }

        public UInt32 Pos
        { get; set; }
        public UInt32 Size
        { get; set; }
        public UInt32 Alignment
        { get; set; }
        public UInt32 type_code;
        public UInt32 type_code_indirect;      //< Index into header.section_types table
        public RW4Object obj;
        public List<UInt32> fixup_offsets = new List<UInt32>();         //< Something is inserted here at load time??

        public event EventHandler SectionChanged;

        public void Changed()
        {
            SectionChanged(this, new EventArgs());
        }

        public SectionTypeCodes TypeCode
        {
            get
            {
                SectionTypeCodes typeCode = SectionTypeCodes.Unknown;
                if (Enum.IsDefined(typeof(SectionTypeCodes), (Int32)type_code))
                {
                    typeCode = (SectionTypeCodes)type_code;
                }
                return typeCode;
            }
        }

        public string DisplayName
        {
            get
            {
                return string.Format("{0:X} ({1})", type_code, TypeCode);
            }
        }

        public void LoadHeader(Stream r, UInt32 number, UInt32 index_end)
        {
            this.Number = number;
            Pos = r.ReadU32();
            r.expect(0, "H201");
            Size = r.ReadU32();
            Alignment = r.ReadU32();
            this.type_code_indirect = r.ReadU32();
            this.type_code = r.ReadU32();
            if (TypeCode == SectionTypeCodes.Blob) Pos += index_end;
        }

        public void WriteHeader(Stream w, UInt32 index_end)
        {
            w.WriteU32(Pos - (TypeCode == SectionTypeCodes.Blob ? index_end : 0));
            w.WriteU32(0);
            w.WriteU32(Size);
            w.WriteU32(Alignment);
            w.WriteU32(type_code_indirect);
            w.WriteU32(type_code);
        }

        public void GetObject<T>(RW4Model model, Stream r, out T o)
        where T : RW4Object, new()
        {
            if (obj != null)
            {
                o = (T)obj;
                return;
            }
            o = new T();
            LoadObject(model, o, r);
        }

        public byte[] GetData(Stream r)
        {
            byte[] data = new byte[this.Size];
            r.Seek(this.Pos, SeekOrigin.Begin);
            r.Read(data, 0, (int)this.Size); 

            return data;
        }

        public void LoadObject(RW4Model model, RW4Object o, Stream r)
        {
            if (obj != null) throw new ModelFormatException(r, "Attempt to decode section twice.", Number);
            long start = r.Position;
            obj = o;
            o.section = this;
            o.model = model;
            r.Seek(Pos, SeekOrigin.Begin);
            obj.Read(model, this, r);
              if (r.Position != Pos + Size)
                throw new ModelFormatException(r, "Section incompletely read.", Number);
            var cs = obj.ComputeSize();
             if (cs != -1 && cs != Size)
                 throw new ModelFormatException(r, "Section Size doesn't match computed Size.", Number);
            r.Seek(start, SeekOrigin.Begin);
        }

        public void Write(Stream w, RW4Model model)
        {
            w.WritePadding(Pos - w.Position);
            obj.Write(model, this, w);
            if (w.Position != Pos + Size)
               throw new ModelFormatException(w, "Section incompletely written.", Number);
        }
    };
}
