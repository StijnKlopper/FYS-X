using UnityEngine;
using LibNoise.Generator;

public class DecorationGenerator : MonoBehaviour
{
    private TerrainGenerator terrainGenerator;
    // Start is called before the first frame update
    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void generateNoiseMap(Vector2 offsets)
    {
        float[,] noiseMap = new float[10, 10];
        float frequency = 1f;
        float persistence = 1f;
        float lacunarity = 2f;

        int octaves = 1;
        float scale = 100.777f;
        Perlin perlin = new Perlin(frequency, lacunarity, persistence, octaves, terrainGenerator.Seed, LibNoise.QualityMode.High);

        for(int y = 0; y< noiseMap.Length; y++)
        {
            for(int x = 0; x < noiseMap.Length; x++)
            {
                double sampleX = (x + offsets.x) / scale;
                double sampleY = (y + offsets.y) / scale;
                float noiseHeight = (float)(perlin.GetValue(sampleY, 0, sampleX) + 1) / 2;

                // Change height based on height curve and heightMultiplier
                Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(x + offsets.x, y + offsets.y));

                // Exclude some biomes from noisemap 
                if (biome.BiomeType is OceanBiomeType || biome.BiomeType is MountainBiomeType)
                {
                    noiseMap[x, y] = 1f;
                }
                else
                {
                    noiseMap[x, y] = noiseHeight;
                }
            }
        }
    }
}
