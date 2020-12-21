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

    private int poolAmount;
    public GameObject terrainPrefab;
    public GameObject cavePrefab;
    public GameObject oceanPrefab;

    public TerrainGenerator terrainGenerator;

    void Start()
    {
        // Total rendered area divided by chunk size area + 10%
        poolAmount = (((WorldBuilder.chunkRenderDistance * WorldBuilder.chunkRenderDistance) / (WorldBuilder.chunkSize * WorldBuilder.chunkSize))) * 4;
        poolAmount = poolAmount + Mathf.CeilToInt(0.1f * poolAmount);
        gameObjectDict = new Dictionary<GameObjectType, List<GameObject>>();

        for (int i = 0; i < poolAmount; i++ )
        {
            GameObject terrain = (GameObject)Instantiate(terrainPrefab);
            GameObject cave = (GameObject)Instantiate(cavePrefab);
            GameObject ocean = (GameObject)Instantiate(oceanPrefab);

            terrain.transform.SetParent(terrainGenerator.transform);
            terrain.transform.rotation = Quaternion.Euler(0, 180, 0);
            cave.transform.SetParent(terrainGenerator.transform);
            ocean.transform.SetParent(terrain.transform);

            terrain.SetActive(false);
            cave.SetActive(false);
            ocean.SetActive(false);

            terrainObjectPool.Add(terrain);
            caveObjectPool.Add(cave);
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
        Debug.Log("Pool empty");
        return null;
    }

    public void UnloadPooledObject(GameObject gameObject) {
        gameObject.SetActive(false);
    }
}
