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
            this.randomNumbers[i] = random.Next(10000, 100000);
        }
    }

    private GridTypes[,,] GenerateGrid()
    {
        int temp = 12345; // TODO: Should be coordinate x * y 
        int floors = GetRandomNumberTo(maxFloors, temp);
        floors = floors > 1 ? floors : 2;
        int depth = GetRandomNumberTo(maxDepth, temp + 1);
        depth = depth > 1 ? depth : 2;

        Debug.Log("Floors: " + floors);
        Debug.Log("Depth: " + depth);

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
        int maxBlockSize = 2;
        if (shouldRemoveParts)
        {
            int blockYEnd = GetRandomNumberTo(floors - 1, temp + 14);
            int blockXStart = GetRandomNumberTo(maxBlockSize, temp + 15);
            int blockZStart = GetRandomNumberTo(maxBlockSize, temp + 16);

            int blockXWidth = GetRandomNumberTo(maxBlockSize, temp + 17);
            int blockZWidth = GetRandomNumberTo(maxBlockSize, temp + 18);

            Debug.Log("blockYEnd: " + blockYEnd + ", blockXStart: " + blockXStart + ", blockZStart: " + blockZStart);

            int removeCount = 0;
            for (int y = 0; y < floors; y++)
            {
                for (int x = 0; x < depth; x++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (y >= blockYEnd && x >= blockXStart && z >= blockZStart)
                        {
                            grid[y, x, z] = GridTypes.Empty;
                            removeCount++;
                        }
                    }
                }
            }
            Debug.Log("Blocks removed: " + removeCount);
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

        // Add walls and roofs
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
                }
            }
        }

        /*grid = new GridTypes[,,]
        {
            {
                { GridTypes.Empty, GridTypes.Wall, GridTypes.Empty, GridTypes.Wall,  GridTypes.Empty},
                { GridTypes.Wall, GridTypes.Floor, GridTypes.Wall, GridTypes.Floor, GridTypes.Wall},
                { GridTypes.Wall, GridTypes.Floor, GridTypes.Floor, GridTypes.Floor, GridTypes.Wall},
                { GridTypes.Empty, GridTypes.Wall, GridTypes.Wall, GridTypes.Door,  GridTypes.Empty},
            }, // Floor 0
            {
                { GridTypes.Empty, GridTypes.Empty, GridTypes.Wall, GridTypes.Wall,  GridTypes.Empty},
                { GridTypes.Empty, GridTypes.DoorFloor, GridTypes.Floor, GridTypes.Floor, GridTypes.Wall},
                { GridTypes.Empty, GridTypes.Roof, GridTypes.WallFloor, GridTypes.WallFloor, GridTypes.Empty},
                { GridTypes.Empty, GridTypes.Empty, GridTypes.Empty, GridTypes.Empty,  GridTypes.Empty},
            }, // Floor 1
            {
                { GridTypes.Empty, GridTypes.Empty, GridTypes.Empty, GridTypes.Empty,  GridTypes.Empty},
                { GridTypes.Empty, GridTypes.Empty, GridTypes.Roof, GridTypes.Roof, GridTypes.Empty},
                { GridTypes.Empty, GridTypes.Empty, GridTypes.Empty, GridTypes.Empty, GridTypes.Empty},
                { GridTypes.Empty, GridTypes.Empty, GridTypes.Empty, GridTypes.Empty,  GridTypes.Empty},
            }, // Floor 2
        };*/

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

    public void Generate()
    {
        // Temp values test
        seed = 69420;
        System.Random random = new System.Random(seed);
        this.randomNumbers = new int[100];
        for (int i = 0; i < this.randomNumbers.Length; i++)
        {
            this.randomNumbers[i] = random.Next(10000, 100000);
        }
        parentObject = GameObject.Find("Buildings");
        Vector3 position = new Vector3(0, 0, 0);

        // Delete old building
        foreach (Transform child in parentObject.transform)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }

        // Grid
        GridTypes[,,] grid = GenerateGrid();
        SpawnBuilding(position, grid);
    }

    public void GenerateRoof(Transform buildingFolder, Vector3 position)
    {
        // TODO: Make actual roofs
        Transform roof = Instantiate(
          floors[0].transform,
          //new Vector3(position.x, position.y + roomWidthHeight, position.z),
          new Vector3(position.x, position.y, position.z),
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
        RandomNumberValidator();
    }

    // Returns a random number from 0 to end (including end)
    public int GetRandomNumberTo(int end, int randomizer)
    {
        if (seed <= 0) seed = 1;
        if (randomizer <= 0) randomizer = 1;

        // TODO: Gaat soms fout omdat het niet allemaal prime numbers zijn waardoor % gedaan wordt ??
        //Debug.Log("Seed: " + seed + ", Randomizer: " + randomizer + ", End: " + end + ", Output: " + (this.randomNumbers[randomizer * seed % randomNumbers.Length] % (end + 1)));
        return this.randomNumbers[randomizer * seed % randomNumbers.Length] % (end + 1);
    }

    private void RandomNumberValidator()
    {
        int total = 1000;
        int maxSizeRandomNumber = 10;
        SortedDictionary<int, List<int>> listOfNumbers = new SortedDictionary<int, List<int>>();
        for (int i = 0; i < total; i++)
        {
            int generatedRandomNumber = GetRandomNumberTo(maxSizeRandomNumber, i);
            for (int j = maxSizeRandomNumber / 10; j <= maxSizeRandomNumber; j += maxSizeRandomNumber / 10)
            {
                if (!listOfNumbers.ContainsKey(j)) listOfNumbers.Add(j, new List<int>());
                if (generatedRandomNumber <= j)
                {
                    listOfNumbers[j].Add(generatedRandomNumber);
                    break;
                }
            }
        }

        // Display generated numbers
        foreach (KeyValuePair<int, List<int>> item in listOfNumbers)
        {
            Debug.Log(item.Value.Count + " numbers generated from last to " + item.Key);

            // Show generated numbers on one line
            string numbersString = "";
            foreach (int numb in item.Value) numbersString += numb + ", ";
            //Debug.Log(numbersString);
        }
    }
}
