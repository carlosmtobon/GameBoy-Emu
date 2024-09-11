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
        private const int SAMPLE_SIZE = 4096;

        // Timer
        private int frameSequenceClocks = 0;
        private int frame = 0;

        private int FRAME_SEQ_RATE = 8192;
        private int sampleClocks = 0;
        private static readonly int SAMPLE_CLOCK_RATE =  Cpu.CYCLES_PER_SECOND/SAMPLE_RATE;

        // device ID
        private uint deviceId;

        // square 1
        private SquareChannel _square1Channel;

        // square 2
        private SquareChannel _square2Channel;
        
        // wave channel
        private WaveChannel _waveChannel;

        
        // main buffer to send to audio stream
        private float[] mixerBuffer = new float[SAMPLE_SIZE];
        private int bufferByteCount = 0;

        
        // test counter
        private int sampleFuncCounter = 0;

        // apu mixer
        SDL.SDL_AudioSpec mixer = new SDL.SDL_AudioSpec();
        SDL.SDL_AudioSpec have;

        public Apu(Mmu mmu)
        {
            _mmu = mmu;
            
            // TODO implement a bus on program.cs
            _square1Channel = new SquareChannel(mmu);
            _square2Channel = new SquareChannel(mmu);
            _waveChannel = new WaveChannel(mmu);

            if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) != 0)
            {
                Debug.WriteLine("Failed to initialize SDL: {0}", SDL.SDL_GetError());
                return;
            }

            mixer.freq = SAMPLE_RATE; // number of samples per second
            mixer.format = SDL.AUDIO_F32; // sample type (here: signed short i.e. 16 bit)
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
                    
                    if (frame == 0 || frame == 2 || frame  == 4 || frame == 6)
                    {
                        _square1Channel.Trigger(1); 
                        _square2Channel.Trigger(2);
                        _waveChannel.Trigger();
                    }

                    if (frame == 7)
                    {
                        _square1Channel.SetVolume(1);
                        _square2Channel.SetVolume(2);
                        _waveChannel.SetVolume();
                        
                        frame = 0;
                    }
                    else
                    {
                        frame++;
                    }
                }

                _square1Channel.Tick(cpuCycles);
                _square2Channel.Tick(cpuCycles);
                _waveChannel.Tick(cpuCycles);

                sampleClocks += cpuCycles;
                if (sampleClocks >= SAMPLE_CLOCK_RATE)
                {
                    sampleFuncCounter++;
                    sampleClocks -= SAMPLE_CLOCK_RATE;
                    if (bufferByteCount < SAMPLE_SIZE)
                    {
                        // mixerBuffer[bufferByteCount] = (float)( _square1Channel.Sample() + _square2Channel.Sample() + _waveChannel.Sample());
                        mixerBuffer[bufferByteCount] = _square2Channel.Sample();
                        bufferByteCount++;
                    }
                }
            }
        }

        public int GetSampleCounter()
        {
            int val = sampleFuncCounter;
            sampleFuncCounter = 0;
            return val;
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