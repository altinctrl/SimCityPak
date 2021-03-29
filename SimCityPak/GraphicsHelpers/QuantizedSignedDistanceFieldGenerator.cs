using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;

namespace SimCityPak
{
    public class QuantizedSignedDistanceFieldGenerator
    {
        public const int infinity = 5000;
        DistancePoint[,] grid1;
        DistancePoint[,] grid2;
        public WriteableBitmap _distanceBitmap;
        nQuant.QuantizedPalette _palette;
        byte[] colourdata;

        private int _width;
        private int _height;

        public QuantizedSignedDistanceFieldGenerator(nQuant.QuantizedPalette palette, int height, int width)
        {
            if (palette != null)
            {
                _width = width;
                _height = height;


                _palette = palette;
                _distanceBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone64);

                colourdata = new byte[width * height * 4];
                //initialize the distance bitmap with empty colors
               /* for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        colourdata[(y * width * 4) + (x * 4)] = 0xFF;
                        colourdata[(y * width * 4) + (x * 4) + 1] = 0xFF;
                        colourdata[(y * width * 4) + (x * 4) + 2] = 0xFF;
                        colourdata[(y * width * 4) + (x * 4) + 3] = 0xFF;
                    }
                }
                _distanceBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), colourdata, width * 4, 0);*/
            }
        }

        public void InitializeGrid(int currentColor)
        {
            grid1 = new DistancePoint[_width, _height];
            grid2 = new DistancePoint[_width, _height];
            int[] pixels = _palette.PixelIndex;
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height ; j++)
                {
                    if (pixels[j *  _width + i] == currentColor)
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

        public WriteableBitmap GetBitmap()
        {
            _distanceBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, _distanceBitmap.PixelWidth, _distanceBitmap.PixelHeight), colourdata, _distanceBitmap.PixelWidth * 4, 0);
            return _distanceBitmap;
        }

        public void Generate(int colorIndex)
        {
            InitializeGrid(colorIndex);

            GenerateSDF(ref grid1);
            GenerateSDF(ref grid2);
            //final pass
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
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

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = (float)Math.Sqrt(grid2[x, y].distSq()) - (float)Math.Sqrt(grid1[x, y].distSq());

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
                    if (colorIndex != 3)
                    {
                        colourdata[(y * width * 4) + (x * 4) + colorIndex] = channel;
                    }
                    else
                    {
                        colourdata[(y * width * 4) + (x * 4) + colorIndex] = channel;
                    }
                }
            }
        }

        public void GenerateSDF(ref DistancePoint[,] grid)
        {
            //pass 0
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    DistancePoint p = grid[x, y];

                    Compare(ref grid, ref p, x, y, -1, 0);
                    Compare(ref grid, ref p, x, y, 0, -1);
                    Compare(ref grid, ref p, x, y, -1, -1);
                    Compare(ref grid, ref p, x, y, 1, -1);

                    grid[x, y] = p;
                }

                for (int x = _width - 1; x >= 0; x--)
                {
                    DistancePoint p = grid[x, y];
                    Compare(ref grid, ref p, x, y, 1, 0);
                    grid[x, y] = p;
                }
            }

            //pass 1
            for (int y = _height - 1; y >= 0; y--)
            {
                for (int x = _width - 1; x >= 0; x--)
                {
                    DistancePoint p = grid[x, y];

                    Compare(ref grid, ref p, x, y, 1, 0);
                    Compare(ref grid, ref p, x, y, 0, 1);
                    Compare(ref grid, ref p, x, y, -1, 1);
                    Compare(ref grid, ref p, x, y, 1, 1);

                    grid[x, y] = p;
                }

                for (int x = 0; x < _width; x++)
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
