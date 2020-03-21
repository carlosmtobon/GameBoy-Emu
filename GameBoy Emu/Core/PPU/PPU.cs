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

        const int startAddr = 0x104;

        public Ppu(Mmu ram)
        {
            _ram = ram;
            _pixelFIFO = new PixelFIFO();
            _fetcher = new Fetcher();
        }

        public void Tick()
        {
            
        }
    }
}
