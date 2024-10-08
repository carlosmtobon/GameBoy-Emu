using System.Net.NetworkInformation;
using GameBoy_Emu.core.cpu;
using GameBoy_Emu.core.mmu;
using GameBoy_Emu.core.utils;

namespace GameBoy_Emu.core.apu
{
    public class WaveChannel
    {
        private Mmu _mmu;

        public bool DacPower { get; set; } = false;
        public int AccumClock { get; set; } = 0;
        public int Length { get; set; } = 0;
        public bool Enabled { get; set; } = false;
        public short Output { get; set; } = 0;
        public short VolumeShift { get; set; } = 0;
        public int TimerPeriod { get; set; } = 0;
        public int Timer { get; set; } = 0;

        public const int WaveTableStartAddr = 0xFF30;

        public byte[] waveTable = new byte[32];
        public int WaveTablePointer { get; set; } = 0;


        public WaveChannel(Mmu mmu)
        {
            _mmu = mmu;
        }

        public void TriggerLength()
        {
            // grab wave registers
            int nr31 = _mmu.LoadUnsigned8(Mmu.NR31_REGISTER);
            int nr33 = _mmu.LoadUnsigned8(Mmu.NR33_REGISTER);
            int nr34 = _mmu.LoadUnsigned8(Mmu.NR34_REGISTER);


            // Debug.WriteLine($"Length Enable: {BitUtils.GetBit((byte)nr4, 6)}");
            byte triggerVal = BitUtils.GetBit((byte)nr34, 7);
            byte lengthEnable = BitUtils.GetBit((byte)nr34, 6);

            if (triggerVal == 1 && !Enabled)
            {
                Enabled = true;
                var low = nr33;
                var high = nr34 & (0b0000_0111) << 8;
                TimerPeriod = 2048 - (high | low);
                Timer = TimerPeriod;
                Length = 256 - (nr31 & 0b11111111);
                WaveTablePointer = 0;
            }

            Length--;
            if (Length == 0)
            {
                Length = 256;
                Enabled = false;
            }


            // Debug.WriteLine($"Channel {channel} enable: {Enabled}");
        }

        public void SetVolume()
        {
            // grab square registers
            int nr32 = _mmu.LoadUnsigned8(Mmu.NR32_REGISTER);
            var volCode = (short)((nr32 & 0b01100000) >> 5);
            if (volCode == 0)
            {
                VolumeShift = 4;
            }
            else if (volCode == 1)
            {
                VolumeShift = 0;
            }
            else if (volCode == 2)
            {
                VolumeShift = 1;
            }
            else
            {
                VolumeShift = 2;
            }
        }

        public void Tick(int cpuCycles)
        {
            AccumClock += cpuCycles;
            while (AccumClock >= 1)
            {
                AccumClock -= 1;

                // load wave table
                int j = 0;
                for (int i = 0; i < waveTable.Length; i++)
                {
                    // get the current sample
                    var samples = _mmu.LoadUnsigned8(WaveTableStartAddr + j);

                    if (i % 2 == 0)
                    {
                        // get high nibble for sample
                        waveTable[i] = (byte)((samples & 0b11110000) >> 4);
                    }
                    else
                    {
                        // get low nibble for sample
                        waveTable[i] = (byte)(samples & 0b00001111);
                        j++;
                    }
                }

                // update the sample point
                Output = waveTable[WaveTablePointer % 32];
                WaveTablePointer++;
            }
        }

        public float Sample()
        {
            if (!Enabled) return 0;
            return ((Output >> VolumeShift)/100f * 100);
        }
    }
}