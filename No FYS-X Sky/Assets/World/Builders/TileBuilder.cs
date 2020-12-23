using CielaSpike;
using System.Collections;
using LibNoise.Generator;
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

public class TileBuilder : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer tileRenderer;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    [System.NonSerialized]
    public float[,] heightMap;

    public AnimationCurve heightCurve;

    TerrainGenerator terrainGenerator;

    float[] tileTextureData;

    GameObject oceanTile;

    private Texture2DArray splatmapsArray;

    private Texture2D oceanSplatmap;

    private bool hasOcean;

    // Start is called before the first frame update

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();

/*    public float[,] Instantiate() {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        hasOcean = false;
        GenerateTile();
        return heightMap;
    }*/

    public void RequestMapData(Action<MapData> callback)
    {
        Vector2 offsets = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z);
        ThreadStart threadStart = delegate
        {
            MapDataThread(callback, offsets);
        };
        new Thread(threadStart).Start();
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

    void MapDataThread(Action<MapData> callback, Vector2 offsets)
    {

        MapData mapData = GenerateHeightMap(11, 11, offsets);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public struct MapData
    {
        public float[,] heightMap;
        public Color[] splatMap1;
        public Color[] splatMap2;
        public Color[] splatMap3;
        public Color[] oceanMap;

        public MapData(float[,] heightMap, Color[] splatMap1, Color[] splatMap2, Color[] splatMap3, Color[] oceanMap)
        {
            this.heightMap = heightMap;
            this.splatMap1 = splatMap1;
            this.splatMap2 = splatMap2;
            this.splatMap3 = splatMap3;
            this.oceanMap = oceanMap;
        }
    }

    public void GenerateTile()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        hasOcean = false;
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileHeight = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileHeight;

        //RequestMapData
        Vector2 offsets = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z);

        RequestMapData(OnMapDataReceived);
    }

    public void OnMapDataReceived(MapData mapData)
    {
        Vector2 offsets = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z);
        splatmapsArray = new Texture2DArray(11, 11, 3, TextureFormat.RGBA32, true);
        oceanSplatmap = new Texture2D(11, 11);

        
        splatmapsArray.SetPixels(mapData.splatMap1, 0);
        splatmapsArray.SetPixels(mapData.splatMap2, 1);
        splatmapsArray.SetPixels(mapData.splatMap3, 2);

        oceanSplatmap.SetPixels(mapData.oceanMap);
        oceanSplatmap.wrapMode = TextureWrapMode.Clamp;
        oceanSplatmap.Apply();

        splatmapsArray.wrapMode = TextureWrapMode.Clamp;
        splatmapsArray.Apply();

        this.tileRenderer.material.SetTexture("_SplatMaps", splatmapsArray);
        UpdateMeshVertices(mapData.heightMap, offsets);

        GameObject ocean = this.transform.GetChild(0).gameObject;
        ocean.SetActive(hasOcean);
        ocean.GetComponent<MeshRenderer>().material.SetTexture("_OceanSplatmap", oceanSplatmap);
        ocean.transform.position = new Vector3(this.gameObject.transform.position.x, 0, this.gameObject.transform.position.z);
    }


    public MapData GenerateHeightMap(int width, int height, Vector2 offsets)
    {
        int tileHeight = width;
        int tileWidth = height;

        int splatmapSize = tileHeight * tileWidth;

        
        Color[] splatMap1 = new Color[splatmapSize];
        Color[] splatMap2 = new Color[splatmapSize];
        Color[] splatMap3 = new Color[splatmapSize];

        Color[] oceanMap = new Color[splatmapSize];

        float[,] heightMap = new float[width, height];

        tileTextureData = new float[width * height];

        float maxPossibleHeight = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float persistance = 0.5f;
        float lacunarity = 2f;

        int octaves = 12;
        float scale = 50.777f;
        int heightMultiplier = 15;

        Perlin perlin = new Perlin(frequency, lacunarity, persistance, octaves, terrainGenerator.seed, LibNoise.QualityMode.High);

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
                double sampleY = (y + offsets.y) / scale;

                float noiseHeight = (float) perlin.GetValue(sampleX, 0, sampleY);

                int colorIndex = y * tileWidth + x;

                // Normalise noise map between minimum and maximum noise heights
                noiseHeight = (noiseHeight + 1) / (2f * maxPossibleHeight / 1.75f);

                // Change height based on height curve and heightMultiplier
                Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(x + offsets.x, y + offsets.y));

                splatMap1[colorIndex] = biome.biomeType.color;
                splatMap2[colorIndex] = biome.biomeType.color2;
                splatMap3[colorIndex] = biome.biomeType.color3;

                if (biome.biomeType is OceanBiomeType)
                {
                    oceanMap[colorIndex] = new Color(1, 0, 0);
                    hasOcean = true;
                }

                else { oceanMap[colorIndex] = new Color(0, 1, 0); }

                oceanMap[colorIndex] = biome.biomeType is OceanBiomeType ? new Color(1, 0, 0) : new Color(0, 1, 0);

                AnimationCurve heightCurve = new AnimationCurve(biome.biomeType.heightCurve.keys);

                noiseHeight = heightCurve.Evaluate(noiseHeight) * heightMultiplier;



                //noiseHeight = UnityEngine.Random.Range(-10.0f, 10.0f);
                //noiseHeight = 1;
                heightMap[x, y] = noiseHeight;
            }
        }

        MapData mapData = new MapData();

        mapData.splatMap1 = splatMap1;
        mapData.splatMap2 = splatMap2;
        mapData.splatMap3 = splatMap3;
        mapData.oceanMap = oceanMap;

        //WorldBuilder.tileDict[new Vector3(-(offsets.x + 5), 0, -(offsets.y + 5))].heightMap = heightMap;
        //this.heightMap = heightMap;

        mapData.heightMap = heightMap;
        return mapData;
    }

    private void UpdateMeshVertices(float[,] heightMap, Vector2 offsets)
    {
        int height = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;

        int vertexIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 vertex = meshVertices[vertexIndex];

                meshVertices[vertexIndex] = new Vector3(vertex.x, heightMap[x, y], vertex.z);

                vertexIndex++;
            }
        }

        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();

        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }
    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    void OnDestroy()
    {
        Destroy(oceanTile);
    }

}