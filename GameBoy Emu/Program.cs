using System;
using System.Diagnostics;
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

            IntPtr renderer;
            SDL.SDL_Rect rect;
            InitSDL(display, out renderer, out rect);

            bool running = true;
            while (running)
            {
                cpu.Tick();
                ppu.Tick(cpu.CpuTickCycles);
                HandleInput(joypad);
                UpdateDisplay(ram, display, ppu, renderer, ref rect, ref running);
                var sc = ram.LoadUnsigned8(0xff02);
                if (sc == 0x81)
                {
                    Debug.Write((char) ram.LoadUnsigned8(0xff01));
                    ram.StoreUnsigned8(0xff02, 0);
                }
                
            }
        }

        private static void InitSDL(Display display, out IntPtr renderer, out SDL.SDL_Rect rect)
        {
            IntPtr window = SDL.SDL_CreateWindow("Chicho's Gameboy Emulator", 200, 200, display.Width * 4, display.Height * 4, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            rect = new SDL.SDL_Rect();
            rect.h = 10 * display.Scale;
            rect.w = 10 * display.Scale;
        }

        private static void UpdateDisplay(Mmu ram, Display display, Ppu ppu, IntPtr renderer, ref SDL.SDL_Rect rect, ref bool running)
        {
            SDL.SDL_RenderClear(renderer);

            if (display.Draw)
            {
                display.Draw = false;
                for (int row = 0; row < ppu.Display.Height; row++)
                {
                    for (int col = 0; col < ppu.Display.Width; col++)
                    {
                        rect.x = col * display.Scale;
                        rect.y = row * display.Scale;

                        if (ppu.Display.Pixels[row][col] == 1)
                        {
                            SDL.SDL_SetRenderDrawColor(renderer, 100, 100, 100, 255);
                        }
                        else if (ppu.Display.Pixels[row][col] == 2)
                        {
                            SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
                        }
                        else if (ppu.Display.Pixels[row][col] == 3)
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
                SDL.SDL_Delay(16);
            }
        }

        private static void HandleInput(Joypad joypad)
        {
            SDL.SDL_Event sdlEvent;
            while (SDL.SDL_PollEvent(out sdlEvent) != 0)
            {
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
        }

        private static Mmu LoadRom()
        {
            var ram = new Mmu();
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\testGame.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\07-jr,jp,call,ret,rst.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\08-misc instrs.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\03-op sp,hl.gb");

            //passed
            ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\Bubble Ghost (USA, Europe).gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\Super Mario Land (World).gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\Dr. Mario.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\Tetris.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\DMG_ROM.bin");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\cpu_instrs.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\01-special.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\02-interrupts.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\04-op r,imm.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\05-op rp.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\06-ld r,r.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\09-op r,r.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\10-bit ops.gb");
           // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\11-op a,(hl).gb");
            return ram;
        }
    }
}
