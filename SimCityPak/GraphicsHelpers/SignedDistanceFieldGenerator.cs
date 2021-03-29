using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace SimCityPak
{
    public class SignedDistanceFieldGenerator
    {
        public const int infinity = 5000;
        DistancePoint[,] grid1;
        DistancePoint[,] grid2;
        WriteableBitmap _bitmap;
        public WriteableBitmap _distanceBitmap;

        public SignedDistanceFieldGenerator(WriteableBitmap bitmap)
        {
            if (bitmap != null)
            {
                grid1 = new DistancePoint[(int)bitmap.PixelHeight, (int)bitmap.PixelWidth];
                grid2 = new DistancePoint[(int)bitmap.PixelHeight, (int)bitmap.PixelWidth];
                _bitmap = bitmap;
                _distanceBitmap = new WriteableBitmap(_bitmap.PixelWidth, _bitmap.PixelHeight, 96, 96, PixelFormats.Pbgra32, BitmapPalettes.Halftone64);
                InitializeGrid();
                Generate();
            }
        }

        public void InitializeGrid()
        {
            SimCityPak.ExtensionMethods.PixelColor[,] pixels = _bitmap.GetPixels();
            for (int i = 0; i < pixels.GetLength(1); i++)
            {
                for (int j = 0; j < pixels.GetLength(0); j++)
                {
                    if (pixels[j, i].Blue < 128)
                    {
                        grid1[i, j].dx = infinity;
                        grid1[i, j].dy = infinity;
                        grid2[i, j].dx = 0;
                        grid2[i, j].dy = 0;
                    }
                    else
                    {
                        grid2[i, j].dx = infinity;
                        grid2[i, j].dy = infinity;
                        grid1[i, j].dx = 0;
                        grid1[i, j].dy = 0;
                    }
                }
            }
        }

        public void Compare(ref DistancePoint[,] grid, ref DistancePoint point, int x, int y, int offsetX, int offsetY)
        {
            DistancePoint other = new DistancePoint();
            if (x + offsetX > 0 && y + offsetY > 0 && x + offsetX < grid.GetLength(0) && y + offsetY < grid.GetLength(1))
            {
                other.dx = grid[x + offsetX, y + offsetY].dx + offsetX;
                other.dy = grid[x + offsetX, y + offsetY].dy + offsetY;
            }
            else
            {
                other = new DistancePoint() { dx = infinity, dy = infinity };
            }

            if (other.distSq() < point.distSq())
            {
                point.dx = other.dx;
                point.dy = other.dy;
            }
        }

        public void Generate2()
        {
            for (int y = 0; y < _bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < _bitmap.PixelWidth; x++)
                {
                    byte a = _bitmap.GetPixel(x, y).R;
                    byte distance = byte.MaxValue;
                }
            }
        }


        public void Generate()
        {
            GenerateSDF(ref grid1);
            GenerateSDF(ref grid2);
            //final pass
            for (int y = 0; y < _bitmap.PixelWidth; y++)
            {
                for (int x = 0; x < _bitmap.PixelHeight; x++)
                {
                    int dist2 = (int)Math.Sqrt(grid1[x, y].distSq());
                    int dist1 = (int)Math.Sqrt(grid2[x, y].distSq());

                }
            }

            int scaleFactor = 1;

            float spread = Math.Max(grid1.GetLength(1), grid1.GetLength(0)) / (1 << scaleFactor);
            float min = -128;

            int width = _distanceBitmap.PixelWidth;
            int height = _distanceBitmap.PixelHeight;

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    float dist = (float)Math.Sqrt(grid2[x, y].distSq()) - (float)Math.Sqrt(grid1[x, y].distSq());
                    /*dist = dist < 0
                        ? 128 * (dist - min) / 128
                        : 129;*/

                    if (dist > 0)
                    {

                        dist = 128 - (float)((dist + 1) * 8);
                    }
                    else
                    {
                        dist = 128 - (float)((dist) * 8); //for thin sections
                      //  dist = 128 - (float)((dist + 0.5) * 8); //for normal sections
                    }

                    byte channel = (byte)Math.Max(0, Math.Min(255, dist));

                    _distanceBitmap.SetPixel(y, x, Color.FromArgb(255, channel, channel, channel));
                }
            }
        }

        public void GenerateSDF(ref DistancePoint[,] grid)
        {
            //pass 0
            for (int y = 0; y < _bitmap.PixelWidth; y++)
            {
                for (int x = 0; x < _bitmap.PixelHeight; x++)
                {
                    DistancePoint p = grid[x, y];

                    Compare(ref grid, ref p, x, y, -1, 0);
                    Compare(ref grid, ref p, x, y, 0, -1);
                    Compare(ref grid, ref p, x, y, -1, -1);
                    Compare(ref grid, ref p, x, y, 1, -1);

                    grid[x, y] = p;
                }

                for (int x = _bitmap.PixelHeight - 1; x >= 0; x--)
                {
                    DistancePoint p = grid[x, y];
                    Compare(ref grid, ref p, x, y, 1, 0);
                    grid[x, y] = p;
                }
            }

            //pass 1
            for (int y = _bitmap.PixelWidth - 1; y >= 0; y--)
            {
                for (int x = _bitmap.PixelHeight - 1; x >= 0; x--)
                {
                    DistancePoint p = grid[x, y];

                    Compare(ref grid, ref p, x, y, 1, 0);
                    Compare(ref grid, ref p, x, y, 0, 1);
                    Compare(ref grid, ref p, x, y, -1, 1);
                    Compare(ref grid, ref p, x, y, 1, 1);

                    grid[x, y] = p;
                }

                for (int x = 0; x < _bitmap.PixelHeight; x++)
                {
                    DistancePoint p = grid[x, y];
                    Compare(ref grid, ref p, x, y, -1, 0);
                    grid[x, y] = p;
                }
            }
        }

        public struct DistancePoint
        {
            public int dx;
            public int dy;

            public int distSq()
            {
                return dx * dx + dy * dy;
            }
        }
    }
}
