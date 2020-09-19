using Assets.World.Generator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour, Generator
{
    [Header("Tile settings")]
    [SerializeField]
    private GameObject tilePrefab;

    private int tileOffset = 5;

    public int seed;

    [System.NonSerialized]
    public int[] randomNumbers;

    [System.NonSerialized]
    public Dictionary<Vector3, GameObject> tileDict = new Dictionary<Vector3, GameObject>();

    [System.NonSerialized]
    public Dictionary<Vector3, Region> regionDict = new Dictionary<Vector3, Region>();

    // Start is called before the first frame update
    void Start()
    {
        System.Random random = new System.Random(seed);
        this.randomNumbers = new int[20];

        for (int i = 0; i < this.randomNumbers.Length; i++)
        {
            this.randomNumbers[i] = random.Next(10000, 100000);
        }
    }

    public GameObject GenerateTile(Vector3 position)
    {
        Vector3 tilePosition = new Vector3(position.x + tileOffset,
                this.gameObject.transform.position.y,
                position.z + tileOffset);
        GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
        return tile;
    }

    public Biome GetBiomeByCoordinates(Vector2 coordinates)
    {
        float scale = 0.017f;
        float x = coordinates.x * scale;
        float z = coordinates.y * scale;
        x += Mathf.PerlinNoise(x, z);
        z += Mathf.PerlinNoise(x, z);

        coordinates = new Vector2(x / scale, z / scale);
        float distance = 1000f;
        // TO-DO: Add DefaultBiome();
        Biome biome = new SnowBiome();
        foreach (KeyValuePair<Vector3, Region> region in regionDict)
        {
            int x1 = (int)coordinates.x;
            int z1 = (int)coordinates.y;
            int x2 = (int)region.Value.seed.x;
            int z2 = (int)region.Value.seed.y;
            float distanceToSeed = Mathf.Sqrt((x1 - x2) * (x1 - x2) + (z1 - z2) * (z1 - z2));
            if (distanceToSeed < distance)
            {
                biome = region.Value.biome;
                distance = distanceToSeed;
            }
        }

        return biome;
    }

    public Biome GetBiomeByNoise(float noise)
    {
        if (noise < 0.2f) return new OceanBiome();
        if (noise < 0.4f) return new BeachBiome();
        if (noise < 0.5f) return new PlainsBiome();
        if (noise < 0.6f) return new ForestBiome();
        if (noise < 0.8f) return new MountainBiome();
        return new SnowBiome();
    }

    // Use later when implementing logical biome placement
    //public Biome GetBiomeByHeightAndMoisture(float height, float moisture)
    //{
    //    // TO-DO: add biomes and tweak values
    //    if (height < 0.2f) return new OceanBiome();

    //    if (height < 0.3f) return new BeachBiome();

    //    if (height > 0.65f) return new MountainBiome();
        
    //    if (height < 0.65f)
    //    {
    //        if (moisture < 0.2) return new DesertBiome();
    //        if (moisture < 0.4) return new ShrublandBiome();
    //        if (moisture < 0.7) return new PlainsBiome();
    //        return new ForestBiome();
    //    }
    //    return new SnowBiome();
    //}

    public void DestroyTile(GameObject obj)
    {
        Destroy(obj);
    }
}
