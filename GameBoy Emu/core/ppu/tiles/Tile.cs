using ChichoGB.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.core.ppu
{
    public class Tile
    {
        public int Address { get; set; }
        public byte[] TileData { get; set; }

        public Tile(int address, byte[] tilePixels)
        {
            Address = address;
            TileData = tilePixels;
        }

        public Queue<PixelData> GetRowPixelData(int row)
        {
            Queue<PixelData> pixels = new Queue<PixelData>();
            byte h = TileData[(row * 2)];
            byte l = TileData[(row * 2)  + 1];
            for (int j = 7; j >= 0; j--)
            {
                int pixelColor = (l >> j) & 1 | (h >> j & 1);
                PixelData pixel = new PixelData();
                pixel.ColorData = pixelColor;
                pixel.Type = PixelData.PixelType.BG;
                pixels.Enqueue(pixel);
            }
            return pixels;
        }
    }
}
