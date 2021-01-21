using LibNoise.Generator;
using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

public class TileBuilder : MonoBehaviour
{
    public const int TILE_DIMENSION = WorldBuilder.CHUNK_SIZE + 1;
    private TerrainGenerator terrainGenerator;
    private DecorationGenerator decorationGenerator;
    private CityGenerator cityGenerator;
    private Texture2DArray splatmapsArray;
    private Texture2D grassMap;
    private CaveBuilder caveBuilder;
    private ConcurrentQueue<MapThreadInfo<TileData>> terrainDataThreadInfoQueue;

    public void Start()
    {
        terrainDataThreadInfoQueue = new ConcurrentQueue<MapThreadInfo<TileData>>();
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        caveBuilder = GameObject.Find("Level").GetComponent<CaveBuilder>();
        cityGenerator = GameObject.Find("CityPoints").GetComponent<CityGenerator>();
        decorationGenerator = GameObject.Find("Decorations").GetComponent<DecorationGenerator>();
    }

    public void Instantiate(Vector3 position)
    {
        RequestTileData(OnTileDataReceived, position);
    }

    // Start a thread to create TileData and perform a callback to the onTileDataReceived method
    private void RequestTileData(Action<TileData> callback, Vector3 offsets)
    {
        ThreadPool.QueueUserWorkItem(delegate
        {
            TileDataThread(callback, offsets);
        });
    }

