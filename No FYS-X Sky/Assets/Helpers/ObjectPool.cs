using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public List<Tile> tilePool;
    public GameObject terrainPrefab;
    public GameObject cavePrefab;
    public GameObject oceanPrefab;
    public TerrainGenerator terrainGenerator;

    private List<Tile> TileObjectList;
    private int PoolAmount;

    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        PoolAmount = (((WorldBuilder.CHUNK_RENDER_DISTANCE * WorldBuilder.CHUNK_RENDER_DISTANCE) / (WorldBuilder.CHUNK_SIZE * WorldBuilder.CHUNK_SIZE))) * 4;
        PoolAmount = PoolAmount + Mathf.CeilToInt(0.1f * PoolAmount);

        TileObjectList = new List<Tile>();

        for (int i = 0; i < PoolAmount; i++)
        {

            GameObject terrain = (GameObject)Instantiate(terrainPrefab);
            GameObject cave = (GameObject)Instantiate(cavePrefab);
            GameObject ocean = (GameObject)Instantiate(oceanPrefab);

            terrain.transform.SetParent(terrainGenerator.transform);
            terrain.transform.rotation = Quaternion.Euler(0, -180, 0);
            cave.transform.SetParent(terrainGenerator.transform);
            ocean.transform.SetParent(terrain.transform);

            terrain.SetActive(false);
            cave.SetActive(false);
            ocean.SetActive(false);

            Tile tile = new Tile(terrain, cave, ocean);
            tile.active = false;
            TileObjectList.Add(tile);
        }
    }

    public Tile GetPooledTile()
    {
        for (int i = 0; i < TileObjectList.Count; i++)
        {
            if (!TileObjectList[i].active)
            {
                TileObjectList[i].enableTile();
                return TileObjectList[i];
            }
        }
        //Maybe add some kind of warning/error?
        return null;
    }
}
