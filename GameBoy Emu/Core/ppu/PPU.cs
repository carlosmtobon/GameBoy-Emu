using GameBoy_Emu.core.cpu;
using GameBoy_Emu.core.ram;
using GameBoy_Emu.core.utils;

namespace GameBoy_Emu.core.ppu
{
    public class Ppu
    {
        private readonly Mmu _ram;
        private readonly BgTileMapManager _bgTileMapManager;
        private readonly PixelFifo _pixelFifo;
        private readonly Fetcher _fetcher;
        public Display Display { get; set; }
        public PpuStatus Status { get; set; }

        public int OAM_SEARCH_CYCLES = 80;
        public int PIXEL_PROCESS_CYCLES = 172;
        public int HBLANK_CYCLES = 204;
        public int VBLANK_CYCLES = 456;

        public enum PpuStatus { HBLANK, VBLANK, OAM_SEARCH, PIXEL_TRANSFER };

        public Ppu(Mmu ram, Display screen)
        {
            _ram = ram;
            _bgTileMapManager = new BgTileMapManager(ram);
            _pixelFifo = new PixelFifo();
            _fetcher = new Fetcher(_bgTileMapManager, ram);
            Display = screen;
            Status = PpuStatus.OAM_SEARCH;
        }

        int clocks;
        int totalClockPerFrame;
        public void Tick(int cpuCycles)
        {
            totalClockPerFrame += cpuCycles;
            clocks += cpuCycles;
            SetMode();
            if (Status == PpuStatus.OAM_SEARCH)
            {
                OamSearch();
            }
            else if (Status == PpuStatus.PIXEL_TRANSFER)
            {
                PixelTransfer(cpuCycles);
            }
            else if (Status == PpuStatus.HBLANK)
            {
                Hblank();
            }
            else if (Status == PpuStatus.VBLANK)
            {
                Vblank();
            }
        }

        public void SetMode()
        {
            byte stat = _ram.LoadStat();
            switch (Status)
            {
                case PpuStatus.OAM_SEARCH:
                    stat = BitUtils.SetBit(stat, 1);
                    stat = BitUtils.ClearBit(stat, 0);
                    break;
                case PpuStatus.PIXEL_TRANSFER:
                    stat = BitUtils.SetBit(stat, 1);
                    stat = BitUtils.SetBit(stat, 0);
                    break;
                case PpuStatus.HBLANK:
                    stat = BitUtils.ClearBit(stat, 1);
                    stat = BitUtils.ClearBit(stat, 0);
                    break;
                case PpuStatus.VBLANK:
                    stat = BitUtils.ClearBit(stat, 1);
                    stat = BitUtils.SetBit(stat, 0);
                    break;
            }
            _ram.StoreUnsigned8(Mmu.STAT_REGISTER, stat);
        }
        private void OamSearch()
        {
            if (clocks >= OAM_SEARCH_CYCLES)
            {
                clocks -= OAM_SEARCH_CYCLES;
                Status = PpuStatus.PIXEL_TRANSFER;
                SetLcdcInterruptIfNeeded(5);
                byte lcdc = _ram.LoadLcdc();
                int spriteHeight = 8;
                if (BitUtils.GetBit(lcdc, 2) == 1)
                {
                    spriteHeight = 16;
                }
                _bgTileMapManager.FindVisibleSprites(Display.Y, spriteHeight);
            }
        }

        private void PixelTransfer(int cpuCycles)
        {
            while (cpuCycles > 0)
            {
                int work = _pixelFifo.Process(Display);
                _fetcher.Process(Display);
                cpuCycles -= work;

                if (_fetcher.State == Fetcher.FetcherState.TRANSFER_READY && _pixelFifo.State == PixelFifo.PixelFifoState.IDLE)
                {
                    _pixelFifo.LoadFifo(_fetcher);
                }
            }

            if (clocks >= PIXEL_PROCESS_CYCLES)
            {
                _pixelFifo.Reset();
                _fetcher.Reset();
                clocks -= PIXEL_PROCESS_CYCLES;
                Status = PpuStatus.HBLANK;
            }
        }

        private void Hblank()
        {
            if (clocks >= HBLANK_CYCLES)
            {
                SetLcdcInterruptIfNeeded(3);
                clocks -= HBLANK_CYCLES;
                IncrementLy();
                Display.X = 0;
                if (Display.Y == (Display.Height - 1))
                {
                    Status = PpuStatus.VBLANK;
                }
                else
                {
                    Status = PpuStatus.OAM_SEARCH;
                }
            }
        }

        private void Vblank()
        {
            if (clocks >= VBLANK_CYCLES)
            {
                if (Display.Y == 143)
                {
                    SetInterrupt(InterruptController.VBLANK_FLAG);
                }
                SetLcdcInterruptIfNeeded(4);               
               
                IncrementLy();
                clocks -= VBLANK_CYCLES;
                byte bgp =_ram.LoadUnsigned8(Mmu.BGP_REGISTER);
                if (Display.Y > 153)
                {
                    Display.Draw = true;
                    Display.Y = 0;
                    Status = PpuStatus.OAM_SEARCH;
                }
            }
        }

        private void SetLcdcInterruptIfNeeded(int bitToCheck)
        {
            // set lcdc vblank?
            byte stat = _ram.LoadStat();
            if (BitUtils.isBitSet(stat, bitToCheck))
            {
                SetInterrupt(InterruptController.LCDC_FLAG);
            }
        }

        private void IncrementLy()
        {
            _ram.StoreUnsigned8(Mmu.LY_REGISTER, (byte)Display.Y++);
        }

        private void SetInterrupt(byte flag)
        {
            byte interruptFlag = _ram.Memory[Mmu.IF_REGISTER];
            interruptFlag = BitUtils.SetBitsWithMask(interruptFlag, InterruptController.VBLANK_FLAG);
            _ram.StoreUnsigned8(Mmu.IF_REGISTER, interruptFlag);
        }
    }
}
