using GameBoy_Emu.core.mmu;
using System.Collections.Generic;

namespace GameBoy_Emu.core.ppu
{
    public class OamEntryManager
    {
        public List<OamEntry> Sprites { get; set; }
        public List<OamEntry> VisibleSprites { get; set; }

        private readonly Mmu _mmu;

        public const int OAM_START_ADDRESS = 0xFE00;
        public const int OAM_END_ADDRESS = 0xFE9F;

        public OamEntryManager(Mmu mmu)
        {
            _mmu = mmu;
            Sprites = new List<OamEntry>();
            VisibleSprites = new List<OamEntry>();
        }

        public List<OamEntry> GetOamEntries()
        {
            Sprites.Clear();
            for (int addr = OAM_START_ADDRESS; addr <= OAM_END_ADDRESS; addr += 4)
            {
                var sprite = new OamEntry();
                sprite.YPos = _mmu.LoadUnsigned8(addr);
                sprite.XPos = _mmu.LoadUnsigned8(addr + 1);
                sprite.TileNumber = _mmu.LoadUnsigned8(addr + 2);
                sprite.AttributeFlag = _mmu.LoadUnsigned8(addr + 3);
                Sprites.Add(sprite);
            }

            return Sprites;
        }

        public void FindVisibleSprites(int currentLine, int spriteHeight)
        {
            VisibleSprites.Clear();
            for (int addr = OAM_START_ADDRESS; addr <= OAM_END_ADDRESS; addr += 4)
            {
                var sprite = new OamEntry();
                sprite.YPos = _mmu.LoadUnsigned8(addr);
                sprite.XPos = _mmu.LoadUnsigned8(addr + 1);
                sprite.TileNumber = _mmu.LoadUnsigned8(addr + 2);
                sprite.AttributeFlag = _mmu.LoadUnsigned8(addr + 3);

                if (sprite.XPos != 0 && currentLine + 16 >= sprite.YPos && currentLine + 16 < sprite.YPos + spriteHeight)
                {
                    VisibleSprites.Add(sprite);
                }

                if (VisibleSprites.Count == 10)
                {
                    break;
                }
            }
        }
    }
}
