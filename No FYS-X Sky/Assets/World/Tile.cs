using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public TileInfo Terrain;
    public TileInfo Cave;
    public TileInfo Ocean;
    public List<GameObject> BuildingObjects;
    public List<GameObject> DecoObjects;
    public bool Active;
    public int LevelOfDetail;
    public float[,] HeightMap;

    public class TileInfo 
    {
        public GameObject GameObject;
        public MeshRenderer MeshRenderer;
        public MeshFilter MeshFilter;
        public MeshCollider MeshCollider;

        public TileInfo(GameObject gameObject) 
        {
            this.GameObject = gameObject;
            this.MeshRenderer = gameObject.GetComponent<MeshRenderer>();
            this.MeshFilter = gameObject.GetComponent<MeshFilter>();
            this.MeshCollider = gameObject.GetComponent<MeshCollider>();
        }
    }

    public Tile(GameObject terrain, GameObject cave, GameObject ocean)
    {
        BuildingObjects = new List<GameObject>();
        DecoObjects = new List<GameObject>();

        this.Terrain = new TileInfo(terrain);
        this.Cave = new TileInfo(cave);
        this.Ocean = new TileInfo(ocean);

        Active = true;
    }

    public void AddBuilding(GameObject building) 
    {
        BuildingObjects.Add(building);
    }

    public void AddDecoration(GameObject go)
    {
        DecoObjects.Add(go);
    }

    public void disableTile()
    {
        this.Terrain.GameObject.SetActive(false);
        this.Cave.GameObject.SetActive(false);
        this.Ocean.GameObject.SetActive(false);
        this.Active = false;

        foreach (GameObject building in BuildingObjects) 
        {
            if (building != null) 
            {
                building.SetActive(false);
            }  
        }

        foreach(GameObject go in DecoObjects)
        {
            if(go != null)
            {
                UnityEngine.MonoBehaviour.Destroy(go);
            }
        }
    }

    public void enableTile() 
    {
        this.Active = true;
    }
}
