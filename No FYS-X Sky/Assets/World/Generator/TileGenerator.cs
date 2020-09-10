using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{

    [SerializeField]
    private MeshRenderer tileRenderer;

    [SerializeField]
    public MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    [SerializeField]
    private float mapScale;

    [SerializeField]
    private float heightMultiplier;

    [SerializeField]
    private AnimationCurve heightCurve;

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

    // Start is called before the first frame update
    void Start()
    {

    }


    public static float[,] generateNoiseMapNew(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noisemap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);

        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }
        float halfwidth = mapWidth / 2;
        float halfheight = mapHeight / 2;
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 1;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x-halfwidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y-halfwidth) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                if(noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                noisemap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                //Normalise noise map
                noisemap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noisemap[x, y]);
            }

        }

        return noisemap;
    }

    // Deprecated, kept in for viewing
    private void GenerateTile(float centerVertexZ, float maxDistanceZ)
    {
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileDepth;

        float offsetX = -this.gameObject.transform.position.x;
        float offsetZ = -this.gameObject.transform.position.z;       

        float[,] heightMap = GeneratePerlinNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ);

        Vector3 tileDimensions = this.meshFilter.mesh.bounds.size;
        float distanceBetweenVertices = tileDimensions.z / (float)tileDepth;
        float vertexOffsetZ = this.gameObject.transform.position.z / distanceBetweenVertices;

        float[,] uniformHeatMap = this.GenerateUniformNoiseMap(tileDepth, tileWidth, centerVertexZ, maxDistanceZ, vertexOffsetZ);

        float[,] randomHeatMap = this.GeneratePerlinNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ);

        float[,] heatMap = new float[tileDepth, tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                // mix both heat maps together by multiplying their values
                heatMap[zIndex, xIndex] = uniformHeatMap[zIndex, xIndex] * randomHeatMap[zIndex, xIndex];
                // makes higher regions colder, by adding the height value to the heat map
                heatMap[zIndex, xIndex] += heightMap[zIndex, xIndex] * heightMap[zIndex, xIndex];


            }
        }

        Texture2D heightTexture = BuildTexture(heightMap, this.heightTerrainTypes);
        Texture2D heatTexture = BuildTexture(heatMap, this.heatTerrainTypes);

        switch (this.visualizationMode)
        {
            case VisualizationMode.Height:
                this.tileRenderer.material.mainTexture = heightTexture;
                break;
            case VisualizationMode.Heat:
                this.tileRenderer.material.mainTexture = heatTexture;
                break;

        }

        UpdateMeshVertices(heightMap);
    }

    private float[,] GeneratePerlinNoiseMap(int mapDepth, int mapWidth, float scale, float offsetX, float offsetZ)
    {
        float[,] noiseMap = new float[mapDepth, mapWidth];

        for (int zIndex = 0; zIndex < mapDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < mapWidth; xIndex++)
            {
                float sampleX = (xIndex + offsetX) / scale;
                float sampleZ = (zIndex + offsetZ) / scale;


                //float noise =   1f * Mathf.PerlinNoise(1 * sampleX, 1 * sampleZ)
                //                + 0.5f * Mathf.PerlinNoise(2 * sampleX, 2 * sampleZ)
                //                + 0.25f * Mathf.PerlinNoise(7 * sampleX, 7 * sampleZ);
                float noise = Mathf.PerlinNoise(sampleX, sampleZ);

                noiseMap[zIndex, xIndex] = noise;
            }
        }
        return noiseMap;
    }

    private float[,] GenerateUniformNoiseMap(int mapDepth, int mapWidth, float maxDistanceZ, float centerVertexZ, float offsetZ)
    {
        float[,] noiseMap = new float[mapDepth, mapWidth];

        for (int zIndex = 0; zIndex < mapDepth; zIndex++)
        {
            float sampleZ = zIndex + offsetZ;

            float noise = Mathf.Abs(sampleZ - centerVertexZ) / maxDistanceZ;


            for (int xIndex = 0; xIndex < mapWidth; xIndex++)
            {
                noiseMap[mapDepth - zIndex - 1, xIndex] = noise;
            }
        }

        return noiseMap;
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
                float height = heightMap[zIndex, xIndex];
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
                float height = heightMap[zIndex, xIndex];
                Vector3 vertex = meshVertices[vertexIndex];
                meshVertices[vertexIndex] = new Vector3(vertex.x, this.heightCurve.Evaluate(height) * this.heightMultiplier, vertex.z);

                vertexIndex++;
            }
        }

        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();

        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }

}
