using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public List<GameObject> loadedObjects;

    public float[,] heightMap;

    public Tile()
    {
        this.loadedObjects = new List<GameObject>();
    }

    public void AddObject(GameObject obj)
    {
        this.loadedObjects.Add(obj);
    }

    public void DestroyObjects(ObjectPool objectPool)
    {

        foreach (GameObject obj in loadedObjects)
        {
            objectPool.UnloadPooledObject(obj);
        }
    }
}
