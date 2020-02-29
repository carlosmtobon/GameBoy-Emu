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
            sbyte sb = -1;
            ushort us = 10;
            us += (ushort)sb;
            Console.WriteLine($"{us}");
            //cpu.Registers.SetAF(0xACDC);

            // Console.WriteLine(String.Format("{0:X}", cpu.Registers.FRegister));

        }
    }
}
