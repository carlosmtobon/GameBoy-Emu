using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.core.ppu
{
    public class TileData
    {
        private int _tileNumber;
        private int _address;
        private byte[] _tilePixels = new byte[16];

        public TileData(int tileNumber, int address, byte[] tilePixels)
        {
            _tileNumber = tileNumber;
            _address = address;
            _tilePixels = tilePixels;
        }
    }
}
