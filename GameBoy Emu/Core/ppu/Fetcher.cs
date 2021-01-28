using System;
using GameBoy_Emu.core.mmu;
using System.Collections.Generic;
using GameBoy_Emu.core.utils;

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
       
        bool window = false;
        private int winX = 0;
        private int winY = 0;
        public void Fetch()
        {
            var scx = _mmu.LoadUnsigned8(Mmu.SCX_REGISTER);
            var scy = _mmu.LoadUnsigned8(Mmu.SCY_REGISTER);

            var wx = _mmu.LoadUnsigned8(Mmu.WX_REGISTER);
            var wy = _mmu.LoadUnsigned8(Mmu.WY_REGISTER);

            
            if (wx-7 <= _display.CurrentX  && wy <= _display.CurrentY && BitUtils.IsBitSet(_mmu.LoadLcdc(), 5))
            {
                if (!window)
                {
                    window = true;
                    Pixels.Clear();
                    State = FetcherState.READ_TILE_NUM;
                    return;
                }
            }
            
            if (State == FetcherState.READ_TILE_NUM)
            {
                var addr = window ? _bgTileMap.GetWinTileAddr() + ((((_display.CurrentY - wy)   % 256) / 8) * 32) + ((winX  % 256) /8) : _bgTileMap.GetBgTileAddr()+ ((((_display.CurrentY + scy) % 256) / 8) * 32) +
                                                                   (((((_currentBgTile ) + scx ) % 256) / 8));
                _currentBgTileAddress = addr;
                 
                if (window)
                {
                    winX += 8;
                }
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
                Pixels = _tile.GetRowPixelData((_display.CurrentY + (window ? 0 : scy)) % 256 % 8, PixelData.PixelType.BG);

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
            winX = 0;
            winY = 0;
            window = false;
        }

        public void GetSprite(OamEntry sprite, int yPos)
        {
            var scy = _mmu.LoadUnsigned8(Mmu.SCY_REGISTER);
            Pixels.Clear();
            var height = _bgTileMap.GetSpriteHeight();
            _tile = _bgTileMap.GetSpriteTile(sprite.TileNumber, height);
            PixelData.PixelType pixelType = sprite.PaletteNumber() == 0 ? PixelData.PixelType.SPRITE_0 : PixelData.PixelType.SPRITE_1;
            int val = sprite.IsYFlip() ?  (height-1) -(((yPos - (sprite.YPos - 16)) % height)) : (yPos -(sprite.YPos - 16)) % height;
            Pixels = _tile.GetRowPixelData(val, pixelType, sprite.IsXFlip());
        }

        public void Tick(int cpuCycles)
        {
            while (cpuCycles > 0)
            {
                OamEntry sprite = null;
                var visibleSprites = _bgTileMap.GetVisibleSprites();
                foreach (OamEntry oamEntry in visibleSprites)
                {
                    if (oamEntry.XPos - 8 == _display.CurrentX && (oamEntry.XPos - 8) > 0)
                    {
                        sprite = oamEntry;
                        break;
                    }
                }

                if (sprite != null && visibleSprites.Remove(sprite))
                {
                    GetSprite(sprite, _display.CurrentY);
                    _tileFifo.Mix(Pixels, sprite.GetPriority());
                    State = FetcherState.READ_DATA_0;
                }

                Process();
                cpuCycles -= _tileFifo.Process(_display, window ? 0 :_mmu.LoadUnsigned8(Mmu.SCX_REGISTER));

                if (State == FetcherState.TRANSFER_READY && _tileFifo.State == PixelFifo.PixelFifoState.IDLE)
                {
                    _tileFifo.LoadFifo(this);
                }
            }
        }

        public void FindVisibleSprites()
        {
            _bgTileMap.FindVisibleSprites(_display.CurrentY);
        }
    }
}