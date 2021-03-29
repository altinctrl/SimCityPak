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
    public class RW4Blob : RW4Object
    {
        public byte[] blob;
        public long required_position = -1;

        protected virtual bool Relocatable() { return true; }
        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            if (!Relocatable())
                this.required_position = r.Position;
            blob = r.ReadBytes((int)s.Size);
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            if (this.required_position >= 0 && w.Position != this.required_position)
                throw new ModelFormatException(w, "Unable to move Unknown2 section", null);
            w.Write(blob, 0, blob.Length);
        }
        public override int ComputeSize() { return blob.Length; }
    };
}