    // Generate Perlin noise map and set get texture info per biome
    private TileData GenerateHeightMap(int width, int height, Vector3 offsets)
    {
        int tileHeight = width;
        int tileWidth = height;

        int splatmapSize = tileHeight * tileWidth;

        Color[] splatMap1 = new Color[splatmapSize];
        Color[] splatMap2 = new Color[splatmapSize];
        Color[] splatMap3 = new Color[splatmapSize];
        Color[] grassMap = new Color[splatmapSize];

        float[,] heightMap = new float[width, height];

        bool hasOcean = false;

        float maxPossibleHeight = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float persistance = 0.5f;
        float lacunarity = 2f;

        int octaves = 12;
        float scale = 50.777f;
        int heightMultiplier = 15;

        Perlin perlin = new Perlin(frequency, lacunarity, persistance, octaves, TerrainGenerator.Seed, LibNoise.QualityMode.High);

        for (int i = 0; i < octaves; i++)
        {
            maxPossibleHeight += amplitude;
            amplitude *= 0.5f;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double sampleX = (x + offsets.x) / scale;
                double sampleY = (y + offsets.z) / scale;

                float noiseHeight = (float)perlin.GetValue(sampleX, 0, sampleY);

                int colorIndex = y * tileWidth + x;

                // Normalise noise map between minimum and maximum noise heights
                noiseHeight = (noiseHeight + 1) / (2f * maxPossibleHeight / 1.75f);

                // Change height based on height curve and heightMultiplier
                Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(x + offsets.x, y + offsets.z));

                splatMap1[colorIndex] = biome.BiomeType.Color;
                splatMap2[colorIndex] = biome.BiomeType.Color2;
                splatMap3[colorIndex] = biome.BiomeType.Color3;

                if (biome.BiomeType is OceanBiomeType)
                {
                    hasOcean = true;
                }

                if (biome.BiomeType is ForestBiomeType ||
                    biome.BiomeType is PlainsBiomeType ||
                    biome.BiomeType is ShrublandBiomeType)
                { grassMap[colorIndex] = new Color(1, 0, 0); }
                else { grassMap[colorIndex] = new Color(0, 1, 0); }

                noiseHeight = biome.BiomeType.HeightCurve.Evaluate(noiseHeight) * heightMultiplier;
                heightMap[x, y] = noiseHeight;
            }
        }

        TileData tileData = new TileData
        {
            splatMap1 = splatMap1,
            splatMap2 = splatMap2,
            splatMap3 = splatMap3,
            offsets = offsets,
            hasOcean = hasOcean,
            grassMap = grassMap,
            heightMap = heightMap
        };
        return tileData;
    }

    public MeshData GenerateMesh(int levelOfDetail, float[,] heightMap = null, bool isOcean = false)
    {
        int size = WorldBuilder.CHUNK_SIZE + 1;
        float topLeft = (size - 1) / 2f;

        // Must be divisible by WorldBuilder.CHUNK_SIZE
        int meshSimplificationIncrement = levelOfDetail;
        int verticesPerLine = (size - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < size; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < size; x += meshSimplificationIncrement)
            {
                float heightValue = isOcean ? 0f : heightMap[x, y];
                meshData.Vertices[vertexIndex] = new Vector3(topLeft - x, heightValue, topLeft - y);
                meshData.UVs[vertexIndex] = new Vector2(x / (float)size, y / (float)size);

                if (x < size - 1 && y < size - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }
                vertexIndex++;
            }
        }

        return meshData;
    }

    // When thread is done set all the unity specific values
    private void OnTileDataReceived(TileData tileData)
    {
        Tile currentTile = WorldBuilder.GetTile(tileData.offsets);

        // Check to see if the current tile is not already unloaded
        if (currentTile != null) 
        {
            Vector2 offsets = new Vector2(tileData.offsets.x, tileData.offsets.z);
            Vector2 cityOffsets = new Vector2(tileData.offsets.x - 5, tileData.offsets.z - 5);
            currentTile.HeightMap = tileData.heightMap;
            caveBuilder.Instantiate(tileData.offsets);

            grassMap = new Texture2D(TILE_DIMENSION, TILE_DIMENSION);
            splatmapsArray = new Texture2DArray(TILE_DIMENSION, TILE_DIMENSION, 3, TextureFormat.RGBA32, true);
            int levelOfDetail = WorldBuilder.GetTile(tileData.offsets).LevelOfDetail;

            splatmapsArray.SetPixels(tileData.splatMap1, 0);
            splatmapsArray.SetPixels(tileData.splatMap2, 1);
            splatmapsArray.SetPixels(tileData.splatMap3, 2);
            grassMap.SetPixels(tileData.grassMap);

            splatmapsArray.wrapMode = TextureWrapMode.Clamp;
            splatmapsArray.Apply();

            grassMap.wrapMode = TextureWrapMode.Clamp;
            grassMap.Apply();

            Material[] matArray = currentTile.Terrain.MeshRenderer.materials;
            matArray[0].SetTexture("_SplatMaps", splatmapsArray);
            matArray[1].SetTexture("_GrassSplatMap", grassMap);
            currentTile.Terrain.MeshRenderer.materials = matArray;

            GameObject ocean = currentTile.Ocean.GameObject;
            ocean.SetActive(tileData.hasOcean);
            MeshData meshData = GenerateMesh(levelOfDetail, tileData.heightMap, false);
            SetMesh(meshData.CreateMesh(), currentTile);
            currentTile.Terrain.GameObject.SetActive(true);

            cityGenerator.Generate(cityOffsets);

            decorationGenerator.Generate(tileData.heightMap, tileData.offsets);
        }
    }

    public void SetMesh(Mesh mesh, Tile currentTile)
    {
        currentTile.Terrain.MeshFilter.mesh = mesh;
        currentTile.Terrain.MeshCollider.sharedMesh = mesh;
    }

    public void RegenerateMesh(Tile tile)
    {
        if (tile.HeightMap != null) { SetMesh(GenerateMesh(tile.LevelOfDetail, tile.HeightMap).CreateMesh(), tile); }
    }

    private void TileDataThread(Action<TileData> callback, Vector3 offsets)
    {
        TileData tileData = GenerateHeightMap(TILE_DIMENSION, TILE_DIMENSION, offsets);
        lock (terrainDataThreadInfoQueue)
        {
            terrainDataThreadInfoQueue.Enqueue(new MapThreadInfo<TileData>(callback, tileData));
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
    private struct TileData
    {
        public float[,] heightMap;
        public Color[] splatMap1;
        public Color[] splatMap2;
        public Color[] splatMap3;
        public Color[] grassMap;
        public Vector3 offsets;
        public bool hasOcean;
    }

    void Update()
    {
        // Check to see if any of the terrainData is done being created
        if (terrainDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < terrainDataThreadInfoQueue.Count; i++)
            {
                if (i < WorldBuilder.MAX_CHUNK_PER_FRAME)
                {
                    MapThreadInfo<TileData> result;

                    if (terrainDataThreadInfoQueue.TryDequeue(out result))
                    {
                        result.callback(result.parameter);
                    }
                }
                else { break; }
            }
        }
    }
}