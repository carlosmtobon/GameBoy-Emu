using System;
using System.IO;

namespace GameBoy_Emu.Core
{
    public class RAM
    {
        private byte[] logoBytes = { 0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B, 0x03,
            0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D, 0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00,
            0x0E, 0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99, 0xBB, 0xBB, 0x67, 0x63, 0x6E,
            0x0E, 0xEC, 0xCC, 0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E };
        private const int RAM_SIZE = 65536;
        private const int RomOffset = 0;

        const int IE = 0xFFFF;
        const int IF = 0xFF0F; 

        public byte[] Memory { get; }

        public RAM()
        {
            Memory = new byte[RAM_SIZE];
            //LoadNintendoLogo();
        }

        private void LoadNintendoLogo()
        {
            for (int i = 0; i < logoBytes.Length; i++)
            {
                Memory[RomOffset + i] = logoBytes[i]; 
            }
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
            return (ushort)(Memory[addr] | Memory[addr + 1] << 8);
        }

        public byte GetIE()
        {
            return Memory[IE];
        }

        public byte GetIF()
        {
            return Memory[IF];
        }
    }
}