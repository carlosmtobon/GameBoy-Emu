using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.core.ppu
{
    public class LcdScreen
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Scale { get; set; }



        public LcdScreen(int width, int height, int scale)
        {
            Width = width;
            Height = height;
            Scale = scale;
        }
    }
}
