using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Spore.Helpers;
using System.Diagnostics;
using SimCityPak;
using System.Globalization;
using Gibbed.Spore.Package;
using System.Windows.Controls;
using System.Windows.Media;
using SimCityPak.PackageReader;
using SimCityPak.Views.AdvancedEditors;
using System.Windows.Media.Imaging;

namespace SporeMaster.RenderWare4
{
    public class MaterialTextureReference
    {
        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
        public uint TextureInstanceId { get; set; }
        public uint Unknown3 { get; set; }
        public uint Unknown4 { get; set; }
        public uint Unknown5 { get; set; }

        public string TextureInstanceIdString
        {
            get
            {
                return TextureInstanceId.ToHex();
            }
            set
            {
                try
                {
                    TextureInstanceId = Convert.ToUInt32(value.Substring(2, 8), 16);
                }
                catch
                {

                }
            }
        }

        WriteableBitmap bitmapCache = null;
        public Image GetImage(float x, float y, float width, float height)
        {
            DatabaseIndex imageIndex = DatabaseManager.Instance.Indices.Find(idx => idx.InstanceId == this.TextureInstanceId && idx.TypeId == PropertyConstants.RasterImageType);
            if (imageIndex != null)
            {
                using (MemoryStream imageByteStream = new MemoryStream(imageIndex.GetIndexData(true)))
                {
                    if (bitmapCache == null)
                    {
                        RasterImage img = RasterImage.CreateFromStream(imageByteStream, RasterChannel.All);
                        bitmapCache = img.MipMaps[0];
                    }

                    float pxX = (float)bitmapCache.Width * x;
                    float pxY = (float)bitmapCache.Width * y;
                    float pxWidth = (float)bitmapCache.Width * width;
                    float pxHeight = (float)bitmapCache.Width * height;

                    WriteableBitmap croppedImage = bitmapCache.Crop(new System.Windows.Rect(pxX, pxY, Math.Abs(pxWidth),  Math.Abs(pxHeight)));

                    return new Image() { Source = croppedImage, Stretch = Stretch.None };

                }
            }
            return null;
        }

       // WriteableBitmap bitmapCache = null;
        public WriteableBitmap GetBitmap(float x, float y, float width, float height)
        {
            DatabaseIndex imageIndex = DatabaseManager.Instance.Indices.Find(idx => idx.InstanceId == this.TextureInstanceId && idx.TypeId == PropertyConstants.RasterImageType);
            if (imageIndex != null)
            {
                using (MemoryStream imageByteStream = new MemoryStream(imageIndex.GetIndexData(true)))
                {
                    if (width != 0 && height != 0)
                    {
                        if (bitmapCache == null)
                        {
                            RasterImage img = RasterImage.CreateFromStream(imageByteStream, RasterChannel.All);
                            bitmapCache = img.MipMaps[0];
                        }

                        float pxX = (float)bitmapCache.Width * x;
                        float pxY = (float)bitmapCache.Width * y;
                        float pxWidth = (float)bitmapCache.Width * width;
                        float pxHeight = (float)bitmapCache.Width * height;

                        WriteableBitmap croppedImage = bitmapCache.Crop(new System.Windows.Rect(pxX, pxY, Math.Abs(pxWidth), Math.Abs(pxHeight)));

                        return croppedImage;
                    }
                }
            }
            return null;
        }

        public WriteableBitmap GetBitmap()
        {
            DatabaseIndex imageIndex = DatabaseManager.Instance.Indices.Find(idx => idx.InstanceId == this.TextureInstanceId && (idx.TypeId == PropertyConstants.RasterImageType || idx.TypeId == PropertyConstants.RW4ImageType));
            if (imageIndex != null)
            {
                if (imageIndex.TypeId == PropertyConstants.RasterImageType)
                {
                    using (MemoryStream imageByteStream = new MemoryStream(imageIndex.GetIndexData(true)))
                    {

                        RasterImage img = RasterImage.CreateFromStream(imageByteStream, RasterChannel.All);
                        return img.MipMaps[0];
                    }
                }
                else if (imageIndex.TypeId == PropertyConstants.RW4ImageType)
                {
                    using (MemoryStream imageByteStream = new MemoryStream(imageIndex.GetIndexData(true)))
                    {
                        RW4Model mod = new RW4Model();
                        mod.Read(imageByteStream);
                        RW4Section texSection = mod.Sections.First(s => s.TypeCode == SectionTypeCodes.Texture);
                        return (texSection.obj as Texture).ToImage(false);
                    }
                }
            }
            return null;
        }

