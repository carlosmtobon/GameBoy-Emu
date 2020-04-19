using System;

namespace GameBoy_Emu.core.ppu
{
    public class Display
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Scale { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int[] Pixels { get; set; }

        public bool Draw;

        public Display(int width, int height, int scale)
        {
            Width = width;
            Height = height;
            Scale = scale;
            InitScreen();
        }

        public void InitScreen()
        {
            Pixels = new int[Width * Height];
        }

        public void Add(int color)
        {
            if (X < Width && Y < Height)
            {
                Pixels[Y * Width + X] = color;
            }
            X++;
        }
    }
}
