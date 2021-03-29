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
    public class DXT8Image : ITexture
    {
        private byte[] _data;

        /// <summary>
        /// Read a DXT8 file
        /// </summary>
        /// <param name="stream"></param>
        public void Read(System.IO.Stream strm)
        {
            strm.expect(0x20534444, "DDS000");  // 'DDS '
            int headerSize = strm.ReadS32();
            if (headerSize < 0x7C) throw new ModelFormatException(strm, "DDS001", headerSize);
            uint flags = strm.ReadU32();
            var _height = strm.ReadS32();
            var _width = strm.ReadS32();
            var pitchOrLinearSize = strm.ReadU32();
            strm.expect(0, "DDS002");  // < depth
            var mipmaps = strm.ReadS32();

            strm.Seek(strm.Position + 11 * 4, SeekOrigin.Begin);
            var pfsize = strm.ReadS32();
            if (pfsize < 32) throw new ModelFormatException(strm, "DDS011", pfsize);
            uint pf_flags = strm.ReadU32();
            var fourcc = strm.ReadU32();
            long pos = strm.Position;

            strm.Seek(headerSize + 4, SeekOrigin.Begin);
            var size = strm.Length - strm.Position;

            var all_mipmaps = new byte[size];

            strm.Read(all_mipmaps, 0, (int)size);


            TextureType = 0x15;

            _data = all_mipmaps;

            Height = _height;
            Width = _width;
        }

        public byte[] GetAsByteArray()
        {
            return _data;
        }

        public void Write(System.IO.Stream stream)
        {
            stream.WriteU32(0x20534444);  // 'DDS '
            stream.WriteU32(0x7C);  // header size
            stream.WriteU32(0x81007);  // flags: 
            stream.WriteU32((uint)this.Height);
            stream.WriteU32((uint)this.Width);
            stream.WriteU32((uint)this.Height * (uint)this.Width * 4);
            stream.WriteU32(0); //dwdepth
            stream.WriteU32(0); // mipmap count
            for (int i = 0; i < 11; i++)
                stream.WriteU32(0); //empty reserved dword entries

            // pixel format
            stream.WriteU32(32); //ddspf
            stream.WriteU32(0x41);  // flags or something
            stream.WriteU32(0x00); // not compressed, no fourcc
            stream.WriteU32(32); // 
            stream.WriteU32(0x00ff0000); //rbbitmask
            stream.WriteU32(0x0000ff00); //gbitmask
            stream.WriteU32(0x000000ff); //bbitmask
            stream.WriteU32(0xff000000); //abitmask

            //caps
            stream.WriteU32(0x00001000); //caps1
            stream.WriteU32(0x00000000); //caps2
            stream.WriteU32(0x00000000); //caps3
            stream.WriteU32(0x00000000); //caps4
            stream.WriteU32(0x00000000); //unused

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
