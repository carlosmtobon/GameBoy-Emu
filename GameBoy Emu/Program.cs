using System;
using System.Diagnostics;
using GameBoy_Emu.core.apu;
using GameBoy_Emu.core.cpu;
using GameBoy_Emu.core.input;
using GameBoy_Emu.core.mmu;
using GameBoy_Emu.core.ppu;
using SDL2;

namespace GameBoy_Emu
{
    static class Program
    {
        static void Main()
        {
            InputDevice inputDevice = InputDevice.GetInstance();
            Mmu mmu = new Mmu(inputDevice);

            mmu.LoadRom(@"C:\Users\Carlos\Desktop\emulator-stuff\roms\gb\Legend of Zelda, The - Link's Awakening.gb");
            // mmu.LoadRom(@"C:\Users\Carlos\Desktop\emulator-stuff\roms\gb\Castlevania - The Adventure (USA).gb");
            // mmu.LoadRom(@"C:\Users\Carlos\Desktop\emulator-stuff\roms\gb\Super Mario Land (World).gb");

            Display display;
#if DEBUG
             display = new Display(160, 144, 4, mmu.GetRomName());
#else
              display = new Display(160, 144, 8, mmu.GetRomName());
#endif
            
            var cpu = new Cpu(mmu);
            var ppu = new Ppu(mmu, display);
            var apu = new Apu(mmu);


            bool running = true;
            var freq = Cpu.CYCLES_PER_SECOND / 59.7;
            var timePerCycle = 1000.0 / Cpu.CYCLES_PER_SECOND;
            var accumTime = 0.0;


            float deltaTime = 0.0f;
            uint lastTime = SDL.SDL_GetTicks();
            while (running)
            {
                var currentTime = SDL.SDL_GetTicks();

                deltaTime = currentTime - lastTime;
                lastTime = currentTime;

                accumTime += deltaTime;
                while (accumTime >= timePerCycle)
                {
                    cpu.Tick();
                    ppu.Tick(cpu.CpuTickCycles);
                    running = inputDevice.HandleInput();
                    display.UpdateDisplay();
                    apu.Tick(cpu.CpuTickCycles);
                    accumTime -= timePerCycle;
                }
            }

            // clean up
            mmu.SaveGameFile();
            display.Dispose();
            apu.Dispose();
            inputDevice.Dispose();
            SDL.SDL_Quit();
        }
    }
}