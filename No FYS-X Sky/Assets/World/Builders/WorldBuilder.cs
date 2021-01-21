using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class WorldBuilder
{
    public const int CHUNK_SIZE = 10;

    public const int CHUNK_RENDER_DISTANCE = 100;

    // Maximum number of chunk that is being set per frame
    public const int MAX_CHUNK_PER_FRAME = 5;

    // Must be factors of CHUNK_SIZE and 4 long
    public static int[] levelsOfDetail = new int[] { 1, 1, 2, 5 };

    private int regionRenderDistance;

    private static Dictionary<Vector3, Tile> tileDict = new Dictionary<Vector3, Tile>();

    private ObjectPool objectPool;
    private GameObject cityPoints;
    private TileBuilder tileBuilder;

    public WorldBuilder()
    {
        this.regionRenderDistance = Mathf.CeilToInt(CHUNK_RENDER_DISTANCE / Region.REGION_SIZE) * Region.REGION_SIZE + Region.REGION_SIZE;
        this.objectPool = GameObject.Find("Level").GetComponent<ObjectPool>();
        this.tileBuilder = GameObject.Find("Level").GetComponent<TileBuilder>();
        this.cityPoints = GameObject.Find("CityPoints/Buildings");
    }

    public void LoadRegions(Vector3 position)
    {
        (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, regionRenderDistance, Region.REGION_SIZE, true);

        // Loop through current region and the surrounding regions
        for (int i = xMin; i < xMax; i += Region.REGION_SIZE)
        {
            for (int j = zMin; j < zMax; j += Region.REGION_SIZE)
            {
                Vector3 regionPosition = new Vector3(i, 0, j);
                if (!TerrainGenerator.RegionDict.ContainsKey(regionPosition))
                {
                    TerrainGenerator.RegionDict.Add(regionPosition, new Region(i, j));
                }
            }
        }
    }

    public void UnloadRegions(Vector3 position)
    {
        (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, regionRenderDistance, Region.REGION_SIZE, true);

        foreach (KeyValuePair<Vector3, Region> region in TerrainGenerator.RegionDict.ToList())
        {
            if (region.Key.x < xMin || region.Key.x > xMax || region.Key.z < zMin || region.Key.z > zMax)
            {
                TerrainGenerator.RegionDict.Remove(region.Key);
            }
        }
    }

    public void LoadTiles(Vector3 position)
    {
        (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, CHUNK_RENDER_DISTANCE, CHUNK_SIZE);

        for (int i = xMin; i < xMax; i += CHUNK_SIZE)
        {
            for (int j = zMin; j < zMax; j += CHUNK_SIZE)
            {
                Vector3 newChunkPosition = new Vector3(i, 0, j);
                Vector3 playerPosV3 = GameObject.FindGameObjectWithTag("Player").transform.position;
                Vector2 playerPosV2 = new Vector2(playerPosV3.x, playerPosV3.z);

                float xzDistance = Vector2.Distance(new Vector2(i, j), playerPosV2);
                int levelOfDetail = CalculateLevelOfDetail(xzDistance);

                if (tileDict.ContainsKey(newChunkPosition))
                {
                    if (tileDict[newChunkPosition].LevelOfDetail != levelOfDetail)
                    {
                        // Regenerate the terrain mesh if the level of detail is different
                        Tile tile = tileDict[newChunkPosition];
                        tile.LevelOfDetail = levelOfDetail;
                        tileBuilder.RegenerateMesh(tile);
                    }
                }
                else
                {
                    // Generate new tile
                    Tile tile = objectPool.GetPooledTile();
                    Vector3 terrainPosition = new Vector3(newChunkPosition.x + 5, 0, newChunkPosition.z + 5);
                    Vector3 cavePosition = new Vector3(newChunkPosition.x + 5, -30, newChunkPosition.z + 5);

                    tile.Terrain.GameObject.transform.position = terrainPosition;
                    tile.Cave.GameObject.transform.position = cavePosition;

                    tileDict.Add(newChunkPosition, tile);
                    
                    tile.LevelOfDetail = levelOfDetail;
                    tileBuilder.Instantiate(newChunkPosition);
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
        (int xMin, int xMax, int zMin, int zMax) = CalcBoundaries(position, CHUNK_RENDER_DISTANCE, CHUNK_SIZE);

        foreach (KeyValuePair<Vector3, Tile> tile in tileDict.ToList())
        {
            if (tile.Key.x < xMin || tile.Key.x > xMax || tile.Key.z < zMin || tile.Key.z > zMax)
            {
                tile.Value.DisableTile();
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

    private static int CalcCoord(float coordinate, int size)
    {
        // Input: 220, 200. Output: 200, gives corners of current location, rounds to the nearest size number
        return Mathf.FloorToInt(coordinate / size) * size;
    }

    // Get tile from tile dict based on worldcoordinates
    public static Tile GetTile(Vector3 coordinate)
    {
        int x = CalcCoord(coordinate.x, CHUNK_SIZE);
        int z = CalcCoord(coordinate.z, CHUNK_SIZE);
        Tile tile;
        if (tileDict.TryGetValue(new Vector3(x, 0, z), out tile))
        {
            return tile;
        }
        else { return null; }
    }

        private int CalculateLevelOfDetail(float distance)
    {
        // Calculate the level of detail by taking the distance from the player to the chunk and dividing it over the different LOD levels
        int levels = levelsOfDetail.Length;
        // Diagonally
        int maxDistance = Mathf.CeilToInt(CHUNK_RENDER_DISTANCE * 1.5f);
        if (distance > maxDistance) return levelsOfDetail[levels - 1];

        float stepSize = (float)maxDistance / levels;

        int levelOfDetail = Mathf.Max(Mathf.CeilToInt(distance / stepSize) - 1, 0);

        return levelsOfDetail[levelOfDetail];
    }
}