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
        private PixelFifo _pixelFifo;
        private Fetcher _fetcher;
        PpuStatus _ppuStatus;

        public int OAM_SEARCH_CYCLES = 80;
        public int PIXEL_PROCESS_CYCLES = 172;
        
        private int _totalElasped;

        enum PpuStatus { HBLANK, VBLANK, OAM_SEARCH, PIXEL_TRANSFER };

        public Ppu(Mmu ram)
        {
            _ram = ram;
            _pixelFifo = new PixelFifo(ram);
            _fetcher = new Fetcher(ram);
            _ppuStatus = PpuStatus.OAM_SEARCH;
        }

        int clocks;
        int totalUpdates;
        public void Tick(int cpuCycles)
        {
            clocks += cpuCycles;

            if (_ppuStatus == PpuStatus.OAM_SEARCH)
            {
                if (clocks >= OAM_SEARCH_CYCLES)
                {
                    OamSearch();
                    clocks -= OAM_SEARCH_CYCLES;
                    _ppuStatus = PpuStatus.PIXEL_TRANSFER;
                }
            }
            else if (_ppuStatus == PpuStatus.PIXEL_TRANSFER)
            {
                if (_fetcher.State == Fetcher.FetcherState.TRANSFER_READY && _pixelFifo.State == PixelFifo.PixelFifoState.IDLE)
                {
                    Console.WriteLine("Load Fifo");
                    _pixelFifo.LoadFifo(_fetcher);
                    _fetcher.State = Fetcher.FetcherState.READ_TILE_NUM;
                    _pixelFifo.State = PixelFifo.PixelFifoState.PUSHING;
                }

                while (cpuCycles > 0)
                {
                    _fetcher.Process(1);
                    _pixelFifo.Process(1);
                    
                    totalUpdates++;
                    cpuCycles--;
                }
              
                if (clocks >= PIXEL_PROCESS_CYCLES)
                {
                    Console.WriteLine(totalUpdates);
                    totalUpdates = 0;
                    clocks -= PIXEL_PROCESS_CYCLES;
                    _ppuStatus = PpuStatus.OAM_SEARCH;
                }
            }
            else if (_ppuStatus == PpuStatus.HBLANK)
            {
                Console.WriteLine();
            }
            else if (_ppuStatus == PpuStatus.VBLANK)
            {

            }
        }

        private void OamSearch()
        {
           // throw new NotImplementedException();
        }
    }
}
