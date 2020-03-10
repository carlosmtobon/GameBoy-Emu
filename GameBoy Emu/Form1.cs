using GameBoy_Emu.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameBoy_Emu
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            var ram = new RAM();
           // ram.LoadRom(@"C:\Users\Carlos\Desktop\cpu_instrs.gb");
            ram.LoadRom(@"C:\Users\Carlos\Desktop\DMG_ROM.bin");
            var cpu = new CPU(ram);
            var ppu = new PPU(ram);

            while (true)
            {
                cpu.ProcessOpcode();
                ppu.Process();
            }
        }
    }
}
