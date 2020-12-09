﻿using CielaSpike;
using System.Collections;
using UnityEngine;

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


    public float[,] Instantiate() {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        hasOcean = false;
        StartCoroutine("GenerateTile");
        return heightMap;
    }

    private IEnumerator GenerateTile()
    {
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileHeight = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileHeight;

        Vector2 offsets = new Vector2(-this.gameObject.transform.position.x, -this.gameObject.transform.position.z);

        // Instead of generating height map:
        GenerateHeightMap(tileWidth, tileHeight, offsets);

        this.tileRenderer.material.SetTexture("_SplatMaps", splatmapsArray);

        UpdateMeshVertices(heightMap, offsets);

        GameObject ocean = this.transform.GetChild(0).gameObject;
        ocean.SetActive(hasOcean);
        ocean.GetComponent<MeshRenderer>().material.SetTexture("_OceanSplatmap", oceanSplatmap);
        ocean.transform.position = new Vector3(this.gameObject.transform.position.x, 0, this.gameObject.transform.position.z);
        yield return null;
    }


    public void GenerateHeightMap(int width, int height, Vector2 offsets)
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
        float amplitude = 1f;
        float persistance = 0.5f;
        float lacunarity = 2f;

        int octaves = 12;
        int scale = 50;
        int heightMultiplier = 10;

        for (int i = 0; i < octaves; i++)
        {
            maxPossibleHeight += amplitude;
            amplitude *= 0.5f;
        }

        // Loop through all coordinates on the tile, for every coordinate calculate a height value using octaves
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                // Needs to ask terraingenerator what the biome is using offsets + x / y
                //Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(offsets.x + x, offsets.y + y));

                for (int i = 0; i < octaves; i++)
                {
                    // Add large number to the sample coordinates to prevent feeding negative numbers into the Perlin Noise function
                    // Prevents the mandela effect around (0,0)
                    float sampleX = (x + offsets.x) / scale * frequency + terrainGenerator.randomNumbers[i];
                    float sampleY = (y + offsets.y) / scale * frequency + terrainGenerator.randomNumbers[i];

                    // Because we * 2 - 1 this value, we stretch out the noise from [0,1] to [-1,1]
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    // Add octave value to the perlin noise once for every octave
                    noiseHeight += perlinValue * amplitude;

                    // Amplitude decreases every octave
                    amplitude *= persistance;

                    // Frequency increases every octave
                    frequency *= lacunarity;

                }

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

                tileTextureData[x + y * height] = biome.biomeType.biomeTypeId;

                noiseHeight = biome.biomeType.heightCurve.Evaluate(noiseHeight) * heightMultiplier;
                heightMap[x, y] = noiseHeight;
            }
        }

        splatmapsArray = new Texture2DArray(tileWidth, tileHeight, 3, TextureFormat.RGBA32, true);
        oceanSplatmap = new Texture2D(tileHeight, tileWidth);

        splatmapsArray.SetPixels(splatMap1, 0);
        splatmapsArray.SetPixels(splatMap2, 1);
        splatmapsArray.SetPixels(splatMap3, 2);

        oceanSplatmap.SetPixels(oceanMap);
        oceanSplatmap.wrapMode = TextureWrapMode.Clamp;
        oceanSplatmap.Apply();

        splatmapsArray.wrapMode = TextureWrapMode.Clamp;
        splatmapsArray.Apply();

        

        //WorldBuilder.tileDict[new Vector3(-(offsets.x + 5), 0, -(offsets.y + 5))].heightMap = heightMap;
        this.heightMap = heightMap;
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

    void OnDestroy()
    {
        Destroy(oceanTile);
    }

}