using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RegionType
{
    public List<BiomeType> availableBiomes;

    public Biome ChooseBiome(int x, int z, Vector2 regionSeed)
    {
        Vector2 position = new Vector2(x, z);
        float distance = Vector2.Distance(position, regionSeed);
        float centreRatio = distance / Region.regionSize;

        if (centreRatio > 0.8)
        {
            return new Biome(position, new OceanBiomeType());
        }
        if (centreRatio > 0.7)
        {
            return new Biome(position, new BeachBiomeType());
        }

        int amountOfBiomes = availableBiomes.Count;

        float scale = 0.17777f;
        float perlinValue = Mathf.PerlinNoise(x * scale, z * scale);

        // Create array of values where it steps evenly from 0 to 1.5, divided by the amount of biomes

        //for (int i = 1; i < amountOfBiomes; i++)
        //{
        //    if (perlinValue + centreRatio < i / amountOfBiomes)
        //    {
        //        return new Biome(position, availableBiomes[i]);
        //    }
        //    // 0-1 + 0.0002 < 1 / 3, 
        //}

        if (centreRatio > 0.6) return new Biome(position, availableBiomes[2]);
        if (centreRatio > 0.4) return new Biome(position, availableBiomes[1]);
        return new Biome(position, availableBiomes[0]);

        // PerlinValue + centreRatio = max 2

        // Available: current coordinates
        //            middle of region
        // Using distance between coordinates and middle of region / size of region
        // If ratio = close to zero, place ocean/ biomes at the start of availableBiomes
        // If ratio = close to one, place biomes at the end of availablebiomes
    }
}
