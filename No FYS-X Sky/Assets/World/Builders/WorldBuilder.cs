using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
class WorldBuilder 
{
    public static int chunkSize = 10;

    public static int chunkRenderDistance = 100;

    private int regionRenderDistance;

    private ObjectPool objectPool;

    public struct MapData
    {
        public float[,] heightMap;
        public Color[,] colorMap;

        public MapData(float[,] heightMap, Color[,] colorMap)
        {
            this.heightMap = heightMap;
            this.colorMap = colorMap;
        }
    }

    public WorldBuilder()
    {
        this.regionRenderDistance = Mathf.CeilToInt(chunkRenderDistance / Region.regionSize) * Region.regionSize + Region.regionSize;
        this.objectPool = GameObject.Find("Level").GetComponent<ObjectPool>();
    }

    public static Dictionary<Vector3, Tile> tileDict = new Dictionary<Vector3, Tile>();

    public void LoadRegions(Vector3 position)
    {
        (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, regionRenderDistance, Region.regionSize, true);

        // Loop through current region and the surrounding regions
        for (int i = xMin; i < xMax; i += Region.regionSize)
        {
            for (int j = zMin; j < zMax; j += Region.regionSize)
            {
                Vector3 regionPosition = new Vector3(i, 0, j);
                if(!TerrainGenerator.regionDict.ContainsKey(regionPosition))
                {
                    TerrainGenerator.regionDict.Add(regionPosition, new Region(i, j));
                }
            }
        }
    }

    public void UnloadRegions(Vector3 position)
    {
        (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, regionRenderDistance, Region.regionSize, true);

        foreach (KeyValuePair<Vector3, Region> region in TerrainGenerator.regionDict.ToList())
        {
            if (region.Key.x < xMin || region.Key.x > xMax || region.Key.z < zMin || region.Key.z > zMax)
            {
                TerrainGenerator.regionDict.Remove(region.Key);
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
                if (!tileDict.ContainsKey(newChunkPosition))
                {
                    Tile tile = new Tile();
                    GameObject terrain = objectPool.GetPooledObject(ObjectPool.GameObjectType.Terrain);
                    GameObject cave = objectPool.GetPooledObject(ObjectPool.GameObjectType.Cave);

                    Vector3 terrainPosition = new Vector3(newChunkPosition.x + 5, 0, newChunkPosition.z + 5);
                    Vector3 cavePosition = new Vector3(newChunkPosition.x + 5, -30, newChunkPosition.z + 5);

                    terrain.transform.position = terrainPosition;
                    cave.transform.position = cavePosition;

                    tile.AddObject(terrain);
                    tile.AddObject(cave);
                    tileDict.Add(newChunkPosition, tile);
                    terrain.GetComponent<TileBuilder>().GenerateTile();



                    //float[,] heightmap =
                    //cave.GetComponent<CaveBuilder>().UpdateCaveMesh();

                    cave.GetComponent<CaveBuilder>().RequestCaveData(cave.GetComponent<CaveBuilder>().OnCaveDataReceived);

                    //RequestCaveData(OnCaveDataReceived);
                }
            }
        }
    }





    public void UnloadTiles(Vector3 position)
    {
        (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, chunkRenderDistance, chunkSize);

        foreach (KeyValuePair<Vector3, Tile> tile in tileDict.ToList())
        {
            if (tile.Key.x < xMin || tile.Key.x > xMax || tile.Key.z < zMin || tile.Key.z > zMax)
            {
                tile.Value.DestroyObjects(objectPool);
                tileDict.Remove(tile.Key);
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

    public static int CalcCoord(float coordinate, int size)
    {
        // Input: 220, 200. Output: 200, gives corners of current location, rounds to the nearest size number
        return Mathf.FloorToInt(coordinate / size) * size;
    }

    public static Tile GetTile(Vector3 coordinate)
    {
        int x = CalcCoord(coordinate.x, 10);
        int z = CalcCoord(coordinate.z, 10);

        return tileDict[new Vector3(x, 0, z)];
    }
}