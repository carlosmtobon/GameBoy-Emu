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
        public int Frequency { set; get; } = 0;
        public int CurrentDuty { get; set; } = 0;

        public SquareChannel(Mmu mmu)
        {
            _mmu = mmu;
        }

        public void Trigger(int channel)
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
                Frequency = (2048 - ((nr4 & 0b00111111) << 5 | nr3) * 4);
                CurrentDuty = (nr1 & 0b11000000) >> 6;
                Length = 64 - (nr1 & 0b00111111);
            }

            Length--;

            if (Length == 0)
            {
                Length = 64;
                Enabled = false;
                
            }

            Debug.WriteLine($"Channel {channel} Duty: {CurrentDuty}");
            Debug.WriteLine($"Channel {channel} Enable: {lengthEnable}");
        }

        public void Tick(int cpuCycles)
        {
            if (Enabled)
            {
                AccumClock += cpuCycles;
                if (AccumClock >= Cpu.CYCLES_PER_SECOND / Frequency)
                {
                    // channel is on
                    var dutySequence = waveDutyTable[CurrentDuty];
                    AccumClock -= Cpu.CYCLES_PER_SECOND / Frequency;
                    // sample square wave
                    Output = dutySequence[DutyCounter % 8];
                    DutyCounter++;
                }
            }
            else
            {
                Output = 0;
            }
        }

        public float Sample()
        {
            return (Output/100f * Volume * 100);
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