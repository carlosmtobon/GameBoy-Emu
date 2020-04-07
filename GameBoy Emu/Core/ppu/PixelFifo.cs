﻿using System.Collections.Generic;

namespace GameBoy_Emu.core.ppu
{
    public class PixelFifo
    {
        public PixelFifoState State { get; set; }
        private readonly Queue<PixelData> _pixels;
        private int _fifoAccumalator;
        public const int FIFO_FREQUENCY = 1;

        public enum PixelFifoState { PUSHING, IDLE }
        public PixelFifo()
        {
            _pixels = new Queue<PixelData>();
            State = PixelFifoState.IDLE;
        }

        public int Process(Display display)
        {
            if (State != PixelFifoState.IDLE)
            {
                if (_pixels.Count <= 8)
                {
                    //Console.WriteLine("FIFO: IDLE");
                    State = PixelFifoState.IDLE;
                    return 0;
                }
                else
                {
                    Push(display);
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

        private void Push(Display display)
        {
            var pix = _pixels.Dequeue();
            display.Add(pix.ColorData);
        }

        public void LoadFifo(Fetcher fetcher)
        {
            foreach (var pixel in fetcher.Pixels)
            {
                _pixels.Enqueue(pixel);
            }
            fetcher.Pixels.Clear();
            fetcher.State = Fetcher.FetcherState.READ_TILE_NUM;
            State = PixelFifo.PixelFifoState.PUSHING;
        }

        public void Reset()
        {
            _pixels.Clear();
        }
    }
}