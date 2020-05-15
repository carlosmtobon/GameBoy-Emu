namespace GameBoy_Emu.core.mbc
{
    public class Mbc1
    {
        
        
        // special registers 
        public bool IsRamEnabled { get; set; }
        public byte RomBank { get; set; }
        public byte RamBank { get; set; }
        private byte _mode;
        
        public Mbc1()
        {
            RomBank = 1;
            RamBank = 0;
        }

        public void WriteRamg(byte val)
        {
            IsRamEnabled = ((val & 0xF) == 0xA);
        }
        
        public void WriteBank1(byte val)
        {
            val &= 0x1F;
            RomBank = val == 0 ? (byte) 1 : val;
        }

        public void WriteBank2(byte val)
        {
            RamBank = (byte)(val & 0x3);
        }

        public void WriteMode(byte val)
        {
            _mode = (byte) (val & 1);
            if (_mode == 0)
            {
                RamBank = 0;
            }
        }

        public int GetMode()
        {
            return _mode;
        }

        public void DoLoRom(byte value)
        {
            byte low5 = (byte)(value & 0x1F);
            RomBank &= 0xE0;
            RomBank |= low5;
            if (RomBank == 0) RomBank++;
        }

        public void DoHiRom(byte value)
        {
            RomBank &= 0x1F;

            // turn off the lower 5 bits of the data
            value &= 0xE0 ;
            RomBank |= value ;
            if (RomBank == 0) RomBank++;
        }
    }
}