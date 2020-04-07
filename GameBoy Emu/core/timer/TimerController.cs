using GameBoy_Emu.core.cpu;
using GameBoy_Emu.core.ram;
using GameBoy_Emu.core.utils;

namespace GameBoy_Emu.core.timer
{
    public class TimerController
    {
        public bool InterruptRequest { get; set; }
        public int Frequency { get; set; }

        private int _divAccumalator;

        private int _timerAccumalator;

        public const int DIV_FREQUENCY = 256;

        private readonly Mmu _ram;

        public TimerController(Mmu ram)
        {
            _ram = ram;
        }

        public int totalTimerUpdates;
        public int cpuClockSince;
        public void Tick(int cpuCycles, bool halt)
        {
            IncrementDiv(cpuCycles, halt);

            byte tac = _ram.LoadUnsigned8(Mmu.TAC_REGISTER);

            if (IsTimerOn(tac))
            {
                if (Frequency == 0)
                {
                    // check first 2 bits for freq
                    byte clockSelect = (byte)(tac & 3);
                    switch (clockSelect)
                    {
                        case 0x0:
                            Frequency = Cpu.CYCLES_PER_SECOND / 4096;
                            break;
                        case 0x1:
                            Frequency = Cpu.CYCLES_PER_SECOND / 262144;
                            break;
                        case 0x2:
                            Frequency = Cpu.CYCLES_PER_SECOND / 65536;
                            break;
                        case 0x3:
                            Frequency = Cpu.CYCLES_PER_SECOND / 16384;
                            break;
                    }
                }

                _timerAccumalator += halt ? 4 : cpuCycles;
                cpuClockSince += cpuCycles;

                if (_timerAccumalator >= Frequency)
                {
                    totalTimerUpdates++;
                    _timerAccumalator -= Frequency;
                    byte tma = _ram.LoadUnsigned8(Mmu.TMA_REGISTER);
                    byte tima = _ram.LoadUnsigned8(Mmu.TIMA_REGISTER);
                    if (tima + 1 > 0xff)
                    {
                        // enable interrupt;
                        InterruptRequest = true;
                        tima = tma;
                    }
                    else
                    {
                        tima++;
                    }

                    _ram.StoreUnsigned8(Mmu.TIMA_REGISTER, tima);
                }
            }

            if (InterruptRequest)
            {
                byte interruptFlag = _ram.Memory[Mmu.IF_REGISTER];
                // set timer overflow
                interruptFlag = BitUtils.SetBitsWithMask(interruptFlag, InterruptController.TIMER_FLAG);
                _ram.StoreUnsigned8(Mmu.IF_REGISTER, interruptFlag);
                InterruptRequest = false;
            }
        }

        public int totalDivUpdates;
        private void IncrementDiv(int cpuCycles, bool halt)
        {
            _divAccumalator += halt ? 4 : cpuCycles;

            if (_divAccumalator >= DIV_FREQUENCY)
            {
                totalDivUpdates++;
                _divAccumalator -= DIV_FREQUENCY;
                byte div = _ram.LoadUnsigned8(Mmu.DIV_REGISTER);
                div++;
                _ram.StoreUnsigned8(Mmu.DIV_REGISTER, div);
            }
        }

        private static bool IsTimerOn(byte tac)
        {
            return BitUtils.isBitSet(tac, 2);
        }
    }
}
