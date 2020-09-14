using Assets.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    // Configurable fields (in Unity)
    [SerializeField]
    private MeshRenderer tileRenderer;

    [SerializeField]
    public MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    [Header("Noisemap Settings")]
    public float noiseScale = 10f;

    [Range (0, 20)]
    public int octaves = 3;

    [Range(0, 1)]
    public float persistance = 0.5f;

    public float lacunarity = 2f;

    public float heightMultiplier;

    public AnimationCurve heightCurve;

    [System.Serializable]
    public class TerrainType
    {
        public string name;
        public float threshold;
        public Color color;
    }

    [SerializeField]
    private TerrainType[] terrainTypes;

    [SerializeField]
    private TerrainType[] heightTerrainTypes;

    [SerializeField]
    private TerrainType[] heatTerrainTypes;

    enum VisualizationMode { Height, Heat }

    [SerializeField]
    private VisualizationMode visualizationMode;

    TerrainGenerator terrainGenerator;

    // Start is called before the first frame update
    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        GenerateTileNew();
    }

    private void GenerateTileNew()
    {
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileHeight = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileHeight;
      
        Vector2 offsets = new Vector2(-this.gameObject.transform.position.x, -this.gameObject.transform.position.z);

        float[,] heightMap = GenerateNoiseMap(tileWidth, tileHeight, noiseScale , octaves, persistance, lacunarity, offsets);

        Texture2D heightTexture = BuildTexture(heightMap, this.terrainTypes);
    
        this.tileRenderer.material.mainTexture = heightTexture;
        UpdateMeshVertices(heightMap);
    }

    public float[,] GenerateNoiseMap(int width, int height, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noisemap = new float[width, height];

        Vector2[] octaveValues = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        // Use pregenerated random numbers to populate octaveValues array so the random numbers used are the same for each tile
        for (int i = 0; i < octaves; i++)
        {
            float valueX = terrainGenerator.randomNumbers[i];
            float valueY = terrainGenerator.randomNumbers[i];
            octaveValues[i] = new Vector2(valueX, valueY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        // Scale can not be negative, using range is not great because it can be a large number
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        // Loop through all coordinates on the tile, for every coordinate calculate a height value using octaves
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    // Add 10000 to the sample coordinates to prevent feeding negative numbers into the Perlin Noise function
                    // Prevents the mandela effect around (0,0)
                    float sampleX = (x + offset.x) / scale * frequency + octaveValues[i].x;
                    float sampleY = (y + offset.y) / scale * frequency + octaveValues[i].y;

                    // Because we * 2 - 1 this value, we stretch out the noise from [0,1] to [-1,1]
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    // Add octave value to the perlin noise once for every octave
                    noiseHeight += perlinValue * amplitude;

                    // Amplitude decreases every octave
                    amplitude *= persistance;

                    // Frequency increases every octave
                    frequency *= lacunarity;
                }
                
                // Set max and min noise height if changed
                if (noiseHeight > terrainGenerator.maxNoiseHeight)
                {
                    terrainGenerator.maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < terrainGenerator.minNoiseHeight)
                {
                    terrainGenerator.minNoiseHeight = noiseHeight;
                }

                noisemap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //Normalise noise map between current minimum and maximum noise heights
                float normalizedHeight =(noisemap[x,y] + 1) / (2f * maxPossibleHeight / 1.75f);
                noisemap[x, y] = normalizedHeight;
            }
        }

        return noisemap;
    }

    private Texture2D BuildTexture(float[,] heightMap, TerrainType[] terrainTypes)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        Color[] colorMap = new Color[tileDepth * tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                int colorIndex = zIndex * tileWidth + xIndex;
                float height = heightMap[xIndex, zIndex];
                TerrainType terrainType = ChooseTerrainType(height, terrainTypes);
                colorMap[colorIndex] = terrainType.color;
            }
        }

        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }

    TerrainType ChooseTerrainType(float noise, TerrainType[] terrainTypes)
    {
        foreach (TerrainType terrainType in terrainTypes)
        {
            
            if (noise < terrainType.threshold)
            {
                return terrainType;
            }
        }
        return terrainTypes[terrainTypes.Length - 1];
    }

    private void UpdateMeshVertices(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;

        int vertexIndex = 0;

        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                float height = heightMap[xIndex, zIndex];
                Vector3 vertex = meshVertices[vertexIndex];
                float terrainHeight = this.heightCurve.Evaluate(height) * this.heightMultiplier;

                meshVertices[vertexIndex] = new Vector3(vertex.x, terrainHeight, vertex.z);

                vertexIndex++;
            }
        }

        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();

        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }

    private void OnValidate()
    {
        if (octaves < 0) octaves = 1;
        if (lacunarity < 1) lacunarity = 1;
    }

}
