using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Biome
{
    // Higher value = more spread out terrain
    public float noiseScale;

    // More octaves = more layers of terrain, makes for more detailed mountains
    public int octaves;

    // Higher persistance = lower amplitude for every octave
    public float persistance;

    // Higher lacunarity = higher amount of mountains for every octave
    public float lacunarity;

    // Cringe
    public float heightMultiplier;


    public Dictionary<float, Color> terrainThreshold = new Dictionary<float, Color>();

    public float GetMaxPossibleHeight()
    {
        float maxPossibleHeight = 0;
        float amplitude = 1;

        for (int i = 0; i < octaves; i++)
        {
            maxPossibleHeight += amplitude;
            amplitude *= this.persistance;
        }

        return maxPossibleHeight;
    }


}
