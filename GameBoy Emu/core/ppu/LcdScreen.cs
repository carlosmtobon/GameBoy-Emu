using System;

namespace GameBoy_Emu.core.ppu
{
    public class LcdScreen
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Scale { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int[][] pixels;

        public bool Draw;

        public LcdScreen(int width, int height, int scale)
        {
            Width = width;
            Height = height;
            Scale = scale;
            InitScreen();
        }

        public void InitScreen()
        {
            pixels = new int[Height][];
            for (int row = 0; row < pixels.Length; row++)
            {
                pixels[row] = new int[Width];
            }
        }

        public void Add(int color)
        {
            if (X < Width && Y < Height)
            {
                pixels[Y][X] = color;
            }
            X++;
        }

        public void DisplayScreen()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    Console.Write(pixels[row][col] == 0 ? ' ' : '%');
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
