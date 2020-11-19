﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Assets.World
{
    class WorldBuilder
    {
        private TerrainGenerator terrainGenerator;

        private CityGenerator cityGenerator;

        private int chunkSize;

        private int chunkRenderDistance;

        private int regionRenderDistance;

        private int cityRenderDistance;

        public WorldBuilder()
        {
            this.chunkSize = 10;
            this.chunkRenderDistance = 100;
            this.regionRenderDistance = Mathf.CeilToInt(chunkRenderDistance / Region.regionSize) * Region.regionSize + Region.regionSize;
            this.terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
            this.cityGenerator = GameObject.Find("CityPoints").GetComponent<CityGenerator>();
            this.cityRenderDistance = this.chunkRenderDistance - cityGenerator.cityRadius;


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
                    Debug.Log(tile.Key);
                    if (cityGenerator.cubes.ContainsKey(tile.Key))
                    {
                        UnityEngine.MonoBehaviour.Destroy(cityGenerator.cubes[tile.Key]);
                        cityGenerator.cubes.Remove(tile.Key);
                        
                    }
                   
                    terrainGenerator.DestroyTile(tile.Value);
                    terrainGenerator.tileDict.Remove(tile.Key);
                }
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

        public void UnloadCityPoints(Vector3 position)
        {
            (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, cityRenderDistance, chunkSize);

            foreach (KeyValuePair<Vector3, List<Vector3>> points in cityGenerator.cityPoints.ToList())
            {
                if (points.Key.x < xMin || points.Key.x > xMax || points.Key.z < zMin || points.Key.z > zMax)
                {
                    cityGenerator.cityPoints.Remove(points.Key);
                }
            }
        }

        private int CalcCoord(float coordinate, int size)
        {
            // Input: 220, 200. Output: 200, gives corners of current location, rounds to the nearest size number
            return Mathf.FloorToInt(coordinate / size) * size;
        }
    }
}