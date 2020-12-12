using Assets.World.Generator;
using System;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class TownGenerator : MonoBehaviour, Generator
{
    public int mapWidth;
    public int mapHeight;
    public Vector2 offsets;

    void Start()
    {
       
    }

    void FixedUpdate()
    {
      
    }

    public void Generate(int mapWidth, int mapHeight, Vector2 offsets)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.offsets = offsets;

        makeHouseLocations();
    }

    private void makeHouseLocations()
    {
        int checkForEveryCoordinates = 2; 
        float[,] noiseMap = GenerateCityNoiseMap(this.mapWidth, this.mapHeight, this.offsets);
        float minNoiseHeight = 0.00f;

        for (int y = 0; y < mapHeight; y+=checkForEveryCoordinates)
        {
            for (int x = 0; x < mapWidth; x += checkForEveryCoordinates)
            {
                // If the current location is within the noisemap position
                if (noiseMap[x, y] <= minNoiseHeight)
                {
                    GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    point.transform.position = new Vector3(-(x + offsets.x), 0, -(y + offsets.y));
                    Physics.SyncTransforms();
                }
            }
        }

    }

    private float[,] GenerateCityNoiseMap(int mapWidth, int mapHeight, Vector2 offsets)
    {
        int scale = 10;
        int octaves = 1;
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float[,] noiseMap = new float[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + offsets.x) / scale;
                    float sampleY = (y + offsets.y) / scale;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

}
