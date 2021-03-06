﻿using LibNoise.Generator;
using UnityEngine;

public class DecorationGenerator : MonoBehaviour
{
    private TerrainGenerator terrainGenerator;

    public float PlantThreshold;
    public float TreeThreshold;
    [Range(0.01f, 1)]
    public float MarginThreshold;

    [Range(1,3)]
    public int interval;

    private Perlin perlin;

    private FractalTree fractalTree;

    // Start is called before the first frame update
    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();

        float frequency = 1f;
        float persistence = 1f;
        float lacunarity = 1f;
        int octaves = 1;
        perlin = new Perlin(frequency, lacunarity, persistence, octaves, TerrainGenerator.Seed, LibNoise.QualityMode.High);
        fractalTree = GetComponent<FractalTree>();
    }

    //Generate Decorations based on Perlin noise
    public void Generate(float[,] heightMap, Vector3 position)
    {
        Tile tile = WorldBuilder.GetTile(position);
        bool placeTree = true;
        float scale = 0.17777f;
       
        for (int y = 0; y < 11; y = y + interval)
        {
            for (int x = 0; x < 11; x = x + interval)
            {
                double sampleX = (x + position.x) / scale;
                double sampleY = (y + position.z) / scale;               
                float noiseHeight = (float)(perlin.GetValue(sampleY, 0, sampleX) + 1) / 2;
                
                Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(position.x + x, position.z + y));
                if (biome.BiomeType is DefaultBiomeType || biome.BiomeType is ForestBiomeType || biome.BiomeType is PlainsBiomeType || biome.BiomeType is ShrublandBiomeType)
                {
                    if (noiseHeight >= PlantThreshold && noiseHeight <= PlantThreshold + MarginThreshold)
                    {
                        Vector3 pos = new Vector3((position.x + x), heightMap[x, y], (position.z + y));
                        tile.AddDecoration(fractalTree.GeneratePlants(pos, biome.BiomeType));
                    }
                    if (noiseHeight >= TreeThreshold && placeTree && noiseHeight <= TreeThreshold + MarginThreshold)
                    {
                        int jitterValue = Mathf.RoundToInt((float)(perlin.GetValue(sampleY + terrainGenerator.RandomNumbers[x], 0, sampleX + terrainGenerator.RandomNumbers[y])) * 5);
                        // Prevent jitterValue to exceed index elemends
                        if(jitterValue > 5 || jitterValue < -5)
                        {
                            jitterValue = 5;
                        }
                        Vector3 pos = new Vector3(position.x + 5 + jitterValue, heightMap[5 + jitterValue, 5 + jitterValue], position.z + 5 + jitterValue);
                        // Check Biome again for Tree Placement
                        Biome biome2 = terrainGenerator.GetBiomeByCoordinates(new Vector2(pos.x, pos.z));
                        if (biome2.BiomeType is DefaultBiomeType || biome2.BiomeType is ForestBiomeType || biome2.BiomeType is PlainsBiomeType || biome2.BiomeType is ShrublandBiomeType)
                        {
                            tile.AddDecoration(fractalTree.GenerateTree(pos, biome.BiomeType));
                            placeTree = false;
                        }
                    }
                }
            }
        }
        
    }
}


    
