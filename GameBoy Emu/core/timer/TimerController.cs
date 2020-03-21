using ChichoGB.Core.CPU;
using System;

namespace ChichoGB.Core.Timer
{
    public class TimerController
    {
        public bool InterruptRequest { get; set; }
        public int Frequency { get; set; }

        private int _totalElaspedDiv;

        private int _divAccumalator;

        private int _totalElaspedTimer;

        private int _timerAccumalator;

        private readonly Mmu _ram;

        public TimerController(Mmu ram)
        {
            _ram = ram;
        }

        public int totalTimerUpdates;
        public int cpuClockSince;
        public void Tick(int cpuCycles, bool halt)
        {
            IncrementDiv(cpuCycles);

            byte tac = _ram.LoadU8Bits(Mmu.TAC_ADDRESS);
           
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

                if (_totalElaspedTimer == 0)
                {
                    _totalElaspedTimer = cpuCycles;
                }
                _timerAccumalator += halt ? 4 : cpuCycles - _totalElaspedTimer;
                cpuClockSince += cpuCycles - _totalElaspedTimer;
                _totalElaspedTimer = cpuCycles;

                if (_timerAccumalator >= Frequency)
                {
                    totalTimerUpdates++;
                    _timerAccumalator -= Frequency;
                    byte tma = _ram.LoadU8Bits(Mmu.TMA_ADDRESS);
                    byte tima = _ram.LoadU8Bits(Mmu.TIMA_ADDRESS);
                    if (tima + 1 > 0xff)
                    {
                        // enable interrupt;
                        InterruptRequest = true;
                        tima = tma;
                    }
                    tima++;
                    _ram.StoreU8Bits(Mmu.TIMA_ADDRESS, tima);
                }
            }

            if (InterruptRequest)
            {
                byte interruptFlag = _ram.Memory[Mmu.IF_ADDRESS];
                // set timer overflow
                interruptFlag = BitUtils.SetBit(interruptFlag, 0x4);
                _ram.StoreU8Bits(Mmu.IF_ADDRESS, interruptFlag);
            }
        }

        public int totalDivUpdates;
        private void IncrementDiv(int cpuCycles)
        {
            _divAccumalator += cpuCycles - _totalElaspedDiv;
            _totalElaspedDiv = cpuCycles;

            if (_divAccumalator > 256)
            {
                totalDivUpdates++;
                _divAccumalator -= 256;
                byte div = _ram.LoadU8Bits(Mmu.DIV_ADDRESS);
                div++;
                _ram.StoreU8Bits(Mmu.DIV_ADDRESS, div);
            }
        }

        private static bool IsTimerOn(byte tac)
        {
            return BitUtils.isBitSet(tac, 2);
        }
    }
}
