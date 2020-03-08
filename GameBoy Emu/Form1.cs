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
            ram.LoadRom(@"C:\Users\Carlos\Desktop\cpu_instrs.gb");
            var cpu = new CPU(ram);

            cpu.Registers.A = 0x3b;
            byte e = 0x2a;
            byte hl = 0x4f;
            cpu.Registers.SetCYFLag(true);
            cpu.SBC(e, 1, 4);
            Console.WriteLine(string.Format("{0:X}", cpu.Registers.A));
            Console.WriteLine("Z: " + cpu.Registers.GetZFlag());
            Console.WriteLine("HCY: " +cpu.Registers.GetHCYFlag());
            Console.WriteLine("N: " + cpu.Registers.GetNFlag());
            Console.WriteLine("CY: " + cpu.Registers.GetCYFlag());
            Console.WriteLine("------------");
            cpu.Registers.A = 0x3b;
            cpu.Registers.SetCYFLag(true);
            cpu.SBC(0x3a, 1, 4);
            Console.WriteLine(string.Format("{0:X}",cpu.Registers.A));
            Console.WriteLine("Z: " + cpu.Registers.GetZFlag());
            Console.WriteLine("HCY: " + cpu.Registers.GetHCYFlag());
            Console.WriteLine("N: " + cpu.Registers.GetNFlag());
            Console.WriteLine("CY: " + cpu.Registers.GetCYFlag());
            Console.WriteLine("------------");
            cpu.Registers.A = 0x3b;
            cpu.Registers.SetCYFLag(true);
            cpu.SBC(hl, 1, 4);
            Console.WriteLine(string.Format("{0:X}", cpu.Registers.A));
            Console.WriteLine("Z: " + cpu.Registers.GetZFlag());
            Console.WriteLine("HCY: " + cpu.Registers.GetHCYFlag());
            Console.WriteLine("N: " + cpu.Registers.GetNFlag());
            Console.WriteLine("CY: " + cpu.Registers.GetCYFlag());
            Console.WriteLine("------------");
            //while (true)
            //{
            //    cpu.ProcessOpcode();
            //}

        }
    }
}
