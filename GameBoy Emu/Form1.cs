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
            var ram = new MMU();
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\cpu_instrs.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\01-special.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\02-interrupts.gb");
             ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\03-op sp,hl.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\04-op r,imm.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\05-op rp.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\06-ld r,r.gb");
           // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\07-jr,jp,call,ret,rst.gb");
             //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\08-misc instrs.gb");
           // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\09-op r,r.gb");
           //  ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\10-bit ops.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\11-op a,(hl).gb");

            var cpu = new CPU(ram);
            var ppu = new PPU(ram);
            while (true)
            {
                cpu.ProcessOpcode();
                //Console.WriteLine(String.Format("AF: {0:X}\nBC: {1:X}\nDE: {2:X}\nHL: {3:X}\nSP: {4:X}\nPC: {5:X}\n", cpu.Registers.GetAF(), cpu.Registers.GetBC(), cpu.Registers.GetDE(), cpu.Registers.GetHL(), cpu.SP, cpu.PC));
                var sc = ram.LoadU8Bits(0xff02);
                if (sc == 0x81)
                {
                    Console.Write((char)ram.LoadU8Bits(0xff01));
                    ram.StoreU8Bits(0xff02, 0);

                }

                //if (cpu.PC == 0xC018)
                //{
                //    Console.WriteLine("Done");
                //}
                ppu.Process();
            }
        }
    }
}
