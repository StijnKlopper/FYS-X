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

}
