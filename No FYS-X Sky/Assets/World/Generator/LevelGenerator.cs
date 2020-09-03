using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{


    [SerializeField]
    private int mapWidthInTiles, mapDepthInTiles;

    [SerializeField]
    private GameObject tilePrefab;
    private Vector3 tileSize;

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


        Vector3 tilePosition = new Vector3(position.x + 5,
                this.gameObject.transform.position.y,
                position.z + 5);
        Debug.Log(tilePosition);
        GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;

        return tile;
    }

    private void GenerateMap()
    {
        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;

        for (int xTileIndex = 0; xTileIndex < mapWidthInTiles; xTileIndex++)
        {
            for (int zTileIndex = 0; zTileIndex < mapDepthInTiles; zTileIndex++)
            {


                Vector3 tilePosition = new Vector3(this.gameObject.transform.position.x + xTileIndex * tileWidth + 5,
                    this.gameObject.transform.position.y,
                    this.gameObject.transform.position.z + zTileIndex * tileDepth + 5);
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;


            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
