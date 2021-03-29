using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimCityPak
{
    public interface ITexture
    {
        int Height { get; set; }
        int Width { get; set; }

        uint TextureType { get; set; }

        int MipMaps { get; set; }

        void Read(Stream stream);

        byte[] GetAsByteArray();

        void SetData(byte[] data);

        void Write(Stream stream);
    }
}
