using GameBoy_Emu.core.utils;

namespace GameBoy_Emu.core.ppu
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
            return BitUtils.IsBitSet(AttributeFlag, 6);
        }

        public bool IsXFlip()
        {
            return BitUtils.IsBitSet(AttributeFlag, 5);
        }

        public byte PaletteNumber()
        {
            return BitUtils.GetBit(AttributeFlag, 4);
        }
    }
}
