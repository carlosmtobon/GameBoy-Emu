using GameBoy_Emu.core.cpu;
using GameBoy_Emu.core.ram;
using GameBoy_Emu.core.utils;

namespace GameBoy_Emu.core.ppu
{
    public class Ppu
    {
        private readonly Mmu _ram;
        private readonly Fetcher _fetcher;
        public Display Display { get; set; }
        public PpuStatus Status { get; set; }

        public int OAM_SEARCH_CYCLES = 80;
        public int PIXEL_PROCESS_CYCLES = 172;
        public int HBLANK_CYCLES = 204;
        public int VBLANK_CYCLES = 456;

        public enum PpuStatus { HBLANK, VBLANK, OAM_SEARCH, PIXEL_TRANSFER };

        public Ppu(Mmu ram, Display display)
        {
            _ram = ram;
            _fetcher = new Fetcher(display, ram);
            Display = display;
            Status = PpuStatus.OAM_SEARCH;
        }

        int clocks;
        
        public void Tick(int cpuCycles)
        {
            clocks += cpuCycles;
            SetMode();
            if (Status == PpuStatus.OAM_SEARCH)
            {
                OamSearch();
            }
            else if (Status == PpuStatus.PIXEL_TRANSFER)
            {
                LcyCompare();
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

        private void LcyCompare()
        {
            //lyc lcy compare
            byte lcy = _ram.LoadUnsigned8(Mmu.LCY_REGISTER);
            byte ly = _ram.LoadLy();
            if (ly == lcy)
            {
                byte stat = _ram.LoadStat();
                _ram.StoreUnsigned8(Mmu.STAT_REGISTER, BitUtils.SetBit(stat, 2));
            }
            
            SetLcdcInterruptIfNeeded(6);
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
                
                _fetcher.FindVisibleSprites();
            }
        }

        private void PixelTransfer(int cpuCycles)
        {
            _fetcher.Tick(cpuCycles);
            
            if (clocks >= PIXEL_PROCESS_CYCLES)
            {
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
            interruptFlag = BitUtils.SetBitsWithMask(interruptFlag, flag);
            _ram.StoreUnsigned8(Mmu.IF_REGISTER, interruptFlag);
        }
    }
}
