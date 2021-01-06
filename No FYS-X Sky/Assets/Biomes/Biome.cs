using UnityEngine;

public class Biome
{
    public BiomeType BiomeType;

    public Vector2 Seed;

    public Biome(Vector2 seed, BiomeType biomeType)
    {
        this.Seed = seed;
        this.BiomeType = biomeType;
    }
}
