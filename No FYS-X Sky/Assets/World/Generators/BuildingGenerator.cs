using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour, Generator
{
    private int[] randomNumbers;

    public int seed;

    protected float roomWidthHeight = 4.0f;
    protected float roomWidthHeightRadius = 2f;

    [Range(0, 10)]
    public int minDepth;
    public int maxDepth;

    [Range(0, 10)]
    public int minFloors;
    public int maxFloors;

    [SerializeField]
    List<GameObject> floors;

    [SerializeField]
    List<GameObject> walls;

    [System.NonSerialized]
    GameObject parentObject;

    enum Rotation
    {
        North,
        East,
        South,
        West,
        Up
    }

    BuildingGenerator()
    {
    }

    private void Start()
    {
        // Set seed
        TerrainGenerator terrainGenerator = new TerrainGenerator();
        seed = terrainGenerator.seed;

        // Set parent object for buildings to be placed in
        parentObject = GameObject.Find("Buildings");

        // Make some random numbers for usage
        System.Random random = new System.Random(seed);
        this.randomNumbers = new int[100];

        for (int i = 0; i < this.randomNumbers.Length; i++)
        {
            this.randomNumbers[i] = random.Next(0, 10);
        }
    }

    public void Generate()
    {
        // Test
        Debug.Log("Generate");
        Debug.Log(seed);
        Debug.Log(CalculateBounds(floors[0]));

        // Temp variables
        parentObject = GameObject.Find("Buildings");
        Vector3 position = new Vector3(0, 0, 0);
        roomWidthHeight = 4f;
        float roomWidthHeightRadius = roomWidthHeight / 2f;

        // Make building folder
        Transform buildingFolder = new GameObject("Building").transform;
        buildingFolder.SetParent(parentObject.transform);

        // Make floor
        Transform floor = Instantiate(
           floors[0].transform,
           position,
           GetQuaternionFrom(Rotation.Up),
           buildingFolder);
        floor.name = "Floor";

        // Make roof
        Transform roof = Instantiate(
           floors[0].transform,
           new Vector3(position.x, position.y + roomWidthHeight, position.z),
           GetQuaternionFrom(Rotation.Up),
           buildingFolder);
        roof.name = "Roof";

        // Make wall (north)
        Renderer wallNorthRenderer = walls[7].GetComponentInChildren<Renderer>();
        Debug.Log("wallNorthRenderer" + wallNorthRenderer.bounds.size);
        Transform wallNorth = Instantiate(
          walls[7].transform,
          new Vector3(position.x, position.y + roomWidthHeightRadius, position.z + roomWidthHeightRadius),
          GetQuaternionFrom(Rotation.North),
          buildingFolder);
        wallNorth.name = "Wall north";
        Debug.Log(CalculateBounds(walls[7]));

        // Make wall (east)
        Renderer wallEastRenderer = walls[0].GetComponentInChildren<Renderer>();
        Debug.Log("wallEastRenderer" + wallEastRenderer.bounds.size);
        Transform wallEast = Instantiate(
          walls[0].transform,
          new Vector3(position.x + roomWidthHeightRadius, position.y + roomWidthHeightRadius, position.z),
          GetQuaternionFrom(Rotation.East),
          buildingFolder);
        wallEast.name = "Wall east";
        Debug.Log(CalculateBounds(walls[0]));

        // Make wall (south)
        Renderer wallSouthRenderer = walls[6].GetComponentInChildren<Renderer>();
        Debug.Log("wallSouthRenderer" + wallSouthRenderer.bounds.size);
        Transform wallSouth = Instantiate(
          walls[6].transform,
          new Vector3(position.x, position.y + roomWidthHeightRadius, position.z - roomWidthHeightRadius),
          GetQuaternionFrom(Rotation.South),
          buildingFolder);
        wallSouth.name = "Wall south";
        Debug.Log(CalculateBounds(walls[6]));

        // Make wall (west)
        Transform wallWest = Instantiate(
          walls[3].transform,
          new Vector3(position.x - roomWidthHeightRadius, position.y + roomWidthHeightRadius, position.z),
          GetQuaternionFrom(Rotation.West),
          buildingFolder);
        wallWest.name = "Wall west";
    }

    public void GenerateWall(Transform buildingFolder, Vector3 position, Enum rotation)
    {
        // TODO: Afmaken en gebruiken
        Transform wall = Instantiate(
          walls[3].transform,
          new Vector3(position.x - roomWidthHeightRadius, position.y + roomWidthHeightRadius, position.z),
          GetQuaternionFrom(rotation),
          buildingFolder);
        wall.name = "Wall";
    }

    public Quaternion GetQuaternionFrom(Enum rotation)
    {
        switch (rotation)
        {
            case Rotation.North:
                return Quaternion.Euler(0, -90, 0);
            case Rotation.South:
                return Quaternion.Euler(0, 90, 0);
            case Rotation.Up:
                return Quaternion.Euler(90, 0, 0);
            case Rotation.West:
            case Rotation.East:
            default:
                return Quaternion.Euler(0, 0, 0);
        }
    }

    public void RandomValues()
    {
        // TODO: Make fake values
        Debug.Log("TODO");
    }

    private void OnValidate()
    {
        // TODO: Validate input values
    }

    public Bounds CalculateBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1, ni = renderers.Length; i < ni; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
        else
        {
            return new Bounds();
        }
    }
}
