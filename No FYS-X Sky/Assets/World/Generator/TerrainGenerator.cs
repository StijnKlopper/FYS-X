using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Tile settings")]
    [SerializeField]
    private GameObject tilePrefab;

    private Vector3 tileSize;

    private int tileOffset = 5;

    private GameObject tile;

    [Header("Noisemap Settings")]

    public float noiseScale = 10f;

    public int octaves = 9;

    [Range(0, 1)]
    public float persistance = 0.5f;

    public float lacunarity;

    public int seed;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        if (octaves < 0) octaves = 1;
    }

    public GameObject GenerateTile(Vector3 position)
    {
        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;

        Vector3 tilePosition = new Vector3(position.x + tileOffset,
                this.gameObject.transform.position.y,
                position.z + tileOffset);
        GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;

        return tile;
    }

}
