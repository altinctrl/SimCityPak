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
    public abstract class RW4Object
    {
        public RW4Section section;
        public abstract void Read(RW4Model m, RW4Section s, Stream r);
        public abstract void Write(RW4Model m, RW4Section s, Stream w);
        public virtual int ComputeSize() { return -1; }
        public RW4Model model;
    };
}
