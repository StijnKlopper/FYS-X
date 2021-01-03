using System.Collections.Generic;
using UnityEngine;

public class Region
{
    public const int REGION_SIZE = 800;

    private const float SCALE = 0.0067f;

    public Vector2 Seed;

    public RegionType RegionType;

    public List<Biome> BiomeList = new List<Biome>();

    private TerrainGenerator terrainGenerator;

    public Region()
    {
        // Used for default assignment;
    }

    public Region(int x, int z)
    {
        this.terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();

        int xMiddle = x + (REGION_SIZE / 2);
        int zMiddle = z + (REGION_SIZE / 2);

        // Calculate noise for middle of the region, adding a random number to account for negative numbers, * 2 - 1 to spread output to range [-1,1]
        float xNoise = (float)terrainGenerator.Perlin.GetValue((xMiddle + terrainGenerator.RandomNumbers[1]) * SCALE, 0, (zMiddle + terrainGenerator.RandomNumbers[1]) * SCALE);
        int xJitter = (int)(xNoise * REGION_SIZE / 4);
        int xFinal = xMiddle + xJitter;

        float zNoise = (float)terrainGenerator.Perlin.GetValue((xMiddle + terrainGenerator.RandomNumbers[1]) * SCALE, 0, (zMiddle + terrainGenerator.RandomNumbers[1]) * SCALE);
        int zJitter = (int)(zNoise * REGION_SIZE / 4);
        int zFinal = zMiddle + zJitter;

        // Calculate heat and moisture to determine region type, + 1 / 2 to spread the result between [0,1] instead of [-1,1]
        float heat = (float)(terrainGenerator.Perlin.GetValue(xFinal + terrainGenerator.RandomNumbers[1] * SCALE, 0, zFinal + terrainGenerator.RandomNumbers[1] * SCALE) + 1) / 2;
        float moisture = (float)(terrainGenerator.Perlin.GetValue(xFinal + terrainGenerator.RandomNumbers[0] * SCALE, 0, zFinal + terrainGenerator.RandomNumbers[0] * SCALE) + 1) / 2;

        this.Seed = new Vector2(xFinal, zFinal);

        this.RegionType = GetRegionType(heat, moisture);

        GenerateBiomes(x, z);
    }

    private RegionType GetRegionType(float heat, float moisture)
    {
        if ((heat < 0.1 || heat > 0.9) && (moisture < 0.1 || moisture > 0.9)) return new OceanRegionType();

        if (heat < 0.5)
        {
            if (moisture < 0.5)
            {
                return new ColdDryRegionType();
            }
            else
            {
                return new ColdWetRegionType();
            }
        }
        else
        {
            if (moisture < 0.5)
            {
                return new HotDryRegionType();
            }
            else
            {
                return new HotWetRegionType();
            }
        }
    }

    private void GenerateBiomes(int x, int z)
    {
        int biomeSize = REGION_SIZE / 10;
        for (int i = x; i < x + REGION_SIZE; i += biomeSize)
        {
            for (int j = z; j < z + REGION_SIZE; j += biomeSize)
            {
                int xMiddle = i + (biomeSize / 2);
                int zMiddle = j + (biomeSize / 2);

                float xNoise = (float)terrainGenerator.Perlin.GetValue((xMiddle + terrainGenerator.RandomNumbers[4]) * SCALE, 0, (zMiddle + terrainGenerator.RandomNumbers[4]) * SCALE);
                int xJitter = (int)(xNoise * biomeSize / 2);
                int xFinal = xMiddle + xJitter;

                float zNoise = (float)terrainGenerator.Perlin.GetValue((xMiddle + terrainGenerator.RandomNumbers[5]) * SCALE, 0, (zMiddle + terrainGenerator.RandomNumbers[5]) * SCALE);
                int zJitter = (int)(zNoise * biomeSize / 2);
                int zFinal = zMiddle + zJitter;

                BiomeList.Add(this.RegionType.ChooseBiome(xFinal, zFinal, this.Seed));
            }
        }
    }

    public Biome GetBiomeByCoordinates(Vector2 position)
    {
        float distance = 1000f;
        Biome nearestBiome = new Biome(position, new DefaultBiomeType());

        float x = position.x;
        float z = position.y;

        // Transitions between biomes
        x += (float)terrainGenerator.Perlin.GetValue(x / SCALE, 0, z / SCALE);
        z += (float)terrainGenerator.Perlin.GetValue(x / SCALE, 0, z / SCALE);

        position = new Vector2(x, z);

        foreach (Biome biome in BiomeList)
        {
            float distanceToSeed = Vector2.Distance(biome.Seed, position);
            if (distanceToSeed < distance)
            {
                nearestBiome = biome;
                // The lower regionRatio is, the nearer the coordinates are to the center of the region (range of [0,1])
                //biomeRatio = distanceToSeed / distance;
                distance = distanceToSeed;
            }
        }

        return nearestBiome;
    }
}
