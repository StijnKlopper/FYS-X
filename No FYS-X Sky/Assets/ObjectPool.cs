using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public List<GameObject> caveObjectPool;
    public List<GameObject> terrainObjectPool;
    public List<GameObject> oceanObjectPool;
    public int poolAmount;
    public GameObject cavePrefab;
    public GameObject terrainPrefab;
    public GameObject oceanPrefab;

    public TerrainGenerator terrainGenerator;
    public CaveGenerator caveGenerator;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < poolAmount; i++ )
        {
            GameObject cave = (GameObject)Instantiate(cavePrefab);
            GameObject terrain = (GameObject)Instantiate(terrainPrefab);
            GameObject ocean = (GameObject)Instantiate(oceanPrefab);

            terrain.transform.SetParent(terrainGenerator.transform);
            cave.transform.SetParent(caveGenerator.transform);
            ocean.transform.SetParent(terrain.transform);
            cave.transform.rotation = Quaternion.Euler(0, 180, 0);

            cave.SetActive(false);
            terrain.SetActive(false);
            ocean.SetActive(false);
            caveObjectPool.Add(cave);
            terrainObjectPool.Add(terrain);
            oceanObjectPool.Add(ocean);

        }
    }

    public GameObject GetPooledCaveObject()
    {

        for (int i = 0; i < caveObjectPool.Count; i++)
        {
            if (!caveObjectPool[i].activeInHierarchy)
            {
                caveObjectPool[i].SetActive(true);
                return caveObjectPool[i];
            }
        }

        Debug.Log("BRO NIET BEST");
        return null;
    }

    public GameObject GetPooledTerrainObject()
    {

        for (int i = 0; i < terrainObjectPool.Count; i++)
        {
            if (!terrainObjectPool[i].activeInHierarchy)
            {
                terrainObjectPool[i].SetActive(true);
                return terrainObjectPool[i];
            }
        }

        Debug.Log("BRO NIET BEST WTFF");
        return null;
    }

    public GameObject GetPooledOceanObject()
    {

        for (int i = 0; i < oceanObjectPool.Count; i++)
        {
            if (!oceanObjectPool[i].activeInHierarchy)
            {
                oceanObjectPool[i].SetActive(true);
                return oceanObjectPool[i];
            }
        }

        Debug.Log("BRO NIET BEST WTFF");
        return null;
    }

    public void unloadPooledObject(GameObject gameObject) {
        gameObject.SetActive(false);
    }
}
