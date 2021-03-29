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
    class RW4HierarchyInfo : RW4Object
    {
        public const int type_code = 0x70002;
        public class Item
        {
            public int index;
            public UInt32 name_fnv;   // e.g. "joint1".fnv()
            public UInt32 flags;      // probably; values are all 0 to 3.   &1 might be "leaf"
            public Item parent;
        };
        public Item[] items;
        public UInt32 id;           // hash or guid, referenced by Anim to specify which bones to animate?

        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            if (s.type_code != type_code) throw new ModelFormatException(r, "HI000", s.type_code);
            var p2 = r.ReadU32();
            var p3 = r.ReadU32();
            var p1 = r.ReadU32();
            var c1 = r.ReadU32();
            id = r.ReadU32();
            r.expect(c1, "HI001");
            if (p1 != r.Position) throw new ModelFormatException(r, "HI010", p1);
            if (p2 != p1 + 4 * c1) throw new ModelFormatException(r, "HI011", p2);
            if (p3 != p1 + 8 * c1) throw new ModelFormatException(r, "HI012", p3);
            items = new Item[c1];
            for (int i = 0; i < c1; i++)
                items[i] = new Item { index = i, name_fnv = r.ReadU32() };
            for (int i = 0; i < c1; i++)
                items[i].flags = r.ReadU32();
            for (int i = 0; i < c1; i++)
            {
                var pind = r.ReadS32();
                if (pind == -1)
                    items[i].parent = null;
                else
                    items[i].parent = items[pind];
            }
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            var c1 = (uint)items.Length;
            var p1 = (uint)w.Position + 6 * 4;
            w.WriteU32(p1 + 4 * c1);
            w.WriteU32(p1 + 8 * c1);
            w.WriteU32(p1);
            w.WriteU32(c1);
            w.WriteU32(id);
            w.WriteU32(c1);
            for (int i = 0; i < c1; i++)
                w.WriteU32(items[i].name_fnv);
            for (int i = 0; i < c1; i++)
                w.WriteU32(items[i].flags);
            for (int i = 0; i < c1; i++)
                w.WriteS32(items[i].parent == null ? -1 : items[i].parent.index);
        }
        public override int ComputeSize()
        {
            return 4 * 6 + 12 * items.Length;
        }
    };
}
