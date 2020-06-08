
namespace GameBoy_Emu.core.mbc
{

    public class Mbc1 : Mbc
    {
        public Mbc1()
        {
            RomBank = 1;
            mmuBank = 0;
        }

        public override void Writemmug(byte val)
        {
            IsmmuEnabled = ((val & 0xF) == 0xA);
        }

        public override void WriteBank1(byte val)
        {
            val &= 0x1F;
            RomBank = val == 0 ? (byte)1 : val;
        }

        public override void WriteBank2(byte val)
        {
            mmuBank = (byte)(val & 0x3);
        }

        public override void WriteMode(byte val)
        {
            Mode = (byte)(val & 1);
            if (Mode == 0)
            {
                mmuBank = 0;
            }
        }

        public override int GetMode()
        {
            return Mode;
        }

        public override void DoLoRom(byte value)
        {
            byte low5 = (byte)(value & 0x1F);
            RomBank &= 0xE0;
            RomBank |= low5;
            if (RomBank == 0) RomBank++;
        }

        public override void DoHiRom(byte value)
        {
            RomBank &= 0x1F;

            // turn off the lower 5 bits of the data
            value &= 0xE0;
            RomBank |= value;
            if (RomBank == 0) RomBank++;
        }
    }
}