        public Image GetImage()
        {
            DatabaseIndex imageIndex = DatabaseManager.Instance.Indices.Find(idx => idx.InstanceId == this.TextureInstanceId && idx.TypeId == PropertyConstants.RasterImageType);
            if (imageIndex != null)
            {
                using (MemoryStream imageByteStream = new MemoryStream(imageIndex.GetIndexData(true)))
                {

                    RasterImage img = RasterImage.CreateFromStream(imageByteStream, RasterChannel.All);
                    return new Image() { Source = img.MipMaps[0], Stretch = Stretch.None };

                }
            }
            return null;
        }
    }


    class RW4Material : RW4Object
    {
        public const int type_code = 0x2000B;
        public RW4TexMetadata tex;
        public UInt32 unk1;

        public uint Size;

        public List<MaterialTextureReference> Materials { get; set; }

        public byte[] Header { get; set; }
        public byte[] Data { get; set; }
        public byte[] VertexFormatData { get; set; }
        public byte[] AdditionalData { get; set; }

        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            Materials = new List<MaterialTextureReference>();

            long pos = r.Position;

            Size = r.ReadU32();

            Header = r.ReadBytes(28); //read the header
            //find the vertex format
            RW4Section vertexFormatSection = m.Sections.Find(sec => sec.TypeCode == SectionTypeCodes.VertexFormat);
            if (vertexFormatSection != null)
            {
                int vertexFormatSize = vertexFormatSection.obj.ComputeSize();
                VertexFormatData = r.ReadBytes(vertexFormatSize);
            }

            long currentPos = r.Position;
            uint u2D = 0;
            while (u2D != 0x2D)
            {
                u2D = r.ReadU32();
            }
            int additionalDataSize = (int)r.Position - (int)currentPos - 4;
            r.Seek(currentPos, SeekOrigin.Begin);

            AdditionalData = r.ReadBytes(additionalDataSize);

            for (int i = 0; i < 6; i++)
            {
                MaterialTextureReference mat = new MaterialTextureReference();
                mat.Unknown1 = r.ReadU32();
                mat.Unknown2 = r.ReadU32();
                mat.TextureInstanceId = r.ReadU32();
                mat.Unknown3 = r.ReadU32();
                mat.Unknown4 = r.ReadU32();
                mat.Unknown5 = r.ReadU32();
                Materials.Add(mat);
            }

            Data = r.ReadBytes((int)Size - (int)(r.Position - pos));
        }

        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            w.WriteU32(Size);
            if (Header != null)
            {
                w.Write(Header, 0, Header.Length);
            }
            if (VertexFormatData != null)
            {
                w.Write(VertexFormatData, 0, VertexFormatData.Length);
            }
            if (AdditionalData != null)
            {
                w.Write(AdditionalData, 0, AdditionalData.Length);
            }
            foreach (MaterialTextureReference mat in Materials)
            {
                w.WriteU32(mat.Unknown1);
                w.WriteU32(mat.Unknown2);
                w.WriteU32(mat.TextureInstanceId);
                w.WriteU32(mat.Unknown3);
                w.WriteU32(mat.Unknown4);
                w.WriteU32(mat.Unknown5);
            }
            if (Data != null)
            {
                w.Write(Data, 0, Data.Length);
            }
        }

        public override int ComputeSize()
        {
            return (int)Size;
        }
    };
}
