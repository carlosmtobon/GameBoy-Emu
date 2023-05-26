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

            bool running = true;
            var freq = Cpu.CYCLES_PER_SECOND / 59.7;

            while (running)
            {
                var start = SDL.SDL_GetPerformanceCounter();
                cpu.Tick();
                ppu.Tick(cpu.CpuTickCycles);
                running = inputDevice.HandleInput();
                display.UpdateDisplay();
                //var end = SDL.SDL_GetPerformanceCounter();
                //float elapsedMS = (end - start) / (float)(SDL.SDL_GetPerformanceFrequency());
                // Debug.WriteLine("FPS: " + 1.0 / elapsedMS);
                //SDL.SDL_Delay((uint)Math.Floor(16.666f - elapsedMS));

            }

            // clean up
            mmu.SaveGameFile();
            display.Dispose();
            inputDevice.Dispose();
            SDL.SDL_Quit();
        }
    }
}