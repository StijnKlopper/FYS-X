using Assets.World.Generator;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour, Generator
{
    [SerializeField]
    public float minimumCityDistanceRadius = 20f;
    public int minimumCitySize = 20;
    int cityRadius = 15;
    float margin = 0.5f;

    public bool raysDebug = true;

    TerrainGenerator terrainGenerator;

    public Dictionary<Vector3, Color> coloredRays;

    public Dictionary<Vector3, List<Vector3>> cityPoints;

    public List<GameObject> houses;

    GameObject parentObj;

    // Start is called before the first frame update
    void Start()
    {
        coloredRays = new Dictionary<Vector3, Color>();
        cityPoints = new Dictionary<Vector3, List<Vector3>>();

        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        parentObj = GameObject.Find("CityPoints");
    }

    // Update is called once per frame
    void Update()
    {
        DebugPoints();
    }

    public void Generate(int mapWidth, int mapHeight, Vector2 offsets)
    {
        List<Vector3> cubePoints = DrawCityLocations(mapWidth, mapHeight, offsets);

        // Loop through the cubes and check with rays if possible locations
        foreach (Vector3 cubePoint in cubePoints)
        {
            StartCoroutine(PerformActionAfterTime(0.1f, () => {
                CheckCoordinatesAroundBox(cubePoint);
                GenerateCity(cubePoint);
            }));
        }
    }

    Vector3 CheckValidCityPoint(Vector3 pointPosition)
    {
        Vector2 cubePosition = new Vector2(-pointPosition.x, -pointPosition.z);
        Biome biome = terrainGenerator.GetBiomeByCoordinates(cubePosition);

        // Check for not a Ocean or Mountain biome
        if (!(biome.biomeType is OceanBiomeType) && !(biome.biomeType is MountainBiomeType))
        {
            Vector3 startPosition = new Vector3(pointPosition.x, 10, pointPosition.z);
            if (Physics.Raycast(startPosition, -transform.up, out RaycastHit tileHit, Mathf.Infinity))
            {
                return tileHit.point;
            }
        }
        return Vector3.zero;
    }

    public void CheckCoordinatesAroundBox(Vector3 cubePosition)
    {
        // Per box make it raining rays to check for valid points        
        for (int x = (int)cubePosition.x + cityRadius; x >= cubePosition.x - cityRadius; x--)
        {
            for (int z = (int)cubePosition.z + cityRadius; z >= cubePosition.z - cityRadius; z--)
            {
                Vector3 rayPosition = new Vector3(x, 10, z);
                Ray ray = new Ray(rayPosition, -transform.up);
                if (Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    // Check if hit is within height margin
                    if (cubePosition.y + margin >= hitInfo.point.y && cubePosition.y - margin <= hitInfo.point.y && cubePosition.y >= 0)
                    {
                        // Ray with a possibility for a city
                        cityPoints[cubePosition].Add(hitInfo.point);
                        if (coloredRays.ContainsKey(hitInfo.point)) coloredRays[hitInfo.point] = Color.green;
                        else coloredRays.Add(hitInfo.point, Color.green);
                    }
                    else
                    {
                        // Invalid map coord positions
                        if (coloredRays.ContainsKey(hitInfo.point)) coloredRays[hitInfo.point] = Color.red;
                        else coloredRays.Add(hitInfo.point, Color.red);
                    }

                }

            }
        }
    }

    public void GenerateCity(Vector3 cityCubeLocation)
    {
        // Check if the city is large enough. If it is it will generate a city.
        if (cityPoints[cityCubeLocation].Count >= minimumCitySize)
        {
            // TODO: Now only one house will spawn, this should be random (maybe based on size of city?)
            GenerateBuilding(cityCubeLocation);
        }
    }

    public void GenerateBuilding(Vector3 cityCubeLocation)
    {
        List<Vector3> rayHits = cityPoints[cityCubeLocation]; // TODO: Test if not null

        int fakeY = 0;
        int randomHouseIndex = UnityEngine.Random.Range(0, houses.Count);
        int randomLocationIndex = UnityEngine.Random.Range(0, rayHits.Count);

        List<Vector3> rayHitsFakeY = new List<Vector3>();
        foreach (var coord in rayHits)
        {
            rayHitsFakeY.Add(new Vector3(coord.x, fakeY, coord.z));
        }

        Vector3 houseBounds = CalculateBounds(houses[randomHouseIndex]);
        Vector3 location = rayHits[randomLocationIndex];

        // All coordinates (corners) to check within
        int smallestX = (int)Math.Round(location.x - (houseBounds.x / 2));
        int highestX = (int)Math.Round(location.x + (houseBounds.x / 2));
        int smallestZ = (int)Math.Round(location.z - (houseBounds.z / 2));
        int highestZ = (int)Math.Round(location.z + (houseBounds.z / 2));

        bool valid = true;
        for (int x = smallestX; x <= highestX; x++)
        {
            for (int z = smallestZ; z <= highestZ; z++)
            {
                Vector3 vectorToCheck = new Vector3(x, fakeY, z);
                if (rayHitsFakeY.Contains(vectorToCheck)) 
                {
                    coloredRays[vectorToCheck] = Color.white;
                }
                else
                {
                    coloredRays.Add(vectorToCheck, Color.blue);
                    valid = false;
                }
            }
        }

        if (valid)
        {
            // TODO: houses should be placed from the centre of a prefab, highestX and highestZ should be replaced by location.x and location.z
            Instantiate(houses[randomHouseIndex], new Vector3(highestX, houses[randomHouseIndex].transform.position.y + location.y, highestZ), Quaternion.LookRotation(cityCubeLocation), parentObj.transform.GetChild(0).transform);
        }

    }

    public Vector3 CalculateBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1, ni = renderers.Length; i < ni; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds.size;
        }
        else
        {
            return new Bounds().size;
        }
    }

    public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
    {
        return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
    }

    private float[,] generateCityNoiseMap(int mapWidth, int mapHeight, Vector2 offsets)
    {
        int scale = 20;
        int octaves = 1;
        mapWidth *= 5;
        mapWidth *= 5;
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float[,] noiseMap = new float[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + offsets.x) / scale;
                    float sampleY = (y + offsets.y) / scale;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    // Create starting search points for city and place a cube
    public List<Vector3> DrawCityLocations(int mapWidth, int mapHeight, Vector2 offsets)
    {
        List<Vector3> cubePoints = new List<Vector3>();
        float[,] noiseMap = generateCityNoiseMap(mapWidth, mapHeight, offsets);
        float pointHeight = 0.00f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                if (currentHeight <= pointHeight)
                {
                    Vector3 possiblePointPosition = new Vector3(-(x + offsets.x), 0, -(y + offsets.y));

                    if (checkNearbyPoints(possiblePointPosition, minimumCityDistanceRadius))
                    {
                        break;
                    }
                    else
                    {
                        Vector3 cubePoint = CheckValidCityPoint(possiblePointPosition);
                        if (cubePoint != Vector3.zero)
                        {
                            // Make a cube on the valid location
                            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            point.transform.SetParent(parentObj.transform.GetChild(1).transform);
                            point.transform.position = possiblePointPosition;
                            Physics.SyncTransforms();

                            // Add the cube point to an array
                            cityPoints.Add(cubePoint, new List<Vector3>());
                            cubePoints.Add(cubePoint);
                            break;
                        }
                    }
                }
            }
        }

        return cubePoints;
    }

    IEnumerator<WaitForSeconds> PerformActionAfterTime(float delayAmount, System.Action action)
    {
        yield return new WaitForSeconds(delayAmount);
        action();
    }

    //Raycast a sphere and check for nearby gameobjects
    private bool checkNearbyPoints(Vector3 center, float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        foreach (var hitCollider in hitColliders)
        {
            // If gameobject is a cube then return true
            if (hitCollider.gameObject.name == ("Cube"))
            {
                return true;
            }
        }
        return false;
    }

    private void DebugPoints()
    {
        if (raysDebug)
        {
            foreach (var ray in coloredRays)
            {
                Debug.DrawRay(ray.Key, transform.up * 10, ray.Value);
            }
        }
    }

}
