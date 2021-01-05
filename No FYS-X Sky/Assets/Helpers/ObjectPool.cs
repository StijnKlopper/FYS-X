using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    /*    public List<GameObject> caveObjectPool;
        public List<GameObject> terrainObjectPool;
        public List<GameObject> oceanObjectPool;*/
    public List<Tile> tilePool;

    public enum GameObjectType
    {
        Cave,
        Terrain,
        Ocean
    }

    //private Dictionary<GameObjectType, List<GameObject>> gameObjectDict;
    private List<Tile> tileObjectList;

    private int poolAmount;
    public GameObject terrainPrefab;
    public GameObject cavePrefab;
    public GameObject oceanPrefab;

    public TerrainGenerator terrainGenerator;

    void Start()
    {
        poolAmount = Mathf.FloorToInt((WorldBuilder.chunkRenderDistance * 8.4f));
        //gameObjectDict = new Dictionary<GameObjectType, List<GameObject>>();
        tileObjectList = new List<Tile>();

        for (int i = 0; i < poolAmount; i++ )
        {
         
            GameObject terrain = (GameObject)Instantiate(terrainPrefab);
            GameObject cave = (GameObject)Instantiate(cavePrefab);
            GameObject ocean = (GameObject)Instantiate(oceanPrefab);

            terrain.GetComponent<MeshRenderer>().enabled = false;
            cave.GetComponent<MeshRenderer>().enabled = false;

            terrain.transform.SetParent(terrainGenerator.transform);
            terrain.transform.rotation = Quaternion.Euler(0, -180, 0);
            cave.transform.SetParent(terrainGenerator.transform);
            ocean.transform.SetParent(terrain.transform);

            terrain.SetActive(false);
            cave.SetActive(false);
            ocean.SetActive(false);

            Tile tile = new Tile(terrain, cave, ocean);
            tile.active = false;
            tileObjectList.Add(tile);

/*            terrainObjectPool.Add(terrain);
            caveObjectPool.Add(cave);
            oceanObjectPool.Add(ocean);*/
        }

/*        gameObjectDict.Add(GameObjectType.Terrain, terrainObjectPool);
        gameObjectDict.Add(GameObjectType.Cave, caveObjectPool);
        gameObjectDict.Add(GameObjectType.Ocean, oceanObjectPool);*/
    }

/*    public GameObject GetPooledObject(GameObjectType gameObjectType)
    {
        List<GameObject> objectList = gameObjectDict[gameObjectType];

        for (int i = 0; i < objectList.Count; i++)
        {
            if (!objectList[i].activeInHierarchy)
            {
                objectList[i].SetActive(true);
                return objectList[i];
            }
        }

        return null;
    }*/

    public Tile GetPooledTile()
    {
        for (int i = 0; i < tileObjectList.Count; i++)
        {
            if (!tileObjectList[i].active)
            {
                tileObjectList[i].enableTile();
                return tileObjectList[i];
            }
        }
        return null;
    }

    public void UnloadPooledObject(GameObject gameObject) {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.SetActive(false);
    }

    public void UnloadPooledTile(Tile tile)
    {
        //gameObject.GetComponent<MeshRenderer>().enabled = false;
        tile.disableTile();
    }
}
