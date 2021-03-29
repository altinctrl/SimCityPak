using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SporeMaster.RenderWare4
{
    public interface IConverter
    {
        void Export(RW4Mesh mesh, string fileName);

        RW4Mesh Import(RW4Mesh mesh, string fileName);
    }
}
