using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.Core
{
    public class PPU
    {
        private MMU _ram;
        private PixelFIFO _pixelFIFO;
        private Fetcher _fetcher;

        const int startAddr = 0x104;

        public PPU(MMU ram)
        {
            _ram = ram;
            _pixelFIFO = new PixelFIFO();
            _fetcher = new Fetcher();
        }

        public void Process()
        {
            
        }
    }
}
