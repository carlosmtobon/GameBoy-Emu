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

            
            // Variables for FPS calculation
            int frameCount = 0;
            float deltaTime = 0.0f;
            uint lastFrameTime = SDL.SDL_GetTicks();
            uint fpsTimer = lastFrameTime;
            int fps = 0;
            while (running)
            {
                var start = SDL.SDL_GetTicks();
                cpu.Tick();
                ppu.Tick(cpu.CpuTickCycles);
                running = inputDevice.HandleInput();
                display.UpdateDisplay();
                apu.Tick(cpu.CpuTickCycles);
               
                // Calculate delta time and FPS
                uint currentFrameTime = SDL.SDL_GetTicks();
                deltaTime = (currentFrameTime - lastFrameTime) / 1000.0f; // Convert to seconds
                lastFrameTime = currentFrameTime;

                fps++;

                // Update FPS display every second
                if (currentFrameTime - fpsTimer >= 1000) {
                    // Debug.WriteLine($"APU: Samples per sec : {apu.GetSampleCounter()}");
                    Debug.WriteLine($"FPS: {fps}");
                    fps = 0;
                    fpsTimer = currentFrameTime;
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