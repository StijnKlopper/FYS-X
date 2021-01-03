using LibNoise.Generator;
using System.Collections.Generic;
using UnityEngine;

public abstract class RegionType
{
    public List<BiomeType> AvailableBiomes;

    public Biome ChooseBiome(int x, int z, Vector2 regionSeed)
    {
        Vector2 position = new Vector2(x, z);
        float distance = Vector2.Distance(position, regionSeed);

        // Ratio to centre, [0, 1], 1 is farther away, 0 is closer
        float centreRatio = (distance / Region.REGION_SIZE) * 2;

        // If furthest 10% away from centre, pick Ocean
        if (centreRatio > 0.9)
        {
            return new Biome(position, new OceanBiomeType());
        }
        if (centreRatio > 0.8)
        {
            return new Biome(position, new BeachBiomeType());
        }

        int amountOfBiomes = AvailableBiomes.Count;

        float scale = 0.17777f;

        Perlin perlin = new Perlin();

        // [-0.1, 0.1], Distort biome choice a little bit
        float perlinValue = (float)perlin.GetValue(x * scale, 0, z * scale) / 10;

        // Pick biomes from AvailableBiomes starting from the middle, equally divided
        for (int i = 1; i <= amountOfBiomes; i++)
        {
            if (perlinValue + centreRatio < i / (float)amountOfBiomes)
            {
                return new Biome(position, AvailableBiomes[i - 1]);
            }
        }

        return new Biome(position, new DefaultBiomeType());
    }
}
