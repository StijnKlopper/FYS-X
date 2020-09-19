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

    [System.NonSerialized]
    public float[,] biomeMap;

    [System.NonSerialized]
    public float[,] moistureMap;

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

        // Instead of generating height map:
        GenerateHeightMap(tileWidth, tileHeight, offsets);
        //GenerateMoistureMap(tileWidth, tileHeight, offsets);

        Texture2D heightTexture = BuildTexture(offsets);
    
        this.tileRenderer.material.mainTexture = heightTexture;
        UpdateMeshVertices(heightMap);
    }

    public void GenerateHeightMap(int width, int height, Vector2 offsets)
    {
        float[,] heightMap = new float[width, height];

        float maxPossibleHeight = 0f;
        float amplitude = 1f;
        float persistance = 0.5f;
        float lacunarity = 2f;

        int octaves = 12;
        int scale = 50;

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

                // Normalise noise map between minimum and maximum noise heights
                noiseHeight = (noiseHeight + 1) / (2f * maxPossibleHeight / 1.75f);

                heightMap[x, y] = noiseHeight;
            }
        }

        this.heightMap = heightMap;
    }

    private Texture2D BuildTexture(Vector2 offsets)
    {
        int tileHeight = this.heightMap.GetLength(0);
        int tileWidth = this.heightMap.GetLength(1);

        Color[] colorMap = new Color[tileHeight * tileWidth];
        for (int y = 0; y < tileHeight; y++)
        {
            for (int x = 0; x < tileWidth; x++)
            {
                int colorIndex = y * tileWidth + x;

                Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(x + offsets.x, y + offsets.y));

                colorMap[colorIndex] = biome.color;
            }
        }

        Texture2D tileTexture = new Texture2D(tileWidth, tileHeight);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }

    private void UpdateMeshVertices(float[,] heightMap)
    {
        int height = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;

        int vertexIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float heightValue = heightMap[x, y];
                Vector3 vertex = meshVertices[vertexIndex];

                float terrainHeight = this.heightCurve.Evaluate(heightValue) * 50; // TODO heightmultiplier

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