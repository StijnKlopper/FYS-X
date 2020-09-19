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
            this.chunkRenderDistance = 400;
            this.regionRenderDistance = Mathf.CeilToInt(chunkRenderDistance / Region.regionSize) * Region.regionSize + Region.regionSize;
            this.terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        }

        public void LoadRegions(Vector3 position)
        {
            // Input: 220, 420. Output: 200, 400, gives corners of current region
            int x = CalcCoord(position.x, Region.regionSize);
            int z = CalcCoord(position.z, Region.regionSize);
            int xMin = x - regionRenderDistance - Region.regionSize;
            int zMin = z - regionRenderDistance - Region.regionSize;
            int xMax = x + regionRenderDistance + Region.regionSize;
            int zMax = z + regionRenderDistance + Region.regionSize;

            // Loop through current region and the 8 surrounding regions
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
            int x = CalcCoord(position.x, Region.regionSize);
            int z = CalcCoord(position.z, Region.regionSize);
            int xMin = x - regionRenderDistance - Region.regionSize;
            int zMin = z - regionRenderDistance - Region.regionSize;
            int xMax = x + regionRenderDistance + Region.regionSize;
            int zMax = z + regionRenderDistance + Region.regionSize;

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
            // x-, x+, z-, z+
            int xMin = CalcCoord(position.x - chunkRenderDistance, chunkSize);
            int xMax = CalcCoord(position.x + chunkRenderDistance, chunkSize);
            int zMin = CalcCoord(position.z - chunkRenderDistance, chunkSize);
            int zMax = CalcCoord(position.z + chunkRenderDistance, chunkSize);

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

            int xMin = CalcCoord(position.x - chunkRenderDistance, chunkSize);
            int xMax = CalcCoord(position.x + chunkRenderDistance, chunkSize);
            int zMin = CalcCoord(position.z - chunkRenderDistance, chunkSize);
            int zMax = CalcCoord(position.z + chunkRenderDistance, chunkSize);

            foreach (KeyValuePair<Vector3, GameObject> tile in terrainGenerator.tileDict.ToList())
            {
                if (tile.Key.x < xMin || tile.Key.x > xMax || tile.Key.z < zMin || tile.Key.z > zMax)
                {
                    terrainGenerator.DestroyTile(tile.Value);
                    terrainGenerator.tileDict.Remove(tile.Key);
                }
            }
        }

        private int CalcCoord(float coordinate, int size)
        {
            return Mathf.FloorToInt(coordinate / size) * size;
        }
    }
}