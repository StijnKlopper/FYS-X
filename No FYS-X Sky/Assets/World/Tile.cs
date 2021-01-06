using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public List<GameObject> LoadedObjects;

    public float[,] HeightMap;

    public int LevelOfDetail;

    public Tile()
    {
        this.LoadedObjects = new List<GameObject>();
    }

    public void AddObject(GameObject obj)
    {
        this.LoadedObjects.Add(obj);
    }

    public void DestroyObjects(ObjectPool objectPool)
    {
        foreach (GameObject obj in LoadedObjects)
        {
            objectPool.UnloadPooledObject(obj);
        }
    }

    public void RegenerateMesh()
    {
        TileBuilder tileBuilder = this.LoadedObjects[0].GetComponent<TileBuilder>();
        tileBuilder.SetMesh(tileBuilder.GenerateMesh(LevelOfDetail, HeightMap).CreateMesh());
    }
}
