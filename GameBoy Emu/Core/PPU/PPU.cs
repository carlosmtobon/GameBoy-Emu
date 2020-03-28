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
        public int HBLANK_CYCLES = 204;

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
                PixelTransfer(cpuCycles);
            }
            else if (_ppuStatus == PpuStatus.HBLANK)
            {
                if (clocks >= HBLANK_CYCLES)
                {
                    clocks -= HBLANK_CYCLES;
                    Console.WriteLine($"Total Cycles Per Line: {clocks}");
                }
            }
            else if (_ppuStatus == PpuStatus.VBLANK)
            {

            }
        }

        private void PixelTransfer(int cpuCycles)
        {
            while (cpuCycles > 0)
            {
                int work = _pixelFifo.Process();
                _fetcher.Process();
                cpuCycles -= work;

                if (_fetcher.State == Fetcher.FetcherState.TRANSFER_READY && _pixelFifo.State == PixelFifo.PixelFifoState.IDLE)
                {
                    Console.WriteLine("Load Fifo");
                    _pixelFifo.LoadFifo(_fetcher);
                    _fetcher.State = Fetcher.FetcherState.READ_TILE_NUM;
                    _pixelFifo.State = PixelFifo.PixelFifoState.PUSHING;
                }
            }

            if (clocks >= PIXEL_PROCESS_CYCLES)
            {
                Console.WriteLine($"PUSH TIMES: {_pixelFifo.pushTimes}");
                _pixelFifo.pushTimes = 0;
                clocks -= PIXEL_PROCESS_CYCLES;
                _ppuStatus = PpuStatus.HBLANK;
            }
        }

        private void OamSearch()
        {
           // throw new NotImplementedException();
        }

        public byte GetLcdcStat()
        {
            return _ram.LoadLcdcStat();
        }

    }
}
