using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class WorldBuilder
{
    public static int chunkSize = 10;

    // Must be factors of chunkSize and 4 long
    public static int[] levelsOfDetail = new int[] {1, 2, 5, 10};

    public static int chunkRenderDistance = 100;

    private int regionRenderDistance;

    public static Dictionary<Vector3, Tile> tileDict = new Dictionary<Vector3, Tile>();

    private ObjectPool objectPool;
    private GameObject cityPoints;
    public WorldBuilder()
    {
        this.regionRenderDistance = Mathf.CeilToInt(chunkRenderDistance / Region.regionSize) * Region.regionSize + Region.regionSize;
        this.objectPool = GameObject.Find("Level").GetComponent<ObjectPool>();
        this.cityPoints = GameObject.Find("CityPoints/Buildings");
    }

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
                Vector3 playerPosV3 = GameObject.FindGameObjectWithTag("Player").transform.position;
                Vector2 playerPosV2 = new Vector2(playerPosV3.x, playerPosV3.z);

                float xzDistance = Vector2.Distance(new Vector2(i, j), playerPosV2);
                int levelOfDetail = CalculateLevelOfDetail(xzDistance);

                if (tileDict.ContainsKey(newChunkPosition))
                {
                    if (tileDict[newChunkPosition].levelOfDetail != levelOfDetail)
                    {
                        // Regenerate the terrain mesh if the level of detail is different
                        Tile tile = tileDict[newChunkPosition];
                        tile.levelOfDetail = levelOfDetail;
                        tile.RegenerateMesh();
                    }
                } else
                {
                    // Generate new tile
                    Tile tile = new Tile();

                    GameObject terrain = objectPool.GetPooledObject(ObjectPool.GameObjectType.Terrain);
                    GameObject cave = objectPool.GetPooledObject(ObjectPool.GameObjectType.Cave);

                    Vector3 terrainPosition = new Vector3(newChunkPosition.x + 5, 0, newChunkPosition.z + 5);
                    Vector3 cavePosition = new Vector3(newChunkPosition.x, -30, newChunkPosition.z);

                    terrain.transform.position = terrainPosition;
                    cave.transform.position = cavePosition;

                    tile.AddObject(terrain);
                    tile.AddObject(cave);

                    tile.levelOfDetail = levelOfDetail;

                    tileDict.Add(newChunkPosition, tile);
                    float[,] heightmap = terrain.GetComponent<TileBuilder>().Instantiate();
                    tile.heightMap = heightmap;
                    cave.GetComponent<CaveBuilder>().Instantiate(heightmap);
                }
            }
        }
    }

    // Destroy Houses if they are inactive
    public void UnloadHouses()
    {
        foreach (Transform houses in cityPoints.GetComponentInChildren<Transform>())
        {
            if (!houses.gameObject.activeSelf)
            {
                GameObject.Destroy(houses.gameObject);
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

        if (region)
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

    // Get tile from tile dict based on worldcoordinates
    public static Tile GetTile(Vector3 coordinate)
    {
        int x = CalcCoord(coordinate.x, chunkSize);
        int z = CalcCoord(coordinate.z, chunkSize);  

        return tileDict[new Vector3(x, 0, z)];
    }

    private int CalculateLevelOfDetail(float distance)
    {
        // Calculate the level of detail by taking the distance from the player to the chunk and dividing it over the different LOD levels
        int levels = levelsOfDetail.Length;
        // Diagonally
        int maxDistance = Mathf.CeilToInt(chunkRenderDistance * 1.5f);
        if (distance > maxDistance) return levelsOfDetail[levels - 1];

        float stepSize = (float)maxDistance / levels;

        return levelsOfDetail[Mathf.CeilToInt(distance / stepSize) - 1];
    }
}