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
    public class RW4TexMetadata : RW4Object
    {
        public const int type_code = 0x2000b;
        public Texture texture;
        public byte[] unk_data_1;
        public Int32[] unk_data_2 = new Int32[36];

        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            if (s.type_code != type_code) throw new ModelFormatException(r, "TM000", s.type_code);
            // This section is hard to really decode because it is almost constant across different models!
            var p = (long)s.Size - (unk_data_2.Length) * 4 - 4;
            if (p < 0) throw new ModelFormatException(r, "TM001", p);
            unk_data_1 = r.ReadBytes((int)p);
            var sn = r.ReadS32();
            for (int i = 0; i < unk_data_2.Length; i++)
                unk_data_2[i] = r.ReadS32();

            m.Sections[sn].GetObject(m, r, out texture);
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            w.Write(unk_data_1, 0, unk_data_1.Length);
            w.WriteU32(texture.section.Number);
            for (int i = 0; i < unk_data_2.Length; i++)
                w.WriteS32(unk_data_2[i]);
        }
        public override int ComputeSize()
        {
            return unk_data_1.Length + 4 + unk_data_2.Length * 4;
        }
    };
}
