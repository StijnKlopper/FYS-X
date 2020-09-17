using UnityEngine;
public class ForestBiome : Biome
{
    public ForestBiome()
    {
        this.noiseScale = 50f;
        this.octaves = 6;
        this.persistance = 0.5f;
        this.lacunarity = 2f;
        this.heightMultiplier = 30f;

        this.terrainThreshold.Add(0.4f, Color.green);
        this.terrainThreshold.Add(1f, new Color32(128, 255, 128, 255));
    }
}
