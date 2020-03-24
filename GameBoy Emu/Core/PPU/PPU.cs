using GameBoy_Emu.core.ppu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChichoGB.Core
{
    public class Ppu
    {
        private Mmu _ram;
        private PixelFIFO _pixelFIFO;
        private Fetcher _fetcher;

        private BackgroundTileMap _backgroundTileMap;

        public Ppu(Mmu ram)
        {
            _ram = ram;
            _pixelFIFO = new PixelFIFO();
            _fetcher = new Fetcher();

            _backgroundTileMap = new BackgroundTileMap(_ram);
        }

        int clocks;
        public void Tick()
        {
            clocks += 4;
            if (clocks > CPU.Cpu.CYCLES_PER_SECOND)
            {
                clocks = 0;
                _backgroundTileMap.LoadTileMap();
            }
        }
    }
}
