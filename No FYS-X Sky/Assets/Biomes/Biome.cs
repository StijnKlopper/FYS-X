using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome
{
    public BiomeType biomeType;

    public Vector2 seed;

    public Biome(Vector2 seed, BiomeType biomeType)
    {
        this.seed = seed;
        this.biomeType = biomeType;
    }
}
