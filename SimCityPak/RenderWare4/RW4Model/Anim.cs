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
    class Anim : RW4Object
    {
        public const int type_code = 0x70001;
        public UInt32 skeleton_id;  // matches Skeleton.jointInfo.id (or something else sometimes!)
        public float length;   // seconds?
        public UInt32 flags;   // probably; generally 0-3
        public UInt32[] channel_names;  // duplicate Skeleton.jointInfo, I believe
        public JointPose[,] channel_frame_pose;
        public UInt32 padding;

        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            if (s.type_code != type_code) throw new ModelFormatException(r, "AN000", s.type_code);
            var p1 = r.ReadU32();
            var channels = r.ReadU32();
            skeleton_id = r.ReadU32();
            r.expect(0, "AN001");      //< Usually zero.. always <= 10.  Maybe a count of something?
            var p3 = r.ReadU32();
            var p4 = r.ReadU32();
            r.expect(channels, "AN002");
            r.expect(0, "AN003");
            length = r.ReadF32();
            r.expect(12, "AN010");
            flags = r.ReadU32();
            var p2 = r.ReadU32();

            if (p1 != r.Position) throw new ModelFormatException(r, "AN100", p1);
            if (p2 != p1 + channels * 4) throw new ModelFormatException(r, "AN101", p2);
            if (p3 != p2 + channels * 12) throw new ModelFormatException(r, "AN102", p3);

            //var keyframe_count = (p4 - p3) / (channels * JointPose.size * 3);
            uint keyframe_count = 0;

            channel_names = new UInt32[channels];
            for (int i = 0; i < channels; i++) channel_names[i] = r.ReadU32();

            for (int i = 0; i < channels; i++)
            {
                var p = r.ReadU32() - (12 * 4 + channels * 4 + channels * 12);
                var pose_size = r.ReadU32();
                var pose_components = r.ReadU32();
                if (pose_components != 0x601 || pose_size != JointPose.size)
                    throw new ModelFormatException(r, "AN200 Pose format not supported", pose_components);
                if (i == 1)
                    keyframe_count = p / JointPose.size;
                else if (i >= 1 && p != i * JointPose.size * keyframe_count)
                    throw new ModelFormatException(r, "AN201", null);
            }

            if (channels == 1) keyframe_count = (p4 - p3) / (channels * JointPose.size);  // Yuck!  This is just a guess.

            channel_frame_pose = new JointPose[channels, keyframe_count];
            for (int c = 0; c < channels; c++)
                for (int f = 0; f < keyframe_count; f++)
                {
                    channel_frame_pose[c, f].Read(r);
                    if (channels == 1 && channel_frame_pose[c, f].time == length)
                    {
                        // We had to guess about the length, so now fix our guess :-(
                        keyframe_count = (uint)f + 1;
                        var t = new JointPose[channels, keyframe_count];
                        for (int c1 = 0; c1 < channels; c1++)
                            for (int f1 = 0; f1 < keyframe_count; f1++)
                                t[c1, f1] = channel_frame_pose[c1, f1];
                        channel_frame_pose = t;
                        break;
                    }
                }

            padding = p4 - p3 - channels * keyframe_count * JointPose.size;
            r.ReadPadding(padding);  //< Huge number of zeros very common
        }

        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            var channels = (uint)channel_names.Length;
            var keyframe_count = (uint)channel_frame_pose.GetLength(1);
            var p1 = (uint)w.Position + 12 * 4;

            w.WriteU32(p1);
            w.WriteU32(channels);
            w.WriteU32(skeleton_id);
            w.WriteU32(0);
            w.WriteU32(p1 + channels * 4 + channels * 12);
            w.WriteU32(p1 + channels * 4 + channels * 12 + channels * keyframe_count * JointPose.size + padding);
            w.WriteU32(channels);
            w.WriteU32(0);
            w.WriteF32(length);
            w.WriteU32(12);
            w.WriteU32(flags);
            w.WriteU32(p1 + channels * 4);

            w.WriteU32s(channel_names);
            for (int i = 0; i < channels; i++)
            {
                w.WriteU32((uint)(12 * 4 + channels * 4 + channels * 12 + i * JointPose.size * keyframe_count));
                w.WriteU32(JointPose.size);
                w.WriteU32(0x601);
            }
            for (int c = 0; c < channels; c++)
                for (int f = 0; f < keyframe_count; f++)
                    channel_frame_pose[c, f].Write(w);

            w.WritePadding(padding);
        }

        public override int ComputeSize()
        {
            var channels = channel_names.Length;
            var keyframe_count = channel_frame_pose.GetLength(1);
            return (int)(12 * 4 + channels * 4 + channels * 12 + channels * keyframe_count * JointPose.size + padding);
        }
    };
}
