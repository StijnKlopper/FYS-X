using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region
{
    public static int regionSize = 400;

    public Vector2 seed;

    public Biome biome;
    
    public Region(int x, int z)
    {
        TerrainGenerator terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();

        int xMiddle = x < 0 ? x - regionSize / 2 : x + regionSize / 2;
        int zMiddle = z < 0 ? z - regionSize / 2 : x + regionSize / 2;

        float scale = 0.15f;

        // Calculate noise for middle of the region, adding a random number to account for negative numbers, * 2 - 1 to spread output to range [-1,1]
        float xNoise = Mathf.PerlinNoise((xMiddle + terrainGenerator.randomNumbers[1]) * scale, (zMiddle + terrainGenerator.randomNumbers[1]) * scale) * 2 - 1;
        int xJitter = (int)(xNoise * regionSize / 2);
        int xFinal = x + xJitter;

        float zNoise = Mathf.PerlinNoise((xMiddle + terrainGenerator.randomNumbers[1]) * scale, (zMiddle + terrainGenerator.randomNumbers[1]) * scale) * 2 - 1;
        int zJitter = (int)(zNoise * regionSize / 2);
        int zFinal = z + zJitter;

        float biomeNoise = Mathf.PerlinNoise(xFinal * scale, zFinal * scale);
        this.biome = terrainGenerator.GetBiomeByNoise(biomeNoise);

        this.seed = new Vector2(xFinal, zFinal);
    }
}
