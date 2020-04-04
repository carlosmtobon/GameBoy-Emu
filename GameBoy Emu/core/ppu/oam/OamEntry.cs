using ChichoGB.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.core.ppu.oam
{
    public class OamEntry
    {
        public byte YPos { get; set; }
        public byte XPos { get; set; }
        public byte TileNumber { get; set; }
        public byte AttributeFlag { get; set; }

        public byte GetPriority()
        {
            return BitUtils.GetBit(AttributeFlag, 7);
        }

        public bool IsYFlip()
        {
            return BitUtils.isBitSet(AttributeFlag, 6);
        }

        public bool IsXFlip()
        {
            return BitUtils.isBitSet(AttributeFlag, 5);
        }

        public byte PaletteNumber()
        {
            return BitUtils.GetBit(AttributeFlag, 4);
        }
    }
}
