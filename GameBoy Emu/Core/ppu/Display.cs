using SDL2;
using System;
using System.Windows;

namespace GameBoy_Emu.core.ppu
{
    public class Display
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Scale { get; set; }
        public int CurrentX { get; set; }
        public int CurrentY { get; set; }
        public int[] Pixels { get; set; }

        public bool Draw;

        // SDL related 
        private IntPtr _window;
        private IntPtr _renderer;
        private SDL.SDL_Rect _rect;
        private object v;

        public Display(int width, int height, int scale, string romName)
        {
            Width = width;
            Height = height;
            Scale = scale;
            InitScreen();
            InitSDL(romName);
        }

        public void InitScreen()
        {
            Pixels = new int[Width * Height];
        }

        private void InitSDL(string romName)
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("Failed to Init SDL");
                return;
            }

            _window = SDL.SDL_CreateWindow("ChiBoy v1.0 - " + romName , SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, Width * Scale,
                Height * Scale, SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN);

            _renderer = SDL.SDL_CreateRenderer(_window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            _rect = new SDL.SDL_Rect();
            _rect.h = Scale;
            _rect.w = Scale;

            SDL.SDL_EventState(SDL.SDL_EventType.SDL_MOUSEMOTION, SDL.SDL_IGNORE);
            SDL.SDL_EventState(SDL.SDL_EventType.SDL_MOUSEWHEEL, SDL.SDL_IGNORE);
            SDL.SDL_EventState(SDL.SDL_EventType.SDL_TEXTINPUT, SDL.SDL_IGNORE);
            SDL.SDL_EventState(SDL.SDL_EventType.SDL_FINGERMOTION, SDL.SDL_IGNORE);
            SDL.SDL_EventState(SDL.SDL_EventType.SDL_FINGERUP, SDL.SDL_IGNORE);
            SDL.SDL_EventState(SDL.SDL_EventType.SDL_FINGERDOWN, SDL.SDL_IGNORE);
        }

        private IntPtr GetWinHandler(IntPtr window)
        {
            SDL.SDL_SysWMinfo infoWindow = new SDL.SDL_SysWMinfo();
            SDL.SDL_VERSION(out infoWindow.version);
            SDL.SDL_bool sDL_bool = SDL.SDL_GetWindowWMInfo(window, ref infoWindow);
            _ = sDL_bool;
            return infoWindow.info.win.window;
        }

        public void UpdateDisplay()
        {
            if (Draw)
            {
                //SDL.SDL_SetRenderDrawColor(_renderer, 255, 255, 255, 255);
                //SDL.SDL_RenderClear(_renderer);
                Draw = false;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        _rect.x = x * Scale;
                        _rect.y = y * Scale;

                        int color = Pixels[y * Width + x];

                        if (color == 3)
                            SDL.SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 255);
                        else if (color == 0)
                            SDL.SDL_SetRenderDrawColor(_renderer, 255, 255, 255, 255);
                        else if (color == 1)
                            SDL.SDL_SetRenderDrawColor(_renderer, 211, 211, 211, 255);
                        else 
                            SDL.SDL_SetRenderDrawColor(_renderer, 100, 100, 100, 255);
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
            _renderer = IntPtr.Zero;
            _window = IntPtr.Zero;
        }

        public void Add(int color)
        {
            if (CurrentX < Width && CurrentY < Height)
            {
                Pixels[CurrentY * Width + CurrentX] = color;
            }

            CurrentX++;
        }
    }
}