using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.core.mbc
{
    public abstract class Mbc
    {
        // special registers 
        public  bool IsmmuEnabled { get; set; }
        public  byte RomBank { get; set; }
        public  byte mmuBank { get; set; }
        public  byte Mode { get; set; }

        public abstract void Writemmug(byte val);
        public abstract void WriteBank1(byte val);
        public abstract void WriteBank2(byte val);
        public abstract void WriteMode(byte val);
        public abstract int GetMode();
        public abstract void DoLoRom(byte value);
        public abstract void DoHiRom(byte value);

        public static Mbc GetInstance(int romType)
        {
            if (romType == 1 || romType == 2 || romType == 3)
            {
                return new Mbc1();
            }

            return null;
        }

    }
}
