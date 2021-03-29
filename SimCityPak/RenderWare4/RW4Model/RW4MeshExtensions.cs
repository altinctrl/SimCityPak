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
    partial class RW4Mesh : RW4Object
    {
        public void Export(string fileName)
        {
            IConverter converter = new WaveFrontOBJConverter();
            converter.Export(this, fileName);
        }
    }
}
