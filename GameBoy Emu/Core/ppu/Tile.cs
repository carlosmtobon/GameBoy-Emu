﻿using System.Collections.Generic;
using System.Linq;

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

        public Queue<PixelData> GetRowPixelData(int row, PixelData.PixelType type, bool reverse = false)
        {
            Queue<PixelData> pixels = new Queue<PixelData>();
            byte lsb = TileData[(row * 2)];
            byte msb = TileData[(row * 2) + 1];
            for (int j = 7; j >= 0; j--)
            {
                int pixelColor = (lsb >> j) & 1 | (((msb >> j) & 1) << 1);
                PixelData pixel = new PixelData();
                pixel.ColorData = pixelColor;
                pixel.Type = type;
                pixels.Enqueue(pixel);
            }

            return reverse ? new Queue<PixelData>(pixels.Reverse()) : pixels;
        }
    }
}
