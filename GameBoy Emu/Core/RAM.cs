using System.IO;

namespace GameBoy_Emu.Core
{
    public class RAM
    {
        private const int RAM_SIZE = 65535;
        private const int RomOffset = 0x00;

        const int IE = 0xFFFF;
        const int IF = 0xFF0F; 


        public byte[] Memory { get; }

        public RAM()
        {
            Memory = new byte[RAM_SIZE];
        }

        public void LoadRom(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(Memory, RomOffset, Memory.Length - RomOffset);
            }
        }
        public void StoreU8Bits(int addr, byte value)
        {
            Memory[addr] = value;
        }
        public void StoreU16Bits(int addr, ushort value)
        {
            Memory[addr] = (byte)(value >> 8);
            Memory[addr + 1] = (byte)(value & 0x00FF);
        }

        public byte LoadU8Bits(int addr)
        {
            return Memory[addr];
        }

        public sbyte LoadI8Bits(int addr)
        {
            return unchecked((sbyte)Memory[addr]);
        }

        public ushort LoadU16Bits(int addr)
        {
            return (ushort)(Memory[addr] << 8 | Memory[addr + 1]);
        }

    }
}