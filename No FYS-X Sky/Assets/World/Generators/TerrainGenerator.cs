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
    public float minNoiseHeight;

    [System.NonSerialized]
    public float maxNoiseHeight;

    [System.NonSerialized]
    public int[] randomNumbers;

    // Start is called before the first frame update
    void Start()
    {
        System.Random random = new System.Random(seed);
        this.randomNumbers = new int[20];
        for (int i = 0; i < this.randomNumbers.Length; i++)
        {
            this.randomNumbers[i] = random.Next(10000, 100000);
        }
        this.minNoiseHeight = float.MaxValue;
        this.maxNoiseHeight = float.MinValue;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GenerateTile(Vector3 position)
    {
        Vector3 tilePosition = new Vector3(position.x + tileOffset,
                this.gameObject.transform.position.y,
                position.z + tileOffset);
        GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
        return tile;
    }

    // TO-DO: Modify to take in height and moisture
    public Biome GetBiomeByBiomeValue(float biomeValue)
    {
        if (biomeValue < 0.1)
        {
            return new OceanBiome();
        }
        if (biomeValue < 0.12)
        {
            return new BeachBiome();
        }
        if (biomeValue < 0.22)
        {
            return new PlainsBiome();
        }
        if (biomeValue < 0.28)
        {
            return new ShrublandBiome();
        }
        if (biomeValue < 0.40)
        {
            return new DesertBiome();
        }
        if (biomeValue < 0.45)
        {
            return new ShrublandBiome();
        }
        if (biomeValue < 0.50)
        {
            return new PlainsBiome();
        }
        if (biomeValue < 0.70)
        {
            return new ForestBiome();
        }
        if (biomeValue < 0.80)
        {
            return new MountainBiome();
        }
        if (biomeValue < 0.90)
        {
            return new SnowBiome();
        }
        return new MountainBiome();
    }

}
