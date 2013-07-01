using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace Hunted
{
    public static class TerrainGeneration
    {
        const int TILESHEET_WIDTH = 10;
        
        const int SAND = 14;
        const int SAND_ALT = 17;
        const int WATER = 12;
        const int GRASS = 47;
        const int GRASS_ALT1 = 15;
        const int GRASS_ALT2 = 29;
        const int CONCRETE = 101;

        const int SAND_EDGE_UP = 22;
        const int SAND_EDGE_DOWN = 2;
        const int SAND_EDGE_LEFT = 13;
        const int SAND_EDGE_RIGHT = 11;

        const int SAND_CORNER_INSIDE_TL = 1;
        const int SAND_CORNER_INSIDE_TR = 3;
        const int SAND_CORNER_INSIDE_BL = 21;
        const int SAND_CORNER_INSIDE_BR = 23;

        const int SAND_CORNER_OUTSIDE_TL = 4;
        const int SAND_CORNER_OUTSIDE_TR = 5;
        const int SAND_CORNER_OUTSIDE_BL = 24;
        const int SAND_CORNER_OUTSIDE_BR = 25;

        const int GRASS_EDGE_UP = 37;
        const int GRASS_EDGE_DOWN = 57;
        const int GRASS_EDGE_LEFT = 46;
        const int GRASS_EDGE_RIGHT = 48;

        const int GRASS_CORNER_INSIDE_TL = 39;
        const int GRASS_CORNER_INSIDE_TR = 40;
        const int GRASS_CORNER_INSIDE_BL = 49;
        const int GRASS_CORNER_INSIDE_BR = 50;

        const int GRASS_CORNER_OUTSIDE_TL = 36;
        const int GRASS_CORNER_OUTSIDE_TR = 38;
        const int GRASS_CORNER_OUTSIDE_BL = 56;
        const int GRASS_CORNER_OUTSIDE_BR = 58;

        const int WALL_HORIZ = 106;
        const int WALL_VERT = 107;
        const int WALL_TL = 102;
        const int WALL_TR = 103;
        const int WALL_BL = 105;
        const int WALL_BR = 104;

        const int TREE = 31;
        static int[] TREES = new int[] { 31, 32, 41, 51, 93, 94, 95, 96, 97, 98, 99, 100 };
        static List<Rectangle> BIG_TREES = new List<Rectangle>() {
            new Rectangle(2,4,4,3),
            new Rectangle(1,7,2,2),
            new Rectangle(1,9,2,2),
            new Rectangle(3,7,3,3),
            new Rectangle(6,7,3,3),
            new Rectangle(9,6,2,2),
            new Rectangle(9,8,2,2)
        };

        static Random rand = new Random();

        public static void GenerateTerrain(Map map)
        {
            List<Compound> compounds = new List<Compound>();

            TileLayer terrainLayer = map.GetLayer("Terrain") as TileLayer;
            TileLayer wallLayer = map.GetLayer("Wall") as TileLayer;

            for (int y = 0; y < map.Width; y++)
            {
                for (int x = 0; x < map.Height; x++)
                {
                    terrainLayer.Tiles[x, y] = null;
                    wallLayer.Tiles[x, y] = null;
                }
            }
            

            // Inital terrain
            float[][] noise = PerlinNoise.GeneratePerlinNoise(map.Width, map.Height, 8);
            for (int y = 0; y < map.Width; y++)
            {
                for (int x = 0; x < map.Height; x++)
                    if (noise[y][x] < 0.5f)
                        terrainLayer.Tiles[x, y] = map.Tiles[WATER];
                    else if (noise[y][x] < 0.6f)
                        terrainLayer.Tiles[x, y] = map.Tiles[SAND];
                    else
                        terrainLayer.Tiles[x, y] = map.Tiles[GRASS];
            }

            // Remove stray tiles
            for (int y = 0; y < map.Width; y++)
            {
                for (int x = 0; x < map.Height; x++)
                {
                    if (GetTileIndex(map, terrainLayer, x, y) == SAND)
                        if (CountSurroundingTiles(map, terrainLayer, x, y, WATER) >= 5) terrainLayer.Tiles[x, y] = map.Tiles[WATER];

                    //if (GetTileIndex(map, terrainLayer, (map.Width-1) - x, (map.Height-1) - y) == SAND)
                    //    if (CountSurroundingTiles(map, terrainLayer, (map.Width - 1) - x, (map.Height - 1) - y, WATER) >= 5) terrainLayer.Tiles[(map.Width - 1) - x, (map.Height - 1) - y] = map.Tiles[WATER];

                    if (GetTileIndex(map, terrainLayer, x, y) == GRASS)
                        if (CountSurroundingTiles(map, terrainLayer, x, y, SAND) >= 5) terrainLayer.Tiles[x, y] = map.Tiles[SAND];

                    //if (GetTileIndex(map, terrainLayer, (map.Width - 1) - x, (map.Height - 1) - y) == GRASS)
                    //    if (CountSurroundingTiles(map, terrainLayer, (map.Width - 1) - x, (map.Height - 1) - y, SAND) >= 5) terrainLayer.Tiles[(map.Width - 1) - x, (map.Height - 1) - y] = map.Tiles[SAND];
                }
            }


            // Trees
            float[][] treeNoise = PerlinNoise.GeneratePerlinNoise(map.Width, map.Height, 4);

            for (int y = 0; y < map.Width; y++)
            {
                for (int x = 0; x < map.Height; x++)
                {
                    if (noise[y][x] > 0.62f)
                        if (treeNoise[y][x] > 0.5f)
                            wallLayer.Tiles[x, y] = map.Tiles[TREE];
                }
            }

            // Detail tiling!
            for (int y = 0; y < map.Width; y++)
            {
                for (int x = 0; x < map.Height; x++)
                {

                    // Sand/Water
                    if (GetTileIndex(map, terrainLayer, x, y) == SAND)
                    {
                        // Edges
                        if (GetTileIndex(map, terrainLayer, x - 1, y) == WATER)
                            if (GetTileIndex(map, terrainLayer, x, y - 1) != WATER)
                                if (GetTileIndex(map, terrainLayer, x, y + 1) != WATER) terrainLayer.Tiles[x, y] = map.Tiles[SAND_EDGE_LEFT];

                        if (GetTileIndex(map, terrainLayer, x + 1, y) == WATER)
                            if (GetTileIndex(map, terrainLayer, x, y - 1) != WATER)
                                if (GetTileIndex(map, terrainLayer, x, y + 1) != WATER) terrainLayer.Tiles[x, y] = map.Tiles[SAND_EDGE_RIGHT];

                        if (GetTileIndex(map, terrainLayer, x, y + 1) == WATER)
                            if (GetTileIndex(map, terrainLayer, x - 1, y) != WATER)
                                if (GetTileIndex(map, terrainLayer, x + 1, y) != WATER) terrainLayer.Tiles[x, y] = map.Tiles[SAND_EDGE_DOWN];

                        if (GetTileIndex(map, terrainLayer, x, y - 1) == WATER)
                            if (GetTileIndex(map, terrainLayer, x - 1, y) != WATER)
                                if (GetTileIndex(map, terrainLayer, x + 1, y) != WATER) terrainLayer.Tiles[x, y] = map.Tiles[SAND_EDGE_UP];

                        // Corners - water inside
                        if (GetTileIndex(map, terrainLayer, x - 1, y - 1) == WATER)
                            if (GetTileIndex(map, terrainLayer, x, y - 1) != WATER)
                                if (GetTileIndex(map, terrainLayer, x - 1, y) != WATER) terrainLayer.Tiles[x, y] = map.Tiles[SAND_CORNER_INSIDE_BR];

                        if (GetTileIndex(map, terrainLayer, x - 1, y + 1) == WATER)
                            if (GetTileIndex(map, terrainLayer, x, y + 1) != WATER)
                                if (GetTileIndex(map, terrainLayer, x - 1, y) != WATER) terrainLayer.Tiles[x, y] = map.Tiles[SAND_CORNER_INSIDE_TR];

                        if (GetTileIndex(map, terrainLayer, x + 1, y - 1) == WATER)
                            if (GetTileIndex(map, terrainLayer, x, y - 1) != WATER)
                                if (GetTileIndex(map, terrainLayer, x + 1, y) != WATER) terrainLayer.Tiles[x, y] = map.Tiles[SAND_CORNER_INSIDE_BL];

                        if (GetTileIndex(map, terrainLayer, x + 1, y + 1) == WATER)
                            if (GetTileIndex(map, terrainLayer, x, y + 1) != WATER)
                                if (GetTileIndex(map, terrainLayer, x + 1, y) != WATER) terrainLayer.Tiles[x, y] = map.Tiles[SAND_CORNER_INSIDE_TL];

                        // Corners - water outside
                        if (GetTileIndex(map, terrainLayer, x - 1, y - 1) == WATER)
                            if (GetTileIndex(map, terrainLayer, x, y - 1) == WATER)
                                if (GetTileIndex(map, terrainLayer, x - 1, y) == WATER) terrainLayer.Tiles[x, y] = map.Tiles[SAND_CORNER_OUTSIDE_TL];

                        if (GetTileIndex(map, terrainLayer, x - 1, y + 1) == WATER)
                            if (GetTileIndex(map, terrainLayer, x, y + 1) == WATER)
                                if (GetTileIndex(map, terrainLayer, x - 1, y) == WATER) terrainLayer.Tiles[x, y] = map.Tiles[SAND_CORNER_OUTSIDE_BL];

                        if (GetTileIndex(map, terrainLayer, x + 1, y - 1) == WATER)
                            if (GetTileIndex(map, terrainLayer, x, y - 1) == WATER)
                                if (GetTileIndex(map, terrainLayer, x + 1, y) == WATER) terrainLayer.Tiles[x, y] = map.Tiles[SAND_CORNER_OUTSIDE_TR];

                        if (GetTileIndex(map, terrainLayer, x + 1, y + 1) == WATER)
                            if (GetTileIndex(map, terrainLayer, x, y + 1) == WATER)
                                if (GetTileIndex(map, terrainLayer, x + 1, y) == WATER) terrainLayer.Tiles[x, y] = map.Tiles[SAND_CORNER_OUTSIDE_BR];

                    }

                    // Grass/Sand
                    if (GetTileIndex(map, terrainLayer, x, y) == GRASS || GetTileIndex(map, terrainLayer, x, y) == GRASS_ALT1 || GetTileIndex(map, terrainLayer, x, y) == GRASS_ALT2)
                    {
                        // Edges
                        if (GetTileIndex(map, terrainLayer, x - 1, y) == SAND)
                            if (GetTileIndex(map, terrainLayer, x, y - 1) != SAND)
                                if (GetTileIndex(map, terrainLayer, x, y + 1) != SAND) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_EDGE_LEFT];

                        if (GetTileIndex(map, terrainLayer, x + 1, y) == SAND)
                            if (GetTileIndex(map, terrainLayer, x, y - 1) != SAND)
                                if (GetTileIndex(map, terrainLayer, x, y + 1) != SAND) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_EDGE_RIGHT];

                        if (GetTileIndex(map, terrainLayer, x, y + 1) == SAND)
                            if (GetTileIndex(map, terrainLayer, x - 1, y) != SAND)
                                if (GetTileIndex(map, terrainLayer, x + 1, y) != SAND) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_EDGE_DOWN];

                        if (GetTileIndex(map, terrainLayer, x, y - 1) == SAND)
                            if (GetTileIndex(map, terrainLayer, x - 1, y) != SAND)
                                if (GetTileIndex(map, terrainLayer, x + 1, y) != SAND) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_EDGE_UP];

                        // Corners - water inside
                        if (GetTileIndex(map, terrainLayer, x - 1, y - 1) == SAND)
                            if (GetTileIndex(map, terrainLayer, x, y - 1) != SAND)
                                if (GetTileIndex(map, terrainLayer, x - 1, y) != SAND) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_CORNER_INSIDE_BR];

                        if (GetTileIndex(map, terrainLayer, x - 1, y + 1) == SAND)
                            if (GetTileIndex(map, terrainLayer, x, y + 1) != SAND)
                                if (GetTileIndex(map, terrainLayer, x - 1, y) != SAND) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_CORNER_INSIDE_TR];

                        if (GetTileIndex(map, terrainLayer, x + 1, y - 1) == SAND)
                            if (GetTileIndex(map, terrainLayer, x, y - 1) != SAND)
                                if (GetTileIndex(map, terrainLayer, x + 1, y) != SAND) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_CORNER_INSIDE_BL];

                        if (GetTileIndex(map, terrainLayer, x + 1, y + 1) == SAND)
                            if (GetTileIndex(map, terrainLayer, x, y + 1) != SAND)
                                if (GetTileIndex(map, terrainLayer, x + 1, y) != SAND) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_CORNER_INSIDE_TL];

                        // Corners - water outside
                        if (GetTileIndex(map, terrainLayer, x - 1, y - 1) == SAND)
                            if (GetTileIndex(map, terrainLayer, x, y - 1) == SAND)
                                if (GetTileIndex(map, terrainLayer, x - 1, y) == SAND) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_CORNER_OUTSIDE_TL];

                        if (GetTileIndex(map, terrainLayer, x - 1, y + 1) == SAND)
                            if (GetTileIndex(map, terrainLayer, x, y + 1) == SAND)
                                if (GetTileIndex(map, terrainLayer, x - 1, y) == SAND) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_CORNER_OUTSIDE_BL];

                        if (GetTileIndex(map, terrainLayer, x + 1, y - 1) == SAND)
                            if (GetTileIndex(map, terrainLayer, x, y - 1) == SAND)
                                if (GetTileIndex(map, terrainLayer, x + 1, y) == SAND) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_CORNER_OUTSIDE_TR];

                        if (GetTileIndex(map, terrainLayer, x + 1, y + 1) == SAND)
                            if (GetTileIndex(map, terrainLayer, x, y + 1) == SAND)
                                if (GetTileIndex(map, terrainLayer, x + 1, y) == SAND) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_CORNER_OUTSIDE_BR];

                    }
                }
            }
            
            // Compounds
            CreateCompounds(map, terrainLayer, wallLayer, compounds, noise, 0.65f, 20000f, 20);
            CreateCompounds(map, terrainLayer, wallLayer, compounds, noise, 0.75f, 12000f, 30);

            // Alt tiles
            for (int y = 0; y < map.Width; y++)
            {
                for (int x = 0; x < map.Height; x++)
                {
                    if (GetTileIndex(map, terrainLayer, x, y) == GRASS)
                    {
                        int r = rand.Next(50);
                        if (r == 9) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_ALT1];
                        else if (r == 8) terrainLayer.Tiles[x, y] = map.Tiles[GRASS_ALT2];
                    }
                    if (GetTileIndex(map, terrainLayer, x, y) == SAND)
                    {
                        int r = rand.Next(50);
                        if (r == 9) terrainLayer.Tiles[x, y] = map.Tiles[SAND_ALT];
                    }

                    // Tree alts
                    if (GetTileIndex(map, wallLayer, x, y) == TREE)
                    {
                        //if (rand.Next(10) == 0)
                        //{
                            // Big tree
                        if(!TryDrawBigTree(map, wallLayer, x, y)) wallLayer.Tiles[x, y] = map.Tiles[TREES[rand.Next(TREES.Length)]];

                    }
                }
            }
        }

        private static bool TryDrawBigTree(Map map, TileLayer wallLayer, int x, int y)
        {
            Rectangle thisTree = BIG_TREES[rand.Next(7)];

            bool canFit = true;
            for (int xx = x; xx < x + thisTree.Width; xx++)
                for (int yy = y; yy < y + thisTree.Height; yy++)
                    if (GetTileIndex(map, wallLayer, xx, yy) != TREE) canFit = false;

            if (canFit)
            {
                int mapx = x;
                int mapy = y;
                for (int xx = thisTree.Left; xx < thisTree.Right; xx++)
                {
                    for (int yy = thisTree.Top; yy < thisTree.Bottom; yy++)
                    {
                        int tile = ((yy-1) * TILESHEET_WIDTH) + (xx % (TILESHEET_WIDTH+1));
                        wallLayer.Tiles[mapx, mapy] = map.Tiles[tile];
                        mapy++;
                    }
                    mapy = y;
                    mapx++;
                }

            }

            return canFit;
        }

        private static void CreateCompounds(Map map, TileLayer terrainLayer, TileLayer wallLayer, List<Compound> compounds, float[][] noise, float height, float distance, int minsize)
        {
            for (int y = 40; y < map.Width - 40; y++)
            {
                for (int x = 40; x < map.Height - 40; x++)
                {
                    if (noise[y][x] > height)
                    {
                        Vector2 thisLoc = new Vector2(x * map.TileWidth, y * map.TileHeight);

                        bool tooClose = false;
                        foreach (Compound c in compounds)
                            if ((c.Position - thisLoc).Length() < distance) tooClose = true;

                        if (!tooClose)
                        {
                            Rectangle bounds = new Rectangle(x, y, 1, 1);
                            bounds.Inflate(minsize, minsize);
                            bounds.Inflate(rand.Next(5) * 2, rand.Next(5) * 2);

                            bool tooBig = true;
                            while (tooBig)
                            {
                                if (GetTileIndex(map, terrainLayer, bounds.Left, bounds.Top) != GRASS ||
                                    GetTileIndex(map, terrainLayer, bounds.Right, bounds.Top) != GRASS ||
                                    GetTileIndex(map, terrainLayer, bounds.Left, bounds.Bottom) != GRASS ||
                                    GetTileIndex(map, terrainLayer, bounds.Right, bounds.Bottom) != GRASS)
                                {
                                    tooBig = true;
                                    bounds.Inflate(-1, -1);
                                }
                                else tooBig = false;
                            }

                            if (bounds.Width >= minsize && bounds.Height >= minsize)
                            {
                                Compound newCompound = new Compound() { Position = thisLoc };

                                for (int xx = bounds.Left; xx <= bounds.Right; xx++)
                                {
                                    for (int yy = bounds.Top; yy <= bounds.Bottom; yy++)
                                    {
                                        wallLayer.Tiles[xx, yy] = null;
                                        if (xx > bounds.Left + 2 && xx < bounds.Right - 2 && yy > bounds.Top + 2 && yy < bounds.Bottom - 2)
                                            terrainLayer.Tiles[xx, yy] = map.Tiles[CONCRETE];
                                    }
                                }

                                Rectangle innerBounds = bounds;
                                innerBounds.Inflate(-3, -3);

                                // Outer walls
                                wallLayer.Tiles[innerBounds.Left, innerBounds.Top] = map.Tiles[WALL_TL];
                                wallLayer.Tiles[innerBounds.Right, innerBounds.Top] = map.Tiles[WALL_TR];
                                wallLayer.Tiles[innerBounds.Left, innerBounds.Bottom] = map.Tiles[WALL_BL];
                                wallLayer.Tiles[innerBounds.Right, innerBounds.Bottom] = map.Tiles[WALL_BR];

                                for (int xx = innerBounds.Left + 1; xx <= innerBounds.Right - 1; xx++)
                                {
                                    wallLayer.Tiles[xx, innerBounds.Top] = map.Tiles[WALL_HORIZ];
                                    wallLayer.Tiles[xx, innerBounds.Bottom] = map.Tiles[WALL_HORIZ];
                                } 
                                for (int yy = innerBounds.Top + 1; yy <= innerBounds.Bottom - 1; yy++)
                                {
                                    wallLayer.Tiles[innerBounds.Left,yy] = map.Tiles[WALL_VERT];
                                    wallLayer.Tiles[innerBounds.Right,yy] = map.Tiles[WALL_VERT];
                                }

                                newCompound.Bounds = bounds;
                                newCompound.InnerBounds = innerBounds;

                                // Exits
                                bool[] exits = new bool[4] { false, false, false, false };
                                for(int i=0;i<4;i++)
                                    exits[rand.Next(4)] = true;

                                bool carparkPlaced = false;

                                if (exits[0])
                                {
                                    int doorx = rand.Next(innerBounds.Width - 7) + 3;
                                    for (int xx = innerBounds.Left + doorx; xx < (innerBounds.Left + doorx) + 4; xx++) wallLayer.Tiles[xx, innerBounds.Top] = null;
                                    if (!carparkPlaced && rand.Next(5) == 1)
                                    {
                                        Building cp = new Building() { Type = BuildingType.Carpark, Rect = new Rectangle((innerBounds.Left + doorx), innerBounds.Top + 2, 4, 4) };
                                        newCompound.Buildings.Add(cp);
                                        carparkPlaced = true;
                                    }
                                }
                                if (exits[1])
                                {
                                    int doorx = rand.Next(innerBounds.Width - 7) + 3;
                                    for (int xx = innerBounds.Left + doorx; xx < (innerBounds.Left + doorx) + 4; xx++) wallLayer.Tiles[xx, innerBounds.Bottom] = null;
                                    if (!carparkPlaced && rand.Next(5) == 1)
                                    {
                                        Building cp = new Building() { Type = BuildingType.Carpark, Rect = new Rectangle((innerBounds.Left + doorx), innerBounds.Top -6, 4, 4) };
                                        newCompound.Buildings.Add(cp);
                                        carparkPlaced = true;
                                    }
                                }
                                if (exits[2])
                                {
                                    int doory = rand.Next(innerBounds.Height - 7) + 3;
                                    for (int yy = innerBounds.Top + doory; yy < (innerBounds.Top + doory) + 4; yy++) wallLayer.Tiles[innerBounds.Left,yy] = null;
                                    if (!carparkPlaced && rand.Next(5) == 1)
                                    {
                                        Building cp = new Building() { Type = BuildingType.Carpark, Rect = new Rectangle(innerBounds.Left + 2, (innerBounds.Top + doory), 4, 4) };
                                        newCompound.Buildings.Add(cp);
                                        carparkPlaced = true;
                                    }
                                }
                                if (exits[3])
                                {
                                    int doory = rand.Next(innerBounds.Height - 7) + 3;
                                    for (int yy = innerBounds.Top + doory; yy < (innerBounds.Top + doory) + 4; yy++) wallLayer.Tiles[innerBounds.Right, yy] = null;
                                    if (!carparkPlaced && rand.Next(5) == 1)
                                    {
                                        Building cp = new Building() { Type = BuildingType.Carpark, Rect = new Rectangle(innerBounds.Right - 6, (innerBounds.Top + doory), 4, 4) };
                                        newCompound.Buildings.Add(cp);
                                        carparkPlaced = true;
                                    }
                                }



                                compounds.Add(newCompund);
                            }
                        }
                    }
                }
            }
        }

        static int GetTileIndex(Map map, TileLayer layer, int x, int y)
        {
            if (x > -1 && x < map.Width && y > -1 && y < map.Height && layer.Tiles[x, y]!=null)
            {
                return map.Tiles.IndexOf(layer.Tiles[x, y]);
            }

            return -1;
        }

        static int CountSurroundingTiles(Map map, TileLayer layer, int x, int y, int index)
        {
            int count = 0;

            for (int yy = y - 1; yy <= y + 1; yy++)
                for (int xx = x - 1; xx <= x + 1; xx++)
                    if (GetTileIndex(map, layer, xx, yy) == index && !(x == xx && y == yy)) count++;

            return count;
        }
    }
}
