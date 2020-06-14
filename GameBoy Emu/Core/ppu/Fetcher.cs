using GameBoy_Emu.core.mmu;
using System.Collections.Generic;

namespace GameBoy_Emu.core.ppu
{
    public class Fetcher
    {
        private int _currentBgTileAddress;
        private int _currentBgTile;
        private int _fetchAccumalator;
        private int _currentTileNumber;
        private readonly Display _display;
        private readonly BgTileMapManager _bgTileMap;
        private readonly Mmu _mmu;
        private readonly PixelFifo _tileFifo;

        public Queue<PixelData> Pixels { get; set; }
        private Tile _tile;

        public FetcherState State { get; set; }
        public const int FETCHER_FREQUENCY = 2;

        public enum FetcherState
        {
            READ_TILE_NUM,
            READ_DATA_0,
            READ_DATA_1,
            TRANSFER_READY
        };

        public Fetcher(Display display, Mmu mmu)
        {
            _currentBgTileAddress = BgTileMapManager.BG_MAP_ADDRESS_1;
            _bgTileMap = new BgTileMapManager(mmu);
            _tileFifo = new PixelFifo(mmu);
            _display = display;
            _mmu = mmu;
            Pixels = new Queue<PixelData>();
            State = FetcherState.READ_TILE_NUM;
        }

        public void Fetch()
        {
            if (State == FetcherState.READ_TILE_NUM)
            {
                var scx = (_mmu.LoadUnsigned8(Mmu.SCX_REGISTER) % 255) / 8;
                var scy = (_mmu.LoadUnsigned8(Mmu.SCY_REGISTER) % 255) / 8;

                _currentBgTileAddress = _bgTileMap.GetBgTileAddr() +
                                        (((((_display.Y) / 8) + scy) * 32) + ((_currentBgTile / 8) + scx));
                _currentBgTile += 8;
                _currentTileNumber = _bgTileMap.GetTileNumber(_currentBgTileAddress);

                State = FetcherState.READ_DATA_0;
            }
            else if (State == FetcherState.READ_DATA_0)
            {
                _tile = _bgTileMap.GetTile(_currentTileNumber);

                State = FetcherState.READ_DATA_1;
            }
            else if (State == FetcherState.READ_DATA_1)
            {
                Pixels = _tile.GetRowPixelData(_display.Y % 8, PixelData.PixelType.BG);

                State = FetcherState.TRANSFER_READY;
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

        public void Reset()
        {
            _tileFifo.Reset();
            State = FetcherState.READ_TILE_NUM;
            _currentBgTile = 0;
        }

        public void GetSprite(OamEntry sprite, int yPos)
        {
            Pixels.Clear();
            var height = _bgTileMap.GetSpriteHeight();
            _tile = _bgTileMap.GetSpriteTile(sprite.TileNumber, height);
            PixelData.PixelType pixelType = sprite.PaletteNumber() == 0 ? PixelData.PixelType.SPRITE_0 : PixelData.PixelType.SPRITE_1;
            Pixels = _tile.GetRowPixelData((_tile.TileData.Length - (sprite.YPos - yPos)) % (_tile.TileData.Length / 2), pixelType, sprite.IsXFlip());
        }

        public void Tick(int cpuCycles)
        {
            while (cpuCycles > 0)
            {
                var sprite = _bgTileMap.GetVisibleSprites().Find(oamEntry => oamEntry.XPos - 8 == _display.X);

                if (_bgTileMap.GetVisibleSprites().Remove(sprite))
                {
                    GetSprite(sprite, _display.Y);
                    _tileFifo.Mix(Pixels, sprite.GetPriority());
                    State = FetcherState.READ_DATA_0;
                }

                Process();
                cpuCycles -= _tileFifo.Process(_display, _mmu.LoadUnsigned8(Mmu.SCX_REGISTER));

                if (State == FetcherState.TRANSFER_READY && _tileFifo.State == PixelFifo.PixelFifoState.IDLE)
                {
                    _tileFifo.LoadFifo(this);
                }
            }
        }

        public void FindVisibleSprites()
        {
            _bgTileMap.FindVisibleSprites(_display.Y);
        }
    }
}