﻿using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.World
{
    class WorldBuilder
    {
        private TerrainGenerator terrainGenerator;

        private CaveGenerator caveGenerator;

        private int chunkSize;

        private int chunkRenderDistance;

        private int regionRenderDistance;

        public WorldBuilder()
        {
            this.chunkSize = 10;
            this.chunkRenderDistance = 100;
            this.regionRenderDistance = Mathf.CeilToInt(chunkRenderDistance / Region.regionSize) * Region.regionSize + Region.regionSize;
            this.terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
            this.caveGenerator = GameObject.Find("Level").GetComponent<CaveGenerator>();
        }

        public void LoadRegions(Vector3 position)
        {
            (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, regionRenderDistance, Region.regionSize, true);

            //Debug.Log("x1: " + xMin + ", x2: " + xMax + ", z1: " + zMin + ", z2: " + zMax);
            // Loop through current region and the surrounding regions
            for (int i = xMin; i < xMax; i += Region.regionSize)
            {
                for (int j = zMin; j < zMax; j += Region.regionSize)
                {
                    Vector3 regionPosition = new Vector3(i, 0, j);
                    if(!terrainGenerator.regionDict.ContainsKey(regionPosition))
                    {
                        //Debug.Log(regionPosition);
                        terrainGenerator.regionDict.Add(regionPosition, new Region(i, j));
                    }
                }
            }
        }

        public void UnloadRegions(Vector3 position)
        {
            (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, regionRenderDistance, Region.regionSize, true);

            foreach (KeyValuePair<Vector3, Region> region in terrainGenerator.regionDict.ToList())
            {
                if (region.Key.x < xMin || region.Key.x > xMax || region.Key.z < zMin || region.Key.z > zMax)
                {
                    terrainGenerator.regionDict.Remove(region.Key);
                }
            }
        }

        public void LoadTiles(Vector3 position)
        {
            (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, chunkRenderDistance, chunkSize);

            for (int i = xMin; i < xMax; i += chunkSize)
            {
                for (int j = zMin; j < zMax; j += chunkSize)
                {
                    Dictionary<TerrainGenerator.TileType, GameObject> tileDict = new Dictionary<TerrainGenerator.TileType, GameObject>();
                    Vector3 newChunkPosition = new Vector3(i, 0, j);
                    if (!terrainGenerator.tileDict.ContainsKey(newChunkPosition))
                    {
                        tileDict.Add(TerrainGenerator.TileType.Terrain, terrainGenerator.GenerateTile(newChunkPosition));
                        tileDict.Add(TerrainGenerator.TileType.Cave, caveGenerator.GenerateTile(newChunkPosition));

                        terrainGenerator.tileDict.Add(newChunkPosition, tileDict);
                    }
                }
            }
        }

        public void UnloadTiles(Vector3 position)
        {
            (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, chunkRenderDistance, chunkSize);

            // Next step, loop through coordinates for tiles and have the value be a list of gameobjects which is looped through to delete them
            foreach (KeyValuePair<Vector3, Dictionary<TerrainGenerator.TileType, GameObject>> tile in terrainGenerator.tileDict.ToList())
            {
                if (tile.Key.x < xMin || tile.Key.x > xMax || tile.Key.z < zMin || tile.Key.z > zMax)
                {
                    DestroyTiles(tile.Value);
                    terrainGenerator.tileDict.Remove(tile.Key);
                }
            }
        }

        private void DestroyTiles(Dictionary<TerrainGenerator.TileType, GameObject> tiles)
        {
            foreach (KeyValuePair<TerrainGenerator.TileType, GameObject> tile in tiles)
            {
                terrainGenerator.DestroyTile(tile.Value);
            }
        }

        private (int xMin, int xMax, int zMin, int zMax) CalcBoundaries(Vector3 position, int renderDistance, int size, bool region = false)
        {
            (int xMin, int xMax, int zMin, int zMax) boundaries;

            int x = CalcCoord(position.x, size);
            int z = CalcCoord(position.z, size);

            if(region)
            {
                renderDistance += size;
            }

            boundaries.xMin = x - renderDistance;
            boundaries.xMax = x + renderDistance;
            boundaries.zMin = z - renderDistance;
            boundaries.zMax = z + renderDistance;

            return boundaries;
        }

        private int CalcCoord(float coordinate, int size)
        {
            // Input: 220, 200. Output: 200, gives corners of current location, rounds to the nearest size number
            return Mathf.FloorToInt(coordinate / size) * size;
        }
    }
}