using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public List<GameObject> caveObjectPool;
    public List<GameObject> terrainObjectPool;
    public List<GameObject> oceanObjectPool;

    public enum GameObjectType
    {
        Cave,
        Terrain,
        Ocean
    }

    private Dictionary<GameObjectType, List<GameObject>> gameObjectDict;

    public int poolAmount;
    public GameObject terrainPrefab;
    public GameObject cavePrefab;
    public GameObject oceanPrefab;

    public TerrainGenerator terrainGenerator;

    // Start is called before the first frame update
    void Start()
    {
        gameObjectDict = new Dictionary<GameObjectType, List<GameObject>>();


        for (int i = 0; i < poolAmount; i++ )
        {
            GameObject cave = (GameObject)Instantiate(cavePrefab);
            GameObject terrain = (GameObject)Instantiate(terrainPrefab);
            GameObject ocean = (GameObject)Instantiate(oceanPrefab);

            terrain.transform.SetParent(terrainGenerator.transform);
            cave.transform.SetParent(terrainGenerator.transform);
            ocean.transform.SetParent(terrain.transform);
            cave.transform.rotation = Quaternion.Euler(0, 180, 0);

            cave.SetActive(false);
            terrain.SetActive(false);
            ocean.SetActive(false);
            caveObjectPool.Add(cave);
            terrainObjectPool.Add(terrain);
            oceanObjectPool.Add(ocean);

            

        }

        gameObjectDict.Add(GameObjectType.Terrain, terrainObjectPool);
        gameObjectDict.Add(GameObjectType.Cave, caveObjectPool);
        gameObjectDict.Add(GameObjectType.Ocean, oceanObjectPool);
    }

    public GameObject GetPooledObject(GameObjectType gameObjectType)
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

        Debug.Log("POOL IS EMPTY " + gameObjectType.ToString());
        return null;
    }

    public void UnloadPooledObject(GameObject gameObject) {
        gameObject.SetActive(false);
    }
}
