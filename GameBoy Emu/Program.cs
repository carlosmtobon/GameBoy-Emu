using System;
using GameBoy_Emu.core.cpu;
using GameBoy_Emu.core.input;
using GameBoy_Emu.core.ppu;
using GameBoy_Emu.core.ram;
using SDL2;

namespace GameBoy_Emu
{
    static class Program
    {
        static void Main()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) < 0)
            {
                Console.WriteLine("Failed to Init SDL");
                return;
            }

            Mmu ram = LoadRom();
            var joypad = new Joypad(ram);
            var display = new Display(160, 144, 4);
            var cpu = new Cpu(ram);
            var ppu = new Ppu(ram, display);

            IntPtr window;
            IntPtr renderer;
            SDL.SDL_Rect rect;
            InitSDL(display, out window, out renderer, out rect);

            bool running = true;
            while (running)
            {
                cpu.Tick();
                ppu.Tick(cpu.CpuTickCycles);
                // running = HandleInput(joypad);
                UpdateDisplay(display, renderer, ref rect);
            }

            // clean up
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }

        private static void InitSDL(Display display, out IntPtr window, out IntPtr renderer, out SDL.SDL_Rect rect)
        {
            window = SDL.SDL_CreateWindow("Chicho's Gameboy Emulator", 100, 100, display.Width * display.Scale,
                display.Height * display.Scale, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

            rect = new SDL.SDL_Rect();
            rect.h = 10 * display.Scale;
            rect.w = 10 * display.Scale;
        }

        private static void UpdateDisplay(Display display, IntPtr renderer, ref SDL.SDL_Rect rect)
        {
            if (display.Draw)
            {
                SDL.SDL_RenderClear(renderer);
                display.Draw = false;
                for (int y = 0; y < display.Height; y++)
                {
                    for (int x = 0; x < display.Width; x++)
                    {
                        rect.x = x * display.Scale;
                        rect.y = y * display.Scale;

                        int color = display.Pixels[y * display.Width + x]; 
                        if (color == 1)
                        {
                            SDL.SDL_SetRenderDrawColor(renderer, 100, 100, 100, 255);
                        }
                        else if (color == 2)
                        {
                            SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
                        }
                        else if (color == 3)
                        {
                            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                        }
                        else
                        {
                            SDL.SDL_SetRenderDrawColor(renderer, 211, 211, 211, 255);
                        }

                        SDL.SDL_RenderFillRect(renderer, ref rect);
                    }
                }

                SDL.SDL_RenderPresent(renderer);
            }
        }

        private static bool HandleInput(Joypad joypad)
        {
            SDL.SDL_Event sdlEvent;
            while (SDL.SDL_PollEvent(out sdlEvent) != 0)
            {
                if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
                    return false;
                if (sdlEvent.type == SDL.SDL_EventType.SDL_KEYDOWN)
                {
                    switch (sdlEvent.key.keysym.sym)
                    {
                        case SDL.SDL_Keycode.SDLK_UP:
                            joypad.SetKeyPressed(Joypad.Input.UP);
                            break;
                        case SDL.SDL_Keycode.SDLK_DOWN:
                            joypad.SetKeyPressed(Joypad.Input.DOWN);
                            break;
                        case SDL.SDL_Keycode.SDLK_LEFT:
                            joypad.SetKeyPressed(Joypad.Input.LEFT);
                            break;
                        case SDL.SDL_Keycode.SDLK_RIGHT:
                            joypad.SetKeyPressed(Joypad.Input.RIGHT);
                            break;
                        case SDL.SDL_Keycode.SDLK_z:
                            joypad.SetKeyPressed(Joypad.Input.A);
                            break;
                        case SDL.SDL_Keycode.SDLK_x:
                            joypad.SetKeyPressed(Joypad.Input.B);
                            break;
                        case SDL.SDL_Keycode.SDLK_SPACE:
                            joypad.SetKeyPressed(Joypad.Input.SELECT);
                            break;
                        case SDL.SDL_Keycode.SDLK_RETURN:
                            joypad.SetKeyPressed(Joypad.Input.START);
                            break;
                    }
                }

                if (sdlEvent.type == SDL.SDL_EventType.SDL_KEYUP)
                {
                    switch (sdlEvent.key.keysym.sym)
                    {
                        case SDL.SDL_Keycode.SDLK_UP:
                            joypad.SetKeyReleased(Joypad.Input.UP);
                            break;
                        case SDL.SDL_Keycode.SDLK_DOWN:
                            joypad.SetKeyReleased(Joypad.Input.DOWN);
                            break;
                        case SDL.SDL_Keycode.SDLK_LEFT:
                            joypad.SetKeyReleased(Joypad.Input.LEFT);
                            break;
                        case SDL.SDL_Keycode.SDLK_RIGHT:
                            joypad.SetKeyReleased(Joypad.Input.RIGHT);
                            break;
                        case SDL.SDL_Keycode.SDLK_z:
                            joypad.SetKeyReleased(Joypad.Input.A);
                            break;
                        case SDL.SDL_Keycode.SDLK_x:
                            joypad.SetKeyReleased(Joypad.Input.B);
                            break;
                        case SDL.SDL_Keycode.SDLK_SPACE:
                            joypad.SetKeyReleased(Joypad.Input.SELECT);
                            break;
                        case SDL.SDL_Keycode.SDLK_RETURN:
                            joypad.SetKeyReleased(Joypad.Input.START);
                            break;
                    }
                }
            }

            return true;
        }

        private static Mmu LoadRom()
        {
            var ram = new Mmu();
            //passed
            // ram.LoadRom(@"C:\Users\Carlos\Desktop\gbtest\Castlevania - The Adventure (USA).gb");
            // ram.LoadRom(@"C:\Users\Carlos\Desktop\gbtest\Bubble Ghost (USA, Europe).gb");
             // ram.LoadRom(@"C:\Users\Carlos\Desktop\gbtest\Super Mario Land (World).gb");
            // ram.LoadRom(@"C:\Users\Carlos\Desktop\gbtest\Dr. Mario.gb");
            ram.LoadRom(@"C:\Users\Carlos\Desktop\gbtest\Tetris.gb");
            //ram.LoadRom(@"C:\Users\Carlos\Desktop\gbtest\cpu_instrs.gb");
           // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\DMG_ROM.bin");
            return ram;
        }
    }
}