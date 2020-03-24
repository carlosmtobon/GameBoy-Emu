using ChichoGB.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.core.ppu
{
    class BackgroundTileMap
    {
        private const int BG_MAP_ADDRESS_1 = 0x9800;

        public const int TILE_DATA_START = 0x8000;
        private Dictionary<int, byte[]> _tileMap;

        private Mmu _ram;

        public BackgroundTileMap(Mmu ram)
        {
            _ram = ram;
            _tileMap = new Dictionary<int, byte[]>();
        }
        public void DisplayBgMap1()
        {
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    byte tileToDisplay = _ram.LoadU8Bits(BG_MAP_ADDRESS_1 + (i * 32));
                    //_tileMap
                    //DisplayBgTile()
                }
            }
        }

        public void LoadTileMap()
        {
            int tileNumber = 0;
            for (int tilesToLoad = 0; tilesToLoad < 128 * 3; tilesToLoad++)
            {
                int startAddr = TILE_DATA_START + (tilesToLoad * 16);
                byte[] tileData = new byte[16];
                for (int i = 0; i < 16; i++)
                {
                    tileData[i] = _ram.LoadU8Bits(startAddr + i);
                }
                _tileMap.Remove(startAddr);
                _tileMap.Add(startAddr, tileData);
                DisplayTile(startAddr, tileData);
            }
        }

        private void DisplayTile(int addr, byte[] tileData)
        {
            Console.WriteLine(String.Format("Address: {0:X}", addr));
            for (int i = 0; i < 16; i += 2)
            {
                byte h = tileData[i];
                byte l = tileData[i + 1];
                for (int j = 7; j > 0; j--)
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
