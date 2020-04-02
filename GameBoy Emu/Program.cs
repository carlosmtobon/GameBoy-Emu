using ChichoGB.Core;
using ChichoGB.Core.CPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SDL2;
using System.Threading;
using GameBoy_Emu.core.ppu;
using System.Runtime.InteropServices;

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

            //IntPtr window = SDL.SDL_CreateWindow("Chicho's Gameboy Emulator", 200, 200, display.Width*4, display.Height*4, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            //IntPtr renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

            //SDL.SDL_Event sdlEvent;
            bool running = true;
            //SDL.SDL_Rect rect = new SDL.SDL_Rect();
            //rect.h = 10 * display.Scale;
            //rect.w = 10 * display.Scale;
            while (running)
            {
                cpu.Tick();
                //ppu.Tick(cpu.CpuTickCycles);

                //while (SDL.SDL_PollEvent(out sdlEvent) != 0)
                //{
                //    if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
                //    {
                //        running = false;
                //    }
                //}

                //SDL.SDL_RenderClear(renderer);

                //if (display.Draw)
                //{

                //    display.Draw = false;
                //    for (int row = 0; row < ppu.LcdScreen.Height; row++)
                //    {
                //        for (int col = 0; col < ppu.LcdScreen.Width; col++)
                //        {
                //            rect.x = col * display.Scale;
                //            rect.y = row * display.Scale;

                //            SDL.SDL_SetRenderDrawColor(renderer, (byte)(ppu.LcdScreen.pixels[row][col] == 1 ? 255 : 0), 255, 255, 255);
                //            SDL.SDL_RenderFillRect(renderer, ref rect);
                //        }
                //    }

                //    SDL.SDL_RenderPresent(renderer);
                //}

                // Console.WriteLine(String.Format("AF: {0:X}\nBC: {1:X}\nDE: {2:X}\nHL: {3:X}\nSP: {4:X}\nPC: {5:X}\n", cpu.Registers.GetAF(), cpu.Registers.GetBC(), cpu.Registers.GetDE(), cpu.Registers.GetHL(), cpu.SP, cpu.PC));

                var sc = ram.LoadUnsigned8(0xff02);
                if (sc == 0x81)
                {
                    Console.Write((char)ram.LoadUnsigned8(0xff01));
                    ram.StoreUnsigned8(0xff02, 0);

                }
            }
        }


        private static Mmu LoadRom()
        {
            var ram = new Mmu();
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\Tetris.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\DMG_ROM.bin");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\cpu_instrs.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\01-special.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\02-interrupts.gb");
           //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\03-op sp,hl.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\04-op r,imm.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\05-op rp.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\06-ld r,r.gb");
            ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\07-jr,jp,call,ret,rst.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\08-misc instrs.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\09-op r,r.gb");
            // ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\10-bit ops.gb");
            //ram.LoadBios(@"C:\Users\Carlos\Desktop\gbtest\11-op a,(hl).gb");
            return ram;
        }
    }
}
