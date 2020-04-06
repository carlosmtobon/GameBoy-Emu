using System;
using System.IO;

namespace ChichoGB.Core
{
    public class Mmu
    {
        private readonly byte[] logoBytes = { 0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B, 0x03,
            0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D, 0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00,
            0x0E, 0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99, 0xBB, 0xBB, 0x67, 0x63, 0x6E,
            0x0E, 0xEC, 0xCC, 0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E };

        private const int RAM_SIZE = 0x10000;
        private const int BIOS_OFFSET = 0;
        private const int ROM_OFFSET = 0x100;

        // special register address
        public const int IE_REGISTER = 0xFFFF;
        public const int IF_REGISTER = 0xFF0F;

        public const int DIV_REGISTER = 0xFF04;
        public const int TIMA_REGISTER = 0xFF05;
        public const int TMA_REGISTER = 0xFF06;
        public const int TAC_REGISTER = 0xFF07;

        public const int LCDC_REGISTER = 0xFF40;
        public const int STAT_REGISTER = 0xFF41;
        public const int SCY_REGISTER = 0xFF42;
        public const int SCX_REGISTER = 0xFF43;
        public const int LY_REGISTER = 0xFF44;
        public const int LCY_REGISTER = 0xFF45;
        public const int DMA_REGISTER = 0xFF46;
        public const int BGP_REGISTER = 0xFF47;
        public const int WY_REGISTER = 0xFF4A;
        public const int WX_REGISTER = 0xFF4B;

        public byte[] Memory { get; }

        public Mmu()
        {
            Memory = new byte[RAM_SIZE];
            BootupValues();
        }

        private void BootupValues()
        {
            Memory[0xFF05] = 0x00; // TIMA
            Memory[0xFF06] = 0x00; // TMA
            Memory[0xFF07] = 0x00; //TAC
            Memory[0xFF10] = 0x80; //NR10
            Memory[0xFF11] = 0xBF; //NR11
            Memory[0xFF12] = 0xF3; //NR12
            Memory[0xFF14] = 0xBF; //NR14
            Memory[0xFF16] = 0x3F; //NR21
            Memory[0xFF17] = 0x00; //NR22
            Memory[0xFF19] = 0xBF; //NR24
            Memory[0xFF1A] = 0x7F; //NR30
            Memory[0xFF1B] = 0xFF; //NR31
            Memory[0xFF1C] = 0x9F; //NR32
            Memory[0xFF1E] = 0xBF; //NR33
            Memory[0xFF20] = 0xFF; //NR41
            Memory[0xFF21] = 0x00; //NR42
            Memory[0xFF22] = 0x00; //NR43
            Memory[0xFF23] = 0xBF; //NR30
            Memory[0xFF24] = 0x77; //NR50
            Memory[0xFF25] = 0xF3; //NR51
            Memory[0xFF26] = 0xF1; //- GB, 0xF0 - SGB; NR52
            Memory[0xFF40] = 0x91; //LCDC
            Memory[0xFF42] = 0x00; //SCY
            Memory[0xFF43] = 0x00; //SCX
            Memory[0xFF45] = 0x00; //LYC
            Memory[0xFF47] = 0xFC; //BGP
            Memory[0xFF48] = 0xFF; //OBP0
            Memory[0xFF49] = 0xFF; //OBP1
            Memory[0xFF4A] = 0x00; //WY
            Memory[0xFF4B] = 0x00; //WX
            Memory[IE_REGISTER] = 0x00; //IE
        }

        private void LoadNintendoLogo()
        {
            for (int i = 0; i < logoBytes.Length; i++)
            {
                Memory[0x104 + i] = logoBytes[i];
            }
        }

        public void LoadBios(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(Memory, BIOS_OFFSET, Memory.Length - BIOS_OFFSET);
            }
            LoadNintendoLogo();
        }

        public void LoadRom(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(Memory, ROM_OFFSET, Memory.Length - ROM_OFFSET);
            }
        }
        public void StoreUnsigned8(int addr, byte value)
        {
            if (addr == DMA_REGISTER)
                Console.WriteLine("Dma Transfer");
                
            Memory[addr] = value;
        }
        public void StoreUnsigned16(int addr, ushort value)
        {
            Memory[addr] = (byte)(value >> 8);
            Memory[addr + 1] = (byte)(value & 0x00FF);
        }

        public byte LoadUnsigned8(int addr)
        {
            return Memory[addr];
        }

        public sbyte LoadSigned8(int addr)
        {
            return unchecked((sbyte)Memory[addr]);
        }

        public ushort LoadUnsigned16(int addr)
        {
            return (ushort)(Memory[addr] | Memory[addr + 1] << 8);
        }

        public byte LoadInterruptEnable()
        {
            return Memory[IE_REGISTER];
        }

        public byte LoadInterruptFlag()
        {
            return Memory[IF_REGISTER];
        }

        public byte LoadLy()
        {
            return Memory[LY_REGISTER];
        }

        public byte LoadStat()
        {
            return Memory[STAT_REGISTER];
        }

        public byte LoadLcdc()
        {
            return Memory[LCDC_REGISTER];
        }
    }
}