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
            new short[] { -1, -1, -1, -1, -1, -1, -1, 1 },
            new short[] { 1, -1, -1, -1, -1, -1, -1, 1 },
            new short[] { 1, -1, -1, -1, -1, 1, 1, 1 },
            new short[] { -1, 1, 1, 1, 1, 1, 1, -1 }
        };

        private Mmu _mmu;

        // square 1
        public int DutyCounter { get; set; } = 0;
        public int AccumClock { get; set; } = 0;
        public int Length { get; set; } = 0;
        public bool Enabled { get; set; } = false;
        public short Output { get; set; } = 0;
        public short Volume { get; set; } = 0;
        public int TimerPeriod { set; get; } = 0;
        public int Timer { get; set; }
        public int CurrentDuty { get; set; } = 0;

        public SquareChannel(Mmu mmu)
        {
            _mmu = mmu;
        }

        public void TriggerLength(int channel)
        {
            // grab square registers
            int nr1;
            int nr3;
            int nr4;

            if (channel == 1)
            {
                // grab square 1 registers
                nr1 = _mmu.LoadUnsigned8(Mmu.NR11_REGISTER);
                nr3 = _mmu.LoadUnsigned8(Mmu.NR13_REGISTER);
                nr4 = _mmu.LoadUnsigned8(Mmu.NR14_REGISTER);
            }
            else
            {
                // grab square 2 registers
                nr1 = _mmu.LoadUnsigned8(Mmu.NR21_REGISTER);
                nr3 = _mmu.LoadUnsigned8(Mmu.NR23_REGISTER);
                nr4 = _mmu.LoadUnsigned8(Mmu.NR24_REGISTER);
            }


            // Debug.WriteLine($"Length Enable: {BitUtils.GetBit((byte)nr4, 6)}");
            byte triggerVal = BitUtils.GetBit((byte)nr4, 7);
            byte lengthEnable = BitUtils.GetBit((byte)nr4, 6);

            if (triggerVal == 1)
            {
                Enabled = true;
                var low = nr3;
                var high = (nr4 & 0b0000_0111) << 8;
                TimerPeriod = (2048 - (high | low));
                Timer = TimerPeriod;
                CurrentDuty = (nr1 & 0b11000000) >> 6;
                Length = 64 - (nr1 & 0b00111111);
            }

            Length--;

            if (Length == 0)
            {
                Length = 64;
                Enabled = false;
            }
        }

        public void Tick(int cpuCycles)
        {
            AccumClock += cpuCycles;

            while (AccumClock > 0)
            {
                AccumClock -= 1;
                Timer--;
                if (Timer == 0)
                {
                    var dutySequence = waveDutyTable[CurrentDuty];
                    Output = dutySequence[DutyCounter % 8];
                    DutyCounter++;
                    Timer = TimerPeriod;
                }
            }
        }

        public float Sample()
        {
            if (!Enabled) return 0;
            return (Output * Volume);
        }

        public void SetVolume(int channel)
        {
            // grab square registers
            int nr2 = 0;

            if (channel == 1)
            {
                // grab square 1 registers
                nr2 = _mmu.LoadUnsigned8(Mmu.NR12_REGISTER);
            }
            else
            {
                // grab square 2 registers
                nr2 = _mmu.LoadUnsigned8(Mmu.NR22_REGISTER);
            }

            Volume = (short)((nr2 & 0b11110000) >> 4);
        }
    }
}