using GameBoy_Emu.core.mmu;
using System.Collections.Generic;

namespace GameBoy_Emu.core.ppu
{
    public class PixelFifo
    {
        public PixelFifoState State { get; set; }
        private Queue<PixelData> _pixels;
        private readonly Mmu _mmu;

        public enum PixelFifoState { PUSHING, IDLE }
        public PixelFifo(Mmu mmu)
        {
            _pixels = new Queue<PixelData>();
            State = PixelFifoState.IDLE;
            _mmu = mmu;
        }

        public int Process(Display display, int scx)
        {
            if (State != PixelFifoState.IDLE)
            {
                var pixelsToDiscard = scx % 8;
                if (pixelsToDiscard != 0 && display.X == 0)
                {
                    for (int i = 0; i < pixelsToDiscard; i++)
                    {
                        if (_pixels.Count > 0)
                        {
                            _pixels.Dequeue();
                        }
                    }
                }
                if (_pixels.Count <= 8)
                {
                    //Console.WriteLine("FIFO: IDLE");
                    State = PixelFifoState.IDLE;
                    return 0;
                }

                Push(display);
                return 1;
            }

            Idle();
            return 0;
        }

        public void Mix(Queue<PixelData> spriteData, byte priority)
        {
            var temp = _pixels;
            var newData = new Queue<PixelData>();
            foreach (var pixel in spriteData)
            {
                PixelData current = null;
                if (temp.Count > 0)
                {
                    current = temp.Dequeue();
                    if (pixel.ColorData != 0)
                    {
                        if (current.Type == PixelData.PixelType.BG || current.Type == PixelData.PixelType.WINDOW)
                        {
                            if (current.ColorData != 0)
                            {
                                if (priority == 0)
                                {
                                    newData.Enqueue(pixel);
                                }
                                else
                                {
                                    newData.Enqueue(current);
                                }
                            }
                            else
                            {
                                newData.Enqueue(pixel);
                            }
                        }
                        else
                        {
                            newData.Enqueue(current);
                        }
                    }
                    else
                    {
                        newData.Enqueue(current);
                    }
                }
                else
                {
                    newData.Enqueue(pixel);
                }
            }
            foreach (var pixel in temp)
            {
                newData.Enqueue(pixel);
            }

            _pixels = newData;
        }

        public int ProcessSprite(Display display)
        {
            if (_pixels.Count > 0)
            {
                Push(display);
                return 1;
            }
            else
            {
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
            byte palette;
            byte color;

            if (pix.Type == PixelData.PixelType.SPRITE_0)
            {
                palette = _mmu.LoadUnsigned8(Mmu.OBP0_REGISTER);
            }
            else if (pix.Type == PixelData.PixelType.SPRITE_1)
            {
                palette = _mmu.LoadUnsigned8(Mmu.OBP1_REGISTER);
            }
            else
            {
                palette = _mmu.LoadUnsigned8(Mmu.BGP_REGISTER);
            }

            var current = pix.ColorData;
            if (current == 0)
            {
                color = (byte)(palette & 3);
            }
            else if (current == 1)
            {
                color = (byte)((palette >> 2) & 3);
            }
            else if (current == 2)
            {
                color = (byte)((palette >> 4) & 3);
            }
            else
            {
                color = (byte)((palette >> 6) & 3);
            }

            display.Add(color);
        }

        public void LoadFifo(Fetcher fetcher)
        {
            foreach (var pixel in fetcher.Pixels)
            {
                _pixels.Enqueue(pixel);
            }
            fetcher.Pixels.Clear();
            fetcher.State = Fetcher.FetcherState.READ_TILE_NUM;
        }

        public void Reset()
        {
            _pixels.Clear();
        }
    }
}