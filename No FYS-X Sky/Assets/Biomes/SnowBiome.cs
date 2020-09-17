using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowBiome : Biome
{
    public SnowBiome()
    {
        this.noiseScale = 50f;
        this.octaves = 6;
        this.persistance = 0.5f;
        this.lacunarity = 2f;
        this.heightMultiplier = 30f;

        this.terrainThreshold.Add(1f, Color.white);
    }
}
