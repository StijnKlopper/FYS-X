using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public TileInfo terrain;
    public TileInfo cave;
    public TileInfo ocean;
    public List<GameObject> buildingObjects;
    public bool active;
    public int LevelOfDetail;
    public float[,] HeightMap;

    public class TileInfo 
    {
        public GameObject gameObject;
        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;

        public TileInfo(GameObject gameObject) 
        {
            this.gameObject = gameObject;
            this.meshRenderer = gameObject.GetComponent<MeshRenderer>();
            this.meshFilter = gameObject.GetComponent<MeshFilter>();
            this.meshCollider = gameObject.GetComponent<MeshCollider>();
        }
    }

    public Tile(GameObject terrain, GameObject cave, GameObject ocean)
    {
        buildingObjects = new List<GameObject>();

        this.terrain = new TileInfo(terrain);
        this.cave = new TileInfo(cave);
        this.ocean = new TileInfo(ocean);

        active = true;
    }

    public void AddBuilding(GameObject building) 
    {
        buildingObjects.Add(building);
    }

    public void disableTile()
    {
        this.terrain.gameObject.SetActive(false);
        this.cave.gameObject.SetActive(false);
        this.ocean.gameObject.SetActive(false);
        this.active = false;

        foreach (GameObject building in buildingObjects) 
        {
            if (building != null) 
            {
                building.SetActive(false);
            }  
        }
    }

    public void enableTile() 
    {
        this.active = true;
    }

    public void RegenerateMesh()
    {
        TileBuilder tileBuilder = GameObject.Find("Level").GetComponent<TileBuilder>();
        if (HeightMap != null) { tileBuilder.SetMesh(tileBuilder.GenerateMesh(LevelOfDetail, HeightMap).CreateMesh(), this); }
    }
}
