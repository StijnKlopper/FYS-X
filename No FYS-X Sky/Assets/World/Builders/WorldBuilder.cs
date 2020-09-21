using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.World
{
    class WorldBuilder
    {
        private TerrainGenerator terrainGenerator;

        private int chunkSize;

        // Must be divisible by Region.regionSize.
        private int chunkRenderDistance;

        private int regionRenderDistance;

        public WorldBuilder()
        {
            this.chunkSize = 10;
            this.chunkRenderDistance = 200;
            this.regionRenderDistance = Mathf.FloorToInt(chunkRenderDistance / Region.regionSize) * Region.regionSize + Region.regionSize;
            this.terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        }

        public void LoadRegions(Vector3 position)
        {
            (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, regionRenderDistance, Region.regionSize);

            // Loop through current region and the surrounding regions
            for (int i = xMin; i < xMax; i += Region.regionSize)
            {
                for (int j = zMin; j < zMax; j += Region.regionSize)
                {
                    Vector3 regionPosition = new Vector3(i, 0, j);
                    if(!terrainGenerator.regionDict.ContainsKey(regionPosition))
                    {
                        terrainGenerator.regionDict.Add(regionPosition, new Region(i, j));
                    }
                }
            }
        }

        public void UnloadRegions(Vector3 position)
        {
            (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, regionRenderDistance, Region.regionSize);

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
                    Vector3 newChunkPosition = new Vector3(i, 0, j);
                    if (!terrainGenerator.tileDict.ContainsKey(newChunkPosition))
                    {
                        GameObject tile = terrainGenerator.GenerateTile(newChunkPosition);
                        //Make the tiles a parent of the Level GameObject to have a clean hierarchy.
                        tile.transform.SetParent(terrainGenerator.transform);
                        terrainGenerator.tileDict.Add(newChunkPosition, tile);
                    }
                }
            }
        }

        public void UnloadTiles(Vector3 position)
        {
            (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, chunkRenderDistance, chunkSize);

            foreach (KeyValuePair<Vector3, GameObject> tile in terrainGenerator.tileDict.ToList())
            {
                if (tile.Key.x < xMin || tile.Key.x > xMax || tile.Key.z < zMin || tile.Key.z > zMax)
                {
                    terrainGenerator.DestroyTile(tile.Value);
                    terrainGenerator.tileDict.Remove(tile.Key);
                }
            }
        }

        private (int xMin, int xMax, int zMin, int zMax) CalcBoundaries(Vector3 position, int renderDistance, int size)
        {
            (int xMin, int xMax, int zMin, int zMax) boundaries;

            int x = CalcCoord(position.x, size);
            int z = CalcCoord(position.z, size);

            boundaries.xMin = x - renderDistance;
            boundaries.xMax = x + renderDistance;
            boundaries.zMin = z - renderDistance;
            boundaries.zMax = z + renderDistance;

            return boundaries;
        }

        private int CalcCoord(float coordinate, int size)
        {
            // Input: 220, 200. Output: 200, gives corners of current region, rounds to the nearest 200
            return Mathf.FloorToInt(coordinate / size) * size;
        }
    }
}