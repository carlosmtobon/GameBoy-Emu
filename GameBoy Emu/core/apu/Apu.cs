using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GameBoy_Emu.core.cpu;
using GameBoy_Emu.core.mmu;
using GameBoy_Emu.core.utils;
using SDL2;

namespace GameBoy_Emu.core.apu
{

    class Apu
    {
        
        private readonly Mmu _mmu;
        private int timerClocks;
        
        // sample code
        const int SAMPLE_RATE = 44100;
        const double AMPLITUDE = 1000;
        
        // device ID
        private uint deviceId;
   

        public Apu(Mmu mmu)
        {
            _mmu = mmu;
        }

        public void Tick(int cpuCycles)
        {
            timerClocks += cpuCycles;
            if (timerClocks >= Cpu.CYCLES_PER_SECOND)
            {
                timerClocks -= Cpu.CYCLES_PER_SECOND;
                // Debug.WriteLine($"Square 1: NR10 {_mmu.LoadUnsigned8(Mmu.NR10_REGISTER)}");
                // Debug.WriteLine($"Square 1: NR11 {_mmu.LoadUnsigned8(Mmu.NR11_REGISTER)}");
                // Debug.WriteLine($"Square 1: NR12 {_mmu.LoadUnsigned8(Mmu.NR12_REGISTER)}");
                // Debug.WriteLine($"Square 1: NR13 {_mmu.LoadUnsigned8(Mmu.NR13_REGISTER)}");
                // Debug.WriteLine($"Square 1: NR14 {_mmu.LoadUnsigned8(Mmu.NR14_REGISTER)}");
                // Debug.WriteLine("-------------------------------------------------------------");
                //
                
                // grab square 1 registers
                short freq = (short)((_mmu.LoadUnsigned8(Mmu.NR14_REGISTER) & 63) << 5 | _mmu.LoadUnsigned8(Mmu.NR13_REGISTER));

                Debug.WriteLine($"Square 1 Freq: {freq}");
            }
            
        }
        
         public void Init()
        {

            // SoundData soundData = new SoundData();
            // GCHandle handle = GCHandle.Alloc(soundData, GCHandleType.Pinned);
            // IntPtr soundDataPtr = handle.AddrOfPinnedObject();
           
            SDL.SDL_AudioSpec want = new SDL.SDL_AudioSpec();
            SDL.SDL_AudioSpec have;
            
            if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) != 0)
            {
                Debug.WriteLine("Failed to initialize SDL: {0}", SDL.SDL_GetError());
                return;
            }


            want.freq = SAMPLE_RATE; // number of samples per second
            want.format = SDL.AUDIO_S16SYS; // sample type (here: signed short i.e. 16 bit)
            want.channels = 1; // only one channel
            want.samples = 1024; // buffer-size
            want.callback = AudioCallback; // function SDL calls periodically to refill the buffer

            deviceId = SDL.SDL_OpenAudioDevice(null, 0, ref want, out have, 0);

            SDL.SDL_PauseAudioDevice(deviceId, 0); // start playing sound
            SDL.SDL_Delay(3000); // wait while sound is playing
            SDL.SDL_PauseAudioDevice(deviceId, 1); // stop playing sound
          
        }

         public void Dispose()
         {
             
             SDL.SDL_CloseAudioDevice(deviceId);
         }

        // struct SoundData
        // {
        //     public double amp;
        //     public double time;
        //     public double freq;
        // }
        //
        void AudioCallback(IntPtr userdata, IntPtr rawBuffer, int bytes)
        {
            
            //var sd = Marshal.PtrToStructure<SoundData>(userdata);
            short[] buffer = new short[bytes / 2];
            int length = bytes / 2;
            double amp = 3000.0;
            double sampleNr = 0;
            double freq = 441;
            for (int i = 0; i < length; i++, sampleNr++)
            {
                double time = sampleNr / SAMPLE_RATE;
                buffer[i] = (short)(amp * Math.Sin(2.0 * Math.PI * freq * time)); // render 441 HZ sine wave
            }
            
            Marshal.Copy(buffer, 0, rawBuffer, length);
        }
    }
}
