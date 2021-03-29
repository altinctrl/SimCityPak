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
    public class TextureBlob : RW4Blob
    {
        public const uint type_code = 0x10030;
        protected override bool Relocatable() { return true; }
    };

}
