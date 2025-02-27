﻿using System.IO;

namespace RaylibLightingJuly
{
    public static class AutoTilingManager
    {
        public const string dataTableFilePath = "..\\..\\..\\neighbourToIndexTable.txt";
        private static readonly byte[] conversionTable = new byte[256];

        //bit order 0b01234567
        // 4 0 5
        // 1 - 2
        // 6 3 7                         //  0   1  2  3   4   5  6  7
        private static int[] bitOffsetsX = { 0, -1, 1, 0, -1, 1, -1, 1 };
        private static int[] bitOffsetsY = { -1, 0, 0, 1, -1, -1, 1, 1 };

        public static byte GetNeighbourMask(World world, int x, int y)
        {
            byte mask = 0;

            for (int i = 0; i < 8; i++)
            {
                int nx = x + bitOffsetsX[i];
                int ny = y + bitOffsetsY[i];

                bool tilePresent;

                if (nx < 0 || nx >= world.mapWidth || ny < 0 || ny >= world.mapHeight)
                {
                    tilePresent = true;
                }
                else
                {
                    tilePresent = world.fgTiles[nx, ny] != world.fgTiles.emptyId && world.fgTiles[nx, ny] != 2;
                }

                if (tilePresent)
                {
                    mask |= (byte)(1 << (7 - i));
                }
            }

            return mask;
        }

        public static byte GetNeighbourMaskFiltered(World world, int x, int y, byte baseline)
        {
            byte mask = 0;

            for (int i = 0; i < 8; i++)
            {
                if ((baseline >> i & 1) == 1 || world.fgTiles[x + bitOffsetsX[i], y + bitOffsetsY[i]] != world.fgTiles.emptyId)
                {
                    mask |= (byte)(1 << (7 - i));
                }
            }

            return mask;
        }

        public static int GetTilesetTileX(int texId)
        {
            return texId & 0xf;
        }
        public static int GetTilesetTileY(int texId)
        {
            return texId >> 4;
        }

        public static void UpdateTileIndexes(World world, int startX, int startY, int endX, int endY)
        {
            startX = Math.Max(0, startX);
            startY = Math.Max(0, startY);
            endX = Math.Min(world.mapWidth - 1, endX);
            endY = Math.Min(world.mapHeight - 1, endY);

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    world.fgTexIds[x, y] = conversionTable[GetNeighbourMask(world, x, y)];
                }
            }
        }

        public static byte GetTileTextureIndex(byte neighbourMask)
        {
            return conversionTable[neighbourMask];
        }

        public static void LoadConversionTable()
        {
            int index = 0;
            string[] lines = File.ReadAllLines(dataTableFilePath);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i][0] != '#')
                {
                    conversionTable[index] = Convert.ToByte(lines[i]);
                    index++;
                }
            }
        }
    }
}
