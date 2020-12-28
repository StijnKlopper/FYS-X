using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    public enum GameObjectType
    {
        Cave,
        Terrain,
        Ocean
    }

    private Dictionary<GameObjectType, List<GameObject>> gameObjectDict;
    
    private int poolAmount;

    public List<GameObject> CaveObjectPool;
    public List<GameObject> TerrainObjectPool;
    public List<GameObject> OceanObjectPool;

    public GameObject TerrainPrefab;
    public GameObject CavePrefab;
    public GameObject OceanPrefab;

    public TerrainGenerator TerrainGenerator;

    void Start()
    {
        // Total rendered area divided by chunk size area + 10%
        poolAmount = (((WorldBuilder.CHUNK_RENDER_DISTANCE * WorldBuilder.CHUNK_RENDER_DISTANCE) / (WorldBuilder.CHUNK_SIZE * WorldBuilder.CHUNK_SIZE))) * 4;
        poolAmount = poolAmount + Mathf.CeilToInt(0.1f * poolAmount);
        gameObjectDict = new Dictionary<GameObjectType, List<GameObject>>();

        for (int i = 0; i < poolAmount; i++ )
        {
            GameObject terrain = (GameObject)Instantiate(TerrainPrefab);
            GameObject cave = (GameObject)Instantiate(CavePrefab);
            GameObject ocean = (GameObject)Instantiate(OceanPrefab);

            terrain.transform.SetParent(TerrainGenerator.transform);
            terrain.transform.rotation = Quaternion.Euler(0, 180, 0);
            cave.transform.SetParent(TerrainGenerator.transform);
            ocean.transform.SetParent(terrain.transform);

            terrain.SetActive(false);
            cave.SetActive(false);
            ocean.SetActive(false);

            TerrainObjectPool.Add(terrain);
            CaveObjectPool.Add(cave);
            OceanObjectPool.Add(ocean);
        }

        gameObjectDict.Add(GameObjectType.Terrain, TerrainObjectPool);
        gameObjectDict.Add(GameObjectType.Cave, CaveObjectPool);
        gameObjectDict.Add(GameObjectType.Ocean, OceanObjectPool);
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

        return null;
    }

    public void UnloadPooledObject(GameObject gameObject) {
        gameObject.SetActive(false);
    }
}
