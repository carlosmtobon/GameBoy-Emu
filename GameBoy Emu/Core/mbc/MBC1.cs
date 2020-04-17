namespace GameBoy_Emu.core.mbc
{
    public class Mbc1
    {
        
        
        // special registers 
        private byte _ramg;
        private byte _bank1;
        private byte _bank2;
        private byte _mode;
        
        public Mbc1()
        {
            _bank1 = 1;
        }

        public void WriteRamg(byte val)
        {
            _ramg = (byte) (val & 0xA);
        }
        
        public void WriteBank1(byte val)
        {
            val &= 0x1F;
            _bank1 = val == 0 ? (byte) 1 : val;
        }

        public void WriteBank2(byte val)
        {
            _bank2 = (byte)(val & 0x3);
        }

        public void WriteMode(byte val)
        {
            _mode = (byte) (val & 1);
        }

        public int GetRomBank()
        {
            return _bank2 | _bank1;
        }

        public int GetMode()
        {
            return _mode;
        }
    }
}