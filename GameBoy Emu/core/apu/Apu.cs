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

        // sample code
        const int SAMPLE_RATE = 44100;
        private const int SAMPLE_SIZE = 1024;

        // Timer
        private int frameSequenceClocks = 0;
        private int frame = 0;

        private int FRAME_SEQ_RATE = 8192;
        private int sampleClocks = 0;
        private static readonly int SAMPLE_CLOCK_RATE = 95; // Cpu.CYCLES_PER_SECOND/SAMPLE_RATE;

        // device ID
        private uint deviceId;

        // square 1
        private SquareChannel _square1Channel;

        // square 2
        private SquareChannel _square2Channel;

        private short[] mixerBuffer = new short[SAMPLE_SIZE];
        private int bufferByteCount = 0;


        // apu mixer
        SDL.SDL_AudioSpec mixer = new SDL.SDL_AudioSpec();
        SDL.SDL_AudioSpec have;

        public Apu(Mmu mmu)
        {
            _mmu = mmu;

            _square1Channel = new SquareChannel(mmu);
            _square2Channel = new SquareChannel(mmu);

            if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) != 0)
            {
                Debug.WriteLine("Failed to initialize SDL: {0}", SDL.SDL_GetError());
                return;
            }

            mixer.freq = SAMPLE_RATE; // number of samples per second
            mixer.format = SDL.AUDIO_S16SYS; // sample type (here: signed short i.e. 16 bit)
            mixer.channels = 1; // only one channel
            mixer.samples = SAMPLE_SIZE; // buffer-size
            mixer.callback = AudioCallback; // function SDL calls periodically to refill the buffer

            deviceId = SDL.SDL_OpenAudioDevice(null, 0, ref mixer, out have, 0);
            SDL.SDL_PauseAudioDevice(deviceId, 0); // start playing sound
        }

        public void Tick(int cpuCycles)
        {
            frameSequenceClocks += cpuCycles;
            // sampleClocks += cpuCycles;

            var nr52 = _mmu.LoadUnsigned8(Mmu.NR52_REGISTER); // sound on/off

            if (BitUtils.GetBit(nr52, 7) == 1)
            {
                // sounds is on
                
                // frame sequencer
                if (frameSequenceClocks >= FRAME_SEQ_RATE) // every 8192 clock or 512 hz
                {
                    frameSequenceClocks -= FRAME_SEQ_RATE;
                    frame++;

                    if (frame % 2 == 0)
                    {
                        _square1Channel.Trigger(1); 
                        _square2Channel.Trigger(2);
                    }
                }

                _square1Channel.Tick(cpuCycles);
                _square2Channel.Tick(cpuCycles);

                if (sampleClocks++ > 95)
                {
                    sampleClocks = 0;
                    if (bufferByteCount < SAMPLE_SIZE)
                    {
                        mixerBuffer[bufferByteCount] = (short)(500 * _square1Channel.Sample() + _square2Channel.Sample());
                        bufferByteCount++;
                    }
                }
                // short output = 0;
                // // // test code
                // if (_square1Channel.Enabled)
                // {
                //     _square1Channel.Tick(cpuCycles);
                //     if (sampleClocks >= SAMPLE_CLOCK_RATE)
                //     {
                //         sampleClocks -= SAMPLE_CLOCK_RATE;
                //         output = _square1Channel.Sample();
                //     }
                //
                //     if (bufferByteCount < SAMPLE_SIZE)
                //     {
                //         mixerBuffer[bufferByteCount] = output;
                //         bufferByteCount++;
                //     }
                // }
            }
        }

        public void Dispose()
        {
            SDL.SDL_CloseAudioDevice(deviceId);
        }

        void AudioCallback(IntPtr userdata, IntPtr rawBuffer, int bytes)
        {
            if (bufferByteCount == SAMPLE_SIZE)
            {
                bufferByteCount = 0;
                Marshal.Copy(mixerBuffer, 0, rawBuffer, mixerBuffer.Length);
            }
        }

        //
        // // Sample code to test sounds
        // void AudioCallback(IntPtr userdata, IntPtr rawBuffer, int bytes)
        // {
        //     //var sd = Marshal.PtrToStructure<SoundData>(userdata);
        //     short[] buffer = new short[bytes / 2];
        //     int length = bytes / 2;
        //     double amp = 3000.0;
        //     double sampleNr = 0;
        //     double freq = 441;
        //
        //     for (int i = 0; i < length; i++, sampleNr++)
        //     {
        //         double time = sampleNr / SAMPLE_RATE;
        //         buffer[i] = (short)(amp * Math.Sin(2.0 * Math.PI * freq * time)); // render 441 HZ sine wave
        //     }
        //
        //     Marshal.Copy(buffer, 0, rawBuffer, length);
        // }
    }
}