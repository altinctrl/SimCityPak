using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Spore.Helpers;

namespace SimCityPak
{
    public class EffectsDirectory
    {
        public uint MajorVersion { get; set; }
        public uint MinorVersion { get; set; }

        public static EffectsDirectory CreateFromStream(Stream strm)
        {
            EffectsDirectory effDir = new EffectsDirectory();

            effDir.MajorVersion = strm.ReadU16BE();
            effDir.MinorVersion = strm.ReadU16BE();

            uint NumberofEntries = strm.ReadU16BE();
            uint Behavior_1 = strm.ReadU16BE();
            uint AlwaysZero = strm.ReadU16BE();
            uint Behavior_2 = strm.ReadU16BE();
            uint EffectDurationMinimum = strm.ReadU16BE();
            uint EffectDurationMaximum = strm.ReadU16BE();
            uint NumberReleasedAtHighDetail = strm.ReadU16BE();
            uint RepeatFlag = strm.ReadU16BE();
            uint Unk1 = strm.ReadU16BE();
            uint Unk2 = strm.ReadU16BE();
            uint Unk3 = strm.ReadU16BE();
            uint TimeDelayMinimum = strm.ReadU16BE();
            uint TimeDelayMaximum = strm.ReadU16BE();
            uint XAxisPushMin = strm.ReadU16BE();
            uint XAxisPushMax = strm.ReadU16BE();
           












            return effDir;
        }


    }
}
