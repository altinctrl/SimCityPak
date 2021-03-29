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
    class Animations : RW4Blob
    {
        public const int type_code = 0xff0001;
        protected override bool Relocatable() { return true; }
    };
}
