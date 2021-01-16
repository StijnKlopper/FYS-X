using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    private int seed;

    protected float roomWidthHeight;
    protected float roomWidthHeightRadius;

    [Range(1, 10)]
    public int maxDepth;

    [Range(1, 10)]
    public int maxFloors;

    [SerializeField]
    private List<GameObject> floors;

    [SerializeField]
    private List<GameObject> walls;

    [SerializeField]
    private List<GameObject> doors;

    [SerializeField]
    private List<GameObject> roofs;

    [System.NonSerialized]
    private GameObject parentObject;

    private System.Random random; 

    enum Rotation
    {
        North,
        East,
        South,
        West,
        Up,
        Down
    }

    enum GridTypes
    {
        Empty,
        Wall,
        WallFloor,
        Door,
        DoorFloor,
        Floor,
        Roof
    }

    private void Start()
    {
        // Set seed
        seed = TerrainGenerator.Seed;

        // Set parent object for buildings to be placed in
        parentObject = GameObject.Find("Buildings");

        random = new System.Random(seed);
        roomWidthHeight = 4f;
        roomWidthHeightRadius = roomWidthHeight / 2;
    }

    public GameObject Generate(Vector3 position)
    {
        // Generate grid and make building
        GridTypes[,,] grid = GenerateGrid();

        GameObject building = SpawnBuilding(Vector3.zero, grid);
        building.transform.position = position;

        return building;
    }

    private GridTypes[,,] GenerateGrid()
    {
        int floors = GetRandomNumberTo(2, maxFloors);
        int depth = GetRandomNumberTo(2, maxDepth);

        // Add extra fields for wall margin
        depth += 2;
        floors += 1;

        // Initialize grid y/floors, x/depth, z/depth
        GridTypes[,,] grid = new GridTypes[floors, depth, depth];

        // Make a grid without corners and with floors only
        for (int y = 0; y < floors; y++)
        {
            for (int x = 0; x < depth; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (x == 0 || x == depth - 1 || z == 0 || z == depth - 1 || y == floors - 1)
                    {
                        // Add empty margin for wall and roof placement
                        grid[y, x, z] = GridTypes.Empty;
                    }
                    else
                    {
                        grid[y, x, z] = GridTypes.Floor;
                    }
                }
            }
        }

        // Removes one random block of the building
        bool shouldRemoveParts = floors * depth * depth > 24 ? true : false;
        if (shouldRemoveParts)
        {
            int blockYEnd = GetRandomNumberTo(2, floors - 1);
            int blockXStart = GetRandomNumberTo(depth);
            int blockZStart = GetRandomNumberTo(depth);

            int blockXWidth = GetRandomNumberTo(1, depth / 2);
            int blockZWidth = GetRandomNumberTo(1, depth / 2);

            for (int y = 0; y < floors; y++)
            {
                for (int x = 0; x < depth; x++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (y >= blockYEnd && x >= blockXStart && z >= blockZStart && x <= blockXStart + blockXWidth && z <= blockZStart + blockZWidth)
                        {
                            grid[y, x, z] = GridTypes.Empty;
                        }
                    }
                }
            }
        }

        // Loop through all floors from top to bottom and add roofs if a floor is below
        for (int y = floors - 1; y >= 0; y--)
        {
            for (int x = 0; x < depth; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (grid[y, x, z] == GridTypes.Floor && y + 1 < floors && grid[y + 1, x, z] == GridTypes.Empty)
                    {
                        // Make roofs
                        grid[y, x, z] = GridTypes.Roof;
                    }
                }
            }
        }

        // Add walls, roofs and doors
        SortedDictionary<int, bool> doorsOnFloor = new SortedDictionary<int, bool>();
        for (int y = 0; y < floors; y++)
        {
            for (int x = 0; x < depth; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // Add walls next to floors
                    Dictionary<Rotation, GridTypes> nearbyGridTypes = GetGridTypeAllDirections(grid, new Vector3Int(x, y, z), true);

                    if (grid[y, x, z] == GridTypes.Empty && nearbyGridTypes.ContainsValue(GridTypes.Floor))
                    {
                        grid[y, x, z] = GridTypes.Wall;
                    }

                    // Add WallFloors if a roof has a floor next to it
                    if (grid[y, x, z] == GridTypes.Roof && nearbyGridTypes.ContainsValue(GridTypes.Floor))
                    {
                        grid[y, x, z] = GridTypes.WallFloor;
                    }

                    // Add doors, only one per Y level 
                    if (grid[y, x, z] == GridTypes.WallFloor || (grid[y, x, z] == GridTypes.Wall && y == 0))
                    {
                        if (!doorsOnFloor.ContainsKey(y)) doorsOnFloor[y] = false;
                        if (!doorsOnFloor[y])
                        {
                            grid[y, x, z] = grid[y, x, z] == GridTypes.WallFloor ? GridTypes.DoorFloor : GridTypes.Door;
                            doorsOnFloor[y] = true;

                        }
                    }
                    
                }
            }
        }

        return grid;
    }

    private GameObject SpawnBuilding(Vector3 position, GridTypes[,,] grid)
    {
        // Make building folder
        GameObject buildingFolder = new GameObject("Building");
        buildingFolder.transform.SetParent(parentObject.transform);

        // Spawn grid elements
        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                for (int z = 0; z < grid.GetLength(2); z++)
                {
                    // Loop through every element of the grid and place prefabs accordingly
                    if (grid[y, x, z] == GridTypes.Empty) continue;
                    SpawnGridElement(buildingFolder.transform, new Vector3(position.x + (x * roomWidthHeight), position.y + (y * roomWidthHeight), position.z + (z * roomWidthHeight)), grid, new Vector3Int(x, y, z));
                }
            }
        }

        return buildingFolder;
    }

    private void SpawnGridElement(Transform buildingFolder, Vector3 position, GridTypes[,,] grid, Vector3Int gridPosition)
    {
        GridTypes gridType = grid[gridPosition.y, gridPosition.x, gridPosition.z];
        switch (gridType)
        {
            case GridTypes.DoorFloor:
                foreach (Rotation rotation in GetElementWallDirections(grid, gridPosition))
                {
                    GenerateDoor(buildingFolder, position, rotation);
                }
                GenerateFloor(buildingFolder, position);
                break;
            case GridTypes.WallFloor:
                foreach (Rotation rotation in GetElementWallDirections(grid, gridPosition))
                {
                    GenerateWall(buildingFolder, position, rotation);
                }
                GenerateFloor(buildingFolder, position);
                break;
            case GridTypes.Wall:
                foreach (Rotation rotation in GetElementWallDirections(grid, gridPosition))
                {
                    GenerateWall(buildingFolder, position, rotation);
                }
                break;
            case GridTypes.Door:
                foreach (Rotation rotation in GetElementWallDirections(grid, gridPosition))
                {
                    GenerateDoor(buildingFolder, position, rotation);
                }
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

    // Get correct wall placement, only for wall placements
    private List<Rotation> GetElementWallDirections(GridTypes[,,] grid, Vector3Int gridPosition)
    {
        List<Rotation> rotations = new List<Rotation>();
        if (gridPosition.z + 1 <= grid.GetLength(2) - 1 && grid[gridPosition.y, gridPosition.x, gridPosition.z + 1] == GridTypes.Floor)
        {
            rotations.Add(Rotation.South);
        }
        if (gridPosition.z - 1 >= 0 && grid[gridPosition.y, gridPosition.x, gridPosition.z - 1] == GridTypes.Floor)
        {
            rotations.Add(Rotation.North);
        }
        if (gridPosition.x + 1 <= grid.GetLength(1) - 1 && grid[gridPosition.y, gridPosition.x + 1, gridPosition.z] == GridTypes.Floor)
        {
            rotations.Add(Rotation.West);
        }
        if (gridPosition.x - 1 >= 0 && grid[gridPosition.y, gridPosition.x - 1, gridPosition.z] == GridTypes.Floor)
        {
            rotations.Add(Rotation.East);
        }
        return rotations;
    }

    // Get all the GridTypes for all direction from gridPosition
    private Dictionary<Rotation, GridTypes> GetGridTypeAllDirections(GridTypes[,,] grid, Vector3Int gridPosition, bool sidesOnly=false)
    {
        Dictionary<Rotation, GridTypes> gridTypes = new Dictionary<Rotation, GridTypes>();
        if (gridPosition.z + 1 <= grid.GetLength(2) - 1)
        {
            gridTypes.Add(Rotation.South, grid[gridPosition.y, gridPosition.x, gridPosition.z + 1]);
        }
        if (gridPosition.z - 1 >= 0)
        {
            gridTypes.Add(Rotation.North, grid[gridPosition.y, gridPosition.x, gridPosition.z - 1]);
        }
        if (gridPosition.x + 1 <= grid.GetLength(1) - 1)
        {
            gridTypes.Add(Rotation.West, grid[gridPosition.y, gridPosition.x + 1, gridPosition.z]);
        }
        if (gridPosition.x - 1 >= 0)
        {
            gridTypes.Add(Rotation.East, grid[gridPosition.y, gridPosition.x - 1, gridPosition.z]);
        }
        if(!sidesOnly && gridPosition.y - 1 >= 0)
        {
            gridTypes.Add(Rotation.Down, grid[gridPosition.y - 1, gridPosition.x, gridPosition.z]);
        }
        if (!sidesOnly && gridPosition.y + 1 <= grid.GetLength(0) - 1)
        {
            gridTypes.Add(Rotation.Up, grid[gridPosition.y + 1, gridPosition.x, gridPosition.z]);
        }
        return gridTypes;
    }

    private void GenerateRoof(Transform buildingFolder, Vector3 position)
    {
        Transform roof = Instantiate(
          roofs[GetRandomNumberTo(roofs.Count - 1)].transform,
          new Vector3(position.x, position.y + 0.8f, position.z),
          GetQuaternionFrom(Rotation.North),
          buildingFolder);
        roof.name = "Roof";
    }

    private void GenerateFloor(Transform buildingFolder, Vector3 position)
    {
        Transform floor = Instantiate(
          floors[GetRandomNumberTo(floors.Count - 1)].transform,
          position,
          GetQuaternionFrom(Rotation.Up),
          buildingFolder);
        floor.name = "Floor";
    }

    private void GenerateWall(Transform buildingFolder, Vector3 position, Enum rotation)
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
          walls[GetRandomNumberTo(walls.Count - 1)].transform,
          wallPosition,
          GetQuaternionFrom(rotation),
          buildingFolder);
        wall.name = "Wall";
    }

    private void GenerateDoor(Transform buildingFolder, Vector3 position, Enum rotation)
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
          doors[GetRandomNumberTo(doors.Count - 1)].transform,
          doorPosition,
          GetQuaternionFrom(rotation),
          buildingFolder);
        door.name = "Door";
    }

    private Quaternion GetQuaternionFrom(Enum rotation)
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

    public void GeneratePreviewHouse()
    {
        // Set values to be able to generate a house
        this.random = new System.Random(TerrainGenerator.Seed);
        parentObject = GameObject.Find("Buildings");
        Vector3 position = Vector3.zero;

        // Delete old building
        foreach (Transform child in parentObject.transform)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }

        Generate(position);
    }

    // Returns a random number from 0 to end (including end)
    private int GetRandomNumberTo(int start, int end)
    {
        return this.random.Next(start, end + 1);
    }

    private int GetRandomNumberTo(int end)
    {
        return GetRandomNumberTo(0, end);
    }
}
