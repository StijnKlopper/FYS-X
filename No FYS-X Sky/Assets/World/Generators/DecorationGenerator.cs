using LibNoise.Generator;
using UnityEngine;

public class DecorationGenerator : MonoBehaviour
{
    private TerrainGenerator terrainGenerator;
    private FractalTree fractalTree;

    public float PlantThreshold;
    public float TreeThreshold;

    Perlin perlin;

    public GameObject PlantPrefab;
    // Start is called before the first frame update
    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        float frequency = 1f;
        float persistence = 1f;
        float lacunarity = 2f;
        int octaves = 1;
        perlin = new Perlin(frequency, lacunarity, persistence, octaves, terrainGenerator.Seed, LibNoise.QualityMode.High);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Generate(float[,] heightMap, Vector3 position)
    {
        bool placeTree = true;
        float scale = 50.777f;
        //if (position.x == -10 && position.z == 370)
        //{
            for (int y = 0; y < 11; y++)
            {
                for (int x = 0; x < 11; x++)
                {
                    double sampleX = (x + position.x) / scale;
                    double sampleY = (y + position.y) / scale;
                    float noiseHeight = (float)(perlin.GetValue(sampleY, 0, sampleX) + 1) / 2;
                    
                    Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(position.x + x, position.z + y));
                    if (biome.BiomeType is DefaultBiomeType || biome.BiomeType is ForestBiomeType || biome.BiomeType is PlainsBiomeType || biome.BiomeType is ShrublandBiomeType)
                    {
                       
                        if (noiseHeight >= PlantThreshold)
                        {
                            Vector3 pos = new Vector3((position.x + x), heightMap[x, y], (position.z + y));
                            transform.GetComponent<FractalTree>().GeneratePlants(pos, biome.BiomeType);
                        }
                        if (noiseHeight >= TreeThreshold && placeTree)
                        {
                            Vector3 pos = new Vector3(position.x + 5, heightMap[5, 5], position.z + 5);
                            transform.GetComponent<FractalTree>().GenerateTree(pos, biome.BiomeType);
                            placeTree = false;
                        }
                    }
                }
            }
        //}
    }
}


    
