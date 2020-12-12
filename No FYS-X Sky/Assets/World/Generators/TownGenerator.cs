using Assets.World.Generator;
using UnityEngine;
using LibNoise.Generator;
using System.Collections.Generic;
using System;

public class TownGenerator : MonoBehaviour, Generator
{
    public int mapWidth;
    public int mapHeight;
    public Vector2 offsets;

    int seed;

    TerrainGenerator terrainGenerator;

    [SerializeField]
    public List<GameObject> houses;

    [System.NonSerialized]
    GameObject parentObject;

    public int[] randomNumbers;


    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        seed = terrainGenerator.seed;

        parentObject = GameObject.Find("CityPoints");

        // Pseudo random numbers
        System.Random random = new System.Random(seed);
        this.randomNumbers = new int[50];
        for (int i = 0; i < this.randomNumbers.Length; i++)
        {
            this.randomNumbers[i] = random.Next(-10000, 10000);
        }
    }

    public void Generate(int mapWidth, int mapHeight, Vector2 offsets)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.offsets = offsets;

        makeHouseLocations();
    }

    bool ValidHousePosition(Vector3 position, Bounds bounds)
    {
        float radius;
        if (bounds.size.x > bounds.size.z) radius = bounds.size.x;
        else radius = bounds.size.z;
        radius += 1;

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

    Vector3 PositionCorrection(Vector3 position)
    {
        // Gives the correct Y value (with the height of the ground)
        Vector3 rayStartPosition = new Vector3(position.x, 10, position.z);
        Ray ray = new Ray(rayStartPosition, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            // Small extra correction
            Vector3 correctionPosition = new Vector3(hitInfo.point.x, hitInfo.point.y - 0.2f, hitInfo.point.z);
            return correctionPosition;
        }

        return Vector3.zero;
    }

    private void makeHouseLocations()
    {
        int checkForEveryCoordinates = 4; 
        float[,] noiseMap = GenerateCityNoiseMap(this.mapWidth, this.mapHeight, this.offsets);
        float minNoiseHeight = 0.05f;

        for (int y = 0; y < mapHeight; y+=checkForEveryCoordinates)
        {
            for (int x = 0; x < mapWidth; x += checkForEveryCoordinates)
            {
                Vector3 position = new Vector3(x + this.offsets.x, 10, y + this.offsets.y);

                // If the current location is within the noisemap position
                if (noiseMap[x, y] <= minNoiseHeight)
                {
                    // Get random building index from thhe list of buildings
                    int randomHouseIndex = (int)Math.Round(Mathf.PerlinNoise(x, y) * houses.Count);

                    // Calculate bounds and calculate the houseposition for the center of the house, also get the correct Y value for the building
                    Bounds houseBounds = CalculateBounds(houses[randomHouseIndex]);
                    Vector3 housePosition = PositionCorrection(new Vector3(position.x - houseBounds.center.x, 0, position.z - houseBounds.center.z));
                    housePosition = new Vector3(housePosition.x, houses[randomHouseIndex].transform.position.y + housePosition.y, housePosition.z);

                    Tile tile = WorldBuilder.GetTile(position); // TODO: Hier weghalen en in de if weer zetten (zie wat nu is uitgecomment)

                    // Check if valid position
                    if (tile != null && ValidHousePosition(housePosition, houseBounds))
                    {
                        GameObject house = Instantiate(houses[randomHouseIndex], housePosition, Quaternion.identity, parentObject.transform.GetChild(0).transform) as GameObject;

                        // Turn house with consistent random numbers
                        int xRandomIndex = this.randomNumbers[(x + 1) * (y + 1) * Math.Abs((int)this.offsets.x) % this.randomNumbers.Length];
                        int zRandomIndex = this.randomNumbers[(x + 1) * (y + 1) * Math.Abs((int)this.offsets.y) % (this.randomNumbers.Length - 1)];
                        Vector3 lookAtPostition = new Vector3(xRandomIndex, house.transform.position.y, zRandomIndex);
                        house.transform.LookAt(lookAtPostition);

                        // Add house to tile
                        //Tile tile = WorldBuilder.GetTile(position); // TODO: Deze terughalen
                        tile.AddObject(house);

                        /*
                        Vector3 positionTemp = new Vector3(x + this.offsets.x, 20, y + this.offsets.y);
                        GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        point.transform.position = positionTemp;
                        Physics.SyncTransforms();
                        */
                    }
                }
            }
        }
    }

    public Bounds CalculateBounds(GameObject go)
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

    private float[,] GenerateCityNoiseMap(int mapWidth, int mapHeight, Vector2 offsets)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        float maxPossibleHeight = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float persistance = 0.5f;
        float lacunarity = 2f;

        int octaves = 12;
        float scale = 50.777f;

        Perlin perlin = new Perlin(frequency, lacunarity, persistance, octaves, seed, LibNoise.QualityMode.High);

        for (int i = 0; i < octaves; i++)
        {
            maxPossibleHeight += amplitude;
            amplitude *= 0.5f;
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                double sampleX = (x + offsets.x) / scale;
                double sampleY = (y + offsets.y) / scale;

                float noiseHeight = (float)perlin.GetValue(sampleX, 0, sampleY);

                // Normalise noise map between minimum and maximum noise heights
                noiseHeight = (noiseHeight + 1) / (2f * maxPossibleHeight / 1.75f);

                // Change height based on height curve and heightMultiplier
                Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(x + offsets.x, y + offsets.y));

                // Exclude some biomes from noisemap 
                if (biome.biomeType is OceanBiomeType || biome.biomeType is MountainBiomeType)
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
