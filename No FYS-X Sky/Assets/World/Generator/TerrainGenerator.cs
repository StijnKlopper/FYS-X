using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{


    [SerializeField]
    private int mapWidthInTiles, mapDepthInTiles;

    [SerializeField]
    private GameObject tilePrefab;
    private Vector3 tileSize;

    private int tileOffset = 5;

    private GameObject tile;
    // Start is called before the first frame update
    void Start()
    {
       

    }

    public GameObject GenerateTile(Vector3 position)
    {
        
        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        //Vector3 tileSize = new Vector3(10, 10, 10);
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;


        Vector3 tilePosition = new Vector3(position.x + tileOffset,
                this.gameObject.transform.position.y,
                position.z + tileOffset);
        GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;

        return tile;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
