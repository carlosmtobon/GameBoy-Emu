using System;
using SDL2;

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

        // SDL related 
        private IntPtr _window;
        private IntPtr _renderer;
        private SDL.SDL_Rect _rect;
        public Display(int width, int height, int scale)
        {
            Width = width;
            Height = height;
            Scale = scale;
            InitScreen();
            InitSDL();
        }

        public void InitScreen()
        {
            Pixels = new int[Width * Height];
        }

        private void InitSDL()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("Failed to Init SDL");
                return;
            }

            _window = SDL.SDL_CreateWindow("Chicho's Gameboy Emulator", 100, 100, Width * Scale,
                Height * Scale, SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);

            _renderer = SDL.SDL_CreateRenderer(_window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

            _rect = new SDL.SDL_Rect();
            _rect.h = 10 * Scale;
            _rect.w = 10 * Scale;

            // disable VSYNC
            //SDL.SDL_GL_SetSwapInterval(1);
            //SDL.SDL_EventState(SDL.SDL_EventType.SDL_MOUSEMOTION, SDL.SDL_IGNORE);
            //SDL.SDL_EventState(SDL.SDL_EventType.SDL_MOUSEWHEEL, SDL.SDL_IGNORE);
            //SDL.SDL_EventState(SDL.SDL_EventType.SDL_TEXTINPUT, SDL.SDL_IGNORE);
            //SDL.SDL_EventState(SDL.SDL_EventType.SDL_FINGERMOTION, SDL.SDL_IGNORE);
            //SDL.SDL_EventState(SDL.SDL_EventType.SDL_FINGERUP, SDL.SDL_IGNORE);
            //SDL.SDL_EventState(SDL.SDL_EventType.SDL_FINGERDOWN, SDL.SDL_IGNORE);
            // SDL.SDL_EventState(SDL.SDL_EventType.SDL_KEYDOWN, SDL.SDL_IGNORE);
        }

        public void UpdateDisplay()
        {
            if (Draw)
            {
                SDL.SDL_RenderClear(_renderer);
                Draw = false;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        _rect.x = x * Scale;
                        _rect.y = y * Scale;

                        int color = Pixels[y * Width + x];
                        if (color == 1)
                        {
                            SDL.SDL_SetRenderDrawColor(_renderer, 100, 100, 100, 255);
                        }
                        else if (color == 2)
                        {
                            SDL.SDL_SetRenderDrawColor(_renderer, 255, 255, 255, 255);
                        }
                        else if (color == 3)
                        {
                            SDL.SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 255);
                        }
                        else
                        {
                            SDL.SDL_SetRenderDrawColor(_renderer, 211, 211, 211, 255);
                        }

                        SDL.SDL_RenderFillRect(_renderer, ref _rect);
                    }
                }

                SDL.SDL_RenderPresent(_renderer);
            }
        }

        public void Dispose()
        {
            SDL.SDL_DestroyRenderer(_renderer);
            SDL.SDL_DestroyWindow(_window);
            SDL.SDL_Quit();
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