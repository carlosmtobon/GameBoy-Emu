using GameBoy_Emu.core.cpu;
using GameBoy_Emu.core.mmu;
using GameBoy_Emu.core.utils;

namespace GameBoy_Emu.core.ppu
{
    public class Ppu
    {
        private readonly Mmu _mmu;
        private readonly Fetcher _fetcher;
        public Display Display { get; set; }
        public PpuStatus Status { get; set; }

        public int OAM_SEARCH_CYCLES = 80;
        public int PIXEL_PROCESS_CYCLES = 172;
        public int HBLANK_CYCLES = 204;
        public int VBLANK_CYCLES = 456;

        public enum PpuStatus { HBLANK, VBLANK, OAM_SEARCH, PIXEL_TRANSFER };

        public Ppu(Mmu mmu, Display display)
        {
            _mmu = mmu;
            _fetcher = new Fetcher(display, mmu);
            Display = display;
            Status = PpuStatus.OAM_SEARCH;
        }

        int clocks;
        private int total;
        
        public void Tick(int cpuCycles)
        {
            // lcdc enable logic?
            //if (!BitUtils.isBitSet(_mmu.LoadLcdc(), 7))
            //{
            //    clocks = 0;
               
            //    return;
            //}

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
            byte stat = _mmu.LoadStat();
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
            _mmu.StoreUnsigned8(Mmu.STAT_REGISTER, stat);
        }
        private void OamSearch()
        {
            if (clocks >= OAM_SEARCH_CYCLES)
            {
                clocks = 0;
                Status = PpuStatus.PIXEL_TRANSFER;
                SetLcdcInterruptIfNeeded(5);
                _fetcher.FindVisibleSprites();
            }
        }

        private void PixelTransfer(int cpuCycles)
        {
            _fetcher.Tick(cpuCycles);
            
            if (clocks >= PIXEL_PROCESS_CYCLES)
            {
                _fetcher.Reset();
                clocks = 0;
                Status = PpuStatus.HBLANK;
            }
        }

        private void Hblank()
        {
            if (clocks >= HBLANK_CYCLES)
            {
                IncrementLy();
                LycCompare();
                SetLcdcInterruptIfNeeded(3);
                clocks = 0;
                Display.X = 0;
                if (Display.Y >= (Display.Height - 1))
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
                //Console.WriteLine($"SCX: {_mmu.LoadUnsigned8(Mmu.SCX_REGISTER)}");
                clocks = 0;
                if (Display.Y == 144)
                {
                    SetInterrupt(InterruptController.VBLANK_MASK);
                   
                }
                if (Display.Y == 154)
                {
                    Display.Draw = true;
                    Display.Y = 0;
                    Status = PpuStatus.OAM_SEARCH;
                }
                else
                    Status = PpuStatus.HBLANK;
            }
        }
        
        private void LycCompare()
        {
            //lyc lcy compare
            byte lyc = _mmu.LoadUnsigned8(Mmu.LYC_REGISTER);
            byte ly = _mmu.LoadLy();
            byte lcdc = _mmu.LoadLcdc();

            if (ly == lyc)
            {
                SetLcdcInterruptIfNeeded(6);
            }
        }

        private void SetLcdcInterruptIfNeeded(int bitToCheck)
        {
            // set lcdc vblank?
            byte stat = _mmu.LoadStat();
            if (BitUtils.isBitSet(stat, bitToCheck))
            {
                SetInterrupt(InterruptController.LCDC_MASK);
            }
        }

        private void IncrementLy()
        {
            _mmu.StoreUnsigned8(Mmu.LY_REGISTER, (byte)Display.Y++);
        }

        private void SetInterrupt(byte flag)
        {
            byte interruptFlag = _mmu.LoadUnsigned8(Mmu.IF_REGISTER);
            interruptFlag = BitUtils.SetBitsWithMask(interruptFlag, flag);
            _mmu.StoreUnsigned8(Mmu.IF_REGISTER, interruptFlag);
        }
    }
}
