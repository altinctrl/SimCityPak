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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework.Graphics;
using Gibbed.Spore.Helpers;
using System.Windows.Interop;
using SporeMaster.RenderWare4;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using SimCityPak;


namespace SporeMaster.RenderWare4
{
    public class MaterialTextureConverter
    {
        public static Texture SetTexture(float[, ,] colors)
        {

            byte[] colorsData = new byte[(colors.GetLength(1) * colors.GetLength(0)) * 16];

            using (MemoryStream writer = new MemoryStream(colorsData, 0, colorsData.Length, true))
            {
                for (int y = 0; y < colors.GetLength(1); y++)
                {
                    for (int x = 0; x < colors.GetLength(0); x++)
                    {
                        writer.WriteF32(colors[x, y, 0]);
                        writer.WriteF32(colors[x, y, 1]);
                        writer.WriteF32(colors[x, y, 2]);
                        writer.WriteF32(colors[x, y, 3]);
                    }
                }
            }

            SporeMaster.RenderWare4.Texture texture = new SporeMaster.RenderWare4.Texture()
            {
                width = (ushort)colors.GetLength(0),
                height = (ushort)colors.GetLength(1),
                mipmapInfo = 0x180,
                textureType = 0x74,
                texData = new TextureBlob() { blob = colorsData },
                unk1 = 0
            };

            return texture;

        }
    }
}
