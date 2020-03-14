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
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\cpu_instrs.gb");
            ram.LoadBios(@"C:\Users\Carlos\Desktop\05-op rp.gb");
            var cpu = new CPU(ram);
            var ppu = new PPU(ram);
            while (true)
            {
                cpu.ProcessOpcode();
               
                 Console.WriteLine(String.Format("AF: {0:X}\nBC: {1:X}\nDE: {2:X}\nHL: {3:X}\nSP: {4:X}\nPC: {5:X}\n", cpu.Registers.GetAF(), cpu.Registers.GetBC(), cpu.Registers.GetDE(), cpu.Registers.GetHL(), cpu.SP, cpu.PC));
                var sc = ram.LoadU8Bits(0xff02);
                if (sc == 0x81)
                {
                    Console.Write((char)ram.LoadU8Bits(0xff01));
                    ram.StoreU8Bits(0xff02, 0);

                }

                if (cpu.PC == 0xCb31)
                {
                    Console.WriteLine("Done");
                }
                ppu.Process();
            }
        }
    }
}
