using GameBoy_Emu.core.mmu;
using GameBoy_Emu.core.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameBoy_Emu.core.ppu
{
    public class BgTileMapManager
    {
        public const int BG_MAP_ADDRESS_1 = 0x9800;
        public const int BG_MAP_ADDRESS_2 = 0x9C00;

        public const int TILE_DATA_START_1 = 0x8000;
        public const int TILE_DATA_START_2 = 0x9000;

        private Dictionary<int, Tile> _tileMap;
        private readonly OamEntryManager _oamEntryManager;

        private readonly Mmu _mmu;

        public BgTileMapManager(Mmu mmu)
        {
            _mmu = mmu;
            _tileMap = new Dictionary<int, Tile>();
            _oamEntryManager = new OamEntryManager(mmu);
        }

        public int GetBgTileAddr()
        {
            int tileMapAddr = BG_MAP_ADDRESS_1;
            if (BitUtils.IsBitSet(_mmu.LoadLcdc(), 3))
            {
                tileMapAddr = BG_MAP_ADDRESS_2;
            }
            return tileMapAddr;
        }
        public int GetWinTileAddr()
        {
            int tileMapAddr = BG_MAP_ADDRESS_1;
            if (BitUtils.IsBitSet(_mmu.LoadLcdc(), 6))
            {
                tileMapAddr = BG_MAP_ADDRESS_2;
            }
            return tileMapAddr;
        }


        public int GetTileNumber(int address)
        {
            return _mmu.LoadUnsigned8(address);
        }

        public Tile GetTile(int tileNumber)
        {
            int startAddr;

            if (!BitUtils.IsBitSet(_mmu.LoadLcdc(), 4))
            {
                startAddr = tileNumber < 127 ? TILE_DATA_START_2 + (tileNumber * 16) : TILE_DATA_START_2 + ((sbyte)tileNumber * 16);
            }
            else
            {
                startAddr = TILE_DATA_START_1 + (tileNumber * 16);
            }
            byte[] tileData = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                tileData[i] = _mmu.LoadUnsigned8(startAddr + i);
            }
            return new Tile(startAddr, tileData);
        }

        public Tile GetSpriteTile(int tileNumber, int height)
        {
            int byteTotal = height * 2;
            int tileDataStart = TILE_DATA_START_1;
            int addr = tileDataStart + (tileNumber * 16);
            byte[] tileData = new byte[byteTotal];
            for (int i = 0; i < byteTotal; i++)
            {
                tileData[i] = _mmu.LoadUnsigned8(addr + i);
            }
            return new Tile(addr, tileData);
        }

        public void DisplayBgMap1()
        {
            LoadTileMap();
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    byte tileNumber = _mmu.LoadUnsigned8(BG_MAP_ADDRESS_1 + (i * 32) + j);
                    Tile tile = _tileMap[tileNumber];
                    DisplayTile(tile.Address, tile.TileData);
                }
            }
        }

        public void LoadTileMap()
        {
            if (_tileMap == null)
            {
                _tileMap = new Dictionary<int, Tile>();
            }

            int tileNumber = 0;
            for (int tilesToLoad = 0; tilesToLoad < 128 * 3; tilesToLoad++)
            {
                int startAddr = TILE_DATA_START_1 + (tilesToLoad * 16);
                byte[] tileData = new byte[16];
                for (int i = 0; i < 16; i++)
                {
                    tileData[i] = _mmu.LoadUnsigned8(startAddr + i);
                }
                _tileMap.Remove(tileNumber);
                _tileMap.Add(tileNumber, new Tile(startAddr, tileData));
                tileNumber++;
            }
        }

        public void FindVisibleSprites(int y)
        {
            _oamEntryManager.FindVisibleSprites(y, GetSpriteHeight());
        }

        public int GetSpriteHeight()
        {
            return BitUtils.GetBit(_mmu.LoadLcdc(), 2) == 1 ? 16 : 8;
        }

        public List<OamEntry> GetVisibleSprites()
        {
            return _oamEntryManager.VisibleSprites;
        }

        private void DisplayTile(int addr, byte[] tileData)
        {
             Debug.WriteLine(string.Format("Address: {0:X}", addr));
            for (int i = 0; i < 16; i += 2)
            {
                byte h = tileData[i];
                byte l = tileData[i + 1];
                for (int j = 8; j > 0; j--)
                {
                    int pixel = (l >> j) & 1 | (h >> (j - 1) & 2);

                    if (pixel == 3)
                    {
                        Console.Write('3');
                    }

                    if (pixel == 2)
                    {
                        Console.Write('2');
                    }

                    if (pixel == 1)
                    {
                        Console.Write('1');
                    }

                    if (pixel == 0)
                    {
                        Console.Write('0');
                    }
                }
                 Debug.WriteLine("");

            }
             Debug.WriteLine("");
        }
    }
}
