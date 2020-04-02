using GameBoy_Emu.core.ppu;
using System;
using System.Collections.Generic;

namespace ChichoGB.Core
{
    public class Fetcher
    {
        private int _currentBgTileAddress;
        private int _currentBgTile;
        private int _fetchAccumalator;
        private BackgroundTileMap _bgTileMap;
        private Mmu _ram;
        public Queue<PixelData> Pixels { get; set; }
        private Tile _tile;

        public FetcherState State { get; set; }
        public const int FETCHER_FREQUENCY = 2;

        public enum FetcherState { READ_TILE_NUM, READ_DATA_0, READ_DATA_1, TRANSFER_READY };
        public Fetcher(BackgroundTileMap bgTileMap, Mmu ram)
        {
            _currentBgTileAddress = BackgroundTileMap.BG_MAP_ADDRESS_1;
            _bgTileMap = bgTileMap;
            _ram = ram;
            Pixels = new Queue<PixelData>();
            State = FetcherState.READ_TILE_NUM;
        }

        public void Fetch(LcdScreen lcdScreen)
        {
            if (State == FetcherState.READ_TILE_NUM)
            {
                var scx = _ram.LoadUnsigned8(Mmu.SCX_ADDRESS) % lcdScreen.Width;
                var scy = _ram.LoadUnsigned8(Mmu.SCY_ADDRESS) % lcdScreen.Height;


                _currentBgTileAddress = BackgroundTileMap.BG_MAP_ADDRESS_1 + (((((lcdScreen.Y) / 8) + scy) * 32) + ((_currentBgTile / 8) + scx));
                _currentBgTile += 8;

                if (lcdScreen.X > lcdScreen.Width)
                {
                    _currentBgTile = 0;
                }

                int tileNum = _bgTileMap.GetTileNumber(_currentBgTileAddress);

                _tile = _bgTileMap.GetTile(tileNum);
                State = FetcherState.READ_DATA_0;
            }
            else if (State == FetcherState.READ_DATA_0)
            {
                State = FetcherState.READ_DATA_1;
            }
            else if (State == FetcherState.READ_DATA_1)
            {
                Pixels = _tile.GetRowPixelData(lcdScreen.Y % 8);

                State = FetcherState.TRANSFER_READY;
            }
        }

        public void Process(LcdScreen lcdScreen)
        {
            if (State != FetcherState.TRANSFER_READY)
            {
                _fetchAccumalator++;
                if (_fetchAccumalator >= FETCHER_FREQUENCY)
                {
                    _fetchAccumalator -= FETCHER_FREQUENCY;
                    Fetch(lcdScreen);
                }
            }
        }

        public void Reset()
        {
            State = FetcherState.READ_TILE_NUM;
        }
    }
}