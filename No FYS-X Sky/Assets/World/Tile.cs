using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    //public List<GameObject> loadedObjects;
    public GameObject terrain;
    public GameObject cave;
    public GameObject ocean;

    public MeshFilter terrainMeshFilter;
    public MeshFilter caveMeshFilter;
    public MeshFilter oceanMeshFilter;

    public MeshRenderer terrainMeshRenderer;
    public MeshRenderer caveMeshRenderer;
    public MeshRenderer oceanMeshRenderer;

    public MeshCollider terrainMeshCollider;
    public MeshCollider caveMeshCollider;
    public MeshCollider oceanMeshCollider;

    public bool active;

    public float[,] heightMap;

    public Tile(GameObject terrain, GameObject cave, GameObject ocean)
    {
        this.terrain = terrain;
        this.cave = cave;
        this.ocean = ocean;

        this.terrainMeshFilter = terrain.GetComponent<MeshFilter>();
        this.caveMeshFilter = cave.GetComponent<MeshFilter>();
        this.oceanMeshFilter = ocean.GetComponent<MeshFilter>();

        this.terrainMeshRenderer = terrain.GetComponent<MeshRenderer>();
        this.caveMeshRenderer = cave.GetComponent<MeshRenderer>();
        this.oceanMeshRenderer = ocean.GetComponent<MeshRenderer>();

        this.terrainMeshCollider = terrain.GetComponent<MeshCollider>();
        this.caveMeshCollider = cave.GetComponent<MeshCollider>();
        this.oceanMeshCollider = ocean.GetComponent<MeshCollider>();

        active = true;
        //this.loadedObjects = new List<GameObject>();
    }

    /*    public void AddObject(GameObject obj)
        {
            this.loadedObjects.Add(obj);
        }*/

    public void disableTile() 
    {
        this.terrainMeshRenderer.enabled = false;
        this.caveMeshRenderer.enabled = false;
        this.oceanMeshRenderer.enabled = false;
        this.terrain.SetActive(false);
        this.cave.SetActive(false);
        this.ocean.SetActive(false);
        this.active = false;
    }

    public void enableTile()
    {
        this.terrain.SetActive(true);
        this.cave.SetActive(true);
        this.ocean.SetActive(true);
        this.active = true;
    }

/*    public void DestroyObjects(ObjectPool objectPool)
    {

*//*        foreach (GameObject obj in loadedObjects)
        {
            objectPool.UnloadPooledObject(obj);
        }*//*
    }*/
}
