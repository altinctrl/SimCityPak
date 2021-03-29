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
    struct JointPose : IRW4Struct
    {
        public const uint size = 12 * 4;
        public float qx, qy, qz, qs;   //< Quaternion rotation.  qx*qx + qy*qy + qz*qz + qs*qs == 1.0
        public float tx, ty, tz;       //< Vector translation
        public float sx, sy, sz;       //< Vector scale
        public float time;
        public uint Size()
        {
            return size;
        }
        public void Read(Stream s)
        {
            qx = s.ReadF32(); qy = s.ReadF32(); qz = s.ReadF32(); qs = s.ReadF32();
            tx = s.ReadF32(); ty = s.ReadF32(); tz = s.ReadF32();
            sx = s.ReadF32(); sy = s.ReadF32(); sz = s.ReadF32();
            s.expect(0, "JP001");
            time = s.ReadF32();
        }
        public void Write(Stream s)
        {
            s.WriteF32(qx); s.WriteF32(qy); s.WriteF32(qz); s.WriteF32(qs);
            s.WriteF32(tx); s.WriteF32(ty); s.WriteF32(tz);
            s.WriteF32(sx); s.WriteF32(sy); s.WriteF32(sz);
            s.WriteU32(0);
            s.WriteF32(time);
        }
    };
}
