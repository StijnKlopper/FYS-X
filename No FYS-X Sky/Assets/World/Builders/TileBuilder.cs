using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEditor;
using UnityEngine;

public class TileBuilder : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer tileRenderer;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    public AnimationCurve heightCurve;

    [System.NonSerialized]
    public float[,] heightMap;

    TerrainGenerator terrainGenerator;

    float[] tileTextureData;

    // Start is called before the first frame update
    void Start()
    {
        //textureData = (TextureData)AssetDatabase.LoadAssetAtPath("Assets/TextureDataSet.asset", typeof(TextureData));
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        GenerateTile();

    }

    private void GenerateTile()
    {
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileHeight = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileHeight;
      
        Vector2 offsets = new Vector2(-this.gameObject.transform.position.x, -this.gameObject.transform.position.z);

        // Instead of generating height map:
        GenerateHeightMap(tileWidth, tileHeight, offsets);

        //BiomeTextureSelection(offsets);

        Debug.Log(tileTextureData);

        //terrainGenerator.textureData.setBiomeIndex(this.tileRenderer.material, tileTextureData);


        //GenerateMoistureMap(tileWidth, tileHeight, offsets);
        //terrainGenerator.textureData.

        //BiomeTextureSelection(offsets);


        UpdateMeshVertices(heightMap, offsets);
    }

    public void GenerateHeightMap(int width, int height, Vector2 offsets)
    {
        float[,] heightMap = new float[width, height];

        tileTextureData = new float[width * height];


        float maxPossibleHeight = 0f;
        float amplitude = 1f;
        float persistance = 0.5f;
        float lacunarity = 2f;

        int octaves = 12;
        int scale = 50;
        int heightMultiplier = 10;

        int p = 0;

        int[] randomNumbersArray = terrainGenerator.randomNumbers;

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
                    float sampleX = (x + offsets.x) / scale * frequency + randomNumbersArray[i];
                    float sampleY = (y + offsets.y) / scale * frequency + randomNumbersArray[i];

                    // Because we * 2 - 1 this value, we stretch out the noise from [0,1] to [-1,1]
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    // Add octave value to the perlin noise once for every octave
                    noiseHeight += perlinValue * amplitude;

                    // Amplitude decreases every octave
                    amplitude *= persistance;

                    // Frequency increases every octave
                    frequency *= lacunarity;

                }
                
                // Normalise noise map between minimum and maximum noise heights
                noiseHeight = (noiseHeight + 1) / (2f * maxPossibleHeight / 1.75f);

                // Change height based on height curve and heightMultiplier
                Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(x + offsets.x, y + offsets.y));

                tileTextureData[x + y * height] = biome.biomeType.textureIndex;

                noiseHeight = biome.biomeType.heightCurve.Evaluate(noiseHeight) * heightMultiplier;
                heightMap[x, y] = noiseHeight;
            }
        }
        this.heightMap = heightMap;
    }

    private void UpdateMeshVertices(float[,] heightMap, Vector2 offsets)
    {
        int height = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        Vector2[] uvs = new Vector2[meshVertices.Length];

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

        int p = uvs.Length -1;
        int prevTextureIndex = 0;
        float drawStrength = 0;

        for (int i = 0; i < uvs.Length; i++) 
        {
            

            Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(meshVertices[p].x + offsets.x + 5, meshVertices[p].z + offsets.y + 5));

            if (prevTextureIndex != biome.biomeType.textureIndex)
            {
                drawStrength = -1;
            }
            else { drawStrength = 1; }

            uvs[i] = new Vector2(drawStrength, biome.biomeType.textureIndex);
            p--;
        }

        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();

        this.meshFilter.mesh.uv = uvs;

        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }



}