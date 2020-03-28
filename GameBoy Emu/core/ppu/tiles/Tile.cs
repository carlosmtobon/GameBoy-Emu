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
    }
}
