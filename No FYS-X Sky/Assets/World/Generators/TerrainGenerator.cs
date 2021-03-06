﻿using System.Collections.Generic;
using LibNoise.Generator;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Tile settings")]
    [SerializeField]
    private GameObject tilePrefab;

    [SerializeField]
    private TextureData textureData;

    public static int Seed;

    [System.NonSerialized]
    public int[] RandomNumbers;

    [System.NonSerialized]
    public static Dictionary<Vector3, Region> RegionDict = new Dictionary<Vector3, Region>();

    [System.NonSerialized]
    public Perlin Perlin;

    void Start()
    {
        System.Random random = new System.Random(Seed);
        this.RandomNumbers = new int[20];

        this.Perlin = new Perlin();

        for (int i = 0; i < this.RandomNumbers.Length; i++)
        {
            this.RandomNumbers[i] = random.Next(10000, 100000);
        }

        // Set shared texture array for all tiles to use to preserve loading and unloading too many textures
        textureData.ApplyToMaterial(tilePrefab.GetComponent<Renderer>().sharedMaterial);
    }

    public Biome GetBiomeByCoordinates(Vector2 coordinates)
    {
        // Using coordinates, determine region / continent, then determine biome based on the continent and position
        float scale = 0.17777f;
        float x = coordinates.x * scale;
        float z = coordinates.y * scale;
        x += (float)Perlin.GetValue(x, 0, z);
        z += (float)Perlin.GetValue(x, 0, z);

        Vector2 newCoordinates = new Vector2(x / scale, z / scale);
        Region region = GetRegionByCoordinates(newCoordinates);
        return region.GetBiomeByCoordinates(newCoordinates);
    }

    public static Region GetRegionByCoordinates(Vector2 coordinates)
    {
        float distance = 10000f;
        Region nearestRegion = new Region();

        // Find distance of coordinates to the seed (middle) of the region
        foreach (KeyValuePair<Vector3, Region> region in RegionDict)
        {
            float distanceToSeed = Vector2.Distance(region.Value.Seed, coordinates);
            if (distanceToSeed < distance)
            {
                nearestRegion = region.Value;
                // The lower regionRatio is, the nearer the coordinates are to the center of the region (range of [0,1])
                distance = distanceToSeed;
            }
        }

        return nearestRegion;
    }
}

