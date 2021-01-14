using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public List<Tile> TilePool;
    public GameObject TerrainPrefab;
    public GameObject CavePrefab;
    public GameObject OceanPrefab;
    [System.NonSerialized]
    public TerrainGenerator TerrainGenerator;

    private List<Tile> TileObjectList;
    private int PoolAmount;

    void Start()
    {
        TerrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        // Total rendered area divided by chunk size area + 10%
        PoolAmount = (((WorldBuilder.CHUNK_RENDER_DISTANCE * WorldBuilder.CHUNK_RENDER_DISTANCE) / (WorldBuilder.CHUNK_SIZE * WorldBuilder.CHUNK_SIZE))) * 4;
        PoolAmount = PoolAmount + Mathf.CeilToInt(0.1f * PoolAmount);

        TileObjectList = new List<Tile>();

        for (int i = 0; i < PoolAmount; i++)
        {

            GameObject terrain = (GameObject)Instantiate(TerrainPrefab);
            GameObject cave = (GameObject)Instantiate(CavePrefab);
            GameObject ocean = (GameObject)Instantiate(OceanPrefab);

            terrain.transform.SetParent(TerrainGenerator.transform);
            terrain.transform.rotation = Quaternion.Euler(0, -180, 0);
            cave.transform.SetParent(TerrainGenerator.transform);
            ocean.transform.SetParent(terrain.transform);

            terrain.SetActive(false);
            cave.SetActive(false);
            ocean.SetActive(false);

            Tile tile = new Tile(terrain, cave, ocean);
            tile.Active = false;
            TileObjectList.Add(tile);
        }
    }

    public Tile GetPooledTile()
    {
        for (int i = 0; i < TileObjectList.Count; i++)
        {
            if (!TileObjectList[i].Active)
            {
                TileObjectList[i].enableTile();
                return TileObjectList[i];
            }
        }
        // Tile amount are calculated using render distance and should not be a problem so this is only to satisfy the method
        return null;
    }
}
