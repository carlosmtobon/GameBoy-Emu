using System.Diagnostics;
using System.Xml.Schema;
using GameBoy_Emu.core.cpu;
using GameBoy_Emu.core.mmu;
using GameBoy_Emu.core.utils;

namespace GameBoy_Emu.core.apu
{
    public class SquareChannel
    {
        public short[][] waveDutyTable =
        {
            new short[] { 1, 1, 1, 1, 1, 1, 1, -1 },
            new short[] { -1, 1, 1, 1, 1, 1, 1, -1 },
            new short[] { -1, 1, 1, 1, 1, -1, -1, -1 },
            new short[] { 1, -1, -1, -1, -1, -1, -1, 1 }
        };

        private Mmu _mmu;

        // square 1
        public int DutyCounter { get; set; } = 0;
        public int SquareAccum { get; set; } = 0;
        public int Length { get; set; } = 0;
        public bool Enabled { get; set; } = false;
        public short Volume { get; set; } = 0;

        public int WaveLength { set; get; } = 0;
        public int CurrentDuty { get; set; } = 0;

        public SquareChannel(Mmu mmu)
        {
            _mmu = mmu;
        }

        public void Trigger(int channel)
        {
            // grab square 1 registers
            int nr1 = 0;
            int nr2 = 0;
            int nr3 = 0;
            int nr4 = 0;
            if (channel == 1)
            {
                // grab square 1 registers
                nr1 = _mmu.LoadUnsigned8(Mmu.NR11_REGISTER);
                nr2 = _mmu.LoadUnsigned8(Mmu.NR12_REGISTER);
                nr3 = _mmu.LoadUnsigned8(Mmu.NR13_REGISTER);
                nr4 = _mmu.LoadUnsigned8(Mmu.NR14_REGISTER);
            }
            else
            {
                // grab square 2 registers
                nr1 = _mmu.LoadUnsigned8(Mmu.NR21_REGISTER);
                nr2 = _mmu.LoadUnsigned8(Mmu.NR22_REGISTER);
                nr3 = _mmu.LoadUnsigned8(Mmu.NR23_REGISTER);
                nr4 = _mmu.LoadUnsigned8(Mmu.NR24_REGISTER);
            }


            Debug.WriteLine($"Length Enable: {BitUtils.GetBit((byte)nr4, 6)}");
            byte triggerVal = BitUtils.GetBit((byte)nr4, 7);

            if (triggerVal == 1 && !Enabled)
            {
                Enabled = true;
                WaveLength = (131072 * 4) / (2048 - ((nr4 & 0b00111111) << 5 | nr3));
                CurrentDuty = (nr1 & 0b11000000) >> 6;
                Length = nr1 & 0b00111111;
            }

            Length++;
            if (Length == 64)
            {
                Enabled = false;
            }
        }

        public void Tick(int cpuCycles)
        {
            if (Enabled)
            {
                SquareAccum += cpuCycles;
                if (SquareAccum >= Cpu.CYCLES_PER_SECOND / WaveLength)
                {
                    // channel 2 is on
                    var dutySequence = waveDutyTable[CurrentDuty];
                    SquareAccum -= Cpu.CYCLES_PER_SECOND / WaveLength;
                    // sample square wave
                    Volume = dutySequence[DutyCounter % 8];
                    DutyCounter++;
                }
            }
            else
            {
                Volume = 0;
            }
        }

        public short Sample()
        {
            return Volume;
        }
    }
}