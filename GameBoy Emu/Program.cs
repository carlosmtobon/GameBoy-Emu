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
            Joypad joypad = new Joypad();
            Mmu ram = new Mmu(joypad);

            //ram.LoadRom(@"C:\Users\Carlos\Desktop\gbtest\Legend of Zelda, The - Link's Awakening (USA, Europe) (Rev A).gb");
            //ram.LoadRom(@"C:\Users\Carlos\Desktop\gbtest\Castlevania - The Adventure (USA).gb");
            ram.LoadRom(@"C:\Users\Carlos\Desktop\gbtest\Super Mario Land (World).gb");
            //ram.LoadRom(@"C:\Users\Carlos\Desktop\gbtest\Bubble Ghost (USA, Europe).gb");
            //ram.LoadRom(@"C:\Users\Carlos\Desktop\gbtest\Dr. Mario.gb");
            //ram.LoadRom(@"C:\Users\Carlos\Desktop\gbtest\Tetris.gb");

            var display = new Display(160, 144, 4);
            var cpu = new Cpu(ram);
            var ppu = new Ppu(ram, display);

            bool running = true;
            while (running)
            {
                cpu.Tick();
                ppu.Tick(cpu.CpuTickCycles);
                running = joypad.HandleInput();
                display.UpdateDisplay();
            }

            // clean up
            display.Dispose();
        }
    }
}