using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlainsBiome : Biome
{
    // Start is called before the first frame update
    public PlainsBiome()
    {
        this.noiseScale = 100000000f;
        this.octaves = 3;
        this.persistance = 0.5f;
        this.lacunarity = 2f;
        this.heightMultiplier = 10f;

        this.terrainThreshold.Add(1f, Color.white);

    }
}
