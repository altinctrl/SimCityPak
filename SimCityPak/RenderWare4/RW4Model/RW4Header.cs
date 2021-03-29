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
    public class RW4Header
    {
        //"\x89RW4w32\x00\r\n\x1a\n\x00 \x04\x00454\x00000\x00\x00\x00\x00\x00";
        readonly byte[] RW4_Magic = new byte[] { 137, 82, 87, 52, 119, 51, 50, 0, 13, 10, 26, 10, 0, 32, 4, 0, 52, 53, 52, 0, 48, 48, 48, 0, 0, 0, 0, 0 };
        readonly UInt32[] fixed_section_types = new UInt32[] { 0, 0x10030, 0x10031, 0x10032, 0x10010 };

        public RW4Model.FileTypes file_type;

        private uint _fileTypeCode;

        public uint HeaderEnd
        { get; set; }

        public UInt32 Unknown
        { get; set; }

        public UInt32 SectionIndexBegin
        { get; set; }

        public UInt32 SectionIndexPadding
        { get; set; }

        public UInt32 SectionIndexEnd { get { return (uint)(SectionIndexBegin + 6 * 4 * Sections.Count + 8 * getFixupCount() + SectionIndexPadding); } }

        public List<RW4Section> Sections = new List<RW4Section>();

        private uint getFixupCount()
        {
            uint total = 0;
            foreach (var s in Sections)
                total += (uint)s.fixup_offsets.Count;
            return total;
        }

        public void Read(System.IO.Stream r)
        {
            //Read the "Magic" (this is a series of bytes that is always the same on every RW4File)
            var b = r.ReadBytes(RW4_Magic.Length);
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i] != RW4_Magic[i]) throw new ModelFormatException(r, "Not a RW4 file", b);
            }

            _fileTypeCode = r.ReadU32();
            if (_fileTypeCode == 1)
            {
                file_type = RW4Model.FileTypes.Model;
            }
            else if (_fileTypeCode == 0x04000000)
            {
                file_type = RW4Model.FileTypes.Texture;
            }
            else
            {
                throw new ModelFormatException(r, "Unknown2 file type", _fileTypeCode);
            }

            uint _fileTypeConstant = (file_type == RW4Model.FileTypes.Model ? 16U : 4U);

            uint _sectionCount = r.ReadU32();
            r.expect(_sectionCount, "H001 Section count not repeated");
            r.expect(_fileTypeConstant, "H002");
            r.expect(0, "H003");

            SectionIndexBegin = r.ReadU32();
            uint _firstHeaderSectionBegin = r.ReadU32();  // Always 0x98?
            r.expect(new UInt32[] { 0, 0, 0 }, "H010");
            uint _sectionIndexEnd = r.ReadU32();
            r.expect(_fileTypeConstant, "H011");
            //this.unknown_size_or_offset_013 = r.ReadU32();
            uint _fileSize = r.ReadU32() + _sectionIndexEnd;
         //   if (r.Length != _fileSize) throw new ModelFormatException(r, "H012", _fileSize);  //< TODO

            r.expect(new UInt32[] { 4, 0, 1, 0, 1 }, "H020");

            this.Unknown = r.ReadU32();

            r.expect(new UInt32[] { 4, 0, 1, 0, 1 }, "H040");
            r.expect(new UInt32[] { 0, 1, 0, 0, 0, 0, 0 }, "H041");

            if (r.Position != _firstHeaderSectionBegin) throw new ModelFormatException(r, "H099", r.Position);

            r.Seek(_firstHeaderSectionBegin, SeekOrigin.Begin);
            r.expect(0x10004, "H140");
            // Offsets of header sections relative to the 0x10004.  4, 12, 28, 28 + (12+4*section_type_count), ... + 36, ... + 28
            uint[] _offsets = new uint[6];
            for (int i = 0; i < _offsets.Length; i++)
            {
                _offsets[i] = r.ReadU32() + _firstHeaderSectionBegin;
            }

            // A list of section types in the file?  If so, redundant with section index
            if (_offsets[2] != r.Position) throw new ModelFormatException(r, "H145", r.Position);
            r.expect(0x10005, "H150");
            uint _sectionTypeCount = r.ReadU32();
            r.expect(12, "H151");
            //read the list if section types, contains the 'indirect' section codes
            uint[] _sectionTypes = new uint[_sectionTypeCount];
            for (int i = 0; i < _sectionTypeCount; i++)
            {
                _sectionTypes[i] = r.ReadU32();
            }

            if (_offsets[3] != r.Position) throw new ModelFormatException(r, "H146", r.Position);
            r.expect(0x10006, "H160");
            // TODO: I think this is actually a variable length structure, with 12 byte header and 3 being the length in qwords
            r.expect(new UInt32[] { 3, 0x18, _fileTypeCode, 0xffb00000, _fileTypeCode, 0, 0, 0 }, "H161");

            if (_offsets[4] != r.Position) throw new ModelFormatException(r, "H147", r.Position);
            r.expect(0x10007, "H170");
            uint _fixupCount = r.ReadU32();
            r.expect(0, "H171");
            r.expect(0, "H172");
            // Fixup index always immediately follows section index
            r.expect(SectionIndexBegin + _sectionCount * 24 + _fixupCount * 8, "H173");
            r.expect(SectionIndexBegin + _sectionCount * 24, "H174");
            r.expect(_fixupCount, "H176");

            if (_offsets[5] != r.Position) throw new ModelFormatException(r, "H148", r.Position);
            r.expect(0x10008, "H180");
            r.expect(new UInt32[] { 0, 0 }, "H181");

            HeaderEnd = (uint)r.Position;

            r.Seek(SectionIndexBegin, SeekOrigin.Begin);
            this.Sections = new List<RW4Section>();
            for (int i = 0; i < _sectionCount; i++)
            {
                RW4Section _newSection = new RW4Section();
                _newSection.LoadHeader(r, (uint)i, _sectionIndexEnd);
                this.Sections.Add(_newSection);
            }
            for (int i = 0; i < _fixupCount; i++)
            {
                uint sind = r.ReadU32();
                var offset = r.ReadU32();
                this.Sections[(int)sind].fixup_offsets.Add(offset);
            }
            SectionIndexPadding = _sectionIndexEnd - (uint)r.Position;
            r.ReadPadding(SectionIndexPadding);

            //process the indirect section codes
            bool[] used = new bool[_sectionTypes.Length];
            foreach (var s in Sections)
            {
                used[s.type_code_indirect] = true;
                if (_sectionTypes[s.type_code_indirect] != s.type_code)
                    throw new ModelFormatException(r, "H300", s.type_code_indirect);
            }
            for (int i = 0; i < fixed_section_types.Length; i++)
                if (_sectionTypes[i] != fixed_section_types[i])
                    throw new ModelFormatException(r, "H301", i);
            for (int i = fixed_section_types.Length; i < _sectionTypes.Length; i++)
                if (!used[i])
                    throw new ModelFormatException(r, "H302", _sectionTypes[i]);

         
        }

        /// <summary>
        /// Gets the section types used in this RW4Model instance
        /// </summary>
        /// <returns></returns>
        private UInt32[] getSectionTypes()
        {
            List<UInt32> stype = new List<UInt32>(fixed_section_types);
            foreach (RW4Section section in Sections.OrderBy(s=> s.type_code))
            {
                if (!stype.Contains(section.type_code))
                {
                    stype.Add(section.type_code);
                }
                section.type_code_indirect = (uint)stype.IndexOf(section.type_code);
            }
            return stype.ToArray();
        }

        public void Write(Stream w)
        {
            uint[] section_types = getSectionTypes();

            uint file_type_code = file_type == RW4Model.FileTypes.Model ? 1U : 0x04000000U;

            w.Write(RW4_Magic, 0, RW4_Magic.Length);
            w.WriteU32(file_type_code);
            w.WriteU32((uint)Sections.Count);
            w.WriteU32((uint)Sections.Count);
            var ft_const1 = (file_type == RW4Model.FileTypes.Model ? 16U : 4U);
            w.WriteU32(ft_const1);
            w.WriteU32(0);

            //The section index is sandwiched between the non-blob and blob sections - starting and ending index should be editable
            w.WriteU32(SectionIndexBegin);
            w.WriteU32(0x98);                            // pointer to section 10004
            w.WriteU32s(new UInt32[] { 0, 0, 0 });
            w.WriteU32((uint)SectionIndexEnd);
            w.WriteU32(ft_const1);

            //Write the size of the blob data
            uint blobSize = (uint)Sections.Where(s => s.TypeCode == SectionTypeCodes.Blob).Sum(s => s.Size);
            w.WriteU32(blobSize);

            w.WriteU32s(new UInt32[] { 4, 0, 1, 0, 1 });

            w.WriteU32(this.Unknown);

            w.WriteU32s(new UInt32[] { 4, 0, 1, 0, 1 });
            w.WriteU32s(new UInt32[] { 0, 1, 0, 0, 0, 0, 0 });

            if (w.Position != 0x98) throw new ModelFormatException(w, "WH099", w.Position);

            w.WriteU32(0x10004);
            var hs_sizes = new UInt32[] { 4, 8, 16, 12 + 4 * (uint)section_types.Length, 36, 28 };
            for (int i = 1; i < hs_sizes.Length; i++)
                hs_sizes[i] += hs_sizes[i - 1];
            w.WriteU32s(hs_sizes);

            w.WriteU32(0x10005);
            w.WriteU32((uint)section_types.Length);
            w.WriteU32(12);
            w.WriteU32s(section_types);

            w.WriteU32(0x10006);
            w.WriteU32s(new UInt32[] { 3, 0x18, file_type_code, 0xffb00000, file_type_code, 0, 0, 0 });

            w.WriteU32(0x10007);
            int fixup_count = 0;
            foreach (var s in Sections)
                fixup_count += s.fixup_offsets.Count;
            w.WriteU32((uint)fixup_count);
            w.WriteU32(0);
            w.WriteU32(0);
            w.WriteU32((uint)(SectionIndexBegin + Sections.Count * 24 + fixup_count * 8));
            w.WriteU32((uint)(SectionIndexBegin + Sections.Count * 24));
            w.WriteU32((uint)fixup_count);

            w.WriteU32(0x10008);
            w.WriteU32s(new UInt32[] { 0, 0 });

            w.WritePadding(this.HeaderEnd - w.Position);
        }
        public void WriteIndex(Stream w)
        {
            if (w.Position != this.SectionIndexBegin) throw new ModelFormatException(w, "WH200", null);
            foreach (var s in Sections)
                s.WriteHeader(w, (uint)SectionIndexEnd);
            foreach (var s in Sections)
                foreach (var f in s.fixup_offsets)
                {
                    w.WriteU32(s.Number);
                    w.WriteU32(f);
                }
            w.WritePadding(SectionIndexEnd - w.Position);
            if (w.Position != SectionIndexEnd) throw new ModelFormatException(w, "WH299", w.Position);
        }

        public UInt32 GetHeaderEnd()
        {
            return 0x98 + 104 + 4 * (uint)getSectionTypes().Length + 12;
           // return this.HeaderEnd;
        }
    };
}
