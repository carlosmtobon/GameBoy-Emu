using System;
using System.Collections.Generic;

namespace ChichoGB.Core
{
    public class PixelFifo
    {
        public PixelFifoState State { get; set; }
        private Queue<PixelData> _pixels;
        private Mmu _ram;
        private int _fifoAccumalator;
        public const int FIFO_FREQUENCY = 1;

        public enum PixelFifoState { PUSHING, IDLE }
        public PixelFifo(Mmu ram)
        {
            _pixels = new Queue<PixelData>();
            State = PixelFifoState.IDLE;
            _ram = ram;
        }

        public void Process(int cpuCycles)
        {
            _fifoAccumalator += cpuCycles;
            if (_fifoAccumalator >= FIFO_FREQUENCY)
            {
                _fifoAccumalator -= FIFO_FREQUENCY;
                if (State != PixelFifoState.IDLE)
                {
                    Push();
                }
                else
                {
                    Idle();
                }
            }
        }

        private void Idle()
        {
            if (_pixels.Count > 8)
            {
                State = PixelFifoState.PUSHING;
            }
        }

        private void Push()
        {
            if (_pixels.Count <= 8)
            {
                Console.WriteLine("FIFO: IDLE");
                State = PixelFifoState.IDLE;
            }
            else
            {
                Console.WriteLine("FIFO: Push Pixel");
                // push pixel to display
                _pixels.Dequeue();
            }
        }

        public void LoadFifo(Fetcher fetcher)
        { 
            foreach (var pixel in fetcher.Pixels)
            {
                _pixels.Enqueue(pixel);
            }
            fetcher.Pixels.Clear();
        }
    }
}