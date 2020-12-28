﻿using Assets.World.Generator;
using LibNoise.Generator;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour, Generator
{
    private int mapWidth;
    private int mapHeight;
    private Vector2 offsets;

    private TerrainGenerator terrainGenerator;

    [SerializeField]
    public List<GameObject> houses;

    private GameObject parentObject;

    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        parentObject = GameObject.Find("CityPoints");
    }

    public void Generate(int mapWidth, int mapHeight, Vector2 offsets)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.offsets = offsets;

        GenerateHouses();
    }

    // Validate house position to prevent overlapping
    private bool ValidHousePosition(Vector3 position, Bounds bounds)
    {
        float buildingMargin = 1.5f;
        float radius = 1;
        if (bounds.size.x > bounds.size.z) radius += bounds.size.x + buildingMargin;
        else radius += bounds.size.z + buildingMargin;

        Collider[] hitColliders = Physics.OverlapSphere(position, radius);
        foreach (var hitCollider in hitColliders)
        {
            // If gameobject is a building 
            if (hitCollider.gameObject.transform.root.name == ("CityPoints"))
            {
                return false;
            }
        }
        return true;
    }

    private Vector3 PositionCorrection(Vector3 position)
    {
        // Gives the correct Y value (with the height of the ground)
        Vector3 rayStartPosition = new Vector3(position.x, 10, position.z);
        Ray ray = new Ray(rayStartPosition, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            if (hitInfo.point.y > -1)
            {
                // Return with small extra correction
                return new Vector3(hitInfo.point.x, hitInfo.point.y - 0.2f, hitInfo.point.z);
            }
        }

        return Vector3.zero;
    }

    // Generate houses based on noise height value
    private void GenerateHouses()
    {
        int aboveGroundPlaceholderY = 10;
        int checkForEveryCoordinates = 50;
        float minNoiseHeight = 0.05f;
        float scale = 500.6667f;
        float[,] noiseMap = GenerateCityNoiseMap(this.mapWidth, this.mapHeight, this.offsets);

        for (int y = 0; y < mapHeight; y += checkForEveryCoordinates)
        {
            for (int x = 0; x < mapWidth; x += checkForEveryCoordinates)
            {
                Vector3 position = new Vector3(x + this.offsets.x, aboveGroundPlaceholderY, y + this.offsets.y);

                // If the current location is within the noisemap position
                if (noiseMap[x, y] <= minNoiseHeight)
                {
                    // Get random building index from thhe list of buildings 
                    int randomHouseIndex = (int)Math.Round(((terrainGenerator.Perlin.GetValue(position.x + terrainGenerator.RandomNumbers[y] / scale, 0, position.y + terrainGenerator.RandomNumbers[y] / scale) + 1) / 2f) * houses.Count);

                    // Calculate bounds and calculate the houseposition for the center of the house, also get the correct Y value for the building
                    Bounds houseBounds = CalculateBounds(houses[randomHouseIndex]);
                    Vector3 housePosition = PositionCorrection(new Vector3(position.x - houseBounds.center.x, 0, position.z - houseBounds.center.z));
                    housePosition = new Vector3(housePosition.x, houses[randomHouseIndex].transform.position.y + housePosition.y, housePosition.z);

                    // Get tile and check if it exists before making a house
                    Tile tile = WorldBuilder.GetTile(position);

                    // Check if valid position
                    if (tile != null && ValidHousePosition(housePosition, houseBounds))
                    {
                        GameObject house = Instantiate(houses[randomHouseIndex], housePosition, Quaternion.identity, parentObject.transform.GetChild(0).transform) as GameObject;

                        // Turn house with consistent random numbers
                        int xRandomIndex = terrainGenerator.RandomNumbers[(x + 1) * (y + 1) * Math.Abs((int)this.offsets.x) % terrainGenerator.RandomNumbers.Length];
                        int zRandomIndex = terrainGenerator.RandomNumbers[(x + 1) * (y + 1) * Math.Abs((int)this.offsets.y) % (terrainGenerator.RandomNumbers.Length - 1)];
                        Vector3 lookAtPosition = new Vector3(xRandomIndex, house.transform.position.y, zRandomIndex);
                        house.transform.LookAt(lookAtPosition);

                        // Add house to tile 
                        tile.AddObject(house);
                    }
                }
            }
        }
    }

    // Calculate bounds of buildings
    private Bounds CalculateBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1, ni = renderers.Length; i < ni; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
        else
        {
            return new Bounds();
        }
    }

    // Generate NoiseMap for the City(Buildings)
    private float[,] GenerateCityNoiseMap(int mapWidth, int mapHeight, Vector2 offsets)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        float frequency = 1f;
        float persistance = 1f;
        float lacunarity = 2f;

        int octaves = 1;
        float scale = 100.777f;

        Perlin perlin = new Perlin(frequency, lacunarity, persistance, octaves, terrainGenerator.Seed, LibNoise.QualityMode.High);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                double sampleX = (x + offsets.x) / scale;
                double sampleY = (y + offsets.y) / scale;

                float noiseHeight = (float)(perlin.GetValue(sampleY, 0, sampleX) + 1) / 2;

                // Change height based on height curve and heightMultiplier
                Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(x + offsets.x, y + offsets.y));

                // Exclude some biomes from noisemap 
                if (biome.BiomeType is OceanBiomeType || biome.BiomeType is MountainBiomeType)
                {
                    noiseMap[x, y] = 1f;
                }
                else
                {
                    noiseMap[x, y] = noiseHeight;
                }
            }
        }

        return noiseMap;
    }
}
