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
        public int X { get; set; }
        public int Y { get; set; }
        public int[][] _screenData;

        public LcdScreen(int width, int height, int scale)
        {
            Width = width;
            Height = height;
            Scale = scale;
            InitScreen();
        }

        public void InitScreen()
        {
            _screenData = new int[Height][];
            for (int row = 0; row < _screenData.Length; row++)
            {
                _screenData[row] = new int[Width];
            }
        }
        
        public void Add(int color)
        {
            if (X < Width && Y < Height)
            {
                _screenData[Y][X] = color;
            }
            X++;
        }

        public void DisplayScreen()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    Console.Write(_screenData[row][col] == 0 ? ' ' : '%');
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
