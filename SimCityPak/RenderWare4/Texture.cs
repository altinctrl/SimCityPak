using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Spore.Helpers;
using System.Windows.Media.Imaging;
using SimCityPak;
using System.Windows.Interop;
using System.Windows;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Media;

namespace SporeMaster.RenderWare4
{
    public class Texture : RW4Object
    {
        public const int type_code = 0x20003;
        public const int DXT5 = 0x35545844;   // 'DXT5'
        public const int DXT1 = 0x31545844;
        public TextureBlob texData;
        public UInt32 textureType;
        public UInt32 unk1;  //< dead pointer?
        public UInt16 width, height;
        public UInt32 mipmapInfo;  // 0x708 for 64x64 (7 mipmap levels); 0x808 for 128x128 (8 mipmap levels)

        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            if (s.type_code != type_code) throw new ModelFormatException(r, "T000", s.type_code);
            textureType = r.ReadU32();
            r.expect(8, "T001");
            unk1 = r.ReadU32();
            width = r.ReadU16();
            height = r.ReadU16();
            mipmapInfo = r.ReadU32();
            r.expect(0, "T002");
            r.expect(0, "T003");
            var sec = r.ReadS32();

            m.Sections[sec].GetObject(m, r, out texData);
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            w.WriteU32(textureType);
            w.WriteU32(8);
            w.WriteU32(unk1);
            w.WriteU16(width);
            w.WriteU16(height);
            w.WriteU32(mipmapInfo);
            w.WriteU32(0);
            w.WriteU32(0);
            w.WriteU32(texData.section.Number);
        }
        public override int ComputeSize() { return 4 * 7 + 2 * 2; }

        public WriteableBitmap ToImage(bool opaqueAlpha)
        {
            if (this.textureType != 21)
            {
                using (var stream = new MemoryStream())
                {
                    stream.WriteU32(0x20534444);  // 'DDS '
                    stream.WriteU32(0x7C);  // header size
                    stream.WriteU32(0xA1007);  // flags: 
                    stream.WriteU32(height);
                    stream.WriteU32(width);
                    stream.WriteU32((uint)height * (uint)width);  // size of top mipmap level... at least in DXT5 for >4x4
                    stream.WriteU32(0);
                    stream.WriteU32(mipmapInfo / 0x100);
                    for (int i = 0; i < 11; i++)
                        stream.WriteU32(0);

                    // pixel format
                    stream.WriteU32(32);
                    stream.WriteU32(4);  // DDPF_FOURCC?
                    stream.WriteU32(textureType);
                    stream.WriteU32(32);
                    stream.WriteU32(0xff0000);
                    stream.WriteU32(0x00ff00);
                    stream.WriteU32(0x0000ff);
                    stream.WriteU32(0xff000000);
                    stream.WriteU32(0);  // 0x41008
                    for (int i = 0; i < 4; i++)
                        stream.WriteU32(0);

                    stream.Write(texData.blob, 0, texData.blob.Length);

                    GraphicsDeviceService.AddRef(new WindowInteropHelper(Application.Current.MainWindow).Handle);
                    Texture2D texture;

                    try
                    {

                        DDSLib.DDSFromStream(stream, GraphicsDeviceService.Instance.GraphicsDevice, 0, true, out texture);
                        return new WriteableBitmap(DDSLib.Texture2Image(texture));
                    }
                    catch
                    {
                        return null;
                    }
                }


            }
            else //In this case it's a regular bitmap saved as a texture
            {
                using (MemoryStream byteStream = new MemoryStream(this.texData.blob, 0, this.texData.blob.Length))
                {
                    for (int i = 0; i < this.mipmapInfo; i++)
                    {
                        //  uint blockSize = byteStream.ReadU32().Swap();
                        WriteableBitmap bitmap = new WriteableBitmap((int)this.width, (int)this.height, 300, 300, PixelFormats.Pbgra32, BitmapPalettes.Halftone64);
                        if (this.textureType == 21)
                        {
                            for (int j = 0; j < (byteStream.Length / 4); j++)
                            {
                                byte b = byteStream.ReadU8();
                                byte g = byteStream.ReadU8();
                                byte r = byteStream.ReadU8();
                                byte a = byteStream.ReadU8();

                                if (opaqueAlpha)
                                {
                                    a = 255;
                                }

                                try
                                {
                                    if ((j / this.width) < this.height)
                                    {
                                        bitmap.SetPixel((int)(j % this.width), (int)(j / this.width), a, r, g, b);
                                    }
                                }
                                catch
                                {

                                }
                            }

                            return bitmap;
                        }

                    }
                }
            }
            return null;
        }

    }
}
