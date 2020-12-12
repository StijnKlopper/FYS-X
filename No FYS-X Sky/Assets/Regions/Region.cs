using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Region
{
    public static int regionSize = 800;

    private static float scale = 0.0067f;

    public Vector2 seed;

    public RegionType regionType;

    public List<Biome> biomeList = new List<Biome>();

    private TerrainGenerator terrainGenerator;

    public Region()
    {
        // Used for default assignment;
    }

    public Region(int x, int z)
    {
        this.terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();

        int xMiddle = x + (regionSize / 2);
        int zMiddle = z + (regionSize / 2);

        // Calculate noise for middle of the region, adding a random number to account for negative numbers, * 2 - 1 to spread output to range [-1,1]
        float xNoise = (float)terrainGenerator.perlin.GetValue((xMiddle + terrainGenerator.randomNumbers[1]) * scale, 0, (zMiddle + terrainGenerator.randomNumbers[1]) * scale);
        int xJitter = (int)(xNoise * regionSize / 4);
        int xFinal = xMiddle + xJitter;

        float zNoise = (float)terrainGenerator.perlin.GetValue((xMiddle + terrainGenerator.randomNumbers[1]) * scale, 0, (zMiddle + terrainGenerator.randomNumbers[1]) * scale);
        int zJitter = (int)(zNoise * regionSize / 4);
        int zFinal = zMiddle + zJitter;

        // Calculate heat and moisture to determine region type, + 1 / 2 to spread the result between [0,1] instead of [-1,1]
        float heat = (float)(terrainGenerator.perlin.GetValue(xFinal + terrainGenerator.randomNumbers[1] * scale, 0, zFinal + terrainGenerator.randomNumbers[1] * scale) + 1) / 2;
        float moisture = (float)(terrainGenerator.perlin.GetValue(xFinal + terrainGenerator.randomNumbers[0] * scale, 0, zFinal + terrainGenerator.randomNumbers[0] * scale) + 1) / 2;

        this.seed = new Vector2(xFinal, zFinal);

        this.regionType = GetRegionType(heat, moisture);

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
        int biomeSize = regionSize / 10;
        for (int i = x; i < x + regionSize; i += biomeSize)
        {
            for (int j = z; j < z + regionSize; j += biomeSize)
            {
                int xMiddle = i + (biomeSize / 2);
                int zMiddle = j + (biomeSize / 2);

                float xNoise = (float)terrainGenerator.perlin.GetValue((xMiddle + terrainGenerator.randomNumbers[4]) * scale, 0, (zMiddle + terrainGenerator.randomNumbers[4]) * scale);
                int xJitter = (int)(xNoise * biomeSize / 2);
                int xFinal = xMiddle + xJitter;

                float zNoise = (float)terrainGenerator.perlin.GetValue((xMiddle + terrainGenerator.randomNumbers[5]) * scale, 0, (zMiddle + terrainGenerator.randomNumbers[5]) * scale);
                int zJitter = (int)(zNoise * biomeSize / 2);
                int zFinal = zMiddle + zJitter;

                biomeList.Add(this.regionType.ChooseBiome(xFinal, zFinal, this.seed));
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
        x += (float) terrainGenerator.perlin.GetValue(x / scale, 0, z / scale);
        z += (float) terrainGenerator.perlin.GetValue(x / scale, 0, z / scale);

        position = new Vector2(x, z);

        foreach (Biome biome in biomeList)
        {
            float distanceToSeed = Vector2.Distance(biome.seed, position);
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
