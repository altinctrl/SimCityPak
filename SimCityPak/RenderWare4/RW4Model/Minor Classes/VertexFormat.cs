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
    public class VertexUsage
    {

        public byte Unknown1 { get; set; }
        public ushort Offset { get; set; }
        public D3DDECLUSAGE Usage { get; set; }
        public D3DDECLTYPE DeclarationType { get; set; }
        public byte Index { get; set; }
        public byte Unknown2 { get; set; }
        public ushort Unknown3 { get; set; }
        public byte Unknown4 { get; set; }
    }

    public class VertexFormat : RW4Object
    {
        public const int type_code = 0x20004;

        ushort _vertexComponents;
        public ushort VertexSize { get; set; }
        private int size = 0;

        uint _unknown2;
        uint _unknown3;

        public List<VertexUsage> VertexElements { get; set; }

        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            int startingPosition = (int)r.Position;

            VertexElements = new List<VertexUsage>();

            r.expect(0x00000000, "Not Zero VF001");
            r.expect(0x00000000, "Not Zero VF002");
            r.expect(0x00000000, "Not Zero VF003");

            _vertexComponents = r.ReadU16(); //amount of 'items' in this vertex?
           
            VertexSize = r.ReadU16BE();

             _unknown2 = r.ReadU32BE();
             _unknown3 = r.ReadU32BE();

            //uint _unknown4 = r.ReadU32BE(); //zero
            for (int i = 0; i < _vertexComponents; i++)
            {
                byte unk1 = r.ReadU8(); //always zero

                ushort _offset = r.ReadU16BE();
                ushort _type = r.ReadU16BE();
                ushort _usage = r.ReadU16BE();
                byte _index = r.ReadU8();
                byte _unknown = r.ReadU8();
                ushort unk3 = r.ReadU16BE(); //always zero
                byte unk4 = r.ReadU8(); //always zero

                VertexElements.Add(new VertexUsage() { 
                    Unknown1 = unk1,
                    Offset = _offset,
                    Usage = (D3DDECLUSAGE)_usage, 
                    Index = _index,
                    DeclarationType = (D3DDECLTYPE)_type,
                    Unknown2 = _unknown,
                    Unknown3 = unk3,
                    Unknown4 = unk4
                });
            }

            size = (int)r.Position - startingPosition;
        }

        public override int ComputeSize()
        {
            return size;
        }

        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            w.WriteU32(0);
            w.WriteU32(0);
            w.WriteU32(0);

            w.WriteU16(_vertexComponents);
            w.WriteU16BE(VertexSize);

            w.WriteU32BE(_unknown2);
            w.WriteU32BE(_unknown3);

            foreach (VertexUsage usage in VertexElements)
            {
                w.WriteU8(usage.Unknown1);
                w.WriteU16BE(usage.Offset);
                w.WriteU16BE((ushort)usage.DeclarationType);
                w.WriteU16BE((ushort)usage.Usage);
                w.WriteU8(usage.Index);
                w.WriteU8(usage.Unknown2);
                w.WriteU16(usage.Unknown3);
                w.WriteU8(usage.Unknown4);
            }
        }
    }
}
