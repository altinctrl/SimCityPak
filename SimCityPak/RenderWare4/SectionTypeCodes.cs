using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SporeMaster.RenderWare4
{


    public enum SectionTypeCodes
    {
        Texture = 0x20003,
        TriangleArray = 0x20007,
        Mesh = 0x20009,
        Material = 0x2000b,
        RW4Skeleton = 0x7000c,
        HierarchyInfo = 0x70002,
        MeshMaterialAssignment = 0x2001a,
        BBox = 0x80005,
        ModelHandles = 0xff0000,
        Anim = 0x70001,
        VertexFormat = 0x20004,
        Animations = 0xff0001,
        Matrices4x4 = 0x7000b,
        Matrices4x3 = 0x7000f,
        VertexArray = 0x20005,
        CollisionMesh = 0x80003,
        Blob = 0x10030,

        Unknown = 0
    }
}
