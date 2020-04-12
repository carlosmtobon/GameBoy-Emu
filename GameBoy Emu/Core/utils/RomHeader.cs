namespace GameBoy_Emu.core.utils
{
    public class RomHeader
    {
        public int RomType { get; }
        public int RomSize { get; }
        
        public int RamSize { get; }

        public RomHeader(byte romType, byte romSize, byte ramSize)
        {
            RomType = romType;
            RomSize = SetRomSize(romSize);
            RamSize = SetRamSize(ramSize);
        }

        private int SetRomSize(byte val)
        {
            switch (val)
            {
                case 0x1:
                    return 64;
                case 0x2:
                    return 128;
                case 0x3:
                    return 256;
                case 0x4:
                    return 512;
                case 0x5:
                    return 1024;
                case 0x6:
                    return 2048;
            }

            return 0;
        }
        
        private int SetRamSize(byte val)
        {
            switch (val)
            {
                case 0x1:
                    return 2;
                case 0x2:
                    return 8;
                case 0x3:
                    return 32;
                case 0x4:
                    return 128;
                case 0x5:
                    return 64;
            }

            return 0;
        }
    }
}