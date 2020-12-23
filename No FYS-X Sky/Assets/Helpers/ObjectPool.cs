﻿using System.Collections;
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
        poolAmount = Mathf.FloorToInt((WorldBuilder.chunkRenderDistance * 4.4f));
        gameObjectDict = new Dictionary<GameObjectType, List<GameObject>>();

        for (int i = 0; i < poolAmount; i++ )
        {
            


            GameObject terrain = (GameObject)Instantiate(terrainPrefab);
            GameObject cave = (GameObject)Instantiate(cavePrefab);
            GameObject ocean = (GameObject)Instantiate(oceanPrefab);

            terrain.transform.SetParent(terrainGenerator.transform);
            terrain.transform.rotation = Quaternion.Euler(0, -180, 0);
            cave.transform.SetParent(terrainGenerator.transform);
            cave.transform.rotation = Quaternion.Euler(0, -180, 0);
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

        return null;
    }

    public void UnloadPooledObject(GameObject gameObject) {
        gameObject.SetActive(false);
    }
}
