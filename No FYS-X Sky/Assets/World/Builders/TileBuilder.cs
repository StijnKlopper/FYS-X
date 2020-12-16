﻿using CielaSpike;
using System.Collections;
using LibNoise.Generator;
using UnityEngine;

public class TileBuilder : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer;

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
        Vector2 offsets = new Vector2(this.gameObject.transform.position.x - 5, this.gameObject.transform.position.z - 5);
        Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        float xzDistance = Vector2.Distance(offsets, new Vector2(playerPos.x, playerPos.z));
        Debug.Log(xzDistance);

        // Instead of generating height map:
        GenerateHeightMap(offsets);

        // Calculate levelOfDetail with offsets and player position

        int levelOfDetail = 1;
        MeshData meshData = GenerateMesh(levelOfDetail, this.heightMap);
        Mesh mesh = meshData.CreateMesh();

        this.meshFilter.mesh = mesh;
        this.meshCollider.sharedMesh = mesh;
        this.meshRenderer.material.SetTexture("_SplatMaps", splatmapsArray);

        GameObject ocean = this.transform.GetChild(0).gameObject;
        ocean.SetActive(hasOcean);

        Mesh oceanMesh = GenerateMesh(levelOfDetail, null, true).CreateMesh();
        
        MeshFilter oceanMeshFilter = ocean.GetComponent<MeshFilter>();
        oceanMeshFilter.mesh = oceanMesh;

        MeshCollider oceanMeshCollider = ocean.GetComponent<MeshCollider>();
        oceanMeshCollider.sharedMesh = oceanMesh;

        ocean.GetComponent<MeshRenderer>().material.SetTexture("_OceanSplatmap", oceanSplatmap);
        ocean.transform.position = new Vector3(this.gameObject.transform.position.x, 0, this.gameObject.transform.position.z);
        yield return null;
    }


    public void GenerateHeightMap(Vector2 offsets)
    {
        int tileSize = WorldBuilder.chunkSize + 1;
        int splatmapSize = tileSize * tileSize;

        Color[] splatMap1 = new Color[splatmapSize];
        Color[] splatMap2 = new Color[splatmapSize];
        Color[] splatMap3 = new Color[splatmapSize];

        Color[] oceanMap = new Color[splatmapSize];

        float[,] heightMap = new float[tileSize, tileSize];

        tileTextureData = new float[tileSize * tileSize];

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

        for (int y = 0; y < tileSize; y++)
        {
            for (int x = 0; x < tileSize; x++)
            {
                
                double sampleX = (x + offsets.x) / scale;
                double sampleY = (y + offsets.y) / scale;

                float noiseHeight = (float) perlin.GetValue(sampleX, 0, sampleY);

                int colorIndex = y * tileSize + x;

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

                tileTextureData[x + y * tileSize] = biome.biomeType.biomeTypeId;

                noiseHeight = biome.biomeType.heightCurve.Evaluate(noiseHeight) * heightMultiplier;
                heightMap[x, y] = noiseHeight;
            }
        }
        splatmapsArray = new Texture2DArray(tileSize, tileSize, 3, TextureFormat.RGBA32, true);
        oceanSplatmap = new Texture2D(tileSize, tileSize);

        splatmapsArray.SetPixels(splatMap1, 0);
        splatmapsArray.SetPixels(splatMap2, 1);
        splatmapsArray.SetPixels(splatMap3, 2);

        oceanSplatmap.SetPixels(oceanMap);
        oceanSplatmap.wrapMode = TextureWrapMode.Clamp;
        oceanSplatmap.Apply();

        splatmapsArray.wrapMode = TextureWrapMode.Clamp;
        splatmapsArray.Apply();

        this.heightMap = heightMap;
    }

    private MeshData GenerateMesh(int levelOfDetail, float[,] heightMap = null, bool isOcean = false)
    {
        int size = WorldBuilder.chunkSize + 1;
        float topLeft = (size - 1) / 2f;

        // Must be divisible by WorldBuilder.chunkSize
        int meshSimplificationIncrement = 1;
        int verticesPerLine = (size - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < size; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < size; x += meshSimplificationIncrement)
            {
                float heightValue = isOcean ? 0f : heightMap[x, y];

                meshData.vertices[vertexIndex] = new Vector3(topLeft - x, heightValue, topLeft - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)size, y / (float)size);

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
}
public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = c;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = a;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }

}