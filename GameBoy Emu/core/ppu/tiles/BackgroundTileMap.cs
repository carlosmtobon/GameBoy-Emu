using ChichoGB.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.core.ppu
{
    public class BackgroundTileMap
    {
        public const int BG_MAP_ADDRESS_1 = 0x9800;
        public const int TILE_DATA_START = 0x8000;
        private Dictionary<int, Tile> _tileMap;

        private Mmu _ram;

        public BackgroundTileMap(Mmu ram)
        {
            _ram = ram;
            _tileMap = new Dictionary<int, Tile>();
        }

        public int GetTileNumber(int address)
        {
            return _ram.LoadUnsigned8(address);
        }

        public int GetTileNumber(int x, int y)
        {
            var addr = BG_MAP_ADDRESS_1 + (y * 32) + x;
            return _ram.LoadUnsigned8(addr);
        }

        public Tile GetTile(int tileNumber)
        {
            int startAddr = TILE_DATA_START + (tileNumber * 16);
            byte[] tileData = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                tileData[i] = _ram.LoadUnsigned8(startAddr + i);
            }
            return new Tile(startAddr, tileData);
        }

        public Tile GetTile(int x, int y)
        {
            int tileNumber = GetTileNumber(x, y);
            int startAddr = TILE_DATA_START + (tileNumber * 16);
            byte[] tileData = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                tileData[i] = _ram.LoadUnsigned8(startAddr + i);
            }
            return new Tile(startAddr, tileData);
        }

        public void DisplayBgMap1()
        {
            LoadTileMap();
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    byte tileNumber = _ram.LoadUnsigned8(BG_MAP_ADDRESS_1  + (i * 32) + j);
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
                int startAddr = TILE_DATA_START + (tilesToLoad * 16);
                byte[] tileData = new byte[16];
                for (int i = 0; i < 16; i++)
                {
                    tileData[i] = _ram.LoadUnsigned8(startAddr + i);
                }
                _tileMap.Remove(tileNumber);
                _tileMap.Add(tileNumber, new Tile(startAddr, tileData));
                tileNumber++;
            }
        }

        private void DisplayTile(int addr, byte[] tileData)
        {
            Console.WriteLine(String.Format("Address: {0:X}", addr));
            for (int i = 0; i < 16; i += 2)
            {
                byte h = tileData[i];
                byte l = tileData[i + 1];
                for (int j = 8; j > 0; j--)
                {
                    int pixel = (l >> j) & 1 | (h >> (j - 1) & 2);

                    if (pixel == 3)
                        Console.Write('3');
                    if (pixel == 2)
                        Console.Write('2');
                    if (pixel == 1)
                        Console.Write('1');
                    if (pixel == 0)
                        Console.Write('0');
                }
                Console.WriteLine("");

            }
            Console.WriteLine("");
        }
    }
}
