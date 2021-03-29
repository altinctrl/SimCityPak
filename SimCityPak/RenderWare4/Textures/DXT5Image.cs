using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework.Graphics;
using Gibbed.Spore.Helpers;
using System.Windows.Interop;
using SporeMaster.RenderWare4;
using System.IO;
using Microsoft.Win32;

namespace SimCityPak
{
    public class DXT5Image : ITexture
    {
        private byte[] _data;

        /// <summary>
        /// Read a DXT5 file
        /// </summary>
        /// <param name="stream"></param>
        public void Read(System.IO.Stream stream)
        {

            stream.expect(0x20534444, "DDS000");  // 'DDS '
            int headerSize = stream.ReadS32();
            if (headerSize < 0x7C) throw new ModelFormatException(stream, "DDS001", headerSize);
            var flags = stream.ReadU32();
            Height = stream.ReadS32();
            Width = stream.ReadS32();
            var pitchOrLinearSize = stream.ReadU32();
            stream.expect(0, "DDS002");  // < depth
            MipMaps = stream.ReadS32();

            stream.Seek(stream.Position + 11 * 4, SeekOrigin.Begin);
            var pfsize = stream.ReadS32();
            if (pfsize < 32) throw new ModelFormatException(stream, "DDS011", pfsize);
            var pf_flags = stream.ReadU32();
            var fourcc = stream.ReadU32();
            if (!(fourcc == SporeMaster.RenderWare4.Texture.DXT5 || fourcc == SporeMaster.RenderWare4.Texture.DXT1))
                throw new NotSupportedException("Texture packing currently only supports DXT5 compressed input textures.");

            stream.Seek(headerSize + 4, SeekOrigin.Begin);
            var sizes = (from i in Enumerable.Range(0, MipMaps)
                         select Math.Max(Width >> i, 4) * Math.Max(Height >> i, 4)  // DXT5: 16 bytes per 4x4=16 pixels
                            ).ToArray();
            var all_mipmaps = new byte[sizes.Sum()];
            for (int offset = 0, i = 0; i < MipMaps; i++)
            {
                if (stream.Read(all_mipmaps, offset, sizes[i]) != sizes[i])
                    throw new ModelFormatException(stream, "Unexpected EOF reading .DDS file", null);
                offset += sizes[i];
            }

            TextureType = fourcc;

            _data = all_mipmaps;

        }

        public byte[] GetAsByteArray()
        {
            return _data;
        }

        public void Write(System.IO.Stream stream)
        {
            stream.WriteU32(0x20534444);  // 'DDS '
            stream.WriteU32(0x7C);  // header size
            stream.WriteU32(0xA1007);  // flags: 
            stream.WriteU32((uint)this.Height);
            stream.WriteU32((uint)this.Width);
            stream.WriteU32((uint)this.Height * (uint)this.Width);  // size of top mipmap level... at least in DXT5 for >4x4
            stream.WriteU32(0);
            stream.WriteU32((uint)this.MipMaps / 0x100);
            for (int i = 0; i < 11; i++)
                stream.WriteU32(0);

            // pixel format
            stream.WriteU32(32);
            stream.WriteU32(4);  // DDPF_FOURCC?
            stream.WriteU32(this.TextureType);
            stream.WriteU32(32);
            stream.WriteU32(0xff0000);
            stream.WriteU32(0x00ff00);
            stream.WriteU32(0x0000ff);
            stream.WriteU32(0xff000000);
            stream.WriteU32(0);  // 0x41008
            for (int i = 0; i < 4; i++)
                stream.WriteU32(0);

            stream.Write(_data, 0, _data.Length);
        }

        public int Height { get; set; }
        public int Width { get; set; }

        public uint TextureType { get; set; }

        public int MipMaps { get; set; }


        public void SetData(byte[] data)
        {
            _data = data;
        }
    }
}
