using System;
using System.Collections.Generic;

namespace ChichoGB.Core
{
    public class PixelFifo
    {
        public PixelFifoState State { get; set; }
        private Queue<PixelData> _pixels;
        private Mmu _ram;
        public int _fifoAccumalator;
        public const int FIFO_FREQUENCY = 1;

        public enum PixelFifoState { PUSHING, IDLE }
        public PixelFifo(Mmu ram)
        {
            _pixels = new Queue<PixelData>();
            State = PixelFifoState.IDLE;
            _ram = ram;
        }

        public int Process()
        {
            if (State != PixelFifoState.IDLE)
            {
                if (_pixels.Count <= 8)
                {
                    Console.WriteLine("FIFO: IDLE");
                    State = PixelFifoState.IDLE;
                    return 0;
                }
                else
                {
                    Push();
                    return 1;
                }
            }
            else
            {
                Idle();
                return 0;
            }
           
        }

        private void Idle()
        {
            if (_pixels.Count > 8)
            {
                State = PixelFifoState.PUSHING;
            }
        }

        public int pushTimes; 
        private void Push()
        {
            pushTimes++;
            Console.WriteLine("FIFO: Push Pixel");
            // push pixel to display
            _pixels.Dequeue();
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