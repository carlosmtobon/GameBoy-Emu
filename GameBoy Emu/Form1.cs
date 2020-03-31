using ChichoGB.Core;
using ChichoGB.Core.CPU;
using GameBoy_Emu.core.ppu;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ChichoGB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            var ram = new Mmu();
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\Tetris.gb");
            ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\DMG_ROM.bin");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\cpu_instrs.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\01-special.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\02-interrupts.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\03-op sp,hl.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\04-op r,imm.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\05-op rp.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\06-ld r,r.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\07-jr,jp,call,ret,rst.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\08-misc instrs.gb");*
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\09-op r,r.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\10-bit ops.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\11-op a,(hl).gb");

            var cpu = new Cpu(ram);
            var ppu = new Ppu(ram);

            while (true)
            {
                cpu.Tick();
                ppu.Tick(cpu.CpuTickCycles);
               // UpdateScreen(ppu);

                // Console.WriteLine(String.Format("AF: {0:X}\nBC: {1:X}\nDE: {2:X}\nHL: {3:X}\nSP: {4:X}\nPC: {5:X}\n", cpu.Registers.GetAF(), cpu.Registers.GetBC(), cpu.Registers.GetDE(), cpu.Registers.GetHL(), cpu.SP, cpu.PC));
                var sc = ram.LoadUnsigned8(0xff02);
                if (sc == 0x81)
                {
                    Console.Write((char)ram.LoadUnsigned8(0xff01));
                    ram.StoreUnsigned8(0xff02, 0);

                }
            }
        }

    }
}
