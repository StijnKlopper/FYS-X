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

    [Range(1, 10)]
    public int maxDepth;

    [Range(1, 10)]
    public int maxFloors;

    [SerializeField]
    List<GameObject> floors;

    [SerializeField]
    List<GameObject> walls;

    [SerializeField]
    List<GameObject> doors;

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

    enum GridTypes
    {
        Empty,
        Wall,
        Door,
        Floor,
        Roof
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
        this.randomNumbers = new int[50];

        for (int i = 0; i < this.randomNumbers.Length; i++)
        {
            this.randomNumbers[i] = random.Next(10000, 100000);
        }
    }

    private GridTypes[,,] GenerateGrid()
    {
        int temp = 69420;
        int floors = GetRandomNumberTo(maxFloors, temp);
        floors = floors > 0 ? floors : 1;
        int depth = GetRandomNumberTo(maxDepth, temp + 1);
        depth = depth > 0 ? depth : 1;

        Debug.Log("Floors: " + floors);
        Debug.Log("Depth: " + depth);

        // Initialize grid y/floors, x/depth, z/depth
        GridTypes[,,] grid = new GridTypes[floors, depth, depth];

        // TODO: Make random grid
        /*for (int y = 0; y < floors; y++)
        {
            for (int x = 0; x < depth; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    grid[y, x, z] = GridTypes.Floor;
                }
            }
        }*/

        grid = new GridTypes[,,]
        {
            {
                { GridTypes.Empty, GridTypes.Wall, GridTypes.Door, GridTypes.Wall,  GridTypes.Empty},
                { GridTypes.Wall, GridTypes.Floor, GridTypes.Floor, GridTypes.Floor, GridTypes.Wall},
                { GridTypes.Wall, GridTypes.Floor, GridTypes.Floor, GridTypes.Floor, GridTypes.Wall},
                { GridTypes.Empty, GridTypes.Wall, GridTypes.Wall, GridTypes.Door,  GridTypes.Empty},
            }, // Floor 0
            {
                { GridTypes.Empty, GridTypes.Wall, GridTypes.Wall, GridTypes.Wall,  GridTypes.Empty},
                { GridTypes.Wall, GridTypes.Floor, GridTypes.Floor, GridTypes.Floor, GridTypes.Wall},
                { GridTypes.Wall, GridTypes.Floor, GridTypes.Floor, GridTypes.Floor, GridTypes.Wall},
                { GridTypes.Empty, GridTypes.Wall, GridTypes.Wall, GridTypes.Wall,  GridTypes.Empty},
            }, // Floor 1
            {
                { GridTypes.Empty, GridTypes.Empty, GridTypes.Wall, GridTypes.Wall,  GridTypes.Empty},
                { GridTypes.Empty, GridTypes.Door, GridTypes.Floor, GridTypes.Floor, GridTypes.Wall},
                { GridTypes.Empty, GridTypes.Floor, GridTypes.Wall, GridTypes.Wall, GridTypes.Empty},
                { GridTypes.Empty, GridTypes.Empty, GridTypes.Empty, GridTypes.Empty,  GridTypes.Empty},
            }, // Floor 2
        };

        return grid;
    }

    private void SpawnBuilding(Vector3 position, GridTypes[,,] grid)
    {
        // Make building folder
        Transform buildingFolder = new GameObject("Building").transform;
        buildingFolder.SetParent(parentObject.transform);

        // Spawn grid elements
        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                for (int z = 0; z < grid.GetLength(2); z++)
                {
                    // Loop through every element of the grid and place prefabs accordingly
                    if (grid[y, x, z] == GridTypes.Empty) continue;
                    SpawnGridElement(buildingFolder, new Vector3(position.x + (x * roomWidthHeight), position.y + (y * roomWidthHeight), position.z + (z * roomWidthHeight)), grid, new Vector3Int(x, y, z));
                }
            }
        }
    }

    private void SpawnGridElement(Transform buildingFolder, Vector3 position, GridTypes[,,] grid, Vector3Int gridPosition)
    {
        GridTypes gridType = grid[gridPosition.y, gridPosition.x, gridPosition.z];

        switch (gridType)
        {
            case GridTypes.Wall:
                GenerateWall(buildingFolder, position, GetElementWallDirection(grid, gridPosition)); // TODO: Check for multiple walls instead of only one
                break;
            case GridTypes.Door:
                GenerateDoor(buildingFolder, position, GetElementWallDirection(grid, gridPosition)); // TODO: Check for multiple walls instead of only one
                break;
            case GridTypes.Floor:
                GenerateFloor(buildingFolder, position);
                break;
            case GridTypes.Roof:
                GenerateRoof(buildingFolder, position);
                break;
            default:
                break;
        }
    }

    private Rotation GetElementWallDirection(GridTypes[,,] grid, Vector3Int gridPosition)
    {

        if (gridPosition.z + 1 <= grid.GetLength(2) - 1 && grid[gridPosition.y, gridPosition.x, gridPosition.z + 1] == GridTypes.Floor)
        {
            return Rotation.South;
        }
        else if (gridPosition.z - 1 >= 0 && grid[gridPosition.y, gridPosition.x, gridPosition.z - 1] == GridTypes.Floor)
        {
            return Rotation.North;
        }
        else if (gridPosition.x + 1 <= grid.GetLength(1) - 1 && grid[gridPosition.y, gridPosition.x + 1, gridPosition.z] == GridTypes.Floor)
        {
            return Rotation.West;
        }
        else if (gridPosition.x - 1 >= 0 && grid[gridPosition.y, gridPosition.x - 1, gridPosition.z] == GridTypes.Floor)
        {
            return Rotation.East;
        }
        else
        {
            // Shouldn't happen
            Debug.Log("GetElementWallDirection shouln't happen");
            return Rotation.Up;
        }

    }

    public void Generate()
    {
        // Temp values test
        Debug.Log("Generate");
        seed = 69420;
        System.Random random = new System.Random(seed);
        this.randomNumbers = new int[100];
        for (int i = 0; i < this.randomNumbers.Length; i++)
        {
            this.randomNumbers[i] = random.Next(0, 10);
        }
        Debug.Log(seed);
        parentObject = GameObject.Find("Buildings");
        Vector3 position = new Vector3(0, 0, 0);
        roomWidthHeight = 4f;

        // Grid
        GridTypes[,,] grid = GenerateGrid();
        SpawnBuilding(position, grid);

        /* Make building folder
        Transform buildingFolder = new GameObject("Building").transform;
        buildingFolder.SetParent(parentObject.transform);

        // Generate 1x1 building
        GenerateRoof(buildingFolder, position);
        GenerateFloor(buildingFolder, position);
        GenerateWall(buildingFolder, position, Rotation.North);
        GenerateWall(buildingFolder, position, Rotation.East);
        GenerateDoor(buildingFolder, position, Rotation.South);
        GenerateWall(buildingFolder, position, Rotation.West);
        */
    }

    public void GenerateRoof(Transform buildingFolder, Vector3 position)
    {
        // TODO: Make actual roofs
        Transform roof = Instantiate(
          floors[0].transform,
          new Vector3(position.x, position.y + roomWidthHeight, position.z),
          GetQuaternionFrom(Rotation.Up),
          buildingFolder);
        roof.name = "Roof";
    }

    public void GenerateFloor(Transform buildingFolder, Vector3 position)
    {
        Transform floor = Instantiate(
          floors[0].transform, // TODO: Random floor
          position,
          GetQuaternionFrom(Rotation.Up),
          buildingFolder);
        floor.name = "Floor";
    }

    public void GenerateWall(Transform buildingFolder, Vector3 position, Enum rotation)
    {
        Vector3 wallPosition = Vector3.zero;
        switch (rotation)
        {
            case Rotation.North:
                wallPosition = new Vector3(position.x, position.y + roomWidthHeightRadius, position.z - roomWidthHeightRadius);
                break;
            case Rotation.South:
                wallPosition = new Vector3(position.x, position.y + roomWidthHeightRadius, position.z + roomWidthHeightRadius);
                break;
            case Rotation.West:
                wallPosition = new Vector3(position.x + roomWidthHeightRadius, position.y + roomWidthHeightRadius, position.z);
                break;
            case Rotation.East:
                wallPosition = new Vector3(position.x - roomWidthHeightRadius, position.y + roomWidthHeightRadius, position.z);
                break;
            default:
                break;
        }

        Transform wall = Instantiate(
          walls[0].transform, // TODO: Random wall
          wallPosition,
          GetQuaternionFrom(rotation),
          buildingFolder);
        wall.name = "Wall";
    }

    public void GenerateDoor(Transform buildingFolder, Vector3 position, Enum rotation)
    {
        position = new Vector3(position.x, position.y + 0.25f, position.z);
        Vector3 doorPosition = Vector3.zero;
        switch (rotation)
        {
            case Rotation.North:
                doorPosition = new Vector3(position.x, position.y + roomWidthHeightRadius, position.z - roomWidthHeightRadius);
                break;
            case Rotation.South:
                doorPosition = new Vector3(position.x, position.y + roomWidthHeightRadius, position.z + roomWidthHeightRadius);
                break;
            case Rotation.West:
                doorPosition = new Vector3(position.x + roomWidthHeightRadius, position.y + roomWidthHeightRadius, position.z);
                break;
            case Rotation.East:
                doorPosition = new Vector3(position.x - roomWidthHeightRadius, position.y + roomWidthHeightRadius, position.z);
                break;
            default:
                break;
        }

        Transform door = Instantiate(
          doors[0].transform, // TODO: Random door
          doorPosition,
          GetQuaternionFrom(rotation),
          buildingFolder);
        door.name = "Door";
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

    // Returns a random number from 0 to end (including end)
    public int GetRandomNumberTo(int end, int randomizer)
    {
        if (seed == 0) seed = 1;
        if (randomizer == 0) randomizer = 1;

        return this.randomNumbers[randomizer * seed % randomNumbers.Length] % (end + 1);
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
