using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Tile settings")]
    [SerializeField]
    private int mapWidthInTiles;
    [SerializeField]
    private int mapDepthInTiles;
    [SerializeField]
    private GameObject tilePrefab;

    private Vector3 tileSize;

    private int tileOffset = 5;

    private GameObject tile;
    [Header("Noisemap Settings")]
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offsets;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
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
    private void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapHeight < 1) mapHeight = 1;
        if (octaves < 0) octaves = 1;
    }

}
