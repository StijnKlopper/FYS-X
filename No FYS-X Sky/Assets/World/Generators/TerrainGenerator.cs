using Assets.World.Generator;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour, Generator
{
    [Header("Tile settings")]
    [SerializeField]
    private GameObject tilePrefab;

    private int tileOffset = 5;

    public int seed;

    public TextureData textureData;

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
        textureData.ApplyToMaterial(tilePrefab.GetComponent<Renderer>().sharedMaterial);
    }

    public GameObject GenerateTile(Vector3 position)
    {
       
        Vector3 tilePosition = new Vector3(position.x + tileOffset, this.gameObject.transform.position.y, position.z + tileOffset);
        GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
        return tile;
    }

    public Biome GetBiomeByCoordinates(Vector2 coordinates)
    {
        //Using coordinates, determine region / continent, then determine biome based on the continent and position
        float scale = 0.17777f;
        float x = coordinates.x * scale;
        float z = coordinates.y * scale;
        x += Mathf.PerlinNoise(x, z) * 2 - 1;
        z += Mathf.PerlinNoise(x, z) * 2 - 1;

        Vector2 newCoordinates = new Vector2(x / scale, z / scale);
        Region region = GetRegionByCoordinates(newCoordinates);
        return region.GetBiomeByCoordinates(newCoordinates);
    }

    public Region GetRegionByCoordinates(Vector2 coordinates)
    {
        float distance = 10000f;
        Region nearestRegion = new Region();

        // Find distance of coordinates to the seed (middle) of the region
        foreach (KeyValuePair<Vector3, Region> region in regionDict)
        {
            float distanceToSeed = Vector2.Distance(region.Value.seed, coordinates);
            if (distanceToSeed < distance)
            {
                nearestRegion = region.Value;
                // The lower regionRatio is, the nearer the coordinates are to the center of the region (range of [0,1])
                //regionRatio = distanceToSeed / distance;
                distance = distanceToSeed;
            }
        }

        return nearestRegion;
    }

    public void DestroyTile(GameObject obj)
    {
        Destroy(obj);
    }
}
