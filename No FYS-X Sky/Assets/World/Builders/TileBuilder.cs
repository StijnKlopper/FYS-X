using Assets.World;
using Assets.World.Generator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

public class TileBuilder : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer tileRenderer;

    [SerializeField]
    public MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    public AnimationCurve heightCurve;

    [System.NonSerialized]
    public float[,] heightMap;

    [System.NonSerialized]
    public float[,] biomeMap;



    enum VisualizationMode { Height, Heat }

    [SerializeField]
    private VisualizationMode visualizationMode;

    TerrainGenerator terrainGenerator;

    // Start is called before the first frame update
    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        GenerateTile();
    }

    private void GenerateTile()
    {
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileHeight = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileHeight;
      
        Vector2 offsets = new Vector2(-this.gameObject.transform.position.x, -this.gameObject.transform.position.z);
        GenerateBiomeMap(tileWidth, tileHeight, offsets);
        GenerateNoiseMap(tileWidth, tileHeight, offsets);

        Texture2D heightTexture = BuildTexture();
    
        this.tileRenderer.material.mainTexture = heightTexture;
        UpdateMeshVertices(heightMap);
    }

    public void GenerateNoiseMap(int width, int height, Vector2 offset)
    {
        float[,] noisemap = new float[width, height];

        // Loop through all coordinates on the tile, for every coordinate calculate a height value using octaves
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                Biome biome = terrainGenerator.GetBiomeByBiomeValue(this.biomeMap[x, y]);

                for (int i = 0; i < biome.octaves; i++)
                {
                    // Add 10000 to the sample coordinates to prevent feeding negative numbers into the Perlin Noise function
                    // Prevents the mandela effect around (0,0)
                    float sampleX = (x + offset.x) / biome.noiseScale * frequency + terrainGenerator.randomNumbers[i];
                    float sampleY = (y + offset.y) / biome.noiseScale * frequency + terrainGenerator.randomNumbers[i];

                    // Because we * 2 - 1 this value, we stretch out the noise from [0,1] to [-1,1]
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    // Add octave value to the perlin noise once for every octave
                    noiseHeight += perlinValue * amplitude;

                    // Amplitude decreases every octave
                    amplitude *= biome.persistance;

                    // Frequency increases every octave
                    frequency *= biome.lacunarity;
                }

                // Normalise noise map between current minimum and maximum noise heights
                noisemap[x, y] = (noiseHeight + 1) / (2f * biome.GetMaxPossibleHeight() / 1.75f);
            }
        }
        this.heightMap = noisemap;
    }

    private void GenerateBiomeMap(int width, int height, Vector2 offsets)
    {
        float[,] biomeMap = new float[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Scale out the values of the Biomes Noise map. 
                // This way changes between values are smaller and make the same biomes value stick more together
                float sampleX = ((x + offsets.x) / 3000 + 100000);
                float sampleY = ((y + offsets.y) / 3000 + 100000);
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                biomeMap[x, y] = perlinValue;
            }
        }
        this.biomeMap = biomeMap;
    }

    private Texture2D BuildTexture()
    {
        int tileDepth = this.biomeMap.GetLength(0);
        int tileWidth = this.biomeMap.GetLength(1);

        Color[] colorMap = new Color[tileDepth * tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                Biome biome = terrainGenerator.GetBiomeByBiomeValue(this.biomeMap[xIndex,zIndex]);
                int colorIndex = zIndex * tileWidth + xIndex;

                float height = this.heightMap[xIndex, zIndex];
                colorMap[colorIndex] = ChooseTerrainType(height, biome);
            }
        }

        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }

    Color ChooseTerrainType(float noise, Biome biome)
    {
        foreach (var terrainTypes in biome.terrainThreshold)
        {
            if (noise < terrainTypes.Key)
            {
                return terrainTypes.Value;
            }
        }
        return Color.white;
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
                float terrainHeight = this.heightCurve.Evaluate(height) * 50; // TODO heightmultiplier

                meshVertices[vertexIndex] = new Vector3(vertex.x, terrainHeight, vertex.z);

                vertexIndex++;
            }
        }
        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();

        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }

}