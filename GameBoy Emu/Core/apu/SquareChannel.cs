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
        
        public bool LengthEnabled { get; set; } = false;
        
        public int NR13 { get; set; } = 0;
        public int NR14 { get; set; } = 0;
        public bool Trigger { get; set; } = false;
        public void TriggerLength(int channel)
        {
            if (!LengthEnabled) return;
            
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
                AccumClock -=1 ;
                Timer--;
                if (Timer <= 0)
                {
                    var dutySequence = waveDutyTable[CurrentDuty];
                    Output = dutySequence[DutyCounter % 8];
                    DutyCounter++;
                    Timer = 2048 - TimerPeriod;  // Reload the timer
                }
            }

        }

        public float Sample()
        {
            if (!Enabled) return 0;
            return (Output * Volume);
        }

        public void SetVolume(short value)
        {
            Volume = value;
        }
    }
}