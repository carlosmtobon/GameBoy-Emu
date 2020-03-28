using GameBoy_Emu.core.ppu;
using System;
using System.Collections.Generic;

namespace ChichoGB.Core
{
    public class Fetcher
    {
        private int _currentBgTile;
        private int _fetchAccumalator;
        private BackgroundTileMap _backgroundTileMap;
        public List<PixelData> Pixels { get; set; }
        private Mmu _ram;
        private Tile _tile;
        public FetcherState State { get; set; }
        public const int FETCHER_FREQUENCY = 2;

        public enum FetcherState { READ_TILE_NUM, READ_DATA_0, READ_DATA_1, TRANSFER_READY };
        public Fetcher(Mmu ram)
        {
            _currentBgTile = BackgroundTileMap.BG_MAP_ADDRESS_1;
            _backgroundTileMap = new BackgroundTileMap(ram);
            Pixels = new List<PixelData>();
            _ram = ram;
            State = FetcherState.READ_TILE_NUM;
        }

        public void Fetch()
        {
            if (State == FetcherState.READ_TILE_NUM)
            {
                Console.WriteLine("Fetch: Tile Num");
                int tileNum = _backgroundTileMap.GetTileNumber(_currentBgTile);
                _tile = _backgroundTileMap.GetTile(tileNum);
                State = FetcherState.READ_DATA_0;
            }
            else if (State == FetcherState.READ_DATA_0)
            {
                Console.WriteLine("Fetch: Data0");
                State = FetcherState.READ_DATA_1;
            }
            else if (State == FetcherState.READ_DATA_1)
            {
                Console.WriteLine("Fetch: Data1");
                for (int i = 0; i < 8; i++)
                {
                    PixelData pixel = new PixelData();
                    pixel.ColorData = 3;
                    pixel.Type = PixelData.PixelType.BG;
                    Pixels.Add(pixel);
                }
                
                State = FetcherState.TRANSFER_READY;
                Console.WriteLine("Fetch: Idle");
            }
        }

        public void Process()
        {
            if (State != FetcherState.TRANSFER_READY)
            {
                _fetchAccumalator++;
                if (_fetchAccumalator >= FETCHER_FREQUENCY)
                {
                    _fetchAccumalator -= FETCHER_FREQUENCY;
                    Fetch();
                }
            }
        }
    }
}