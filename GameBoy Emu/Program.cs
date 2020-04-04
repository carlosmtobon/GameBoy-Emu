using ChichoGB.Core;
using ChichoGB.Core.CPU;
using GameBoy_Emu.core.ppu;
using SDL2;
using System;
using System.IO;

namespace ChichoGB
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
            var display = new LcdScreen(160, 144, 4);
            var cpu = new Cpu(ram);
            var ppu = new Ppu(ram, display);

            IntPtr renderer;
            SDL.SDL_Event sdlEvent;
            SDL.SDL_Rect rect;
            InitSDL(display, out renderer, out rect);

            bool running = true;
            cpu.CpuCycles = 23440332;

            // var sw = new StreamWriter("emu.log");
            while (running)
            {
                // var opNow = ram.Memory[cpu.PC];
                //   sw.WriteLine(String.Format("Cy:{0}", cpu.CpuCycles));
                //  sw.WriteLine(String.Format("{0:X4}: {10:X2} A:{1:X2} B:{2:X2} C:{3:X2} D:{4:X2} E:{5:X2} F:{6:X2} H:{7:X2} L:{8:X2} SP:{9:X4} CY:{11}", cpu.PC, cpu.Registers.A, cpu.Registers.B, cpu.Registers.C, cpu.Registers.D, cpu.Registers.E, cpu.Registers.F, cpu.Registers.H, cpu.Registers.L, cpu.SP, opNow, cpu.CpuCycles));
                //if (cpu.PC == 0xcb44)
                //{
                //    sw.Flush();
                //    sw.Close();
                //    Console.WriteLine();
                //}
                cpu.Tick();
                ppu.Tick(cpu.CpuTickCycles);

                sdlEvent = UpdateDisplay(ram, display, ppu, renderer, ref rect, ref running);
            }
        }

        private static void InitSDL(LcdScreen display, out IntPtr renderer, out SDL.SDL_Rect rect)
        {
            IntPtr window = SDL.SDL_CreateWindow("Chicho's Gameboy Emulator", 200, 200, display.Width * 4, display.Height * 4, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            rect = new SDL.SDL_Rect();
            rect.h = 10 * display.Scale;
            rect.w = 10 * display.Scale;
        }

        private static SDL.SDL_Event UpdateDisplay(Mmu ram, LcdScreen display, Ppu ppu, IntPtr renderer, ref SDL.SDL_Rect rect, ref bool running)
        {
            SDL.SDL_Event sdlEvent;
            while (SDL.SDL_PollEvent(out sdlEvent) != 0)
            {
                if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    running = false;
                }
            }

            SDL.SDL_RenderClear(renderer);

            if (display.Draw)
            {

                display.Draw = false;
                for (int row = 0; row < ppu.LcdScreen.Height; row++)
                {
                    for (int col = 0; col < ppu.LcdScreen.Width; col++)
                    {
                        rect.x = col * display.Scale;
                        rect.y = row * display.Scale;

                        if (ppu.LcdScreen.pixels[row][col] == 1)
                        {
                            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                        }
                        else
                        {
                            SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
                        }

                        SDL.SDL_RenderFillRect(renderer, ref rect);
                    }
                }

                SDL.SDL_RenderPresent(renderer);
            }

            var sc = ram.LoadUnsigned8(0xff02);
            if (sc == 0x81)
            {
                Console.Write((char)ram.LoadUnsigned8(0xff01));
                ram.StoreUnsigned8(0xff02, 0);

            }

            return sdlEvent;
        }

        private static Mmu LoadRom()
        {
            var ram = new Mmu();
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\testGame.gb");
             //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\07-jr,jp,call,ret,rst.gb");
             //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\08-misc instrs.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\03-op sp,hl.gb");

            //passed
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\Tetris.gb");
            ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\DMG_ROM.bin");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\cpu_instrs.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\01-special.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\02-interrupts.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\04-op r,imm.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\05-op rp.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\06-ld r,r.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\09-op r,r.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\10-bit ops.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\11-op a,(hl).gb");
            return ram;
        }
    }
}
